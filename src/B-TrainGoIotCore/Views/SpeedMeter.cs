using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Documents;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Shapes;

// The Templated Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234235

namespace B_TrainGoIotCore.Views
{
    [TemplatePart(Name = ContainerPartName, Type = typeof(Grid))]
    [TemplatePart(Name = ScalePartName, Type = typeof(Path))]
    [TemplatePart(Name = TrailPartName, Type = typeof(Path))]
    public sealed class SpeedMeter : Control
    {
        #region Constants

        // Template Parts.
        private const string ContainerPartName = "PART_Container";
        private const string ScalePartName = "PART_Scale";
        private const string TrailPartName = "PART_Trail";

        // For convenience.
        private const double Degrees2Radians = Math.PI / 180;

        // Candidate dependency properties.
        // Feel free to modify...
        private const double MinAngle = -150.0;
        private const double MaxAngle = 150.0;
        private const float ScalePadding = 23.0f;

        #endregion Constants

        #region Dependency Property Registrations

        public static readonly DependencyProperty MinimumProperty =
            DependencyProperty.Register("Minimum", typeof(double), typeof(SpeedMeter), new PropertyMetadata(0.0));

        public static readonly DependencyProperty MaximumProperty =
            DependencyProperty.Register("Maximum", typeof(double), typeof(SpeedMeter), new PropertyMetadata(100.0));

        public static readonly DependencyProperty ScaleWidthProperty =
            DependencyProperty.Register("ScaleWidth", typeof(Double), typeof(SpeedMeter), new PropertyMetadata(26.0));

        public static readonly DependencyProperty ValueProperty =
            DependencyProperty.Register("Value", typeof(double), typeof(SpeedMeter), new PropertyMetadata(0.0, OnValueChanged));

        public static readonly DependencyProperty UnitProperty =
            DependencyProperty.Register("Unit", typeof(string), typeof(SpeedMeter), new PropertyMetadata(string.Empty));

        public static readonly DependencyProperty ScaleBrushProperty =
            DependencyProperty.Register("ScaleBrush", typeof(Brush), typeof(SpeedMeter), new PropertyMetadata(new SolidColorBrush(Colors.DarkGray)));

        public static readonly DependencyProperty TrailBrushProperty =
            DependencyProperty.Register("TrailBrush", typeof(Brush), typeof(SpeedMeter), new PropertyMetadata(new SolidColorBrush(Colors.Orange)));

        public static readonly DependencyProperty UnitBrushProperty =
            DependencyProperty.Register("UnitBrush", typeof(Brush), typeof(SpeedMeter), new PropertyMetadata(new SolidColorBrush(Colors.White)));

        public static readonly DependencyProperty ValueAngleProperty =
            DependencyProperty.Register("ValueAngle", typeof(double), typeof(SpeedMeter), new PropertyMetadata(null));

        //public static readonly DependencyProperty SpeedLevelProperty =
        //    DependencyProperty.Register(nameof(SpeedLevel), typeof(ESpeedLevel), typeof(SpeedMeter), new PropertyMetadata(ESpeedLevel.Stop, OnSpeedLevelChanged));

        #endregion Dependency Property Registrations

        #region Constructors

        #endregion Constructors

        public SpeedMeter()
        {
            this.DefaultStyleKey = typeof(SpeedMeter);

            Minimum = 0;
            Maximum = 5;
        }

        #region Properties

        /// <summary>
        /// Gets or sets the minimum on the scale.
        /// </summary>
        public double Minimum
        {
            get { return (double)GetValue(MinimumProperty); }
            set { SetValue(MinimumProperty, value); }
        }

        /// <summary>
        /// Gets or sets the maximum on the scale.
        /// </summary>
        public double Maximum
        {
            get { return (double)GetValue(MaximumProperty); }
            set { SetValue(MaximumProperty, value); }
        }

        /// <summary>
        /// Gets or sets the width of the scale.
        /// </summary>
        public Double ScaleWidth
        {
            get { return (Double)GetValue(ScaleWidthProperty); }
            set { SetValue(ScaleWidthProperty, value); }
        }

        /// <summary>
        /// Gets or sets the current value.
        /// </summary>
        public double Value
        {
            get { return (double)GetValue(ValueProperty); }
            set { SetValue(ValueProperty, value); }
        }

        /// <summary>
        /// Gets or sets the unit measure.
        /// </summary>
        public string Unit
        {
            get { return (string)GetValue(UnitProperty); }
            set { SetValue(UnitProperty, value); }
        }

        /// <summary>
        /// Gets or sets the trail brush.
        /// </summary>
        public Brush TrailBrush
        {
            get { return (Brush)GetValue(TrailBrushProperty); }
            set { SetValue(TrailBrushProperty, value); }
        }

