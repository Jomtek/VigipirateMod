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
            return World.GetDistance(new Vector3(-198, -905, 29), position) > 1100;
        }

        private static int SoldiersAroundPlayer()
        {
            int result = 0;
            foreach (Blip blip in World.GetActiveBlips())
            {
                if (GlobalInfo.soldiersBlipsHandles.Contains(blip.Handle))
                {
                    Vector3 blipPos = blip.Position;
                    if (World.GetDistance(Game.Player.Character.Position, blipPos) < 250) result++;
                }
            }
            return result;     
        }

        public static void Tick()
        {
            if (SoldiersAroundPlayer() == 0)
            {
                for (int i = 0; i < 2; i++) {
                    Patrol patrol = new Patrol();
                    patrol.Spawn(3, Game.Player.Character.Position);
                }
            } 
                
        }
    }
}
