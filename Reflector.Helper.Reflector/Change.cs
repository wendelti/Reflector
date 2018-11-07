using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Reflector.Helper.Reflector
{
    public class Change
    {
        public string Field { get; set; }
        public object OldValue { get; set; }
        public object NewValue { get; set; }
    }
}
