using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Management;
using System.Windows.Controls;
using Microsoft.Win32;

namespace XDCheckerRecode
{
    public partial class AboutPage : Page
    {
        public ObservableCollection<SystemInfoItem> SystemInfoList { get; set; }

        public AboutPage()
        {
            InitializeComponent();
            SystemInfoList = new ObservableCollection<SystemInfoItem>();
            DataContext = this;
            LoadSystemInfo();
        }

        private async void LoadSystemInfo()
        {
            await System.Threading.Tasks.Task.Run(() =>
            {
                var infos = new List<SystemInfoItem>();

                try
                {
                    // OS & Install Date
                    string osName = "Unknown";
                    string installDate = "Unknown";
                    string buildVersion = Environment.OSVersion.Version.Build.ToString();

                    using (var searcher = new ManagementObjectSearcher("SELECT Caption, InstallDate FROM Win32_OperatingSystem"))
                    {
                        foreach (var item in searcher.Get())
                        {
                            osName = item["Caption"]?.ToString();
                            string rawDate = item["InstallDate"]?.ToString(); // yyyyMMddHHmmss...
                            if (!string.IsNullOrEmpty(rawDate) && rawDate.Length >= 8)
                            {
                                installDate = $"{rawDate.Substring(6, 2)}.{rawDate.Substring(4, 2)}.{rawDate.Substring(0, 4)}";
                            }
                            break;
                        }
                    }
                    infos.Add(new SystemInfoItem("Recoded by", "0xBuffer"));
                    infos.Add(new SystemInfoItem("OS", osName));
                    infos.Add(new SystemInfoItem("Install Date", installDate));
                    infos.Add(new SystemInfoItem("Build Version", buildVersion));
                    
                    // CPU
                    using (var searcher = new ManagementObjectSearcher("SELECT Name FROM Win32_Processor"))
                    {
                        foreach (var item in searcher.Get())
                        {
                            infos.Add(new SystemInfoItem("CPU", item["Name"]?.ToString()));
                            break;
                        }
                    }

                    // GPU
                    using (var searcher = new ManagementObjectSearcher("SELECT Name FROM Win32_VideoController"))
                    {
                        foreach (var item in searcher.Get())
                        {
                            infos.Add(new SystemInfoItem("GPU", item["Name"]?.ToString()));
                            break;
                        }
                    }

                    // Motherboard
                    using (var searcher = new ManagementObjectSearcher("SELECT Product FROM Win32_BaseBoard"))
                    {
                        foreach (var item in searcher.Get())
                        {
                            infos.Add(new SystemInfoItem("Motherboard", item["Product"]?.ToString()));
                            break;
                        }
                    }

                    // RAM
                    using (var searcher = new ManagementObjectSearcher("SELECT Capacity FROM Win32_PhysicalMemory"))
                    {
                        long totalRam = 0;
                        foreach (var item in searcher.Get())
                        {
                            if (long.TryParse(item["Capacity"]?.ToString(), out long capacity))
                            {
                                totalRam += capacity;
                            }
                        }
                        double gb = Math.Round(totalRam / (1024.0 * 1024 * 1024), 1);
                        infos.Add(new SystemInfoItem("RAM", $"{gb} GB"));
                    }

                    // OBS Studio
                    bool obsInstalled = false;
                    // Check various common paths
                    var obsPaths = new List<string>
                    {
                        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "OBS Studio"),
                        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86), "OBS Studio"),
                        @"C:\Program Files\obs-studio", // Specific request
                        @"C:\ProgramData\Microsoft\Windows\Start Menu\Programs\OBS Studio" // Start Menu
                    };

                    foreach (var path in obsPaths)
                    {
                        if (Directory.Exists(path))
                        {
                            obsInstalled = true;
                            break;
                        }
                    }

                    // Also check for shortcut in Start Menu root if folder doesn't exist
                    if (!obsInstalled)
                    {
                         string startMenuRoot = @"C:\ProgramData\Microsoft\Windows\Start Menu\Programs";
                         string lnkPath = Path.Combine(startMenuRoot, "OBS Studio.lnk");
                         if (File.Exists(lnkPath)) obsInstalled = true;
                    }
                    
                    infos.Add(new SystemInfoItem("OBS Studio - Installed", obsInstalled ? "Yes" : "No"));

                    bool obsRunning = Process.GetProcessesByName("obs64").Length > 0 || Process.GetProcessesByName("obs32").Length > 0;
                    infos.Add(new SystemInfoItem("OBS Studio - Running", obsRunning ? "Yes" : "No"));

                    // VM Check
                    string vmType = "None";
                    using (var searcher = new ManagementObjectSearcher("SELECT Manufacturer, Model FROM Win32_ComputerSystem"))
                    {
                        foreach (var item in searcher.Get())
                        {
                            string manufacturer = item["Manufacturer"]?.ToString().ToLower() ?? "";
                            string model = item["Model"]?.ToString().ToLower() ?? "";

                            if (manufacturer.Contains("vmware") || model.Contains("vmware")) vmType = "VMware";
                            else if (manufacturer.Contains("virtualbox") || model.Contains("virtualbox")) vmType = "VirtualBox";
                            else if (manufacturer.Contains("qemu") || model.Contains("qemu")) vmType = "QEMU/KVM";
                            else if (manufacturer.Contains("microsoft corporation") && model.Contains("virtual")) vmType = "Hyper-V";
                            break;
                        }
                    }
                    infos.Add(new SystemInfoItem("Virtual Machine", vmType == "None" ? "No" : $"Yes ({vmType})"));

                }
                catch (Exception)
                {
                    // Fallback
                }

                Dispatcher.Invoke(() =>
                {
                    SystemInfoList.Clear();
                    foreach (var i in infos) SystemInfoList.Add(i);
                });
            });
        }
    }

    public class SystemInfoItem
    {
        public string Name { get; set; }
        public string Value { get; set; }

        public SystemInfoItem(string name, string value)
        {
            Name = name;
            Value = value;
        }
    }
}
