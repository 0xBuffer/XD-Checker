using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace XDCheckerRecode
{
    public partial class ProgramsPage : Page
    {
        public class ProgramItem
        {
            public string Name { get; set; }
            public string Description { get; set; }
            public string FileName { get; set; }
            public string IconUrl { get; set; }
            public bool IsAvailable { get; set; }
        }

        public ProgramsPage()
        {
            InitializeComponent();
            LoadPrograms();
        }

        private async void LoadPrograms()
        {
            var programs = new List<ProgramItem>
            {
                new ProgramItem { Name = "Ocean", Description = "Автоматически проверяет Ваш ПК на наличие или использование запрещенного ПО", FileName = "Ocean.exe", IconUrl = "https://anticheat.ac/home/ocean_logosvg.svg" },
                new ProgramItem { Name = "AnyDesk", Description = "Приложение для удалённого рабочего стола.", FileName = "AnyDesk.exe", IconUrl = "https://anydesk.com/_static/img/logos/anydesk-logo-white-red-ec4e3a.png" },
                new ProgramItem { Name = "Everything", Description = "Мгновенный поиск по папкам, названиям или весу файлов", FileName = "everything.exe", IconUrl = "https://www.voidtools.com/voidtools9.png" },
                new ProgramItem { Name = "JournalTrace", Description = "Отслеживает действия, которые пользователь совершал с файлами и папками", FileName = "JournalTrace.exe", IconUrl = "" },
                new ProgramItem { Name = "PreviousFilesRecovery", Description = "Узнает, какие файлы и папки удалялись, с возможностью восстановления", FileName = "PreviousFilesRecovery.exe", IconUrl = "https://www.nirsoft.net/utils/previousfilesrecovery.png" },
                new ProgramItem { Name = "ShellBags Analyzer & Cleaner", Description = "Узнает, какие папки посещались, удалялись и прочее", FileName = "shellbag_analyzer_cleaner.exe", IconUrl = "https://privazer.com/en/images/privazer-logo-name.jpg.pagespeed.ce.rSQ2t4kGtk.jpg" },
                new ProgramItem { Name = "Process Hacker 2", Description = "Позволяет залезть в память устройства и свойства процессов", FileName = "ProcessHacker.exe", IconUrl = "https://a.fsdn.com/allura/p/processhacker/icon?1518244867" },
                new ProgramItem { Name = "SystemInformer", Description = "Позволяет залезть в память устройства и свойства процессов", FileName = "SystemInformer.exe", IconUrl = "https://systeminformer.sourceforge.io/include/systeminformer.png" },
                new ProgramItem { Name = "USB-DriveLog", Description = "Отслеживает присоединение/отключение внешних носителей (USB Флешек)", FileName = "USBDriveLog.exe", IconUrl = "https://www.nirsoft.net/utils/usbdrivelog.png" },
                new ProgramItem { Name = "USB-Deview", Description = "Отслеживает присоединение/отключение внешних устройств (USB устройств)", FileName = "USBDeview.exe", IconUrl = "" },
                new ProgramItem { Name = "ExecutedProgramsList", Description = "Поиск и анализ ранее запускавшихся приложений или программ", FileName = "ExecutedProgramsList.exe", IconUrl = "https://www.nirsoft.net/utils/executedprogramslist.png" },
                new ProgramItem { Name = "WinPrefetchView", Description = "Подробный анализ папки Prefetch", FileName = "WinPrefetchView.exe", IconUrl = "https://www.nirsoft.net/utils/winprefetchview.gif" },
                new ProgramItem { Name = "LastActivityView", Description = "Анализирует активность ПК, чтобы найти запускавшиеся .exe-читы", FileName = "LastActivityView.exe", IconUrl = "https://www.nirsoft.net/utils/lastactivityview.png" },
                new ProgramItem { Name = "CachedProgramsList", Description = "Позволяет найти в памяти системы информацию о запускавшихся файлах", FileName = "CachedProgramsList.exe", IconUrl = "" },
                new ProgramItem { Name = "OpenSaveFilesView", Description = "Позволяет найти в памяти системы информацию о запускавшихся файлах", FileName = "OpenSaveFilesView.exe", IconUrl = "https://www.nirsoft.net/utils/opensavefilesview.png" }
            };

            await System.Threading.Tasks.Task.Run(() =>
            {
                string dir = "C:\\XDC";
                foreach (var program in programs)
                {
                    try {
                        program.IsAvailable = File.Exists(Path.Combine(dir, program.FileName));
                    } catch { program.IsAvailable = false; }
                }
            });

            ProgramsItemsControl.ItemsSource = programs;
        }

        private void LaunchButton_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            if (button != null)
            {
                var program = button.DataContext as ProgramItem;
                if (program != null && program.IsAvailable)
                {
                    Process.Start(Path.Combine("C:\\XDC", program.FileName));
                }
            }
        }
    }

    public class AvailabilityToTextConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool isAvailable && isAvailable)
            {
                return "Открыть";
            }
            return "Недоступно";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
