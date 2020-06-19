using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GTA;
using GTA.Math;
using GTA.Native;
using System.Windows.Forms;
using System.Drawing;
using Microsoft.Win32;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Diagnostics;
using NativeUI;

namespace Vigipirate
{
    public class Main : Script
    {
        private int relocateIdleGroupsTickRounds = 5000;
        private int spawnPatrolsTickRounds = 1000;

        public Main()
        {
            UI.Notify("Vigipirate Mod (by Jomtek)");
                
            foreach (Blip blip in World.GetActiveBlips())
                blip.Remove();

            GlobalInfo.RELATIONSHIP_SOLDIER = World.AddRelationshipGroup("ARMY");   

            World.SetRelationshipBetweenGroups(Relationship.Companion, GlobalInfo.RELATIONSHIP_SOLDIER, GlobalInfo.RELATIONSHIP_COP);
            World.SetRelationshipBetweenGroups(Relationship.Companion, GlobalInfo.RELATIONSHIP_COP, GlobalInfo.RELATIONSHIP_SOLDIER);
            
            this.Tick += onTick;
            this.KeyDown += onKeyDown;
        }

        private void onTick(object sender, EventArgs e)
        {
            foreach (Patrol patrol in GlobalInfo.addedPatrols)
                patrol.ManageDeadSoldiers();

            if (relocateIdleGroupsTickRounds == 0)
            {
                int counter = 0;
                for (int i = 0; i < GlobalInfo.addedPatrols.Count; i++)
                {
                    Patrol patrol = GlobalInfo.addedPatrols[counter];
                    if (patrol.ManageIdleSoldiers()) counter--;
                    counter++;
                }

                relocateIdleGroupsTickRounds = 5000;
            }
            else
            {
                relocateIdleGroupsTickRounds--;
            }


            if (spawnPatrolsTickRounds == 0)
            {
                SpawnPatrols.Tick();
                spawnPatrolsTickRounds = 1000;
            }
            else
            {
                spawnPatrolsTickRounds--;
            }

        }

        private void onKeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.E)
            {
                //Patrol patrol = new Patrol();
                //patrol.Spawn(3, Game.Player.Character.Position);

                //if (World.GetDistance(new Vector3(-198, -905, 29), Game.Player.Character.Position) > 1100)
                
                
            }
        }
            
    }
}
