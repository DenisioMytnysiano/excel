FROM mcr.microsoft.com/dotnet/sdk:6.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["Excel.API/Excel.API.csproj", "Excel.API/"]
COPY ["Excel.Core/Excel.Core.csproj", "Excel.Core/"]
COPY ["Excel.Infrastructure/Excel.Infrastructure.csproj", "Excel.Infrastructure/"]
COPY ["Excel.Tests.Unit/Excel.Tests.Unit.csproj", "Excel.Tests.Unit/"]
COPY ["Excel.Tests.Integration/Excel.Tests.Integration.csproj", "Excel.Tests.Integration/"]
RUN dotnet restore -v diag "Excel.API/Excel.API.csproj"
RUN dotnet restore -v diag "Excel.Tests.Unit/Excel.Tests.Unit.csproj"
RUN dotnet restore -v diag "Excel.Tests.Integration/Excel.Tests.Integration.csproj"
COPY . .
WORKDIR "/src/Excel.API"
RUN dotnet build "Excel.API.csproj" -c Release -o /app/build
WORKDIR "/src/Excel.Tests.Unit"
RUN dotnet build "Excel.Tests.Unit.csproj" -c Release -o /app/tests
WORKDIR "/src/Excel.Tests.Integration"
RUN dotnet build "Excel.Tests.Integration.csproj" -c Release -o /app/integration-tests

FROM build AS publish
WORKDIR "/src/Excel.API"
RUN dotnet publish "Excel.API.csproj" -c Release -o /app/publish/build /p:UseAppHost=false
WORKDIR "/src/Excel.Tests.Unit"
RUN dotnet publish "Excel.Tests.Unit.csproj" -c Release -o /app/publish/tests /p:UseAppHost=false
WORKDIR "/src/Excel.Tests.Integration"
RUN dotnet publish "Excel.Tests.Integration.csproj" -c Release -o /app/publish/integration-tests /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "build/Excel.API.dll"]
