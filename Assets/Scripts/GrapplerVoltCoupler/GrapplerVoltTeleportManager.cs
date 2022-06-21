#if USING_VOLT
using Rhinox.Grappler.Teleportation.VOLT;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Rhinox.Grappler.EventManagement.VOLT
{
    public class GrapplerVoltTeleportManager : MonoBehaviour
    {
        public GrapplerFakeXRDeviceStates DeviceState;
        public GrapplerFakeXRDevice Device;

        public bool IsInitialised { get; private set; } = false;

        public static GrapplerVoltTeleportManager _instance {  get; private set; }

        private void Awake()
        {
            InputSystem.FlushDisconnectedDevices();
            var prevFake = InputSystem.GetDevice("GrapplerFakeXRDevice");
            if (prevFake != null)
                Device = prevFake as GrapplerFakeXRDevice;

            if (Device == null)
                Device = InputSystem.AddDevice<GrapplerFakeXRDevice>("GrapplerFakeXRDevice");

            if (_instance != null)
            {
                Debug.LogError("There can only be one GrapplerVoltTeleportManager");
                return;
            }

            _instance = this;

            IsInitialised = true;
        }

        private void Update()
        {
            if (!IsInitialised)
                return;

            InputSystem.QueueStateEvent(Device, DeviceState);
        }
    }

    
}
#endif
