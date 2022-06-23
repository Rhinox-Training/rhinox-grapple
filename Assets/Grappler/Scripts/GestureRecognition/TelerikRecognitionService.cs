#if USING_TELERIK

using Rhinox.Grappler.BoneManagement;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Rhinox.Lightspeed;

namespace Rhinox.Grappler.Recognition
{
    public class TelerikRecognitionService : BaseRecognitionService
    {
        [SerializeField] private bool _saveLeftPose = false;
        [SerializeField] private bool _saveRightPose = false;

        [SerializeField] private bool _useDetectionTreshHold = true;
        [SerializeField] private float _detectionTreshHold = 0.01f;

        [SerializeField] private bool _useRotationalDetectionTreshHold = true;
        [SerializeField] private float _rotationalDetectionTreshHold = 5.0f;

        private TelerikBoneService _telerikBoneService;

        private void Update()
        {
            if (base.IsInitialised && base.IsEnabled && _telerikBoneService != null)
            {
                HandleSaving();

                HandleRecognition(ref LeftHandGestures, _telerikBoneService.GetBones(Hand.Left), Hand.Left);
                HandleRecognition(ref RightHandGestures, _telerikBoneService.GetBones(Hand.Right), Hand.Right);
            }
        }

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

        /// <summary>
        ///  initialises the recognition service whilst checking if the correct bone service is in place to facilitate the services funcrtionality
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
            base.IsInitialised = true;
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