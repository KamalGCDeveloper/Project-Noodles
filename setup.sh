#!/bin/bash

# Noodle API Setup Script
# This script performs complete setup including .NET installation

set -e

echo "🍜 Noodle API Complete Setup Script"
echo "====================================="
echo ""

# Function to install .NET on macOS
install_dotnet_macos() {
    echo "📥 Downloading .NET 9.0 SDK for macOS..."
    
    # Detect CPU architecture
    ARCH=$(uname -m)
    if [ "$ARCH" = "arm64" ]; then
        DOTNET_URL="https://download.visualstudio.microsoft.com/download/pr/0f71eef2-a17d-4c47-baeb-7f1a15de5e56/e0bd2e826e08b6c48ed01daca4ab76e8/dotnet-sdk-9.0.101-osx-arm64.pkg"
        echo "Detected: Apple Silicon (ARM64)"
        EXPECTED_MIN_SIZE=180000000  # ~180 MB minimum
    else
        DOTNET_URL="https://download.visualstudio.microsoft.com/download/pr/50d48c7e-a172-4f27-87a7-cde97ca0d86e/00b2b1e29d3c5f52a0bb61b039fe65e4/dotnet-sdk-9.0.101-osx-x64.pkg"
        echo "Detected: Intel (x64)"
        EXPECTED_MIN_SIZE=180000000  # ~180 MB minimum
    fi
    
    # Download installer with better error handling
    PKG_PATH="/tmp/dotnet-installer.pkg"
    echo "Downloading from Microsoft..."
    
    if curl -fSL -o "$PKG_PATH" "$DOTNET_URL"; then
        # Check if file was actually downloaded and has reasonable size
        if [ -f "$PKG_PATH" ]; then
            FILE_SIZE=$(stat -f%z "$PKG_PATH" 2>/dev/null || echo "0")
            
            if [ "$FILE_SIZE" -lt "$EXPECTED_MIN_SIZE" ]; then
                echo "❌ Download failed: File too small ($FILE_SIZE bytes)"
                echo "Expected at least $EXPECTED_MIN_SIZE bytes"
                rm -f "$PKG_PATH"
                echo ""
                echo "Please install .NET 9.0 manually from:"
                echo "https://dotnet.microsoft.com/download/dotnet/9.0"
                return 1
            fi
            
            # Convert bytes to MB for display
            SIZE_MB=$((FILE_SIZE / 1048576))
            echo "✅ Downloaded successfully (~${SIZE_MB}MB)"
        else
            echo "❌ Download failed: File not created"
            return 1
        fi
    else
        echo "❌ Download failed"
        echo ""
        echo "Please install .NET 9.0 manually from:"
        echo "https://dotnet.microsoft.com/download/dotnet/9.0"
        return 1
    fi
    
    # Install .NET
    echo "🔧 Installing .NET 9.0 SDK..."
    echo "Note: This may require administrator privileges."
    
    if sudo installer -pkg "$PKG_PATH" -target /; then
        echo "✅ .NET 9.0 SDK installed successfully!"
        
        # Clean up
        rm -f "$PKG_PATH"
        
        # Update PATH for current session
        export PATH="$PATH:/usr/local/share/dotnet"
        
        echo ""
        return 0
    else
        echo "❌ Installation failed"
        rm -f "$PKG_PATH"
        return 1
    fi
}

# Function to install .NET on Linux
install_dotnet_linux() {
    echo "📥 Installing .NET 9.0 SDK for Linux..."
    
    # Download and run the Microsoft install script
    INSTALL_SCRIPT="/tmp/dotnet-install.sh"
    
    if curl -fsSL https://dot.net/v1/dotnet-install.sh -o "$INSTALL_SCRIPT"; then
        chmod +x "$INSTALL_SCRIPT"
        
        # Install .NET 9.0
        if "$INSTALL_SCRIPT" --channel 9.0 --install-dir "$HOME/.dotnet"; then
            # Clean up
            rm -f "$INSTALL_SCRIPT"
            
            # Update PATH for current session
            export PATH="$PATH:$HOME/.dotnet"
            export DOTNET_ROOT="$HOME/.dotnet"
            
            # Add to shell profile for persistence
            SHELL_PROFILE="$HOME/.bashrc"
            if [ -f "$HOME/.zshrc" ]; then
                SHELL_PROFILE="$HOME/.zshrc"
            fi
            
            if ! grep -q "DOTNET_ROOT" "$SHELL_PROFILE" 2>/dev/null; then
                echo "" >> "$SHELL_PROFILE"
                echo "# .NET Configuration" >> "$SHELL_PROFILE"
                echo 'export DOTNET_ROOT=$HOME/.dotnet' >> "$SHELL_PROFILE"
                echo 'export PATH=$PATH:$DOTNET_ROOT' >> "$SHELL_PROFILE"
            fi
            
            echo "✅ .NET 9.0 SDK installed successfully!"
            echo "⚠️  Note: You may need to restart your terminal or run: source $SHELL_PROFILE"
            echo ""
            return 0
        else
            echo "❌ Installation failed"
            rm -f "$INSTALL_SCRIPT"
            return 1
        fi
    else
        echo "❌ Failed to download installation script"
        return 1
    fi
}

