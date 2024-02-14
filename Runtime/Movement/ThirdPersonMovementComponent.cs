using UnityEngine;

namespace ThirdPersonComponents.Movement
{
    public class ThirdPersonMovementComponent : MovementComponent
    {
        [SerializeField] new Rigidbody rigidbody;

        [Header("Walking")]
        [SerializeField] float acceleration = 10;
        [SerializeField] public float rotationSpeed = 10;
        [SerializeField] bool autoRotate = true;

        [Header("Jumping")]
        [SerializeField] public float jumpHeight = 6;
        [SerializeField] public float groundCheckDistance = 1f;
        [SerializeField] public float groundCheckRadius = 1f;
        [SerializeField] public LayerMask groundLayer;

        public float Acceleration
        {
            get => acceleration;
            set => acceleration = Mathf.Max(0, value);
        }

        public float RotationSpeed
        {
            get => rotationSpeed;
            set => rotationSpeed = Mathf.Max(0, value);
        }

        public float JumpHeight
        {
            get => jumpHeight;
            set => jumpHeight = Mathf.Max(0, value);
        }

        public float GroundCheckDistance
        {
            get => groundCheckDistance;
            set => groundCheckDistance = Mathf.Max(0, value);
        }

        public float GroundCheckRadius
        {
            get => groundCheckRadius;
            set => groundCheckRadius = Mathf.Max(0, value);
        }

        public LayerMask GroundLayer
        {
            get => groundLayer;
            set => groundLayer = value;
        }

        float targetSpeed;
        float currentSpeed;
        Vector3 currentDirection;

        Quaternion targetRotation;
        Quaternion CurrentRotation => rigidbody.rotation;

        void FixedUpdate()
        {
            // Update speed
            currentSpeed = Mathf.MoveTowards(currentSpeed, targetSpeed, Time.fixedDeltaTime * acceleration);
            targetSpeed = 0;


            // Move character
            var velocity = currentDirection * currentSpeed;
            rigidbody.MovePosition(rigidbody.position + velocity * Time.fixedDeltaTime);

            if (!autoRotate) return;

            // Update rotation
            var lookRotation = Quaternion.Lerp(CurrentRotation, targetRotation, Time.fixedDeltaTime * rotationSpeed);
            rigidbody.MoveRotation(lookRotation);
        }

        /// <summary>
        /// Start moving the character in a direction.
        /// </summary>
        /// <param name="direction">the direction to move in.</param>
        /// <param name="forward">the forward direction of the camera.</param>
        /// <param name="speed">the speed to move at.</param>
        public override void Move(Vector2 direction, Vector3 forward, float speed)
        {
            if (direction.sqrMagnitude == 0) return;

            // Translate direction to forward vector
            forward.y = 0;
            var right = Vector3.Cross(Vector3.up, forward);
            var direction3 = new Vector3(direction.x, 0, direction.y);
            var translatedDirection = direction3.x * right + direction3.z * forward;

            // Set direction and target speed
            currentDirection = translatedDirection;

            // Set target rotation
            var velocity = currentDirection;
            targetRotation = Quaternion.LookRotation(velocity);

            targetSpeed = speed;
        }

        /// <summary>
        /// Makes character jump.
        /// </summary>
        public void Jump()
        {
            if (!CheckIfGrounded()) return;
            if (rigidbody.velocity.y > 0) return;

            rigidbody.velocity += new Vector3(rigidbody.velocity.x, jumpHeight, rigidbody.velocity.z);
        }

        /// <summary>
        /// Checks if the character is on the ground.
        /// </summary>
        /// <returns>true if character is on the ground.</returns>
        bool CheckIfGrounded()
        {
            var size = Physics.SphereCastNonAlloc(rigidbody.position, groundCheckRadius, Vector3.down, new RaycastHit[1], groundCheckDistance, groundLayer);
            return size > 0;
        }

        #if UNITY_EDITOR
        [Header("Debug")]
        [SerializeField] bool enableDebug;
        [SerializeField] Color debugColor = Color.red;

        void OnDrawGizmos()
        {
            if (!enableDebug) return;
            if (!rigidbody) return;

            Gizmos.color = debugColor;
            var endPosition = rigidbody.position + Vector3.down * groundCheckDistance;
            Gizmos.DrawLine(rigidbody.position, endPosition);
            Gizmos.DrawWireSphere(endPosition, groundCheckRadius);
        }
        #endif
    }
}