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
    class Patrol
    {
        private List<Ped> soldiers;
        private PedGroup pedGroup;
        private bool spawned = false;

        public Patrol() {

        }

        public void RefreshDeadSoldiers()
        {
            for (int i = 0; i < soldiers.Count; i++)
            {
                Ped soldier = soldiers[i];
                if (!soldier.IsAlive)
                {
                    GlobalInfo.soldiersBlipsHandles.Remove(soldier.CurrentBlip.Handle);
                    soldier.CurrentBlip.Remove();
                    soldiers.Remove(soldier);
                }
            }
        }

        public void RelocateIdleSoldiers()
        {
            foreach (Ped soldier in soldiers)
                if ((!soldier.IsWalking && !soldier.IsInCombat) || soldier.IsInWater || soldier.IsIdle) Relocate();
        }

        private void Register(List<Ped> soldiers)
        {
            foreach (Ped soldier in soldiers)
            {
                soldier.AddBlip();
                soldier.CurrentBlip.Color = BlipColor.GreenDark;
                GlobalInfo.soldiersBlipsHandles.Add(soldier.CurrentBlip.Handle);
            }

            this.soldiers = soldiers;
            pedGroup = soldiers[0].CurrentPedGroup;

            GlobalInfo.addedPatrols.Add(this);
        }

        private Vector3 GenerateSoldierPos(Vector3 basePos, int around)
        {
            return World.GetNextPositionOnSidewalk(basePos.Around(around));
        }
        public void Delete()
        {
            GlobalInfo.addedPatrols.Remove(this);
            foreach (Ped soldier in soldiers)
            {
                GlobalInfo.soldiersBlipsHandles.Remove(soldier.CurrentBlip.Handle);
                soldier.CurrentBlip.Remove();
            }
        }

        private void Relocate()
        {
            if (soldiers.Count() != 3)
            {
                Delete();
            }
            else
            {
                Ped groupLeader = pedGroup.Leader;
                groupLeader.Position = GenerateSoldierPos(Game.Player.Character.Position, 10);

                foreach (Ped soldier in soldiers)
                    soldier.Position = GenerateSoldierPos(groupLeader.Position, 5);
            }
        }

        private void GiveWeapons(Ped soldier)
        {
            soldier.Weapons.Give(WeaponHash.Grenade, 3, false, false);
            soldier.Weapons.Give(WeaponHash.HeavyPistol, 15, false, true);
            soldier.Weapons.Give(WeaponHash.AdvancedRifle, 45, true, true);
        }

        private Ped SpawnSoldier(Vector3 position, int pedGroup, bool isLeader = false)
        {
            var ped = World.CreatePed(PedHash.Blackops01SMY, position);
            ped.RelationshipGroup = GlobalInfo.RELATIONSHIP_SOLDIER;

            ped.CanSwitchWeapons = true;
            ped.CanWrithe = false;
            //ped.AlwaysKeepTask = true;

            GiveWeapons(ped);

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

        public List<Ped> Spawn(int number, Vector3 basePos)
        {
            if (spawned)
                throw new Exception("Patrol already spawned");
            
            try
            {
                if (number < 2) throw new ArgumentException("number should be higher than 1");

                Vector3 groupPos = GenerateSoldierPos(basePos, 10);
                int pedGroup = Function.Call<int>(Hash.CREATE_GROUP);

                Ped principalSoldier = SpawnSoldier(groupPos, pedGroup, true);
                //principalSoldier.CurrentPedGroup.SeparationRange = 2000;

                var soldierGroup = new List<Ped>() { principalSoldier };

                for (int i = 1; i < number; i++)
                {
                    Ped soldier = SpawnSoldier(GenerateSoldierPos(principalSoldier.Position, 5), pedGroup);
                    soldierGroup.Add(soldier);
                }

                Register(soldierGroup);
                return soldierGroup;

            }
            catch (Exception ex)
            {
                UI.Notify(ex.ToString());
            }

            return null;
        }
    }
}
