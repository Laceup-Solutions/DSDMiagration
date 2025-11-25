using LaceupMigration.ViewModels;
using LaceupMigration.Views;

namespace LaceupMigration
{
    public partial class AppShell : Shell
    {
        private readonly MainPageViewModel _mainPageViewModel;

        public AppShell(MainPageViewModel mainPageViewModel)
        {
            InitializeComponent();

            _mainPageViewModel = mainPageViewModel;

            // [MIGRATION]: Update company name in header (matches Xamarin UpdateCompanyName)
            UpdateCompanyName();

            Routing.RegisterRoute("loginconfig", typeof(LoginConfigPage));
            Routing.RegisterRoute("termsandconditions", typeof(TermsAndConditionsPage));

            // Note: Tab routes (Clients, Invoices, Orders, Payments) are defined in AppShell.xaml
            // and should NOT be registered here to avoid ambiguous route errors

            // Client Details related routes
            Routing.RegisterRoute("clientdetails", typeof(ClientDetailsPage));
            Routing.RegisterRoute("selectinvoice", typeof(SelectInvoicePage));
            Routing.RegisterRoute("paymentselectclient", typeof(PaymentSelectClientPage));
            Routing.RegisterRoute("paymentsetvalues", typeof(PaymentSetValuesPage));
            Routing.RegisterRoute("invoicedetails", typeof(InvoiceDetailsPage));
            Routing.RegisterRoute("editclient", typeof(EditClientPage));

            // Order related routes
            Routing.RegisterRoute("batch", typeof(BatchPage));
            Routing.RegisterRoute("orderdetails", typeof(OrderDetailsPage));
            Routing.RegisterRoute("ordercredit", typeof(OrderCreditPage));
            Routing.RegisterRoute("superordertemplate", typeof(SuperOrderTemplatePage));
            Routing.RegisterRoute("previouslyorderedtemplate", typeof(PreviouslyOrderedTemplatePage));
            Routing.RegisterRoute("batchdepartment", typeof(BatchDepartmentPage));
            Routing.RegisterRoute("workorder", typeof(WorkOrderPage));
            Routing.RegisterRoute("consignment", typeof(ConsignmentPage));
            Routing.RegisterRoute("noservice", typeof(NoServicePage));

            // Supporting routes
            Routing.RegisterRoute("managedepartments", typeof(ManageDepartmentsPage));
            Routing.RegisterRoute("clientimages", typeof(ClientImagesPage));
            Routing.RegisterRoute("fullcategory", typeof(FullCategoryPage));
            Routing.RegisterRoute("additem", typeof(AddItemPage));
            Routing.RegisterRoute("advancedcatalog", typeof(AdvancedCatalogPage));
            Routing.RegisterRoute("productcatalog", typeof(ProductCatalogPage));
            Routing.RegisterRoute("productimage", typeof(ProductImagePage));
            Routing.RegisterRoute("productdetails", typeof(ProductDetailsPage));
            Routing.RegisterRoute("timesheet", typeof(TimeSheetPage));

            // Menu routes
            Routing.RegisterRoute("addclient", typeof(AddClientPage));
            Routing.RegisterRoute("sentorders", typeof(SentOrdersPage));
            Routing.RegisterRoute("sentordersorderslist", typeof(SentOrdersOrdersListPage));
            Routing.RegisterRoute("sentpayments", typeof(SentPaymentsPage));
            Routing.RegisterRoute("sentpaymentsinpackage", typeof(SentPaymentsInPackagePage));
            Routing.RegisterRoute("vieworderstatus", typeof(ViewOrderStatusPage));
            Routing.RegisterRoute("vieworderstatusdetails", typeof(ViewOrderStatusDetailsPage));
            Routing.RegisterRoute("inventory", typeof(InventoryMainPage));
            Routing.RegisterRoute("viewprintinventory", typeof(ViewPrintInventoryPage));
            Routing.RegisterRoute("checkinventory", typeof(CheckInventoryPage));
            Routing.RegisterRoute("transferonoff", typeof(TransferOnOffPage));
            Routing.RegisterRoute("viewloadorder", typeof(ViewLoadOrderPage));
            Routing.RegisterRoute("newloadordertemplate", typeof(NewLoadOrderTemplatePage));
            Routing.RegisterRoute("setparlevel", typeof(SetParLevelPage));
            Routing.RegisterRoute("cyclecount", typeof(CycleCountPage));
            Routing.RegisterRoute("inventorysummary", typeof(InventorySummaryPage));
            Routing.RegisterRoute("reports", typeof(ReportsPage));
            Routing.RegisterRoute("salesreport", typeof(SalesReportPage));
            Routing.RegisterRoute("paymentsreport", typeof(PaymentsReportPage));
            Routing.RegisterRoute("commissionreport", typeof(CommissionReportPage));
            Routing.RegisterRoute("salesmencommissionreport", typeof(SalesmenCommissionReportPage));
            Routing.RegisterRoute("qtyprodsalesreport", typeof(QtyProdSalesReportPage));
            Routing.RegisterRoute("salesproductcatreport", typeof(SalesProductCatReportPage));
            Routing.RegisterRoute("transmissionreport", typeof(TransmissionReportPage));
            Routing.RegisterRoute("loadorderreport", typeof(LoadOrderReportPage));
            Routing.RegisterRoute("saporderreport", typeof(SAPOrderReportPage));
            Routing.RegisterRoute("configuration", typeof(ConfigurationPage));
            Routing.RegisterRoute("acceptload", typeof(AcceptLoadPage));
            Routing.RegisterRoute("routemanagement", typeof(ManageRoutePage));
            Routing.RegisterRoute("goals", typeof(SelectGoalPage));
            Routing.RegisterRoute("endofday", typeof(EndOfDayPage));
            Routing.RegisterRoute("selectpricelevel", typeof(SelectPriceLevelPage));
            Routing.RegisterRoute("selectterms", typeof(SelectTermsPage));
            Routing.RegisterRoute("routereturns", typeof(RouteReturnsPage));
            Routing.RegisterRoute("endinventory", typeof(EndInventoryPage));
            Routing.RegisterRoute("printreports", typeof(PrintReportsPage));
            Routing.RegisterRoute("endofdayprocess", typeof(EndOfDayProcessPage));
            Routing.RegisterRoute("routeexpenses", typeof(RouteExpensesPage));
            Routing.RegisterRoute("routemap", typeof(RouteMapPage));
            Routing.RegisterRoute("addorderstoroute", typeof(AddOrdersToRoutePage));
            Routing.RegisterRoute("addpostoroute", typeof(AddPOSToRoutePage));
            Routing.RegisterRoute("goaldetails", typeof(GoalDetailsPage));
            Routing.RegisterRoute("setupprinter", typeof(SetupPrinterPage));
            Routing.RegisterRoute("setupscanner", typeof(SetupScannerPage));
            Routing.RegisterRoute("addclientbillto", typeof(AddClientBillToPage));

            // Self Service routes
            Routing.RegisterRoute("selfservice/selectcompany", typeof(Views.SelfService.SelfServiceSelectCompanyPage));
            Routing.RegisterRoute("selfservice/clientlist", typeof(Views.SelfService.SelfServiceClientListPage));
            Routing.RegisterRoute("selfservice/checkout", typeof(Views.SelfService.SelfServiceCheckOutPage));
            Routing.RegisterRoute("selfservice/template", typeof(Views.SelfService.SelfServiceTemplatePage));
            Routing.RegisterRoute("selfservice/catalog", typeof(Views.SelfService.SelfServiceCatalogPage));
            Routing.RegisterRoute("selfservice/categories", typeof(Views.SelfService.SelfServiceCategoriesPage));
            Routing.RegisterRoute("selfservice/collectpayment",
                typeof(Views.SelfService.SelfServiceCollectPaymentPage));
            Routing.RegisterRoute("selfservice/credittemplate",
                typeof(Views.SelfService.SelfServiceCreditTemplatePage));
        }

        // [MIGRATION]: Update company name in header (matches Xamarin MainActivity.UpdateCompanyName)
        // This method sets the Shell title to the company name, just like Xamarin sets Activity.Title
        public void UpdateCompanyName()
        {
            var company = CompanyInfo.GetMasterCompany();
            if (company != null && !string.IsNullOrEmpty(company.CompanyName))
                Title = company.CompanyName;
            else
                Title = "Laceup";
        }
    }
}