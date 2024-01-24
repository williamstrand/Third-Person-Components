using UnityEngine;

namespace Components
{
    public class MovementComponent : MonoBehaviour
    {
        [SerializeField] new Rigidbody rigidbody;

        public void Move(Vector2 direction, float speed)
        {
            if (direction.sqrMagnitude == 0) return;

            var velocity = new Vector3(direction.x, 0, direction.y) * speed;
            rigidbody.MoveRotation(Quaternion.LookRotation(velocity));
            rigidbody.MovePosition(rigidbody.position + velocity);
        }

        public void Move(Vector2 direction, float speed, Vector3 forward)
        {
            if (direction.sqrMagnitude == 0) return;

            var right = Vector3.Cross(Vector3.up, forward);
            var direction3 = new Vector3(direction.x, 0, direction.y);
            var translatedDirection = direction3.x * right + direction3.z * forward;


            Move(new Vector2(translatedDirection.x, translatedDirection.z), speed);
        }
    }
}