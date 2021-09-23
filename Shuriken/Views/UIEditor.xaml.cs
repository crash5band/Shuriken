using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
        private Renderer r;
        private GLWpfControlSettings glSettings;

        public UIEditor()
        {
            InitializeComponent();

            glSettings = new GLWpfControlSettings
            {
                GraphicsProfile = ContextProfile.Core,
                MajorVersion = 3,
                MinorVersion = 3,
            };

            glControl.Start(glSettings);
            r = new Renderer(1280, 720);

            GL.Enable(EnableCap.Blend);
            GL.BlendEquation(BlendEquationMode.FuncAdd);
            GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
            GL.Enable(EnableCap.FramebufferSrgb);
        }

        private void glControlRender(TimeSpan obj)
        {
            GL.ClearColor(0.2f, 0.2f, 0.2f, 0.0f);
            GL.Clear(ClearBufferMask.ColorBufferBit);

            var vm = DataContext as MainViewModel;
            float delta = obj.Milliseconds / 1000.0f * 60.0f;

            if (vm != null)
            {
                UIProject project = vm.SelectedProject;
                if (project != null)
                    vm.Viewer.UpdateScenes(project.Scenes, project.Fonts, delta);
            }
        }

        private void ScenesTreeViewSelected(object sender, RoutedEventArgs e)
        {
            // Move up the tree view until we reach the TreeViewItem holding the UIScene
            TreeViewItem item = e.OriginalSource as TreeViewItem;
            while (item.DataContext is not UIScene && item != null)
                item = Utilities.GetParentTreeViewItem(item);

            var vm = DataContext as MainViewModel;
            if (vm != null && item != null)
            {
                vm.SelectedScene = item.DataContext as UIScene;
            }
        }
    }
}
