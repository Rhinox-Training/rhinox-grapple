using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;

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
        public CapsuleCollider BoneCollisionCapsule { get; private set; } = null;

        public RhinoxBone(string boneName, Transform boneTransform, CapsuleCollider boneCollisionCapsule)
        {
            Name = boneName;
            BoneTransform = boneTransform;
            BoneCollisionCapsule = boneCollisionCapsule;
        }
    }


    /// <summary>
    /// Handles the standardisation of bones to be used by the physics framework of Grapple
    /// </summary>
    public class BoneManager : MonoBehaviour
    {

        [SerializeField] private IBoneService _boneConvertorService = new UnityXRBoneService();

        private List<RhinoxBone> _leftHandBones = new List<RhinoxBone>();
        private List<RhinoxBone> _rightHandBones = new List<RhinoxBone>();

        private bool _isLeftHandInitialised = false;
        private bool _isRightHandInitialised = false;


        #region Singleton
        public static BoneManager Instance { get; private set; }
        private void Awake()
        {

            if (Instance != null && Instance != this)
            {
                Destroy(this);
            }
            else
            {
                Instance = this;
            }
            Initialize();
        }
        private void Initialize()
        {
            ClearBones(Hand.Both);
            _boneConvertorService.Initialise(this.gameObject);
        }
        #endregion Singleton

        private void Update()
        {
            if (_boneConvertorService.GetIsInitialised() && !_boneConvertorService.GetAreBonesLoaded())
                _boneConvertorService.TryLoadBones();
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
            GetBonesFromCouplerService();
            switch (hand)
            {
                case Hand.Left:
                    return _leftHandBones;
                case Hand.Right:
                    return _rightHandBones;
                case Hand.Both:
                    return null;
            }
            return null;
        }

        private void GetBonesFromCouplerService(bool refreshBoneList = true)
        {
            if (!_boneConvertorService.GetAreBonesLoaded())
                return;

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

