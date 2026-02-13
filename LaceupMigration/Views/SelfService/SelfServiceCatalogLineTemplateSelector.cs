using Microsoft.Maui.Controls;
using LaceupMigration.ViewModels.SelfService;

namespace LaceupMigration.Views.SelfService
{
    /// <summary>Selects catalog line template: Advanced Catalog style (image) when UseLaceupAdvancedCatalogKey=1, otherwise ProductCatalog style.</summary>
    public class SelfServiceCatalogLineTemplateSelector : DataTemplateSelector
    {
        public DataTemplate? AdvancedCatalogTemplate { get; set; }
        public DataTemplate? StandardTemplate { get; set; }

        protected override DataTemplate? OnSelectTemplate(object item, BindableObject container)
        {
            var viewModel = (container as CollectionView)?.BindingContext as SelfServiceCatalogPageViewModel;
            if (viewModel != null && viewModel.UseAdvancedCatalogStyle)
                return AdvancedCatalogTemplate ?? StandardTemplate;
            return StandardTemplate;
        }
    }
}
