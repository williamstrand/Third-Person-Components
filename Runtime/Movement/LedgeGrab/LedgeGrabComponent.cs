using System;
using UnityEngine;

namespace ThirdPersonComponents.Movement.LedgeGrab
{
    public class LedgeGrabComponent : MovementComponent
    {
        const float XMoveThreshold = .5f;

        [SerializeField] new Rigidbody rigidbody;

        [Header("Grabbing Ledge")]
        [SerializeField] LayerMask grabbableLayers;
        [SerializeField] float grabRange = 0.5f;
        [SerializeField] float grabGracePeriod = 0.5f;

        [Header("Moving On Ledge")]
        [SerializeField] float moveDelay = .2f;
        [SerializeField] float moveDistance = .5f;

        public Action OnLedgeGrab { get; set; }
        public Action OnLedgeRelease { get; set; }
        public bool IsGrabbingLedge { get; private set; }

        bool IsMoving => transform.position != targetPosition || transform.forward != targetDirection;
        bool GrabGraceEnabled => grabGraceTimer > 0;
        bool MoveDelayEnabled => moveDelayTimer > 0;

        Vector3 targetPosition;
        Vector3 targetDirection;
        float currentSpeed = 5;

        float grabGraceTimer;
        float moveDelayTimer;

        Transform characterTransform;

        void Start()
        {
            characterTransform = transform;
        }

        void Update()
        {
            // If the character is not moving and the move delay is enabled, decrease the move delay timer
            if (!IsMoving && MoveDelayEnabled)
            {
                moveDelayTimer -= Time.deltaTime;
            }

            // If the grab grace is enabled, decrease the grab grace timer and return
            if (GrabGraceEnabled)
            {
                grabGraceTimer -= Time.deltaTime;
                return;
            }

            // If the character is not grabbing the ledge, check for a ledge
            if (IsGrabbingLedge) return;
            if (!CheckForLedge(out var ledge)) return;

            // Attach to the found ledge
            OnLedgeGrab?.Invoke();
            AttachToLedge(ledge);
        }

        void FixedUpdate()
        {
            if (!IsGrabbingLedge) return;
            if (!IsMoving) return;

            // Update character position and rotation
            characterTransform.position = Vector3.MoveTowards(characterTransform.position, targetPosition, Time.fixedDeltaTime * currentSpeed);
            characterTransform.forward = Vector3.MoveTowards(characterTransform.forward, targetDirection, Time.fixedDeltaTime * currentSpeed);
        }

        /// <summary>
        /// Drop from the ledge.
        /// </summary>
        public void Release()
        {
            rigidbody.useGravity = true;
            IsGrabbingLedge = false;
            grabGraceTimer = grabGracePeriod;
            OnLedgeRelease?.Invoke();
        }

        /// <summary>
        /// Moves the character on the ledge.
        /// </summary>
        /// <param name="direction">the direction to move in.</param>
        /// <param name="forward">the forward direction of the camera.</param>
        /// <param name="speed">the speed to move at.</param>
        public override void Move(Vector2 direction, Vector3 forward, float speed)
        {
            if (!IsGrabbingLedge) return;
            if (direction.sqrMagnitude == 0) return;
            if (IsMoving) return;
            if (MoveDelayEnabled) return;
            if (Mathf.Abs(direction.x) < XMoveThreshold) return;

            // Translate direction to camera local space
            var right = Vector3.Cross(Vector3.up, forward);
            var translatedDirection = direction.x * right + direction.y * forward;

            // Check for a ledge in the direction of movement
            var moveDirection = characterTransform.right * (Mathf.Sign(translatedDirection.x) * moveDistance);
            if (!CheckForLedge(out var hitInfo, characterTransform.position + moveDirection, characterTransform.forward))
            {
                // Check for a ledge in the direction of movement, but closer to the character
                if (!CheckForLedge(out hitInfo, characterTransform.position + moveDirection / 2, characterTransform.forward)) return;
            }
            
            currentSpeed = speed;
            AttachToLedge(hitInfo);
        }

        /// <summary>
        /// Checks for a ledge in front of the character.
        /// </summary>
        /// <param name="hitInfo">the RaycastHit of the found ledge.</param>
        /// <returns>true if a ledge was found.</returns>
        bool CheckForLedge(out RaycastHit hitInfo)
        {
            return CheckForLedge(out hitInfo, characterTransform.position, characterTransform.forward);
        }

        /// <summary>
        /// Checks for a ledge at target position.
        /// </summary>
        /// <param name="hitInfo">the RaycastHit of the found ledge.</param>
        /// <param name="origin">the origin position of the check.</param>
        /// <param name="direction">the direction of the check.</param>
        /// <returns>true if a ledge was found.</returns>
        bool CheckForLedge(out RaycastHit hitInfo, Vector3 origin, Vector3 direction)
        {
            return Physics.Raycast(origin, direction, out hitInfo, grabRange, grabbableLayers);
        }

        /// <summary>
        /// Attaches character to a ledge.
        /// </summary>
        /// <param name="ledge">the ledge to attach to.</param>
        void AttachToLedge(RaycastHit ledge)
        {
            // Disable gravity and reset velocity
            rigidbody.useGravity = false;
            rigidbody.velocity = Vector3.zero;

            IsGrabbingLedge = true;
            moveDelayTimer = moveDelay;

            // Set target position and direction
            targetPosition = ledge.point + ledge.normal * grabRange / 2;
            targetPosition.y = ledge.transform.position.y;
            targetDirection = -ledge.normal;

            #if UNITY_EDITOR
            if (enableDebug) Debug.DrawRay(ledge.point, ledge.normal * grabRange / 2, Color.red, 1f);
            #endif
        }

        #if UNITY_EDITOR
        [Header("Debug")]
        [SerializeField] bool enableDebug = true;

        void OnDrawGizmos()
        {
            if (!enableDebug) return;

            Gizmos.color = Color.red;
            Gizmos.DrawRay(transform.position, transform.forward * grabRange);
        }
        #endif
    }
}