using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Documents;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;

// The Templated Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234235

namespace B_TrainGoIotCore.Views
{
    public sealed class ManualPanel : Control
    {
        private static readonly Brush DefaultEmergencyBrush = new SolidColorBrush(Colors.Transparent);

        public static DependencyProperty EmergencyBrushProperty = DependencyProperty.Register(nameof(EmergencyBrush),
            typeof(Brush), typeof(ManualPanel), new PropertyMetadata(DefaultEmergencyBrush));

        public Brush EmergencyBrush
        {
            get => GetValue(EmergencyBrushProperty) as Brush;
            set => SetValue(EmergencyBrushProperty, value);
        }

        public static readonly DependencyProperty EmergencyMessageProperty =
            DependencyProperty.Register(nameof(EmergencyMessage), typeof(string), typeof(ManualPanel),
                new PropertyMetadata(""));

        public string EmergencyMessage
        {
            get => GetValue(EmergencyMessageProperty) as string;
            set => SetValue(EmergencyMessageProperty, value);
        }

        public static DependencyProperty SpeedProperty = DependencyProperty.Register(nameof(Speed), typeof(int),
            typeof(ManualPanel), new PropertyMetadata(0));

        public int Speed
        {
            get => (int) GetValue(SpeedProperty);
            set => SetValue(SpeedProperty, value);
        }

        public ManualPanel()
        {
            this.DefaultStyleKey = typeof(ManualPanel);
        }
    }
}
