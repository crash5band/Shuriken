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

namespace Shuriken
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private MainViewModel vm;

        public MainWindow()
        {
            InitializeComponent();

            vm = new MainViewModel();
            DataContext = vm;

            editorSelect.SelectedIndex = 0;
        }

        private void OpenMenu_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog fileDialog = new OpenFileDialog();
            fileDialog.Filter = "Ninja Chao Project Files|*.xncp;*.yncp";

            if (fileDialog.ShowDialog() == true)
            {
                vm.Load(fileDialog.FileName);
            }
        }

        private void HelpClick(object sender, RoutedEventArgs e)
        {
            HandyControl.Controls.MessageBox.Show("Help is on the way...", "Send Help", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void ViewClick(object sender, RoutedEventArgs e)
        {
            HandyControl.Controls.MessageBox.Show("Same tbh...", "IDK", MessageBoxButton.OK, MessageBoxImage.Question);
        }
    }
}
