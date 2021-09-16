using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XNCPLib.XNCP
{
    public struct CastHierarchyTreeNode
    {
        public int ChildIndex { get; set; }
        public int NextIndex { get; set; }

        public CastHierarchyTreeNode(int child, int next)
        {
            ChildIndex = child;
            NextIndex = next;
        }
    }
}
