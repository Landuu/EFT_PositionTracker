using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EFT_Posreader
{
    internal class Storage
    {
        public bool SwitchRegionSelect = false;
        public ScreenRegion ScreenRegion = new();
        public bool isRunning = false;
        public bool debugMode = false;
    }
}
