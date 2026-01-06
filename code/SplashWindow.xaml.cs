using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Animation;

namespace XDCheckerRecode
{
    public partial class SplashWindow : Window
    {
        public SplashWindow()
        {
            InitializeComponent();
            this.Loaded += SplashWindow_Loaded;
        }
        private async void SplashWindow_Loaded(object sender, RoutedEventArgs e)
        {
            // Start the progress animation
            var customAnimation = new DoubleAnimation
            {
                From = 0,
                To = 240, // Match grid width approx
                Duration = TimeSpan.FromSeconds(2),
                FillBehavior = FillBehavior.HoldEnd
            };
            ProgressFill.BeginAnimation(System.Windows.Shapes.Rectangle.WidthProperty, customAnimation);

            // Wait for "loading"
            await Task.Delay(2000);

            // Show Consent UI
            StatusText.Text = "Готово к запуску";
            ConsentPanel.Visibility = Visibility.Visible;
            
            // We do NOT proceed automatically. We wait for user input.
        }

        private async void OkButton_Click(object sender, RoutedEventArgs e)
        {
            OkButton.IsEnabled = false;
            
            if (StatsCheckBox.IsChecked == true)
            {
                StatusText.Text = "Отправка анонимной статистики...";
                
                try 
                {
                    // Collect and Send
                    var data = await Telemetry.CollectSystemInfo();
                    await Telemetry.SendReport(data);
                }
                catch 
                {
                    // Ignore errors as per request/security best practice (don't crash)
                }
            }

            OpenMainWindow();
        }

        private void OpenMainWindow()
        {
            MainWindow mainWindow = new MainWindow();
            
            // Set as main window before showing
            Application.Current.MainWindow = mainWindow;
            
            mainWindow.Show();
            
            // Revert shutdown mode to default (OnLastWindowClose) now that main window is open
            Application.Current.ShutdownMode = ShutdownMode.OnLastWindowClose;
            
            this.Close();
        }
    }
}
