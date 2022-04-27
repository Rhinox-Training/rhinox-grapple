using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;


namespace Rhinox.Grappler.BoneManagement
{
    public class UnityXRBoneService : IBoneService
    {

        [SerializeField] private OVRSkeleton _skeletonRefLeftHand = null;
        [SerializeField] private OVRSkeleton _skeletonRefRightHand = null;

        private GameObject _controllerParent = null;

        private bool _isInitialised = false;


        public void Initialise(GameObject controllerParent)
        {
            if (controllerParent == null)
            {
                Debug.Log("Rhinox.Grappler.Bonemanagement.IBoneService.Initialise() : Controller parent cannot be null");
                return;
            }

            Thread InitialiseThread = new Thread(new ThreadStart(Coroutine_Initialise));
            InitialiseThread.Start();

        }

        private void Coroutine_Initialise()
        {

            // NOTE: this is not very optimised at all
            var temp = _controllerParent.GetComponentsInChildren<OVRSkeleton>();
            foreach (var skeleton in temp)
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
                        Debug.Log("Rhinox.Grappler.Bonemanagement.IBoneService.Coroutine_Initialise() : Cannot determine hand type");
                        return;
                }
            }
            _isInitialised = true;
        }

        public bool GetIsInitialised()
        {


            return false;
        }

        public List<RhinoxBone> GetBones(Hand hand)
        {
            List<RhinoxBone> retVal = new List<RhinoxBone>();


            return retVal;
        }
    }
}

