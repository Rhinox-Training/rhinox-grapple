using Rhinox.Grappler.BoneManagement;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Rhinox.Grappler.Recognition
{
    [Serializable]
    public struct RhinoxGesture
    {
        public string name;
        public List<Vector3> fingerPositions;
        public UnityEvent onRecognised;
    }
    public interface IRecognitionService
    {
        void Initialise(BoneManagement.BoneManager boneManager);
        bool GetIsInitialised();
        void SetEnabled(bool newState);
        bool GetIsEnabled();
    }

    public abstract class BaseRecognitionService : MonoBehaviour, IRecognitionService
    {
        public List<RhinoxGesture> LeftHandGestures;
        public List<RhinoxGesture> RightHandGestures;
        
        [HideInInspector]
        public bool IsInitialised;

        [HideInInspector]
        public bool IsEnabled;

        public abstract bool GetIsEnabled();
        public abstract bool GetIsInitialised();
        public abstract void Initialise(BoneManager boneManager);
        public abstract void SetEnabled(bool newState);
    }


}
