using UnityEngine;

namespace Components
{
    public class MovementComponent : MonoBehaviour
    {
        [SerializeField] new Rigidbody rigidbody;

        [Header("Walking")]
        [SerializeField] float acceleration = 10;

        [SerializeField] float rotationSpeed = 10;

        [Header("Jumping")]
        [SerializeField] float jumpHeight = 6;

        [SerializeField] float groundCheckDistance = 1f;
        [SerializeField] float groundCheckRadius = 1f;
        [SerializeField] LayerMask groundLayer;

        float targetSpeed;
        float currentSpeed;
        Vector3 currentDirection;

        Quaternion targetRotation;
        Quaternion CurrentRotation => rigidbody.rotation;

        void Update()
        {
            // Update speed
            currentSpeed = Mathf.MoveTowards(currentSpeed, targetSpeed, Time.deltaTime * acceleration);
            targetSpeed = 0;
        }

        void FixedUpdate()
        {
            // Move character
            var velocity = currentDirection * currentSpeed;
            rigidbody.MovePosition(rigidbody.position + velocity * Time.fixedDeltaTime);

            // Update rotation
            var lookRotation = Quaternion.Lerp(CurrentRotation, targetRotation, Time.fixedDeltaTime * rotationSpeed);
            rigidbody.MoveRotation(lookRotation);
        }

        public void Move(Vector2 direction, float speed)
        {
            if (direction.sqrMagnitude == 0) return;

            // Set direction and target speed
            currentDirection = new Vector3(direction.x, 0, direction.y);
            targetSpeed = speed;

            // Set target rotation
            var velocity = currentDirection * targetSpeed;
            targetRotation = Quaternion.LookRotation(velocity);
        }

        public void Move(Vector2 direction, float speed, Vector3 forward)
        {
            if (direction.sqrMagnitude == 0) return;

            // Translate direction to forward vector
            var right = Vector3.Cross(Vector3.up, forward);
            var direction3 = new Vector3(direction.x, 0, direction.y);
            var translatedDirection = direction3.x * right + direction3.z * forward;

            Move(new Vector2(translatedDirection.x, translatedDirection.z), speed);
        }

        public void Jump()
        {
            if (!CheckIfGrounded()) return;
            if (rigidbody.velocity.y > 0) return;

            rigidbody.velocity += new Vector3(rigidbody.velocity.x, jumpHeight, rigidbody.velocity.z);
        }

        bool CheckIfGrounded()
        {
            var size = Physics.SphereCastNonAlloc(rigidbody.position, groundCheckRadius, Vector3.down, new RaycastHit[1], groundCheckDistance, groundLayer);
            return size > 0;
        }

        #if UNITY_EDITOR
        void OnDrawGizmosSelected()
        {
            if (!rigidbody) return;

            Gizmos.color = Color.red;
            var endPosition = rigidbody.position + Vector3.down * groundCheckDistance;
            Gizmos.DrawLine(rigidbody.position, endPosition);
            Gizmos.DrawWireSphere(endPosition, groundCheckRadius);
        }
        #endif
    }
}