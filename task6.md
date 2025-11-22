# Developer Onboarding Task: Noodle API

**Objective**: Get familiar with the Noodle API by setting it up, testing endpoints, and making your first code modification.

## 📋 Task Overview

This task will guide you through:

1. ✅ Setting up the development environment
2. ✅ Running the API and testing with Swagger
3. ✅ Understanding the codebase structure
4. ✅ Making a simple modification
5. ✅ Testing your changes

---

## Part 1: Setup & Running

### Step 1.1: Clone and Setup

```bash
# Clone the repository
git clone https://git.mvp.studio/production-modules/noodle-api.git
cd noodle-api

# Run automated setup
./setup.sh              # macOS/Linux
.\setup.ps1             # Windows (as Administrator)
```

**⚠️ Important**: If you see "command not found: dotnet" after setup:

```bash
# macOS/Linux
export PATH="$PATH:$HOME/.dotnet"
export DOTNET_ROOT="$HOME/.dotnet"

# Then verify
dotnet --version  # Should show 9.0.101+
```

### Step 1.2: Start the API

```bash
dotnet run
```

**✅ Success Indicators:**

- You should see: `Now listening on: http://localhost:5130`
- No error messages in the console

**📸 Screenshot Checkpoint #1**: Take a screenshot of your terminal showing the API running successfully.

### Step 1.3: Access Swagger UI

Open your browser and navigate to:

```
http://localhost:5130/swagger
```

**✅ What You Should See:**

- Interactive API documentation
- List of all available endpoints organized by controller
- Green "Schemas" section at the bottom

**📸 Screenshot Checkpoint #2**: Take a screenshot of Swagger UI showing all the endpoints.

---

## Part 2: Testing the API

### Step 2.1: Test Your First Endpoint

In Swagger UI, find and test the following endpoint:

1. **Locate**: `GET /noodle/top-growth-stablecoins`
2. **Click**: "Try it out" button
3. **Click**: "Execute" button

**✅ Expected Result:**

- Status Code: `200`
- Response body with stablecoin data

**📝 Task**: Document the response:

- How many stablecoins were returned?
- What fields does each stablecoin object contain?
- What is the structure of the response?

### Step 2.2: Test Pagination Endpoint

Test the stablecoins list with pagination:

1. **Locate**: `GET /noodle/stablecoins`
2. **Click**: "Try it out"
3. **Set Parameters**:
   - `page`: 1
   - `limit`: 5
   - `q`: (leave empty)
4. **Click**: "Execute"

**📝 Task**: Answer these questions:

- How is pagination implemented in the response?
- What happens if you change the `limit` parameter?
- Try adding a search query in the `q` parameter - does it filter results?

### Step 2.3: Test Another Resource

Pick one endpoint from either Stocks or Commodities and test it:

- `GET /noodle/top-growth-stocks`
- `GET /noodle/top-growth-commodities`

**📸 Screenshot Checkpoint #3**: Screenshot showing a successful API response with data.

---

## Part 3: Understanding the Code

### Step 3.1: Explore the Project Structure

Open the project in your IDE and examine:

```
Controllers/          # API endpoint handlers
├── StablecoinsController.cs
├── StocksController.cs
└── CommoditiesController.cs

Services/            # Business logic
├── StablecoinsService.cs
├── StocksService.cs
└── CommoditiesService.cs

Repositories/        # Data access
├── StablecoinsRepository.cs
├── StocksRepository.cs
└── CommoditiesRepository.cs

Models/             # Data models
├── StablecoinItem.cs
├── StockItem.cs
└── CommodityItem.cs
```

### Step 3.2: Trace a Request

Let's trace how `GET /noodle/top-growth-stablecoins` works:

1. **Open**: `Controllers/StablecoinsController.cs`

   - Find the `GetTopGrowthStablecoins` method
   - Notice it calls `_service.GetTopGrowthStablecoinsAsync()`

2. **Open**: `Services/StablecoinsService.cs`

   - Find the `GetTopGrowthStablecoinsAsync` method
   - Notice it calls the repository

