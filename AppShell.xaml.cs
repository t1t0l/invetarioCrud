using InvetarioCrud.Views;

namespace InvetarioCrud
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();

            //Registrar rutas para navegación con string
            Routing.RegisterRoute(nameof(ProductosPage), typeof(ProductosPage));
            Routing.RegisterRoute(nameof(ProductoDetailPage), typeof(ProductoDetailPage));
            Routing.RegisterRoute(nameof(AlmacenesPage), typeof(AlmacenesPage));
            Routing.RegisterRoute(nameof(AlmacenDetailPage), typeof(AlmacenDetailPage));
            Routing.RegisterRoute(nameof(AlmacenProductoFormPage), typeof(AlmacenProductoFormPage));
            Routing.RegisterRoute(nameof(MovimientosPage), typeof(MovimientosPage));
            Routing.RegisterRoute(nameof(ReportesPage), typeof(ReportesPage));
            // ... y así sucesivamente para todas las páginas que necesiten ser navegables por ruta.
        }
    }
}
