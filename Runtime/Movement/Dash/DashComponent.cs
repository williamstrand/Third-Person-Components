using ThirdPersonComponents.Extensions;
using UnityEngine;

namespace ThirdPersonComponents.Movement.Dash
{
    public class DashComponent : MonoBehaviour
    {
        [SerializeField] ThirdPersonMovementComponent movementComponent;
        [SerializeField] float dashCooldown = 1f;

        public bool IsDashing { get; private set; }
        public float DashCooldown
        {
            get => dashCooldown;
            set => dashCooldown = Mathf.Max(0, value);
        }

        public bool CanDash => cooldownTimer <= 0;

        float cooldownTimer;
        float dashTime;

        /// <summary>
        ///     Dashes in the specified direction.
        /// </summary>
        /// <param name="direction">the direction to dash in</param>
        /// <param name="speed">the speed to dash at</param>
        /// <param name="distance">the distance of the dash</param>
        /// <param name="forward">the forward vector of the camera</param>
        public void Dash(Vector2 direction, float speed, float distance, Vector3 forward)
        {
            if (!CanDash) return;
            if (IsDashing) return;

            var translatedDirection = direction.ToLocalSpace(forward.Flatten());

            IsDashing = true;
            movementComponent.LockVelocity = true;
            movementComponent.AutoRotate = false;

            var velocity = translatedDirection * speed;
            movementComponent.TargetVelocity = velocity;
            movementComponent.Velocity = velocity;
            transform.forward = movementComponent.Velocity.normalized;
            dashTime = distance / speed;
            cooldownTimer = dashCooldown;
        }

        void Update()
        {
            cooldownTimer -= Time.deltaTime;

            if (!IsDashing) return;

            dashTime -= Time.deltaTime;
            if (dashTime <= 0)
            {
                IsDashing = false;
                movementComponent.TargetVelocity = Vector3.zero;
                movementComponent.LockVelocity = false;
                movementComponent.AutoRotate = true;
            }
        }
    }
}