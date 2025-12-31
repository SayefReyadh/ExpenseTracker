# Build stage
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy csproj and restore dependencies
COPY ["ExpenseTrackerAPI.csproj", "./"]
RUN dotnet restore "ExpenseTrackerAPI.csproj"

# Copy everything else and build
COPY . .
RUN dotnet publish "ExpenseTrackerAPI.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:9.0
WORKDIR /app
EXPOSE 8080

# Copy published app
COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "ExpenseTrackerAPI.dll"]