# Alternative: Use dotnet-install script for macOS too (fallback)
install_dotnet_with_script() {
    echo "📥 Using alternative installation method..."
    
    INSTALL_SCRIPT="/tmp/dotnet-install.sh"
    
    if curl -fsSL https://dot.net/v1/dotnet-install.sh -o "$INSTALL_SCRIPT"; then
        chmod +x "$INSTALL_SCRIPT"
        
        # Install .NET 9.0
        if "$INSTALL_SCRIPT" --channel 9.0 --install-dir "$HOME/.dotnet"; then
            rm -f "$INSTALL_SCRIPT"
            
            # Update PATH for current session
            export PATH="$PATH:$HOME/.dotnet"
            export DOTNET_ROOT="$HOME/.dotnet"
            
            # Add to shell profile
            SHELL_PROFILE="$HOME/.zshrc"
            if [ ! -f "$HOME/.zshrc" ] && [ -f "$HOME/.bashrc" ]; then
                SHELL_PROFILE="$HOME/.bashrc"
            fi
            
            if ! grep -q "DOTNET_ROOT" "$SHELL_PROFILE" 2>/dev/null; then
                echo "" >> "$SHELL_PROFILE"
                echo "# .NET Configuration" >> "$SHELL_PROFILE"
                echo 'export DOTNET_ROOT=$HOME/.dotnet' >> "$SHELL_PROFILE"
                echo 'export PATH=$PATH:$DOTNET_ROOT' >> "$SHELL_PROFILE"
            fi
            
            echo "✅ .NET 9.0 SDK installed successfully!"
            echo "⚠️  Note: You may need to restart your terminal or run: source $SHELL_PROFILE"
            echo ""
            return 0
        else
            rm -f "$INSTALL_SCRIPT"
            return 1
        fi
    else
        return 1
    fi
}

# Check if .NET is installed
if ! command -v dotnet &> /dev/null; then
    echo "❌ .NET SDK is not installed."
    echo ""
    
    # Detect OS
    OS_TYPE=$(uname -s)
    
    if [ "$OS_TYPE" = "Darwin" ]; then
        echo "🍎 Detected macOS"
        read -p "Would you like to install .NET 9.0 SDK automatically? (y/n) " -n 1 -r
        echo ""
        if [[ $REPLY =~ ^[Yy]$ ]]; then
            if ! install_dotnet_macos; then
                echo ""
                echo "⚠️  PKG installation failed. Trying alternative method..."
                echo ""
                if ! install_dotnet_with_script; then
                    echo ""
                    echo "❌ All installation methods failed."
                    echo "Please install .NET 9.0 SDK manually from:"
                    echo "https://dotnet.microsoft.com/download/dotnet/9.0"
                    echo ""
                    echo "Or try using Homebrew:"
                    echo "  brew install --cask dotnet-sdk"
                    exit 1
                fi
            fi
        else
            echo "Please install .NET 9.0 SDK manually from:"
            echo "https://dotnet.microsoft.com/download/dotnet/9.0"
            echo ""
            echo "Or use Homebrew:"
            echo "  brew install --cask dotnet-sdk"
            exit 1
        fi
    elif [ "$OS_TYPE" = "Linux" ]; then
        echo "🐧 Detected Linux"
        read -p "Would you like to install .NET 9.0 SDK automatically? (y/n) " -n 1 -r
        echo ""
        if [[ $REPLY =~ ^[Yy]$ ]]; then
            if ! install_dotnet_linux; then
                echo ""
                echo "❌ Installation failed."
                echo "Please install .NET 9.0 SDK manually from:"
                echo "https://dotnet.microsoft.com/download/dotnet/9.0"
                exit 1
            fi
        else
            echo "Please install .NET 9.0 SDK manually from:"
            echo "https://dotnet.microsoft.com/download/dotnet/9.0"
            exit 1
        fi
    else
        echo "Unsupported OS: $OS_TYPE"
        echo "Please install .NET 9.0 SDK manually from:"
        echo "https://dotnet.microsoft.com/download/dotnet/9.0"
        exit 1
    fi
