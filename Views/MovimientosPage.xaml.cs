using InvetarioCrud.ViewModels;

namespace InvetarioCrud.Views;

public partial class MovimientosPage : ContentPage
{
    private readonly MovimientosViewModel _viewModel;
    public MovimientosPage(MovimientosViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = _viewModel;
    }
    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _viewModel.LoadInitialDataCommand.ExecuteAsync(null);
    }
}
