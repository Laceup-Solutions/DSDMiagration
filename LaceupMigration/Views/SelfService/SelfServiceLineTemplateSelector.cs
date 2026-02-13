using Microsoft.Maui.Controls;
using LaceupMigration.ViewModels.SelfService;

namespace LaceupMigration.Views.SelfService
{
    /// <summary>Selects line item template: AdvancedCatalog-style when UseLaceupAdvancedCatalogKey=1, otherwise default style.</summary>
    public class SelfServiceLineTemplateSelector : DataTemplateSelector
    {
        public DataTemplate? AdvancedCatalogTemplate { get; set; }
        public DataTemplate? StandardTemplate { get; set; }

        protected override DataTemplate? OnSelectTemplate(object item, BindableObject container)
        {
            var viewModel = (container as CollectionView)?.BindingContext as SelfServiceCheckOutPageViewModel;
            if (viewModel != null && viewModel.UseAdvancedCatalogStyle)
                return AdvancedCatalogTemplate ?? StandardTemplate;
            return StandardTemplate;
        }
    }
}
