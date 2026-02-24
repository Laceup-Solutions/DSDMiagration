using CommunityToolkit.Maui;
using LaceupMigration.Controls;
using LaceupMigration.Views;
using MauiIcons.Material.Outlined;
using Microsoft.Extensions.Logging;
using Maui.PDFView;
using Microsoft.Maui.Controls.Compatibility.Hosting;
using ZXing.Net.Maui;
using ZXing.Net.Maui.Controls;

namespace LaceupMigration
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
				.UseMauiCommunityToolkit()
				.UseMauiPdfView()
				.UseMaterialOutlinedMauiIcons()
				.UseMauiCompatibility()
				.ConfigureMauiHandlers(handlers =>
				{
					handlers.AddHandler(typeof(Shell), typeof(CustomTabBarRenderer));
#if IOS
					// Map SearchBar to add Done button and white background
					Microsoft.Maui.Handlers.SearchBarHandler.Mapper.AppendToMapping("CustomSearchBar", Platforms.iOS.Handlers.SearchBarHandlerMapper.MapSearchBar);
#endif
#if ANDROID
					// Map CollectionView to apply Gray100 ripple to RecyclerView items
					Microsoft.Maui.Controls.Handlers.Items.CollectionViewHandler.Mapper.AppendToMapping("RippleEffect", Platforms.Android.Handlers.CollectionViewRippleMapper.MapCollectionView);
#endif
					handlers.AddCompatibilityRenderers();
				})
				.UseBarcodeReader()
                .ConfigureFonts(fonts =>
                {
					fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
					fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                    fonts.AddFont("roboto-regular.ttf", "Roboto");
                    fonts.AddFont("roboto-medium.ttf", "Roboto-Medium");
                    fonts.AddFont("roboto-bold.ttf", "Roboto-Bold");
                });

#if DEBUG
    		builder.Logging.AddDebug();
#endif

            builder.Services.AddSingleton<IInterfaceHelper, InterfaceHelper>();
            builder.Services.AddSingleton<IScannerService, ScannerService>();

            // Register CameraBarcodeScannerService for platform-specific camera scanning
#if ANDROID
            builder.Services.AddSingleton<LaceupMigration.Business.Interfaces.ICameraBarcodeScannerService, Platforms.Android.Services.CameraBarcodeScannerService>();
            builder.Services.AddSingleton<LaceupMigration.Business.Interfaces.IDatePickerService, Platforms.Android.DatePickerService>();
#elif IOS
            builder.Services.AddSingleton<LaceupMigration.Business.Interfaces.ICameraBarcodeScannerService, Platforms.iOS.Services.CameraBarcodeScannerService>();
            builder.Services.AddSingleton<LaceupMigration.Business.Interfaces.IDatePickerService, Platforms.iOS.DatePickerService>();
