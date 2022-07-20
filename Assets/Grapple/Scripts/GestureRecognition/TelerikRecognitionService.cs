#if USING_TELERIK

using Rhinox.Grappler.BoneManagement;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Rhinox.Lightspeed;
using UnityEngine.Events;
using Telerik.Unity.XR;
using System.Linq;

namespace Rhinox.Grappler.Recognition
{
    [Serializable]
    public class TelerikPinchGesture
    {
        public string name;

        [Header("Thumb")]
        public bool CheckThumb;
        [Range(0, 1)]
        public float ThumbPinchMinTreshhold;
        [Range(0,1)]
        public float ThumbPinchMaxTreshhold;

        [Header("Index")]
        public bool CheckIndex;
        [Range(0, 1)]
        public float IndexPinchMinTreshhold;
        [Range(0, 1)]
        public float IndexPinchMaxTreshhold;

        [Header("Middle")]
        public bool CheckMiddle;
        [Range(0, 1)]
        public float MiddlePinchMinTreshhold;
        [Range(0, 1)]
        public float MiddlePinchMaxTreshhold;

        [Header("Ring")]
        public bool CheckRing;
        [Range(0, 1)]
        public float RingPinchMinTreshhold;
        [Range(0, 1)]
        public float RingPinchMaxTreshhold;

        [Header("Pinky")]
        public bool Checkpinky;
        [Range(0, 1)]
        public float PinkyPinchMinTreshhold;
        [Range(0, 1)]
        public float PinkyPinchMaxTreshhold;

        [SerializeField]
        private UnityEvent onActivated;

        [SerializeField]
        private UnityEvent onDeActivated;

        private bool prevState;

        public void SetActive(bool newState)
        {
            if (prevState == newState)
                return;

            if (newState)
                onActivated?.Invoke();
            else onDeActivated?.Invoke();

            prevState = newState;
        }
    }

    public class TelerikRecognitionService : BaseRecognitionService
    {

        [Header("Gesture saving")]
        [SerializeField] private bool _saveLeftPose = false;
        [SerializeField] private bool _saveRightPose = false;

        [Header("Gesture settings")]
        [SerializeField] private bool _useDetectionTreshHold = true;
        [SerializeField] private float _detectionTreshHold = 0.01f;

        [SerializeField] private bool _useRotationalDetectionTreshHold = true;
        [SerializeField] private float _rotationalDetectionTreshHold = 5.0f;


        [Header("Telerik pinch gestures")]
        public List<TelerikPinchGesture> LeftHandTelerikPinchGestures;
        public List<TelerikPinchGesture> RightHandTelerikPinchGestures;
        public UnityEvent OnLeftHandTelerikPinchRecognised = new UnityEvent();
        public UnityEvent OnRightHandTelerikPinchRecognised = new UnityEvent();

        private TelerikBoneService _telerikBoneService;

        private Telerik.Unity.XR.Rig.Tracking.TrackingHandPose _leftTrackingHand;
        private Telerik.Unity.XR.Rig.Tracking.TrackingHandPose _rightTrackingHand;

        [Header("DEBUG")]
        [SerializeField] private bool _enableDebug = false;

        /// <summary>
        ///  initialises the recognition service whilst checking if the correct bone service is in place to facilitate the services funcrtionality
        ///  it also gets the tracking hands for telerik pinch recognition
        /// </summary>
        /// <param name="boneManager"></param>
        public override void Initialise(BoneManager boneManager)
        {
            base.IsEnabled = true;
            base.IsInitialised = false;

            _telerikBoneService = boneManager.GetBoneConvertorService() as TelerikBoneService;
            if (_telerikBoneService == null)
            {
                Debug.LogError("Rhinox.Grappler.Recognition.TelerikRecognitionService.Initialise() : Cannot initialise TelerikRecognitionService, not using an telerik compatible BoneConvertorService");
                return;
            }

            var trackingHands = this.gameObject.GetComponentsInChildren<Telerik.Unity.XR.Rig.Tracking.TrackingHandPose>();

            _rightTrackingHand = trackingHands.First(hand => Telerik.Unity.XR.Rig.Input.HandInput.Right == hand.hand);
            if (_rightTrackingHand == null)
            {
                Debug.LogError("Rhinox.Grappler.Recognition.TelerikRecognitionService.Initialise() : Cannot find RightTrackingHand");
                return;
            }

            _leftTrackingHand = trackingHands.First(hand => Telerik.Unity.XR.Rig.Input.HandInput.Left == hand.hand);
            if (_leftTrackingHand == null)
            {
                Debug.LogError("Rhinox.Grappler.Recognition.TelerikRecognitionService.Initialise() : Cannot find LeftTrackingHand");
                return;
            }
            base.IsInitialised = true;
        }

