# Fava Studio

Fava Studio is a desktop IDE for the Fava language, built with WPF on .NET 8.
It combines editing, diagnostics, test workflows, debugging, and VM visualization in one place so you can go from writing code to validating output fast.

## Why Fava Studio

- Clean dark interface built for longer coding sessions.
- End-to-end workflow: edit → run → inspect errors → debug → verify tests.
- Dedicated tools for both everyday coding and low-level VM understanding.

---

## Feature Overview

### Editor & Navigation

- **Project explorer** with folder/file creation and deletion actions.
- **Multi-tab editor** with dirty markers (`*`) and close controls.
- **Quick Open palette** (`Ctrl+P`) to instantly filter and open `.fava` / `.txt` files.
- **Find/Replace/Go To Line** bar with next/previous navigation, case match, and whole-word search.
- **Context actions** in the editor for formatting, line operations, comments, save, run, and breakpoint toggling.

### Diagnostics & Error Feedback

- **Live diagnostics** while typing (lexer/parser/semantic errors).
- **Inline error underline rendering** directly in the editor.
- **Errors panel** with line/column and message details.
- **Hover tooltips** on underlined code for quick error context.
- **Unsaved changes popups** when switching files/projects, with save/discard/cancel options.

### Run, Debug & Breakpoints

- One-click **Run Current File** flow.
- **Inline debug mode** with:
  - Breakpoint toggling (`F9`)
  - Start (`F5`), step (`F10`), continue, step back, jump-to-call, and stop (`Shift+F5`)
- Dedicated debug panel with current instruction, stack state, and execution status.

### Testing Workflows

- Built-in **Test Suite** panel in the workspace.
- Run **all tests** or **selected test**.
- Pass/fail counters, progress bar, and per-test duration.
- Detailed **Diff / Expected / Actual** result views.
- Quick actions to open test input/output files.
- **Create New Test** directly from the IDE flow.

### Tools & Analysis

- **Test Compare Tool** for folder-based input/output pairing and batch comparisons.
- **Visualizer • Stack Explorer** with:
  - Instruction stream filtering
  - Constant pool filtering
  - Stack and globals views
  - Timeline (before/after stack evolution)
  - VM output simulation
  - Opcode reference search

### Workspace & Settings

- Startup **welcome screen** with create/open project actions.
- **Recent projects** and **recent files** tracking.
- Central settings for Java path, compiler root, ANTLR jar, test folders, and output preferences.

---

## Screenshots

### Welcome screen and recent projects
![Welcome Screen](https://github.com/user-attachments/assets/aed53368-21d2-41d2-b402-7ea5a36284f7)

### Main workspace header (run, tests, visualizer access)
![Header Tools Menu](https://github.com/user-attachments/assets/10286fa4-6942-45bc-a699-0c7d85b9645f)

---

## Requirements

- Windows 10/11
- .NET 8 SDK (for building from source)
- Java (on PATH, or configured in Settings)
- FavaCompiler repository
- `antlr-4.13.2-complete.jar`

---

## Run from Source

1. Open `FavaStudio.sln` in Visual Studio 2022.
2. Restore NuGet packages.
3. Build and run (`F5`).

CLI build (Linux/macOS/CI compatibility mode for WPF targeting metadata):

```bash
dotnet build FavaStudio.sln -p:EnableWindowsTargeting=true
```

---

## Packaging and Releases

Local publish (Windows x64 self-contained):

PowerShell:

```powershell
./scripts/publish-win-x64.ps1
```

Bash:

```bash
./scripts/publish-win-x64.sh
```

Output is generated in `publish/win-x64`.

GitHub Release workflow:

- Workflow: `.github/workflows/release.yml`
- Triggers:
  - Push tags like `v1.0.0`
  - Manual `workflow_dispatch`
- Produces:
  - `FavaStudio-win-x64.zip` artifact
  - Automatic GitHub Release asset upload on tag builds

---

## Settings Location

Settings are stored at:

- `%AppData%\FavaStudio\settings.json`

Key values include Java path, compiler root, ANTLR jar, recent history, and test folder paths.