fi

# Verify .NET installation
if ! command -v dotnet &> /dev/null; then
    echo "❌ .NET installation failed or not found in PATH"
    exit 1
fi

echo "✅ .NET SDK found: $(dotnet --version)"
echo ""

# Check if the correct version is installed
DOTNET_VERSION=$(dotnet --version | cut -d '.' -f 1)
if [ "$DOTNET_VERSION" -lt 9 ]; then
    echo "⚠️  Warning: .NET 9.0 or higher is required."
    echo "Current version: $(dotnet --version)"
    echo ""
    read -p "Would you like to install .NET 9.0 SDK? (y/n) " -n 1 -r
    echo ""
    if [[ $REPLY =~ ^[Yy]$ ]]; then
        OS_TYPE=$(uname -s)
        if [ "$OS_TYPE" = "Darwin" ]; then
            if ! install_dotnet_macos; then
                echo ""
                echo "⚠️  Trying alternative installation method..."
                if ! install_dotnet_with_script; then
                    echo "Please upgrade to .NET 9.0 or higher manually"
                    exit 1
                fi
            fi
        elif [ "$OS_TYPE" = "Linux" ]; then
            if ! install_dotnet_linux; then
                echo "Please upgrade to .NET 9.0 or higher manually"
                exit 1
            fi
        fi
    else
        echo "Please upgrade to .NET 9.0 or higher"
        exit 1
    fi
fi

# Restore dependencies
echo "📦 Restoring NuGet packages..."
dotnet restore

if [ $? -eq 0 ]; then
    echo "✅ Dependencies restored successfully"
else
    echo "❌ Failed to restore dependencies"
    exit 1
fi
echo ""

# Build the project
echo "🔨 Building the project..."
dotnet build

if [ $? -eq 0 ]; then
    echo "✅ Project built successfully"
else
    echo "❌ Build failed"
    exit 1
fi
echo ""

# Check for configuration file
if [ ! -f "appsettings.Development.local.json" ]; then
    echo "⚠️  appsettings.Development.local.json not found"
    echo "📝 You can use appsettings.Development.json or create a local configuration file"
    echo ""
fi

# Check for .env file (for Docker)
if [ ! -f ".env" ]; then
    echo "ℹ️  .env file not found (optional for Docker)"
    echo "You can copy .env.example to .env if you plan to use Docker"
    echo ""
fi

echo "✅ Setup complete!"
echo ""

# Check if dotnet is in PATH
if ! command -v dotnet &> /dev/null; then
    echo "⚠️  IMPORTANT: .NET was installed but not yet in your PATH for this session"
    echo ""
    echo "To use .NET in this terminal, run one of these commands:"
    echo ""
    
    # Detect the installation location
    if [ -d "$HOME/.dotnet" ]; then
        echo "  export PATH=\"\$PATH:\$HOME/.dotnet\""
        echo "  export DOTNET_ROOT=\"\$HOME/.dotnet\""
    elif [ -d "/usr/local/share/dotnet" ]; then
        echo "  export PATH=\"\$PATH:/usr/local/share/dotnet\""
    fi
    
    echo ""
    echo "Or simply restart your terminal (recommended)"
    echo ""
    SHELL_NAME=$(basename "$SHELL")
    if [ "$SHELL_NAME" = "zsh" ]; then
        echo "Or run: source ~/.zshrc"
    else
        echo "Or run: source ~/.bashrc"
    fi
    echo ""
fi

echo "Next steps:"
echo "1. Review your configuration in appsettings.Development.json"
echo "2. Run the application with: dotnet run"
echo "3. Access Swagger UI at: http://localhost:5130/swagger"
echo ""
echo "For more information, see:"
echo "- README.md - Full documentation"
echo "- GETTING_STARTED.md - Quick start guide"
echo "- API_DOCUMENTATION.md - API reference"
echo ""
echo "Happy coding! 🚀"

