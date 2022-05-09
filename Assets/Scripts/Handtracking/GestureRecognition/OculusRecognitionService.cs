using Rhinox.Grappler.BoneManagement;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Rhinox.Grappler.Recognition
{
    public class OculusRecognitionService : BaseRecognitionService
    {
        [SerializeField] private bool _saveLeftPose = false;
        [SerializeField] private bool _saveRightPose = false;
        [SerializeField] private float _detectionTreshHold = 0.01f;
        
        private UnityXRBoneService _unityBoneService;


        private void Update()
        {
            HandleSaving();
            if (IsInitialised && IsEnabled)
            {
                HandleRecognition(ref LeftHandGestures, _unityBoneService.GetOculusSkeleton(Hand.Left), Hand.Left);
                HandleRecognition(ref RightHandGestures, _unityBoneService.GetOculusSkeleton(Hand.Right), Hand.Right);

            }
        }

        /// <summary>
        /// This function solely exists to create new gestures during runtime,
        /// To use this: Click the save booleans to save the current gesture, when you are done adding gestures
        /// do not stop the play simulation, first copy this component and past it when you exit the play mode to keep the 
        /// gesture data
        /// (I know it is very fucked up to use rn but don't worry. soon TM there will be a better way to do this I swear)
        /// </summary>
        private void HandleSaving()
        {
            if (_saveLeftPose)
            {
                var lhgesture = new RhinoxGesture();
                lhgesture.name = "NEWPOSE";
                List<Vector3> data = new List<Vector3>();
                foreach (var bone in _unityBoneService.GetOculusBones(BoneManagement.Hand.Left))
                {
                    data.Add(_unityBoneService.GetOculusSkeleton(BoneManagement.Hand.Left).transform.InverseTransformPoint(bone.Transform.position));
                }
                lhgesture.fingerPositions = data;

                base.LeftHandGestures.Add(lhgesture);

                _saveLeftPose = false;
            }

            if (_saveRightPose)
            {
                var rhgesture = new RhinoxGesture();
                rhgesture.name = "NEWPOSE";
                List<Vector3> data = new List<Vector3>();
                foreach (var bone in _unityBoneService.GetOculusBones(BoneManagement.Hand.Right))
                {
                    data.Add(_unityBoneService.GetOculusSkeleton(BoneManagement.Hand.Right).transform.InverseTransformPoint(bone.Transform.position));
                }
                rhgesture.fingerPositions = data;

                base.RightHandGestures.Add(rhgesture);

                _saveRightPose = false;
            }
        }


        /// <summary>
        /// Goes over all currently known gestures in the system and finds the one that matches the best whilst keeping a detection treshholdin mind
        /// Also handles the unrecognising of previously recognised hand gestured 
        /// </summary>
        /// <param name="gestures">Possible gestures that cna be detected</param>
        /// <param name="skeleton">Skeleton parent of the bones</param>
        /// <param name="handedness">Which hand is it</param>
        private void HandleRecognition(ref List<RhinoxGesture> gestures, OVRSkeleton skeleton, Hand handedness)
        {      
            RhinoxGesture currentGesture = new RhinoxGesture();
            float currentMin = Mathf.Infinity;

            foreach (var gesture in gestures)
            {
                float sumDist = 0;
                bool isDiscarded = false;
                for (int i = 0; i < _unityBoneService.GetOculusBones(handedness).Count; i++)
                {
                    Vector3 currdata = skeleton.transform.InverseTransformPoint(_unityBoneService.GetOculusBones(handedness)[i].Transform.position);
                    float dist = Vector3.Distance(currdata, gesture.fingerPositions[i]);
                    if (dist > _detectionTreshHold)
                    {
                        isDiscarded = true;
                        break;
                    }

                    sumDist += dist;
                }

                if (!isDiscarded && sumDist < currentMin)
                {
                    currentMin = sumDist;
                    currentGesture = gesture;
                }

            }
            currentGesture.onRecognised?.Invoke();

            if (currentGesture != base._previousGesture)
            {
                base._previousGesture.onUnRecognised?.Invoke();
            }
            base._previousGesture = currentGesture;
        }

        public override bool GetIsEnabled()
        {
            return base.IsEnabled;
        }

        public override bool GetIsInitialised()
        {
            return base.IsInitialised;
        }


        /// <summary>
        ///  initialises the recognition service whilst checking if the correct bone service is in place to facilitate the services funcrtionality
        /// </summary>
        /// <param name="boneManager"></param>
        public override void Initialise(BoneManager boneManager)
        {

            base.IsEnabled = true;
            base.IsInitialised = false;
            _unityBoneService = boneManager.GetBoneConvertorService() as UnityXRBoneService;
            if (_unityBoneService == null)
            {
                Debug.LogError("Rhinox.Grappler.Recognition.OculusRecognitionService.Initialise() : Cannot initialise OculusRecognitionService, not using an oculus compatible BoneConvertorService");
                return;
            }
            base.IsInitialised = true;
        }

        public override void SetEnabled(bool newState)
        {
            base.IsEnabled = newState;
        }
    }

}

