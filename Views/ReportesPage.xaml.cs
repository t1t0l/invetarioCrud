using InvetarioCrud.ViewModels;

namespace InvetarioCrud.Views 
{
    public partial class ReportesPage : ContentPage
    {
        private readonly ReportesViewModel _viewModel;

        public ReportesPage(ReportesViewModel viewModel)
        {
            InitializeComponent();
            _viewModel = viewModel;
            BindingContext = _viewModel;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();

            if (_viewModel != null && (_viewModel.ListaAlmacenes == null || !_viewModel.ListaAlmacenes.Any()))
            {
                await _viewModel.LoadPickersDataCommand.ExecuteAsync(null);
            }
        }
    }
}
