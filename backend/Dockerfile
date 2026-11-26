FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY src/Admission.Api/Admission.Api.csproj src/Admission.Api/
RUN dotnet restore src/Admission.Api/Admission.Api.csproj
COPY src/ src/
RUN dotnet publish src/Admission.Api/Admission.Api.csproj -c Release -o /app --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:9.0
WORKDIR /app
RUN apt-get update && apt-get install -y --no-install-recommends curl && rm -rf /var/lib/apt/lists/*
COPY --from=build /app .
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080
ENTRYPOINT ["dotnet", "Admission.Api.dll"]
