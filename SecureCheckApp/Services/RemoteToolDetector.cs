using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;

namespace SecureCheckApp.Services;

public class RemoteToolDetector
{
    // Define remote tools to check
    private static readonly List<RemoteTool> RemoteTools = new()
    {
        new RemoteTool(
            "AnyDesk",
            new[] { "anydesk" },
            new[]
            {
                Path.Combine("AnyDesk", "AnyDesk.exe"),
                Path.Combine("AnyDesk", "ad_svc.exe")
            },
            new[] { "AnyDesk.app" }),

        new RemoteTool(
            "TeamViewer",
            new[] { "teamviewer" },
            new[]
            {
                Path.Combine("TeamViewer", "TeamViewer.exe"),
                Path.Combine("TeamViewer", "TeamViewer_Service.exe"),
                Path.Combine("TeamViewer", "TeamViewer_Desktop.exe")
            },
            new[] { "TeamViewer.app", "TeamViewer Host.app" }),

        new RemoteTool(
            "Chrome Remote Desktop",
            new[] { "remoting_host", "chrome_remote_desktop_host" },
            new[]
            {
                Path.Combine("Google", "Chrome Remote Desktop", "CurrentVersion", "remoting_host.exe"),
                Path.Combine("Google", "Chrome Remote Desktop", "CurrentVersion", "remote_assistance_host.exe")
            },
            new[] { "Chrome Remote Desktop Host.app" }),

        new RemoteTool(
            "RustDesk",
            new[] { "rustdesk" },
            new[] { Path.Combine("RustDesk", "rustdesk.exe") },
            new[] { "RustDesk.app" }),

        new RemoteTool(
            "Splashtop",
            new[] { "splashtop", "strwinclt" },
            new[]
            {
                Path.Combine("Splashtop", "Splashtop Remote", "SRServer.exe"),
                Path.Combine("Splashtop", "Splashtop Streamer", "SRServer.exe")
            },
            new[] { "Splashtop Streamer.app", "Splashtop Business.app" }),

        new RemoteTool(
            "LogMeIn",
            new[] { "logmein", "lmi" },
            new[]
            {
                Path.Combine("LogMeIn", "LogMeIn.exe"),
                Path.Combine("LogMeIn", "x86", "LogMeInSystray.exe"),
                Path.Combine("LogMeIn", "x64", "LogMeInSystray.exe")
            },
            new[] { "LogMeIn.app" }),

        new RemoteTool(
            "VNC Server",
            new[] { "vncserver", "winvnc", "tvnserver" },
            new[] { Path.Combine("RealVNC", "VNC Server", "vncserver.exe") },
            new[] { "VNC Server.app", "VNC Viewer.app" }),

        new RemoteTool(
            "Windows Remote Desktop",
            new[] { "mstsc" },
            Array.Empty<string>(),
            Array.Empty<string>()),

        new RemoteTool(
            "Remote Utilities",
            new[] { "rfusclient", "rutserv" },
            new[]
            {
                Path.Combine("Remote Utilities - Host", "rutserv.exe"),
                Path.Combine("Remote Utilities - Viewer", "rutview.exe")
            },
            new[] { "Remote Utilities.app" })
    };

    public class DetectionResult
    {
        public List<string> DetectedTools { get; set; } = new();
        public Dictionary<string, string> DetectionDetails { get; set; } = new();
        public DateTime CheckTime { get; set; }
        public bool HasWarnings => DetectedTools.Any();
    }

    private class RemoteTool
    {
        public string Name { get; }
        public string[] ProcessNames { get; }
        public string[] WindowsInstallMarkers { get; }
        public string[] MacInstallMarkers { get; }

        public RemoteTool(string name, string[] processNames, string[] windowsInstallMarkers, string[] macInstallMarkers)
        {
            Name = name;
            ProcessNames = processNames;
            WindowsInstallMarkers = windowsInstallMarkers ?? Array.Empty<string>();
            MacInstallMarkers = macInstallMarkers ?? Array.Empty<string>();
        }
    }

