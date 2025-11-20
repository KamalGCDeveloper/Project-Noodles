# Base ASP.NET Runtime 9.0
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 5130

ENV ASPNETCORE_URLS=http://0.0.0.0:5130

# Build using SDK 9.0
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

COPY . .

RUN dotnet restore

RUN dotnet publish -c Release -o /publish

# Final stage
FROM base AS final
WORKDIR /app
COPY --from=build /publish .

# ENTRYPOINT chuẩn để tránh lỗi shell
ENTRYPOINT ["dotnet"]
CMD ["Noodle.Api.dll"]