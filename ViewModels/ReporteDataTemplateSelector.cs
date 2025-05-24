using InvetarioCrud.Models;     
using Microsoft.Maui.Controls;

namespace InvetarioCrud.ViewModels 
{
    public class ReporteDataTemplateSelector : DataTemplateSelector
    {
        public DataTemplate StockConsolidadoTemplate { get; set; }
        public DataTemplate StockPorAlmacenTemplate { get; set; }
        public DataTemplate HistorialMovimientosTemplate { get; set; }
        public DataTemplate DefaultTemplate { get; set; }

        protected override DataTemplate OnSelectTemplate(object item, BindableObject container)
        {
            if (item == null)
            {
                System.Diagnostics.Debug.WriteLine("[ReporteDataTemplateSelector] Item es null.");
                return DefaultTemplate ?? null; // Usar default o null si el item es null
            }

            if (item is AlmacenProductoInfo api)
            {
                if (!string.IsNullOrEmpty(api.NombreAlmacen) && StockPorAlmacenTemplate != null)
                {
                    return StockPorAlmacenTemplate;
                }
                if (StockConsolidadoTemplate != null) 
                {
                    return StockConsolidadoTemplate;
                }
            }
            else if (item is Movimiento && HistorialMovimientosTemplate != null)
            {
                return HistorialMovimientosTemplate;
            }

            System.Diagnostics.Debug.WriteLine($"[ReporteDataTemplateSelector] No se encontró plantilla para el tipo: {item.GetType().FullName}. Usando DefaultTemplate si existe.");
            return DefaultTemplate ?? null; 
        }
    }
}