        private void Update()
        {
            if (base.IsInitialised && base.IsEnabled && _telerikBoneService != null)
            {
                HandleSaving();

                HandleRecognition(ref LeftHandGestures, _telerikBoneService.GetBones(Hand.Left), Hand.Left);
                HandleRecognition(ref RightHandGestures, _telerikBoneService.GetBones(Hand.Right), Hand.Right);

                HandleTelerikPinchRecognition(ref LeftHandTelerikPinchGestures, _leftTrackingHand,Hand.Left);
                HandleTelerikPinchRecognition(ref RightHandTelerikPinchGestures, _rightTrackingHand, Hand.Right);
            }
        }

        /// <summary>
        /// when a saving boolean is active this function will create a gesture, snapshotting the current
        /// bone positions of the hands in question.
        /// </summary>
        private void HandleSaving()
        {
            if (_saveLeftPose)
            {
                var lhgesture = new RhinoxGesture();
                lhgesture.name = "NEWPOSE";
                List<Vector3> data = new List<Vector3>();
                foreach (var bone in _telerikBoneService.GetBones(BoneManagement.Hand.Left))
                {
                    data.Add(_telerikBoneService.GetBones(Hand.Left)[0].BoneTransform.InverseTransformPoint(bone.BoneTransform.position));
                }
                lhgesture.fingerPositions = data;
                lhgesture.handRotation = _telerikBoneService.GetBones(Hand.Left)[0].BoneTransform.rotation;
                base.LeftHandGestures.Add(lhgesture);

                _saveLeftPose = false;
            }

            if (_saveRightPose)
            {
                var rhgesture = new RhinoxGesture();
                rhgesture.name = "NEWPOSE";
                List<Vector3> data = new List<Vector3>();
                foreach (var bone in _telerikBoneService.GetBones(BoneManagement.Hand.Right))
                {
                    data.Add(_telerikBoneService.GetBones(Hand.Right)[0].BoneTransform.InverseTransformPoint(bone.BoneTransform.position));
                }
                rhgesture.fingerPositions = data;
                rhgesture.handRotation = _telerikBoneService.GetBones(Hand.Right)[0].BoneTransform.rotation;
                base.RightHandGestures.Add(rhgesture);

                _saveRightPose = false;
            }
        }

