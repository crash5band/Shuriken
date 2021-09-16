using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Numerics;
using System.Windows;
using System.Windows.Controls;
using HandyControl.Controls;
using System.Windows.Data;
using Shuriken.Controls;
using Shuriken.Converters;

namespace Shuriken.Editors
{
    public class Vector2Editor : PropertyEditorBase
    {
        public override FrameworkElement CreateElement(PropertyItem propertyItem)
        {
            var editor = new Vector2Control();
            editor.DataContext = editor;

            return editor;
        }

        public override DependencyProperty GetDependencyProperty()
        {
            return Vector2Control.ValueProperty;
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
