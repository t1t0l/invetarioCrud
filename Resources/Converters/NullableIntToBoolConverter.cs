using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Globalization;


namespace InvetarioCrud.Resources.Converters
{
    public class NullableIntToBoolConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
           
            int? nullableInt = value as int?;

            if (nullableInt.HasValue)
            {
               
                return nullableInt.Value > 0;
            }

           
            return false;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
          
            throw new NotImplementedException("ConvertBack no implementado para NullableIntToBoolConverter.");
        }
    }
}
