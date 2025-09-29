# Stage 1: Build
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy csproj and restore dependencies
COPY *.csproj ./
RUN dotnet restore

# Copy everything else and build
COPY . ./
RUN dotnet publish -c Release -o /app --no-restore

# Stage 2: Runtime
FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app

# Copy the published output from build stage
COPY --from=build /app ./

# Expose the port the app listens on
EXPOSE 5000
EXPOSE 5001

# Entry point
ENTRYPOINT ["dotnet", "image-app.dll"]
# Stage 1: Build
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy csproj and restore dependencies
COPY *.csproj ./
RUN dotnet restore

# Copy everything else and build
COPY . ./
RUN dotnet publish -c Release -o /app --no-restore

# Stage 2: Runtime
FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app

# Copy the published output from build stage
COPY --from=build /app ./

# Expose the port the app listens on
EXPOSE 5000
EXPOSE 5001

# Entry point
ENTRYPOINT ["dotnet", "image-app.dll"]
