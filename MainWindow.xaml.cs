using System.Diagnostics;
using System.Net.Http;
using System.Security.Policy;
using System.Text;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace CreatorCoffinTracker
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private const int SuccessStatusCode = 200;
        private const double MilestoneIndicatorWidth = 2.25;
        private const double MilestoneIndicatorHeight = 57;
        private const double MilestoneLabelWidth = 150;
        private const double MilestoneLabelTop = 66;
        private int PageNumber = 0;
        private int IndicatorOffset = -9;

        private static readonly Uri ResourcePath = new("pack://application:,,,/");

        private CreatorData? Data;
        private NAudioAudioPlayer Audio = new();
        public MainWindow()
        {
            InitializeComponent();
            Audio.Play("SFX/sfx_btnclick.ogg", 0f);
        }

        private async void BTN_Fetch_Click(object sender, RoutedEventArgs e)
        {
            Audio.Play("SFX/sfx_btnclick.ogg", 0.7f);

            if (!string.IsNullOrWhiteSpace(TxtField_CreatorCodeInput.Text))
            {
                CreatorCodeDataNotFoundYet.Text = "Fetching...";
                CreatorCodeDataNotFoundYet.Fill = Brushes.White;

                BTN_ClearData.IsEnabled = false;

                CreatorCodeDataDisplay.Visibility = Visibility.Collapsed;
                CreatorCodeDataNotFoundYet.Visibility = Visibility.Visible;

                Data = await Utils.FetchData(TxtField_CreatorCodeInput.Text.ToLower());

                BTN_ClearData.IsEnabled = true;

                if (Data.StatusCode == SuccessStatusCode)
                {
                    CreatorCodeDataDisplay.Visibility = Visibility.Visible;
                    CreatorCodeDataNotFoundYet.Visibility = Visibility.Collapsed;

                    CreatorName.Text = Data.DisplayName ?? "N/A";
                    CreatorTeamScore.Text = "with " + Data.Players.ToString("#,###") + " players";

                    CreatorHandle.Text = "@" + Data.Channel?.Handle ?? "idontknow";

                    CreatorPFP.ImageSource = Utils.DecodeWebP(Data.AvatarData);
                    PageNumber = Data.Days.Count - 1;
                    UpdateProgressDisplay();

                    return;
                }

                Audio.Play("SFX/snd_splat.wav", 0.7f);
                CreatorCodeDataNotFoundYet.Text = "Invalid creator code! Please recheck!";
                CreatorCodeDataNotFoundYet.Fill = Brushes.OrangeRed;
            }
        }

        private void BTN_ClearData_Click(object sender, RoutedEventArgs e)
        {
            Audio.Play("SFX/sfx_btnclick.ogg", 0.7f);

            Data = null;

            TxtField_CreatorCodeInput.Text = string.Empty;

            PageNumber = 0;

            CreatorProgressValue.Text = "0";
            CreatorProgress.Maximum = 1;
            CreatorProgress.Value = 0;

            CreatorProgressIndicators.Children.Clear();

            CreatorDayX.Text = "Day -/-";
            BTN_PageLeft.IsEnabled = false;
            BTN_PageRight.IsEnabled = false;

            CreatorCodeDataNotFoundYet.Text = "Enter a valid creator code on the textfield above and press Fetch to start!";
            CreatorCodeDataNotFoundYet.Fill = Brushes.White;

            CreatorCodeDataDisplay.Visibility = Visibility.Collapsed;
            CreatorCodeDataNotFoundYet.Visibility = Visibility.Visible;
        }

        private void BTN_GoToChannel_Click(object sender, RoutedEventArgs e)
        {
            Audio.Play("SFX/sfx_btnclick.ogg", 0.7f);
            if (Data != null && Data.Channel != null && Data.Channel.URL != null)
            {
                Process.Start(new ProcessStartInfo(Data.Channel.URL) { UseShellExecute = true });
            }
        }

        private void BTN_PageLeft_Click(object sender, RoutedEventArgs e)
        {
            if (Data is null || PageNumber == 0) return;

            Audio.Play("SFX/sfx_btnclick.ogg", 0.7f);
            PageNumber--;
            UpdateProgressDisplay();
        }

        private void BTN_PageRight_Click(object sender, RoutedEventArgs e)
        {
            if (Data is null || PageNumber >= Data.Days.Count - 1) return;

            Audio.Play("SFX/sfx_btnclick.ogg", 0.7f);
            PageNumber++;
            UpdateProgressDisplay();
        }

        private void CreatorProgressIndicators_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            PositionMilestoneIndicators();
        }

        private void UpdateProgressDisplay()
        {
            if (Data is null || Data.Days.Count == 0) return;

            PageNumber = Math.Clamp(PageNumber, 0, Data.Days.Count - 1);
            Days CurrentDay = Data.Days[PageNumber];

            double Maximum = CurrentDay.Milestones.Count > 0 ? Math.Max(CurrentDay.Milestones.Max(), 1) : Math.Max(CurrentDay.Value, 1);

            CreatorProgress.Maximum = Maximum;
            CreatorProgress.Value = Math.Clamp(CurrentDay.Value, 0, Maximum);
            CreatorProgressValue.Text = CurrentDay.Value.ToString("#,##0");
            CreatorDayX.Text = $"Day {PageNumber + 1}/{Data.Days.Count}";

            BTN_PageLeft.IsEnabled = PageNumber > 0;
            BTN_PageRight.IsEnabled = PageNumber < Data.Days.Count - 1;

            CreateMilestoneIndicators(CurrentDay.Milestones, CurrentDay.Value, Maximum);
        }

        private void CreateMilestoneIndicators(List<double> Milestones, double CurrentValue, double Maximum)
        {
            CreatorProgressIndicators.Children.Clear();

            if (Maximum <= 0) return;

            foreach (double Milestone in Milestones.Distinct().Order())
            {
                var Indicator = new Canvas
                {
                    Width = MilestoneIndicatorWidth,
                    Height = MilestoneLabelTop + 28,
                    Tag = Math.Clamp(Milestone / Maximum, 0, 1),
                    RenderTransformOrigin = new Point(1, 1),
                    RenderTransform = new SkewTransform() { AngleX = -4.5, AngleY = 0 },
                    CacheMode = new BitmapCache()
                };

                var Tick = new Border
                {
                    Width = MilestoneIndicatorWidth,
                    Height = MilestoneIndicatorHeight,
                    Background = Brushes.Black,
                    CacheMode = new BitmapCache()
                };

                var Label = new OutlinedTextBlock
                {
                    Width = MilestoneLabelWidth,
                    Height = 28,
                    HorizontalAlignment = HorizontalAlignment.Right,
                    VerticalAlignment = VerticalAlignment.Center,
                    HorizontalContentAlignment = TextAlignment.Right,
                    VerticalContentAlignment = VerticalAlignment.Top,
                    Text = Milestone.ToString("#,##0"),
                    Fill = Milestone <= CurrentValue ? Brushes.Gold : Brushes.White,
                    FontSize = 18,
                    FontFamily = new FontFamily(ResourcePath, "Media/#Lilita One"),
                    StrokeThiccness = 2.5,
                    ShadowColor = Colors.Black,
                    ShadowOffset = new(0, 2),
                    CacheMode = new BitmapCache()
                };

                double LabelLeft = (MilestoneIndicatorWidth - MilestoneLabelWidth) / 2;
                if (Milestone <= 0) LabelLeft = 0;
                else if (Milestone >= Maximum) LabelLeft = MilestoneIndicatorWidth - MilestoneLabelWidth;

                Canvas.SetTop(Tick, 3);
                Canvas.SetLeft(Label, LabelLeft);
                Canvas.SetTop(Label, MilestoneLabelTop);

                Indicator.Children.Add(Tick);
                Indicator.Children.Add(Label);
                CreatorProgressIndicators.Children.Add(Indicator);
            }

            PositionMilestoneIndicators();
        }

        private void PositionMilestoneIndicators()
        {
            double TrackWidth = CreatorProgressIndicators.ActualWidth;
            if (TrackWidth <= 0) return;

            foreach (UIElement Child in CreatorProgressIndicators.Children)
            {
                if (Child is not FrameworkElement Indicator || Indicator.Tag is not double Position) continue;

                double Left = (TrackWidth * Position) - (Indicator.Width / 2) + IndicatorOffset;
                Canvas.SetLeft(Indicator, Math.Clamp(Left, 0, CreatorProgressIndicators.ActualWidth - Indicator.Width));
            }
        }
    }
}
