using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using InvetarioCrud.Models;
using InvetarioCrud.Services;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Diagnostics;
using System;
using System.Linq;

namespace InvetarioCrud.ViewModels
{
    [QueryProperty(nameof(AlmacenIdString), "almacenId")]
    [QueryProperty(nameof(AlmacenProductoIdString), "almacenProductoId")]
    public partial class AlmacenProductoFormViewModel : ObservableObject
    {
        private readonly DatabaseService _databaseService;

        [ObservableProperty]
        ObservableCollection<Producto> todosLosProductos; 

        [ObservableProperty]
        Producto selectedProducto;

        [ObservableProperty]
        int cantidad;

        [ObservableProperty]
        DateTime fechaVencimiento;

        [ObservableProperty]
        string pageTitle;

        [ObservableProperty]
        bool isEditMode;

        [ObservableProperty]
        bool isBusy;

        private AlmacenProducto _currentAlmacenProducto; 

        private int _almacenId;
        public string AlmacenIdString
        {
            get => _almacenId.ToString();
            set
            {
                if (int.TryParse(value, out int id))
                    _almacenId = id;
            }
        }

        private int _almacenProductoId; 
        public string AlmacenProductoIdString
        {
            get => _almacenProductoId.ToString();
            set
            {
                if (int.TryParse(value, out int id) && id > 0)
                {
                    _almacenProductoId = id;
                    IsEditMode = true;
                    LoadAlmacenProductoCommand.ExecuteAsync(id);
                }
                else
                {
                    IsEditMode = false;
                    PageTitle = "Añadir Producto al Almacén";
                    
                    _currentAlmacenProducto = new AlmacenProducto();
                    FechaVencimiento = DateTime.Today.AddMonths(6); 
                    Cantidad = 1;
                    SelectedProducto = null;
                }
            }
        }


        public AlmacenProductoFormViewModel(DatabaseService databaseService)
        {
            _databaseService = databaseService;
            TodosLosProductos = new ObservableCollection<Producto>();
            FechaVencimiento = DateTime.Today.AddMonths(6); 
            _currentAlmacenProducto = new AlmacenProducto();
            LoadProductosDisponiblesCommand = new AsyncRelayCommand(LoadProductosDisponiblesAsync);
        }

        public IAsyncRelayCommand LoadProductosDisponiblesCommand { get; }


        async Task LoadProductosDisponiblesAsync()
        {
            if (IsBusy) return;
            IsBusy = true;
            try
            {
                var prods = await _databaseService.GetProductosAsync();
                TodosLosProductos.Clear();
                foreach (var p in prods)
                {
                    TodosLosProductos.Add(p);
                }

                
                if (IsEditMode && _currentAlmacenProducto != null && _currentAlmacenProducto.IDProducto > 0)
                {
                    SelectedProducto = TodosLosProductos.FirstOrDefault(p => p.ID == _currentAlmacenProducto.IDProducto);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error cargando productos disponibles: {ex.Message}");
                await Shell.Current.DisplayAlert("Error", "No se pudieron cargar los productos.", "OK");
            }
            finally
            {
                IsBusy = false;
            }
        }

        [RelayCommand]
        async Task LoadAlmacenProductoAsync(int almacenProductoId)
        {
            if (IsBusy) return;
            IsBusy = true;
            try
            {
                _currentAlmacenProducto = await _databaseService.GetAlmacenProductoByIdAsync(almacenProductoId);
                if (_currentAlmacenProducto != null)
                {
                    Cantidad = _currentAlmacenProducto.Cantidad;
                    FechaVencimiento = _currentAlmacenProducto.FechaVencimiento;
                   
                    PageTitle = "Editar Producto en Almacén";
                }
                else
                {
                    await Shell.Current.DisplayAlert("Error", "Item de inventario no encontrado.", "OK");
                    await Shell.Current.GoToAsync("..");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error cargando item de inventario: {ex.Message}");
                await Shell.Current.DisplayAlert("Error", "No se pudo cargar el item de inventario.", "OK");
            }
            finally
            {
                IsBusy = false;
            }
        }

        [RelayCommand]
        async Task SaveAlmacenProductoAsync()
        {
            if (SelectedProducto == null)
            {
                await Shell.Current.DisplayAlert("Validación", "Debe seleccionar un producto.", "OK");
                return;
            }
            if (Cantidad <= 0)
            {
                await Shell.Current.DisplayAlert("Validación", "La cantidad debe ser mayor que cero.", "OK");
                return;
            }
            if (_almacenId == 0)
            {
                await Shell.Current.DisplayAlert("Error", "ID de almacén no válido.", "OK");
                return;
            }


            IsBusy = true;
            try
            {
                _currentAlmacenProducto.IDAlmacen = _almacenId;
                _currentAlmacenProducto.IDProducto = SelectedProducto.ID;
                _currentAlmacenProducto.Cantidad = Cantidad;
                _currentAlmacenProducto.FechaVencimiento = FechaVencimiento;

                
                await _databaseService.AddOrUpdateProductoEnAlmacenAsync(_currentAlmacenProducto);

                await Shell.Current.DisplayAlert("Éxito", "Producto guardado en el almacén.", "OK");
                await Shell.Current.GoToAsync(".."); 
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error guardando producto en almacén: {ex.Message}");
                await Shell.Current.DisplayAlert("Error", "No se pudo guardar el producto en el almacén.", "OK");
            }
            finally
            {
                IsBusy = false;
            }
        }
    }
}