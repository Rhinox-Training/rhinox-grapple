﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Rhinox.Grappler.BoneManagement;
using System;
using System.Linq;

namespace Rhinox.Grappler.HandPhysics
{
    public class ContactPointBasedPhysics : IPhysicsService
    {
        protected static GameObject LeftHandConnectedObject = null;
        protected static int LeftHandConnections = 0;
        protected static GameObject rightHandConnectedObject = null;
        protected static int RightHandConnections = 0;
        
        public class ContactSensor
        {
            /// SETTINGS ///
            private float _detectDistance = 0.05f;
            private float _deadzone = 0.02f;
            private float _breakDistance = 0.15f;
            private float _forceMultiplier = 300.0f;

            public bool IsInitialised { get; private set; } = false;
            private RhinoxBone _rhinoxBone = null;
            private LayerMask _collisionLayer = 0;
            private ContactPoint _contactPoint = null;
            private Hand _handedness = Hand.Both;

            public ContactSensor(RhinoxBone bone, LayerMask collisionLayer, Hand handedness)
            {
                _rhinoxBone = bone;
                _collisionLayer = collisionLayer;
                _handedness = handedness;
                Initialise();
            }

            private void Initialise()
            {
                // make the old bone capsules triggers, if any exists
                if (_rhinoxBone.BoneCollisionCapsules.Count > 0)
                {
                    foreach (var boneCollisionCapsule in _rhinoxBone.BoneCollisionCapsules)
                    {
                        boneCollisionCapsule.isTrigger = true;
                    }              
                }
                IsInitialised = true;
            }

            public void Update()
            {
                if (_rhinoxBone.BoneCollisionCapsules.Count <= 0)
                    return;

                if (_contactPoint == null)
                    FindContactPoint();
                else
                    HandleContactPoint();
            }

            public GameObject GetConnectedObject()
            {
                if (_contactPoint != null)
                    return _contactPoint.GetContactObject();
                else return null;
            }

            public Hand GetHandedness()
            {
                return _handedness;
            }

            private void FindContactPoint()
            {
                // get the layer number for the overlapSphere
                int layerNumber = 0;
                int layerVal = _collisionLayer.value;
                while (layerVal > 0)
                {
                    layerVal = layerVal >> 1;
                    layerNumber++;
                }

                // do a sphere cast
                var origin = GetCapsuleColliderOrigin();
                Collider[] colls = Physics.OverlapSphere(origin, _detectDistance, layerNumber, QueryTriggerInteraction.Ignore);

                // check if you hit something with a rigidbody
                if (colls.Length > 0)
                {
                    foreach (var collision in colls)
                    {
                        Rigidbody rb = null;
                        if ((rb = collision.gameObject.GetComponent<Rigidbody>()) == null)
                            continue;
                        else
                        {
                            Debug.Log("HIT: " + collision.gameObject);
                            
                            // create the contact point
                            _contactPoint = new ContactPoint(collision.gameObject, _handedness, collision.ClosestPoint(origin));
                            return;
                        }
                    }
                }
            }
            private void HandleContactPoint()
            {
                var origin = GetCapsuleColliderOrigin();
                var distance = Vector3.Distance(origin, _contactPoint.getContactPosition());

                // check if the contact should break according to distance
                if (distance > _breakDistance)
                {
                    _contactPoint.Break();
                    _contactPoint = null;
                    return;
                }
                // calculate amount of force depending on distance from contactpoint to realpoint
                Vector3 delta = origin - _contactPoint.getContactPosition();

                // apply the force at the point of the contactpoint to the picked up object
                _contactPoint.EmitForce(delta * (distance * _forceMultiplier));
            }

            private Vector3 GetCapsuleColliderOrigin(int idx = 0)
            {
                return _rhinoxBone.BoneCollisionCapsules[idx].transform.TransformPoint(_rhinoxBone.BoneCollisionCapsules[0].center);
            }

        }

        public class ContactPoint
        {
            GameObject _contactPoint = null;
            Hand _handedness = Hand.Both;

            public ContactPoint(GameObject contactObject, Hand handedness, Vector3 contactPosition)
            {


                if (handedness == Hand.Left)
                {
                    // prevents mutliple object grabbing
                    if (LeftHandConnectedObject != null && LeftHandConnectedObject != contactObject)
                        return;

                    LeftHandConnections++;
                    if (LeftHandConnections == 1)
                    {
                        LeftHandConnectedObject = contactObject;
                        LeftHandConnectedObject.GetComponent<Rigidbody>().useGravity = false;
                    }
                }
                else
                {
                    // prevents mutliple object grabbing
                    if (rightHandConnectedObject != null && rightHandConnectedObject != contactObject)
                        return;

                    RightHandConnections++;
                    if (RightHandConnections == 1)
                    {
                        rightHandConnectedObject = contactObject;
                        rightHandConnectedObject.GetComponent<Rigidbody>().useGravity = false;
                    }
                }

                _handedness = handedness;
                _contactPoint = new GameObject("ContactPoint_" + contactObject.name);
                _contactPoint.transform.position = contactPosition;
                _contactPoint.transform.parent = contactObject.transform;

            }

            /// <summary>
            /// Breaks the connection between the contact point and the connected object
            /// also handles the counting and setting of the static connectedObject depending on handedness
            /// </summary>
            public void Break()
            {
                if (_contactPoint != null)
                {
                    GameObject.Destroy(_contactPoint);
                }

                if (_handedness == Hand.Left)
                {
                    LeftHandConnections--;
                    if (LeftHandConnections == 0)
                    {
                        LeftHandConnectedObject.GetComponent<Rigidbody>().useGravity = true;
                        LeftHandConnectedObject = null;
                    }
                }
                else
                {
                    RightHandConnections--;
                    if (RightHandConnections == 0)
                    {
                        rightHandConnectedObject = null;
                        rightHandConnectedObject.GetComponent<Rigidbody>().useGravity = true;
                    }
                }              
            }

