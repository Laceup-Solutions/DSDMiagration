# Guía Completa del Proyecto MAUI - LaceupMigration

## 📋 Tabla de Contenidos
1. [Estructura del Proyecto](#estructura-del-proyecto)
2. [Navegación](#navegación)
3. [Almacenamiento y Carga de Información](#almacenamiento-y-carga-de-información)
4. [Acceso a Datos (DataAccess)](#acceso-a-datos-dataaccess)

---

## 🏗️ Estructura del Proyecto

### Arquitectura General

El proyecto está dividido en **dos proyectos principales**:

#### 1. **LaceupMigration** (Proyecto Principal - UI)
Este es el proyecto de la aplicación MAUI que contiene toda la interfaz de usuario:

```
LaceupMigration/
├── Views/              # Páginas XAML (91 páginas)
│   ├── MainPage.xaml
│   ├── ClientsPage.xaml
│   ├── OrdersPage.xaml
│   └── ...
├── ViewModels/         # ViewModels (91 ViewModels)
│   ├── MainPageViewModel.cs
│   ├── ClientsPageViewModel.cs
│   └── ...
├── Services/          # Servicios de la aplicación
│   ├── LaceupAppService.cs
│   ├── DialogService.cs
│   └── ...
├── Helpers/           # Clases auxiliares
│   ├── NavigationHelper.cs
│   └── ...
├── Controls/          # Controles personalizados
├── Platforms/         # Código específico de plataforma (Android/iOS)
├── Resources/         # Recursos (imágenes, estilos, fuentes)
├── App.xaml           # Aplicación principal
├── AppShell.xaml      # Shell de navegación
└── MauiProgram.cs     # Configuración e inyección de dependencias
```

#### 2. **LaceupMigration.Business** (Capa de Negocio)
Contiene toda la lógica de negocio y acceso a datos:

```
LaceupMigration.Business/
├── Classes/           # Clases de dominio (304 archivos)
│   ├── DataAccess.cs      # Acceso principal a datos
│   ├── DataAccessEx.cs     # Extensiones de DataAccess
│   ├── Client.cs          # Modelo de Cliente
│   ├── Order.cs           # Modelo de Orden
│   ├── Product.cs         # Modelo de Producto
│   ├── Config/            # Configuración
│   └── ...
└── Interfaces/        # Interfaces para servicios
```

### Patrón de Arquitectura: MVVM (Model-View-ViewModel)

El proyecto sigue el patrón **MVVM**:

- **Model**: Clases en `LaceupMigration.Business/Classes` (Client, Order, Product, etc.)
- **View**: Páginas XAML en `LaceupMigration/Views`
- **ViewModel**: Clases en `LaceupMigration/ViewModels`

### Inyección de Dependencias

La configuración se realiza en `MauiProgram.cs`:

```csharp
// Servicios registrados como Singleton (una instancia para toda la app)
builder.Services.AddSingleton<IInterfaceHelper, InterfaceHelper>();
builder.Services.AddSingleton<IScannerService, ScannerService>();
builder.Services.AddSingleton<DialogService>();

// Views y ViewModels registrados como Transient (nueva instancia cada vez)
builder.Services.AddTransient<ClientsPage>();
builder.Services.AddTransient<ClientsPageViewModel>();
```

---

## 🧭 Navegación

### Sistema de Navegación: Shell Navigation

MAUI usa **Shell Navigation** para la navegación. El proyecto implementa un sistema híbrido:

#### 1. **AppShell.xaml** - Definición de Rutas

El `AppShell.xaml` define la estructura de navegación:

```xml
<Shell>
    <!-- Página inicial -->
    <ShellContent Route="Splash" ContentTemplate="{DataTemplate local:SplashPage}" />
    
    <!-- TabBar principal con 4 pestañas -->
    <TabBar Route="MainPage">
        <ShellContent Route="Clients" ContentTemplate="{DataTemplate local:ClientsPage}" />
        <ShellContent Route="Invoices" ContentTemplate="{DataTemplate local:InvoicesPage}" />
        <ShellContent Route="Orders" ContentTemplate="{DataTemplate local:OrdersPage}" />
        <ShellContent Route="Payments" ContentTemplate="{DataTemplate local:PaymentsPage}" />
    </TabBar>
</Shell>
```

#### 2. **AppShell.xaml.cs** - Registro de Rutas

Todas las rutas se registran en el constructor de `AppShell`:

```csharp
public AppShell(MainPageViewModel mainPageViewModel)
{
    InitializeComponent();
    
    // Registro de rutas
    Routing.RegisterRoute("clientdetails", typeof(ClientDetailsPage));
    Routing.RegisterRoute("orderdetails", typeof(OrderDetailsPage));
    Routing.RegisterRoute("batch", typeof(BatchPage));
    // ... más rutas
}
```

#### 3. **NavigationHelper** - Helper para Navegación

El proyecto tiene un helper personalizado (`NavigationHelper.cs`) que:

- **Guarda el estado de navegación** automáticamente
- **Mapea rutas a ActivityTypes** (para compatibilidad con Xamarin)
- **Maneja parámetros de consulta**

**Ejemplo de uso:**

```csharp
// Navegación simple
await NavigationHelper.GoToAsync("clientdetails");

// Navegación con parámetros
await NavigationHelper.GoToAsync("clientdetails?clientId=123");

// Navegación sin guardar estado
await NavigationHelper.GoToAsync("loginconfig", saveState: false);
```

#### 4. **Navegación desde ViewModels**

Los ViewModels usan `Shell.Current.GoToAsync()` o `NavigationHelper`:

```csharp
// En un ViewModel
[RelayCommand]
private async Task NavigateToClientDetailsAsync(int clientId)
{
    await NavigationHelper.GoToAsync($"clientdetails?clientId={clientId}");
}
```

#### 5. **Pasar Parámetros entre Páginas**

**Opción 1: Query Parameters (Recomendado)**
```csharp
// Navegar con parámetros
await Shell.Current.GoToAsync($"orderdetails?orderId={orderId}&clientId={clientId}");

// Recibir en el ViewModel
public async Task InitializeAsync(int orderId, int clientId)
{
    _order = Order.Orders.FirstOrDefault(x => x.OrderId == orderId);
    _client = Client.Clients.FirstOrDefault(x => x.ClientId == clientId);
}
```

**Opción 2: Usando QueryProperty Attribute**
```csharp
[QueryProperty(nameof(OrderId), "orderId")]
public partial class OrderDetailsPageViewModel : ObservableObject
{
    [ObservableProperty]
    private int _orderId;
    
    partial void OnOrderIdChanged(int value)
    {
        // Cargar datos cuando cambia OrderId
        LoadOrderData(value);
    }
}
```

---

## 💾 Almacenamiento y Carga de Información

### Sistema de Almacenamiento: Archivos Locales

El proyecto **NO usa base de datos tradicional**. En su lugar, usa:

1. **Archivos CSV** para datos estructurados
2. **Archivos XML** para datos complejos
3. **Archivos de texto** para logs y configuración
4. **SecureStorage** (MAUI) para datos sensibles (tarjetas de crédito, tokens)

### Estructura de Directorios

Todos los archivos se almacenan en `FileSystem.AppDataDirectory` (directorio de datos de la app):

```
AppDataDirectory/
├── DataStatic/              # Datos estáticos
│   ├── clients.cvs          # Lista de clientes
│   ├── products.cvs         # Catálogo de productos
│   └── InvoicesData/        # Facturas por cliente
├── LaceupData/              # Datos dinámicos
│   ├── Data/                # Datos de la aplicación
│   │   ├── Orders.xml       # Órdenes guardadas
│   │   ├── inventory.cvs    # Inventario
│   │   └── ...
│   ├── Orders/              # Archivos de órdenes individuales
│   ├── BatchData/           # Datos de lotes
│   ├── PaymentsData/        # Datos de pagos
│   └── CurrentOrders/       # Órdenes temporales
├── Images/                  # Imágenes de productos
├── ClientPictures/          # Fotos de clientes
└── Logos/                   # Logos de empresas
```

### Configuración de Rutas

Las rutas se definen en `Config/ConfigFilePaths.cs`:

```csharp
public static string BasePath => FileSystem.AppDataDirectory;
public static string CodeBase => Path.Combine(BasePath, "LaceupData");
public static string DataPath => Path.Combine(CodeBase, "Data");
public static string ClientStoreFile => Path.Combine(StaticDataPath, "clients.cvs");
public static string ProductStoreFile => Path.Combine(StaticDataPath, "products.cvs");
public static string OrderStorageFile => Path.Combine(DataPath, "Orders.xml");
```

### Inicialización de Datos

El proceso de inicialización ocurre en varios puntos:

#### 1. **Al Iniciar la App** (`App.xaml.cs`)
```csharp
public App(IServiceProvider serviceProvider, AppShell appShell)
{
    // Inicializa Config y crea directorios
    Config.Initialize();
}
```

#### 2. **Al Hacer Login** (`LoginConfigPageViewModel.cs`)
```csharp
// Después de autenticación exitosa
DataAccess.Initialize();              // Carga datos desde archivos locales
DataAccess.GetSalesmanSettings();     // Descarga configuración del servidor
DataAccessEx.DownloadStaticData();    // Descarga clientes y productos
```

#### 3. **Carga de Datos Locales** (`DataAccessEx.Initialize()`)
```csharp
public static void Initialize()
{
    LoadingData = true;
    
    // Cargar inventario
    ProductInventory.Load();
    
    // Cargar productos desde archivo
    if (File.Exists(Config.ProductStoreFile))
        LoadData__(Config.ProductStoreFile, false, true);
    
    // Cargar clientes desde archivo
    if (File.Exists(Config.ClientStoreFile))
        LoadData__(Config.ClientStoreFile, false, false);
    
    // Cargar órdenes
    DataAccess.LoadOrders();
    
    // Cargar pagos
    DataAccess.LoadPayments();
    
    // ... más cargas
}
```

### Sincronización con Servidor

Los datos se sincronizan con el servidor mediante `NetAccess`:

```csharp
// En MainPageViewModel.DownloadDataAsync()
using (var access = new NetAccess())
{
    access.OpenConnection();
    access.WriteStringToNetwork("HELO");
    access.WriteStringToNetwork(Config.GetAuthString());
    
    // Descargar productos
    access.WriteStringToNetwork("Products");
    access.ReceiveFile(Config.ProductStoreFile);
    
    // Descargar clientes
    access.WriteStringToNetwork("Clients");
    access.ReceiveFile(Config.ClientStoreFile);
}
```

### Guardado de Datos

Cada modelo tiene métodos `Save()` y `Delete()`:

```csharp
// Ejemplo: Guardar una orden
var order = new Order();
order.ClientId = 123;
order.Save();  // Guarda en archivo XML

// Ejemplo: Guardar un cliente
var client = new Client();
client.ClientName = "Nuevo Cliente";
client.Save();  // Guarda en archivo CSV
```

---

## 🔌 Acceso a Datos (DataAccess)

### Clases Principales

#### 1. **DataAccess** (`LaceupMigration.Business/Classes/DataAccess.cs`)
Clase principal para acceso a datos. Hereda de `DataAccessEx`.

#### 2. **DataAccessEx** (`LaceupMigration.Business/Classes/DataAccessEx.cs`)
Contiene métodos estáticos para:
- Cargar datos desde archivos
- Descargar datos del servidor
- Procesar archivos CSV/XML

### Cómo Acceder a los Datos desde ViewModels

Los datos están disponibles como **colecciones estáticas** en las clases de modelo:

#### Ejemplo 1: Acceder a Clientes

```csharp
public partial class ClientsPageViewModel : ObservableObject
{
    public async Task RefreshAsync()
    {
        // Acceder a la colección estática de clientes
        var clients = Client.Clients;  // Lista de todos los clientes
        
        // Filtrar clientes
        var routeClients = Client.Clients
            .Where(c => c.RouteId == currentRouteId)
            .ToList();
        
        // Buscar un cliente específico
        var client = Client.Clients.FirstOrDefault(c => c.ClientId == clientId);
    }
}
```

#### Ejemplo 2: Acceder a Órdenes

```csharp
public partial class OrdersPageViewModel : ObservableObject
{
    public void LoadOrders()
    {
        // Todas las órdenes
        var orders = Order.Orders;
        
        // Órdenes pendientes
        var pendingOrders = Order.Orders
            .Where(o => o.Status == OrderStatus.Pending)
            .ToList();
        
        // Orden específica
        var order = Order.Orders.FirstOrDefault(o => o.OrderId == orderId);
    }
}
```

#### Ejemplo 3: Acceder a Productos

```csharp
public partial class ProductCatalogPageViewModel : ObservableObject
{
    public void LoadProducts()
    {
        // Todos los productos
        var products = Product.Products;
        
        // Productos de una categoría
        var categoryProducts = Product.Products
            .Where(p => p.CategoryId == categoryId)
            .ToList();
        
        // Buscar producto por código de barras
        var product = Product.Products
            .FirstOrDefault(p => p.Barcode == barcode);
    }
}
```

### Métodos Estáticos de DataAccess

#### Cargar Datos desde Archivos

```csharp
// Cargar órdenes desde archivo
DataAccess.LoadOrders();

// Cargar pagos desde archivo
DataAccess.LoadPayments();

// Cargar lotes
DataAccess.LoadBatches();

// Cargar inventario
ProductInventory.Load();
```

#### Descargar Datos del Servidor

```csharp
// Descargar datos estáticos (productos y clientes)
string result = DataAccessEx.DownloadStaticData();

// Descargar todos los datos (sincronización completa)
string result = DataAccessEx.DownloadData(updateProducts: true, updateInventory: true);

// Descargar configuración del vendedor
DataAccess.GetSalesmanSettings();
```

#### Verificar Estado

```csharp
// Verificar si puede usar la aplicación
bool canUse = DataAccess.CanUseApplication();

// Verificar si debe hacer End of Day
bool mustEOD = DataAccess.MustEndOfDay();

// Verificar autorización
DataAccess.CheckAuthorization();
if (Config.AuthorizationFailed)
{
    // Usuario no autorizado
}
```

### Flujo Completo de Carga de Datos

```
1. Usuario inicia sesión
   ↓
2. LoginConfigPageViewModel.ContinueSignIn()
   ↓
3. DataAccess.Initialize()
   - Carga ProductInventory
   - Carga productos desde Config.ProductStoreFile
   - Carga clientes desde Config.ClientStoreFile
   - Carga órdenes desde archivos XML
   - Carga pagos
   ↓
4. DataAccess.GetSalesmanSettings()
   - Descarga configuración del servidor
   ↓
5. DataAccessEx.DownloadStaticData()
   - Descarga productos actualizados
   - Descarga clientes actualizados
   ↓
6. Los ViewModels acceden a:
   - Client.Clients (colección estática)
   - Order.Orders (colección estática)
   - Product.Products (colección estática)
```

### Ejemplo Completo: Cargar Datos en un ViewModel

```csharp
public partial class ClientsPageViewModel : ObservableObject
{
    public ObservableCollection<ClientListItemViewModel> Clients { get; } = new();
    
    public async Task OnAppearingAsync()
    {
        // Verificar si los datos están cargados
        if (!DataAccess.CanUseApplication())
        {
            await _dialogService.ShowAlertAsync("Debe sincronizar datos primero", "Advertencia");
            return;
        }
        
        // Cargar clientes desde la colección estática
        await RefreshAsync();
    }
    
    private async Task RefreshAsync()
    {
        IsBusy = true;
        
        try
        {
            // Acceder a Client.Clients (colección estática cargada en DataAccess.Initialize())
            var allClients = Client.Clients;
            
            // Filtrar según el modo de visualización
            var filteredClients = _displayMode == DisplayMode.Route
                ? allClients.Where(c => RouteEx.Routes.Any(r => r.ClientId == c.ClientId))
                : allClients;
            
            // Aplicar búsqueda si existe
            if (!string.IsNullOrEmpty(SearchQuery))
            {
                filteredClients = filteredClients
                    .Where(c => c.ClientName.Contains(SearchQuery, StringComparison.OrdinalIgnoreCase));
            }
            
            // Actualizar la colección observable
            Clients.Clear();
            foreach (var client in filteredClients)
            {
                Clients.Add(new ClientListItemViewModel(client));
            }
        }
        finally
        {
            IsBusy = false;
        }
    }
}
```

### Notas Importantes

1. **Las colecciones son estáticas**: `Client.Clients`, `Order.Orders`, `Product.Products` son propiedades estáticas que se cargan una vez al iniciar la app.

2. **Los datos se cargan en memoria**: Todos los datos se cargan en memoria al inicio. No hay consultas a base de datos en tiempo real.

3. **Sincronización manual**: Los datos se sincronizan con el servidor cuando el usuario presiona "Sync" o automáticamente según configuración.

4. **Persistencia en archivos**: Los cambios se guardan inmediatamente en archivos locales usando métodos `Save()`.

5. **Thread Safety**: Las operaciones de carga/guardado deben ejecutarse en el hilo principal o usar locks para evitar condiciones de carrera.

---

## 📝 Resumen de Conceptos Clave

### Estructura
- **2 proyectos**: LaceupMigration (UI) y LaceupMigration.Business (Lógica)
- **Patrón MVVM**: Views, ViewModels, Models separados
- **Inyección de dependencias**: Configurada en `MauiProgram.cs`

### Navegación
- **Shell Navigation**: Sistema de navegación de MAUI
- **Rutas registradas**: En `AppShell.xaml.cs`
- **NavigationHelper**: Helper personalizado que guarda estado
- **Query Parameters**: Para pasar datos entre páginas

### Almacenamiento
- **Archivos locales**: CSV, XML, texto plano
- **SecureStorage**: Para datos sensibles
- **Estructura de directorios**: Definida en `Config`
- **Persistencia**: Métodos `Save()` y `Delete()` en modelos

### Acceso a Datos
- **Colecciones estáticas**: `Client.Clients`, `Order.Orders`, `Product.Products`
- **DataAccess.Initialize()**: Carga datos al inicio
- **DataAccessEx.DownloadData()**: Sincroniza con servidor
- **Acceso directo**: Los ViewModels acceden directamente a las colecciones estáticas

---

## 🚀 Próximos Pasos para Desarrollar

1. **Entender el flujo de una página**:
   - Ver cómo `ClientsPage.xaml` se conecta con `ClientsPageViewModel`
   - Observar cómo se cargan los datos en `OnAppearingAsync()`

2. **Practicar navegación**:
   - Crear una nueva página y registrarla en `AppShell.xaml.cs`
   - Navegar desde un ViewModel usando `NavigationHelper`

3. **Acceder a datos**:
   - Usar `Client.Clients`, `Order.Orders`, `Product.Products` en ViewModels
   - Filtrar y buscar usando LINQ

4. **Guardar datos**:
   - Llamar `Save()` en modelos después de modificaciones
   - Entender cómo se persisten en archivos

---

¡Espero que esta guía te ayude a entender el proyecto! Si tienes preguntas específicas sobre alguna parte, no dudes en preguntar.

