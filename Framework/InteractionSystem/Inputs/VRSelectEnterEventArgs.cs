using UnityEngine;

namespace VRPF.Framework.InteractionSystem.Inputs
{
    public class VRSelectEnterEventArgs
    {
        public VRInteractor interactor;
        public VRGrabbable interactable;
        public Vector3 grabPoint;

        public VRSelectEnterEventArgs(VRInteractor interactor, VRGrabbable interactable, Vector3 grabPoint)
        {
            this.interactor = interactor;
            this.interactable = interactable;
            this.grabPoint = grabPoint;
        }
    }

    public class VRSelectExitEventArgs
    {
        public VRInteractor interactor;
        public VRGrabbable interactable;

        public VRSelectExitEventArgs(VRInteractor interactor, VRGrabbable interactable)
        {
            this.interactor = interactor;
            this.interactable = interactable;
        }
    }
}