using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace CreatorCoffinTracker
{
    /// <summary>
    /// Interaction logic for ScrollingIconsBgComponent.xaml
    /// </summary>
    public partial class ScrollingIconsBgComponent : UserControl
    {
        public ScrollingIconsBgComponent()
        {
            InitializeComponent();

            var StartThemeAnim = (Storyboard)FindResource("BasicTheme");
            StartThemeAnim.Begin();
        }
    }
}
