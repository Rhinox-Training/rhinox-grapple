using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Rhinox.Grappler;
using Rhinox.Grappler.BoneManagement;
using System;

namespace Rhinox.Grappler.HandPhysics
{
    public class KinematicProxyPhysics : BasePhysicsService
    {
        private class KinematicProxyObject
        {
            // settings
            private const float _gracePeriod = 0.5f;
            private float _gracePeriodTimer = -1;
            private Hand _handedness = Hand.Both;

            public bool IsInitialised { get; private set; } = false;
            private bool _prevState = false;
            private RhinoxBone _rhinoxBone = null;
            private LayerMask _collisionLayer = 0;

            // proxy object
            private GameObject _kinematicProxyObject = null;
            private CapsuleCollider _proxyObjectCapsuleCollider = null;
            private Rigidbody _proxyObjectRigidBody = null;
            private ProxyPhysicsProxyCollisionEventHandler _eventHandler = null;

            // dummy object
            private GameObject _dummyObject = null;
            private CapsuleCollider _dummyObjectCapsuleCollider = null;
            private Rigidbody _dummyObjectRigidBody = null;

            /// <summary>
            /// The KinematicProxy object class is there to handle the dummy and proxy object,
            /// handles the creation and management of them
            /// </summary>
            /// <param name="bone"></param>
            /// <param name="collisionLayer"></param>
            public KinematicProxyObject(RhinoxBone bone, Hand handedness, LayerMask collisionLayer)
            {
                _rhinoxBone = bone;
                _collisionLayer = collisionLayer;
                _handedness = handedness;
                _kinematicProxyObject = new GameObject("ProxyObject_" + _rhinoxBone.Name);
                _dummyObject = new GameObject("DummyObject_" + _rhinoxBone.Name);
                Initialise();
            }

            private void Initialise()
            {
                if (_rhinoxBone.BoneCollisionCapsules.Count <= 0)
                {
                    GameObject.Destroy(_kinematicProxyObject);
                    GameObject.Destroy(_dummyObject);
                    return;
                }

                BuildDummyObject();
                BuildProxyObject();
                SetCollisionLayer();
                _prevState = true;
                IsInitialised = true;
            }

            /// <summary>
            /// creates and assigns a dummy object with the correct settings
            /// </summary>
            private void BuildDummyObject()
            {
                // disabling object to disable physics
                _dummyObject.SetActive(false);

                _dummyObject.transform.localPosition = new Vector3(0, 0, 0);

                _dummyObjectRigidBody = _dummyObject.AddComponent<Rigidbody>();
                _dummyObjectRigidBody.isKinematic = true;
                _dummyObjectRigidBody.collisionDetectionMode = CollisionDetectionMode.ContinuousSpeculative;
                _dummyObjectRigidBody.ResetInertiaTensor();

                _dummyObject.transform.parent = _rhinoxBone.BoneCollisionCapsules[0].transform;
                _dummyObject.transform.rotation = _rhinoxBone.BoneCollisionCapsules[0].transform.rotation;
                _dummyObject.transform.localPosition = new Vector3(0, 0, 0);

                // re-enable object to enable physics
                _dummyObject.SetActive(true);
            }


            /// <summary>
            /// creates and assigns a proxy object with the correct settings
            /// </summary>
            private void BuildProxyObject()
            {
                // disabling object to disable physics
                _kinematicProxyObject.SetActive(false);

                _kinematicProxyObject.transform.position = _dummyObject.transform.position;
                _kinematicProxyObject.transform.rotation = _dummyObject.transform.rotation;

                _proxyObjectCapsuleCollider = _kinematicProxyObject.AddComponent<CapsuleCollider>();
                GrappleUtils.CopyCapsuleColliderValues(_proxyObjectCapsuleCollider, _rhinoxBone.BoneCollisionCapsules[0]); 
                _proxyObjectCapsuleCollider.isTrigger = false;
                _proxyObjectCapsuleCollider.enabled = true;

                // disable old BoneCollisionCapsules
                foreach (var boneCollisionCapsule in _rhinoxBone.BoneCollisionCapsules)
                {
                    boneCollisionCapsule.isTrigger = true;
                }

                _proxyObjectRigidBody = _kinematicProxyObject.AddComponent<Rigidbody>();
                _proxyObjectRigidBody.isKinematic = true;
                _proxyObjectRigidBody.collisionDetectionMode = CollisionDetectionMode.ContinuousSpeculative;

                _eventHandler = _kinematicProxyObject.AddComponent<ProxyPhysicsProxyCollisionEventHandler>();
                _eventHandler.Initialise(_handedness);

                // re-enable object to enable physics
                _kinematicProxyObject.SetActive(true);
            }

