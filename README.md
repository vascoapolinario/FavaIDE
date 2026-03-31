# Fava Studio

A Windows‑native IDE for the Fava language, built with WPF (.NET 8).

## Features

- Modern dark UI
- Code editor with syntax highlighting (AvalonEdit)
- Project explorer (`.fava` files) with always-available project directory
- Run current file via Java compiler
- Diagnostics panel with live error visibility
- Bottom output panel for compiler/runtime output
- Uses Java compiler in background
- Settings stored in `%AppData%\FavaStudio\settings.json`

## Requirements

- .NET 8 SDK
- Java installed and accessible on PATH (or set the path in Settings)
- Your FavaCompiler repository
- `antlr-4.13.2-complete.jar`

## Build & Run

1. Open `FavaStudio.sln` in Visual Studio 2022
2. Restore NuGet packages (automatic on first build)
3. Build and run (`F5`)

## Settings

Fill in the Settings tab inside the app:

| Setting | Description |
|---------|-------------|
| Java Path | Path to `java.exe` (or `java` if on PATH) |
| Compiler Root | Path to your FavaCompiler folder |
| ANTLR Jar Path | Path to `antlr-4.13.2-complete.jar` |

## Project Structure

```
FavaStudio/
  FavaStudio.sln
  src/
    FavaStudio/
      App.xaml / App.xaml.cs
      MainWindow.xaml / MainWindow.xaml.cs
      FavaStudio.csproj
      Themes/
        Theme.xaml
      Models/
        ProjectConfig.cs
        TestCase.cs
        TestResult.cs
      Services/
        SettingsService.cs
        JavaCompilerService.cs
        TestRunnerService.cs
        FileService.cs
      ViewModels/
        MainViewModel.cs
        RelayCommand.cs
```
