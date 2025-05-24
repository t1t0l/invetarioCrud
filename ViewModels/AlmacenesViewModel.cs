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
    public partial class AlmacenesViewModel : ObservableObject
    {
        private readonly DatabaseService _databaseService;

        [ObservableProperty]
        ObservableCollection<Almacen> almacenes;

        [ObservableProperty]
        bool isBusy;

        public AlmacenesViewModel(DatabaseService databaseService)
        {
            _databaseService = databaseService;
            Almacenes = new ObservableCollection<Almacen>();
        }

        [RelayCommand]
        async Task LoadAlmacenesAsync()
        {
            if (IsBusy) return;
            IsBusy = true;
            try
            {
                var alms = await _databaseService.GetAlmacenesAsync();
                Almacenes.Clear();
                foreach (var alm in alms)
                {
                    Almacenes.Add(alm);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error cargando almacenes: {ex.Message}");
                await Shell.Current.DisplayAlert("Error", "No se pudieron cargar los almacenes.", "OK");
            }
            finally
            {
                IsBusy = false;
            }
        }

        [RelayCommand]
        async Task GoToDetailAsync(Almacen almacen)
        {
            if (almacen == null) return;
            await Shell.Current.GoToAsync($"{nameof(AlmacenDetailPage)}?id={almacen.ID}");
        }

        [RelayCommand]
        async Task AddNewAlmacenAsync()
        {
            await Shell.Current.GoToAsync(nameof(AlmacenDetailPage));
        }
    }
}

