using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;
using B_TrainGoIotCore.Models;

namespace B_TrainGoIotCore.Views.Converter
{
    class ControlStateVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            var state = (EControlState) value;

            var targetStateStr = parameter as string;

            EControlState targetState = EControlState.None;


            if (!Enum.TryParse(targetStateStr, out targetState)) return Visibility.Collapsed;

            return targetState == state ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            return DependencyProperty.UnsetValue;
        }
    }
}
