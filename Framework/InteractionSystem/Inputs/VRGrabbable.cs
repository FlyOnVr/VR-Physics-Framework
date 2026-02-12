using UnityEngine;
using UnityEngine.Events;

namespace VRPF.Framework.InteractionSystem.Inputs
{
    [RequireComponent(typeof(Rigidbody))]
    public class VRGrabbable : MonoBehaviour
    {
        public UnityEvent<VRSelectEnterEventArgs> onSelectEnter;
        public UnityEvent<VRSelectExitEventArgs> onSelectExit;

        [HideInInspector] public bool isGrabbed;
        private Rigidbody rb;
        private Transform parentBeforeGrab;
        private Transform grabAnchor;

        void Awake()
        {
            rb = GetComponent<Rigidbody>();
        }

        public void OnGrab(VRInteractor interactor, Transform anchor)
        {
            isGrabbed = true;
            parentBeforeGrab = transform.parent;
            grabAnchor = anchor;
            rb.isKinematic = true;
            transform.SetParent(anchor, worldPositionStays: false);
        }

        public void OnRelease(Vector3 throwVelocity)
        {
            isGrabbed = false;
            transform.SetParent(parentBeforeGrab, true);
            rb.isKinematic = false;
            rb.linearVelocity = throwVelocity;
        }
    }
}