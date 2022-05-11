using Rhinox.Grappler.BoneManagement;

namespace Rhinox.Grappler.HandPhysics
{    public interface IPhysicsService
    {
        void Initialise(BoneManagement.BoneManager boneManager, HandPhysicsController controller);
        bool GetIsInitialised();
        void SetEnabled(bool newState, Hand handedness);
        bool GetIsEnabled(Hand handedness);
        void Update();
        void SetHandLayer(UnityEngine.LayerMask layer);

    }
}