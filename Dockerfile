# syntax=docker/dockerfile:1
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY Directory.Build.props Directory.Packages.props nuget.config global.json Intake.sln ./
COPY src/ src/
COPY tests/ tests/
RUN dotnet restore Intake.sln
RUN dotnet build Intake.sln -c Release --no-restore

FROM build AS test
RUN dotnet test Intake.sln -c Release --no-build --logger "console;verbosity=normal"

FROM build AS publish
RUN dotnet publish src/Intake.Worker/Intake.Worker.csproj -c Release --no-restore -o /app

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=publish /app .
ENV ASPNETCORE_HTTP_PORTS=8080
EXPOSE 8080
ENTRYPOINT ["dotnet", "Intake.Worker.dll"]
