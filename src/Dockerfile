# Use the .NET SDK image to build and test the app
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

# Copy the solution file and restore the dependencies
COPY *.sln .
COPY */*.csproj ./
RUN for file in $(ls *.csproj); do mkdir -p ${file%.*} && mv $file ${file%.*}; done
RUN dotnet restore

# Copy everything else and build the app
COPY . ./aspnetapp
WORKDIR /app/aspnetapp
RUN dotnet build -c Release

# Run the tests
WORKDIR /app/aspnetapp/Otus.Teaching.PromoCodeFactory.UnitTests
RUN dotnet test --no-restore --verbosity normal

# Publish the app
WORKDIR /app/aspnetapp
RUN dotnet publish -c Release -o out

# Use the ASP.NET Core runtime image
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app/aspnetapp/out ./
ENTRYPOINT ["dotnet", "Otus.Teaching.PromoCodeFactory.WebHost.dll"]
