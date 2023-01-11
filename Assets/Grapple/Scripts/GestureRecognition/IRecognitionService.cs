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
        public Quaternion handRotation;
        public UnityEvent onRecognised;
        public UnityEvent onUnRecognised;

        public override bool Equals(object obj)
        {
            var objectToCompareWith = (RhinoxGesture)obj;

            // are we dealing with two null gestures?
            if (fingerPositions == null && objectToCompareWith.fingerPositions == null)
                return true;

            // is one of them null?
            if (fingerPositions == null || objectToCompareWith.fingerPositions == null)
                return false;

            // are the finger positions correct?
            if (fingerPositions.Count == objectToCompareWith.fingerPositions.Count)
            {
                for (int i = 0; i < fingerPositions.Count; i++)
                    if (fingerPositions[i] != objectToCompareWith.fingerPositions[i])
                        return false;
            }
            return true;
        }

        public static bool operator ==(RhinoxGesture gesture_one, RhinoxGesture gesture_two)
        {
            return gesture_one.Equals(gesture_two);
        }

        public static bool operator !=(RhinoxGesture gesture_one, RhinoxGesture gesture_two)
        {
            return !gesture_one.Equals(gesture_two);
        }

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

        [Header("Gestures")]
        public List<RhinoxGesture> LeftHandGestures;
        public List<RhinoxGesture> RightHandGestures;


        [HideInInspector] public RhinoxGesture? _previousGestureLeftHand = null;
        [HideInInspector] public RhinoxGesture? _currentGestureLeftHand = null;

        [HideInInspector] public RhinoxGesture? _previousGestureRightHand = null;
        [HideInInspector] public RhinoxGesture? _currentGestureRightHand = null;

        public UnityEvent OnLeftHandGestureRecognised = new UnityEvent();
        public UnityEvent OnRightHandGestureRecognised = new UnityEvent();


        [HideInInspector]
        public bool IsInitialised = false;

        [HideInInspector]
        public bool IsEnabled = true;

        public abstract bool GetIsEnabled();
        public abstract bool GetIsInitialised();
        public abstract void Initialise(BoneManager boneManager);
        public abstract void SetEnabled(bool newState);
    }


}
