using InvetarioCrud.ViewModels;

namespace InvetarioCrud.Views;
public partial class AlmacenProductoFormPage : ContentPage
{
    private readonly AlmacenProductoFormViewModel _viewModel;
    public AlmacenProductoFormPage(AlmacenProductoFormViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = _viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _viewModel.LoadProductosDisponiblesCommand.ExecuteAsync(null);
       
    }
}
