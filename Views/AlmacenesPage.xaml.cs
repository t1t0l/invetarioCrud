namespace InvetarioCrud.Views;
using InvetarioCrud.ViewModels;

public partial class AlmacenesPage : ContentPage
{
    private readonly AlmacenesViewModel _viewModel;
    public AlmacenesPage(AlmacenesViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = _viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        await _viewModel.LoadAlmacenesCommand.ExecuteAsync(null);
    }
}
