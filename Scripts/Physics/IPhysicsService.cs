using Rhinox.Grappler.BoneManagement;
using System;
using UnityEngine;

namespace Rhinox.Grappler.HandPhysics
{    public interface IPhysicsService
    {
        void Initialise(BoneManagement.BoneManager boneManager, HandPhysicsController controller);
        bool GetIsInitialised();
        void SetEnabled(bool newState, Hand handedness);
        bool GetIsEnabled(Hand handedness);
        void ManualUpdate();
        void SetHandLayer(UnityEngine.LayerMask layer);

    }

    [Serializable]
    public abstract class BasePhysicsService : MonoBehaviour, IPhysicsService
    {
        public abstract bool GetIsEnabled(Hand handedness);

        public abstract bool GetIsInitialised();

        public abstract void Initialise(BoneManager boneManager, HandPhysicsController controller);

        public abstract void SetEnabled(bool newState, Hand handedness);

        public abstract void SetHandLayer(LayerMask layer);

        public abstract void ManualUpdate();
    }
}