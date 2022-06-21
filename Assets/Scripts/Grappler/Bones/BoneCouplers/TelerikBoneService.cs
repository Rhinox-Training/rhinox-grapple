#if USING_TELERIK
using System.Collections;
using System.Collections.Generic;
using Telerik.Unity.XR.Rig.Tracking;
using UnityEngine;

namespace Rhinox.Grappler.BoneManagement
{
    public class TelerikBoneService : IBoneService
    {
        [SerializeField] private TrackingHandPose _trackingRefLeftHand = null;
        [SerializeField] private TrackingHandPose _trackingRefRightHand = null;

        private GameObject _controllerParent = null;

        private bool _isInitialised = false;
        private bool _areBonesLoaded = false;


        public void Initialise(GameObject controllerParent)
        {
            if (controllerParent == null)
            {
                Debug.LogError("Rhinox.Grappler.Bonemanagement.TelerikBoneService.Initialise() : Controller parent cannot be null");
                return;
            }
            _controllerParent = controllerParent;
            _isInitialised = true;
        }

        public bool GetIsInitialised()
        {
            return _isInitialised;
        }

        public bool TryLoadBones()
        {
            _areBonesLoaded = false;

            var hands = _controllerParent.GetComponentsInChildren<Telerik.Unity.XR.Rig.Tracking.Hand>();

            if (hands.Length != 2)
                return false;

            foreach (var hand in hands)
            {
                var type = hand.hand;
                switch (type)
                {
                    case Telerik.Unity.XR.Rig.Input.HandInput.Left:
                        _trackingRefLeftHand = hand.gameObject.GetComponent<TrackingHandPose>();
                        break;
                    case Telerik.Unity.XR.Rig.Input.HandInput.Right:
                        _trackingRefRightHand = hand.gameObject.GetComponent<TrackingHandPose>();
                        break;
                    default:
                        Debug.LogError("Rhinox.Grappler.Bonemanagement.TelerikBoneService.TryLoadBones() : Cannot determine hand type");
                        return false;
                }
            }
            Debug.Log("Rhinox.Grappler.Bonemanagement.TelerikBoneService.TryLoadBones() : OVRSkeletons loaded in");

            _areBonesLoaded = true;
            return true;
        }
        public bool GetAreBonesLoaded()
        {
            return _areBonesLoaded;
        }

        public Transform GetControllerParent()
        {
            return _controllerParent.transform;
        }

        public List<RhinoxBone> GetBones(Hand hand)
        {
            List<RhinoxBone> retVal = new List<RhinoxBone>();
            
            List<Transform> tempBoneTransforms = new List<Transform>();
            List<CapsuleCollider> tempCapsuleColliders = new List<CapsuleCollider>();

            switch (hand)
            {
                case Hand.Left:

                    // left hand
                    tempBoneTransforms.Clear();
                    tempBoneTransforms = GetTelerikBoneTransforms(_trackingRefLeftHand);

                    tempCapsuleColliders.Clear();
                    tempCapsuleColliders = CreateCorrespondingCapsuleColliders(tempBoneTransforms);

                    for (int i = 0; i < tempBoneTransforms.Count;i++)
                    {
                        var boneColliders = new List<CapsuleCollider>();
                        boneColliders.Add(tempCapsuleColliders[i]);

                        retVal.Add(new RhinoxBone(
                            tempBoneTransforms[i].ToString(),
                            tempBoneTransforms[i],
                            boneColliders
                            ));
                    }

                    break;
                case Hand.Right:

                    // right hand
                    tempBoneTransforms.Clear();
                    tempBoneTransforms = GetTelerikBoneTransforms(_trackingRefRightHand);

                    tempCapsuleColliders.Clear();
                    tempCapsuleColliders = CreateCorrespondingCapsuleColliders(tempBoneTransforms);

                    for (int i = 0; i < tempBoneTransforms.Count; i++)
                    {
                        var boneColliders = new List<CapsuleCollider>();
                        boneColliders.Add(tempCapsuleColliders[i]);

                        retVal.Add(new RhinoxBone(
                            tempBoneTransforms[i].ToString(),
                            tempBoneTransforms[i],
                            boneColliders
                            ));
                    }
                    break;
                case Hand.Both:
                    
                    //left hand
                    tempBoneTransforms.Clear();
                    tempBoneTransforms = GetTelerikBoneTransforms(_trackingRefLeftHand);

                    tempCapsuleColliders.Clear();
                    tempCapsuleColliders = CreateCorrespondingCapsuleColliders(tempBoneTransforms);

                    for (int i = 0; i < tempBoneTransforms.Count; i++)
                    {
                        var boneColliders = new List<CapsuleCollider>();
                        boneColliders.Add(tempCapsuleColliders[i]);

                        retVal.Add(new RhinoxBone(
                            tempBoneTransforms[i].ToString(),
                            tempBoneTransforms[i],
                            boneColliders
                            ));
                    }

                    // right hand
                    tempBoneTransforms.Clear();
                    tempBoneTransforms = GetTelerikBoneTransforms(_trackingRefRightHand);

                    tempCapsuleColliders.Clear();
                    tempCapsuleColliders = CreateCorrespondingCapsuleColliders(tempBoneTransforms);

                    for (int i = 0; i < tempBoneTransforms.Count; i++)
                    {
                        var boneColliders = new List<CapsuleCollider>();
                        boneColliders.Add(tempCapsuleColliders[i]);

                        retVal.Add(new RhinoxBone(
                            tempBoneTransforms[i].ToString(),
                            tempBoneTransforms[i],
                            boneColliders
                            ));
                    }
                    break;
            }

            return retVal;
        }

        public List<Transform> GetTelerikBoneTransforms(TrackingHandPose pose)
        {
            List<Transform> retval = new List<Transform>();

            for (int i = 0; i < 5; i++)
            {
                DigitPose fingerPose =  pose.GetFinger((Telerik.Unity.XR.Rig.Input.Finger)i);
                retval.Add(fingerPose.carpal);
                retval.Add(fingerPose.bone1);
                retval.Add(fingerPose.bone2);
                retval.Add(fingerPose.bone3);
            }

            retval.RemoveAll(x => !x);
            return retval;
        }

        private List<CapsuleCollider> CreateCorrespondingCapsuleColliders(List<Transform> boneTransforms)
        {
            const float colliderRadius = 0.010f;
            const float colliderHeight = 0.050f;

            List<CapsuleCollider> retVal = new List<CapsuleCollider>();

            foreach (var boneTransform in boneTransforms)
            {
                // remove other colliders
                Collider[] colliders = boneTransform.gameObject.GetComponents<Collider>();
                foreach (var coll in colliders)
                {
                    GameObject.Destroy(coll);
                }

                // create a collider
                var sc = boneTransform.gameObject.AddComponent<CapsuleCollider>();
                sc.radius = colliderRadius;
                sc.height = colliderHeight;
                sc.direction = 2;

                // make it inactive
                sc.enabled = false;
                retVal.Add(sc);        
            }
            return retVal; 
        }

    }
}

#endif
