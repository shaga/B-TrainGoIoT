using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;

namespace B_TrainGoIotCore.Views.Converter
{
    class BooleanVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            var state = (bool)value;


            if (parameter == null) return state ? Visibility.Visible : Visibility.Collapsed;

            var paramStr = parameter as string;

            if (!bool.TryParse(paramStr, out var visibleState)) visibleState = true;

            return state == visibleState ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            return DependencyProperty.UnsetValue;
        }
    }
}
