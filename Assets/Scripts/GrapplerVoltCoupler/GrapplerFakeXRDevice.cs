using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using UnityEngine.InputSystem.Layouts;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.InputSystem.Utilities;


//https://github.com/Unity-Technologies/InputSystem/blob/develop/Packages/com.unity.inputsystem/Documentation~/Devices.md#creating-custom-devices

namespace Rhinox.Grappler.Teleportation.VOLT
{
    public struct GrapplerFakeXRDeviceStates : IInputStateTypeInfo
    {
        public FourCC format => new FourCC('G', 'R', 'P', 'L');

        [InputControl(name = "simulated right XR trigger", layout = "Vector2")]
        public Vector2 simulatedRightTrigger;

        [InputControl(name = "simulated left XR trigger", layout = "Vector2")]
        public Vector2 simulatedLeftTrigger;
    }



#if UNITY_EDITOR
    [InitializeOnLoad]
#endif
    [InputControlLayout(displayName = "GrapplerFakeXRDevice", stateType = typeof(GrapplerFakeXRDeviceStates))]
    public class GrapplerFakeXRDevice : InputDevice
    {
        public Vector2Control simulatedRightTriggerButton { get; private set; }
        public Vector2Control simulatedLeftTriggerButton { get; private set; }

        #region setup

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void InitializeInPlayer() { }
        static GrapplerFakeXRDevice()
        {
            InputSystem.RegisterLayout<GrapplerFakeXRDevice>();
        }

        #endregion

        protected override void FinishSetup()
        {
            base.FinishSetup();

            simulatedRightTriggerButton = GetChildControl<Vector2Control>("simulated right XR trigger");

            simulatedLeftTriggerButton = GetChildControl<Vector2Control>("simulated left XR trigger");
        }
    }
}

