using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Shuriken.ViewModels;
using System.Collections.ObjectModel;
using System.Diagnostics;

namespace Shuriken
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private MainViewModel vm;
        public string titleAtLaunch;

        public MainWindow()
        {
            InitializeComponent();

            vm = new MainViewModel();
            DataContext = vm;

            editorSelect.SelectedIndex = 0;

            titleAtLaunch = Title;
        }
        private void NewClick(object sender, RoutedEventArgs e)
        {
            if (DataContext is MainViewModel vm)
            {
                vm.Clear();
            }
        }

        private void OpenMenu_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog fileDialog = new OpenFileDialog();
            fileDialog.Filter = "Ninja Chao Project Files|*.xncp;*.yncp";

            if (fileDialog.ShowDialog() == true)
            {
                vm.Load(fileDialog.FileName);
                Title = $"{titleAtLaunch} - {fileDialog.FileName}";
            }
        }

        private void HelpClick(object sender, RoutedEventArgs e)
        {
            string url = "https://hedgedocs.github.io/";
            url = url.Replace("&", "^&");
            Process.Start(new ProcessStartInfo(url) { UseShellExecute = true });
        }

        private void ViewClick(object sender, RoutedEventArgs e)
        {
        }
        private void Exit_Click(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show("Are you sure you want to exit?", "Quit", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if(result == MessageBoxResult.Yes)
            {
                App.Current.Shutdown();
            }
        }
    }
}
