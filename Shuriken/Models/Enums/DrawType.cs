using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shuriken.Models
{
    public enum DrawType : uint
    {
        [Description("No Draw")]
        None,

        [Description("Sprite")]
        Sprite,

        [Description("Font")]
        Font
    }
}
