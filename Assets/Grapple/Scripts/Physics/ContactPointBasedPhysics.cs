using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Rhinox.Grappler.BoneManagement;
using System;
using System.Linq;
using Rhinox.Grappler.EventManagement;

namespace Rhinox.Grappler.HandPhysics
{
    public class ContactPointBasedPhysics : BasePhysicsService
    {
        protected static GameObject LeftHandConnectedObject = null;
        protected static int LeftHandConnections = 0;
        protected static GameObject rightHandConnectedObject = null;
        protected static int RightHandConnections = 0;
        protected static HandPhysicsController _controller;

        public class ContactSensor
        {
            /// SETTINGS ///
            private float _detectDistance = 0.010f;
            private float _deadzone = 0.02f;
            private float _breakDistance = 0.40f;
            private float _forceMultiplier = 300.0f;

            public bool IsInitialised { get; private set; } = false;
            public bool IsEnabled { get; set; } = false;
            public ContactPoint ContactPoint { get => _contactPoint; set => _contactPoint = value; }

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
                if (!IsEnabled && ContactPoint != null)
                {
                    ContactPoint.Break();
                }
                else if (!IsEnabled)
                    return;
                    

                if (_rhinoxBone.BoneCollisionCapsules.Count <= 0)
                    return;

                if (ContactPoint == null)
                    FindContactPoint();
                else
                    HandleContactPoint();
            }

