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
    public partial class MovimientosViewModel : ObservableObject
    {
        private readonly DatabaseService _databaseService;

        [ObservableProperty]
        ObservableCollection<Producto> listaProductos;
        [ObservableProperty]
        ObservableCollection<Almacen> listaAlmacenes;
        [ObservableProperty]
        ObservableCollection<Movimiento> historialMovimientos;

        [ObservableProperty]
        Producto selectedProducto;
        [ObservableProperty]
        Almacen selectedAlmacenOrigen;
        [ObservableProperty]
        Almacen selectedAlmacenDestino;

        [ObservableProperty]
        int cantidadMovimiento;
        [ObservableProperty]
        DateTime fechaVencimientoIngreso;

        [ObservableProperty]
        bool isIngresoMode = true;
        [ObservableProperty]
        bool isTrasladoMode = false;


        [ObservableProperty]
        bool isBusy;
        [ObservableProperty]
        string busyText;

        public MovimientosViewModel(DatabaseService databaseService)
        {
            _databaseService = databaseService;
            ListaProductos = new ObservableCollection<Producto>();
            ListaAlmacenes = new ObservableCollection<Almacen>();
            HistorialMovimientos = new ObservableCollection<Movimiento>();
            FechaVencimientoIngreso = DateTime.Today.AddMonths(6);
            CantidadMovimiento = 1;
        }

        partial void OnIsIngresoModeChanged(bool value)
        {
            IsTrasladoMode = !value;
            if (value) SelectedAlmacenOrigen = null;
        }
        partial void OnIsTrasladoModeChanged(bool value)
        {
            IsIngresoMode = !value;
        }


        [RelayCommand]
        async Task LoadInitialDataAsync()
        {
            if (IsBusy) return;
            IsBusy = true;
            BusyText = "Cargando datos...";
            try
            {
                var prods = await _databaseService.GetProductosAsync();
                ListaProductos.Clear();
                foreach (var p in prods) ListaProductos.Add(p);

                var alms = await _databaseService.GetAlmacenesAsync();
                ListaAlmacenes.Clear();
                foreach (var a in alms) ListaAlmacenes.Add(a);

                await LoadHistorialMovimientosAsync();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error cargando datos iniciales para movimientos: {ex.Message}");
                await Shell.Current.DisplayAlert("Error", "No se pudieron cargar los datos necesarios.", "OK");
            }
            finally
            {
                IsBusy = false;
                BusyText = "";
            }
        }

        async Task LoadHistorialMovimientosAsync()
        {
           
            var movs = await _databaseService.GetMovimientosAsync();
            HistorialMovimientos.Clear();
            foreach (var m in movs) HistorialMovimientos.Add(m);
        }

        [RelayCommand]
        async Task EjecutarMovimientoAsync()
        {
           
            if (SelectedProducto == null)
            {
                await Shell.Current.DisplayAlert("Validación", "Seleccione un producto.", "OK");
                return;
            }
            if (CantidadMovimiento <= 0)
            {
                await Shell.Current.DisplayAlert("Validación", "La cantidad debe ser mayor a cero.", "OK");
                return;
            }
            if (SelectedAlmacenDestino == null)
            {
                await Shell.Current.DisplayAlert("Validación", "Seleccione un almacén de destino.", "OK");
                return;
            }


            if (IsTrasladoMode) 
            {
                if (SelectedAlmacenOrigen == null)
                {
                    await Shell.Current.DisplayAlert("Validación", "Seleccione un almacén de origen para el traslado.", "OK");
                    return;
                }

               
                if (SelectedAlmacenOrigen.ID == SelectedAlmacenDestino.ID)
                {
                    await Shell.Current.DisplayAlert("Validación", "El almacén de origen y destino no pueden ser el mismo para un traslado.", "OK");
                    return;
                }
            }


            IsBusy = true;
            BusyText = "Procesando movimiento...";
            try
            {
                if (IsIngresoMode)
                {
                    await _databaseService.IngresarProductoAsync(
                        SelectedProducto.ID,
                        SelectedAlmacenDestino.ID,
                        CantidadMovimiento,
                        FechaVencimientoIngreso);
                    await Shell.Current.DisplayAlert("Éxito", "Ingreso de producto realizado.", "OK");
                }
                else 
                {
                    
                    bool success = await _databaseService.MoverProductoAsync(
                        SelectedProducto.ID,
                        SelectedAlmacenOrigen.ID, 
                        SelectedAlmacenDestino.ID,
                        CantidadMovimiento);

                   
                    await Shell.Current.DisplayAlert("Éxito", "Traslado de producto realizado.", "OK");
                }

                
                SelectedProducto = null;
                SelectedAlmacenOrigen = null;
                SelectedAlmacenDestino = null;
                CantidadMovimiento = 1;
                FechaVencimientoIngreso = DateTime.Today.AddMonths(6);
                await LoadHistorialMovimientosAsync();
            }
            catch (Exception ex) 
            {
                Debug.WriteLine($"Error ejecutando movimiento: {ex.Message}");
                await Shell.Current.DisplayAlert("Error en Movimiento", $"No se pudo completar el movimiento: {ex.Message}", "OK");
            }
            finally
            {
                IsBusy = false;
                BusyText = "";
            }
        }
    }
}