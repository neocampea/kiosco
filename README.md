# Kiosco / StockVentas

Aplicación de escritorio para gestión de stock, empleados y ventas.

## Plataformas soportadas

El proyecto usa **C# + .NET 8 + Avalonia UI**, sin depender de Windows Forms ni WPF:

- Windows x64
- Linux x64
- macOS Intel (x64)
- macOS Apple Silicon (ARM64)

## Requisitos

- .NET 8 SDK
- Git
- MySQL 8.x o MariaDB compatible
- Docker Desktop (opcional, recomendado para el entorno de desarrollo)

## Clonar

```bash
git clone https://github.com/neocampea/kiosco.git
cd kiosco/Kiosco-Stock-Paradigma-main
```

## Base de datos

Copiá la configuración:

Linux/macOS:
```bash
cp db.config.example db.config
```

Windows PowerShell:
```powershell
Copy-Item db.config.example db.config
```

Podés usar MySQL/MariaDB instalado localmente o Docker.

Con Docker, desde la raíz del repositorio:

```bash
docker compose up -d
```

El esquema se carga automáticamente en el primer arranque del volumen.

## Ejecutar

Dentro de `Kiosco-Stock-Paradigma-main`:

```bash
dotnet restore
dotnet run
```

También se puede configurar la conexión con la variable de entorno `STOCKVENTAS_CONNECTION_STRING`, evitando guardar credenciales en archivos.

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

El repositorio es público, pero eso no otorga permisos de escritura. Para colaborar directamente, agregá a cada integrante como colaborador desde la configuración de GitHub.

Flujo recomendado:

1. `main` se mantiene estable.
2. Cada integrante trabaja en su propia rama.
3. Se sube la rama al repositorio.
4. Se abre un Pull Request.
5. El grupo revisa los cambios.
6. Se integra a `main`.

No subir `db.config`, contraseñas, `.env`, `bin/`, `obj/` ni datos reales.

## Usuario inicial

Si no existe ningún Dueño, la aplicación crea:

- Usuario: `admin`
- Contraseña: `admin123`

Cambien la contraseña después del primer ingreso.
