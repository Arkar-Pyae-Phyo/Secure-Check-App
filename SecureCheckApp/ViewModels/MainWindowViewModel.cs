using System;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using SecureCheckApp.Commands;
using SecureCheckApp.Services;

namespace SecureCheckApp.ViewModels;

public class MainWindowViewModel : ViewModelBase
{
    private string _statusText = "⏳ Status: Waiting";
    private string _ipv4Address = "IPv4: -";
    private string _detectionResults = "Click 'Run Security Check' to begin...";
    private bool _hasWarnings;
    private bool _isChecking;
    private string _resultPanelBackground = "#F0F9FF";
    private string _resultPanelBorderBrush = "#06B6D4";
    private string _generatedCode = "";
    private bool _showContinueButton = false;
    private readonly ApiService _apiService;

    public MainWindowViewModel()
    {
        _apiService = new ApiService();
        RunSecurityCheckCommand = new RelayCommand(RunSecurityCheckAsync, () => !IsChecking);
        RefreshCommand = new RelayCommand(RefreshAsync, () => !IsChecking);
        GenerateCodeCommand = new RelayCommand(GenerateAndUploadCodeAsync, () => !IsChecking);
        ContinueToExamCommand = new RelayCommand(ContinueToExamAsync, () => !IsChecking);
    }

    public string StatusText
    {
        get => _statusText;
        set => SetProperty(ref _statusText, value);
    }

    public string IPv4Address
    {
        get => _ipv4Address;
        set => SetProperty(ref _ipv4Address, value);
    }

    public string DetectionResults
    {
        get => _detectionResults;
        set => SetProperty(ref _detectionResults, value);
    }

    public string ResultPanelBackground
    {
        get => _resultPanelBackground;
        set => SetProperty(ref _resultPanelBackground, value);
    }

    public string ResultPanelBorderBrush
    {
        get => _resultPanelBorderBrush;
        set => SetProperty(ref _resultPanelBorderBrush, value);
    }

    public string GeneratedCode
    {
        get => _generatedCode;
        set => SetProperty(ref _generatedCode, value);
    }

    public bool ShowContinueButton
    {
        get => _showContinueButton;
        set => SetProperty(ref _showContinueButton, value);
    }

    public bool HasWarnings
    {
        get => _hasWarnings;
        set
        {
            if (SetProperty(ref _hasWarnings, value))
            {
                ResultPanelBackground = value ? "#FEF3C7" : "#F0F9FF";
                ResultPanelBorderBrush = value ? "#F59E0B" : "#06B6D4";
            }
        }
    }

    public bool IsChecking
    {
        get => _isChecking;
        set
        {
            if (SetProperty(ref _isChecking, value))
            {
                (RunSecurityCheckCommand as RelayCommand)?.RaiseCanExecuteChanged();
                (RefreshCommand as RelayCommand)?.RaiseCanExecuteChanged();
                (GenerateCodeCommand as RelayCommand)?.RaiseCanExecuteChanged();
                (ContinueToExamCommand as RelayCommand)?.RaiseCanExecuteChanged();
            }
        }
    }

    public ICommand RunSecurityCheckCommand { get; }
    public ICommand RefreshCommand { get; }
    public ICommand GenerateCodeCommand { get; }
    public ICommand ContinueToExamCommand { get; }

    private async Task GenerateAndUploadCodeAsync()
    {
        if (IsChecking)
            return;

        IsChecking = true;
        StatusText = "🔄 Status: Generating code...";

        try
        {
            var code = CodeGenerator.GenerateCode();
            GeneratedCode = code;

            var ipAddress = NetworkInfo.GetLocalIPv4();
            StatusText = "📤 Status: Uploading to database...";

            var result = await _apiService.UploadCodeAsync(code, ipAddress);

            if (result != null)
            {
                StatusText = "✅ Status: Code uploaded successfully";
                DetectionResults = $"Generated Code: {code}\n\n" +
                                 $"Token: {result.Token}\n" +
                                 $"IPv4: {result.IPv4}\n" +
                                 $"Expires: {result.ExpiresAt:yyyy-MM-dd HH:mm:ss}";
                HasWarnings = false;
            }
            else
            {
                StatusText = "❌ Status: Upload failed";
                DetectionResults = $"Generated Code: {code}\n\n" +
                                 "Failed to upload to database.\n" +
                                 "Please check your internet connection.";
                HasWarnings = true;
            }
        }
        catch (Exception ex)
        {
            StatusText = "❌ Status: Error";
            DetectionResults = $"Error generating code:\n{ex.Message}";
            HasWarnings = true;
        }
        finally
        {
            IsChecking = false;
        }
    }

