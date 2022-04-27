using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Rhinox.Grappler.BoneManagement
{
    public interface IBoneService
    {
        void Initialise(GameObject controllerParent);
        bool GetIsInitialised();
        List<RhinoxBone> GetBones(Hand hand);
    }
}