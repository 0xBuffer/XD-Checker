using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Security.Principal;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Animation;

namespace XDCheckerRecode
{
    public partial class MainWindow : Window
    {
        private bool isMenuOpen = false;

        public MainWindow()
        {
            InitializeComponent();
            WindowAccentHelper.EnableBlur(this);
            Loaded += MainWindow_Loaded;
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            AddDefenderExclusion();
            ExtractResources();

            // Handle window dragging
            BackgroundBorder.MouseLeftButtonDown += (s, a) => DragMove();

            MenuButton.Click += ToggleMenu;
            CloseMenuButton.Click += ToggleMenu;
            
            CheckButton.Click += (s, a) => { MainFrame.Navigate(new CheckPage()); ToggleMenu(s, a); };
            ProgramsButton.Click += (s, a) => { MainFrame.Navigate(new ProgramsPage()); ToggleMenu(s, a); };
            AboutButton.Click += (s, a) => { MainFrame.Navigate(new AboutPage()); ToggleMenu(s, a); };
            
            // Navigate to initial page (ProgramsPage as shown in the screenshot)
            MainFrame.Navigate(new ProgramsPage());
        }

        private void MinimizeButton_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void AddDefenderExclusion()
        {
            if (!IsAdministrator())
            {
                // Restart and run as admin
                var exeName = Process.GetCurrentProcess().MainModule.FileName;
                ProcessStartInfo startInfo = new ProcessStartInfo(exeName);
                startInfo.Verb = "runas";
                try
                {
                    Process.Start(startInfo);
                }
                catch { }
                Application.Current.Shutdown();
                return;
            }

            var script = "Add-MpPreference -ExclusionPath 'C:\\XDC'";
            var processInfo = new ProcessStartInfo("powershell.exe", $"-NoProfile -ExecutionPolicy Bypass -Command \"{script}\"");
            processInfo.CreateNoWindow = true;
            processInfo.UseShellExecute = false;
            Process.Start(processInfo);
        }

        public static bool IsAdministrator()
        {
            WindowsIdentity identity = WindowsIdentity.GetCurrent();
            WindowsPrincipal principal = new WindowsPrincipal(identity);
            return principal.IsInRole(WindowsBuiltInRole.Administrator);
        }

        private void ExtractResources()
        {
            string dir = "C:\\XDC";
            if (!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }

            var assembly = Assembly.GetExecutingAssembly();
            string[] resources = { "AnyDesk.exe", "JournalTrace.exe", "everything.exe", "shellbag_analyzer_cleaner.exe" };

            foreach (var resourceName in resources)
            {
                string resourcePath = "XDCheckerRecode.Resources." + resourceName;
                string filePath = Path.Combine(dir, resourceName);

                if (!File.Exists(filePath))
                {
                    using (Stream stream = assembly.GetManifestResourceStream(resourcePath))
                    {
                        if (stream != null)
                        {
                            using (FileStream fileStream = new FileStream(filePath, FileMode.Create))
                            {
                                stream.CopyTo(fileStream);
                            }
                        }
                    }
                }
            }
        }

        private void ToggleMenu(object sender, RoutedEventArgs e)
        {
            var animation = new ThicknessAnimation();
            animation.Duration = TimeSpan.FromSeconds(0.3);
            
            // Assuming default margin is -250,0,0,0
            if (isMenuOpen)
            {
                animation.To = new Thickness(-250, 0, 0, 0);
            }
            else
            {
                animation.To = new Thickness(0, 0, 0, 0);
            }

            SideMenu.BeginAnimation(MarginProperty, animation);
            isMenuOpen = !isMenuOpen;
        }
    }
}