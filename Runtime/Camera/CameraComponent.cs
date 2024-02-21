using ThirdPersonComponents.Extensions;
using UnityEngine;

namespace ThirdPersonComponents.Camera
{
    public class CameraComponent : MonoBehaviour
    {
        [Header("Camera movement")]
        [SerializeField] Vector2 rotationLimits = new(0, 45);

        [SerializeField] float cameraSmoothing = 30;

        [Header("Camera position")]
        [SerializeField] Transform target;

        [SerializeField] Vector2 cameraOffset;
        [SerializeField] Vector2 cameraLookOffset;
        [SerializeField] float cameraDistance = 5f;

        [Space(10)]
        [SerializeField] bool attachOnStart;

        public Vector3 Forward => cameraAttachment.Forward;

        Vector3 TargetPosition
        {
            get
            {
                var eulerAngles = cameraBoom.eulerAngles;
                var offset = Quaternion.Euler(eulerAngles.x, eulerAngles.y, 0) * cameraOffset;
                return target.position + offset;
            }
        }

        public Vector2 RotationLimits
        {
            get => rotationLimits;
            set => rotationLimits = value;
        }

        public Transform Target
        {
            get => target;
            set => target = value;
        }

        public Vector2 CameraOffset
        {
            get => cameraOffset;
            set => cameraOffset = value;
        }

        public float CameraDistance
        {
            get => cameraDistance;
            set => cameraDistance = Mathf.Max(0, value);
        }

        CameraAttachment cameraAttachment;
        Transform cameraBoom;

        float targetXRotation;
        float targetYRotation;

        void Start()
        {
            cameraBoom = new GameObject("Camera Boom").transform;
            cameraBoom.hideFlags = HideFlags.HideInHierarchy;

            cameraAttachment = new CameraAttachment();
            if (attachOnStart) CameraBrain.Attach(cameraAttachment);
        }

        /// <summary>
        ///     Rotates the camera.
        /// </summary>
        /// <param name="direction">the direction to rotate in.</param>
        /// <param name="speed">the speed of the rotation.</param>
        public void Rotate(Vector2 direction, float speed)
        {
            // Update target x and y rotation
            targetXRotation -= direction.y * speed;
            targetYRotation += direction.x * speed;

            // Clamp the rotation of the camera boom to the rotation limits
            targetXRotation = Mathf.Clamp(targetXRotation, rotationLimits.x, rotationLimits.y);
        }

        void Update()
        {
            var directionToTarget = (TargetPosition - cameraAttachment.Position).normalized;

            // Lerp the rotation of the camera boom to the target rotation
            var eulerAngles = cameraBoom.eulerAngles;
            var currentXRotation = Mathf.LerpAngle(eulerAngles.x, targetXRotation, cameraSmoothing * Time.deltaTime);
            var currentYRotation = Mathf.LerpAngle(eulerAngles.y, targetYRotation, cameraSmoothing * Time.deltaTime);

            // Set the rotation of the camera boom
            cameraBoom.eulerAngles = new Vector3(currentXRotation, currentYRotation, 0);

            // Set the position of the camera
            var distance = cameraDistance;
            if (CheckForWall(out var hitInfo))
            {
                var point = hitInfo.point.Flatten();
                distance = Vector3.Distance(point, target.position.Flatten());
                //distance = hitInfo.distance - 0.1f;
            }

            cameraAttachment.Position = cameraBoom.position + distance * -cameraBoom.forward;

            // Set the position of the camera boom
            // cameraBoom.position = TargetPosition;
            cameraBoom.position = target.position;

            // Set the rotation of the camera
            cameraAttachment.Forward = directionToTarget;
        }

        bool CheckForWall(out RaycastHit hitInfo)
        {
            var direction = cameraAttachment.Position - target.position;
            var ray = new Ray(target.position, direction);
            var didHit = Physics.Raycast(ray, out hitInfo, cameraDistance);
            Debug.DrawRay(target.position, direction * cameraDistance, didHit ? Color.red : Color.green);
            return didHit;
        }
    }
}