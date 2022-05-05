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
using XNCPLib;

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
            FrameworkCompatibilityPreferences.KeepTextBoxDisplaySynchronizedWithTextProperty = false;
            InitializeComponent();

            vm = new MainViewModel();
            DataContext = vm;

            editorSelect.SelectedIndex = 0;
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
            fileDialog.Filter = Filters.NinjaTypeFilter;
            fileDialog.FilterIndex = (int)MainViewModel.Type;

            if (fileDialog.ShowDialog() == true)
            {
                MainViewModel.Type = (NinjaType)fileDialog.FilterIndex;
                vm.Load(fileDialog.FileName);
            }
        }
        private void SaveMenu_Click(object sender, RoutedEventArgs e)
        {
            vm.Save(null);
        }

        private void SaveAsMenu_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog fileDialog = new SaveFileDialog();
            fileDialog.Filter = Filters.NinjaTypeFilter;
            fileDialog.FilterIndex = (int)MainViewModel.Type;

            if (fileDialog.ShowDialog() == true)
            {
                MainViewModel.Type = (NinjaType)fileDialog.FilterIndex;
                vm.Save(fileDialog.FileName);
            }
        }

        private void HelpClick(object sender, RoutedEventArgs e)
        {
        }

        private void ViewClick(object sender, RoutedEventArgs e)
        {
        }

        private void ExitMenu_Click(object sender, RoutedEventArgs e)
        {
            // TODO: Check for differences in the loaded file and prompt the user to save
            Application.Current.Shutdown();
        }
    }
}
