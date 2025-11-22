# Contributing to Noodle API

Thank you for your interest in contributing to the Noodle API! This guide will help you get started.

## Table of Contents

- [Getting Started](#getting-started)
- [Development Workflow](#development-workflow)
- [Code Style Guidelines](#code-style-guidelines)
- [Project Structure](#project-structure)
- [Adding New Features](#adding-new-features)
- [Testing](#testing)
- [Submitting Changes](#submitting-changes)

## Getting Started

### Prerequisites

1. Install .NET 9.0 SDK
2. Clone the repository
3. Read the [GETTING_STARTED.md](GETTING_STARTED.md) guide

### Setting Up Your Development Environment

```bash
# Clone the repository
git clone https://git.mvp.studio/production-modules/noodle-api.git
cd noodle-api

# Restore dependencies
dotnet restore

# Build the project
dotnet build

# Run the application
dotnet run
```

## Development Workflow

### 1. Create a Feature Branch

```bash
git checkout -b feature/your-feature-name
```

Branch naming conventions:
- `feature/` - New features
- `bugfix/` - Bug fixes
- `hotfix/` - Critical fixes for production
- `refactor/` - Code refactoring
- `docs/` - Documentation updates

### 2. Make Your Changes

Follow the [Code Style Guidelines](#code-style-guidelines) when making changes.

### 3. Test Your Changes

```bash
# Run the application
dotnet run

# Test using Swagger UI
# Navigate to http://localhost:5130/swagger
```

### 4. Commit Your Changes

```bash
git add .
git commit -m "feat: add new endpoint for X"
```

### Commit Message Format

Follow the [Conventional Commits](https://www.conventionalcommits.org/) specification:

```
<type>(<scope>): <subject>

<body>

<footer>
```

**Types:**
- `feat`: New feature
- `fix`: Bug fix
- `docs`: Documentation changes
- `style`: Code style changes (formatting, etc.)
- `refactor`: Code refactoring
- `perf`: Performance improvements
- `test`: Adding or updating tests
- `chore`: Maintenance tasks

**Examples:**
```
feat(stablecoins): add pagination to stablecoins endpoint

fix(database): resolve connection timeout issue

docs(readme): update installation instructions

refactor(services): simplify data retrieval logic
```

## Code Style Guidelines

### C# Conventions

#### Naming

- **Classes/Interfaces**: PascalCase
  ```csharp
  public class StablecoinsService { }
  public interface IStablecoinsRepository { }
  ```

- **Methods**: PascalCase
  ```csharp
  public async Task<List<Stablecoin>> GetStablecoinsAsync() { }
  ```

- **Private Fields**: camelCase with underscore prefix
  ```csharp
  private readonly IStablecoinsRepository _repository;
  ```

- **Parameters/Local Variables**: camelCase
  ```csharp
  public void ProcessData(string itemName, int itemCount) { }
  ```

#### Async/Await

Always use async/await for I/O operations:

```csharp
// Good
public async Task<List<Stablecoin>> GetStablecoinsAsync()
{
    return await _repository.GetAllAsync();
}

// Bad
public List<Stablecoin> GetStablecoins()
{
    return _repository.GetAll();
}
```

#### Dependency Injection

Use constructor injection:

```csharp
public class StablecoinsService : IStablecoinsService
{
    private readonly IStablecoinsRepository _repository;
    
    public StablecoinsService(IStablecoinsRepository repository)
    {
        _repository = repository;
    }
}
```

#### API Controllers

Follow RESTful conventions:

```csharp
[ApiController]
[Route("noodle")]
public class StablecoinsController : ControllerBase
{
    private readonly IStablecoinsService _service;

    public StablecoinsController(IStablecoinsService service)
    {
        _service = service;
    }

    [HttpGet("stablecoins")]
    public async Task<ActionResult<List<Stablecoin>>> GetStablecoins()
    {
        var data = await _service.GetStablecoinsAsync();
        return Ok(data);
    }
}
```

## Project Structure

```
noodle-api/
├── Controllers/          # API Controllers - HTTP endpoint handlers
├── Services/            # Business logic layer
├── Repositories/        # Data access layer (MongoDB)
├── Models/              # Data models and DTOs
├── Helpers/             # Utility functions and extensions
├── Constants/           # Application constants
├── Data/               # Database configuration
└── Program.cs          # Application entry point
```

### Layer Responsibilities

**Controllers:**
- Handle HTTP requests/responses
- Validate input parameters
- Call service methods
- Return appropriate HTTP status codes

**Services:**
- Implement business logic
- Coordinate between repositories
- Transform data as needed
- No direct HTTP concerns

**Repositories:**
- Handle database operations
- Query MongoDB collections
- No business logic

**Models:**
- Define data structures
- Include validation attributes
- Keep them simple and focused

## Adding New Features

### Example: Adding a New Endpoint

#### 1. Create the Model

```csharp
// Models/NewFeatureItem.cs
namespace Noodle.Api.Models
{
    public class NewFeatureItem
    {
        public string Id { get; set; }
        public string Name { get; set; }
        // ... other properties
    }
}
```

#### 2. Create the Repository Interface and Implementation

```csharp
// Repositories/INewFeatureRepository.cs
public interface INewFeatureRepository
{
    Task<List<NewFeatureItem>> GetAllAsync();
}

// Repositories/NewFeatureRepository.cs
public class NewFeatureRepository : INewFeatureRepository
{
    private readonly IMongoCollection<NewFeatureItem> _collection;

    public NewFeatureRepository(IMongoClient mongoClient, IOptions<DatabaseSettings> settings)
    {
        var database = mongoClient.GetDatabase(settings.Value.DatabaseName);
        _collection = database.GetCollection<NewFeatureItem>("new_feature_items");
    }

    public async Task<List<NewFeatureItem>> GetAllAsync()
    {
        return await _collection.Find(_ => true).ToListAsync();
    }
}
```

#### 3. Create the Service Interface and Implementation

```csharp
// Services/INewFeatureService.cs
public interface INewFeatureService
{
    Task<List<NewFeatureItem>> GetAllItemsAsync();
}

// Services/NewFeatureService.cs
public class NewFeatureService : INewFeatureService
{
    private readonly INewFeatureRepository _repository;

    public NewFeatureService(INewFeatureRepository repository)
    {
        _repository = repository;
    }

    public async Task<List<NewFeatureItem>> GetAllItemsAsync()
    {
        return await _repository.GetAllAsync();
    }
}
```

#### 4. Create the Controller

```csharp
// Controllers/NewFeatureController.cs
[ApiController]
[Route("noodle")]
public class NewFeatureController : ControllerBase
{
    private readonly INewFeatureService _service;

    public NewFeatureController(INewFeatureService service)
    {
        _service = service;
    }

    [HttpGet("new-feature-items")]
    public async Task<ActionResult<List<NewFeatureItem>>> GetItems()
    {
        var items = await _service.GetAllItemsAsync();
        return Ok(items);
    }
}
```

#### 5. Register Services in Program.cs

```csharp
// Program.cs
builder.Services.AddScoped<INewFeatureRepository, NewFeatureRepository>();
builder.Services.AddScoped<INewFeatureService, NewFeatureService>();
```

#### 6. Update Documentation

- Add endpoint details to `API_DOCUMENTATION.md`
- Update `README.md` if needed

## Testing

### Manual Testing

1. Run the application in development mode:
   ```bash
   dotnet run --environment Development
   ```

2. Test using Swagger UI:
   ```
   http://localhost:5130/swagger
   ```

3. Test using cURL or Postman:
   ```bash
   curl http://localhost:5130/noodle/your-endpoint
   ```

### Automated Testing

_(To be implemented)_

When adding tests in the future:

```bash
# Run tests
dotnet test

# Run tests with coverage
dotnet test /p:CollectCoverage=true
```

## Submitting Changes

### Before Submitting

- [ ] Code follows the style guidelines
- [ ] All endpoints are tested manually
- [ ] Documentation is updated
- [ ] Commit messages follow the convention
- [ ] No sensitive data in commits

### Creating a Merge Request

1. Push your branch to GitLab:
   ```bash
   git push origin feature/your-feature-name
   ```

2. Go to the GitLab repository

3. Click "Create Merge Request"

4. Fill in the template:
   ```markdown
   ## Description
   Brief description of changes

   ## Type of Change
   - [ ] Bug fix
   - [ ] New feature
   - [ ] Breaking change
   - [ ] Documentation update

   ## Testing
   How has this been tested?

   ## Checklist
   - [ ] My code follows the style guidelines
   - [ ] I have tested my changes
   - [ ] I have updated the documentation
   - [ ] My changes generate no new warnings
   ```

5. Request review from team members

6. Address review comments

7. Once approved, the merge request will be merged

## Code Review Guidelines

### For Reviewers

- Be respectful and constructive
- Check for code style adherence
- Verify functionality makes sense
- Look for potential bugs or edge cases
- Suggest improvements when appropriate

### For Authors

- Be open to feedback
- Respond to all comments
- Make requested changes promptly
- Ask questions if unclear
- Thank reviewers for their time

## Environment Variables

Never commit sensitive data like:
- Database connection strings with credentials
- API keys
- Passwords
- Private tokens

Use `appsettings.Development.local.json` (gitignored) for local secrets.

## Database Changes

When making database schema changes:

1. Document the change in your MR
2. Consider backwards compatibility
3. Plan for data migration if needed
4. Test with production-like data volumes

## Common Issues

### Port Already in Use

```bash
# macOS/Linux
lsof -ti:5130 | xargs kill -9

# Windows
netstat -ano | findstr :5130
taskkill /PID <PID> /F
```

### MongoDB Connection Issues

1. Verify connection string format
2. Check network connectivity
3. Verify credentials
4. Check firewall rules

## Getting Help

- Check existing documentation
- Search existing issues/MRs
- Ask in the team chat
- Create a GitLab issue for bugs
- Reach out to maintainers

## License

By contributing, you agree that your contributions will be licensed under the same license as the project.

## Thank You!

Thank you for contributing to Noodle API! Your efforts help make this project better for everyone.

