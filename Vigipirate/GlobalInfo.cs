using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Vigipirate
{
    class GlobalInfo
    {
        public static int RELATIONSHIP_SOLDIER  ;
        public static int RELATIONSHIP_COP = 3;
        public static List<Patrol> addedPatrols = new List<Patrol>();
        public static List<int> soldiersBlipsHandles = new List<int>();

        // Settings
        public static int spawnDistance = 100;
    }
}
