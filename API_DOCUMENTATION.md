# Noodle API - Endpoint Documentation

Complete reference for all available API endpoints.

## Base URL

All endpoints are prefixed with `/noodle`

**Development:** `http://localhost:5130/noodle`  
**Production:** `https://your-domain.com/noodle`

## Table of Contents

- [Stablecoins](#stablecoins)
- [Stocks](#stocks)
- [Commodities](#commodities)
- [Price History](#price-history)
- [Response Formats](#response-formats)
- [Error Handling](#error-handling)

---

## Stablecoins

### Get Top Growth Stablecoins

Returns the stablecoins with the highest growth rate.

**Endpoint:** `GET /noodle/top-growth-stablecoins`

**Response:**
```json
{
  "data": [
    {
      "symbol": "USDT",
      "growth": 12.5,
      "...": "..."
    }
  ]
}
```

---

### Get Most Talked About Stablecoins

Returns the stablecoins with the most mentions/discussions.

**Endpoint:** `GET /noodle/most-talked-about-stablecoins`

**Response:**
```json
[
  {
    "symbol": "USDC",
    "mentions": 1234,
    "...": "..."
  }
]
```

---

### Get Number of Tracked Stablecoins

Returns the total count of stablecoins being tracked.

**Endpoint:** `GET /noodle/stablecoins-number-tracked`

**Response:**
```json
{
  "count": 50,
  "...": "..."
}
```

---

### Get Active Users for Stablecoins

Returns the number of active users tracking stablecoins.

**Endpoint:** `GET /noodle/active-users-stablecoins`

**Response:**
```json
{
  "activeUsers": 1234,
  "...": "..."
}
```

---

### Get Stablecoins List

Returns a paginated list of stablecoins with search capability.

**Endpoint:** `GET /noodle/stablecoins`

**Query Parameters:**
- `q` (string, optional): Search query
- `page` (integer, optional, default: 1): Page number
- `limit` (integer, optional, default: 20): Items per page

**Example:**
```
GET /noodle/stablecoins?q=USD&page=1&limit=20
```

**Response:**
```json
{
  "data": [
    {
      "id": "...",
      "symbol": "USDT",
      "name": "Tether",
      "...": "..."
    }
  ],
  "pagination": {
    "page": 1,
    "limit": 20,
    "total": 50
  }
}
```

---

## Stocks

### Get Top Growth Stocks

Returns the stocks with the highest growth rate.

**Endpoint:** `GET /noodle/top-growth-stocks`

**Response:**
```json
{
  "data": [
    {
      "symbol": "AAPL",
      "growth": 8.5,
      "...": "..."
    }
  ]
}
```

---

### Get Most Talked About Stocks

Returns the stocks with the most mentions/discussions.

**Endpoint:** `GET /noodle/most-talked-about-stocks`

**Response:**
```json
{
  "data": [
    {
      "symbol": "TSLA",
      "mentions": 5678,
      "...": "..."
    }
  ]
}
```

---

### Get Number of Tracked Stocks

Returns the total count of stocks being tracked.

**Endpoint:** `GET /noodle/stock-number-tracked`

**Response:**
```json
{
  "count": 150,
  "...": "..."
}
```

---

### Get Active Users for Stocks

Returns the number of active users tracking stocks.

**Endpoint:** `GET /noodle/active-users-stock`

**Response:**
```json
{
  "activeUsers7d": 2345,
  "...": "..."
}
```

---

### Get Stocks List

Returns a paginated list of stocks with search and filter capabilities.

**Endpoint:** `GET /noodle/stocks`

**Query Parameters:**
- `limit` (integer, optional, default: 25): Items per page
- `page` (integer, optional, default: 1): Page number
- `search` (string, optional): Search query
- `groupFilter` (string, optional): Filter by group/category

**Example:**
```
GET /noodle/stocks?limit=25&page=1&search=Apple&groupFilter=tech
```

**Response:**
```json
{
  "data": [
    {
      "symbol": "AAPL",
      "name": "Apple Inc.",
      "healthRank": 95,
      "...": "..."
    }
  ],
  "pagination": {
    "page": 1,
    "limit": 25,
    "total": 150
  }
}
```

---

## Commodities

### Get Top Growth Commodities

Returns the commodities with the highest growth rate.

**Endpoint:** `GET /noodle/top-growth-commodities`

**Response:**
```json
{
  "data": [
    {
      "name": "Gold",
      "growth": 5.2,
      "...": "..."
    }
  ]
}
```

---

### Get Most Talked About Commodities

Returns the commodities with the most mentions/discussions.

**Endpoint:** `GET /noodle/most-talked-about-commodities`

**Response:**
```json
{
  "data": [
    {
      "name": "Oil",
      "mentions": 3456,
      "...": "..."
    }
  ]
}
```

---

### Get Number of Tracked Commodities

Returns the total count of commodities being tracked.

**Endpoint:** `GET /noodle/commodities-number-tracked`

**Response:**
```json
{
  "count": 30,
  "...": "..."
}
```

---

### Get Active Users for Commodities

Returns the number of active users tracking commodities.

**Endpoint:** `GET /noodle/active-users-commodities`

**Response:**
```json
{
  "activeUsers7d": 890,
  "...": "..."
}
```

---

### Get Commodities List

Returns a paginated list of commodities with filter capabilities.

**Endpoint:** `GET /noodle/commodities`

**Query Parameters:**
- `limit` (integer, optional, default: 10): Items per page
- `page` (integer, optional, default: 1): Page number
- `groupFilter` (string, optional): Filter by group/category

**Example:**
```
GET /noodle/commodities?limit=10&page=1&groupFilter=metals
```

**Response:**
```json
{
  "data": [
    {
      "name": "Gold",
      "healthRank": 88,
      "group": "metals",
      "...": "..."
    }
  ],
  "pagination": {
    "page": 1,
    "limit": 10,
    "total": 30
  }
}
```

---

## Price History

### Get Price History

Returns historical price data for a specific asset.

**Endpoint:** `GET /noodle/price-history`

**Query Parameters:**
The endpoint accepts a `PriceHistoryRequest` object with the following properties (check the model for exact fields):
- Asset identifier parameters
- Time range parameters
- Granularity settings

**Example:**
```
GET /noodle/price-history?symbol=AAPL&from=2024-01-01&to=2024-12-31
```

**Response:**
```json
{
  "data": [
    {
      "timestamp": "2024-01-01T00:00:00Z",
      "price": 185.50,
      "...": "..."
    }
  ]
}
```

---

## Response Formats

### Success Response

Most endpoints return data in this format:

```json
{
  "data": [...],
  "pagination": {
    "page": 1,
    "limit": 20,
    "total": 100
  }
}
```

### Error Response

Error responses follow this format:

```json
{
  "error": {
    "message": "Error description",
    "code": "ERROR_CODE"
  }
}
```

---

## Error Handling

### Common HTTP Status Codes

- **200 OK**: Request successful
- **400 Bad Request**: Invalid parameters or request format
- **404 Not Found**: Resource not found
- **500 Internal Server Error**: Server error occurred

### Error Response Example

```json
{
  "error": {
    "message": "Invalid page number",
    "code": "INVALID_PARAMETER"
  }
}
```

---

## Rate Limiting

_(To be implemented)_

Currently, there are no rate limits enforced. This may change in future versions.

---

## Authentication

_(Currently not required)_

The API currently does not require authentication. If authentication is added in the future, this section will be updated.

---

## CORS

The API supports CORS for the following origins (configurable in `appsettings.json`):
- Development: `http://localhost:3000`, `http://localhost:3001`
- Production: Configured allowed origins

---

## Testing with Swagger

When running in **Development** mode, you can test all endpoints interactively using Swagger UI:

```
http://localhost:5130/swagger
```

Swagger provides:
- Interactive API documentation
- Request/response examples
- Ability to test endpoints directly from the browser
- Schema definitions for all models

---

## Example Usage

### Using cURL

```bash
# Get top growth stablecoins
curl http://localhost:5130/noodle/top-growth-stablecoins

# Search stablecoins
curl "http://localhost:5130/noodle/stablecoins?q=USD&page=1&limit=10"

# Get stocks with filters
curl "http://localhost:5130/noodle/stocks?limit=25&search=Apple"
```

### Using JavaScript (Fetch API)

```javascript
// Get top growth stablecoins
fetch('http://localhost:5130/noodle/top-growth-stablecoins')
  .then(response => response.json())
  .then(data => console.log(data))
  .catch(error => console.error('Error:', error));

// Search stablecoins with pagination
const params = new URLSearchParams({
  q: 'USD',
  page: 1,
  limit: 20
});

fetch(`http://localhost:5130/noodle/stablecoins?${params}`)
  .then(response => response.json())
  .then(data => console.log(data))
  .catch(error => console.error('Error:', error));
```

### Using Python (requests)

```python
import requests

# Get top growth stablecoins
response = requests.get('http://localhost:5130/noodle/top-growth-stablecoins')
data = response.json()
print(data)

# Search stablecoins with pagination
params = {
    'q': 'USD',
    'page': 1,
    'limit': 20
}
response = requests.get('http://localhost:5130/noodle/stablecoins', params=params)
data = response.json()
print(data)
```

---

## Changelog

### Version 1.0.0 (Current)
- Initial API release
- Stablecoins endpoints
- Stocks endpoints
- Commodities endpoints
- Price history endpoints

---

## Support

For questions or issues:
- Check the [README.md](README.md) for general documentation
- Review the [GETTING_STARTED.md](GETTING_STARTED.md) for quick setup
- Create an issue in the GitLab repository
- Contact the development team

---

## Additional Resources

- **Swagger UI** (Development only): `http://localhost:5130/swagger`
- **Main Documentation**: [README.md](README.md)
- **Quick Start Guide**: [GETTING_STARTED.md](GETTING_STARTED.md)

