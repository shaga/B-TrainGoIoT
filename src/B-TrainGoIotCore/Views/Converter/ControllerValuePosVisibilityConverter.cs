using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;

namespace B_TrainGoIotCore.Views.Converter
{
    class ControllerValuePosVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            var v = (int) value;

            var paramStr = (string) parameter;

            if (!int.TryParse(paramStr, out var paramValue)) return Visibility.Collapsed;

            return paramValue == v ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            return DependencyProperty.UnsetValue;
        }
    }
}