        /// <summary>
        /// Gets or sets the scale brush.
        /// </summary>
        public Brush ScaleBrush
        {
            get { return (Brush)GetValue(ScaleBrushProperty); }
            set { SetValue(ScaleBrushProperty, value); }
        }

        /// <summary>
        /// Gets or sets the unit brush.
        /// </summary>
        public Brush UnitBrush
        {
            get { return (Brush)GetValue(UnitBrushProperty); }
            set { SetValue(UnitBrushProperty, value); }
        }

        public double ValueAngle
        {
            get { return (double)GetValue(ValueAngleProperty); }
            set { SetValue(ValueAngleProperty, value); }
        }

        //public ESpeedLevel SpeedLevel
        //{
        //    get { return (ESpeedLevel)GetValue(SpeedLevelProperty); }
        //    set { SetValue(SpeedLevelProperty, value); }
        //}

        #endregion Properties

        protected override void OnApplyTemplate()
        {
            // Scale.
            var scale = this.GetTemplateChild(ScalePartName) as Path;
            if (scale != null)
            {
                var pg = new PathGeometry();
                var pf = new PathFigure();
                pf.IsClosed = false;
                var middleOfScale = 100 - ScalePadding - this.ScaleWidth / 2;
                pf.StartPoint = this.ScalePoint(MinAngle, middleOfScale);
                var seg = new ArcSegment();
                seg.SweepDirection = SweepDirection.Clockwise;
                seg.IsLargeArc = true;
                seg.Size = new Size(middleOfScale, middleOfScale);
                seg.Point = this.ScalePoint(MaxAngle, middleOfScale);
                pf.Segments.Add(seg);
                pg.Figures.Add(pf);
                scale.Data = pg;
            }

            OnValueChanged(this);
            base.OnApplyTemplate();
        }

        private static void OnValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            OnValueChanged(d);
        }

        //private static void OnSpeedLevelChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        //{
        //    var ctrl = obj as SpeedMeter;

        //    if (ctrl == null) return;

        //    switch (ctrl.SpeedLevel)
        //    {
        //        case ESpeedLevel.Lv1:
        //            ctrl.Value = 1;
        //            break;
        //        case ESpeedLevel.Lv2:
        //            ctrl.Value = 2;
        //            break;
        //        case ESpeedLevel.Lv3:
        //            ctrl.Value = 3;
        //            break;
        //        case ESpeedLevel.Lv4:
        //            ctrl.Value = 4;
        //            break;
        //        default:
        //            ctrl.Value = 0;
        //            break;
        //    }
        //}

        /// <summary>
        /// Updates the needle rotation, the trail, and the value text according to the new value.
        /// </summary>
        private static void OnValueChanged(DependencyObject d)
        {
            var c = d as SpeedMeter;
            if (!Double.IsNaN(c.Value))
            {
                var middleOfScale = 100 - ScalePadding - c.ScaleWidth / 2;
                c.ValueAngle = c.ValueToAngle(c.Value);

                // Trail
                var trail = c.GetTemplateChild(TrailPartName) as Path;
                if (trail != null)
                {
                    if (c.ValueAngle > MinAngle)
                    {
                        trail.Visibility = Visibility.Visible;
                        var pg = new PathGeometry();
                        var pf = new PathFigure();
                        pf.IsClosed = false;
                        pf.StartPoint = c.ScalePoint(MinAngle, middleOfScale);
                        var seg = new ArcSegment();
                        seg.SweepDirection = SweepDirection.Clockwise;
                        // We start from -150, so +30 becomes a large arc.
                        seg.IsLargeArc = c.ValueAngle > (180 + MinAngle);
                        seg.Size = new Size(middleOfScale, middleOfScale);
                        seg.Point = c.ScalePoint(Math.Min(c.ValueAngle, MaxAngle), middleOfScale);  // On overflow, stop trail at MaxAngle.
                        pf.Segments.Add(seg);
                        pg.Figures.Add(pf);
                        trail.Data = pg;
                    }
                    else
                    {
                        trail.Visibility = Visibility.Collapsed;
                    }
                }
            }
        }

        /// <summary>
        /// Transforms a set of polar coordinates into a Windows Point.
        /// </summary>
        private Point ScalePoint(double angle, double middleOfScale)
        {
            return new Point(100 + Math.Sin(Degrees2Radians * angle) * middleOfScale, 100 - Math.Cos(Degrees2Radians * angle) * middleOfScale);
        }

        /// <summary>
        /// Returns the angle for a specific value.
        /// </summary>
        /// <returns>In degrees.</returns>
        private double ValueToAngle(double value)
        {
            // Off-scale on the left.
            if (value < this.Minimum)
            {
                return MinAngle - 7.5;
            }

            // Off-scale on the right.
            if (value > this.Maximum)
            {
                return MaxAngle + 7.5;
            }

            return (value - this.Minimum) / (this.Maximum - this.Minimum) * (MaxAngle - MinAngle) + MinAngle;
        }
    }
}
