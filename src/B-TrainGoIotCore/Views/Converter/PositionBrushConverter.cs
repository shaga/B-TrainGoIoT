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
    class PositionBrushConverter : IValueConverter
    {
        private static readonly Brush CurrentBrush = new SolidColorBrush(Colors.Red);

        private static readonly Brush NormalBrush = new SolidColorBrush(Colors.White);

        public object Convert(object value, Type targetType, object parameter, string language)
        {
            var pos = (int) value;
            var paramStr = parameter as string;

            if (!int.TryParse(paramStr, out int selfPos)) return NormalBrush;

            return pos == selfPos ? CurrentBrush : NormalBrush;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            return DependencyProperty.UnsetValue;
        }
    }
}
