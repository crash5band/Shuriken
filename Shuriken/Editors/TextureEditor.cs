using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Numerics;
using System.Windows;
using HandyControl.Controls;
using System.Windows.Data;
using Shuriken.Controls;
using Shuriken.Converters;

namespace Shuriken.Editors
{
    public class TextureEditor : PropertyEditorBase
    {
        public override FrameworkElement CreateElement(PropertyItem propertyItem)
        {
            return new ComboBox();
        }

        public override DependencyProperty GetDependencyProperty()
        {
            return ComboBox.SelectedItemProperty;
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
