FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

USER root

# Copy csproj and restore any dependencies
COPY *.csproj ./
RUN dotnet restore

# Copy the entire project and build it
COPY ./ ./
RUN dotnet publish -c Release -o out

# Build runtime image
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

# Copy the published output from the build environment
COPY --from=build /app/out .

# Set the environment variable for ASP.NET Core to use HTTPS
ENV ASPNETCORE_URLS=https://+:443;http://+:80

# Expose ports for HTTP and HTTPS
EXPOSE 80
EXPOSE 443

# COPY ./Config/credentials.p12 ./Config/

# Run the application
ENTRYPOINT ["dotnet", "GoogleGroups.dll"]