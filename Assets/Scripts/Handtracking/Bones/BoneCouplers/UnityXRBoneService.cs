using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


namespace Rhinox.Grappler.BoneManagement
{
    public class UnityXRBoneService : IBoneService
    {

        [SerializeField] private OVRSkeleton _skeletonRefLeftHand = null;
        [SerializeField] private OVRSkeleton _skeletonRefRightHand = null;

        private GameObject _controllerParent = null;

        private bool _isInitialised = false;
        private bool _areBonesLoaded = false;

        void IBoneService.Initialise(GameObject controllerParent)
        {
            if (controllerParent == null)
            {
                Debug.LogError("Rhinox.Grappler.Bonemanagement.UnityXRBoneService.Initialise() : Controller parent cannot be null");
                return;
            }
            _controllerParent = controllerParent;
            _isInitialised = true;
        }

        bool IBoneService.GetIsInitialised()
        {
            return _isInitialised;
        }

        bool IBoneService.TryLoadBones()
        {
            _areBonesLoaded = false;

            var skeletons = _controllerParent.GetComponentsInChildren<OVRSkeleton>();
            foreach (var skeleton in skeletons)
            {
                if (!skeleton.IsInitialized)
                    return false;
            }
            foreach (var skeleton in skeletons)
            {
                var type = skeleton.GetSkeletonType();
                switch (type)
                {
                    case OVRSkeleton.SkeletonType.HandLeft:
                        _skeletonRefLeftHand = skeleton;
                        break;
                    case OVRSkeleton.SkeletonType.HandRight:
                        _skeletonRefRightHand = skeleton;
                        break;
                    default:
                        Debug.LogError("Rhinox.Grappler.Bonemanagement.UnityXRBoneService.TryLoadBones() : Cannot determine hand type");
                        return false;
                }
            }
            Debug.Log("Rhinox.Grappler.Bonemanagement.UnityXRBoneService.TryLoadBones() : OVRSkeletons loaded in");
            
            _areBonesLoaded = true;
            return true;
        }

        bool IBoneService.GetAreBonesLoaded()
        {
            return _areBonesLoaded;
        }

        List<RhinoxBone> IBoneService.GetBones(Hand hand)
        {
            List<RhinoxBone> retVal = new List<RhinoxBone>();
            switch (hand)
            {
                case Hand.Left:
                    for (int i = 0; i < _skeletonRefLeftHand.Bones.Count; i++)
                    {
                        retVal.Add(new RhinoxBone(
                            _skeletonRefLeftHand.Bones[i].Id.ToString(),
                            _skeletonRefLeftHand.Bones[i].Transform,
                            FindCorrespondingColliderCapsules(_skeletonRefLeftHand, i)));
                    }
                    break;
                case Hand.Right:
                    for (int i = 0; i < _skeletonRefRightHand.Bones.Count; i++)
                    {
                        retVal.Add(new RhinoxBone(
                            _skeletonRefRightHand.Bones[i].Id.ToString(),
                            _skeletonRefRightHand.Bones[i].Transform,
                            FindCorrespondingColliderCapsules(_skeletonRefRightHand, i)));
                    }
                    break;
                case Hand.Both:
                    for (int i = 0; i < _skeletonRefLeftHand.Bones.Count; i++)
                    {
                        retVal.Add(new RhinoxBone(
                            _skeletonRefLeftHand.Bones[i].Id.ToString(),
                            _skeletonRefLeftHand.Bones[i].Transform,
                            FindCorrespondingColliderCapsules(_skeletonRefLeftHand, i)));
                    }
                    for (int i = 0; i < _skeletonRefRightHand.Bones.Count; i++)
                    {
                        retVal.Add(new RhinoxBone(
                            _skeletonRefRightHand.Bones[i].Id.ToString(),
                            _skeletonRefRightHand.Bones[i].Transform,
                            FindCorrespondingColliderCapsules(_skeletonRefRightHand, i)));
                    }
                    break;
            }
            return retVal;
        }

        public List<OVRBone> GetOculusBones(Hand hand)
        {
            switch (hand)
            {
                case Hand.Left:
                    return new List<OVRBone>(_skeletonRefLeftHand.Bones);
                case Hand.Right:
                    return new List<OVRBone> (_skeletonRefRightHand.Bones);
                case Hand.Both:
                    return new List<OVRBone>((_skeletonRefLeftHand.Bones).Concat(_skeletonRefRightHand.Bones));
            }
            return null;
        }

        public OVRSkeleton GetOculusSkeleton(Hand hand)
        {
            switch (hand)
            {
                case Hand.Left:
                    return _skeletonRefLeftHand;
                case Hand.Right:
                    return _skeletonRefRightHand;
                case Hand.Both:
                    return null;
            }
            return null;
        }

        private List<CapsuleCollider> FindCorrespondingColliderCapsules(OVRSkeleton skeleton, int i)
        {
            List<CapsuleCollider> retVal = new List<CapsuleCollider>();

            foreach (var capsule in skeleton.Capsules)
            {
                if (capsule.BoneIndex == i)
                {
                    retVal.Add(capsule.CapsuleCollider);
                }
            }
            return retVal;
        }
    }
}

