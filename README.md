# Skaso Handheld Switcher

**Skaso Handheld Switcher** es una aplicación de código abierto desarrollada en Windows Forms (C# .NET 8) diseñada específicamente para facilitar la activación y desactivación del **Modo Xbox (Xbox Handheld Mode)** en Windows 11. 

Originalmente, esta característica está oculta y requiere modificar el registro del sistema y habilitar IDs experimentales de ViVeTool. Esta herramienta automatiza todo el proceso de forma segura y cuenta con una interfaz amigable (estilo Discord Dark) para alternar el estado del sistema con un solo clic.

## Requisitos

- Windows 11 Build **26100+** (24H2 con KB5083631 o 25H2 Dev Channel)
- Permisos de **Administrador**
- .NET 8 SDK (solo para compilar desde código)
- Conexion a Internet (para descarga automatica de recursos)

## Archivos del proyecto

| Archivo | Proposito |
|---------|-----------|
| `Program.cs` | Punto de entrada `[STAThread]` |
| `MainForm.cs` | UI principal (layout Discord Dark, badge, verificaciones, herramientas) |
| `ToggleSwitch.cs` | Control toggle personalizado dibujado con GDI+ |
| `SkasoHandheldSwitcher.csproj` | Configuracion del proyecto .NET 8 |
| `physpanel.exe` | Herramienta para simular panel handheld (embedded resource) |
| `ViVeTool.exe` | ViVeTool para feature flags (embedded resource) |
| `Albacore.ViVe.dll` | Libreria ViVe (embedded resource) |
| `Newtonsoft.Json.dll` | Dependencia de ViVeTool (embedded resource) |
| `FeatureDictionary.pfs` | Diccionario de features de ViVeTool (embedded resource) |

## Recursos incrustados

Los archivos en la carpeta del proyecto marcados como `EmbeddedResource` viajan dentro del .exe y se extraen a `C:\ProgramData\Skaso\` en tiempo de ejecucion.

## Funcionamiento

### Al abrir la app
1. Verifica permisos de administrador
2. Lee `CurrentBuild` del registro y valida >= 26100
3. Lee `HKLM\...\OEM\DeviceForm` para determinar estado actual
4. Muestra badge verde "ACTIVO" / gris "INACTIVO"

### Activar (toggle ON)
1. **Registro**: `DeviceForm = 0x2e` (identifica el PC como handheld)
2. **Recursos**: Extrae physpanel.exe + ViVeTool.exe + .dlls
3. **Tarea programada**: Crea `SkasoPhyspanelStartup` como **SYSTEM** (no solo admin) con `schtasks /create /ru SYSTEM`
4. **ViVeTool**: Activa feature IDs `58989070` y `59765208`

### Desactivar (toggle OFF)
1. **Registro**: Elimina `DeviceForm`
2. **Tarea**: Elimina la tarea programada
3. **ViVeTool**: Desactiva los feature IDs
4. **Limpieza**: Elimina `C:\ProgramData\Skaso\`

### Herramientas extra (panel inferior)
- **Aplicar solo ViVeTool**: Activa/desactiva los feature IDs seleccionados sin tocar registro ni physpanel
- Muestra estado actual de cada ID via `/query`

## Comandos ViVeTool

```powershell
# Activar
ViVeTool.exe /enable /id:58989070
ViVeTool.exe /enable /id:59765208

# Desactivar
ViVeTool.exe /disable /id:58989070  
ViVeTool.exe /disable /id:59765208

# Consultar estado
ViVeTool.exe /query /id:58989070
```

## Notas tecnicas

- **physpanel** requiere ejecucion como **SYSTEM**, no solo admin. La tarea programada usa `<UserId>SYSTEM</UserId>`.
- Los feature IDs corresponden al Xbox Mode lanzado con KB5083631 (Abril/Mayo 2026). Microsoft puede cambiarlos en updates futuros (actualizar en `xboxFeatureIds` en `MainForm.cs`).
- ViVeTool v0.3.4 requiere .NET Framework 4.8.1 y Newtonsoft.Json.dll.

## Compilar

```powershell
# Build rapido (DLL)
dotnet build -c Release

# Publicar single-file EXE (autocontenido, ~150 MB)
dotnet publish -c Release -r win-x64 --self-contained true /p:PublishSingleFile=true /p:UseAppHost=true
```

O doble click en `publish.bat`.

## Descargar recursos

Automatico: `publish.bat` descarga physpanel y ViVeTool si no existen.

Manual:
- `download-physpanel.bat` — descarga physpanel.exe oficial
- `download-vivetool.bat` — descarga ViVeTool oficial

---

## Agradecimientos

Este proyecto no sería posible sin el increíble trabajo de la comunidad Open Source. Un agradecimiento especial a:

- **[ViVeTool](https://github.com/thebookisclosed/ViVe)**: Creado por [thebookisclosed (Lucas)](https://github.com/thebookisclosed). Herramienta fundamental para interactuar con los feature flags ocultos de Windows.
- **[physpanel](https://github.com/riverar/physpanel)**: Creado por [riverar (Rafael Rivera)](https://github.com/riverar). Herramienta clave para simular el comportamiento de un panel físico y habilitar la interfaz adaptada.

---

## ⚠️ Descargo de Responsabilidad (Disclaimer)

Esta herramienta modifica claves del registro del sistema (Registry) y características ocultas de Windows mediante ViVeTool, además de crear tareas programadas con privilegios de `SYSTEM`. 

El autor no se hace responsable por cualquier inestabilidad del sistema, pérdida de datos, pantallazos azules (BSoD) o daños que puedan ocurrir por el uso de esta herramienta. **Úsalo bajo tu propio riesgo.** Se recomienda crear un punto de restauración del sistema antes de activarlo.

## Licencia

Este proyecto se distribuye bajo la licencia **MIT**. Puedes usarlo, modificarlo y distribuirlo libremente, siempre y cuando se incluya el aviso de copyright y la nota de exención de responsabilidad.
