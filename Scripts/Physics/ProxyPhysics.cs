using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Rhinox.Grappler.BoneManagement;
using System;

namespace Rhinox.Grappler.HandPhysics
{
    public class ProxyPhysics : BasePhysicsService
    {
        private static GameObject _proxyParentObject = null;

        private class ProxyObject
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
            private GameObject _proxyObject = null;
            private CapsuleCollider _proxyObjectCapsuleCollider = null;
            private Rigidbody _proxyObjectRigidBody = null;
            private ProxyPhysicsProxyCollisionEventHandler _eventHandler = null;

            // dummy object
            private GameObject _dummyObject = null;
            private CapsuleCollider _dummyObjectCapsuleCollider = null;
            private Rigidbody _dummyObjectRigidBody = null;

            private Joint _connectionJoint = null;

            private ProxyPhysics _physXSolution = null;


            /// <summary>
            /// The proxy object class is there to handle the dummy and proxy object,
            /// handles the creation and management of them
            /// </summary>
            /// <param name="bone"></param>
            /// <param name="collisionLayer"></param>
            public ProxyObject(RhinoxBone bone, Hand handedness, LayerMask collisionLayer, ProxyPhysics physXSolution)
            {
                if (_proxyParentObject == null)
                {
                    _proxyParentObject = new GameObject("[GENERATED]ProxyParent");
                }
                _physXSolution = physXSolution;

                _rhinoxBone = bone;
                _collisionLayer = collisionLayer;
                _handedness = handedness;
                _proxyObject = new GameObject("ProxyObject_" + _rhinoxBone.Name);
                _proxyObject.transform.parent = _proxyParentObject.transform;

                _dummyObject = new GameObject("DummyObject_" + _rhinoxBone.Name);
                Initialise();
            }

            private void Initialise()
            {
                if (_rhinoxBone.BoneCollisionCapsules.Count <= 0)
                {
                    GameObject.Destroy(_proxyObject);
                    GameObject.Destroy(_dummyObject);
                    return;
                }

                BuildDummyObject();
                BuildProxyObject();
                SetCollisionLayer();
                _prevState = true;
                IsInitialised = true;
            }

            public void Rebuild()
            {
                if (_rhinoxBone.BoneCollisionCapsules.Count <= 0)
                    return;

                if (_proxyParentObject == null)
                {
                    _proxyParentObject = new GameObject("[GENERATED]ProxyParent");
                }


                Debug.Log("Rebuilding");

                GameObject.Destroy(_proxyObject);
                _proxyObject = new GameObject("ProxyObject_" + _rhinoxBone.Name);
                _proxyObject.transform.parent = _proxyParentObject.transform;
                _proxyObject.SetActive(false);

                _gracePeriodTimer = _gracePeriod;

                BuildProxyObject();
                SetCollisionLayer();
            }

            private void Destroy()
            {
                GameObject.Destroy(_proxyObject);
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
                _proxyObject.SetActive(false);

                _proxyObject.transform.position = _dummyObject.transform.position;
                _proxyObject.transform.rotation = _dummyObject.transform.rotation;

                _proxyObjectCapsuleCollider = _proxyObject.AddComponent<CapsuleCollider>();
                GrappleUtils.CopyCapsuleColliderValues(_proxyObjectCapsuleCollider, _rhinoxBone.BoneCollisionCapsules[0]);
                _proxyObjectCapsuleCollider.isTrigger = false;
                _proxyObjectCapsuleCollider.enabled = true;

                // disable old BoneCollisionCapsules
                foreach (var boneCollisionCapsule in _rhinoxBone.BoneCollisionCapsules)
                {
                    boneCollisionCapsule.isTrigger = true;
                }

                _proxyObjectRigidBody = _proxyObject.AddComponent<Rigidbody>();
                _proxyObjectRigidBody.isKinematic = false;
                _proxyObjectRigidBody.collisionDetectionMode = CollisionDetectionMode.ContinuousSpeculative;
                _proxyObjectRigidBody.ResetInertiaTensor();

                _connectionJoint = _proxyObject.AddComponent<FixedJoint>();
                _connectionJoint.connectedBody = _dummyObjectRigidBody;
                _connectionJoint.autoConfigureConnectedAnchor = false;
                _connectionJoint.enablePreprocessing = false;
                _connectionJoint.connectedAnchor = new Vector3(0, 0, 0);

                _eventHandler = _proxyObject.AddComponent<ProxyPhysicsProxyCollisionEventHandler>();
                _eventHandler.Initialise(_handedness, _physXSolution._allowTriggersForTouchEvents);

                // re-enable object to enable physics
                _proxyObject.SetActive(true);
            }

            public void Update()
            {
                if (!IsInitialised)
                    return;

                if (_gracePeriodTimer > 0.0f)
                {
                    _proxyObject.SetActive(false);
                    _gracePeriodTimer -= Time.deltaTime;
                    return;
                }

                _proxyObject.SetActive(_dummyObject.activeInHierarchy);

                if (_dummyObject.activeInHierarchy == true && _prevState == false)
                {
                    Rebuild();
                }
                _prevState = _dummyObject.activeInHierarchy;
            }


            // TODO, check how prevstate changes in the code flow, it is being set multiple times
            public void SetEnabled(bool newState)
            {
                if (newState == _prevState)
                    return;

                if (!newState)
                    Destroy();
                else
                {
                    Rebuild();
                }
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
                _proxyObject.layer = layerNumber - 1;
            }
        }

        [Header("Settings")]
        public bool _allowTriggersForTouchEvents = false;

        private bool _isInitialised = false;

        private List<ProxyObject> _leftHandProxyObjects = new List<ProxyObject>();
        private bool _isLeftHandEnabled = false;

        private List<ProxyObject> _rightHandProxyObjects = new List<ProxyObject>();
        private bool _isRightHandEnabled = false;


        private LayerMask _handLayer = -1;

        public override void Initialise(BoneManager boneManager, HandPhysicsController controller)
        {
            List<RhinoxBone> leftHandBones = boneManager.GetRhinoxBones(Hand.Left);
            foreach (var Bone in leftHandBones)
            {
                _leftHandProxyObjects.Add(new ProxyObject(Bone, Hand.Left, _handLayer,this));
            }

            List<RhinoxBone> rightHandBones = boneManager.GetRhinoxBones(Hand.Right);
            foreach (var Bone in rightHandBones)
            {
                _rightHandProxyObjects.Add(new ProxyObject(Bone, Hand.Right, _handLayer,this));
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

