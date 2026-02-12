using Unity.XR.CoreUtils;
using UnityEngine;

namespace VRPhysicsFramework
{
    public class PhysicsRig : MonoBehaviour
    {
        public Transform playerHead;

        public Transform rightController;
        public Transform leftController;

        public XROrigin origin;

        public CapsuleCollider bodyCollider;

        public ConfigurableJoint headJoint;
        public ConfigurableJoint leftHandJoint;
        public ConfigurableJoint rightHandJoint;

        public float bodyHeightMin = 0.5f;
        public float bodyHeightMax = 2f;


        void Start()
        {

        }

        // Update is called once per frame
        void FixedUpdate()
        {
            bodyCollider.height = Mathf.Clamp(playerHead.localPosition.y, bodyHeightMin, bodyHeightMax);
            bodyCollider.center = new Vector3(playerHead.localPosition.x, bodyCollider.height / 2, playerHead.localPosition.z);

            leftHandJoint.targetPosition = leftController.localPosition;
            leftHandJoint.targetRotation = leftController.localRotation;
            rightHandJoint.targetPosition = rightController.localPosition;
            rightHandJoint.targetRotation = rightController.localRotation;

            headJoint.targetPosition = playerHead.localPosition;
        }
    }
}
