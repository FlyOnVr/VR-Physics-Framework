using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace VRPF.Framework.InteractionSystem.Inputs
{
    public class VRInteractor : MonoBehaviour
    {
        [Header("Interactor Settings")]
        public Transform grabAnchor;
        public LayerMask grabbableLayer;
        public float grabRadius = 0.15f;
        public bool useGripForGrab = true;
        public float grabThreshold = 0.8f;

        [Header("Events")]
        public UnityEvent<VRSelectEnterEventArgs> onSelectEnter;
        public UnityEvent<VRSelectExitEventArgs> onSelectExit;

        private VRInputDevice input;
        private VRGrabbable grabbed;
        private Vector3 lastPos;
        private Vector3 velocity;
        private Queue<Vector3> velHistory = new();

        void Awake()
        {
            input = GetComponent<VRInputDevice>();
            if (!grabAnchor) grabAnchor = transform;
        }

        void Update()
        {
            velocity = (transform.position - lastPos) / Time.deltaTime;
            lastPos = transform.position;
            velHistory.Enqueue(velocity);
            if (velHistory.Count > 5) velHistory.Dequeue();

            // Check grab/release
            bool grabPressed = useGripForGrab ? input.grip > grabThreshold : input.trigger > grabThreshold;
            bool grabReleased = useGripForGrab ? input.grip < 0.1f : input.trigger < 0.1f;

            if (grabPressed && !grabbed)
                TryGrab();
            else if (grabReleased && grabbed)
                Release();
        }

        void TryGrab()
        {
            Collider[] hits = Physics.OverlapSphere(grabAnchor.position, grabRadius, grabbableLayer);
            foreach (var h in hits)
            {
                VRGrabbable g = h.GetComponentInParent<VRGrabbable>();
                if (g && !g.isGrabbed)
                {
                    grabbed = g;
                    grabbed.OnGrab(this, grabAnchor);

                    var args = new VRSelectEnterEventArgs(this, g, grabAnchor.position);
                    onSelectEnter?.Invoke(args);
                    g.onSelectEnter?.Invoke(args);
                    return;
                }
            }
        }

        void Release()
        {
            if (!grabbed) return;

            Vector3 avgVel = Vector3.zero;
            foreach (var v in velHistory) avgVel += v;
            avgVel /= velHistory.Count;

            grabbed.OnRelease(avgVel);
            var args = new VRSelectExitEventArgs(this, grabbed);
            onSelectExit?.Invoke(args);
            grabbed.onSelectExit?.Invoke(args);

            grabbed = null;
        }

        void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(grabAnchor ? grabAnchor.position : transform.position, grabRadius);
        }
    }
}
