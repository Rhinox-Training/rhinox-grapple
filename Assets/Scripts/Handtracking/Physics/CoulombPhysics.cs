using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Rhinox.Grappler.BoneManagement;
using System;

namespace Rhinox.Grappler.HandPhysics
{
    /// <summary>
    /// THIS IS SHELFED!
    /// COULOMB IS WAY TOO REALISTIC FOR CURRENT APPLICATIONS
    /// CODE IS BUSTED!
    /// </summary>
    
    [Obsolete("Non functioning prototype", true)]
    public class CoulombPhysics : IPhysicsService
    {

        class CoulombObject
        {
            public bool IsInitialised { get; private set; } = false;
            private RhinoxBone _rhinoxBone = null;
            private LayerMask _collisionLayer = 0;
            private Hand _handedness = Hand.Both;

            private ContactPoint _contactPoint = null;

            public CoulombObject(RhinoxBone bone, LayerMask collisionLayer, Hand handedness)
            {
                _rhinoxBone = bone;
                _collisionLayer = collisionLayer;
                _handedness = handedness;

                Initialise();
            }
            private void Initialise()
            {

                // make the old bone capsules a trigger, if it exists
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
                    FindCollisionObject();
                else
                    HandleCollision();


            }

            private void FindCollisionObject()
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
                var origin = _rhinoxBone.BoneCollisionCapsules[0].transform.TransformPoint(_rhinoxBone.BoneCollisionCapsules[0].center);
                Collider[] colls = Physics.OverlapSphere(origin, 0.05f, layerNumber, QueryTriggerInteraction.Ignore);

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
                            // find the contact point
                            _contactPoint = new ContactPoint(collision.gameObject, collision.ClosestPoint(origin));
                            return;
                        }
                    }
                }
            }

            private void HandleCollision()
            {

            }

        }

        class ContactPoint
        {
            GameObject _contactPoint = null;
            public ContactPoint(GameObject contactObject, Vector3 contactPosition)
            {
                _contactPoint = new GameObject("ContactoPoint_" + contactObject.name);
                _contactPoint.transform.position = contactPosition;
                _contactPoint.transform.parent = contactObject.transform;
            }
            public Vector3 getContactPosition(bool inLocal = false)
            {
                if (inLocal)
                    return _contactPoint.transform.localPosition;
                else
                    return _contactPoint.transform.position;
            }

        }

        List<CoulombObject> _coulombObjects = new List<CoulombObject>();
        private bool _isInitialised = false;
        private bool _isEnabled = false;
        private LayerMask _handLayer = -1;
        public bool GetIsEnabled()
        {
            return _isEnabled;
        }

        public bool GetIsInitialised()
        {
            return _isInitialised;
        }

        public void Initialise(BoneManager boneManager, HandPhysicsController controller)
        {
            List<RhinoxBone> leftHandBones = boneManager.GetRhinoxBones(Hand.Left);
            List<RhinoxBone> rightHandBones = boneManager.GetRhinoxBones(Hand.Right);

            foreach (var lhBone in leftHandBones)
            {
                _coulombObjects.Add(new CoulombObject(lhBone, _handLayer, Hand.Left));
            }
            foreach (var rhBone in rightHandBones)
            {
                _coulombObjects.Add(new CoulombObject(rhBone, _handLayer, Hand.Right));
            }
            _isInitialised = true;
        }

        public void SetEnabled(bool newState)
        {
            _isEnabled = newState;
        }

        public void SetHandLayer(LayerMask layer)
        {
            _handLayer = layer;
        }

        public void Update()
        {
            foreach (var CoulombObject in _coulombObjects)
            {
                CoulombObject.Update();
            }
        }

        public void SetEnabled(bool newState, Hand handedness)
        {
            throw new NotImplementedException();
        }

        public bool GetIsEnabled(Hand handedness)
        {
            throw new NotImplementedException();
        }
    }
}


