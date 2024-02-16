using UnityEngine;

namespace ThirdPersonComponents.Extensions
{
    public static class Extensions
    {
        /// <summary>
        /// Sets a Vector3's y component to 0.
        /// </summary>
        /// <param name="vector">the vector to update</param>
        /// <returns>the flattened vector</returns>
        public static Vector3 Flatten(this Vector3 vector)
        {
            return new Vector3(vector.x, 0, vector.z);
        }

        /// <summary>
        /// Converts a Vector2 to a Vector3 in another Vector3's local space.
        /// </summary>
        /// <param name="direction">the original direction</param>
        /// <param name="forward">the forward vector of the local space vector</param>
        /// <returns>the converted vector</returns>
        public static Vector3 ToLocalSpace(this Vector2 direction, Vector3 forward)
        {
            var right = Vector3.Cross(Vector3.up, forward);
            var direction3 = new Vector3(direction.x, 0, direction.y);
            return direction3.x * right + direction3.z * forward;
        }
    }
}