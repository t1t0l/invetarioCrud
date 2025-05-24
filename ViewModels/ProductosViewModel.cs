using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using InvetarioCrud.Models;
using InvetarioCrud.Services;
using InvetarioCrud.Views;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InvetarioCrud.ViewModels
{
    public partial class ProductosViewModel : ObservableObject
    {
        private readonly DatabaseService _databaseService;
        [ObservableProperty]
        ObservableCollection<Producto> productos;

        [ObservableProperty]
        bool isBusy;

       
        public ProductosViewModel(DatabaseService databaseService)
        {
            _databaseService = databaseService;
            Productos = new ObservableCollection<Producto>();
            LoadProductosCommand = new AsyncRelayCommand(LoadProductosAsync);
            GoToDetailCommand = new AsyncRelayCommand<Producto>(GoToDetailAsync);
            AddNewProductoCommand = new AsyncRelayCommand(AddNewProductoAsync);
        }

        public IAsyncRelayCommand LoadProductosCommand { get; }
        public IAsyncRelayCommand<Producto> GoToDetailCommand { get; }
        public IAsyncRelayCommand AddNewProductoCommand { get; }

        public async Task LoadProductosAsync()
        {
            if (IsBusy) return;
            IsBusy = true;
            try
            {
                var prods = await _databaseService.GetProductosAsync();
                if (Productos == null) Productos = new ObservableCollection<Producto>();
                Productos.Clear();
                foreach (var prod in prods)
                {
                    Productos.Add(prod);
                }
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("Error", $"No se pudieron cargar los productos: {ex.Message}", "OK");
            }
            finally
            {
                IsBusy = false;
            }
        }

        private async Task GoToDetailAsync(Producto producto)
        {
            if (producto == null) return;
           
            await Shell.Current.GoToAsync($"{nameof(ProductoDetailPage)}?id={producto.ID}");
        }

        private async Task AddNewProductoAsync()
        {
           
            await Shell.Current.GoToAsync(nameof(ProductoDetailPage));
        }
    }
}