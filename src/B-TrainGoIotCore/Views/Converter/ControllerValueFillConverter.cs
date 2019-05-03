using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Media;

namespace B_TrainGoIotCore.Views.Converter
{
    class ControllerValueFillConverter : IValueConverter
    {
        public static readonly SolidColorBrush DisableBrush = new SolidColorBrush(Colors.LightGray);
        public static readonly SolidColorBrush EnableBrakeBrush = new SolidColorBrush(Colors.Red);
        public static readonly SolidColorBrush EnableAccelBrush = new SolidColorBrush(Colors.GreenYellow);

        public object Convert(object value, Type targetType, object parameter, string language)
        {
            var v = (int) value;
            var paramStr = (string) parameter;

            if (!int.TryParse(paramStr, out var paramValue)) return DisableBrush;

            if (v == 0 || paramValue == 0) return DisableBrush;

            if ((v < 0 && paramValue > 0) || (v > 0 && paramValue < 0)) return DisableBrush;

            if (v == -9)
            {
                return paramValue == -9 ? EnableBrakeBrush : DisableBrush;
            }

            if (v < 0)
            {
                return paramValue < v ? DisableBrush : EnableBrakeBrush;
            }

            return paramValue > v ? DisableBrush : EnableAccelBrush;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            return DependencyProperty.UnsetValue;
        }
    }
}
