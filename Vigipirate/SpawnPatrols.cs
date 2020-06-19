using GTA;
using GTA.Math;
using GTA.Native;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Vigipirate
{
    class SpawnPatrols
    {

        private static bool IsInCity(Vector3 position)
        {
            return World.GetDistance(new Vector3(-198, -905, 29), position) < 1100;
        }

        private static int SoldiersAroundPlayer()
        {
            int result = 0;
            foreach (Blip blip in World.GetActiveBlips())
            {
                if (GlobalInfo.soldiersBlipsHandles.Contains(blip.Handle))
                {
                    result++;
                }
            }
            return result;     
        }

        public static void Tick()
        {
            if (SoldiersAroundPlayer() == 0 && IsInCity(Game.Player.Character.Position))
            {
                for (int i = 0; i < 5; i++) {
                    Patrol patrol = new Patrol();
                    patrol.Spawn(3, Game.Player.Character.Position);
                }
            } 
                
        }
    }
}
