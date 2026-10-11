# Optional Windows container; LDAP runs on a separate Linux container engine.
FROM mcr.microsoft.com/dotnet/framework/sdk:4.8.1 AS build
WORKDIR /app
COPY aspnetapp.sln .
COPY aspnetapp/ aspnetapp/
RUN msbuild aspnetapp.sln /p:Configuration=Release
FROM mcr.microsoft.com/dotnet/framework/aspnet:4.8.1
WORKDIR /inetpub/wwwroot
COPY --from=build /app/aspnetapp/ ./
