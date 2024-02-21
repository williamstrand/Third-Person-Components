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

        protected virtual Vector3 TargetPosition
        {
            get
            {
                var eulerAngles = cameraBoom.eulerAngles;
                var offset = Quaternion.Euler(eulerAngles.x, eulerAngles.y, 0) * cameraLookOffset;
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
        
        public Vector2 CameraLookOffset
        {
            get => cameraLookOffset;
            set => cameraLookOffset = value;
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

        protected virtual void Start()
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
        public virtual void Rotate(Vector2 direction, float speed)
        {
            // Update target x and y rotation
            targetXRotation -= direction.y * speed;
            targetYRotation += direction.x * speed;

            // Clamp the rotation of the camera boom to the rotation limits
            targetXRotation = Mathf.Clamp(targetXRotation, rotationLimits.x, rotationLimits.y);
        }

        /// <summary>
        ///     Attaches the camera to the camera brain.
        /// </summary>
        /// <returns></returns>
        public void Attach()
        {
            CameraBrain.Attach(cameraAttachment);
        }

        protected virtual void Update()
        {

            // Lerp the rotation of the camera boom to the target rotation
            var eulerAngles = cameraBoom.eulerAngles;
            var currentXRotation = Mathf.LerpAngle(eulerAngles.x, targetXRotation, cameraSmoothing * Time.deltaTime);
            var currentYRotation = Mathf.LerpAngle(eulerAngles.y, targetYRotation, cameraSmoothing * Time.deltaTime);

            // Set the rotation of the camera boom
            cameraBoom.eulerAngles = new Vector3(currentXRotation, currentYRotation, 0);


            // Set the position of the camera boom
            // cameraBoom.position = TargetPosition;
            var offset = cameraOffset.ToLocalSpace(cameraAttachment.Forward);
            cameraBoom.position = target.position + offset;

            // Set the position of the camera
            var distance = cameraDistance;
            if (CheckForWall(out var hitInfo))
            {
                var point = hitInfo.point.Flatten();
                distance = Vector3.Distance(point, target.position.Flatten());
                //distance = hitInfo.distance - 0.1f;
            }

            cameraAttachment.Position = cameraBoom.position + distance * -cameraBoom.forward;

            // Set the rotation of the camera
            var directionToTarget = (TargetPosition + offset - cameraAttachment.Position).normalized;
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