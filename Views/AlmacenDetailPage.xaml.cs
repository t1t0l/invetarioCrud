using InvetarioCrud.ViewModels; // Asegúrate de tener este using

namespace InvetarioCrud.Views;

public partial class AlmacenDetailPage : ContentPage
{
    public AlmacenDetailPage(AlmacenDetailViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        if (BindingContext is AlmacenDetailViewModel vm)
        {
            // Si se está cargando un almacén existente (AlmacenIdString tiene un valor > 0)
            // o si se está refrescando después de una operación.
            if (!string.IsNullOrEmpty(vm.AlmacenIdString) && int.TryParse(vm.AlmacenIdString, out int id) && id > 0)
            {
                if (!vm.IsNewAlmacen) // Solo si no es un almacén conceptualmente "nuevo" que aún no se guarda
                {
                    await vm.LoadAlmacenDetailsAsync(id); // Carga o recarga los detalles completos
                }
            }
            else if (vm.CurrentAlmacen != null && vm.CurrentAlmacen.ID > 0 && !vm.IsNewAlmacen)
            {
                // Si ya hay un almacén cargado (no nuevo) y volvemos a la página, refrescar.
                await vm.RefreshDataAsync();
            }
            // Si es un almacén completamente nuevo (ID=0), vm.LoadAlmacenDetailsAsync(0) lo manejará.
            // O la lógica en el constructor del VM ya lo preparó.
        }
    }
}