#if USING_OVR
using Rhinox.Grappler.BoneManagement;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Rhinox.Grappler.MaterialManagement
{
    [Serializable]
    public class OculusMaterialService : BaseMaterialService
    {
        private bool _isInitialised = false;
        private UnityXRBoneService _unityBoneService = null;

        private SkinnedMeshRenderer _leftHandSkinnedMesh = null;
        private SkinnedMeshRenderer _rightHandSkinnedMesh = null;

        public override void Initialise(BoneManager boneManager, HandPhysicsController controller)
        {
            _unityBoneService = boneManager.GetBoneConvertorService() as UnityXRBoneService;
            if (_unityBoneService == null)
            {
                Debug.LogError("Rhinox.Grappler.MaterialManagement.OculusMaterialService.Initialise() : Cannot initialise OculusMeshBakingService, not using an oculus compatible BoneConvertorService");
                return;
            }

            // retreive the skinned meshes
            _leftHandSkinnedMesh = _unityBoneService.GetOculusSkeleton(Hand.Left).gameObject.GetComponent<SkinnedMeshRenderer>();
            _rightHandSkinnedMesh = _unityBoneService.GetOculusSkeleton(Hand.Right).gameObject.GetComponent<SkinnedMeshRenderer>();

            _isInitialised = true;
        }

        public override bool GetIsInitialised()
        {
            return _isInitialised;
        }

        public override void SetHandMaterial(Hand handedness, Material newMat)
        {
            switch (handedness)
            {
                case Hand.Left:
                    _leftHandSkinnedMesh.material = newMat;
                    break;
                case Hand.Right:
                    _rightHandSkinnedMesh.material = newMat;
                    break;
                case Hand.Both:
                    _leftHandSkinnedMesh.material = newMat;
                    _rightHandSkinnedMesh.material = newMat;
                    break;
            }

        }
    }

}

#endif