using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using InvetarioCrud.Models;
using InvetarioCrud.Services;

namespace InvetarioCrud.ViewModels
{
    [QueryProperty(nameof(ProductId), "id")] 
    public partial class ProductoDetailViewModel : ObservableObject
    {
        private readonly DatabaseService _databaseService;

        [ObservableProperty]
        Producto currentProducto;

        [ObservableProperty]
        string pageTitle;

        private int _productId;
        public int ProductId
        {
            get => _productId;
            set
            {
                SetProperty(ref _productId, value);
                if (value != 0)
                {
                    LoadProductoCommand.ExecuteAsync(value);
                }
                else 
                {
                    CurrentProducto = new Producto();
                    PageTitle = "Nuevo Producto";
                }
            }
        }

        public ProductoDetailViewModel(DatabaseService databaseService)
        {
            _databaseService = databaseService;
            CurrentProducto = new Producto(); 
            PageTitle = "Nuevo Producto";
        }

        [RelayCommand]
        async Task LoadProductoAsync(int id)
        {
            var producto = await _databaseService.GetProductoAsync(id);
            if (producto != null)
            {
                CurrentProducto = producto;
                PageTitle = $"Editar: {producto.NombreProducto}";
            }
        }

        [RelayCommand]
        async Task SaveProductoAsync()
        {
            if (string.IsNullOrWhiteSpace(CurrentProducto.NombreProducto))
            {
                await Shell.Current.DisplayAlert("Error", "El nombre del producto es obligatorio.", "OK");
                return;
            }
            await _databaseService.SaveProductoAsync(CurrentProducto);
            await Shell.Current.GoToAsync(".."); 
        }

        [RelayCommand]
        async Task DeleteProductoAsync()
        {
            if (CurrentProducto.ID == 0) return; 

            bool confirm = await Shell.Current.DisplayAlert("Confirmar", $"¿Eliminar {CurrentProducto.NombreProducto}?", "Sí", "No");
            if (confirm)
            {
              
                await _databaseService.DeleteProductoAsync(CurrentProducto);
                await Shell.Current.GoToAsync("..");
            }
        }
    }
}
