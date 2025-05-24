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
    public enum TipoReporte
    {
        StockTotalConsolidado,
        StockPorAlmacen,
        HistorialMovimientosProducto
    }

    public partial class ReportesViewModel : ObservableObject
    {
        private readonly DatabaseService _databaseService;

        [ObservableProperty]
        ObservableCollection<TipoReporte> tiposReporte;

        [ObservableProperty]
        TipoReporte selectedTipoReporte;

        [ObservableProperty]
        ObservableCollection<Almacen> listaAlmacenes;

        [ObservableProperty]
        Almacen selectedAlmacenReporte;

        [ObservableProperty]
        ObservableCollection<Producto> listaProductos;

        [ObservableProperty]
        Producto selectedProductoReporte;

        [ObservableProperty]
        ObservableCollection<object> resultadosReporte;

        [ObservableProperty]
        bool isBusy;

        [ObservableProperty]
        string busyText;

        [ObservableProperty]
        bool showAlmacenPicker;

        [ObservableProperty]
        bool showProductoPicker;

        public ReportesViewModel(DatabaseService databaseService)
        {
            _databaseService = databaseService;
            TiposReporte = new ObservableCollection<TipoReporte>(Enum.GetValues(typeof(TipoReporte)).Cast<TipoReporte>());
            ListaAlmacenes = new ObservableCollection<Almacen>();
            ListaProductos = new ObservableCollection<Producto>();
            ResultadosReporte = new ObservableCollection<object>();
            SelectedTipoReporte = TipoReporte.StockTotalConsolidado; // Default
        }

        partial void OnSelectedTipoReporteChanged(TipoReporte value)
        {
            ShowAlmacenPicker = value == TipoReporte.StockPorAlmacen;
            ShowProductoPicker = value == TipoReporte.HistorialMovimientosProducto;
            ResultadosReporte.Clear();
            SelectedAlmacenReporte = null;
            SelectedProductoReporte = null;
        }

        [RelayCommand]
        async Task LoadPickersDataAsync()
        {
            if (IsBusy) return;
            IsBusy = true;
            BusyText = "Cargando opciones...";
            try
            {
                var alms = await _databaseService.GetAlmacenesAsync();
                ListaAlmacenes.Clear();
                if (alms != null) foreach (var a in alms) ListaAlmacenes.Add(a);

                var prods = await _databaseService.GetProductosAsync();
                ListaProductos.Clear();
                if (prods != null) foreach (var p in prods) ListaProductos.Add(p);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error cargando datos para pickers de reporte: {ex.Message}");
                await Shell.Current.DisplayAlert("Error", "No se pudieron cargar las opciones.", "OK");
            }
            finally
            {
                IsBusy = false;
                BusyText = "";
            }
        }

        [RelayCommand]
        async Task GenerarReporteAsync()
        {
            if (IsBusy) return;

            if (SelectedTipoReporte == TipoReporte.StockPorAlmacen && SelectedAlmacenReporte == null)
            {
                await Shell.Current.DisplayAlert("Validación", "Seleccione un almacén para el reporte.", "OK");
                return;
            }
            if (SelectedTipoReporte == TipoReporte.HistorialMovimientosProducto && SelectedProductoReporte == null)
            {
                await Shell.Current.DisplayAlert("Validación", "Seleccione un producto para el reporte.", "OK");
                return;
            }

            IsBusy = true;
            BusyText = "Generando reporte...";
            ResultadosReporte.Clear(); 
            try
            {
                switch (SelectedTipoReporte)
                {
                    case TipoReporte.StockTotalConsolidado:
                        var stockTotal = await _databaseService.GetStockTotalConsolidadoAsync();
                        if (stockTotal != null) foreach (var item in stockTotal) ResultadosReporte.Add(item);
                        break;
                    case TipoReporte.StockPorAlmacen:
                        if (SelectedAlmacenReporte != null)
                        {
                            var stockAlmacen = await _databaseService.GetStockPorAlmacenAsync(SelectedAlmacenReporte.ID);
                            if (stockAlmacen != null) foreach (var item in stockAlmacen) ResultadosReporte.Add(item);
                        }
                        break;
                    case TipoReporte.HistorialMovimientosProducto:
                        if (SelectedProductoReporte != null)
                        {
                            var historial = await _databaseService.GetHistorialMovimientosProductoAsync(SelectedProductoReporte.ID);
                            if (historial != null) foreach (var item in historial) ResultadosReporte.Add(item);
                        }
                        break;
                }

                if (!ResultadosReporte.Any())
                {
                    await Shell.Current.DisplayAlert("Info", "No se encontraron datos para este reporte con los criterios seleccionados.", "OK");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error generando reporte: {ex.Message}");
                await Shell.Current.DisplayAlert("Error", $"No se pudo generar el reporte: {ex.Message}", "OK");
            }
            finally
            {
                IsBusy = false;
                BusyText = "";
            }
        }
    }
}