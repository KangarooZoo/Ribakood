# How to Run in Debug Mode and View Logs

## Option 1: Visual Studio (Recommended)

1. **Open the project in Visual Studio:**
   - Open Visual Studio 2022
   - File → Open → Project/Solution
   - Navigate to `BarcodeApp.csproj` and open it

2. **Set Debug Configuration:**
   - In the toolbar, ensure "Debug" is selected (not "Release")
   - Select the target framework: `net8.0-windows`

3. **View Debug Output:**
   - Go to: **View → Output** (or press `Ctrl+Alt+O`)
   - In the "Show output from:" dropdown, select **"Debug"**
   - This will show all `Debug.WriteLine()` messages

4. **Run the Application:**
   - Press `F5` to start debugging, or
   - Click the green "Start" button, or
   - Press `Ctrl+F5` to run without debugging (logs still appear)

5. **Set Breakpoints (Optional):**
   - Click in the left margin next to any line of code
   - When execution reaches that line, it will pause
   - You can inspect variables, step through code, etc.

## Option 2: Visual Studio Code

1. **Install Extensions:**
   - C# (by Microsoft)
   - .NET Extension Pack

2. **Open the Project:**
   - File → Open Folder → Select the `BarcodeApp` folder

3. **Create Launch Configuration:**
   - Press `F5` or go to Run → Add Configuration
   - Select ".NET 5+ and .NET Core"
   - This creates `.vscode/launch.json`

4. **View Debug Output:**
   - Open the "Debug Console" tab (View → Debug Console)
   - Or open the "Output" panel (View → Output)
   - Select "Debug Console" from the dropdown

5. **Run:**
   - Press `F5` to start debugging

## Option 3: Command Line with Debug Output

1. **Build in Debug mode:**
   ```powershell
   dotnet build --configuration Debug
   ```

2. **Run with debug output visible:**
   - For WPF apps, debug output goes to Visual Studio's Output window
   - To see it in console, you can use DebugView (see below)

3. **Use DebugView (Windows):**
   - Download DebugView from Microsoft Sysinternals
   - Run DebugView as Administrator
   - It will capture all `Debug.WriteLine()` output
   - Filter by process name: `BarcodeApp`

## Option 4: Add Console Output (Alternative)

If you want to see logs in a console window, you can temporarily add:

```csharp
Console.WriteLine($"UpdatePreview called: InputData='{InputData}'...");
```

But note: WPF apps don't show a console by default. You'd need to change the project type or use DebugView.

## Quick Debug Checklist

- ✅ Build configuration set to "Debug"
- ✅ Output window open in Visual Studio
- ✅ "Show output from:" set to "Debug"
- ✅ Application running (F5)
- ✅ Enter barcode data to trigger logs

## What Logs to Look For

When you enter barcode data, you should see:
1. `UpdatePreview called: InputData='...'`
2. `Bitmap generated successfully: Width=..., Height=...`
3. `MemoryStream created: Length=... bytes`
4. `BitmapImage created: Width=..., Height=...`
5. `PreviewImage set successfully. Width=..., Height=...`

If you see errors instead, they will show the exception details.

