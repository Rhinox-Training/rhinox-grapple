using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Rhinox.Grappler
{
    [CreateAssetMenu(fileName = "HandPhysicsSetting", menuName = "Grappler/HandPhysicsSetting", order = 1)]
    public class HandPhysicsSettings : ScriptableObject
    {
        //public List<HandPhysics.IPhysicsService> PhysicsServices;
        //public MeshBaking.IMeshBakingService MeshBakingService;

        public MaterialManagement.OculusMaterialService MaterialService;
    }

}
