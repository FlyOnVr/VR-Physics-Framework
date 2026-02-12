using Unity.Mathematics;
using UnityEngine;
using UnityEngine.InputSystem;

namespace VRPhysicsFramework
{
    public class PhysicsBasedMovement : MonoBehaviour
    {
        [Header("Velocity And Speed")]
        public float turnspeed = 60;
        public float speed = 1;
        private float jumpVel = 7;
        public float minJumpWithHandSpeed = 2;
        public float maxJumpWithHandSpeed = 7;
        public float jumpHeight = 1.5f;
        [Header("Sources")]
        public InputActionProperty moveSource;
        public InputActionProperty turnSource;
        public InputActionProperty jumpInputSource;
        [Header("Rigidbody Stuff")]
        public Rigidbody rb;
        public Rigidbody leftHandRB;
        public Rigidbody rightHandRB;
        [Header("Transforms")]
        public Transform directions;
        public Transform turnSourcse;
        private Vector2 moveAxis;
        [Header("Body Collider")]
        public CapsuleCollider bodyCollider;
        [Header("Other/Optional")]
        public LayerMask groundLayer;
        public bool onlyMoveGrounded = false;
        public bool jumpWithHand = true;
        private float TurnAxis;
        private bool isGrounded;


        void Update()
        {
            moveAxis = moveSource.action.ReadValue<Vector2>();
            TurnAxis = turnSource.action.ReadValue<Vector2>().x;

            bool jumping = jumpInputSource.action.WasPressedThisFrame();

            if (jumpWithHand)
            {
                if (jumping && isGrounded)
                {
                    jumpVel = Mathf.Sqrt(2 * -Physics.gravity.y * jumpHeight);
                    rb.linearVelocity += Vector3.up * jumpVel;
                }
            }
            else
            {
                bool inputJumpPressed = jumpInputSource.action.IsPressed();

                float handSpeed = ((leftHandRB.linearVelocity - rb.linearVelocity).magnitude +
                                   (rightHandRB.linearVelocity - rb.linearVelocity).magnitude) / 2;

                if (inputJumpPressed && isGrounded && handSpeed > minJumpWithHandSpeed)
                {
                    rb.linearVelocity = Vector3.up * Mathf.Clamp(handSpeed, minJumpWithHandSpeed, maxJumpWithHandSpeed);
                }
            }
        }

        private void FixedUpdate()
        {
            isGrounded = CheckIfGrounded();

            if (!onlyMoveGrounded || (onlyMoveGrounded && isGrounded))
            {
                Quaternion q = Quaternion.Euler(0, directions.eulerAngles.y, 0);
                Vector3 direction = q * new Vector3(moveAxis.x, 0, moveAxis.y);

                Vector3 targetmovePostition = rb.position + direction * Time.fixedDeltaTime * speed;

                Vector3 axis = Vector3.up;
                float angle = turnspeed * Time.fixedDeltaTime * TurnAxis;

                Quaternion qs = Quaternion.AngleAxis(angle, axis);

                rb.MoveRotation(rb.rotation * qs);

                Vector3 newPosition = qs * (targetmovePostition - turnSourcse.position) + turnSourcse.position;

                rb.MovePosition(newPosition);
            }
        }

        public bool CheckIfGrounded()
        {
            Vector3 start = bodyCollider.transform.TransformPoint(bodyCollider.center); 
            float rayLenght = bodyCollider.height / 2 - bodyCollider.radius + 0.05f;

            bool hasHit = Physics.SphereCast(start, bodyCollider.radius, Vector3.down, out RaycastHit hitInfo, rayLenght, groundLayer);

            return hasHit;
        }
    }
}
