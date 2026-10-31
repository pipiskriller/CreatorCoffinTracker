using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;

namespace CreatorCoffinTracker
{
    [TemplatePart(Name = PressScalePartName, Type = typeof(ScaleTransform))]
    public class SpriteButton : Button
    {
        private const string PressScalePartName = "PART_PressScale";

        private ScaleTransform? PressScale;

        public static readonly DependencyProperty SpriteSourceProperty =
            DependencyProperty.Register(
                nameof(SpriteSource),
                typeof(ImageSource),
                typeof(SpriteButton),
                new PropertyMetadata(null, OnSpriteChanged));

        public static readonly DependencyProperty SpriteRectProperty =
            DependencyProperty.Register(
                nameof(SpriteRect),
                typeof(Int32Rect),
                typeof(SpriteButton),
                new PropertyMetadata(Int32Rect.Empty, OnSpriteChanged));

        public static readonly DependencyProperty DisplayedSpriteProperty =
            DependencyProperty.Register(
                nameof(DisplayedSprite),
                typeof(ImageSource),
                typeof(SpriteButton),
                new PropertyMetadata(null));

        public static readonly DependencyProperty SpriteStretchProperty =
            DependencyProperty.Register(
                nameof(SpriteStretch),
                typeof(Stretch),
                typeof(SpriteButton),
                new PropertyMetadata(Stretch.Uniform));

        public static readonly DependencyProperty SpriteBitmapScalingModeProperty =
            DependencyProperty.Register(
                nameof(SpriteBitmapScalingMode),
                typeof(BitmapScalingMode),
                typeof(SpriteButton),
                new PropertyMetadata(BitmapScalingMode.LowQuality));

        public static readonly DependencyProperty PressedScaleProperty =
            DependencyProperty.Register(
                nameof(PressedScale),
                typeof(double),
                typeof(SpriteButton),
                new PropertyMetadata(0.95, OnPressedScaleChanged, CoercePressedScale));

        public ImageSource? SpriteSource
        {
            get => (ImageSource?)GetValue(SpriteSourceProperty);
            set => SetValue(SpriteSourceProperty, value);
        }

        public Int32Rect SpriteRect
        {
            get => (Int32Rect)GetValue(SpriteRectProperty);
            set => SetValue(SpriteRectProperty, value);
        }

        public ImageSource? DisplayedSprite
        {
            get => (ImageSource?)GetValue(DisplayedSpriteProperty);
            private set => SetValue(DisplayedSpriteProperty, value);
        }

        public Stretch SpriteStretch
        {
            get => (Stretch)GetValue(SpriteStretchProperty);
            set => SetValue(SpriteStretchProperty, value);
        }

        public BitmapScalingMode SpriteBitmapScalingMode
        {
            get => (BitmapScalingMode)GetValue(SpriteBitmapScalingModeProperty);
            set => SetValue(SpriteBitmapScalingModeProperty, value);
        }

        public double PressedScale
        {
            get => (double)GetValue(PressedScaleProperty);
            set => SetValue(PressedScaleProperty, value);
        }

        public SpriteButton()
        {
            Focusable = false;
        }

        protected override void OnClick()
        {
            base.OnClick();
            Keyboard.ClearFocus(); // always clear keyboard focus when clicked
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            PressScale = GetTemplateChild(PressScalePartName) as ScaleTransform;
            SetScale(IsPressed ? PressedScale : 1.0);
        }

        protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
        {
            base.OnPropertyChanged(e);

            if (e.Property == IsPressedProperty)
            {
                if ((bool)e.NewValue)
                {
                    AnimatePressed();
                }
                else
                {
                    AnimateReleased();
                }
            }
        }

        protected override void OnLostKeyboardFocus(KeyboardFocusChangedEventArgs e)
        {
            base.OnLostKeyboardFocus(e);
            AnimateReleased();
        }

        private static object CoercePressedScale(DependencyObject d, object BaseValue)
        {
            var Scale = (double)BaseValue;
            return double.IsFinite(Scale) ? Math.Clamp(Scale, 0.5, 1.0) : 0.95;
        }

        private static void OnPressedScaleChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var Btn = (SpriteButton)d;

            if (Btn.IsPressed)
            {
                Btn.AnimatePressed();
            }
        }

        private static void OnSpriteChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((SpriteButton)d).UpdateDisplayedSprite();
        }

        private void UpdateDisplayedSprite()
        {
            var Source = SpriteSource;
            var Rect = SpriteRect;

            if (Source is BitmapSource BitmapSource && !Rect.IsEmpty)
            {
                if (IsValidSpriteRect(BitmapSource, Rect))
                {
                    var Cropped = new CroppedBitmap(BitmapSource, Rect);

                    if (Cropped.CanFreeze)
                    {
                        Cropped.Freeze();
                    }

                    DisplayedSprite = Cropped;
                    return;
                }
            }

            DisplayedSprite = Source;
        }

        private static bool IsValidSpriteRect(BitmapSource Source, Int32Rect Rect)
        {
            return Rect.X >= 0
                && Rect.Y >= 0
                && Rect.Width > 0
                && Rect.Height > 0
                && Rect.X + Rect.Width <= Source.PixelWidth
                && Rect.Y + Rect.Height <= Source.PixelHeight;
        }

        private void AnimatePressed()
        {
            if (PressScale is null)
            {
                return;
            }

            var Anim = CreateScaleAnimation(PressedScale, 80);
            PressScale.BeginAnimation(ScaleTransform.ScaleXProperty, Anim, HandoffBehavior.SnapshotAndReplace);
            PressScale.BeginAnimation(ScaleTransform.ScaleYProperty, Anim, HandoffBehavior.SnapshotAndReplace);
        }

        private void AnimateReleased()
        {
            if (PressScale is null)
            {
                return;
            }

            var AnimX = CreateReleaseAnimation();
            var AnimY = CreateReleaseAnimation();

            PressScale.BeginAnimation(ScaleTransform.ScaleXProperty, AnimX, HandoffBehavior.SnapshotAndReplace);
            PressScale.BeginAnimation(ScaleTransform.ScaleYProperty, AnimY, HandoffBehavior.SnapshotAndReplace);
        }

        private static DoubleAnimation CreateScaleAnimation(double TargetScale, int Milliseconds)
        {
            return new DoubleAnimation
            {
                To = TargetScale,
                Duration = TimeSpan.FromMilliseconds(Milliseconds),
                EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
            };
        }

        private static DoubleAnimationUsingKeyFrames CreateReleaseAnimation()
        {
            return new DoubleAnimationUsingKeyFrames
            {
                KeyFrames =
                {
                    new EasingDoubleKeyFrame(
                        1.04,
                        KeyTime.FromTimeSpan(TimeSpan.FromMilliseconds(90)),
                        new CubicEase { EasingMode = EasingMode.EaseOut }),
                    new EasingDoubleKeyFrame(
                        1.0,
                        KeyTime.FromTimeSpan(TimeSpan.FromMilliseconds(170)),
                        new CubicEase { EasingMode = EasingMode.EaseInOut })
                }
            };
        }

        private void SetScale(double Scale)
        {
            if (PressScale is null)
            {
                return;
            }

            PressScale.BeginAnimation(ScaleTransform.ScaleXProperty, null);
            PressScale.BeginAnimation(ScaleTransform.ScaleYProperty, null);
            PressScale.ScaleX = Scale;
            PressScale.ScaleY = Scale;
        }
    }
}
