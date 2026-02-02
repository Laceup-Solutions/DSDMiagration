using Microsoft.Maui.Controls;
using LaceupMigration.ViewModels;

namespace LaceupMigration.Views
{
    public class LoadOrderTemplateSelector : DataTemplateSelector
    {
        public DataTemplate? AdvancedCatalogTemplate { get; set; }
        public DataTemplate? StandardTemplate { get; set; }
        
        public NewLoadOrderTemplatePageViewModel? ViewModel { get; set; }

        protected override DataTemplate? OnSelectTemplate(object item, BindableObject container)
        {
            // Use ViewModel property if set, otherwise try to get from container
            var viewModel = ViewModel;
            if (viewModel == null && container is CollectionView collectionView)
            {
                viewModel = collectionView.BindingContext as NewLoadOrderTemplatePageViewModel;
            }

            if (viewModel != null && viewModel.IsAdvancedCatalogTemplate)
            {
                return AdvancedCatalogTemplate ?? StandardTemplate;
            }

            // Fallback to standard template
            return StandardTemplate;
        }
    }
}

