using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using HandyControl.Controls;
using Shuriken.Controls;
using Shuriken.Converters;

namespace Shuriken.Editors
{
    public class ColorEditor : PropertyEditorBase
    {
        public override FrameworkElement CreateElement(PropertyItem propertyItem)
        {
            var editor = new ColorControl();
            editor.DataContext = editor;

            return editor;
        }

        public override DependencyProperty GetDependencyProperty()
        {
            return ColorControl.ValueProperty;
        }

        public override void CreateBinding(PropertyItem propertyItem, DependencyObject element)
        {
            BindingOperations.SetBinding(element, GetDependencyProperty(),
                new Binding($"{propertyItem.PropertyName}")
                {
                    Source = propertyItem.Value,
                    Mode = BindingMode.TwoWay,
                    UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
                });
        }
    }
}
