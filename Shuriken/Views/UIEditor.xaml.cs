using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using Shuriken.Models;
using Shuriken.Rendering;
using Shuriken.ViewModels;
using Shuriken.Misc;
using OpenTK;
using OpenTK.Core;
using OpenTK.Windowing.Common;
using OpenTK.Graphics.OpenGL;
using OpenTK.Wpf;

namespace Shuriken.Views
{
    /// <summary>
    /// Interaction logic for UIEditor.xaml
    /// </summary>
    public partial class UIEditor : UserControl
    {
        Converters.ColorToBrushConverter colorConverter;
        Renderer renderer;

        public UIEditor()
        {
            InitializeComponent();

            GLWpfControlSettings glSettings = new GLWpfControlSettings
            {
                GraphicsProfile = ContextProfile.Core,
                MajorVersion = 3,
                MinorVersion = 3,
            };

            glControl.Start(glSettings);

            GL.Enable(EnableCap.Blend);
            GL.BlendEquation(BlendEquationMode.FuncAdd);
            GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
            GL.Enable(EnableCap.FramebufferSrgb);

            colorConverter = new Converters.ColorToBrushConverter();
            renderer = new Renderer(1280, 720);
        }

        private void glControlRender(TimeSpan obj)
        {
            /*
            System.Windows.Media.Brush brush = Application.Current.TryFindResource("RegionBrush") as System.Windows.Media.Brush;
            Color clearColor = new Color();
            if (brush != null)
            {
                clearColor = (Color)colorConverter.ConvertBack(brush, typeof(Color), null, CultureInfo.InvariantCulture);
            }

            GL.ClearColor(clearColor.R / 255.0f, clearColor.G / 255.0f, clearColor.B / 255.0f, 1.0f);
            */
            GL.ClearColor(0.2f, 0.2f, 0.2f, 1.0f);
            GL.Clear(ClearBufferMask.ColorBufferBit);

            float delta = obj.Milliseconds / 1000.0f * 60.0f;
            var vm = DataContext as ScenesViewModel;
            if (vm != null)
            {
                vm.UpdateScenes(Project.Scenes, Project.Fonts, delta);
                //renderer.DrawScenes(Project.Scenes, Project.Fonts, delta);
            }
        }

        private void ScenesTreeViewSelected(object sender, RoutedEventArgs e)
        {
            // Move up the tree view until we reach the TreeViewItem holding the UIScene
            TreeViewItem item = e.OriginalSource as TreeViewItem;
            while (item != null && item.DataContext != null && item.DataContext is not UIScene)
                item = Utilities.GetParentTreeViewItem(item);

            var vm = DataContext as ScenesViewModel;
            if (vm != null && item != null)
            {
                vm.SelectedScene = item.DataContext as UIScene;
            }
        }
    }
}
