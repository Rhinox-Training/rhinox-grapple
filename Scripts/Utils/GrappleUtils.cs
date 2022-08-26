using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Rhinox.Grappler.HandPhysics
{
    public class GrappleUtils
    {
        public static void CopyCapsuleColliderValues(CapsuleCollider to, CapsuleCollider from)
        {
            to.contactOffset = from.contactOffset;
            to.direction = from.direction;
            to.enabled = from.enabled;
            to.height = from.height;
            to.radius = from.radius;
            to.isTrigger = from.isTrigger;
            to.material = from.material;
            to.center = from.center;
        }
    }
}
