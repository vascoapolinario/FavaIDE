param(
    [string]$Configuration = "Release",
    [string]$OutputDir = "./publish/win-x64"
)

$ErrorActionPreference = "Stop"

Write-Host "Publishing FavaStudio ($Configuration) for win-x64..."
dotnet publish ./src/FavaStudio/FavaStudio.csproj `
  -c $Configuration `
  -r win-x64 `
  --self-contained true `
  -p:PublishSingleFile=true `
  -p:IncludeNativeLibrariesForSelfExtract=true `
  -o $OutputDir

if ($LASTEXITCODE -ne 0) {
    throw "dotnet publish failed with exit code $LASTEXITCODE"
}

Write-Host "Done. Output: $OutputDir"
