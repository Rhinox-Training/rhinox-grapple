#if USING_VOLT

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.XR;
using UnityEngine.InputSystem.LowLevel;

using Rhinox.Grappler.BoneManagement;
using Rhinox.Grappler.Recognition;

using Rhinox.VOLT;
using Rhinox.VOLT.XR;
using Rhinox.VOLT.XR.UnityXR;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.Events;
using System;
using UnityEngine.InputSystem.Utilities;
using Rhinox.Grappler.EventManagement.VOLT;

namespace Rhinox.Grappler.Teleportation.VOLT
{
    public class GrapplerTeleportController : MonoBehaviour
    {


        private XRRayInteractor _teleportInteractor;

        GrapplerFakeXRDevice _fakeInputDevice;

        [Header("Settings")]
        [SerializeField] private Rhinox.VOLT.Hand _handedness = Rhinox.VOLT.Hand.Right;

        [Header("Referneces")]
        [SerializeField] private Transform _interactorsRoot = null;

        [Header("Debug")]
        [SerializeField] private bool _press;

        private void Start()
        {
            // child teleporter to the handtracked hand
            // BAD! this is now hard depending on telerik but fuck it
            _interactorsRoot.parent = GetComponentInChildren<Telerik.Unity.XR.Rig.Tracking.TrackingHandPose>().transform;
            _interactorsRoot.localPosition = new Vector3(0, 0, 0);
        }

        private void Update()
        {
            HandleInputFaking();
        }

        private void HandleInputFaking()
        {
            switch (_handedness)
            {
                case Rhinox.VOLT.Hand.Left:
                    if (_press)
                        GrapplerVoltTeleportManager._instance.DeviceState.simulatedLeftTrigger = new Vector2(0, 10);
                    else
                        GrapplerVoltTeleportManager._instance.DeviceState.simulatedLeftTrigger = new Vector2(0, 0);
                    break;
                case Rhinox.VOLT.Hand.Right:
                    if (_press)
                        GrapplerVoltTeleportManager._instance.DeviceState.simulatedRightTrigger = new Vector2(0, 10);
                    else
                        GrapplerVoltTeleportManager._instance.DeviceState.simulatedRightTrigger = new Vector2(0, 0);
                    break;
            }
        }

        public void StartTeleport()
        {
            _press = true;
        }

        public void StopTeleport()
        {
            // with the current volt implementation it is currently impossible to make this happen without adaptiong VOlT.XR.UNITYXR

            throw new NotImplementedException();
        }

        public void ConfirmTeleport()
        {
            _press = false;
        }
    }
}

#endif