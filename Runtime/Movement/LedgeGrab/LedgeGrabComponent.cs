using System;
using UnityEngine;

namespace ThirdPersonComponents.Movement.LedgeGrab
{
    public class LedgeGrabComponent : MonoBehaviour
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
        [SerializeField] float moveSpeed = 5;

        public Action OnLedgeGrab { get; set; }
        public Action OnLedgeRelease { get; set; }
        public bool IsGrabbingLedge { get; private set; }
        public float MoveSpeed
        {
            get => moveSpeed;
            set => moveSpeed = Mathf.Max(0, value);
        }

        public bool IsMoving => transform.position != targetPosition || transform.forward != targetDirection;
        public bool GrabGraceEnabled => grabGraceTimer > 0;
        public bool MoveDelayEnabled => moveDelayTimer > 0;

        Vector3 targetPosition;
        Vector3 targetDirection;

        float grabGraceTimer;
        float moveDelayTimer;

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

            transform.position = Vector3.MoveTowards(transform.position, targetPosition, Time.fixedDeltaTime * MoveSpeed);
            transform.forward = Vector3.MoveTowards(transform.forward, targetDirection, Time.fixedDeltaTime * MoveSpeed);
        }

        public void Release()
        {
            rigidbody.useGravity = true;
            IsGrabbingLedge = false;
            grabGraceTimer = grabGracePeriod;
            OnLedgeRelease?.Invoke();
        }

        public void Move(Vector2 direction)
        {
            if (!IsGrabbingLedge) return;
            if (direction.sqrMagnitude == 0) return;
            if (IsMoving) return;
            if (MoveDelayEnabled) return;
            if (Mathf.Abs(direction.x) < XMoveThreshold) return;

            var moveDirection = transform.right * (Mathf.Sign(direction.x) * moveDistance);

            if (!CheckForLedge(out var hitInfo, transform.position + moveDirection, transform.forward))
            {
                if (!CheckForLedge(out hitInfo, transform.position + moveDirection / 2, transform.forward)) return;
            }

            AttachToLedge(hitInfo);
        }

        bool CheckForLedge(out RaycastHit hitInfo)
        {
            return CheckForLedge(out hitInfo, transform.position, transform.forward);
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
            targetPosition = ledge.point + ledge.normal * grabRange / 2;
            targetPosition.y = ledge.transform.position.y;
            targetDirection = -ledge.normal;

            moveDelayTimer = moveDelay;

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