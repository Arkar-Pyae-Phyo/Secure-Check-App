# Secure Check App

A cross-platform desktop security check application built with .NET 8 and Avalonia UI 11.

## Overview
This app detects remote control software on your computer (like AnyDesk, TeamViewer, RustDesk, etc.) and displays your network information. It's designed for transparency and runs completely locally - no data is transmitted.

## Features
- 🔍 Detects running and installed remote control software
- 🌐 Displays your local IPv4 address
- ⚠️ Visual warnings when remote tools are found
- 🔄 Refresh button to re-check
- 🖥️ Cross-platform: Windows & macOS
- 🔒 Privacy-focused: All checks are local, no telemetry

## Security Checks
The app currently detects:
- AnyDesk
- TeamViewer
- Chrome Remote Desktop
- RustDesk
- Splashtop
- LogMeIn
- VNC Server
- Windows Remote Desktop
- Remote Utilities

## How It Works
1. **Process Detection**: Scans running processes for known remote tool names
2. **Installation Detection**: 
   - Windows: Checks Program Files, AppData directories
   - macOS: Checks /Applications folders
3. **Network Info**: Reads IPv4 from active network adapters

## Privacy & Safety
- ✅ All detection is local-only
- ✅ No data sent to any server
- ✅ No background monitoring or auto-start
- ✅ Read-only operations (doesn't kill processes or uninstall software)
- ✅ No admin/elevated privileges required

## Build & Run

### Prerequisites
- .NET 8 SDK installed ([download here](https://dotnet.microsoft.com/download/dotnet/8.0))

### Running the App
```bash
cd SecureCheckApp
dotnet restore
dotnet run
```

### Building Release Version
```bash
dotnet build -c Release
```

The executable will be in: `bin/Release/net8.0/`

### Platform-Specific Builds
```bash
# Windows
dotnet publish -c Release -r win-x64 --self-contained

# macOS (Intel)
dotnet publish -c Release -r osx-x64 --self-contained

# macOS (Apple Silicon)
dotnet publish -c Release -r osx-arm64 --self-contained
```

## Adding More Software to Detect

To add more remote control software to the detection list:

1. Open [Services/RemoteToolDetector.cs](Services/RemoteToolDetector.cs)
2. Find the `RemoteTools` list in the `RemoteToolDetector` class
3. Add a new entry following this pattern:

```csharp
new RemoteTool(
    "Software Name",                           // Display name
    new[] { "process1", "process2" },         // Process names (lowercase)
    new[] { "InstallFolder", "App.app" }      // Windows folder name or macOS .app name
)
```

### Example: Adding UltraVNC
```csharp
new RemoteTool(
    "UltraVNC", 
    new[] { "ultravnc", "winvnc" }, 
    new[] { "UltraVNC", "UltraVNC.app" }
)
```

### Tips for Finding Process Names
- **Windows**: Open Task Manager → Details tab → look for the process
- **macOS**: Open Activity Monitor → search for the app name
- Use lowercase names in the array

### Tips for Finding Install Paths
- **Windows**: Usually the folder name in `C:\Program Files\` or `C:\Program Files (x86)\`
- **macOS**: The `.app` bundle name in `/Applications`

## Project Structure
```
SecureCheckApp/
├── App.axaml                          # Application definition
├── App.axaml.cs                       # Application code-behind
├── Program.cs                         # Entry point
├── SecureCheckApp.csproj              # Project file
├── Commands/
│   └── RelayCommand.cs                # ICommand implementation
├── Services/
│   ├── RemoteToolDetector.cs          # Detection logic
│   └── NetworkInfo.cs                 # Network information
├── ViewModels/
│   ├── ViewModelBase.cs               # Base ViewModel
│   └── MainWindowViewModel.cs         # Main window logic
└── Views/
    ├── MainWindow.axaml               # Main window UI
    └── MainWindow.axaml.cs            # Main window code-behind
```

## Technology Stack
- **.NET 8**: Modern, cross-platform runtime
- **Avalonia UI 11**: Cross-platform XAML-based UI framework
- **MVVM Pattern**: Clean separation of concerns
- **Async/Await**: Responsive UI during checks

## License
This is example code provided for educational purposes. Feel free to modify and use as needed.

## Troubleshooting

### App won't start
- Ensure .NET 8 SDK is installed: `dotnet --version`
- Try: `dotnet clean` then `dotnet restore`

### False negatives (software not detected)
- Some software may use different process names or install locations
- Add specific process/path names to the detector (see "Adding More Software" above)

### macOS: "App can't be opened"
- Run: `chmod +x SecureCheckApp` to make it executable
- Or: Right-click → Open (first time only)

## Contributing
Feel free to add more remote tools to detect or improve the detection logic. The code is structured to make additions easy.
