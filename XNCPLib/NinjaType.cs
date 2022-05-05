using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XNCPLib
{
    public enum NinjaType
    {
        [Description("Unknown")]
        Unknown, 

        [Description("Sonic Generations")]
        BlueBlur,

        [Description("Sonic Unleashed")]
        SWA,

        [Description("SONIC THE HEDGEHOG")]
        SonicNext,
    }

    public class Filters
    {
        public static readonly string NinjaTypeFilter
            = "Sonic Generations (*.xncp)|*.xncp"
            + "|Sonic Unleashed (*.yncp)|*.yncp"
            + "|SONIC THE HEDGEHOG (*.xncp)|*.xncp"
            ;
    }
}
