using Rhinox.Grappler.BoneManagement;
using System;
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

    [Serializable]
    public abstract class BaseMeshBakingService : MonoBehaviour, IMeshBakingService
    {
        public abstract void BakeMesh(Hand handedness, GameObject parentObj);

        public abstract bool GetIsInitialised();

        public abstract void Initialise(BoneManager boneManager, HandPhysicsController controller);

        public abstract void RemoveMesh(Hand handedness);
    }
}
