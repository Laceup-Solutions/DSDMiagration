namespace LaceupMigration.Views.SelfService
{
    public partial class SelfServiceShell : Shell
    {
        public SelfServiceShell()
        {
            InitializeComponent();
            UpdateCompanyName();

            // Register routes (no shell tabs). Select Company = separate screen when adding credentials only.
            Routing.RegisterRoute("selfservice/selectcompany", typeof(SelfServiceSelectCompanyPage));
            Routing.RegisterRoute("selfservice/template", typeof(SelfServiceTemplatePage));
            Routing.RegisterRoute("selfservice/checkout", typeof(SelfServiceCheckOutPage));
            Routing.RegisterRoute("selfservice/catalog", typeof(SelfServiceCatalogPage));
            Routing.RegisterRoute("selfservice/categories", typeof(SelfServiceCategoriesPage));
            Routing.RegisterRoute("selfservice/collectpayment", typeof(SelfServiceCollectPaymentPage));
            Routing.RegisterRoute("selfservice/credittemplate", typeof(SelfServiceCreditTemplatePage));
            // Reuse main app catalog flow: Categories → ProductCatalog (and FullCategory if needed)
            Routing.RegisterRoute("fullcategory", typeof(LaceupMigration.Views.FullCategoryPage));
            Routing.RegisterRoute("productcatalog", typeof(LaceupMigration.Views.ProductCatalogPage));
            // More options: View Captured Images, PDF viewer (Send by Email)
            Routing.RegisterRoute("viewcapturedimages", typeof(LaceupMigration.Views.ViewCapturedImagesPage));
            Routing.RegisterRoute("pdfviewer", typeof(LaceupMigration.UtilDlls.PdfViewer));
        }

        /// <summary>
        /// Update shell title to company name (matches Xamarin SelfServiceClientListActivity.UpdateCompanyName).
        /// </summary>
        public void UpdateCompanyName()
        {
            var company = CompanyInfo.GetMasterCompany();
            if (company != null && !string.IsNullOrEmpty(company.CompanyName))
                Title = company.CompanyName;
            else
                Title = "Self Service";
        }
    }
}
