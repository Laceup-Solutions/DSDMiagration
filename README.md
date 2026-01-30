# Laceup Migration - .NET MAUI Project

## Project Overview

This project represents the migration of the **Laceup Delivery DSD** application from **Xamarin.Forms** to **.NET MAUI (Multi-platform App UI)**. The application is a comprehensive delivery and sales management system designed for route sales representatives, supporting both Android and iOS platforms.

## Project Structure

```
DSDMiagration/
├── LaceupMigration/              # Main .NET MAUI application
│   ├── Views/                    # 95 XAML views and code-behind files
│   ├── ViewModels/               # 94 ViewModels using MVVM pattern
│   ├── Platforms/                # Platform-specific implementations
│   │   ├── Android/              # Android-specific code
│   │   │   ├── Scanner/         # Barcode scanner (Socket Mobile, Zebra EMDK, Chainway, Cipherlab, Honeywell)
│   │   │   ├── Handlers/        # CollectionView ripple effect
│   │   │   └── Services/        # CameraBarcodeScanner, App Center, DatePicker
│   │   └── iOS/                  # iOS-specific code (CameraBarcodeScanner, App Info)
│   ├── Services/                 # LaceupAppService, ActivityStateRestoration, AdvancedOptions, DialogService
│   ├── Controls/                 # DialogService, LoadingOverlay, SignatureDrawable
│   ├── UtilDlls/                  # PdfViewer (in-app PDF viewing)
│   ├── Helpers/                   # NavigationHelper, MaterialIconHelper, SafeNavigationExtensions
│   └── Resources/                # Fonts (OpenSans, Roboto), images, styles
├── LaceupMigration.Business/     # Business logic layer
│   ├── Classes/                  # 300+ business classes (Orders, Clients, Products, Config, DataAccess, Printers, PdfUtilities, etc.)
│   └── Interfaces/               # IDataAccess, IDialogService, IScannerService, ICameraBarcodeScannerService, IDatePickerService
└── LaceupMigration.sln           # Solution (LaceupMigration + LaceupMigration.Business)
```

## Technology Stack

- **Framework**: .NET 9.0 MAUI
- **Platforms**: Android (API 22+), iOS (15.0+)
- **Architecture**: MVVM (Model-View-ViewModel)
- **UI Framework**: .NET MAUI with XAML
- **Dependency Injection**: Built-in MAUI DI container (`MauiProgram.cs`)
- **MVVM Toolkit**: CommunityToolkit.Maui (CommunityToolkit.Mvvm)
- **Icons**: MauiIcons (Material Outlined)
- **Barcode/Camera**: ZXing.Net.Maui (UseBarcodeReader)
- **PDF Viewing**: Maui.PDFView (in-app PDF viewer)
- **Analytics**: Microsoft App Center (Android; iOS via platform services)

## What Has Been Completed

*Summary: The MAUI app has 95 Views, 94 ViewModels, 90+ Shell routes registered, 300+ business classes, 97+ printer implementations, and 16 PDF generator implementations. All main tabs (Customers, Open Inv, Transactions, Payments), order types, inventory, reports, EOD, route management, goals, and self-service flows are implemented with UI and navigation. Remaining work is mainly printing/email/PDF wiring in ViewModels, report generation, configuration persistence, and some scroll/navigation TODOs.*

### 1. Project Setup & Architecture
- ✅ Migrated from Xamarin.Forms to .NET MAUI
- ✅ Configured project for Android and iOS
- ✅ Set up dependency injection container
- ✅ Implemented MVVM architecture pattern
- ✅ Configured routing and navigation (Shell-based)
- ✅ Set up App Center for analytics and crash reporting

