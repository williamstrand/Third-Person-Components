using UnityEngine;

namespace ThirdPersonComponents.Movement
{
    public abstract class MovementComponent : MonoBehaviour
    {
        public abstract void Move(Vector2 direction, Vector3 forward, float speed);
    }
}