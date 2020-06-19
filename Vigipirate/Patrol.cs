using GTA;
using GTA.Math;
using GTA.Native;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Vigipirate
{
    class Patrol
    {
        private List<Ped> soldiers;
        private Ped leader;
        private int soldiersCount;
        private Vector3 basePos;
        private bool spawned = false;

        public Patrol() {

        }

        public void ManageDeadSoldiers()
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

        public bool ManageIdleSoldiers() // Returns true if the patrol was deleted
        {
            if (leader != null)
                if ((!leader.IsWalking && !leader.IsInCombat) || leader.IsInWater)
                    return Relocate();

            foreach (Ped soldier in soldiers)
                if ((!soldier.IsWalking && !soldier.IsInCombat) || soldier.IsInWater) return Relocate();

            return false;
        }

        private void Register(List<Ped> soldiers, Ped leader)
        {
            foreach (Ped soldier in soldiers)
            {
                soldier.AddBlip();
                soldier.CurrentBlip.Color = BlipColor.GreenDark;
                GlobalInfo.soldiersBlipsHandles.Add(soldier.CurrentBlip.Handle);
            }

            this.soldiers = soldiers;
            this.leader = leader;

            GlobalInfo.addedPatrols.Add(this);
        }

        private Vector3 GenerateSoldierPos(Vector3 basePos, int around) =>
            World.GetNextPositionOnSidewalk(basePos.Around(around));

        public void Delete()
        {
            foreach (Ped soldier in soldiers)
            {
                GlobalInfo.soldiersBlipsHandles.Remove(soldier.CurrentBlip.Handle);
                soldier.CurrentBlip.Remove();
                soldier.Delete();
            }

            soldiers.Clear();
            leader = null;
            spawned = false;

            GlobalInfo.addedPatrols.Remove(this);
        }

        public bool Relocate() // Returns true if the patrol was deleted
        {
            if (soldiers.Count() != soldiersCount - 1) // Here I removed 1 from soldiersCount because we don't need to know about the leader
            {
                Delete();
                return true;
            }
            else
            {
                Vector3 newBasePos = GenerateSoldierPos(Game.Player.Character.Position, GlobalInfo.spawnDistance);

                if (leader != null)
                    leader.Position = newBasePos;

                foreach (Ped soldier in soldiers)
                    soldier.Position = GenerateSoldierPos(newBasePos, 2);
            }

            return false;
        }

        private void GiveWeapons(Ped soldier)
        {
            soldier.Weapons.Give(WeaponHash.Grenade, 3, false, false);
            soldier.Weapons.Give(WeaponHash.HeavyPistol, 15, false, true);
            soldier.Weapons.Give(WeaponHash.AdvancedRifle, 45, true, true);
        }

        private Ped SpawnSoldier(Vector3 position, int leaderHandle, bool isLeader = false)
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
                //Function.Call(Hash.SET_PED_AS_GROUP_LEADER, ped, pedGroup);
                ped.Task.WanderAround();
                ped.FiringPattern = FiringPattern.BurstFire;
            }
            else
            {
                //Function.Call(Hash.SET_PED_AS_GROUP_MEMBER, ped, pedGroup);
                Utils.FollowToOffsetFromEntity(ped, leaderHandle, new Vector3(0, 0, 0), 1, -1, 2, true);
                ped.FiringPattern = FiringPattern.FromGround;
            }

            return ped;
        }

        public List<Ped> Spawn(int number, Vector3 basePos)
        {
            if (spawned)
                throw new Exception("Patrol already spawned");

            this.soldiersCount = number;
            this.basePos = basePos;

            try
            {
                if (number < 2) throw new ArgumentException("number should be higher than 1");

                Vector3 groupPos = GenerateSoldierPos(basePos, GlobalInfo.spawnDistance);
                Ped patrolLeader = SpawnSoldier(groupPos, -1, true);

                var soldierGroup = new List<Ped>();

                for (int i = 1; i < number; i++)
                {
                    Ped soldier = SpawnSoldier(GenerateSoldierPos(patrolLeader.Position, 5), patrolLeader.Handle);
                    soldierGroup.Add(soldier);
                }

                spawned = true;

                Register(soldierGroup, patrolLeader);
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