3. **Open**: `Repositories/StablecoinsRepository.cs`
   - Find the corresponding method
   - Notice how it queries MongoDB

**📝 Task**: Write a brief explanation (3-4 sentences) of how data flows from the database to the API response.

---

## Part 4: Make Your First Modification

### Task: Add a New Endpoint to Get Stablecoin Count

You will add a simple endpoint that returns the total count of stablecoins in the database.

### Step 4.1: Add Repository Method

**File**: `Repositories/StablecoinsRepository.cs`

Find the interface `IStablecoinsRepository` and add:

```csharp
Task<long> GetTotalCountAsync();
```

Then in the `StablecoinsRepository` class, implement it:

```csharp
public async Task<long> GetTotalCountAsync()
{
    return await _stablecoins.CountDocumentsAsync(_ => true);
}
```

### Step 4.2: Add Service Method

**File**: `Services/StablecoinsService.cs`

Find the interface `IStablecoinsService` and add:

```csharp
Task<long> GetTotalCountAsync();
```

Then in the `StablecoinsService` class, implement it:

```csharp
public async Task<long> GetTotalCountAsync()
{
    return await _repository.GetTotalCountAsync();
}
```

### Step 4.3: Add Controller Endpoint

**File**: `Controllers/StablecoinsController.cs`

Add this new endpoint method:

```csharp
[HttpGet("stablecoins/count")]
public async Task<ActionResult<long>> GetTotalCount()
{
    var count = await _service.GetTotalCountAsync();
    return Ok(new { totalCount = count });
}
```

### Step 4.4: Test Your Changes

1. **Stop** the running API (Ctrl+C)
2. **Rebuild**:
   ```bash
   dotnet build
   ```
3. **Run**:
   ```bash
   dotnet run
   ```
4. **Open Swagger**: http://localhost:5130/swagger
5. **Find your new endpoint**: `GET /noodle/stablecoins/count`
6. **Test it**: Click "Try it out" → "Execute"

**✅ Expected Result:**

```json
{
  "totalCount": 123
}
```

**📸 Screenshot Checkpoint #4**: Screenshot showing your new endpoint in Swagger and its successful response.

---

## Part 5: Bonus Challenge (Optional)

### Challenge: Add the Same Endpoint for Stocks

Apply what you learned to add a count endpoint for stocks:

- Endpoint should be: `GET /noodle/stocks/count`
- Follow the same pattern as you did for stablecoins
- Test it in Swagger

**Files to modify:**

1. `Repositories/StocksRepository.cs`
2. `Services/StocksService.cs`
3. `Controllers/StocksController.cs`

---

## 📝 Deliverables

Please submit:

### 1. Screenshots (4 required)

- ✅ Checkpoint #1: API running in terminal
- ✅ Checkpoint #2: Swagger UI homepage
- ✅ Checkpoint #3: Successful API response
- ✅ Checkpoint #4: Your new endpoint working

### 2. Written Responses

- Answer to Step 2.1: Response structure analysis
- Answer to Step 2.2: Pagination questions
- Answer to Step 3.2: Data flow explanation

---

## 🆘 Troubleshooting Guide

### Issue: Port 5130 already in use

```bash
# macOS/Linux
lsof -ti:5130 | xargs kill -9

# Windows
netstat -ano | findstr :5130
taskkill /PID <PID> /F
```

### Issue: MongoDB connection error

- The API uses a remote MongoDB instance
- Default connection is configured in `appsettings.Development.json`
- You should be able to use the existing connection

### Issue: Build errors after changes

```bash
dotnet clean
dotnet build
```

### Issue: Can't find your new endpoint in Swagger

- Make sure you stopped and restarted the API
- Check that your method has the `[HttpGet]` attribute
- Verify the route is correct

---

## 📚 Additional Resources

- [ASP.NET Core Documentation](https://docs.microsoft.com/aspnet/core)
- [MongoDB C# Driver](https://mongodb.github.io/mongo-csharp-driver)
- [Swagger/OpenAPI](https://swagger.io/docs)

---

**Questions?** Ask in the team channel or reach out to your mentor.

**Good luck! 🚀**