            public GameObject GetConnectedObject()
            {
                if (ContactPoint != null)
                    return ContactPoint.GetContactObject();
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
                            ContactPoint = new ContactPoint(collision.gameObject, _handedness, collision.ClosestPoint(origin));
                            return;
                        }
                    }
                }
            }
            private void HandleContactPoint()
            {
                var origin = GetCapsuleColliderOrigin();
                var distance = Vector3.Distance(origin, ContactPoint.getContactPosition());

                // check if the contact should break according to distance
                if (distance > _breakDistance)
                {
                    ContactPoint.Break();
                    ContactPoint = null;
                    return;
                }
                // calculate amount of force depending on distance from contactpoint to realpoint
                Vector3 delta = origin - ContactPoint.getContactPosition();

                // apply the force at the point of the contactpoint to the picked up object
                ContactPoint.EmitForce(delta * (distance * _forceMultiplier));
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
                    if (contactObject.GetComponent<NotGrabbable>())
                        return;

                    // prevents mutliple object grabbing
                    if (LeftHandConnectedObject != null && LeftHandConnectedObject != contactObject)
                        return;

                    // prevents grabbing an objecct already grabbed
                    if (contactObject == rightHandConnectedObject)
                        return;

                    LeftHandConnections++;
                    if (LeftHandConnections == 1)
                    {
                        _controller.MeshBakingService.BakeMesh(Hand.Left, contactObject);
                        LeftHandConnectedObject = contactObject;
                        LeftHandConnectedObject.GetComponent<Rigidbody>().isKinematic = false;
                        LeftHandConnectedObject.GetComponent<Rigidbody>().useGravity = false;
                        LeftHandConnectedObject.GetComponent<Rigidbody>().drag = 10;

                        GrapplerEventManager.Instance?.OnGrab?.Invoke(_contactPoint, LeftHandConnectedObject, Hand.Left);
                    }
                }
                else
                {
                    if (contactObject.GetComponent<NotGrabbable>())
                        return;

                    // prevents mutliple object grabbing
                    if (rightHandConnectedObject != null && rightHandConnectedObject != contactObject)
                        return;

                    // prevents grabbing an object already grabbed
                    if (contactObject == LeftHandConnectedObject)
                        return;

                    RightHandConnections++;
                    if (RightHandConnections == 1)
                    {
                        _controller.MeshBakingService.BakeMesh(Hand.Right, contactObject);
                        rightHandConnectedObject = contactObject;
                        rightHandConnectedObject.GetComponent<Rigidbody>().useGravity = false;
                        rightHandConnectedObject.GetComponent<Rigidbody>().drag = 10;

                        GrapplerEventManager.Instance?.OnGrab?.Invoke(_contactPoint, rightHandConnectedObject, Hand.Right);
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
                    if (LeftHandConnectedObject == null)
                        return;

                    LeftHandConnections--;
                    if (LeftHandConnections <= 0)
                    {
                        _controller.MeshBakingService.RemoveMesh(Hand.Left);
                        LeftHandConnectedObject.GetComponent<Rigidbody>().isKinematic = true;
                        LeftHandConnectedObject.GetComponent<Rigidbody>().useGravity = true;
                        LeftHandConnectedObject.GetComponent<Rigidbody>().drag = 0;
                        GrapplerEventManager.Instance?.OnDrop?.Invoke(_contactPoint, LeftHandConnectedObject, Hand.Left);
                        LeftHandConnectedObject = null;
                    }
                }
                else
                {
                    if (rightHandConnectedObject == null)
                        return;

                    RightHandConnections--;
                    if (RightHandConnections <= 0)
                    {
                        _controller.MeshBakingService.RemoveMesh(Hand.Right);
                        rightHandConnectedObject.GetComponent<Rigidbody>().useGravity = true;
                        rightHandConnectedObject.GetComponent<Rigidbody>().drag = 0;
                        GrapplerEventManager.Instance?.OnDrop?.Invoke(_contactPoint, rightHandConnectedObject, Hand.Right);
                        rightHandConnectedObject = null;
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
        private LayerMask _handLayer = -1;

        List<ContactSensor> _leftHandedSensorObjects = new List<ContactSensor>();
        private bool _isLeftHandEnabled = false;

        List<ContactSensor> _rightHandedSensorObjects = new List<ContactSensor>();
        private bool _isRightHandEnabled = false;

        private RhinoxBone _leftHandRoot = null;
        private RhinoxBone _rightHandRoot = null;

        private GameObject _leftHandDummyRoot = null;
        private GameObject _rightHandDummyRoot = null;

        private ConfigurableJoint _leftHandRotationalJoint = null;
        private ConfigurableJoint _rightHandRotationalJoint = null;
        public override bool GetIsEnabled(Hand handedness)
        {
            switch (handedness)
            {
                case Hand.Left:
                    return _isLeftHandEnabled;
                case Hand.Right:
                    return _isRightHandEnabled;
                case Hand.Both:
                    return _isLeftHandEnabled && _isRightHandEnabled;
            }
            return false;

        }

        public override bool GetIsInitialised()
        {
            return _isInitialised;
        }


        /// <summary>
        /// Gets the list of bones for both hands and finds the first bone for each(which is hopefully the root)
        /// After that it sets up de dummy object used for rotation calculations with the joint
        /// Then it sets up all the contact sensors for each bone
        /// </summary>
        /// <param name="boneManager"></param>
        /// <param name="controller"></param>
        public override void Initialise(BoneManager boneManager, HandPhysicsController controller)
        {
            _controller = controller;

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
        /// creates a dummy object with a joint and does the setup for it 
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

        public void BreakContacts(Hand hand)
        {
            switch (hand)
            {
                case Hand.Left:
                    foreach (var sensor in _leftHandedSensorObjects)
                    {
                        sensor.ContactPoint?.Break();
                    }
                    break;
                case Hand.Right:
                    foreach (var sensor in _rightHandedSensorObjects)
                    {
                        sensor.ContactPoint?.Break();
                    }
                    break;
                case Hand.Both:
                    foreach (var sensor in _leftHandedSensorObjects)
                    {
                        sensor.ContactPoint?.Break();
                    }
                    foreach (var sensor in _rightHandedSensorObjects)
                    {
                        sensor.ContactPoint?.Break();
                    }
                    break;
            }
        }

        public override void ManualUpdate()
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
                else if (rightHandConnectedObject == null)
                {
                    _rightHandRotationalJoint.connectedBody = null;
                }
            }
        }

        public override void SetEnabled(bool newState, Hand handedness)
        {
            switch (handedness)
            {
                case Hand.Left:
                    _isLeftHandEnabled = newState;
                    foreach (var contactSensor in _leftHandedSensorObjects)
                    {
                        contactSensor.IsEnabled = newState;
                    }
                    break;
                case Hand.Right:
                    _isRightHandEnabled = newState;
                    foreach (var contactSensor in _rightHandedSensorObjects)
                    {
                        contactSensor.IsEnabled = newState;
                    }
                    break;
                case Hand.Both:
                    _isLeftHandEnabled = newState;
                    foreach (var contactSensor in _leftHandedSensorObjects)
                    {
                        contactSensor.IsEnabled = newState;
                    }

                    _isRightHandEnabled = newState;
                    foreach (var contactSensor in _rightHandedSensorObjects)
                    {
                        contactSensor.IsEnabled = newState;
                    }
                    break;
            }

        }

        public override void SetHandLayer(LayerMask layer)
        {
            _handLayer = layer;
        }

    }

}
