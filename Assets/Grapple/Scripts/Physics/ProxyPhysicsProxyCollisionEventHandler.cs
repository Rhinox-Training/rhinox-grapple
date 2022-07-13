using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Rhinox.Grappler.BoneManagement;

namespace Rhinox.Grappler.HandPhysics
{

    public class ProxyPhysicsProxyCollisionEventHandler : MonoBehaviour
    {
        public Hand _handdedness { get; private set; } = Hand.Both;
        private bool _allowTriggers = false;

        public void Initialise(Hand handedness, bool allowTriggers = false)
        {
            _handdedness = handedness;
            _allowTriggers = allowTriggers;
        }
        private void OnCollisionEnter(Collision collision)
        {
            Grappler.EventManagement.GrapplerEventManager.Instance.OnTouch.Invoke(this.gameObject, collision.gameObject, _handdedness);
        }
        private void OnCollisionExit(Collision collision)
        {
            Grappler.EventManagement.GrapplerEventManager.Instance.OnUnTouched.Invoke(this.gameObject, collision.gameObject, _handdedness);
        }
        private void OnTriggerEnter(Collider other)
        {
            if (!_allowTriggers)
                return;
            Grappler.EventManagement.GrapplerEventManager.Instance.OnTouch.Invoke(this.gameObject, other.gameObject, _handdedness);
        }
        private void OnTriggerExit(Collider other)
        {
            if (!_allowTriggers)
                return;
            Grappler.EventManagement.GrapplerEventManager.Instance.OnUnTouched.Invoke(this.gameObject, other.gameObject, _handdedness);
        }
    }
}
