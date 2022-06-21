using Rhinox.Grappler.BoneManagement;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace Rhinox.Grappler.MaterialManagement
{
    public interface IMaterialService
    {
        void Initialise(BoneManagement.BoneManager boneManager, HandPhysicsController controller);
        bool GetIsInitialised();
        void SetHandMaterial(Hand handedness, Material newMat);
    }

    [Serializable]
    public abstract class BaseMaterialService : MonoBehaviour, IMaterialService
    {
        public abstract bool GetIsInitialised();
        public abstract void Initialise(BoneManager boneManager, HandPhysicsController controller);
        public abstract void SetHandMaterial(Hand handedness, Material newMat);
    }
}
