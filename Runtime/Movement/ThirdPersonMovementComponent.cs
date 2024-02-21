using ThirdPersonComponents.Extensions;
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

        public Vector3 TargetVelocity
        {
            get => targetVelocity;
            set => targetVelocity = value;
        }

        public Vector3 Velocity
        {
            get => rigidbody.velocity;
            set => rigidbody.velocity = new Vector3(value.x, 0, value.z);
        }

        public bool AutoRotate
        {
            get => autoRotate;
            set => autoRotate = value;
        }

        public bool LockVelocity { get; set; }

        Vector3 targetVelocity;

        Quaternion targetRotation;
        Quaternion CurrentRotation => rigidbody.rotation;

        void FixedUpdate()
        {
            UpdateVelocity();
            UpdateRotation();
        }

        /// <summary>
        ///     Updates the rotation of the character.
        /// </summary>
        void UpdateRotation()
        {
            if (!autoRotate) return;

            // Update rotation
            var lookRotation = Quaternion.Lerp(CurrentRotation, targetRotation, Time.fixedDeltaTime * rotationSpeed);
            rigidbody.MoveRotation(lookRotation);
        }

        /// <summary>
        ///     Updates the velocity of the character.
        /// </summary>
        void UpdateVelocity()
        {
            if (LockVelocity) return;

            // Update velocity
            targetVelocity.y = rigidbody.velocity.y;
            rigidbody.velocity = Vector3.MoveTowards(rigidbody.velocity, targetVelocity, Time.fixedDeltaTime * acceleration);
            targetVelocity = Vector3.zero;
        }

        /// <summary>
        ///     Start moving the character in a direction.
        /// </summary>
        /// <param name="direction">the direction to move in.</param>
        /// <param name="speed">the speed to move at.</param>
        /// <param name="forward">the forward direction of the camera.</param>
        public override void Move(Vector2 direction, float speed, Vector3 forward)
        {
            if (direction.sqrMagnitude == 0) return;

            // Translate direction to camera local space
            var translatedDirection = direction.ToLocalSpace(forward.Flatten());

            // Set direction and target speed
            targetVelocity = translatedDirection * speed;

            // Set target rotation
            var velocity = targetVelocity.normalized;
            velocity.y = 0;
            targetRotation = Quaternion.LookRotation(velocity);
        }

        public void Rotate(Vector2 direction)
        {
            var angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            targetRotation = Quaternion.Euler(0, angle, 0);
        }

        /// <summary>
        ///     Makes character jump.
        /// </summary>
        public void Jump()
        {
            if (!CheckIfGrounded()) return;
            if (rigidbody.velocity.y > 0) return;

            rigidbody.velocity += new Vector3(0, jumpHeight, 0);
        }

        /// <summary>
        ///     Checks if the character is on the ground.
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