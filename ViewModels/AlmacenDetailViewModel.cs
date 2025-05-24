using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using InvetarioCrud.Models;
using InvetarioCrud.Services;
using InvetarioCrud.Views;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Diagnostics;

namespace InvetarioCrud.ViewModels
{
    [QueryProperty(nameof(AlmacenIdString), "id")]
    public partial class AlmacenDetailViewModel : ObservableObject
    {
        private readonly DatabaseService _databaseService;

        [ObservableProperty]
        Almacen currentAlmacen;

        [ObservableProperty]
        ObservableCollection<AlmacenProductoInfo> productosEnAlmacen;

        [ObservableProperty]
        string pageTitle;

        [ObservableProperty]
        bool isBusy;

        [ObservableProperty]
        bool isNewAlmacen;

        private string _almacenIdString;
        public string AlmacenIdString
        {
            get => _almacenIdString;
            set
            {
                SetProperty(ref _almacenIdString, value);
                IsNewAlmacen = string.IsNullOrEmpty(value) || value == "0";
                if (int.TryParse(value, out int id) && id > 0)
                {
                    LoadAlmacenDetailsCommand.ExecuteAsync(id);
                }
                else
                {
                    CurrentAlmacen = new Almacen();
                    PageTitle = "Nuevo Almacén";
                    ProductosEnAlmacen.Clear();
                }
            }
        }

        public AlmacenDetailViewModel(DatabaseService databaseService)
        {
            _databaseService = databaseService;
            CurrentAlmacen = new Almacen();
            ProductosEnAlmacen = new ObservableCollection<AlmacenProductoInfo>();
            PageTitle = "Nuevo Almacén";
            IsNewAlmacen = true;
        }

        [RelayCommand]
        async Task LoadAlmacenDetailsAsync(int almacenId)
        {
            if (IsBusy) return;
            IsBusy = true;
            try
            {
                var almacen = await _databaseService.GetAlmacenAsync(almacenId);
                if (almacen != null)
                {
                    CurrentAlmacen = almacen;
                    PageTitle = $"Gestionar: {almacen.NombreAlmacen}";
                    IsNewAlmacen = false;
                    await LoadProductosEnAlmacenAsync(almacenId);
                }
                else
                {
                    await Shell.Current.DisplayAlert("Error", "Almacén no encontrado.", "OK");
                    await Shell.Current.GoToAsync("..");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error cargando detalles del almacén: {ex.Message}");
                await Shell.Current.DisplayAlert("Error", "No se pudieron cargar los detalles del almacén.", "OK");
            }
            finally
            {
                IsBusy = false;
            }
        }

        private async Task LoadProductosEnAlmacenAsync(int almacenId)
        {
           
            try
            {
                var productos = await _databaseService.GetProductosEnAlmacenAsync(almacenId);
                ProductosEnAlmacen.Clear();
                foreach (var prodInfo in productos)
                {
                    ProductosEnAlmacen.Add(prodInfo);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error cargando productos en almacén: {ex.Message}");
               
            }
        }

        public async Task RefreshProductosEnAlmacenAsync()
        {
            if (CurrentAlmacen != null && CurrentAlmacen.ID > 0)
            {
                await LoadProductosEnAlmacenAsync(CurrentAlmacen.ID);
            }
        }


        [RelayCommand]
        async Task SaveAlmacenAsync()
        {
            if (CurrentAlmacen == null || string.IsNullOrWhiteSpace(CurrentAlmacen.NombreAlmacen))
            {
                await Shell.Current.DisplayAlert("Validación", "El nombre del almacén es obligatorio.", "OK");
                return;
            }

            IsBusy = true;
            try
            {
                await _databaseService.SaveAlmacenAsync(CurrentAlmacen);
                if (IsNewAlmacen)
                {
                    IsNewAlmacen = false;
                    PageTitle = $"Gestionar: {CurrentAlmacen.NombreAlmacen}";
                    
                }
                await Shell.Current.DisplayAlert("Éxito", "Almacén guardado.", "OK");
                
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error guardando almacén: {ex.Message}");
                await Shell.Current.DisplayAlert("Error", "No se pudo guardar el almacén.", "OK");
            }
            finally
            {
                IsBusy = false;
            }
        }

        [RelayCommand]
        async Task DeleteAlmacenAsync()
        {
            if (CurrentAlmacen == null || CurrentAlmacen.ID == 0)
            {
                await Shell.Current.DisplayAlert("Info", "No se puede eliminar un almacén no guardado.", "OK");
                return;
            }

            bool confirm = await Shell.Current.DisplayAlert("Confirmar", $"¿Eliminar {CurrentAlmacen.NombreAlmacen}?", "Sí", "No");
            if (confirm)
            {
                IsBusy = true;
                try
                {
                   
                    await _databaseService.DeleteAlmacenAsync(CurrentAlmacen);
                    await Shell.Current.DisplayAlert("Éxito", "Almacén eliminado.", "OK"); 
                    await Shell.Current.GoToAsync("..");
                }
                catch (InvalidOperationException ex)
                {
                    Debug.WriteLine($"Error de validación al eliminar almacén: {ex.Message}");
                    await Shell.Current.DisplayAlert("Operación no permitida", ex.Message, "OK");
                }
                catch (Exception ex) 
                {
                    Debug.WriteLine($"Error eliminando almacén: {ex.Message}");
                    await Shell.Current.DisplayAlert("Error", "No se pudo eliminar el almacén.", "OK");
                }
                finally
                {
                    IsBusy = false;
                }
            }
        }

        [RelayCommand]
        async Task AddProductoToAlmacenAsync()
        {
            if (CurrentAlmacen == null || CurrentAlmacen.ID == 0)
            {
                await Shell.Current.DisplayAlert("Info", "Guarde el almacén antes de añadir productos.", "OK");
                return;
            }
            await Shell.Current.GoToAsync($"{nameof(AlmacenProductoFormPage)}?almacenId={CurrentAlmacen.ID}");
        }

        [RelayCommand]
        async Task EditProductoInAlmacenAsync(AlmacenProductoInfo productoInfo)
        {
            if (productoInfo == null || CurrentAlmacen == null || CurrentAlmacen.ID == 0) return;
           
            await Shell.Current.GoToAsync($"{nameof(AlmacenProductoFormPage)}?almacenId={CurrentAlmacen.ID}&almacenProductoId={productoInfo.ID}");
        }

        [RelayCommand]
        async Task DeleteProductoFromAlmacenAsync(AlmacenProductoInfo productoInfo)
        {
            if (productoInfo == null) return;

            bool confirm = await Shell.Current.DisplayAlert("Confirmar", $"¿Eliminar {productoInfo.NombreProducto} de este almacén?", "Sí", "No");
            if (confirm)
            {
                IsBusy = true;
                try
                {
                    await _databaseService.DeleteProductoDeAlmacenAsync(productoInfo.ID); 
                    await LoadProductosEnAlmacenAsync(CurrentAlmacen.ID); 
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Error eliminando producto del almacén: {ex.Message}");
                    await Shell.Current.DisplayAlert("Error", "No se pudo eliminar el producto del almacén.", "OK");
                }
                finally
                {
                    IsBusy = false;
                }
            }
        }
    }
}