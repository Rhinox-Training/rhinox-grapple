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
        #region Singleton
        private BoneManager() { }
        private static BoneManager _instance;
        public static BoneManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new BoneManager();
                    _instance.Initialize();
                }
                return _instance;
            }
            private set
            {
                _instance = value;
            }
        }
        private void Initialize()
        {
            ClearBones(Hand.Both);
        }
        #endregion Singleton

        private List<RhinoxBone> _leftHandBones = new List<RhinoxBone>();
        private List<RhinoxBone> _rightHandBones = new List<RhinoxBone>();

        private bool _isLeftHandInitialised = false;
        private bool _isRightHandInitialised = false;

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

        private void AddBones(Hand hand, List<RhinoxBone> bones)
        {
            foreach (var bone in bones)
            {
                AddBone(hand, bone);
            }
        }

        public void GetBonesFromCouplerService(bool refreshBoneList = true)
        {
            if (refreshBoneList)
            {
                _leftHandBones.Clear();
                _rightHandBones.Clear();
            }
        }

        public List<RhinoxBone> GetRhinoxBones(Hand hand)
        {
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
    }
}

