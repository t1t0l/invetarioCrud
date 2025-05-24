using InvetarioCrud.Services;
using InvetarioCrud.ViewModels;
using InvetarioCrud.Views;
using Microsoft.Extensions.Logging;
using AlmacenProductoFormPage = InvetarioCrud.Views.AlmacenProductoFormPage;

namespace InvetarioCrud
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                });

#if DEBUG
    		builder.Logging.AddDebug();
#endif

            // Servicio de Base de Datos
            builder.Services.AddSingleton<DatabaseService>();

            
            // CRUD Productos
            builder.Services.AddTransient<ProductosViewModel>();
            builder.Services.AddTransient<ProductosPage>();
            builder.Services.AddTransient<ProductoDetailViewModel>();
            builder.Services.AddTransient<ProductoDetailPage>();

            // CRUD Almacenes
            builder.Services.AddTransient<AlmacenesViewModel>();
            builder.Services.AddTransient<AlmacenesPage>();
            builder.Services.AddTransient<AlmacenDetailViewModel>();
            builder.Services.AddTransient<AlmacenDetailPage>();

            // Inventario por Almacén
            builder.Services.AddTransient<AlmacenProductoFormViewModel>();
            builder.Services.AddTransient<AlmacenProductoFormPage>();

            // Movimientos
            builder.Services.AddTransient<MovimientosViewModel>();
            builder.Services.AddTransient<MovimientosPage>();

            // Reportes
            builder.Services.AddTransient<ReportesViewModel>();
            builder.Services.AddTransient<ReportesPage>();


            return builder.Build();
        }
    }
}
