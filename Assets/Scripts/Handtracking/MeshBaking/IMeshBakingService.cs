using Rhinox.Grappler.BoneManagement;
using UnityEngine;

namespace Rhinox.Grappler.MeshBaking
{
    public interface IMeshBakingService
    {
        void Initialise(BoneManagement.BoneManager boneManager, HandPhysicsController controller);
        bool GetIsInitialised();
        void BakeMesh(Hand handedness, GameObject parentObj);
        void RemoveMesh(Hand handedness);

    }

}