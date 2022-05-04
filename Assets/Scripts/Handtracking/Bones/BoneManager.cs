using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

namespace Rhinox.Grappler.BoneManagement
{
    public enum Hand
    {
        Left,
        Right,
        Both,
    }

    /// <summary>
    /// A standardized bone structure needed for the physics framework to function
    /// </summary>
    public class RhinoxBone
    {
        public string Name { get; private set; } = null;
        public Transform BoneTransform { get; private set; } = null;
        public List<CapsuleCollider> BoneCollisionCapsules { get; private set; } = null;

        public RhinoxBone(string boneName, Transform boneTransform, List<CapsuleCollider> boneCollisionCapsules)
        {
            Name = boneName;
            BoneTransform = boneTransform;
            BoneCollisionCapsules = boneCollisionCapsules;
        }
    }


    /// <summary>
    /// Handles the standardisation of bones to be used by the physics framework of Grapple
    /// </summary>
    public class BoneManager : MonoBehaviour
    {

        private IBoneService _boneConvertorService = new NULLBoneService();

        private List<RhinoxBone> _leftHandBones = new List<RhinoxBone>();
        private List<RhinoxBone> _rightHandBones = new List<RhinoxBone>();

        public bool IsInitialised { get; private set; } = false;
        public UnityEvent onIsInitialised = new UnityEvent();

        private void Update()
        {
            if (_boneConvertorService.GetIsInitialised() && !_boneConvertorService.GetAreBonesLoaded())
                _boneConvertorService.TryLoadBones();
            else if(_boneConvertorService.GetIsInitialised() && !IsInitialised)
            {
                GetBonesFromCouplerService();
                IsInitialised = true;
                onIsInitialised.Invoke();
            }
        }

        public void SetBoneConvertorService(IBoneService newService)
        {
            _boneConvertorService = newService;
            _boneConvertorService.Initialise(this.gameObject);
        }

        public void ClearBones(Hand hand)
        {
            switch (hand)
            {
                case Hand.Left:
                    _leftHandBones.Clear();
                    break;
                case Hand.Right:
                    _rightHandBones.Clear();
                    break;
                case Hand.Both:
                    _leftHandBones.Clear();
                    _rightHandBones.Clear();
                    break;
            }
        }

        public void AddBone(Hand hand, RhinoxBone bone)
        {
            switch (hand)
            {
                case Hand.Left:
                    _leftHandBones.Add(bone);
                    break;
                case Hand.Right:
                    _rightHandBones.Add(bone);
                    break;
                case Hand.Both:
                    _leftHandBones.Add(bone);
                    _rightHandBones.Add(bone);
                    break;
            }
        }

        public void AddBones(Hand hand, List<RhinoxBone> bones)
        {
            foreach (var bone in bones)
            {
                AddBone(hand, bone);
            }
        }

        public List<RhinoxBone> GetRhinoxBones(Hand hand)
        {
            if (_leftHandBones.Count == 0 || _rightHandBones.Count == 0)
            {
                Debug.Log("Rhinox.Grappler.Bonemanagement.BoneManager.GetRhinoxBones() : Re-Getting bones from coupler service");
                GetBonesFromCouplerService();
            }
            switch (hand)
            {
                case Hand.Left:
                    return _leftHandBones;
                case Hand.Right:
                    return _rightHandBones;
                case Hand.Both:
                    return _leftHandBones.Concat(_rightHandBones).ToList();
            }
            return null;
        }

        private void GetBonesFromCouplerService(bool refreshBoneList = true)
        {
            if (refreshBoneList)
            {
                _leftHandBones.Clear();
                _rightHandBones.Clear();
            }

            _leftHandBones = _boneConvertorService.GetBones(Hand.Left);
            _rightHandBones = _boneConvertorService.GetBones(Hand.Right);
        }

    }
}

