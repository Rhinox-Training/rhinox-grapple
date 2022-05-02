using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Rhinox.Grappler.BoneManagement;
using UnityEditor;

namespace Rhinox.Grappler.HandPhysics
{
    public class ProxyPhysics : IPhysicsService
    {
        private class ProxyObject
        {
            public bool IsInitialised { get; private set; } = false;
            private bool _prevState = false;
            private RhinoxBone _rhinoxBone = null;

            private LayerMask _collisionLayer = 0;

            // proxy object
            private GameObject _proxyObject = null;
            private CapsuleCollider _proxyObjectCapsuleCollider = null;
            private Rigidbody _proxyObjectRigidBody = null;

            // dummy object
            private GameObject _dummyObject = null;
            private CapsuleCollider _dummyObjectCapsuleCollider = null;
            private Rigidbody _dummyObjectRigidBody = null;

            private Joint _connectionJoint = null;



            public ProxyObject(RhinoxBone bone, LayerMask collisionLayer)
            {
                _rhinoxBone = bone;
                _collisionLayer = collisionLayer;
                _proxyObject = new GameObject("ProxyObject_" + _rhinoxBone.Name);
                _dummyObject = new GameObject("DummyObject_" + _rhinoxBone.Name);
                Initialise();
            }

            private void Initialise()
            {
                if (!_rhinoxBone.BoneCollisionCapsule)
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
                if (!_rhinoxBone.BoneCollisionCapsule)
                    return;
                Debug.Log("Rebuilding");

                GameObject.Destroy(_proxyObject);
                _proxyObject = new GameObject("ProxyObject_" + _rhinoxBone.Name);

                BuildProxyObject();
                SetCollisionLayer();
            }


            private void BuildDummyObject()
            {
                // disabling object to disable physics
                _dummyObject.SetActive(false);

                _dummyObject.transform.localPosition = new Vector3(0, 0, 0);

                _dummyObjectRigidBody = _dummyObject.AddComponent<Rigidbody>();
                _dummyObjectRigidBody.isKinematic = true;
                _dummyObjectRigidBody.collisionDetectionMode = CollisionDetectionMode.ContinuousSpeculative;
                _dummyObjectRigidBody.ResetInertiaTensor();

                _dummyObject.transform.parent = _rhinoxBone.BoneCollisionCapsule.transform;
                _dummyObject.transform.rotation = _rhinoxBone.BoneCollisionCapsule.transform.rotation;
                _dummyObject.transform.localPosition = new Vector3(0, 0, 0);



                // re-enable object to enable physics
                _dummyObject.SetActive(true);
            }

            private void BuildProxyObject()
            {
                // disabling object to disable physics
                _proxyObject.SetActive(false);

                _proxyObject.transform.position = _dummyObject.transform.position;
                _proxyObject.transform.rotation = _dummyObject.transform.rotation;

                // needs to be made without editor scripts
                _proxyObjectCapsuleCollider = _proxyObject.AddComponent<CapsuleCollider>();
                var jsonRhinBoCaCo = EditorJsonUtility.ToJson(_rhinoxBone.BoneCollisionCapsule);
                EditorJsonUtility.FromJsonOverwrite(jsonRhinBoCaCo, _proxyObjectCapsuleCollider);
                _proxyObjectCapsuleCollider.isTrigger = false;
                _proxyObjectCapsuleCollider.enabled = true;

                // disable old BoneCollisionCapsule
                _rhinoxBone.BoneCollisionCapsule.enabled = false;

                _proxyObjectRigidBody = _proxyObject.AddComponent<Rigidbody>();
                _proxyObjectRigidBody.isKinematic = false;
                _proxyObjectRigidBody.collisionDetectionMode = CollisionDetectionMode.ContinuousSpeculative;
                _proxyObjectRigidBody.ResetInertiaTensor();

                _connectionJoint = _proxyObject.AddComponent<FixedJoint>();
                _connectionJoint.connectedBody = _dummyObjectRigidBody;
                _connectionJoint.autoConfigureConnectedAnchor = false;
                _connectionJoint.enablePreprocessing = false;
                _connectionJoint.connectedAnchor = new Vector3(0, 0, 0);

                // re-enable object to enable physics
                _proxyObject.SetActive(true);
            }

            public void Update()
            {
                if (!IsInitialised)
                    return;

                _proxyObject.SetActive(_dummyObject.activeInHierarchy);
                
                if (_dummyObject.activeInHierarchy == true && _prevState == false)
                {
                    Rebuild();
                }
                _prevState = _dummyObject.activeInHierarchy;
            }

            public void SetEnabled(bool newState)
            {
                _proxyObject.SetActive(newState);
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

        private bool _isInitialised = false;
        private bool _isEnabled = false;
        private List<ProxyObject> _proxyObjects = new List<ProxyObject>();
        private LayerMask _handLayer = -1;

        void IPhysicsService.Initialise(BoneManager boneManager)
        {
            List<RhinoxBone> handBones = boneManager.GetRhinoxBones(Hand.Both);
            foreach (var Bone in handBones)
            {
                _proxyObjects.Add(new ProxyObject(Bone,_handLayer));
            }
            _isInitialised = true;
        }

        bool IPhysicsService.GetIsInitialised()
        {
            return _isInitialised;
        }

        void IPhysicsService.SetEnabled(bool newState)
        {
            foreach (var proxyObject in _proxyObjects)
            {
                proxyObject.SetEnabled(newState);
            }
            _isEnabled = newState;
        }

        bool IPhysicsService.GetIsEnabled()
        {
            return _isEnabled;
        }

        void IPhysicsService.SetHandLayer(LayerMask layer) 
        {
            _handLayer = layer;
        }

        void IPhysicsService.Update()
        {
            if (!_isEnabled)
                return;

            foreach (var proxyObject in _proxyObjects)
            {
                proxyObject.Update();
            }
        }
    }
}

