# Script de Limpieza para HuertosApp
# Ejecutar desde la raíz del proyecto

Write-Host "?? Iniciando limpieza de HuertosApp..." -ForegroundColor Cyan

# Detener procesos de ADB si existen
Write-Host "Deteniendo procesos ADB..." -ForegroundColor Yellow
try {
    Stop-Process -Name "adb" -Force -ErrorAction SilentlyContinue
} catch {}

# Ejecutar dotnet clean
Write-Host "Ejecutando dotnet clean..." -ForegroundColor Yellow
dotnet clean "HuertosApp\HuertosApp.csproj"

# Eliminar carpetas obj y bin
Write-Host "Eliminando carpetas obj y bin..." -ForegroundColor Yellow
Remove-Item -Path "HuertosApp\obj" -Recurse -Force -ErrorAction SilentlyContinue
Remove-Item -Path "HuertosApp\bin" -Recurse -Force -ErrorAction SilentlyContinue

# Restaurar paquetes
Write-Host "Restaurando paquetes NuGet..." -ForegroundColor Yellow
dotnet restore "HuertosApp\HuertosApp.csproj"

# Compilar
Write-Host "Compilando proyecto..." -ForegroundColor Yellow
dotnet build "HuertosApp\HuertosApp.csproj"

Write-Host "? Limpieza completada!" -ForegroundColor Green
Write-Host "Ahora puedes ejecutar la aplicación desde Visual Studio" -ForegroundColor Cyan
