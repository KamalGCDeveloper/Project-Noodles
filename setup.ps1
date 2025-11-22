# Noodle API Complete Setup Script (PowerShell)
# This script performs complete setup including .NET installation
# Run as Administrator for best results

# Note: This script may require administrator privileges for .NET installation
$isAdmin = ([Security.Principal.WindowsPrincipal] [Security.Principal.WindowsIdentity]::GetCurrent()).IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)

if (-not $isAdmin) {
    Write-Host "⚠️  Warning: Running without administrator privileges." -ForegroundColor Yellow
    Write-Host "Some installation steps may require administrator rights." -ForegroundColor Yellow
    Write-Host ""
}

$ErrorActionPreference = "Continue"

Write-Host "🍜 Noodle API Complete Setup Script" -ForegroundColor Cyan
Write-Host "=====================================" -ForegroundColor Cyan
Write-Host ""

# Function to install .NET on Windows
function Install-DotNetSDK {
    Write-Host "📥 Downloading .NET 9.0 SDK for Windows..." -ForegroundColor Cyan
    
    # Detect architecture
    $arch = $env:PROCESSOR_ARCHITECTURE
    if ($arch -eq "AMD64") {
        $installerUrl = "https://download.visualstudio.microsoft.com/download/pr/9c97c9c6-c42a-4f56-b9d9-c1c1ff43a8e0/ed2830cc82c5b4b01c4e7f6a5b7c9e30/dotnet-sdk-9.0.100-win-x64.exe"
        Write-Host "Detected: x64 architecture" -ForegroundColor Cyan
    }
    elseif ($arch -eq "ARM64") {
        $installerUrl = "https://download.visualstudio.microsoft.com/download/pr/6b1c1dbb-d6d3-48b3-97d8-a84be43b7088/1a917e80b58e9f94660c00e650bb7be5/dotnet-sdk-9.0.100-win-arm64.exe"
        Write-Host "Detected: ARM64 architecture" -ForegroundColor Cyan
    }
    else {
        $installerUrl = "https://download.visualstudio.microsoft.com/download/pr/e7bd7a06-bcad-4249-ad28-c2a0e7cdfd50/0b8f55a1d0a36b6efdd9cd60d7a91b36/dotnet-sdk-9.0.100-win-x86.exe"
        Write-Host "Detected: x86 architecture" -ForegroundColor Cyan
    }
    
    # Download installer
    $installerPath = "$env:TEMP\dotnet-sdk-installer.exe"
    Write-Host "Downloading to: $installerPath" -ForegroundColor Gray
    
    try {
        $ProgressPreference = 'SilentlyContinue'
        Invoke-WebRequest -Uri $installerUrl -OutFile $installerPath -UseBasicParsing
        $ProgressPreference = 'Continue'
        Write-Host "✅ Download complete" -ForegroundColor Green
    }
    catch {
        Write-Host "❌ Failed to download .NET SDK installer" -ForegroundColor Red
        Write-Host $_.Exception.Message -ForegroundColor Red
        return $false
    }
    
    # Install .NET
    Write-Host "🔧 Installing .NET 9.0 SDK..." -ForegroundColor Cyan
    Write-Host "Note: This may take a few minutes." -ForegroundColor Gray
    
    try {
        Start-Process -FilePath $installerPath -ArgumentList "/install", "/quiet", "/norestart" -Wait -NoNewWindow
        Write-Host "✅ .NET 9.0 SDK installed successfully!" -ForegroundColor Green
        
        # Clean up
        Remove-Item $installerPath -Force -ErrorAction SilentlyContinue
        
        # Refresh PATH
        $env:Path = [System.Environment]::GetEnvironmentVariable("Path", "Machine") + ";" + [System.Environment]::GetEnvironmentVariable("Path", "User")
        
        return $true
    }
    catch {
        Write-Host "❌ Failed to install .NET SDK" -ForegroundColor Red
        Write-Host $_.Exception.Message -ForegroundColor Red
        return $false
    }
}

# Check if .NET is installed
$dotnetInstalled = $false
try {
    $dotnetVersion = dotnet --version 2>$null
    if ($LASTEXITCODE -eq 0 -and $dotnetVersion) {
        $dotnetInstalled = $true
    }
}
catch {
    $dotnetInstalled = $false
}