            /// <summary>
            /// emit a force at the contact points current position
            /// </summary>
            /// <param name="force">the force to be exerted</param>
            public void EmitForce(Vector3 force)
            {
                var rb = _contactPoint.transform.parent.GetComponent<Rigidbody>();
                rb.AddForceAtPosition(force, getContactPosition());
            }

            /// <summary>
            /// 
            /// </summary>
            /// <param name="inLocal"> should the position be in world or local space </param>
            /// <returns></returns>
            public Vector3 getContactPosition(bool inLocal = false)
            {
                if (_contactPoint == null)
                {
                    Break();
                    return Vector3.zero;
                } 

                if (inLocal)
                    return _contactPoint.transform.localPosition;
                else
                    return _contactPoint.transform.position;
            }

            /// <summary>
            /// returns the object where the contactpoint is currently attached to
            /// </summary>
            /// <returns></returns>
            public GameObject GetContactObject()
            {
                if (_contactPoint == null)
                {
                    Break();
                    return null;
                }
                return _contactPoint.transform.parent.gameObject;
            }
        }


        private bool _isInitialised = false;
        private bool _isEnabled = false;
        private LayerMask _handLayer = -1;
        List<ContactSensor> _leftHandedSensorObjects = new List<ContactSensor>();
        List<ContactSensor> _rightHandedSensorObjects = new List<ContactSensor>();
        private RhinoxBone _leftHandRoot = null;
        private RhinoxBone _rightHandRoot = null;

        private GameObject _leftHandDummyRoot = null;
        private GameObject _rightHandDummyRoot = null;

        private ConfigurableJoint _leftHandRotationalJoint = null;
        private ConfigurableJoint _rightHandRotationalJoint = null;

        public bool GetIsEnabled()
        {
            return _isEnabled;
        }

        public bool GetIsInitialised()
        {
            return _isInitialised;
        }

        public void Initialise(BoneManager boneManager)
        {
            List<RhinoxBone> leftHandBones = boneManager.GetRhinoxBones(Hand.Left);
            List<RhinoxBone> rightHandBones = boneManager.GetRhinoxBones(Hand.Right);

            _leftHandRoot = leftHandBones.FirstOrDefault();
            _rightHandRoot = rightHandBones.FirstOrDefault();

            SetupDummyObject(ref _leftHandDummyRoot, _leftHandRoot.BoneCollisionCapsules[0].gameObject, ref _leftHandRotationalJoint);
            SetupDummyObject(ref _rightHandDummyRoot, _rightHandRoot.BoneCollisionCapsules[0].gameObject, ref _rightHandRotationalJoint);

            foreach (var lhBone in leftHandBones)
            {
                _leftHandedSensorObjects.Add(new ContactSensor(lhBone, _handLayer, Hand.Left));
            }
            foreach (var rhBone in rightHandBones)
            {
                _rightHandedSensorObjects.Add(new ContactSensor(rhBone, _handLayer, Hand.Right));
            }
            _isInitialised = true;
        }

        /// <summary>
        /// creates a dummy object with and setups the join to be used 
        /// </summary>
        /// <param name="dummy"></param>
        /// <param name="root"></param>
        /// <param name="joint"></param>
        private void SetupDummyObject(ref GameObject dummy, GameObject root, ref ConfigurableJoint joint)
        {
            dummy = new GameObject("HandDummyRoot");
            dummy.transform.position = root.transform.position;
            dummy.transform.rotation = root.transform.rotation;
            dummy.transform.parent = root.transform;
            
            var rb = root.AddComponent<Rigidbody>();
            rb.isKinematic = true;

            joint = root.AddComponent<ConfigurableJoint>();
            joint.angularXMotion = ConfigurableJointMotion.Locked;
            joint.angularYMotion = ConfigurableJointMotion.Locked;
            joint.angularZMotion = ConfigurableJointMotion.Locked;
        }

        public void Update()
        {
            foreach (var sensor in _leftHandedSensorObjects)
            {
                sensor.Update();
                // if the rotational join is not yet connected, check if any contact is connected and there is a connected object
                if (_leftHandRotationalJoint.connectedBody == false && LeftHandConnectedObject != null)
                {
                    // if there is one connected, connect it to that one
                    _leftHandRotationalJoint.connectedBody = LeftHandConnectedObject.GetComponent<Rigidbody>();
                }
                else if (LeftHandConnectedObject == null)
                {
                    _leftHandRotationalJoint.connectedBody = null;
                }
            }

            foreach (var sensor in _rightHandedSensorObjects)
            {
                sensor.Update();
                // if the rotational join is not yet connected, check if any contact is connected and there is a connected object
                if (_rightHandRotationalJoint.connectedBody == false && rightHandConnectedObject != null)
                {
                    // if there is one connected, connect it to that one
                    _rightHandRotationalJoint.connectedBody = rightHandConnectedObject.GetComponent<Rigidbody>();
                }
                else if (LeftHandConnectedObject == null)
                {
                    _rightHandRotationalJoint.connectedBody = null;
                }
            }
        }

        public void SetEnabled(bool newState)
        {
            _isEnabled = newState;
        }

        public void SetHandLayer(LayerMask layer)
        {
            _handLayer = layer;
        }


    }

}