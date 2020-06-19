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
    class Utils
    {
        public static void FollowToOffsetFromEntity(Ped _ped, int targetHandle, Vector3 offset, float movementSpeed, int timeout = -1, float distanceToFollow = 10f, bool persistFollowing = true)
        {
            Function.Call(Hash.TASK_FOLLOW_TO_OFFSET_OF_ENTITY, _ped.Handle, targetHandle, offset.X, offset.Y, offset.Z, movementSpeed, timeout, distanceToFollow, persistFollowing);
        }
    }
}