#endif

            // Register DialogService - use same instance for both interface and concrete type
            var dialogService = new DialogService();
            builder.Services.AddSingleton<IDialogService>(dialogService);
            builder.Services.AddSingleton(dialogService);

			builder.Services.AddSingleton<Services.ILaceupAppService, Services.LaceupAppService>();
			builder.Services.AddSingleton<Services.AdvancedOptionsService>();
			builder.Services.AddSingleton<Services.IActivityStateRestorationService, Services.ActivityStateRestorationService>();

			// Register AppShell and SelfServiceShell (separate shell for self service module)
			builder.Services.AddSingleton<AppShell>();
			builder.Services.AddSingleton<Views.SelfService.SelfServiceShell>();

			// Views + ViewModels
			builder.Services.AddTransient<SplashPage>();
			builder.Services.AddTransient<ViewModels.SplashPageViewModel>();
			builder.Services.AddTransient<LoginConfigPage>();
			builder.Services.AddTransient<ViewModels.LoginConfigPageViewModel>();
			builder.Services.AddTransient<TermsAndConditionsPage>();
			builder.Services.AddTransient<ViewModels.TermsAndConditionsPageViewModel>();
			builder.Services.AddTransient<MainPage>();
			builder.Services.AddSingleton<ViewModels.MainPageViewModel>();
		builder.Services.AddTransient<ClientsPage>();
		builder.Services.AddTransient<ViewModels.ClientsPageViewModel>();
		builder.Services.AddTransient<ClientDetailsPage>();
		builder.Services.AddTransient<ViewModels.ClientDetailsPageViewModel>();
		builder.Services.AddTransient<SelectInvoicePage>();
		builder.Services.AddTransient<ViewModels.SelectInvoicePageViewModel>();
		builder.Services.AddTransient<PaymentSelectClientPage>();
		builder.Services.AddTransient<ViewModels.PaymentSelectClientPageViewModel>();
		builder.Services.AddTransient<PaymentSetValuesPage>();
		builder.Services.AddTransient<ViewModels.PaymentSetValuesPageViewModel>();
		builder.Services.AddTransient<CreateDepositPage>();
		builder.Services.AddTransient<ViewModels.CreateDepositPageViewModel>();
		builder.Services.AddTransient<InvoiceDetailsPage>();
		builder.Services.AddTransient<ViewModels.InvoiceDetailsPageViewModel>();
		builder.Services.AddTransient<EditClientPage>();
		builder.Services.AddTransient<ViewModels.EditClientPageViewModel>();
		builder.Services.AddTransient<InvoicesPage>();
		builder.Services.AddTransient<ViewModels.InvoicesPageViewModel>();
		builder.Services.AddTransient<OrdersPage>();
		builder.Services.AddTransient<ViewModels.OrdersPageViewModel>();
		builder.Services.AddTransient<PaymentsPage>();
		builder.Services.AddTransient<ViewModels.PaymentsPageViewModel>();
		
		// Order related pages
		builder.Services.AddTransient<BatchPage>();
		builder.Services.AddTransient<ViewModels.BatchPageViewModel>();
		builder.Services.AddTransient<FinalizeBatchPage>();
		builder.Services.AddTransient<ViewModels.FinalizeBatchPageViewModel>();
		builder.Services.AddTransient<OrderSignaturePage>();
		builder.Services.AddTransient<ViewModels.OrderSignaturePageViewModel>();
		builder.Services.AddTransient<OrderDetailsPage>();
		builder.Services.AddTransient<ViewModels.OrderDetailsPageViewModel>();
		builder.Services.AddTransient<OrderCreditPage>();
		builder.Services.AddTransient<ViewModels.OrderCreditPageViewModel>();
		builder.Services.AddTransient<SuperOrderTemplatePage>();
		builder.Services.AddTransient<ViewModels.SuperOrderTemplatePageViewModel>();
		builder.Services.AddTransient<PreviouslyOrderedTemplatePage>();
		builder.Services.AddTransient<ViewModels.PreviouslyOrderedTemplatePageViewModel>();
		builder.Services.AddTransient<Views.ItemGroupedTemplate.NewOrderTemplatePage>();
		builder.Services.AddTransient<ViewModels.NewOrderTemplatePageViewModel>();
		builder.Services.AddTransient<Views.ItemGroupedTemplate.NewCreditTemplatePage>();
		builder.Services.AddTransient<ViewModels.NewCreditTemplatePageViewModel>();
		builder.Services.AddTransient<Views.ItemGroupedTemplate.NewOrderTemplateDetailsPage>();
		builder.Services.AddTransient<ViewModels.NewOrderTemplateDetailsPageViewModel>();
		builder.Services.AddTransient<BatchDepartmentPage>();
		builder.Services.AddTransient<ViewModels.BatchDepartmentPageViewModel>();
		builder.Services.AddTransient<WorkOrderPage>();
		builder.Services.AddTransient<ViewModels.WorkOrderPageViewModel>();
		builder.Services.AddTransient<ConsignmentPage>();
		builder.Services.AddTransient<ViewModels.ConsignmentPageViewModel>();
		builder.Services.AddTransient<NoServicePage>();
		builder.Services.AddTransient<ViewModels.NoServicePageViewModel>();
		
		// Supporting pages
		builder.Services.AddTransient<ManageDepartmentsPage>();
		builder.Services.AddTransient<ViewModels.ManageDepartmentsPageViewModel>();
		builder.Services.AddTransient<ClientImagesPage>();
		builder.Services.AddTransient<ViewModels.ClientImagesPageViewModel>();
		builder.Services.AddTransient<ViewInvoiceImagesPage>();
		builder.Services.AddTransient<ViewModels.ViewInvoiceImagesPageViewModel>();
		builder.Services.AddTransient<FullCategoryPage>();
		builder.Services.AddTransient<ViewModels.FullCategoryPageViewModel>();
		builder.Services.AddTransient<FullProductListPage>();
		builder.Services.AddTransient<ViewModels.FullProductListPageViewModel>();
		builder.Services.AddTransient<AddItemPage>();
		builder.Services.AddTransient<ViewModels.AddItemPageViewModel>();
		builder.Services.AddTransient<AdvancedCatalogPage>();
		builder.Services.AddTransient<ViewModels.AdvancedCatalogPageViewModel>();
		builder.Services.AddTransient<ProductCatalogPage>();
		builder.Services.AddTransient<ViewModels.ProductCatalogPageViewModel>();
		builder.Services.AddTransient<ProductImagePage>();
		builder.Services.AddTransient<ViewModels.ProductImagePageViewModel>();
		builder.Services.AddTransient<ViewImagePage>();
		builder.Services.AddTransient<ViewModels.ViewImagePageViewModel>();
		builder.Services.AddTransient<ViewCapturedImagesPage>();
		builder.Services.AddTransient<ViewModels.ViewCapturedImagesPageViewModel>();
		builder.Services.AddTransient<ProductDetailsPage>();
		builder.Services.AddTransient<ViewModels.ProductDetailsPageViewModel>();
		builder.Services.AddTransient<TimeSheetPage>();
		builder.Services.AddTransient<ViewModels.TimeSheetPageViewModel>();
		
		// Menu pages
		builder.Services.AddTransient<AddClientPage>();
		builder.Services.AddTransient<ViewModels.AddClientPageViewModel>();
		builder.Services.AddTransient<SentOrdersPage>();
		builder.Services.AddTransient<ViewModels.SentOrdersPageViewModel>();
		builder.Services.AddTransient<SentOrdersOrdersListPage>();
		builder.Services.AddTransient<ViewModels.SentOrdersOrdersListPageViewModel>();
		builder.Services.AddTransient<SentPaymentsPage>();
		builder.Services.AddTransient<ViewModels.SentPaymentsPageViewModel>();
		builder.Services.AddTransient<SentPaymentsInPackagePage>();
		builder.Services.AddTransient<ViewModels.SentPaymentsInPackagePageViewModel>();
		builder.Services.AddTransient<ViewOrderStatusPage>();
		builder.Services.AddTransient<ViewModels.ViewOrderStatusPageViewModel>();
		builder.Services.AddTransient<ViewOrderStatusDetailsPage>();
		builder.Services.AddTransient<ViewModels.ViewOrderStatusDetailsPageViewModel>();
		builder.Services.AddTransient<InventoryMainPage>();
		builder.Services.AddTransient<ViewModels.InventoryMainPageViewModel>();
		builder.Services.AddTransient<ReportsPage>();
		builder.Services.AddTransient<ViewModels.ReportsPageViewModel>();
		builder.Services.AddTransient<ConfigurationPage>();
		builder.Services.AddTransient<ViewModels.ConfigurationPageViewModel>();
		builder.Services.AddTransient<AcceptLoadPage>();
		builder.Services.AddTransient<ViewModels.AcceptLoadPageViewModel>();
		builder.Services.AddTransient<AcceptLoadEditDeliveryPage>();
		builder.Services.AddTransient<ViewModels.AcceptLoadEditDeliveryPageViewModel>();
		builder.Services.AddTransient<ManageRoutePage>();
		builder.Services.AddTransient<ViewModels.ManageRoutePageViewModel>();
		builder.Services.AddTransient<SelectGoalPage>();
		builder.Services.AddTransient<ViewModels.SelectGoalPageViewModel>();
		builder.Services.AddTransient<GoalFilterPage>();
		builder.Services.AddTransient<EndOfDayPage>();
		builder.Services.AddTransient<ViewModels.EndOfDayPageViewModel>();
		
		// Inventory subpages
		builder.Services.AddTransient<ViewPrintInventoryPage>();
		builder.Services.AddTransient<ViewModels.ViewPrintInventoryPageViewModel>();
		builder.Services.AddTransient<CheckInventoryPage>();
		builder.Services.AddTransient<ViewModels.CheckInventoryPageViewModel>();
		builder.Services.AddTransient<TransferOnOffPage>();
		builder.Services.AddTransient<ViewModels.TransferOnOffPageViewModel>();
		builder.Services.AddTransient<ViewLoadOrderPage>();
		builder.Services.AddTransient<ViewModels.ViewLoadOrderPageViewModel>();
		builder.Services.AddTransient<NewLoadOrderTemplatePage>();
		builder.Services.AddTransient<ViewModels.NewLoadOrderTemplatePageViewModel>();
		builder.Services.AddTransient<SetParLevelPage>();
		builder.Services.AddTransient<ViewModels.SetParLevelPageViewModel>();
		builder.Services.AddTransient<CycleCountPage>();
		builder.Services.AddTransient<ViewModels.CycleCountPageViewModel>();
		builder.Services.AddTransient<InventorySummaryPage>();
		builder.Services.AddTransient<ViewModels.InventorySummaryPageViewModel>();
		
		// Report pages
		builder.Services.AddTransient<SalesReportPage>();
		builder.Services.AddTransient<ViewModels.SalesReportPageViewModel>();
		builder.Services.AddTransient<PaymentsReportPage>();
		builder.Services.AddTransient<ViewModels.PaymentsReportPageViewModel>();
		builder.Services.AddTransient<CommissionReportPage>();
		builder.Services.AddTransient<ViewModels.CommissionReportPageViewModel>();
		builder.Services.AddTransient<SalesmenCommissionReportPage>();
		builder.Services.AddTransient<ViewModels.SalesmenCommissionReportPageViewModel>();
		builder.Services.AddTransient<QtyProdSalesReportPage>();
		builder.Services.AddTransient<ViewModels.QtyProdSalesReportPageViewModel>();
		builder.Services.AddTransient<SalesProductCatReportPage>();
		builder.Services.AddTransient<ViewModels.SalesProductCatReportPageViewModel>();
		builder.Services.AddTransient<TransmissionReportPage>();
		builder.Services.AddTransient<ViewModels.TransmissionReportPageViewModel>();
		builder.Services.AddTransient<LoadOrderReportPage>();
		builder.Services.AddTransient<ViewModels.LoadOrderReportPageViewModel>();
		builder.Services.AddTransient<SAPOrderReportPage>();
		builder.Services.AddTransient<ViewModels.SAPOrderReportPageViewModel>();
		
		// Selection pages
		builder.Services.AddTransient<SelectPriceLevelPage>();
		builder.Services.AddTransient<ViewModels.SelectPriceLevelPageViewModel>();
		builder.Services.AddTransient<SelectRetailPriceLevelPage>();
		builder.Services.AddTransient<ViewModels.SelectRetailPriceLevelPageViewModel>();
		builder.Services.AddTransient<SelectTermsPage>();
		builder.Services.AddTransient<ViewModels.SelectTermsPageViewModel>();
		
		// End of day pages
		builder.Services.AddTransient<RouteReturnsPage>();
		builder.Services.AddTransient<ViewModels.RouteReturnsPageViewModel>();
		builder.Services.AddTransient<EndInventoryPage>();
		builder.Services.AddTransient<ViewModels.EndInventoryPageViewModel>();
		builder.Services.AddTransient<PrintReportsPage>();
		builder.Services.AddTransient<ViewModels.PrintReportsPageViewModel>();
		builder.Services.AddTransient<EndOfDayProcessPage>();
		builder.Services.AddTransient<ViewModels.EndOfDayProcessPageViewModel>();
		
		// Route pages
		builder.Services.AddTransient<RouteExpensesPage>();
		builder.Services.AddTransient<ViewModels.RouteExpensesPageViewModel>();
		builder.Services.AddTransient<RouteMapPage>();
		builder.Services.AddTransient<ViewModels.RouteMapPageViewModel>();
		builder.Services.AddTransient<AddOrdersToRoutePage>();
		builder.Services.AddTransient<ViewModels.AddOrdersToRoutePageViewModel>();
		builder.Services.AddTransient<AddPOSToRoutePage>();
		builder.Services.AddTransient<ViewModels.AddPOSToRoutePageViewModel>();
		
		// Goal pages
		builder.Services.AddTransient<GoalDetailsPage>();
		builder.Services.AddTransient<ViewModels.GoalDetailsPageViewModel>();
		
		// Setup pages
		builder.Services.AddTransient<SetupPrinterPage>();
		builder.Services.AddTransient<ViewModels.SetupPrinterPageViewModel>();
		builder.Services.AddTransient<SetupScannerPage>();
		builder.Services.AddTransient<ViewModels.SetupScannerPageViewModel>();
		
		// Client pages
		builder.Services.AddTransient<AddClientBillToPage>();
		builder.Services.AddTransient<ViewModels.AddClientBillToPageViewModel>();
		
		// Log viewer page
		builder.Services.AddTransient<LogViewerPage>();
		builder.Services.AddTransient<ViewModels.LogViewerPageViewModel>();
		
		// Self Service pages
		builder.Services.AddTransient<Views.SelfService.SelfServiceSelectCompanyPage>();
		builder.Services.AddTransient<ViewModels.SelfService.SelfServiceSelectCompanyPageViewModel>();
		builder.Services.AddTransient<Views.SelfService.SelfServiceClientListPage>();
		builder.Services.AddTransient<ViewModels.SelfService.SelfServiceClientListPageViewModel>();
		builder.Services.AddTransient<Views.SelfService.SelfServiceCheckOutPage>();
		builder.Services.AddTransient<ViewModels.SelfService.SelfServiceCheckOutPageViewModel>();
		builder.Services.AddTransient<Views.SelfService.SelfServiceTemplatePage>();
		builder.Services.AddTransient<ViewModels.SelfService.SelfServiceTemplatePageViewModel>();
		builder.Services.AddTransient<Views.SelfService.SelfServiceCatalogPage>();
		builder.Services.AddTransient<ViewModels.SelfService.SelfServiceCatalogPageViewModel>();
		builder.Services.AddTransient<Views.SelfService.SelfServiceCategoriesPage>();
		builder.Services.AddTransient<ViewModels.SelfService.SelfServiceCategoriesPageViewModel>();
		builder.Services.AddTransient<Views.SelfService.SelfServiceCollectPaymentPage>();
		builder.Services.AddTransient<ViewModels.SelfService.SelfServiceCollectPaymentPageViewModel>();
		builder.Services.AddTransient<Views.SelfService.SelfServiceCreditTemplatePage>();
		builder.Services.AddTransient<ViewModels.SelfService.SelfServiceCreditTemplatePageViewModel>();
		builder.Services.AddTransient<Views.SelfService.SelfServiceOffersPage>();
		builder.Services.AddTransient<ViewModels.SelfService.SelfServiceOffersPageViewModel>();
		builder.Services.AddTransient<SortByDialogPage>();
		builder.Services.AddTransient<ViewModels.SortByDialogViewModel>();

            return builder.Build();
        }
    }
}
