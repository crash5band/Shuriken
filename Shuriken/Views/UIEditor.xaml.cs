using System;
using System.IO;
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
using System.Collections.ObjectModel;
using Shuriken.Models;
using Shuriken.Rendering;
using Shuriken.ViewModels;
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
        }

        private void glControlRender(TimeSpan obj)
        {
            GL.ClearColor(0.2f, 0.2f, 0.2f, 0.0f);
            GL.Clear(ClearBufferMask.ColorBufferBit);

            var vm = DataContext as MainViewModel;
            float delta = obj.Milliseconds / 1000.0f * 60.0f;

            if (vm != null)
            {
                if (vm.Scenes != null)
                    vm.SceneManager.UpdateScenes(vm.Scenes, vm.UIFonts, delta);
            }
        }
    }
}