        /// <summary>
        /// handles the recognition for base gesture recognition using bone positions
        /// </summary>
        /// <param name="gestures"></param>
        /// <param name="bones"></param>
        /// <param name="handedness"></param>
        private void HandleRecognition(ref List<RhinoxGesture> gestures, List<RhinoxBone> bones, Hand handedness)
        {
            RhinoxGesture currentGesture = new RhinoxGesture();
            float currentMin = Mathf.Infinity;
            float currentRotationalMin = Mathf.Infinity;

            foreach (var gesture in gestures)
            {
                float sumDist = 0;
                float rotationSumDist = 0;
                bool isDiscarded = false;
                for (int i = 0; i < bones.Count; i++)
                {
                    Vector3 currdata = bones[0].BoneTransform.InverseTransformPoint(bones[i].BoneTransform.position);
                    float dist = Vector3.Distance(currdata, gesture.fingerPositions[i]);
                    var rotationalDist = (bones[0].BoneTransform.rotation.AngleTo(gesture.handRotation));       
                    
                    if ((_useDetectionTreshHold && dist > _detectionTreshHold) || (_useRotationalDetectionTreshHold && rotationalDist > _rotationalDetectionTreshHold))
                    {
                        isDiscarded = true;
                        break;
                    }

                    sumDist += dist;
                    rotationSumDist += rotationalDist;
                }

                if (!isDiscarded && sumDist < currentMin && rotationSumDist < currentRotationalMin)
                {
                    currentMin = sumDist;
                    currentGesture = gesture;
                }
            }

            switch (handedness)
            {
                case Hand.Left:
                    _currentGestureLeftHand = currentGesture;
                    if (_currentGestureLeftHand != base._previousGestureLeftHand)
                    {
                        base.OnLeftHandGestureRecognised?.Invoke();
                        _currentGestureLeftHand.onRecognised?.Invoke();
                        base._previousGestureLeftHand.onUnRecognised?.Invoke();
                    }

                    if (_currentGestureLeftHand.name == null)
                        return;
                    base._previousGestureLeftHand = _currentGestureLeftHand;
                    break;
                case Hand.Right:
                    _currentGestureRightHand = currentGesture;
                    if (_currentGestureRightHand != base._previousGestureRightHand)
                    {
                        base.OnRightHandGestureRecognised?.Invoke();
                        _currentGestureRightHand.onRecognised?.Invoke();
                        base._previousGestureRightHand.onUnRecognised?.Invoke();
                    }

                    if (_currentGestureRightHand.name == null)
                        return;


                    base._previousGestureRightHand = _currentGestureRightHand;
                    break;
            }
        }


        private void HandleTelerikPinchRecognition(ref List<TelerikPinchGesture> pinchGestures, Telerik.Unity.XR.Rig.Tracking.TrackingHandPose trackingHand , Hand handedness)
        {
            if (_enableDebug)
            {
                Debug.Log(handedness.ToString() + "Hand -> \nThumb:" + trackingHand.thumb.open + "\n" +
                    "Index  :" + trackingHand.indexFinger.open + "\n" +
                    "Middle :" + trackingHand.middleFinger.open + "\n" +
                    "Ring   :" + trackingHand.ringFinger.open + "\n" +
                    "Pinky  :" + trackingHand.pinkyFinger.open + "\n");
            }

            foreach (var pinchGesture in pinchGestures)
            {
                if (pinchGesture.CheckThumb && ((trackingHand.thumb.open > pinchGesture.ThumbPinchMaxTreshhold) || (trackingHand.thumb.open <= pinchGesture.ThumbPinchMinTreshhold)))
                {
                    pinchGesture.SetActive(false);
                    continue;
                }
                if (pinchGesture.CheckIndex && ((trackingHand.indexFinger.open > pinchGesture.IndexPinchMaxTreshhold) || (trackingHand.indexFinger.open <= pinchGesture.IndexPinchMinTreshhold)))
                {
                    pinchGesture.SetActive(false);
                    continue;
                }
                if (pinchGesture.CheckMiddle && ((trackingHand.middleFinger.open > pinchGesture.MiddlePinchMaxTreshhold) || (trackingHand.middleFinger.open <= pinchGesture.MiddlePinchMinTreshhold)))
                {
                    pinchGesture.SetActive(false);
                    continue;
                }
                if (pinchGesture.CheckRing && ((trackingHand.ringFinger.open > pinchGesture.RingPinchMaxTreshhold) || (trackingHand.ringFinger.open <= pinchGesture.RingPinchMinTreshhold)))
                {
                    pinchGesture.SetActive(false);
                    continue;
                }
                if (pinchGesture.Checkpinky && ((trackingHand.pinkyFinger.open > pinchGesture.PinkyPinchMaxTreshhold) || (trackingHand.pinkyFinger.open <= pinchGesture.PinkyPinchMinTreshhold)))
                {
                    pinchGesture.SetActive(false);
                    continue;
                }

                pinchGesture.SetActive(true);
            }
        }

        public override bool GetIsInitialised()
        {
            return base.IsInitialised;
        }

        public override void SetEnabled(bool newState)
        {
            base.IsEnabled = newState;
        }
        public override bool GetIsEnabled()
        {
            return base.IsEnabled;
        }
    }
}

#endif