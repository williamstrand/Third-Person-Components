using UnityEngine;

namespace ThirdPersonComponents.Movement
{
    public abstract class MovementComponent : MonoBehaviour
    {
        public abstract float Speed { get; set; }

        public abstract void Move(Vector2 direction, Vector3 forward);
    }
}