    public static DetectionResult DetectRemoteTools()
    {
        var result = new DetectionResult
        {
            CheckTime = DateTime.Now
        };

        // Check for running processes
        var runningProcesses = GetRunningProcesses();
        
        foreach (var tool in RemoteTools)
        {
            bool isRunning = false;
            bool isInstalled = false;
            List<string> details = new();

            // Check if process is running
            foreach (var processName in tool.ProcessNames)
            {
                if (runningProcesses.Any(p => p.Contains(processName, StringComparison.OrdinalIgnoreCase)))
                {
                    isRunning = true;
                    details.Add($"Process running: {processName}");
                }
            }

            // Check if installed
            var installationLocations = FindInstallations(tool).ToList();
            if (installationLocations.Count > 0)
            {
                isInstalled = true;
                foreach (var location in installationLocations)
                {
                    details.Add($"Installed at: {location}");
                }
            }

            if (isRunning || isInstalled)
            {
                string status = isRunning ? "⚠️ RUNNING" : "📦 Installed";
                result.DetectedTools.Add($"{tool.Name} ({status})");
                result.DetectionDetails[tool.Name] = string.Join(", ", details);
            }
        }

        return result;
    }

    private static List<string> GetRunningProcesses()
    {
        var processNames = new List<string>();
        try
        {
            var processes = Process.GetProcesses();
            processNames.AddRange(processes.Select(p => p.ProcessName.ToLowerInvariant()));
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error getting processes: {ex.Message}");
        }
        return processNames;
    }

    private static IEnumerable<string> FindInstallations(RemoteTool tool)
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            return FindWindowsInstallations(tool);
        }

        if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            return FindMacInstallations(tool);
        }

        return Enumerable.Empty<string>();
    }

    private static IEnumerable<string> FindWindowsInstallations(RemoteTool tool)
    {
        if (tool.WindowsInstallMarkers.Length == 0)
            return Enumerable.Empty<string>();

        var results = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var baseDirectories = new[]
        {
            Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles),
            Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86)
        }
        .Where(path => !string.IsNullOrWhiteSpace(path) && Directory.Exists(path));

        foreach (var marker in tool.WindowsInstallMarkers)
        {
            if (string.IsNullOrWhiteSpace(marker))
                continue;

            var expectsFile = marker.EndsWith(".exe", StringComparison.OrdinalIgnoreCase);

            if (Path.IsPathRooted(marker))
            {
                if (TryResolvePath(marker, expectsFile, out var resolved))
                {
                    results.Add(resolved);
                }
                continue;
            }

            foreach (var baseDir in baseDirectories)
            {
                var candidate = Path.Combine(baseDir, marker);
                if (TryResolvePath(candidate, expectsFile, out var resolved))
                {
                    results.Add(resolved);
                    break;
                }
            }
        }

        return results;
    }

    private static IEnumerable<string> FindMacInstallations(RemoteTool tool)
    {
        if (tool.MacInstallMarkers.Length == 0)
            return Enumerable.Empty<string>();

        var results = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var homeDir = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
        var baseDirectories = new List<string> { "/Applications" };

        if (!string.IsNullOrWhiteSpace(homeDir))
        {
            baseDirectories.Add(Path.Combine(homeDir, "Applications"));
        }

        foreach (var marker in tool.MacInstallMarkers)
        {
            if (string.IsNullOrWhiteSpace(marker))
                continue;

            if (Path.IsPathRooted(marker))
            {
                if (Directory.Exists(marker))
                {
                    results.Add(marker);
                }
                continue;
            }

            foreach (var baseDir in baseDirectories)
            {
                var candidate = Path.Combine(baseDir, marker);
                if (Directory.Exists(candidate))
                {
                    results.Add(candidate);
                    break;
                }
            }
        }

        return results;
    }

    private static bool TryResolvePath(string candidate, bool expectsFile, out string resolvedPath)
    {
        try
        {
            if (expectsFile)
            {
                if (File.Exists(candidate))
                {
                    resolvedPath = candidate;
                    return true;
                }
            }
            else
            {
                if (Directory.Exists(candidate))
                {
                    resolvedPath = candidate;
                    return true;
                }
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error checking path {candidate}: {ex.Message}");
        }

        resolvedPath = string.Empty;
        return false;
    }
}