            public void Update()
            {
                if (!IsInitialised)
                    return;

                if (_gracePeriodTimer > 0.0f)
                {
                    _kinematicProxyObject.SetActive(false);
                    _gracePeriodTimer -= Time.deltaTime;
                    return;
                }

                _kinematicProxyObject.SetActive(_dummyObject.activeInHierarchy);
                _kinematicProxyObject.transform.position = (_dummyObject.transform.position);
                _kinematicProxyObject.transform.rotation = (_dummyObject.transform.rotation);


                _prevState = _dummyObject.activeInHierarchy;
            }

            public void SetEnabled(bool newState)
            {
                if (newState == _prevState)
                    return;

                // reset grace period
                if(newState == true)
                    _gracePeriodTimer = _gracePeriod;

                _kinematicProxyObject.SetActive(newState);
                _prevState = newState;
            }

            private void SetCollisionLayer()
            {
                int layerNumber = 0;
                int layerVal = _collisionLayer.value;
                while (layerVal > 0)
                {
                    layerVal = layerVal >> 1;
                    layerNumber++;
                }
                _kinematicProxyObject.layer = layerNumber - 1;
            }
        }

        private bool _isInitialised = false;

        private List<KinematicProxyObject> _leftHandProxyObjects = new List<KinematicProxyObject>();
        private bool _isLeftHandEnabled = false;

        private List<KinematicProxyObject> _rightHandProxyObjects = new List<KinematicProxyObject>();
        private bool _isRightHandEnabled = false;


        private LayerMask _handLayer = -1;

        public override void Initialise(BoneManager boneManager, HandPhysicsController controller)
        {
            List<RhinoxBone> leftHandBones = boneManager.GetRhinoxBones(Hand.Left);
            foreach (var Bone in leftHandBones)
            {
                _leftHandProxyObjects.Add(new KinematicProxyObject(Bone, Hand.Left, _handLayer));
            }

            List<RhinoxBone> rightHandBones = boneManager.GetRhinoxBones(Hand.Right);
            foreach (var Bone in rightHandBones)
            {
                _rightHandProxyObjects.Add(new KinematicProxyObject(Bone, Hand.Right, _handLayer));
            }
            _isInitialised = true;
        }

        public override bool GetIsInitialised()
        {
            return _isInitialised;
        }

        public override void SetEnabled(bool newState, Hand handedness)
        {
            switch (handedness)
            {
                case Hand.Left:
                    _isLeftHandEnabled = newState;
                    foreach (var proxyObject in _leftHandProxyObjects)
                    {
                        proxyObject.SetEnabled(newState);
                    }
                    break;
                case Hand.Right:
                    _isRightHandEnabled = newState;
                    foreach (var proxyObject in _rightHandProxyObjects)
                    {
                        proxyObject.SetEnabled(newState);
                    }
                    break;
                case Hand.Both:
                    _isLeftHandEnabled = newState;
                    foreach (var proxyObject in _leftHandProxyObjects)
                    {
                        proxyObject.SetEnabled(newState);
                    }

                    _isRightHandEnabled = newState;
                    foreach (var proxyObject in _rightHandProxyObjects)
                    {
                        proxyObject.SetEnabled(newState);
                    }
                    break;
            }
        }
        public override bool GetIsEnabled(Hand handedness)
        {
            switch (handedness)
            {
                case Hand.Left:
                    return _isLeftHandEnabled;
                case Hand.Right:
                    return _isRightHandEnabled;
            }
            return false;
        }

        public override void SetHandLayer(LayerMask layer)
        {
            _handLayer = layer;
        }

        public override void ManualUpdate()
        {
            if (_isLeftHandEnabled)
            {
                foreach (var proxyObject in _leftHandProxyObjects)
                {
                    proxyObject.Update();
                }
            }
            if (_isRightHandEnabled)
            {
                foreach (var proxyObject in _rightHandProxyObjects)
                {
                    proxyObject.Update();
                }
            }
        }
    }
}
