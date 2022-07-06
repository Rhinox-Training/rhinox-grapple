using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Rhinox.Grappler.BoneManagement;

namespace Rhinox.Grappler.HandPhysics
{

    public class ProxyPhysicsProxyCollisionEventHandler : MonoBehaviour
    {
        private Hand _handdedness = Hand.Both;

        public void Initialise(Hand handedness)
        {
            _handdedness = handedness;
        }
        private void OnCollisionEnter(Collision collision)
        {
            Grappler.EventManagement.GrapplerEventManager.Instance.OnTouch.Invoke(this.gameObject, collision.gameObject, _handdedness);
        }

        private void OnCollisionExit(Collision collision)
        {
            Grappler.EventManagement.GrapplerEventManager.Instance.OnUnTouched.Invoke(this.gameObject, collision.gameObject, _handdedness);
        }
    }

}
