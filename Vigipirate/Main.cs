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

namespace Vigipirate
{
    public class Main : Script
    {
        public int RELATIONSHIP_SOLDIER = 0;
        public List<Ped> addedSoldiers = new List<Ped>();

        public Main()
        {
            UI.Notify("Vigipirate Mod - Jomtek");
            foreach (Blip blip in World.GetActiveBlips()) blip.Remove();

            RELATIONSHIP_SOLDIER = World.AddRelationshipGroup("MILITARY");
            this.Tick += onTick;
            this.KeyDown += onKeyDown;
        }

        private void registerSoldier(Ped soldier)
        {
            soldier.AddBlip();
            soldier.CurrentBlip.Color = BlipColor.GreenDark;
            addedSoldiers.Add(soldier);
        }

        private Ped spawnSoldier(Vector3 position, int pedGroup, bool isLeader = false)
        {
            var ped = World.CreatePed(PedHash.Blackops01SMY, position);
            ped.RelationshipGroup = RELATIONSHIP_SOLDIER;
            ped.Weapons.Give(WeaponHash.AdvancedRifle, 100, true, true);
            ped.CanSwitchWeapons = true;

            Function.Call(Hash.SET_PED_PATH_CAN_USE_CLIMBOVERS, ped, true);

            if (isLeader) {
                Function.Call(Hash.SET_PED_AS_GROUP_LEADER, ped, pedGroup);
                ped.Task.WanderAround(ped.Position, 150);
            } else
            {
                Function.Call(Hash.SET_PED_AS_GROUP_MEMBER, ped, pedGroup);
            }

            return ped;
        }

        private List<Ped> spawnSoldierGroup(int number)
        {
            try
            {
                if (number < 2) throw new ArgumentException("number should be higher than 1");

                Vector3 groupPos = World.GetNextPositionOnSidewalk(Game.Player.Character.Position.Around(10));
                int pedGroup = Function.Call<int>(Hash.CREATE_GROUP);

                Ped principalSoldier = spawnSoldier(groupPos, pedGroup, true);
                principalSoldier.CurrentPedGroup.SeparationRange = 5000;
                    
                registerSoldier(principalSoldier);

                var soldiersGroup = new List<Ped>() { principalSoldier };

                for (int i = 1; i < number; i++)
                {
                    Ped soldier = spawnSoldier(groupPos.Around(1), pedGroup);
                    registerSoldier(soldier);

                    Function.Call(Hash.FREEZE_ENTITY_POSITION, soldier, false);
                    // Game.Player.Character.CurrentPedGroup.SeparationRange = 2000;

                    // soldier.Task.FollowToOffsetFromEntity(Game.Player.Character, new Vector3(0, 0,0), 3000, 1);
                    //soldier.AlwaysKeepTask = true;
                    soldiersGroup.Add(soldier);
                }
                return soldiersGroup;
            } catch (Exception ex)
            {
                UI.Notify(ex.Message);
            }

            return null;
        }

        private void onTick(object sender, EventArgs e)
        {
            for (int i = 0; i < addedSoldiers.Count; i++)
            {
                Ped soldier = addedSoldiers[i];
                if (!soldier.IsAlive)
                {
                    soldier.CurrentBlip.Remove();
                    addedSoldiers.Remove(soldier);
                }
            }

        }

        private void onKeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.E)
            {
                spawnSoldierGroup(3);
            }
        }

    }
}
