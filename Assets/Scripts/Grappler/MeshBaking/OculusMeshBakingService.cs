using Rhinox.Grappler.BoneManagement;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Rhinox.Grappler.MeshBaking
{
    public class OculusMeshBakingService : BaseMeshBakingService
    {
        private bool _isInitialised = false;

        private SkinnedMeshRenderer _leftHandSkinnedMesh = null;
        GameObject _leftHandBakedObj = null;

        private SkinnedMeshRenderer _rightHandSkinnedMesh = null;
        GameObject _rightHandBakedObj = null;
        
        private UnityXRBoneService _unityBoneService = null;
        private HandPhysicsController _controller = null;

        public override bool GetIsInitialised()
        {
            return _isInitialised;
        }

        public override void Initialise(BoneManager boneManager, HandPhysicsController controller)
        {
            _controller = controller;

            _unityBoneService = boneManager.GetBoneConvertorService() as UnityXRBoneService;
            if (_unityBoneService == null)
            {
                Debug.LogError("Rhinox.Grappler.MeshBaking.OculusMeshBakingService.Initialise() : Cannot initialise OculusMeshBakingService, not using an oculus compatible BoneConvertorService");
                return;
            }

            // retreive the skinned meshes
            _leftHandSkinnedMesh = _unityBoneService.GetOculusSkeleton(Hand.Left).gameObject.GetComponent<SkinnedMeshRenderer>();
            _rightHandSkinnedMesh = _unityBoneService.GetOculusSkeleton(Hand.Right).gameObject.GetComponent<SkinnedMeshRenderer>();

            _isInitialised = true;
        }


        public override void BakeMesh(Hand handedness, GameObject parentObj)
        {
            MeshRenderer meshRenderer = null;
            MeshFilter meshFilter = null;

            switch (handedness)
            {
                case Hand.Left:
                    _leftHandSkinnedMesh.GetComponent<OVRMeshRenderer>().enabled = false;
                    _leftHandSkinnedMesh.enabled = false;

                    _leftHandBakedObj = new GameObject("LeftHandBakedMesh");
                    meshRenderer = _leftHandBakedObj.AddComponent<MeshRenderer>();
                    meshRenderer.material = _controller.BakedHandMaterial;
                    meshFilter = _leftHandBakedObj.AddComponent<MeshFilter>();
                    _leftHandSkinnedMesh.BakeMesh(meshFilter.mesh);


                    _leftHandBakedObj.transform.position = _unityBoneService.GetOculusSkeleton(Hand.Left).gameObject.transform.parent.parent.transform.position;
                    _leftHandBakedObj.transform.rotation = _unityBoneService.GetOculusSkeleton(Hand.Left).gameObject.transform.parent.parent.transform.rotation;
                    _leftHandBakedObj.transform.parent = parentObj.transform;               

                    break;
                case Hand.Right:
                    _rightHandSkinnedMesh.GetComponent<OVRMeshRenderer>().enabled = false;
                    _rightHandSkinnedMesh.enabled = false;

                    _rightHandBakedObj = new GameObject("RightHandBakedMesh");
                    meshRenderer = _rightHandBakedObj.AddComponent<MeshRenderer>();
                    meshRenderer.material = _controller.BakedHandMaterial;
                    meshFilter = _rightHandBakedObj.AddComponent<MeshFilter>();
                    _rightHandSkinnedMesh.BakeMesh(meshFilter.mesh);

                    _rightHandBakedObj.transform.position = _unityBoneService.GetOculusSkeleton(Hand.Right).gameObject.transform.parent.parent.transform.position;
                    _rightHandBakedObj.transform.rotation = _unityBoneService.GetOculusSkeleton(Hand.Right).gameObject.transform.parent.parent.transform.rotation;
                    _rightHandBakedObj.transform.parent = parentObj.transform;
                    break;
                case Hand.Both:
                    _leftHandSkinnedMesh.GetComponent<OVRMeshRenderer>().enabled = false;
                    _leftHandSkinnedMesh.enabled = false;

                    _leftHandBakedObj = new GameObject("LeftHandBakedMesh");
                    meshRenderer = _leftHandBakedObj.AddComponent<MeshRenderer>();
                    meshRenderer.material = _controller.BakedHandMaterial;
                    meshFilter = _leftHandBakedObj.AddComponent<MeshFilter>();
                    _leftHandSkinnedMesh.BakeMesh(meshFilter.mesh);

                    _leftHandBakedObj.transform.position = _unityBoneService.GetOculusSkeleton(Hand.Left).gameObject.transform.parent.parent.transform.position;
                    _leftHandBakedObj.transform.rotation = _unityBoneService.GetOculusSkeleton(Hand.Left).gameObject.transform.parent.parent.transform.rotation;
                    _leftHandBakedObj.transform.parent = parentObj.transform;


                    _rightHandSkinnedMesh.GetComponent<OVRMeshRenderer>().enabled = false;
                    _rightHandSkinnedMesh.enabled = false; 
                    
                    _rightHandBakedObj = new GameObject("RightHandBakedMesh");
                    meshRenderer = _rightHandBakedObj.AddComponent<MeshRenderer>();
                    meshRenderer.material = _controller.BakedHandMaterial;
                    meshFilter = _rightHandBakedObj.AddComponent<MeshFilter>();
                    _rightHandSkinnedMesh.BakeMesh(meshFilter.mesh);

                    _rightHandBakedObj.transform.position = _unityBoneService.GetOculusSkeleton(Hand.Right).gameObject.transform.parent.parent.transform.position;
                    _rightHandBakedObj.transform.rotation = _unityBoneService.GetOculusSkeleton(Hand.Right).gameObject.transform.parent.parent.transform.rotation;
                    _rightHandBakedObj.transform.parent = parentObj.transform;
                    break;
            }
        }

        public override void RemoveMesh(Hand handedness)
        {
            switch (handedness)
            {
                case Hand.Left:
                    _leftHandSkinnedMesh.GetComponent<OVRMeshRenderer>().enabled = true;
                    _leftHandSkinnedMesh.enabled = true;
                    GameObject.Destroy(_leftHandBakedObj);
                    break;
                case Hand.Right:
                    _rightHandSkinnedMesh.GetComponent<OVRMeshRenderer>().enabled = true;
                    _rightHandSkinnedMesh.enabled = true;
                    GameObject.Destroy(_rightHandBakedObj);
                    break;
                case Hand.Both:
                    _leftHandSkinnedMesh.GetComponent<OVRMeshRenderer>().enabled = true;
                    _leftHandSkinnedMesh.enabled = true;
                    GameObject.Destroy(_leftHandBakedObj);

                    _rightHandSkinnedMesh.GetComponent<OVRMeshRenderer>().enabled = true;
                    _rightHandSkinnedMesh.enabled = true;
                    GameObject.Destroy(_rightHandBakedObj);
                    break;
            }
        }
    }

}
