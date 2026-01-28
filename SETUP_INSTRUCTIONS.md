# SETUP INSTRUCTIONS

## Quick Start Guide for Secure Check App

### Step 1: Verify Prerequisites
Open PowerShell and check if .NET 8 SDK is installed:
```powershell
dotnet --version
```
Should show version 8.x.x. If not, download from: https://dotnet.microsoft.com/download/dotnet/8.0

### Step 2: Navigate to Project Directory
```powershell
cd "d:\Projects\Security Check App\SecureCheckApp"
```

### Step 3: Restore Dependencies
```powershell
dotnet restore
```
This downloads all required NuGet packages (Avalonia UI, etc.)

### Step 4: Build the Project
```powershell
dotnet build
```
This compiles the application and checks for errors.

### Step 5: Run the Application
```powershell
dotnet run
```
The Secure Check App window should appear!

---

## Alternative: Using Visual Studio or Rider

### Visual Studio 2022
1. Open Visual Studio 2022
2. File → Open → Project/Solution
3. Navigate to: `d:\Projects\Security Check App\SecureCheckApp\SecureCheckApp.csproj`
4. Press F5 to run

### JetBrains Rider
1. Open Rider
2. File → Open
3. Select: `d:\Projects\Security Check App\SecureCheckApp`
4. Click the Run button (▶️)

---

## Building Release Version

For a standalone executable:
```powershell
dotnet publish -c Release -r win-x64 --self-contained -p:PublishSingleFile=true
```

The .exe will be in: `bin\Release\net8.0\win-x64\publish\`

---

## Project File Structure
```
SecureCheckApp/
├── SecureCheckApp.csproj         # Project configuration
├── Program.cs                    # Application entry point
├── App.axaml                     # Application resources
├── App.axaml.cs                  # Application code
├── app.manifest                  # Windows manifest
├── Commands/
│   └── RelayCommand.cs           # Command pattern implementation
├── Services/
│   ├── RemoteToolDetector.cs     # Core detection logic
│   └── NetworkInfo.cs            # Network utilities
├── ViewModels/
│   ├── ViewModelBase.cs          # Base ViewModel class
│   └── MainWindowViewModel.cs    # Main window business logic
└── Views/
    ├── MainWindow.axaml          # UI layout (XAML)
    └── MainWindow.axaml.cs       # UI code-behind
```

---

## Testing the App

### Test Case 1: Basic Functionality
1. Click "Run Security Check"
2. Verify status changes to "Checking..."
3. Verify IPv4 address is displayed
4. Verify results show "No remote tools detected" (if you don't have any installed)

### Test Case 2: Refresh Function
1. Click "Refresh" button
2. Verify IP address is re-read
3. Verify check runs again

### Test Case 3: With Remote Tools (if available)
1. Install AnyDesk or TeamViewer (optional)
2. Run the app
3. Click "Run Security Check"
4. Verify it detects the installed tool
5. Verify warning message appears

---

## Adding More Software to Detect

Edit: `Services\RemoteToolDetector.cs`

Find the `RemoteTools` list (around line 15) and add:

```csharp
new RemoteTool(
    "Your Software Name",
    new[] { "process_name_1", "process_name_2" },  // lowercase process names
    new[] { "InstallFolderName", "AppName.app" }   // folder or .app name
),
```

**Example - Adding Zoom (if you want to detect it):**
```csharp
new RemoteTool(
    "Zoom",
    new[] { "zoom", "zoomhd" },
    new[] { "Zoom", "zoom.us.app" }
),
```

**Finding Process Names:**
- Windows: Task Manager → Details tab
- macOS: Activity Monitor

**Finding Install Paths:**
- Windows: Check `C:\Program Files\` or `C:\Program Files (x86)\`
- macOS: Check `/Applications/`

---

## Troubleshooting

### Error: "Avalonia not found"
Run: `dotnet restore`

### Error: "SDK not found"
Install .NET 8 SDK from: https://dotnet.microsoft.com/download

### App window is blank
- Check if there are build errors: `dotnet build`
- Check the Output window in Visual Studio

### Detection not working
- Run as regular user (no admin needed)
- Check process names are correct (lowercase)
- Check install paths match your system

### macOS: "Cannot open SecureCheckApp"
```bash
chmod +x SecureCheckApp
# Or right-click → Open (first time)
```

---

## What Each File Does

| File | Purpose |
|------|---------|
| **Program.cs** | Entry point, starts the Avalonia application |
| **App.axaml[.cs]** | Application-level configuration and theming |
| **MainWindow.axaml** | UI layout (buttons, text, styling) |
| **MainWindow.axaml.cs** | Connects UI to ViewModel |
| **MainWindowViewModel.cs** | Business logic, commands, state management |
| **RelayCommand.cs** | Enables button click handling (ICommand) |
| **RemoteToolDetector.cs** | Scans for remote control software |
| **NetworkInfo.cs** | Reads IPv4 address from network adapters |
| **ViewModelBase.cs** | Base class for property change notifications |

---

## Next Steps

1. **Customize Detection**: Add more software to detect
2. **Improve UI**: Modify colors, fonts in MainWindow.axaml
3. **Export Report**: Add functionality to save results to a file
4. **Scheduling**: Add a "Check on Startup" option
5. **Logging**: Add detailed logging to a text file

---

## Need Help?

- Avalonia Docs: https://docs.avaloniaui.net/
- .NET Docs: https://learn.microsoft.com/en-us/dotnet/
- Check the README.md for more details

---

**You're all set! Run `dotnet run` to start the app.**
