using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Rhinox.Grappler.Recognition
{
    
    [Obsolete("Newer and better ways are implemented -> see OculusRecognitionService", true)]
    public class OculusGestureSaver : MonoBehaviour
    {
        public List<RhinoxGesture> leftHandGestures;
        public List<RhinoxGesture> rightHandGestures;
        [SerializeField] private bool _saveLeftPose = false;
        [SerializeField] private bool _saveRightPose = false;

        private void Update()
        {
            Save();
        }

        public void Save()
        {
            BoneManagement.UnityXRBoneService unityBoneService = this.gameObject.GetComponent<BoneManagement.BoneManager>().GetBoneConvertorService() as BoneManagement.UnityXRBoneService;

            if (_saveLeftPose)
            {
                var lhgesture = new RhinoxGesture();
                lhgesture.name = "NEWPOSE";
                List<Vector3> data = new List<Vector3>();
                foreach (var bone in unityBoneService.GetOculusBones(BoneManagement.Hand.Left))
                {
                    data.Add(unityBoneService.GetOculusSkeleton(BoneManagement.Hand.Left).transform.InverseTransformPoint(bone.Transform.position));
                }
                lhgesture.fingerPositions = data;

                leftHandGestures.Add(lhgesture);

                _saveLeftPose = false;
            }

            if (_saveRightPose)
            {
                var rhgesture = new RhinoxGesture();
                rhgesture.name = "NEWPOSE";
                List<Vector3> data = new List<Vector3>();
                foreach (var bone in unityBoneService.GetOculusBones(BoneManagement.Hand.Right))
                {
                    data.Add(unityBoneService.GetOculusSkeleton(BoneManagement.Hand.Right).transform.InverseTransformPoint(bone.Transform.position));
                }
                rhgesture.fingerPositions = data;

                rightHandGestures.Add(rhgesture);

                _saveRightPose = false;
            }

        }
        public List<RhinoxGesture> GetLeftHandGestures()
        {
            return leftHandGestures;
        }
        public List<RhinoxGesture> GetRightHandGestures()
        {
            return rightHandGestures;
        }
    }


}