if (-not $dotnetInstalled) {
    Write-Host "❌ .NET SDK is not installed." -ForegroundColor Red
    Write-Host ""
    
    $response = Read-Host "Would you like to install .NET 9.0 SDK automatically? (y/n)"
    
    if ($response -eq 'y' -or $response -eq 'Y') {
        $installResult = Install-DotNetSDK
        
        if (-not $installResult) {
            Write-Host ""
            Write-Host "Please install .NET 9.0 SDK manually from:" -ForegroundColor Yellow
            Write-Host "https://dotnet.microsoft.com/download/dotnet/9.0" -ForegroundColor Cyan
            exit 1
        }
        
        Write-Host ""
    }
    else {
        Write-Host "Please install .NET 9.0 SDK manually from:" -ForegroundColor Yellow
        Write-Host "https://dotnet.microsoft.com/download/dotnet/9.0" -ForegroundColor Cyan
        exit 1
    }
}

# Verify .NET installation
try {
    $dotnetVersion = dotnet --version
    Write-Host "✅ .NET SDK found: $dotnetVersion" -ForegroundColor Green
    Write-Host ""
    
    # Check if the correct version is installed
    $majorVersion = [int]($dotnetVersion.Split('.')[0])
    if ($majorVersion -lt 9) {
        Write-Host "⚠️  Warning: .NET 9.0 or higher is required." -ForegroundColor Yellow
        Write-Host "Current version: $dotnetVersion" -ForegroundColor Yellow
        Write-Host ""
        
        $response = Read-Host "Would you like to install .NET 9.0 SDK? (y/n)"
        
        if ($response -eq 'y' -or $response -eq 'Y') {
            $installResult = Install-DotNetSDK
            
            if (-not $installResult) {
                Write-Host "Please upgrade to .NET 9.0 or higher" -ForegroundColor Yellow
                exit 1
            }
        }
        else {
            Write-Host "Please upgrade to .NET 9.0 or higher" -ForegroundColor Yellow
            exit 1
        }
    }
}
catch {
    Write-Host "❌ .NET installation failed or not found in PATH" -ForegroundColor Red
    Write-Host "Please restart your terminal and try again, or install manually:" -ForegroundColor Yellow
    Write-Host "https://dotnet.microsoft.com/download/dotnet/9.0" -ForegroundColor Cyan
    exit 1
}

$ErrorActionPreference = "Stop"

# Restore dependencies
Write-Host "📦 Restoring NuGet packages..." -ForegroundColor Cyan
try {
    dotnet restore
    Write-Host "✅ Dependencies restored successfully" -ForegroundColor Green
}
catch {
    Write-Host "❌ Failed to restore dependencies" -ForegroundColor Red
    exit 1
}
Write-Host ""

# Build the project
Write-Host "🔨 Building the project..." -ForegroundColor Cyan
try {
    dotnet build
    Write-Host "✅ Project built successfully" -ForegroundColor Green
}
catch {
    Write-Host "❌ Build failed" -ForegroundColor Red
    exit 1
}
Write-Host ""

# Check for configuration file
if (-Not (Test-Path "appsettings.Development.local.json")) {
    Write-Host "⚠️  appsettings.Development.local.json not found" -ForegroundColor Yellow
    Write-Host "📝 You can use appsettings.Development.json or create a local configuration file" -ForegroundColor Yellow
    Write-Host ""
}

# Check for .env file (for Docker)
if (-Not (Test-Path ".env")) {
    Write-Host "ℹ️  .env file not found (optional for Docker)" -ForegroundColor Cyan
    Write-Host "You can copy .env.example to .env if you plan to use Docker" -ForegroundColor Cyan
    Write-Host ""
}

Write-Host "✅ Setup complete!" -ForegroundColor Green
Write-Host ""
Write-Host "Next steps:" -ForegroundColor Cyan
Write-Host "1. Review your configuration in appsettings.Development.json"
Write-Host "2. Run the application with: dotnet run"
Write-Host "3. Access Swagger UI at: http://localhost:5130/swagger"
Write-Host ""
Write-Host "For more information, see:" -ForegroundColor Cyan
Write-Host "- README.md - Full documentation"
Write-Host "- GETTING_STARTED.md - Quick start guide"
Write-Host "- API_DOCUMENTATION.md - API reference"
Write-Host ""
Write-Host "Happy coding! 🚀" -ForegroundColor Green

