# Changelog

All notable changes to the Noodle API will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

### Added
- **Automated setup scripts with .NET installation**
  - Enhanced setup.sh for macOS/Linux with automatic .NET 9.0 installation
  - Enhanced setup.ps1 for Windows with automatic .NET 9.0 installation
  - Auto-detection of OS and architecture (Intel, ARM64, Apple Silicon)
  - Interactive installation prompts
  - PATH configuration
  - Complete dependency restoration and build
- Comprehensive documentation suite
  - Main README with getting started guide
  - Quick start guide (GETTING_STARTED.md)
  - Complete API documentation (API_DOCUMENTATION.md)
  - Contributing guidelines (CONTRIBUTING.md)
  - Installation guide (INSTALLATION.md)
  - Documentation index (DOCUMENTATION_INDEX.md)
  - Start here guide (START_HERE.md)
- .gitignore file for .NET projects
- Configuration template (appsettings.Development.local.json.template)
- Docker Compose configuration

### Changed
- Prerequisites updated - .NET installation now automated
- Documentation updated to reflect automated setup

## [1.0.0] - 2024

### Added
- Initial release of Noodle API
- Stablecoins endpoints
  - Top growth stablecoins
  - Most talked about stablecoins
  - Number of tracked stablecoins
  - Active users tracking
  - Stablecoins list with search and pagination
- Stocks endpoints
  - Top growth stocks
  - Most talked about stocks
  - Number of tracked stocks
  - Active users tracking
  - Stocks list with search and pagination
- Commodities endpoints
  - Top growth commodities
  - Most talked about commodities
  - Number of tracked commodities
  - Active users tracking
  - Commodities list with filtering
- Price History endpoints
  - Historical price data retrieval
- MongoDB integration
- CORS support for frontend integration
- Swagger/OpenAPI documentation
- Docker support
- Background jobs for data refresh

### Technical Details
- .NET 9.0 framework
- MongoDB.Driver 3.5.0
- Repository pattern for data access
- Dependency injection
- Async/await throughout

## Version History

- **1.0.0** - Initial release with core functionality
- **Unreleased** - Documentation improvements

---

## Types of Changes

- **Added** - New features
- **Changed** - Changes in existing functionality
- **Deprecated** - Soon-to-be removed features
- **Removed** - Removed features
- **Fixed** - Bug fixes
- **Security** - Security improvements

## Links

- [GitLab Repository](https://git.mvp.studio/production-modules/noodle-api)
- [API Documentation](API_DOCUMENTATION.md)
- [Contributing Guidelines](CONTRIBUTING.md)

