using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace XDCheckerRecode
{
    public partial class CheckPage : Page
    {
        public ObservableCollection<ScanResultItem> ScanResults { get; set; }
        public ObservableCollection<ExtensionFilterItem> ExtensionFilters { get; set; }

        private CancellationTokenSource _cts;
        private bool _isScanning = false;
        
        // The "Base" (List) for searching
        private List<string> _searchDatabase = new List<string>
        {
            "cheat", 
            "catlavan", 
            ".catlavan", 
            "celestial", 
            "celka", 
            "midnight", 
            "sk3d",
            "Nurik",
            "Nursultan",
            "Monoton",
            "expensive",
            "eternity",
            "impact",
            "intellij",
            "takker",
            "wexide",
            ".wex",
            "crack",
            "zenith",
            "thunder",
            "delta",
            "rockstar",
            "venus",
            "inertia",
            "energy",
            "dimasik",
            "hachclient",
            "excellent",
            "darkside",
            "privateclient",
            "dreamcore",
            "haruka",
            "newlauncher",
            "mentality",
            "minced",
            "quickclient",
            "meowdlc",
            "liquid",
            "wurst",
            "shakefree",
            "nightdlc",
            "arbuz",
            "xray",
            "baritone",


        };

        public CheckPage()
        {
            InitializeComponent();
            ScanResults = new ObservableCollection<ScanResultItem>();
            ResultsGrid.ItemsSource = ScanResults;

            // Initialize extensions
            ExtensionFilters = new ObservableCollection<ExtensionFilterItem>
            {
                new ExtensionFilterItem { Extension = ".exe", IsSelected = true },
                new ExtensionFilterItem { Extension = ".bat", IsSelected = true },
                new ExtensionFilterItem { Extension = ".java", IsSelected = true },
                new ExtensionFilterItem { Extension = ".mac", IsSelected = true }, // Macros?
                new ExtensionFilterItem { Extension = ".ahk", IsSelected = true },
                new ExtensionFilterItem { Extension = ".dat", IsSelected = true },
                new ExtensionFilterItem { Extension = ".dll", IsSelected = false },
                new ExtensionFilterItem { Extension = ".txt", IsSelected = false },
                new ExtensionFilterItem { Extension = ".cmd", IsSelected = false },
                new ExtensionFilterItem { Extension = ".vbs", IsSelected = false },
                new ExtensionFilterItem { Extension = ".ps1", IsSelected = false }
            };
            
            // Bind the extensions list in UI (Need to set ItemsSource for the Popup's ItemsControl)
            // Ideally we do this in XAML or here. Let's assume we name the ItemsControl in XAML.
            // ExtensionsItemsControl.ItemsSource = ExtensionFilters; (Will do in XAML via binding if possible, or Name reference)
        }

        private async void StartScanButton_Click(object sender, RoutedEventArgs e)
        {
            if (_isScanning)
            {
                // Cancel
                _cts?.Cancel();
                _isScanning = false;
                StartScanButton.Content = "Start Disk Scan";
                StatusTextBlock.Text = "Scan cancelled.";
                ScanProgressBar.IsIndeterminate = false;
                return;
            }

            _isScanning = true;
            StartScanButton.Content = "Stop Scan";
            ScanResults.Clear();
            _cts = new CancellationTokenSource();
            ScanProgressBar.IsIndeterminate = true;
            StatusTextBlock.Text = "Starting scan...";

            // Determine drives to scan
            List<string> drivesToScan = new List<string>();
            if (FullScanCheckBox.IsChecked == true)
            {
                foreach (var drive in DriveInfo.GetDrives())
                {
                    if (drive.DriveType == DriveType.Fixed && drive.IsReady)
                    {
                        drivesToScan.Add(drive.RootDirectory.FullName);
                    }
                }
            }
            else
            {
                drivesToScan.Add("C:\\");
            }

            // Determine extensions to scan
            HashSet<string> allowedExtensions = null;
            if (TypeCheckBox.IsChecked == true)
            {
                allowedExtensions = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                foreach (var item in ExtensionFilters)
                {
                    if (item.IsSelected)
                    {
                        allowedExtensions.Add(item.Extension);
                    }
                }
            }

            try
            {
                await Task.Run(() => 
                {
                    foreach (var drive in drivesToScan)
                    {
                        if (_cts.Token.IsCancellationRequested) break;
                        ScanDrive(drive, _cts.Token, allowedExtensions);
                    }
                }, _cts.Token);

                if (!_cts.Token.IsCancellationRequested)
                    StatusTextBlock.Text = "Scan complete.";
            }
            catch (OperationCanceledException)
            {
                StatusTextBlock.Text = "Scan cancelled.";
            }
            catch (Exception ex)
            {
                StatusTextBlock.Text = $"Error: {ex.Message}";
            }
            finally
            {
                _isScanning = false;
                StartScanButton.Content = "Start Disk Scan";
                ScanProgressBar.IsIndeterminate = false;
            }
        }

        private void ScanDrive(string rootPath, CancellationToken token, HashSet<string> allowedExtensions)
        {
            var directories = new Stack<string>();
            directories.Push(rootPath);
            
            long fileCount = 0;

            while (directories.Count > 0)
            {
                token.ThrowIfCancellationRequested();
                string currentDir = directories.Pop();

                try
                {
                    // Update Status text less frequently for directories
                    Dispatcher.Invoke(() => 
                    {
                        StatusTextBlock.Text = $"Scanning: {currentDir}";
                    });

                    // Get files
                    string[] files = Directory.GetFiles(currentDir);
                    foreach (var file in files)
                    {
                        token.ThrowIfCancellationRequested();
                        fileCount++;
                        
                        // Extension Filter
                        if (allowedExtensions != null)
                        {
                            string ext = Path.GetExtension(file);
                            if (!allowedExtensions.Contains(ext))
                            {
                                continue; 
                            }
                        }

                        // UI Update
                        if (fileCount % 50 == 0)
                        {
                             Dispatcher.Invoke(() => StatusTextBlock.Text = $"Checking: {Path.GetFileName(file)}");
                        }

                        // Check against database
                        CheckFile(file);
                    }

                    // Push subdirectories
                    string[] subDirs = Directory.GetDirectories(currentDir);
                    foreach (var dir in subDirs)
                    {
                        directories.Push(dir);
                    }
                }
                catch (UnauthorizedAccessException) { }
                catch (Exception) { }
            }
        }

        private void CheckFile(string filePath)
        {
            string fileName = Path.GetFileName(filePath);
            
            for (int i = 0; i < _searchDatabase.Count; i++)
            {
                string term = _searchDatabase[i];
                if (fileName.IndexOf(term, StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    try
                    {
                        FileInfo fi = new FileInfo(filePath);
                        var result = new ScanResultItem
                        {
                            FileName = fileName,
                            FoundReason = $"Index: {i} ({term})",
                            FilePath = filePath,
                            Timestamp = $"{fi.LastWriteTime:yyyy-MM-dd HH:mm}"
                        };

                        Dispatcher.Invoke(() => ScanResults.Add(result));
                    }
                    catch { }
                    break;
                }
            }
        }

        private void FullScanCheckBox_CheckedChanged(object sender, RoutedEventArgs e)
        {
            if (TypeCheckBox == null) return;

            bool isFullScan = FullScanCheckBox.IsChecked == true;
            
            // disable/enable TypeCheckBox if FullScan is checked?
            // "сканирует все диски... другие галочки становятся не активны"
            // So if Full Scan is Checked, Type is Disabled (unchecked?) or just disabled?
            // User said "other checkboxes become inactive".
            
            TypeCheckBox.IsEnabled = !isFullScan;
            
            if (isFullScan)
            {
                // Optionally uncheck or close the popup
                TypeCheckBox.IsChecked = false;
            }
        }
    }

    public class ExtensionFilterItem
    {
        public string Extension { get; set; }
        public bool IsSelected { get; set; }
    }

    public class ScanResultItem
    {
        public string FileName { get; set; }
        public string FoundReason { get; set; }
        public string FilePath { get; set; }
        public string Timestamp { get; set; }
    }
}
