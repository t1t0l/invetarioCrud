using InvetarioCrud.ViewModels; // Aseg�rate de tener este using

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
            // Si se est� cargando un almac�n existente (AlmacenIdString tiene un valor > 0)
            // o si se est� refrescando despu�s de una operaci�n.
            if (!string.IsNullOrEmpty(vm.AlmacenIdString) && int.TryParse(vm.AlmacenIdString, out int id) && id > 0)
            {
                if (!vm.IsNewAlmacen) // Solo si no es un almac�n conceptualmente "nuevo" que a�n no se guarda
                {
                    await vm.LoadAlmacenDetailsAsync(id); // Carga o recarga los detalles completos
                }
            }
            else if (vm.CurrentAlmacen != null && vm.CurrentAlmacen.ID > 0 && !vm.IsNewAlmacen)
            {
                // Si ya hay un almac�n cargado (no nuevo) y volvemos a la p�gina, refrescar.
                await vm.RefreshDataAsync();
            }
            // Si es un almac�n completamente nuevo (ID=0), vm.LoadAlmacenDetailsAsync(0) lo manejar�.
            // O la l�gica en el constructor del VM ya lo prepar�.
        }
    }
}