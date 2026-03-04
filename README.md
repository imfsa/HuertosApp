# HuertosApp
![.NET MAUI](https://img.shields.io/badge/.NET%20MAUI-9.0-512BD4?logo=.net)
![Android](https://img.shields.io/badge/Android-21%2B-3DDC84?logo=android)
![Version](https://img.shields.io/badge/version-4.0-blue)


Aplicación móvil multiplataforma para la gestión de huertos operacionales, desarrollada con .NET MAUI (Target: .NET 9).

## Descripción

HuertosApp es una app cliente que incluye pantallas para autenticación, descarga/actualización de datos y registro de cosechas. Está diseñada para ejecutarse en Android, iOS y Windows usando .NET MAUI.

## Requisitos

- .NET 9 SDK
- .NET MAUI workload: `dotnet workload install maui`
- Visual Studio 2022/2023 con soporte MAUI (recomendado) o Visual Studio for Mac para iOS
- Android SDK/NDK y emuladores configurados
- Para iOS: macOS con Xcode y configuración de firma

## Clonar el repositorio

```bash
git clone https://github.com/imfsa/HuertosApp "HuertosAppV2-master"
cd "HuertosAppV2-master"
```

## Restaurar dependencias y compilar (CLI)

Restaurar paquetes:

```bash
dotnet restore
```

Compilar para Android:

```bash
dotnet build -f net9.0-android -c Debug
```

Compilar para Windows:

```bash
dotnet build -f net9.0-windows10.0.19041.0 -c Debug
```

Ejecutar desde Visual Studio: abrir la solución `HuertosApp.sln`, seleccionar el proyecto `HuertosApp`, elegir la plataforma (Android, iOS o Windows) y presionar ejecutar/depurar.

## Estructura y archivos relevantes

- Vistas y páginas: `HuertosApp/Pages/` (por ejemplo `LoginPage.xaml`)
- Lógica de páginas: `HuertosApp/Pages/*.xaml.cs`
- Estilos y colores: `HuertosApp/Resources/Styles/Styles.xaml`, `HuertosApp/Resources/Styles/Colors.xaml`
- Recursos por plataforma: `HuertosApp/Platforms/Android/Resources/` (ej. `values/colors.xml`)
- Imágenes: `HuertosApp/Resources/Images/` (asegúrate de incluir `logo.png`, `footer_logo.png` si se usan en UI)

## Ejecutar en dispositivos/emuladores

- Android: usar un emulador o dispositivo conectado; en la CLI se puede usar `dotnet build` y desplegar desde Visual Studio.
- iOS: requiere macOS y configuración de signing.
- Windows: ejecutar la configuración `net9.0-windows10.0.19041.0` en Visual Studio.

## Contribuir

1. Crear una rama: `git checkout -b feature/descripción`
2. Hacer commits claros y pequeños
3. Abrir un Pull Request contra `main`

## Notas

- Este proyecto usa .NET MAUI y apunta a .NET 9. No usar referencias a Xamarin.Forms.
- Verifica las rutas de los recursos y que las imágenes estén incluidas en `Resources/Images`.

## Licencia

Revisa el archivo `LICENSE` en la raíz del repositorio si está presente o consulta con los mantenedores.

---

Si quieres, puedo añadir badges, instrucciones de CI, o una guía rápida de debugging.

