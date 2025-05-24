namespace InvetarioCrud.Views;
using InvetarioCrud.ViewModels;

public partial class ProductosPage : ContentPage
{
    private readonly ProductosViewModel _viewModel;
    public ProductosPage(ProductosViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = _viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _viewModel.LoadProductosCommand.ExecuteAsync(null); 
    }
}