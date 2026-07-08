# 1. Etapa de compilación
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build-env
WORKDIR /app

# Copiar archivos del proyecto y restaurar dependencias
COPY *.csproj ./
RUN dotnet restore

# Copiar el resto del código y compilar
COPY . ./
RUN dotnet publish -c Release -o out

# 2. Etapa de ejecución
FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app
COPY --from=build-env /app/out .

# Exponer el puerto por defecto (Render lo sobreescribirá con la variable PORT)
EXPOSE 5000

ENTRYPOINT ["dotnet", "GestionEventosAPI.dll"]