namespace InvetarioCrud.Views;
using InvetarioCrud.ViewModels;

public partial class ProductoDetailPage : ContentPage
{
    public ProductoDetailPage(ProductoDetailViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}