    private async Task RunSecurityCheckAsync()
    {
        if (IsChecking)
            return;

        IsChecking = true;
        StatusText = "⏳ Status: Checking...";
        DetectionResults = "Running security check, please wait...";
        HasWarnings = false;

        try
        {
            var detectionData = await Task.Run(() =>
            {
                var ipAddress = NetworkInfo.GetLocalIPv4();
                var detectionResult = RemoteToolDetector.DetectRemoteTools();
                return (ipAddress, detectionResult);
            });

            IPv4Address = $"IPv4: {detectionData.ipAddress}";

            var detectionResult = detectionData.detectionResult;
            var resultsBuilder = new StringBuilder();
            resultsBuilder.AppendLine("🔍 Security Check Completed");
            resultsBuilder.AppendLine($"⏰ Last Check: {detectionResult.CheckTime:yyyy-MM-dd HH:mm:ss}");
            resultsBuilder.AppendLine("━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");
            resultsBuilder.AppendLine();

            if (detectionResult.HasWarnings)
            {
                HasWarnings = true;
                ShowContinueButton = false;
                StatusText = "⚠️ Status: Warning - Remote tools detected!";
                resultsBuilder.AppendLine($"⚠️ DETECTED REMOTE TOOLS ({detectionResult.DetectedTools.Count}):");
                resultsBuilder.AppendLine();

                foreach (var tool in detectionResult.DetectedTools)
                {
                    resultsBuilder.AppendLine($"  • {tool}");
                }

                resultsBuilder.AppendLine();
                resultsBuilder.AppendLine("━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");
                resultsBuilder.AppendLine("⚠️ SECURITY WARNING:");
                resultsBuilder.AppendLine("Remote access tools can allow others to control");
                resultsBuilder.AppendLine("your computer. Ensure you trust any remote sessions.");
            }
            else
            {
                HasWarnings = false;
                ShowContinueButton = true;
                StatusText = "✅ Status: Completed - No threats detected";
                resultsBuilder.AppendLine("✅ NO REMOTE TOOLS DETECTED");
                resultsBuilder.AppendLine();
                resultsBuilder.AppendLine("Your system appears clean. No common remote");
                resultsBuilder.AppendLine("control software was found running or installed.");
                resultsBuilder.AppendLine();
                resultsBuilder.AppendLine("━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");
                resultsBuilder.AppendLine("✅ You can now continue to the exam.");
            }

            DetectionResults = resultsBuilder.ToString();
            await Task.Delay(300);
        }
        catch (Exception ex)
        {
            StatusText = "❌ Status: Error";
            DetectionResults = $"Error during check:\n{ex.Message}";
            HasWarnings = true;
            ShowContinueButton = false;
        }
        finally
        {
            IsChecking = false;
        }
    }

    private async Task ContinueToExamAsync()
    {
        if (IsChecking)
            return;

        IsChecking = true;
        StatusText = "🔄 Status: Generating exam code...";

        try
        {
            var code = CodeGenerator.GenerateCode();
            GeneratedCode = code;

            var ipAddress = NetworkInfo.GetLocalIPv4();
            StatusText = "📤 Status: Uploading to server...";

            var result = await _apiService.UploadCodeAsync(code, ipAddress);

            if (result != null)
            {
                StatusText = "✅ Status: Ready for exam!";
                DetectionResults = $"✅ EXAM ACCESS GRANTED\n\n" +
                                 $"Generated Code: {code}\n\n" +
                                 $"Token: {result.Token}\n" +
                                 $"IPv4: {result.IPv4}\n" +
                                 $"Expires: {result.ExpiresAt:yyyy-MM-dd HH:mm:ss}\n\n" +
                                 $"━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━\n" +
                                 $"Opening exam page...";
                HasWarnings = false;
                ShowContinueButton = false;
            }
            else
            {
                StatusText = "⚠️ Status: Proceeding to exam";
                DetectionResults = $"Generated Code: {code}\n\n" +
                                 $"IPv4: {ipAddress}\n\n" +
                                 $"⚠️ Could not connect to server.\n" +
                                 $"Opening exam page anyway...\n\n" +
                                 $"━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━\n" +
                                 $"Please provide your code to the exam system.";
                HasWarnings = true;
                ShowContinueButton = false;
            }

            // Always open the exam page in the default browser
            try
            {
                var psi = new ProcessStartInfo
                {
                    FileName = "https://mse.mfu.ac.th/landing",
                    UseShellExecute = true
                };
                Process.Start(psi);
            }
            catch
            {
                DetectionResults += $"\n\n❌ Could not open browser automatically.\n" +
                                  $"Please manually open: https://mse.mfu.ac.th/landing";
            }
        }
        catch (Exception ex)
        {
            StatusText = "❌ Status: Error";
            DetectionResults = $"Error generating code:\n{ex.Message}\n\n" +
                             $"Please try again or contact support.";
            HasWarnings = true;
        }
        finally
        {
            IsChecking = false;
        }
    }

    private async Task RefreshAsync()
    {
        if (IsChecking)
            return;

        StatusText = "🔄 Status: Refreshing...";
        await RunSecurityCheckAsync();
    }
}
