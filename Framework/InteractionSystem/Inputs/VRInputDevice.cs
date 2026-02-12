using UnityEngine;
using UnityEngine.XR;
using System.Collections.Generic;

namespace VRPF.Framework.InteractionSystem.Inputs
{
    public class VRInputDevice : MonoBehaviour
    {
        public enum HandSide { Left, Right }
        public HandSide handSide;

        public float grip;
        public float trigger;
        public bool primaryButton;
        public bool secondaryButton;
        public Vector2 joystick;

        private InputDevice device;

        void Start()
        {
            InitializeDevice();
        }

        void InitializeDevice()
        {
            var devices = new List<InputDevice>();
            InputDeviceCharacteristics handedness = handSide == HandSide.Left ?
                InputDeviceCharacteristics.Left : InputDeviceCharacteristics.Right;
            InputDevices.GetDevicesWithCharacteristics(InputDeviceCharacteristics.Controller | handedness, devices);
            if (devices.Count > 0) device = devices[0];
        }

        void Update()
        {
            if (!device.isValid) InitializeDevice();

            // Read trigger & grip
            device.TryGetFeatureValue(CommonUsages.grip, out grip);
            device.TryGetFeatureValue(CommonUsages.trigger, out trigger);

            // Read primary (A/X) & secondary (B/Y)
            device.TryGetFeatureValue(CommonUsages.primaryButton, out primaryButton);
            device.TryGetFeatureValue(CommonUsages.secondaryButton, out secondaryButton);

            // Read joystick
            device.TryGetFeatureValue(CommonUsages.primary2DAxis, out joystick);
        }
    }
}