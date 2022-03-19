using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shuriken.Models
{
    public interface ICastContainer
    {
        public void AddCast(UICast cast);
        public void RemoveCast(UICast cast);
    }
}
