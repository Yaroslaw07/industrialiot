using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Industrialiot.Lib.Data
{
    internal class MethodUserContext
    {
        public string deviceName { get; set; }

        public MethodUserContext(string deviceName) { this.deviceName = deviceName; }
    }
}
