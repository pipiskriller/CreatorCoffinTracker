using System.Windows;
using System.Windows.Media;
using System.Globalization;
using System.Windows.Documents;

namespace CreatorCoffinTracker
{
    public class OutlinedTextBlock : FrameworkElement
    {
        public static readonly DependencyProperty TextProperty =
            DependencyProperty.Register(nameof(Text), typeof(string), typeof(OutlinedTextBlock),
                new FrameworkPropertyMetadata("Text", FrameworkPropertyMetadataOptions.AffectsRender));

        public string Text { get => (string)GetValue(TextProperty); set => SetValue(TextProperty, value); }

        public static readonly DependencyProperty FontSizeProperty =
            TextElement.FontSizeProperty.AddOwner(typeof(OutlinedTextBlock),
                new FrameworkPropertyMetadata(36.0, FrameworkPropertyMetadataOptions.AffectsRender));

        public double FontSize { get => (double)GetValue(FontSizeProperty); set => SetValue(FontSizeProperty, value); }

        public static readonly DependencyProperty FontFamilyProperty =
            TextElement.FontFamilyProperty.AddOwner(typeof(OutlinedTextBlock),
                new FrameworkPropertyMetadata(new FontFamily("Arial"), FrameworkPropertyMetadataOptions.AffectsRender));

        public FontFamily FontFamily { get => (FontFamily)GetValue(FontFamilyProperty); set => SetValue(FontFamilyProperty, value); }

        public static readonly DependencyProperty FillProperty =
            DependencyProperty.Register(nameof(Fill), typeof(Brush), typeof(OutlinedTextBlock),
                new FrameworkPropertyMetadata(Brushes.White, FrameworkPropertyMetadataOptions.AffectsRender));

        public Brush Fill { get => (Brush)GetValue(FillProperty); set => SetValue(FillProperty, value); }

        public static readonly DependencyProperty StrokeProperty =
            DependencyProperty.Register(nameof(Stroke), typeof(Brush), typeof(OutlinedTextBlock),
                new FrameworkPropertyMetadata(Brushes.Black, FrameworkPropertyMetadataOptions.AffectsRender));

        public Brush Stroke { get => (Brush)GetValue(StrokeProperty); set => SetValue(StrokeProperty, value); }

        public static readonly DependencyProperty StrokeThiccnessProperty =
            DependencyProperty.Register(nameof(StrokeThiccness), typeof(double), typeof(OutlinedTextBlock),
                new FrameworkPropertyMetadata(2.0, FrameworkPropertyMetadataOptions.AffectsRender));

        public double StrokeThiccness { get => (double)GetValue(StrokeThiccnessProperty); set => SetValue(StrokeThiccnessProperty, value); }

        public static readonly DependencyProperty FontStyleProperty =
            DependencyProperty.Register(nameof(FontStyle), typeof(FontStyle), typeof(OutlinedTextBlock),
                new FrameworkPropertyMetadata(FontStyles.Normal, FrameworkPropertyMetadataOptions.AffectsRender));

        public FontStyle FontStyle { get => (FontStyle)GetValue(FontStyleProperty); set => SetValue(FontStyleProperty, value); }

        public static readonly DependencyProperty FontWeightProperty =
            DependencyProperty.Register(nameof(FontWeight), typeof(FontWeight), typeof(OutlinedTextBlock),
                new FrameworkPropertyMetadata(FontWeights.Normal, FrameworkPropertyMetadataOptions.AffectsRender));

        public FontWeight FontWeight { get => (FontWeight)GetValue(FontWeightProperty); set => SetValue(FontWeightProperty, value); }

        public static readonly DependencyProperty ShadowColorProperty =
            DependencyProperty.Register(nameof(ShadowColor), typeof(Color), typeof(OutlinedTextBlock),
                new FrameworkPropertyMetadata(Colors.Transparent, FrameworkPropertyMetadataOptions.AffectsRender));

        public Color ShadowColor { get => (Color)GetValue(ShadowColorProperty); set => SetValue(ShadowColorProperty, value); }

        public static readonly DependencyProperty ShadowOffsetProperty =
            DependencyProperty.Register(nameof(ShadowOffset), typeof(Vector), typeof(OutlinedTextBlock),
                new FrameworkPropertyMetadata(new Vector(0, 1), FrameworkPropertyMetadataOptions.AffectsRender));

        public Vector ShadowOffset { get => (Vector)GetValue(ShadowOffsetProperty); set => SetValue(ShadowOffsetProperty, value); }

