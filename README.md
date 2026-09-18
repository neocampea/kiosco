# Kiosco / StockVentas

Aplicación de escritorio para gestión de stock, empleados y ventas.

## Plataformas soportadas

El proyecto usa **C# + .NET 8 + Avalonia UI + SQLite local**, sin depender de Windows Forms, WPF ni de un servidor de base de datos:

- Windows x64
- Linux x64
- macOS Intel (x64)
- macOS Apple Silicon (ARM64)

## Requisitos

- .NET 8 SDK
- Git

La aplicación crea automáticamente su base de datos SQLite local en el directorio de datos de la aplicación. No hace falta instalar MySQL, MariaDB ni Docker para ejecutarla.

## Clonar

```bash
git clone https://github.com/neocampea/kiosco.git
cd kiosco/Kiosco-Stock-Paradigma-main
```

## Base de datos local

Al iniciar por primera vez, la aplicación crea automáticamente:

- La carpeta de datos de StockVentas.
- El archivo `stockventas.db`.
- Las tablas de usuarios, productos, ventas e items de venta.

Los datos quedan guardados localmente y sobreviven al cierre de la aplicación.

SQLite utiliza almacenamiento local y memoria del sistema para trabajar rápidamente; no se necesita un servidor externo.

## Ejecutar

Dentro de `Kiosco-Stock-Paradigma-main`:

```bash
dotnet restore
dotnet run
```

## Publicar

Windows x64:
```bash
dotnet publish -c Release -r win-x64 --self-contained true
```

Linux x64:
```bash
dotnet publish -c Release -r linux-x64 --self-contained true
```

macOS Intel:
```bash
dotnet publish -c Release -r osx-x64 --self-contained true
```

macOS Apple Silicon:
```bash
dotnet publish -c Release -r osx-arm64 --self-contained true
```

## Trabajo en grupo

Cada integrante puede ejecutar el programa con su propia base SQLite local. Esto evita depender de un servidor para el desarrollo.

Flujo recomendado:

1. `main` se mantiene estable.
2. Cada integrante trabaja en su propia rama.
3. Se sube la rama al repositorio.
4. Se abre un Pull Request.
5. El grupo revisa los cambios.
6. Se integra a `main`.

No subir archivos de bases de datos locales, `bin/`, `obj/` ni datos reales.

## Usuario inicial

Si no existe ningún Dueño, la aplicación crea:

- Usuario: `admin`
- Contraseña: `admin123`

Cambien la contraseña después del primer ingreso.
