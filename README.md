# Fava Studio

A modern Windows IDE for the Fava language, built with WPF (.NET 8), focused on productivity, diagnostics, and compiler workflow.

## ✨ IDE Highlights

### Core IDE Experience
- Dark, professional UI optimized for coding sessions.
- Fast project explorer with file/folder management.
- AvalonEdit-powered code editor for `.fava` and text files.
- Live diagnostics panel with inline underline rendering.
- Integrated run pipeline for the current file.

### New Productivity Features (this branch)
1. **Welcome Start Screen**
   - On startup (when no project is loaded), users now see a start experience with:
     - **Create New Project**
     - **Open Project**
     - **Recent Projects**
2. **Recent Projects (MRU)**
   - Automatically tracks recently opened projects.
   - Accessible from the welcome screen and **Project → Recent Projects**.
3. **Recent Files (MRU)**
   - Tracks opened/saved files.
   - Accessible from **Project → Recent Files**.
4. **Quick Open Palette**
   - **Ctrl+P** (or **Project → Quick Open**) to quickly filter/open project files.
   - Supports `.fava` and `.txt` file lookup.
5. **Unsaved Changes Guard + File Dirty State**
   - Current file name shows `*` when modified.
   - Save/discard/cancel prompt when switching file/project with unsaved changes.

### Existing Tooling
- **Tools → Open Test Tool** for pair-based compare workflows.
- **Tools → Open Visualizer** for VM/stack execution analysis.
- Click **Fava Studio** in header to return to editor quickly.

---

## 📸 Screenshots

### Welcome Screen + Recent Projects
![Welcome Screen](https://github.com/user-attachments/assets/aed53368-21d2-41d2-b402-7ea5a36284f7)

### Header + Tools Navigation (Open Test Tool + Open Visualizer)
![Header Tools Menu](https://github.com/user-attachments/assets/10286fa4-6942-45bc-a699-0c7d85b9645f)

---

## 🧰 Requirements

- Windows 10/11
- .NET 8 SDK (for building from source)
- Java (on PATH, or configured in Settings)
- FavaCompiler repository
- `antlr-4.13.2-complete.jar`

---

## 🚀 Run From Source

1. Open `FavaStudio.sln` in Visual Studio 2022.
2. Restore NuGet packages.
3. Build and run (`F5`).

CLI build (Linux/macOS/CI compatibility mode for WPF targeting metadata):

```bash
dotnet build FavaStudio.sln -p:EnableWindowsTargeting=true
```

---

## 📦 Release-Ready Packaging

This repository now includes release automation and local publish scripts.

### Local publish (Windows x64 self-contained)

PowerShell:

```powershell
./scripts/publish-win-x64.ps1
```

Bash:

```bash
./scripts/publish-win-x64.sh
```

Outputs are placed in `publish/win-x64`.

### GitHub Release workflow

- Workflow file: `.github/workflows/release.yml`
- Triggers:
  - Push tags like `v1.0.0`
  - Manual `workflow_dispatch`
- Produces:
  - `FavaStudio-win-x64.zip` artifact
  - Automatic GitHub Release asset upload on tag builds

---

## ⚙️ Settings

Settings are stored in:

- `%AppData%\FavaStudio\settings.json`

Key options:

| Setting | Description |
|---------|-------------|
| Java Path | Path to `java.exe` (or `java` on PATH) |
| Compiler Root | Path to your FavaCompiler folder |
| ANTLR Jar | Path to `antlr-4.13.2-complete.jar` |
| RecentProjects | Most recently used project folders |
| RecentFiles | Most recently used files |

---

## 📁 Project Structure

```
FavaStudio/
  FavaStudio.sln
  README.md
  .github/
    workflows/
      release.yml
  scripts/
    publish-win-x64.ps1
    publish-win-x64.sh
  src/
    FavaStudio/
      App.xaml / App.xaml.cs
      MainWindow.xaml / MainWindow.xaml.cs
      FavaStudio.csproj
      Themes/
        Theme.xaml
      Models/
      Services/
      ViewModels/
```
