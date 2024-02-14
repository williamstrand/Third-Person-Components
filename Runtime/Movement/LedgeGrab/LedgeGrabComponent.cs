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
            if (!IsMoving && MoveDelayEnabled)
            {
                moveDelayTimer -= Time.deltaTime;
            }

            if (GrabGraceEnabled)
            {
                grabGraceTimer -= Time.deltaTime;
                return;
            }

            if (IsGrabbingLedge) return;
            if (!CheckForLedge(out var ledge)) return;

            OnLedgeGrab?.Invoke();
            AttachToLedge(ledge);
        }

        void FixedUpdate()
        {
            if (!IsGrabbingLedge) return;
            if (!IsMoving) return;

            characterTransform.position = Vector3.MoveTowards(characterTransform.position, targetPosition, Time.fixedDeltaTime * currentSpeed);
            characterTransform.forward = Vector3.MoveTowards(characterTransform.forward, targetDirection, Time.fixedDeltaTime * currentSpeed);
        }

        public void Release()
        {
            rigidbody.useGravity = true;
            IsGrabbingLedge = false;
            grabGraceTimer = grabGracePeriod;
            OnLedgeRelease?.Invoke();
        }

        public override void Move(Vector2 direction, Vector3 forward, float speed)
        {
            if (!IsGrabbingLedge) return;
            if (direction.sqrMagnitude == 0) return;
            if (IsMoving) return;
            if (MoveDelayEnabled) return;
            if (Mathf.Abs(direction.x) < XMoveThreshold) return;

            var right = Vector3.Cross(Vector3.up, forward);
            var translatedDirection = direction.x * right + direction.y * forward;

            var moveDirection = characterTransform.right * (Mathf.Sign(translatedDirection.x) * moveDistance);

            if (!CheckForLedge(out var hitInfo, characterTransform.position + moveDirection, characterTransform.forward))
            {
                if (!CheckForLedge(out hitInfo, characterTransform.position + moveDirection / 2, characterTransform.forward)) return;
            }

            currentSpeed = speed;
            AttachToLedge(hitInfo);
        }

        bool CheckForLedge(out RaycastHit hitInfo)
        {
            return CheckForLedge(out hitInfo, characterTransform.position, characterTransform.forward);
        }

        bool CheckForLedge(out RaycastHit hitInfo, Vector3 origin, Vector3 direction)
        {
            return Physics.Raycast(origin, direction, out hitInfo, grabRange, grabbableLayers);
        }

        void AttachToLedge(RaycastHit ledge)
        {
            rigidbody.useGravity = false;
            rigidbody.velocity = Vector3.zero;

            IsGrabbingLedge = true;
            moveDelayTimer = moveDelay;

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