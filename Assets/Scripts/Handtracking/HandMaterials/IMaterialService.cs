using Rhinox.Grappler.BoneManagement;
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
}
