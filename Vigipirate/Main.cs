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
        public int RELATIONSHIP_SOLDIER = 0;
        public List<List<Ped>> addedGroups = new List<List<Ped>>();
        private int relocateIdleGroupsTickRounds = 5000;

        public Main()
        {
            //Interval = 2000;

            try
            {
                UI.Notify("Vigipirate Mod - Jomtek");
                foreach (Blip blip in World.GetActiveBlips()) blip.Remove();

                RELATIONSHIP_SOLDIER = World.AddRelationshipGroup("MILITARY");
                this.Tick += onTick;
                this.KeyDown += onKeyDown;
            }
            catch (Exception ex)
            {
                var st = new StackTrace(ex, true);
                UI.Notify(st.GetFrame(0).GetFileLineNumber().ToString());
            }
        }

        private void RegisterSoldierGroup(List<Ped> soldiers)
        {
            foreach (Ped soldier in soldiers)
            {
                soldier.AddBlip();
                soldier.CurrentBlip.Color = BlipColor.GreenDark;
            }

            addedGroups.Add(soldiers);
        }

        private Vector3 GenerateSoldierPos(Vector3 basePos, int around)
        {
            return World.GetNextPositionOnSidewalk(basePos.Around(around));
        }

        private void RelocateGroup(List<Ped> soldierGroup, Vector3 basePos, int around)
        {
            if (soldierGroup.Count() != 3)
            {
                addedGroups.Remove(soldierGroup);
                foreach (Ped soldier in soldierGroup) soldier.CurrentBlip.Remove();
                return;
            }
            else
            {
                Ped groupLeader = soldierGroup[0].CurrentPedGroup.Leader;
                groupLeader.Position = GenerateSoldierPos(basePos, around);

                foreach (Ped soldier in soldierGroup) soldier.Position = GenerateSoldierPos(groupLeader.Position, 5);
            }
        }

        private Ped SpawnSoldier(Vector3 position, int pedGroup, bool isLeader = false)
        {
            var ped = World.CreatePed(PedHash.Blackops01SMY, position);
            ped.RelationshipGroup = RELATIONSHIP_SOLDIER;
            ped.Weapons.Give(WeaponHash.Grenade, 3, false, false);
            ped.Weapons.Give(WeaponHash.HeavyPistol, 15, false, true);
            ped.Weapons.Give(WeaponHash.AdvancedRifle, 45, true, true);

            ped.CanSwitchWeapons = true;
            ped.CanWrithe = false;
            //ped.AlwaysKeepTask = true;

            Function.Call(Hash.SET_PED_PATH_CAN_USE_CLIMBOVERS, ped, true);
            //Function.Call(Hash.FREEZE_ENTITY_POSITION, ped, false);

            if (isLeader)
            {
                Function.Call(Hash.SET_PED_AS_GROUP_LEADER, ped, pedGroup);
                ped.Task.WanderAround();
                ped.FiringPattern = FiringPattern.BurstFire;
            }
            else
            {
                Function.Call(Hash.SET_PED_AS_GROUP_MEMBER, ped, pedGroup);
                ped.FiringPattern = FiringPattern.FromGround;
            }

            return ped;
        }

        private List<Ped> SpawnSoldierGroup(int number)
        {
            try
            {

                if (number < 2) throw new ArgumentException("number should be higher than 1");

                Vector3 groupPos = GenerateSoldierPos(Game.Player.Character.Position, 10);
                int pedGroup = Function.Call<int>(Hash.CREATE_GROUP);

                Ped principalSoldier = SpawnSoldier(groupPos, pedGroup, true);

                //principalSoldier.CurrentPedGroup.SeparationRange = 5000;

                var soldierGroup = new List<Ped>() { principalSoldier };

                for (int i = 1; i < number; i++)
                {
                    Ped soldier = SpawnSoldier(GenerateSoldierPos(principalSoldier.Position, 5), pedGroup);
                    // Game.Player.Character.CurrentPedGroup.SeparationRange = 2000;

                    // soldier.Task.FollowToOffsetFromEntity(Game.Player.Character, new Vector3(0, 0,0), 3000, 1);
                    //soldier.AlwaysKeepTask = true;
                    soldierGroup.Add(soldier);
                }

                RegisterSoldierGroup(soldierGroup);
                return soldierGroup;

            }
            catch (Exception ex)
            {
                UI.Notify(ex.ToString());
            }

            return null;
        }

        private void onTick(object sender, EventArgs e)
        {
            foreach (List<Ped> soldierGroup in addedGroups)
            {
                for (int i = 0; i < soldierGroup.Count; i++)
                {
                    Ped soldier = soldierGroup[i];

                    if (!soldier.IsAlive)
                    {
                        soldier.CurrentBlip.Remove();
                        soldierGroup.Remove(soldier);
                    }
                }
            }

            if (relocateIdleGroupsTickRounds == 0)
            {
                foreach (List<Ped> soldierGroup in addedGroups)
                    foreach (Ped soldier in soldierGroup)
                        if (!soldier.IsWalking && !soldier.IsInCombat)
                            RelocateGroup(soldierGroup, Game.Player.Character.Position, 10);

                relocateIdleGroupsTickRounds = 5000;
            }
            else
            {
                relocateIdleGroupsTickRounds--;
            }
        }

        private void onKeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.E)
            {
                SpawnSoldierGroup(3);
            }
        }

    }
}
