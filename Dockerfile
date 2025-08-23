# ========== Build ==========
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copia y restaura
COPY . .
RUN dotnet restore

# Publica (UseAppHost=false evita binario nativo innecesario)
RUN dotnet publish ./ApiEcommerce.csproj -c Release -o /app/publish /p:UseAppHost=false

# ========== Runtime ==========
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

# Variables típicas para prod y contenedor
ENV ASPNETCORE_ENVIRONMENT=Production
# IMPORTANTE: Kestrel debe escuchar en el puerto que Railway expone en $PORT
# No se expande ${PORT} en tiempo de build, así que lo seteamos en CMD.
# (Alternativa: configurarlo en el panel de Railway como variable)
COPY --from=build /app/publish .

# Command: vincula ASPNETCORE_URLS al $PORT que pone Railway y arranca
CMD bash -lc 'ASPNETCORE_URLS=http://0.0.0.0:${PORT:-8080} dotnet ApiEcommerce.dll'
