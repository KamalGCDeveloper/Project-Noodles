Noodle API endpoint documentation

This document is a complete plain language reference for all available API endpoints.

Base URL information
All endpoints are prefixed with /noodle.
Development base URL is http://localhost:5130/noodle.
Production base URL is https://your-domain.com/noodle.

Sections in this document
Stablecoins
Stocks
Commodities
Price history
Response formats
Error handling

Stablecoins endpoints

Get top growth stablecoins
Endpoint GET /noodle/top-growth-stablecoins
Purpose returns stablecoins with the highest growth rate.
Typical response includes symbol growth and related fields.

Get most talked about stablecoins
Endpoint GET /noodle/most-talked-about-stablecoins
Purpose returns stablecoins with the highest mention count.
Typical response includes symbol mentions and related fields.

Get number of tracked stablecoins
Endpoint GET /noodle/stablecoins-number-tracked
Purpose returns total number of stablecoins being tracked.
Typical response includes count and related fields.

Get active users for stablecoins
Endpoint GET /noodle/active-users-stablecoins
Purpose returns active users tracking stablecoins.
Typical response includes activeUsers and related fields.

Get stablecoins list
Endpoint GET /noodle/stablecoins
Purpose returns paginated stablecoins list with search support.
Query parameters are q optional search text, page optional default 1, and limit optional default 20.
Example request path is /noodle/stablecoins?q=USD&page=1&limit=20.
Typical response includes a data array and pagination object with page limit and total.

Stocks endpoints

Get top growth stocks
Endpoint GET /noodle/top-growth-stocks
Purpose returns stocks with highest growth rate.
Typical response includes symbol growth and related fields.

Get most talked about stocks
Endpoint GET /noodle/most-talked-about-stocks
Purpose returns stocks with highest mention count.
Typical response includes symbol mentions and related fields.

Get number of tracked stocks
Endpoint GET /noodle/stock-number-tracked
Purpose returns total number of tracked stocks.
Typical response includes count and related fields.

Get active users for stocks
Endpoint GET /noodle/active-users-stock
Purpose returns active users tracking stocks.
Typical response includes activeUsers7d and related fields.

Get stocks list
Endpoint GET /noodle/stocks
Purpose returns paginated stock list with search and optional group filter.
Query parameters are limit optional default 25, page optional default 1, search optional text, and groupFilter optional category.
Example request path is /noodle/stocks?limit=25&page=1&search=Apple&groupFilter=tech.
Typical response includes a data array and pagination with page limit and total.

Commodities endpoints

Get top growth commodities
Endpoint GET /noodle/top-growth-commodities
Purpose returns commodities with highest growth rate.
Typical response includes name growth and related fields.

Get most talked about commodities
Endpoint GET /noodle/most-talked-about-commodities
Purpose returns commodities with highest mention count.
Typical response includes name mentions and related fields.

Get number of tracked commodities
Endpoint GET /noodle/commodities-number-tracked
Purpose returns total tracked commodities.
Typical response includes count and related fields.

Get active users for commodities
Endpoint GET /noodle/active-users-commodities
Purpose returns active users tracking commodities.
Typical response includes activeUsers7d and related fields.

Get commodities list
Endpoint GET /noodle/commodities
Purpose returns paginated commodities list with optional group filter.
Query parameters are limit optional default 10, page optional default 1, and groupFilter optional category.
Example request path is /noodle/commodities?limit=10&page=1&groupFilter=metals.
Typical response includes data array and pagination with page limit and total.

Price history endpoint

Get price history
Endpoint GET /noodle/price-history
Purpose returns historical price points for an asset.
Query parameters follow the PriceHistoryRequest model and usually include asset identifier, time range, and granularity fields.
Example request path is /noodle/price-history?symbol=AAPL&from=2024-01-01&to=2024-12-31.
Typical response includes data points with timestamp price and related fields.

Response formats

Success responses usually include data and optionally pagination.
Pagination usually includes page limit and total.

Error responses usually include an error message and an error code.

Error handling

Common status codes
200 means request successful.
400 means bad request or invalid parameters.
404 means resource not found.
500 means internal server error.

Rate limiting
Rate limiting is not currently enforced and may be added in future versions.

Authentication
Authentication is not currently required.

CORS
CORS support is configurable.
Typical development origins include http://localhost:3000 and http://localhost:3001.
Production origins are configured by environment.

Testing with Swagger
In development mode Swagger UI is available at http://localhost:5130/swagger.
Swagger provides interactive endpoint testing, request and response examples, and schema definitions.

Example usage

Example curl commands
curl http://localhost:5130/noodle/top-growth-stablecoins
curl "http://localhost:5130/noodle/stablecoins?q=USD&page=1&limit=10"
curl "http://localhost:5130/noodle/stocks?limit=25&search=Apple"

Example JavaScript fetch flow
Call the endpoint URL.
Parse JSON response.
Handle errors in catch block.

Example Python requests flow
Send GET request.
Parse JSON response.
Use query parameters when needed.

Changelog

Version 1.0.0 current
Initial API release.
Stablecoins endpoints added.
Stocks endpoints added.
Commodities endpoints added.
Price history endpoint added.

Support
For issues review README and GETTING_STARTED documents, open a repository issue, or contact the development team.

Additional resources
Swagger UI development URL is http://localhost:5130/swagger.
Main documentation is in README.md.
Quick start guide is in GETTING_STARTED.md.

