# Noodle API

A .NET 9.0 ASP.NET Core Web API providing financial data endpoints for stablecoins, stocks, commodities, and price history with MongoDB storage.

## Quick Start

```bash
# Clone the repository
git clone https://git.mvp.studio/production-modules/noodle-api.git
cd noodle-api

# Run automated setup (installs .NET 9.0 if needed)
./setup.sh              # macOS/Linux
.\setup.ps1             # Windows (as Administrator)

# If .NET is not in PATH after setup, run:
export PATH="$PATH:$HOME/.dotnet"        # macOS/Linux
export DOTNET_ROOT="$HOME/.dotnet"

# Or restart your terminal

# Start the API
dotnet run

# Access Swagger UI
# http://localhost:5130/swagger
```

## Table of Contents

- [Features](#features)
- [Installation](#installation)
- [Configuration](#configuration)
- [Running the Application](#running-the-application)
- [API Endpoints](#api-endpoints)
- [Docker Deployment](#docker-deployment)
- [Troubleshooting](#troubleshooting)

## Features

- **Financial Data APIs**: Stablecoins, stocks, commodities analytics
- **MongoDB Integration**: Robust data storage and queries
- **Swagger Documentation**: Interactive API testing and documentation
- **CORS Support**: Pre-configured for frontend integration
- **Background Jobs**: Automated data refresh

## Installation

### Automated Setup (Recommended)

The setup script automatically installs .NET 9.0 SDK if needed:

**macOS/Linux:**

```bash
./setup.sh
```

**Windows PowerShell (run as Administrator):**

```powershell
.\setup.ps1
```

**What it does:**

- Detects your OS and architecture
- Downloads and installs .NET 9.0 SDK if not present
- Restores NuGet packages
- Builds the project

### Important: PATH Setup

After installation, if `dotnet` command is not found:

**macOS/Linux:**

```bash
# Add to current session
export PATH="$PATH:$HOME/.dotnet"
export DOTNET_ROOT="$HOME/.dotnet"

# Or add to your shell profile (~/.zshrc or ~/.bashrc)
echo 'export DOTNET_ROOT=$HOME/.dotnet' >> ~/.zshrc
echo 'export PATH=$PATH:$DOTNET_ROOT' >> ~/.zshrc
source ~/.zshrc
```

**Windows:**

- Restart your terminal or
- Restart your computer

**Verify installation:**

```bash
dotnet --version  # Should show 9.0.101 or higher
```

### Manual Installation

If the automated setup fails:

1. **Install .NET 9.0 SDK manually:**

   - Download from: https://dotnet.microsoft.com/download/dotnet/9.0
   - Or use Homebrew (macOS): `brew install --cask dotnet-sdk`

2. **Build the project:**
   ```bash
   dotnet restore
   dotnet build
   ```

## Configuration

### Database Connection

Edit `appsettings.Development.json`:

```json
{
  "DatabaseSettings": {
    "ConnectionString": "mongodb://username:password@host:port/?authSource=database",
    "DatabaseName": "BitCountry-Warehouse"
  },
  "Cors": {
    "AllowedOrigins": ["http://localhost:3000"]
  }
}
```

### Environment Variables (Alternative)

```bash
export DatabaseSettings__ConnectionString="mongodb://..."
export DatabaseSettings__DatabaseName="YourDatabaseName"
```

## Running the Application

```bash
# Development mode (with Swagger UI)
dotnet run

# With hot reload
dotnet watch run

# Production mode
dotnet run --environment Production
```

**Access the API:**

- Swagger UI: http://localhost:5130/swagger
- API Base: http://localhost:5130/noodle

## API Endpoints

All endpoints are prefixed with `/noodle`. View complete documentation at: http://localhost:5130/swagger

### Key Endpoints

**Stablecoins:**

- `GET /noodle/stablecoins` - List with search & pagination
- `GET /noodle/top-growth-stablecoins` - Top performers
- `GET /noodle/most-talked-about-stablecoins` - Most discussed

**Stocks:**

- `GET /noodle/stocks` - List with search & pagination
- `GET /noodle/top-growth-stocks` - Top performers
- `GET /noodle/most-talked-about-stocks` - Most discussed

**Commodities:**

- `GET /noodle/commodities` - List with filters
- `GET /noodle/top-growth-commodities` - Top performers
- `GET /noodle/most-talked-about-commodities` - Most discussed

**Example:**

```bash
curl http://localhost:5130/noodle/top-growth-stablecoins
```

## Docker Deployment

```bash
# Build image
docker build -t noodle-api .

# Run container
docker run -d -p 5130:5130 \
  -e DatabaseSettings__ConnectionString="mongodb://..." \
  -e DatabaseSettings__DatabaseName="YourDatabase" \
  noodle-api

# Or use Docker Compose
docker-compose up -d
```

## Troubleshooting

### .NET Command Not Found

If `dotnet` command is not found after setup:

```bash
# macOS/Linux - Add to PATH temporarily
export PATH="$PATH:$HOME/.dotnet"
export DOTNET_ROOT="$HOME/.dotnet"

# Or permanently (add to ~/.zshrc or ~/.bashrc)
echo 'export DOTNET_ROOT=$HOME/.dotnet' >> ~/.zshrc
echo 'export PATH=$PATH:$DOTNET_ROOT' >> ~/.zshrc
source ~/.zshrc

# Windows - Restart your terminal or computer
```

Verify with: `dotnet --version` (should show 9.0.101+)

### MongoDB Connection Failed

- Check connection string in `appsettings.Development.json`
- Verify MongoDB is accessible: `telnet your-mongo-host 27017`
- Check credentials and authSource parameter

### Port 5130 Already in Use

```bash
# macOS/Linux
lsof -ti:5130 | xargs kill -9

# Windows
netstat -ano | findstr :5130
taskkill /PID <PID> /F

# Or change port in Properties/launchSettings.json
```

### Build Errors

```bash
dotnet clean
dotnet restore --force
dotnet build
```

## Project Structure

```
Controllers/    # API endpoints
Services/       # Business logic
Repositories/   # Data access
Models/         # Data models
Program.cs      # Application entry point
```

## Contributing

1. Fork the repository
2. Create feature branch: `git checkout -b feature/name`
3. Commit changes: `git commit -m 'Add feature'`
4. Push: `git push origin feature/name`
5. Open a Merge Request

## License

[Specify your license here]
