param(
  [ValidateSet('Debug','Release')]
  [string]$Configuration = 'Release'
)

$ErrorActionPreference = 'Stop'
Write-Host "Building Roll On Injector..." -ForegroundColor Cyan
dotnet restore
dotnet publish -c $Configuration -r win-x64 --self-contained true -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true
Write-Host "Done." -ForegroundColor Green