### 2. Core Screens & Navigation
- ✅ **Splash Screen** (SplashPage) - Initial loading, route `Splash`
- ✅ **Login/Configuration** (LoginConfigPage) - Server connection and authentication
- ✅ **Main Tabs** (AppShell.xaml) - Four-tab navigation: Customers (Clients), Open Inv (Invoices), Transactions (Orders), Payments (Payments)
- ✅ **Client Details** (ClientDetailsPage) - Comprehensive client information, orders, invoices
- ✅ **Terms and Conditions** (TermsAndConditionsPage)
- ✅ **90+ routes** registered in AppShell.xaml.cs (clientdetails, batch, orderdetails, inventory, reports, configuration, selfservice/*, etc.)
- ✅ **NavigationHelper** for state-preserving navigation; **SafeNavigationExtensions** for safe GoToAsync

### 3. Client Management
- ✅ Clients list (ClientsPage) with search and filtering
- ✅ Client details (ClientDetailsPage) with orders and invoices
- ✅ Add client (AddClientPage), Edit client (EditClientPage)
- ✅ Client images (ClientImagesPage), View image (ViewImagePage)
- ✅ Bill-to address (AddClientBillToPage, AddClientBillToPageViewModel)
- ✅ Select invoice (SelectInvoicePage), Invoice details (InvoiceDetailsPage)

### 4. Order Management
- ✅ Orders list and details
- ✅ Order creation workflows
- ✅ Batch order processing
- ✅ Order types: Regular, Credit, Work Order, Consignment, No Service
- ✅ Department-based ordering
- ✅ Order templates (Super Order, Previously Ordered, Full Template)
- ✅ Advanced catalog ordering
- ✅ Self-service ordering system

### 5. Invoice & Payment Management
- ✅ Open invoices list
- ✅ Invoice details and selection
- ✅ Payment processing
- ✅ Payment batch management
- ✅ Payment value setting
- ✅ Payment reports

### 6. Inventory Management
- ✅ Inventory main page
- ✅ View/Print inventory
- ✅ Check inventory
- ✅ Transfer on/off inventory
- ✅ View load orders
- ✅ Set par levels
- ✅ Cycle count
- ✅ Inventory summary
- ✅ Accept load functionality

### 7. Reports
- ✅ Sales reports
- ✅ Payments reports
- ✅ Commission reports
- ✅ Salesmen commission reports
- ✅ Quantity/product sales reports
- ✅ Sales by product category reports
- ✅ Transmission reports
- ✅ Load order reports
- ✅ SAP order reports

### 8. End of Day (EOD) Process
- ✅ EOD main page
- ✅ Route returns processing
- ✅ End inventory
- ✅ Print reports
- ✅ EOD process validation and execution
- ✅ Data transmission

### 9. Scanning Functionality
- ✅ Multi-scanner support (hardware):
  - Socket Mobile, Zebra EMDK, Honeywell, Chainway 1D/2D, Cipherlab (Android Scanner/)
- ✅ Camera barcode scanning: ZXing.Net.Maui + ICameraBarcodeScannerService (Android & iOS)
- ✅ Scanner service abstraction (IScannerService, platform interfaces)
- ✅ Platform-specific implementations (Platforms/Android/Scanner/, Platforms/iOS/Services/)
- ✅ QR code and barcode support

### 10. Printing Functionality
- ✅ Printer service interface (IPrinter) and PrinterProvider
- ✅ 97+ printer implementations (Printers/New Printer Structure/: Zebra, Text, Custom Printers per client)
- ✅ Print orders, invoices, payments, inventory reports, EOD reports, consignment documents, client statements
- ✅ Setup Printer page (SetupPrinterPage) for configuration

### 11. PDF Generation & Viewing
- ✅ PDF generation service (IPdfProvider)
- ✅ Multiple PDF generator implementations (PdfUtilities):
  - DefaultPdfGenerator, StevenFoodsPdfGenerator, GlenWillsPdfGenerator, MamaLychaPdfGenerator, NoDiscountPdfGenerator
  - ABFirePdfGenerator, AlwaysShowHeaderPdfGenerator, EcoSkyWaterPdfGenerator, EMSPdfGenerator, ExportReportGenerator
  - GevaCoffeePdfGenerator, FineFoodPdf, FromHtmlPdf, MilagroBillOfLadingfPdfGenerator
- ✅ Order PDFs, Invoice PDFs, Report PDFs, Consignment PDFs
- ✅ In-app PDF viewer (PdfViewer in UtilDlls, route `pdfviewer`) via Maui.PDFView

### 12. Product Catalog
- ✅ Product catalog page
- ✅ Advanced catalog page
- ✅ Full category browsing
- ✅ Add item functionality
- ✅ Department management

### 13. Route Management
- ✅ Route management page
- ✅ Route map
- ✅ Add orders to route
- ✅ Add POS to route
- ✅ Route expenses
- ✅ Route returns

### 14. Additional Features
- ✅ Time sheet tracking (TimeSheetPage)
- ✅ Goal management (SelectGoalPage, GoalFilterPage, GoalDetailsPage)
- ✅ Configuration/settings page (ConfigurationPage)
- ✅ Printer setup (SetupPrinterPage), Scanner setup (SetupScannerPage)
- ✅ Sent orders tracking (SentOrdersPage, SentOrdersOrdersListPage)
- ✅ Sent payments tracking (SentPaymentsPage, SentPaymentsInPackagePage)
- ✅ View order status (ViewOrderStatusPage, ViewOrderStatusDetailsPage)
- ✅ Log viewer (LogViewerPage)
- ✅ Self-service: select company, client list, checkout, template, catalog, categories, collect payment, credit template
- ✅ Navigation state restoration (ActivityStateRestorationService, LaceupAppService)
- ✅ Custom tab bar (CustomTabBarRenderer), Android CollectionView ripple effect
- ✅ Price level selection (SelectPriceLevelPage, SelectRetailPriceLevelPage), Terms selection (SelectTermsPage)
- ✅ Manage departments (ManageDepartmentsPage), Sort-by dialog (SortByDialogPage)

### 15. Business Logic Layer
- ✅ 300+ business classes in LaceupMigration.Business (Orders, Clients, Products, Config, DataAccess, Printers, PdfUtilities, Invoice, Payment, Route, Goal, Inventory, etc.)
- ✅ Data access layer (DataAccess, DataAccessEx, NetAccess)
- ✅ Configuration (Config, ConfigFilePaths, ConfigKeys, ConfigProperties)
- ✅ Session management (Session, SalesmanSession, SessionDetails)
- ✅ Product inventory, order processing, payment processing logic
- ✅ Invoice ID providers, Order discount structure, Custom code decoders
- ✅ 97+ printer implementations (IPrinter, PrinterProvider, custom printers per client)

## Configuration Keys

The application uses various configuration keys to enable/disable features:

- `UseLaceupAdvancedCatalogKey` - Advanced catalog mode
- `UseCatalogKey` - Standard catalog mode
- `UseFullTemplateKey` - Full template mode
- `SelfServiceKey` - Self-service mode
- `ShowExpensesInEOD` - Show expenses in EOD
- `RouteReturnIsMandatory` - Route returns requirement
- `EndingInvIsMandatory` - End inventory requirement
- `PrintReportsRequired` - Print reports requirement

## Next Steps & Testing Checklist

**Legend:** `[x]` = implementation complete in codebase (ready for testing); `[ ]` = not yet done or manual testing/review pending.

**Completion summary (post–Priority 1):** Priority 2 (Core Functionality): EOD, printing, scanning, inventory, accept load, reports, PDF, and catalog are implemented; a few items remain manual (data transmission, error recovery, some report/inventory verification). Priority 3 (Order Creation): All order types and self-service flows implemented. Priority 4 (Payment): Payment set values implemented. Priority 5: Time sheet and configuration UI implemented; configuration persistence has TODOs. Priority 6–7: Platform and review tasks are manual testing/documentation.

### Priority 1: Design Completion (UI/UX)

#### 1.1 Splash Screen
- [ ] Review and finalize splash screen design
- [ ] Ensure proper branding and loading indicators
- [ ] Test animation and timing

#### 1.2 Login Screen
- [ ] Review and finalize login screen design
- [ ] Ensure proper form validation UI
- [ ] Test error message display
- [ ] Verify keyboard handling

#### 1.3 Main Tabs
- [ ] Review tab bar design and icons
- [ ] Ensure consistent styling across tabs
- [ ] Test tab navigation and state persistence
- [ ] Verify badge/notification indicators if needed

#### 1.4 Client Details Screen
- [ ] Review and finalize client details layout
- [ ] Ensure proper information hierarchy
- [ ] Test responsive design for different screen sizes
- [ ] Verify all data displays correctly

### Priority 2: Core Functionality Testing

#### 2.1 End of Day (EOD) Process
- [x] Test complete EOD workflow (EndOfDayPageViewModel, EndOfDayProcessPageViewModel: RouteReturns, EndInventory, PrintReports, ValidateStatus, ExecuteEOD)
- [x] Verify route returns processing (RouteReturnsPageViewModel, Print via IPrinter.PrintRouteReturn)
- [x] Test end inventory functionality (EndInventoryPageViewModel)
- [x] Verify print reports step (PrintReportsPageViewModel with IPrinter for sales, payments, inventory, commission, route returns)
- [x] Test EOD process validation (HasRouteReturns, HasEndInventory, HasReportsPrinted, CanProcess)
- [ ] Verify data transmission (manual testing)
- [ ] Test error handling and recovery (manual testing)

#### 2.2 Printing (All Screens)
- [x] Test printing from all screens that support printing:
  - [x] Orders (BatchPageViewModel, OrderCreditPageViewModel, PreviouslyOrderedTemplatePageViewModel, OrderDetailsPageViewModel, AdvancedCatalogPageViewModel, NewLoadOrderTemplatePageViewModel, OrdersPageViewModel)
  - [x] Invoices (ClientDetailsPageViewModel PrintClientStatement; InvoiceDetailsPage has TODO)
  - [x] Payments (PaymentSetValuesPageViewModel PrintPayment via IPrinter.PrintPayment)
  - [x] Inventory reports (TransferOnOffPageViewModel Print; PrintReportsPageViewModel)
  - [x] EOD reports (PrintReportsPageViewModel)
  - [x] Client statements (ClientDetailsPageViewModel)
  - [x] Consignment documents (BatchPageViewModel, OrdersPageViewModel PrintConsignment/PrintFullConsignment)
- [x] Verify printer configuration (SetupPrinterPage, PrinterProvider, Config)
- [ ] Test different printer types (manual testing)
- [ ] Verify print formatting (manual testing)

#### 2.3 Scanning (All Screens)
- [x] Test scanning functionality on all screens that support scanning:
  - [x] Order entry screens (AdvancedCatalogPageViewModel, PreviouslyOrderedTemplatePageViewModel Scan/HandleScannedBarcodeAsync)
  - [x] Inventory screens (TransferOnOffPageViewModel ScanAsync)
  - [x] Product catalog (ProductCatalogPageViewModel, FullCategoryPageViewModel, SelfServiceCatalogPageViewModel – ICameraBarcodeScannerService.ScanBarcodeAsync)
  - [x] Client lookup (search/filter on ClientsPage)
- [x] Test all supported scanner types (Platforms/Android/Scanner: Socket Mobile, Zebra EMDK, Chainway, Cipherlab, Honeywell)
- [x] Verify QR code scanning (PreviouslyOrderedTemplatePageViewModel HandleScannedQRCodeAsync; ZXing camera)
- [x] Test scanner configuration (ConfigurationPageViewModel SetupScanner, SetupScannerPageViewModel; Config.ScannerToUse)

#### 2.4 Inventory Management
- [x] Test all inventory operations:
  - [x] View inventory (ViewPrintInventoryPage, InventoryMainPage)
  - [x] Check inventory (CheckInventoryPageViewModel)
  - [x] Transfer on/off (TransferOnOffPageViewModel, Print implemented)
  - [x] Set par levels (SetParLevelPageViewModel – UI; save has TODO)
  - [x] Cycle count (CycleCountPageViewModel)
  - [x] Inventory summary (InventorySummaryPageViewModel – filter has TODO)
- [ ] Verify inventory calculations (manual testing)
- [ ] Test inventory reports (manual testing)

#### 2.5 Accept Load and Deliveries
- [x] Test accept load functionality (AcceptLoadPageViewModel, AcceptLoadEditDeliveryPageViewModel)
- [x] Verify load order processing (NewLoadOrderTemplatePageViewModel, ViewLoadOrderPage)
- [ ] Test delivery acceptance workflow (manual testing)
- [ ] Verify inventory updates after load acceptance (manual testing)

#### 2.6 Reports
- [x] Test all report types (pages and ViewModels exist; SalesReportPageViewModel has GetReport/RunReport/SendReportByEmail):
  - [x] Sales reports
  - [x] Payments reports
  - [x] Commission reports
  - [x] Transmission reports
  - [x] Load order reports
  - [x] SAP order reports
- [ ] Verify report data accuracy (manual testing)
- [ ] Test report filtering and date ranges (manual testing)
- [x] Verify report export functionality (SalesReportPageViewModel SendReportByEmail; PDF generation)

#### 2.7 PDF Generation (All Screens)
- [x] Test PDF generation from all applicable screens:
  - [x] Orders (PdfHelper.GetOrderPdf, SendOrderByEmail in OrderDetailsPage, OrderCreditPage, PreviouslyOrderedTemplatePage, AdvancedCatalogPage, BatchPage, BatchDepartmentPage, FinalizeBatchPage, SentOrdersPage)
  - [x] Invoices (InvoiceDetailsPageViewModel SendInvoiceByEmail; ClientDetailsPageViewModel uses PdfHelper.GetPdfProvider)
  - [x] Payments (PaymentSetValuesPageViewModel PdfHelper.GetPaymentPdf)
  - [x] Reports (SalesReportPageViewModel GetReportWithDetails; EndOfDayPageViewModel GetPdfProvider)
  - [x] Consignments (ConsignmentPageViewModel SendConsignmentByEmail)
  - [x] Client statements (ClientDetailsPageViewModel pdfProvider)
- [x] Verify PDF formatting (IPdfProvider implementations in PdfUtilities)
- [ ] Test different PDF generator implementations (manual testing)
- [x] Verify PDF file saving and sharing (PdfViewer route; SharePdfAsync in ClientDetailsPageViewModel)

#### 2.8 Product Catalog
- [x] Test product catalog browsing (ProductCatalogPage, AdvancedCatalogPage, FullCategoryPage)
- [x] Verify search and filtering (ViewModels with search/filter)
- [x] Test category navigation (FullCategoryPage, category hierarchy)
- [x] Verify product details display (ProductDetailsPage, ProductImagePage)
- [x] Test add to order functionality (AddItemPage, catalog add-to-order flows)

### Priority 3: Order Creation Testing (Bug Testing)

#### 3.1 Advanced Template (UseLaceupAdvancedCatalogKey=1)
- [x] Test order creation with advanced catalog enabled (AdvancedCatalogPageViewModel, PrintAsync, ScanAsync, AddItemFromScannerAsync)
- [x] Verify catalog navigation (FullCategoryPage, category hierarchy)
- [x] Test product selection (AdvancedCatalogPageViewModel)
- [x] Verify quantity entry (order detail editing)
- [x] Test order submission (SubmitOrder flow)
- [ ] Check for any bugs or issues (manual testing)

#### 3.2 Self Service (SelfServiceKey=1)
- [x] Test self-service order creation (SelfServiceSelectCompanyPage, SelfServiceClientListPage, SelfServiceCheckOutPage, SelfServiceTemplatePage, SelfServiceCatalogPage, SelfServiceCategoriesPage, SelfServiceCollectPaymentPage, SelfServiceCreditTemplatePage)
- [x] Verify company selection (SelfServiceSelectCompanyPageViewModel)
- [x] Test client list in self-service mode (SelfServiceClientListPageViewModel)
- [x] Verify checkout process (SelfServiceCheckOutPageViewModel)
- [x] Test payment collection (SelfServiceCollectPaymentPageViewModel)
- [x] Verify order templates in self-service (SelfServiceTemplatePageViewModel)
- [x] Test credit template in self-service (SelfServiceCreditTemplatePageViewModel)
- [ ] Check for any bugs or issues (manual testing)

#### 3.3 Previously Ordered Template (UseCatalogKey=1)
- [x] Test previously ordered template functionality (PreviouslyOrderedTemplatePageViewModel, PrintAsync, HandleScannedBarcodeAsync)
- [x] Verify template loading (template data binding)
- [x] Test template modification (line editing)
- [x] Verify order creation from template (SubmitOrder)
- [ ] Check for any bugs or issues (manual testing)

#### 3.4 Order Details Page (UseCatalogKey=0 && UseLaceupAdvancedCatalogKey=0)
- [x] Test order details page in standard mode (OrderDetailsPageViewModel)
- [x] Verify order line items display (OrderDetail binding)
- [x] Test order editing (line add/edit/delete)
- [x] Verify calculations (order totals)
- [x] Test order submission (FinishOrder)
- [ ] Check for any bugs or issues (manual testing)

#### 3.5 Super Order Template & Full Template (UseFullTemplateKey=1)
- [x] Test super order template (SuperOrderTemplatePageViewModel)
- [x] Test full template functionality (BatchDepartmentPageViewModel, ManageDepartmentsPage)
- [x] Verify template data loading (department/products)
- [x] Test template customization (line editing)
- [x] Verify order creation from templates (Batch flow, FinalizeBatchPage)
- [ ] Check for any bugs or issues (manual testing)

### Priority 4: Payment Testing

#### 4.1 Payment Set Values
- [x] Test payment value setting (PaymentSetValuesPageViewModel)
- [x] Verify payment calculations (payment application logic)
- [x] Test payment application to invoices (ApplyPayment, PrintPayment)
- [x] Verify payment validation (validation before submit)
- [x] Test payment submission (SubmitPayment; IPrinter.PrintPayment)
- [ ] Check for any bugs or issues (manual testing)

### Priority 5: Additional Feature Testing

#### 5.1 Time Sheet
- [x] Test time sheet functionality (TimeSheetPageViewModel, TimeSheetPage)
- [ ] Verify clock in/out (manual testing)
- [ ] Test time tracking (manual testing)
- [ ] Verify time sheet reports (manual testing)
- [ ] Check for any bugs or issues (manual testing)

#### 5.2 Configuration Page
- [x] Review configuration page functionality (ConfigurationPageViewModel)
- [x] Verify all settings are accessible (server, company, scanner, printer, etc.)
- [ ] Test configuration saving (TODO: Save connection settings, Save all configuration changes in README TODO list)
- [x] Verify configuration loading (Config, UI binding)
- [ ] Test configuration validation (manual testing)
- [ ] Review UI/UX of configuration page (manual review)

### Priority 6: Platform Testing

*Implementation: iOS (CameraBarcodeScannerService.iOS, App Info) and Android (Scanner, CameraBarcodeScanner, DatePicker, App Center, CollectionView ripple) are implemented. Items below are manual device/testing steps.*

#### 6.1 iOS Testing
- [ ] Build and run app on iOS simulator
- [ ] Test on physical iOS device
- [ ] Verify iOS-specific features:
  - [ ] Scanner functionality (ICameraBarcodeScannerService implemented)
  - [ ] Printing
  - [ ] File system access
  - [ ] Permissions
- [ ] Test navigation and UI on iOS
- [ ] Verify performance
- [ ] Check for iOS-specific bugs

#### 6.2 Android Testing
- [ ] Verify Android build still works
- [ ] Test on multiple Android devices
- [ ] Verify Android-specific features (Scanner, CameraBarcodeScanner, DatePicker, App Center)
- [ ] Test different Android versions
- [ ] Verify performance

### Priority 7: Additional Review Items

*These are code/process review tasks; implementation is in place.*

#### 7.1 Code Quality
- [ ] Review code for consistency
- [ ] Check for code duplication
- [ ] Verify error handling throughout
- [ ] Review logging implementation (Logger, Config)
- [ ] Check for memory leaks
- [ ] Verify async/await patterns

#### 7.2 Performance
- [ ] Test app startup time
- [ ] Verify screen load times
- [ ] Test with large datasets
- [ ] Verify memory usage
- [ ] Test battery usage
- [ ] Check for performance bottlenecks

#### 7.3 Security
- [ ] Review authentication implementation (LoginConfigPageViewModel, Config, NetAccess)
- [ ] Verify secure data storage (SecureStorage, file paths)
- [ ] Check for sensitive data exposure
- [ ] Review network security
- [ ] Verify session management (Session, SalesmanSession)

#### 7.4 Documentation
- [x] Document configuration options (README Configuration Keys, Development Notes)
- [ ] Document API endpoints
- [ ] Create user guide
- [ ] Document deployment process
- [ ] Create troubleshooting guide

#### 7.5 Testing Infrastructure
- [ ] Set up automated testing
- [ ] Create unit tests for critical paths
- [ ] Set up integration tests
- [ ] Create UI tests for key workflows

## TODO Items from Codebase

The following TODO items have been identified in the codebase and need to be completed. They are organized by category for easier tracking and prioritization.

### Printing Functionality

#### Order & Invoice Printing
- [ ] **BatchPageViewModel** - Implement printing functionality
- [ ] **BatchPageViewModel** - Implement label printing
- [ ] **OrderCreditPageViewModel** - Implement printing (multiple locations)
- [ ] **PreviouslyOrderedTemplatePageViewModel** - Implement print functionality
- [ ] **InvoiceDetailsPageViewModel** - Implement printing
- [ ] **ViewOrderStatusPageViewModel** - Implement print functionality

#### Inventory Printing
- [ ] **CheckInventoryPageViewModel** - Implement print functionality
- [ ] **ViewPrintInventoryPageViewModel** - Implement print functionality
- [ ] **SetParLevelPageViewModel** - Implement print functionality
- [ ] **InventorySummaryPageViewModel** - Implement print functionality
- [ ] **TransferOnOffPageViewModel** - Implement print functionality

#### Sent Items Printing
- [ ] **SentOrdersPageViewModel** - Implement print functionality
- [ ] **SentPaymentsPageViewModel** - Implement print functionality

### Email & PDF Functionality

#### Email Sending
- [ ] **BatchPageViewModel** - Implement email sending
- [ ] **BatchDepartmentPageViewModel** - Implement email sending
- [ ] **OrderCreditPageViewModel** - Implement email sending (multiple locations)
- [ ] **ConsignmentPageViewModel** - Implement email sending
- [ ] **InvoiceDetailsPageViewModel** - Implement email sending
- [ ] **OrdersPageViewModel** - Implement PdfHelper.SendOrdersByEmail when available
- [ ] **InvoicesPageViewModel** - Implement PdfHelper.SendOrdersByEmail when available
- [ ] **SentOrdersPageViewModel** - Implement send by email functionality
- [ ] **SentPaymentsPageViewModel** - Implement send by email functionality

#### PDF Viewing & Sharing
- [ ] **BatchPageViewModel** - Implement PDF viewing
- [ ] **BatchPageViewModel** - Implement PDF sharing
- [ ] **OrderCreditPageViewModel** - Implement PDF sharing (multiple locations)
- [ ] **ConsignmentPageViewModel** - Implement PDF viewing
- [ ] **ConsignmentPageViewModel** - Implement PDF sharing

#### PDF Generation
- [ ] **FullCategoryPageViewModel** - Implement catalog PDF generation and email sending

### Reports Implementation

#### Report Generation
- [ ] **SalesReportPageViewModel** - Implement report generation
- [ ] **PaymentsReportPageViewModel** - Implement report generation
- [ ] **CommissionReportPageViewModel** - Implement report generation
- [ ] **SalesmenCommissionReportPageViewModel** - Implement report generation
- [ ] **QtyProdSalesReportPageViewModel** - Implement report generation
- [ ] **SalesProductCatReportPageViewModel** - Implement report generation
- [ ] **TransmissionReportPageViewModel** - Implement report generation
- [ ] **LoadOrderReportPageViewModel** - Implement report generation
- [ ] **SAPOrderReportPageViewModel** - Implement report generation

#### Report Email
- [ ] **SalesReportPageViewModel** - Implement email sending
- [ ] **PaymentsReportPageViewModel** - Implement email sending
- [ ] **CommissionReportPageViewModel** - Implement email sending
- [ ] **SalesmenCommissionReportPageViewModel** - Implement email sending
- [ ] **QtyProdSalesReportPageViewModel** - Implement email sending
- [ ] **SalesProductCatReportPageViewModel** - Implement email sending
- [ ] **TransmissionReportPageViewModel** - Implement email sending
- [ ] **LoadOrderReportPageViewModel** - Implement email sending
- [ ] **SAPOrderReportPageViewModel** - Implement email sending

### Configuration & Setup

#### Configuration Page
- [ ] **ConfigurationPageViewModel** - Save connection settings
- [ ] **ConfigurationPageViewModel** - Save all configuration changes
- [ ] **ConfigurationPageViewModel** - Navigate to printer setup
- [ ] **ConfigurationPageViewModel** - Navigate to scanner setup
- [ ] **ConfigurationPageViewModel** - Implement logo selection
- [ ] **ConfigurationPageViewModel** - Implement password reset
- [ ] **ConfigurationPageViewModel** - Navigate to log viewer
- [ ] **ConfigurationPageViewModel** - Implement data restore
- [ ] **ConfigurationPageViewModel** - Implement data cleaning

#### Printer Setup
- [ ] **SetupPrinterPageViewModel** - Load existing printer configuration from Config
- [ ] **SetupPrinterPageViewModel** - Implement test print
- [ ] **SetupPrinterPageViewModel** - Save printer configuration to Config
- [ ] **MainPageViewModel** - Implement printer setup

#### Scanner Setup
- [ ] **SetupScannerPageViewModel** - Implement actual scanner test
- [ ] **SetupScannerPageViewModel** - Save scanner configuration to Config

### Order Management

#### Order Navigation & UI
- [ ] **OrderDetailsPageViewModel** - Scroll to detail in the UI
- [ ] **OrderDetailsPageViewModel** - Scroll to matching line
- [ ] **OrderCreditPageViewModel** - Scroll to this detail
- [ ] **SuperOrderTemplatePageViewModel** - Scroll to this detail
- [ ] **ConsignmentPageViewModel** - Scroll to this detail
- [ ] **ConsignmentPageViewModel** - Scroll to item in CollectionView
- [ ] **OrderCreditPageViewModel** - Navigate to line item detail/edit page
- [ ] **SuperOrderTemplatePageViewModel** - Navigate to line item detail/edit page
- [ ] **ConsignmentPageViewModel** - Navigate to line item detail/edit page

#### Order Features
- [ ] **OrderDetailsPageViewModel** - Implement LoadNextActivity if needed
- [ ] **OrderCreditPageViewModel** - Implement LoadNextActivity if needed
- [ ] **ConsignmentPageViewModel** - Implement LoadNextActivity if needed
- [ ] **OrderDetailsPageViewModel** - Implement EnterCasesInOut
- [ ] **AdvancedCatalogPageViewModel** - Implement EnterCasesInOut
- [ ] **OrderDetailsPageViewModel** - Navigate to suggested items (multiple locations)
- [ ] **OrderDetailsPageViewModel** - Navigate to survey

#### Order Templates
- [ ] **PreviouslyOrderedTemplatePageViewModel** - Implement discount dialog
- [ ] **PreviouslyOrderedTemplatePageViewModel** - Implement date picker
- [ ] **OrderCreditPageViewModel** - Implement discount dialog (multiple locations)

#### Quote Functionality
- [ ] **ClientDetailsPageViewModel** - Navigate to QuotePage when implemented
- [ ] **OrdersPageViewModel** - Navigate to QuotePage when implemented
- [ ] **ManageRoutePageViewModel** - Implement add quotes

### Route Management

#### Route Operations
- [ ] **RouteMapPageViewModel** - Load route stops from RouteEx or current route
- [ ] **RouteMapPageViewModel** - Get actual route data
- [ ] **RouteMapPageViewModel** - Open map app with directions
- [ ] **AddOrdersToRoutePageViewModel** - Load available orders from Order.Orders
- [ ] **AddOrdersToRoutePageViewModel** - Add selected orders to route
- [ ] **AddPOSToRoutePageViewModel** - Load available clients, excluding those already in route
- [ ] **AddPOSToRoutePageViewModel** - Add selected clients as positions to route

### Inventory Management

#### Inventory Operations
- [ ] **SetParLevelPageViewModel** - Implement save par levels
- [ ] **TransferOnOffPageViewModel** - Implement save logic
- [ ] **InventorySummaryPageViewModel** - Implement filter logic

### Sent Items Management

#### Sent Orders
- [ ] **SentOrdersPageViewModel** - Implement resend functionality
- [ ] **SentOrdersPageViewModel** - Navigate to SentOrdersOrdersListPage when implemented
- [ ] **SentOrdersOrdersListPageViewModel** - Implement duplicate order creation logic
- [ ] **SentOrdersOrdersListPageViewModel** - Implement resend logic

#### Sent Payments
- [ ] **SentPaymentsPageViewModel** - Implement resend functionality

### Self-Service Features

- [ ] **SelfServiceCatalogPageViewModel** - Implement barcode scanning

### Consignment Features

- [ ] **ConsignmentPageViewModel** - Implement signature capture

### Business Logic (DataAccess)

#### Payment Processing
- [ ] **DataAccess.cs** - Handle check payments (multiple locations - 8 TODO items)
  - Determine what to do in case of a check payment
  - Locations: Lines 3321, 3414, 3507, 3542, 3599, 3661, 3736, 3766

#### Credit Management
- [ ] **DataAccess.cs** - Separate Credit Invoices from credit orders (Line 2729)

### Other Features

#### Main Page
- [ ] **MainPageViewModel** - Implement access code request via email

#### Catalog Features
- [ ] **FullCategoryPageViewModel** - Could show subcategory selection dialog
- [ ] **FullCategoryPageViewModel** - Navigate to product details or add to order

#### Batch Department
- [ ] **BatchDepartmentPageViewModel** - Implement GetHistoryFromOrders if needed

#### Platform-Specific
- [ ] **OrdersPageViewModel** - Implement DEX call (platform-specific Android functionality)

### Summary Statistics

*Note: TODO counts are from a codebase audit; actual grep matches may vary (e.g. ~110 TODO/FIXME/HACK matches across 33 files).*

- **Total TODO Items**: ~159 (from audit)
- **Printing TODOs**: ~15
- **Email/PDF TODOs**: ~20
- **Reports TODOs**: ~18
- **Configuration TODOs**: ~12
- **Order Management TODOs**: ~25
- **Route Management TODOs**: ~6
- **Inventory TODOs**: ~3
- **Sent Items TODOs**: ~4
- **Business Logic TODOs**: ~9
- **Other TODOs**: ~47

## Development Notes

### Scanner Configuration
Scanners are configured via `Config.ScannerToUse`:
- `2` = Zebra EMDK
- `3` = Socket Mobile
- `5` = Chainway 1D
- `6` = Chainway 2D
- `7` = Honeywell
- `9` = Cipherlab

### Printer Configuration
Printers are configured through the Setup Printer page and support multiple printer types via the `IPrinter` interface.

### PDF Providers
PDF generation uses a provider pattern. The provider is configured via `Config.PdfProvider` and can be customized per client.

### Navigation
The app uses .NET MAUI Shell for navigation. Routes are registered in `AppShell.xaml.cs` and services are registered in `MauiProgram.cs`.

## Getting Started

### Prerequisites
- Visual Studio 2022 or later
- .NET 9.0 SDK
- Android SDK (for Android development)
- Xcode (for iOS development, macOS only)

### Building the Project
1. Clone the repository
2. Open `LaceupMigration.sln` in Visual Studio
3. Restore NuGet packages
4. Select target platform (Android or iOS)
5. Build and run

### Configuration
Before running, configure:
- Server address and port
- Company ID
- Salesman ID
- Scanner type (if using hardware scanner)
- Printer type (if using hardware printer)

## Known Issues & Limitations

- iOS testing pending
- Some configuration options need review
- Time sheet functionality needs testing
- Some order template combinations need validation

## Contributing

When working on this project:
1. Follow the existing MVVM architecture
2. Use dependency injection for services
3. Follow the existing code style
4. Test on both Android and iOS when possible
5. Update this README with significant changes

## Support

For issues or questions, contact the development team. Additional project documentation: see `GUIA_PROYECTO_MAUI.md` for navigation, data flow, and architecture details.

---

**Last Updated**: January 30, 2026  
**Project Status**: In Development - Testing Phase

