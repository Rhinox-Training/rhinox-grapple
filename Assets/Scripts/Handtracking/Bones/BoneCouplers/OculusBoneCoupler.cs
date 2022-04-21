using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace Rhinox.Grappler.BoneManagement
{
    public class OculusBoneCoupler : MonoBehaviour
    {

        [SerializeField] private OVRSkeleton m_skeletonRefLeftHand = null;
        [SerializeField] private OVRSkeleton m_skeletonRefRightHand = null;

        private void Awake()
        {
        }
    }
}

