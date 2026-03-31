# Fava Studio

A Windows‑native IDE for the Fava language, built with WPF (.NET 8).

## Features

- Modern dark UI
- Code editor with syntax highlighting (AvalonEdit)
- Project explorer (`.fava` files)
- Run current file via Java compiler
- Test runner comparing Inputs/Outputs
- Captures output and compares against expected
- Uses Java compiler in background
- Settings stored in `%AppData%\FavaStudio\settings.json`
- Pass/fail summary

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
| Inputs Folder | Folder containing `*.fava` test input files |
| Outputs Folder | Folder containing `*.txt` expected output files |

## Tests

Place test files as:
- `Inputs/<name>.fava` — input Fava source
- `Outputs/<name>.txt` — expected output text

Click **Run Tests** to run all tests and see pass/fail results.

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