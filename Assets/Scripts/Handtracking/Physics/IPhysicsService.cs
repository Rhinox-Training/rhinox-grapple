namespace Rhinox.Grappler.HandPhysics
{    public interface IPhysicsService
    {
        void Initialise(BoneManagement.BoneManager boneManager);
        bool GetIsInitialised();
        void SetEnabled(bool newState);
        bool GetIsEnabled();
        void Update();
        void SetHandLayer(UnityEngine.LayerMask layer);

    }
}