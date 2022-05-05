using Rhinox.Grappler.BoneManagement;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Rhinox.Grappler.Recognition
{
    public class OculusRecognitionService : BaseRecognitionService
    {
        [HideInInspector]
        private List<OVRBone> _leftHandSkeleton;

        [HideInInspector]
        private List<OVRBone> _rightHandSkeleton;

        [SerializeField] private bool _saveLeftPose = false;
        [SerializeField] private bool _saveRightPose = false;


        private void Update()
        {
            HandleSaving();
            if(IsInitialised && IsEnabled)
                HandleRecognition();
        }
        private void HandleSaving()
        {
            if (_saveLeftPose)
            {
                BoneManagement.UnityXRBoneService unityBoneService = this.gameObject.GetComponent<BoneManagement.BoneManager>().GetBoneConvertorService() as BoneManagement.UnityXRBoneService;
                var lhgesture = new RhinoxGesture();
                lhgesture.name = "NEWPOSE";
                List<Vector3> data = new List<Vector3>();
                foreach (var bone in unityBoneService.GetOculusBones(BoneManagement.Hand.Left))
                {
                    data.Add(unityBoneService.GetOculusSkeleton(BoneManagement.Hand.Left).transform.InverseTransformPoint(bone.Transform.position));
                }
                lhgesture.fingerPositions = data;

                base.LeftHandGestures.Add(lhgesture);

                _saveLeftPose = false;
            }

            if (_saveRightPose)
            {
                BoneManagement.UnityXRBoneService unityBoneService = this.gameObject.GetComponent<BoneManagement.BoneManager>().GetBoneConvertorService() as BoneManagement.UnityXRBoneService;
                var rhgesture = new RhinoxGesture();
                rhgesture.name = "NEWPOSE";
                List<Vector3> data = new List<Vector3>();
                foreach (var bone in unityBoneService.GetOculusBones(BoneManagement.Hand.Right))
                {
                    data.Add(unityBoneService.GetOculusSkeleton(BoneManagement.Hand.Right).transform.InverseTransformPoint(bone.Transform.position));
                }
                rhgesture.fingerPositions = data;

                base.RightHandGestures.Add(rhgesture);

                _saveRightPose = false;
            }
        }

        private void HandleRecognition()
        {

        }

        public override bool GetIsEnabled()
        {
            return base.IsEnabled;
        }

        public override bool GetIsInitialised()
        {
            return base.IsInitialised;
        }

        public override void Initialise(BoneManager boneManager)
        {

            base.IsEnabled = true;
            base.IsInitialised = false;

            BoneManagement.UnityXRBoneService unityBoneService = (UnityXRBoneService)boneManager.GetBoneConvertorService();
            if (unityBoneService == null)
            {
                Debug.LogError("Rhinox.Grappler.Recognition.OculusRecognitionService.Initialise() : Cannot initialise OculusRecognitionService, not using an oculus compatible BoneConvertorService");
                return;
            }

            _leftHandSkeleton = unityBoneService.GetOculusBones(Hand.Left);
            _rightHandSkeleton = unityBoneService.GetOculusBones(Hand.Right);
            base.IsInitialised = true;
        }

        public override void SetEnabled(bool newState)
        {
            base.IsEnabled = newState;
        }
    }

}