        public static readonly DependencyProperty HorizontalContentAlignmentProperty =
            DependencyProperty.Register(nameof(HorizontalContentAlignment), typeof(TextAlignment), typeof(OutlinedTextBlock),
                new FrameworkPropertyMetadata(TextAlignment.Left, FrameworkPropertyMetadataOptions.AffectsRender));

        public TextAlignment HorizontalContentAlignment { get => (TextAlignment)GetValue(HorizontalContentAlignmentProperty); set => SetValue(HorizontalContentAlignmentProperty, value); }

        public static readonly DependencyProperty VerticalContentAlignmentProperty =
            DependencyProperty.Register(nameof(VerticalContentAlignment), typeof(VerticalAlignment), typeof(OutlinedTextBlock),
                new FrameworkPropertyMetadata(VerticalAlignment.Top, FrameworkPropertyMetadataOptions.AffectsRender));

        public VerticalAlignment VerticalContentAlignment { get => (VerticalAlignment)GetValue(VerticalContentAlignmentProperty); set => SetValue(VerticalContentAlignmentProperty, value); }

        public OutlinedTextBlock()
        {
            // CacheMode = new BitmapCache();
        }

        protected override void OnRender(DrawingContext DrawingContext)
        {
            double X = 0, Y = 0;

            if (string.IsNullOrEmpty(Text)) return;

            var FormattedText = new FormattedText(
                Text,
                CultureInfo.CurrentUICulture, FlowDirection.LeftToRight,
                new Typeface(FontFamily, FontStyles.Normal, FontWeights.Normal, FontStretches.Normal),
                FontSize, Fill,
                VisualTreeHelper.GetDpi(this).PixelsPerDip);

            FormattedText.MaxTextWidth = ActualWidth;
            FormattedText.TextAlignment = HorizontalContentAlignment;

            switch (VerticalContentAlignment)
            {
                case VerticalAlignment.Top:
                default:
                    Y = 0;
                    break;
                case VerticalAlignment.Center:
                    Y = (ActualHeight - FormattedText.Height) / 2;
                    break;
                case VerticalAlignment.Bottom:
                    Y = ActualHeight - FormattedText.Height;
                    break;
                case VerticalAlignment.Stretch:
                    Console.WriteLine("Vertical alignment stretching is not supported yet... (Using top alignment instead.)");
                    break;
            }

            var Geometry = FormattedText.BuildGeometry(new Point(X, Y));

            if (ShadowColor.A > 0)
            {
                var ShadowBrush = new SolidColorBrush(ShadowColor);
                ShadowBrush.Freeze();
                var ShadowPen = new Pen(ShadowBrush, StrokeThiccness);
                DrawingContext.PushTransform(new TranslateTransform(ShadowOffset.X, ShadowOffset.Y));
                DrawingContext.DrawGeometry(ShadowBrush, ShadowPen, Geometry);
                DrawingContext.Pop();
            }

            if (Stroke != null && StrokeThiccness > 0)
            {
                DrawingContext.DrawGeometry(null, new Pen(Stroke, StrokeThiccness), Geometry);
            }

            if (Fill != null)
            {
                DrawingContext.DrawGeometry(Fill, null, Geometry);
            }
        }

        protected override Size MeasureOverride(Size AvailableSize)
        {
            if (string.IsNullOrEmpty(Text))
                return new Size(0, 0);

            var FormattedText = new FormattedText(
                Text,
                CultureInfo.CurrentUICulture, FlowDirection.LeftToRight,
                new Typeface(FontFamily, FontStyle, FontWeight, FontStretches.Normal),
                FontSize, Fill,
                VisualTreeHelper.GetDpi(this).PixelsPerDip);

            if (!double.IsInfinity(AvailableSize.Width))
                FormattedText.MaxTextWidth = AvailableSize.Width;
            if (!double.IsInfinity(AvailableSize.Height))
                FormattedText.MaxTextHeight = AvailableSize.Height;

            double TxtWidth = double.IsInfinity(AvailableSize.Width) ? FormattedText.Width : Math.Min(FormattedText.Width, AvailableSize.Width);
            double TxtHeight = double.IsInfinity(AvailableSize.Height) ? FormattedText.Height : Math.Min(FormattedText.Height, AvailableSize.Height);

            return new Size(TxtWidth, TxtHeight);
        }

        public Size MeasureTextSize()
        {
            var FormattedText = new FormattedText(
                Text,
                CultureInfo.CurrentUICulture, FlowDirection.LeftToRight,
                new Typeface(FontFamily, FontStyle, FontWeight, FontStretches.Normal),
                FontSize, Fill,
                VisualTreeHelper.GetDpi(this).PixelsPerDip);

            return new Size(FormattedText.Width, FormattedText.Height);
        }
    }
}