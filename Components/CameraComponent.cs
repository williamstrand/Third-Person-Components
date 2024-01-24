using UnityEngine;

namespace Components
{
    public class CameraComponent : MonoBehaviour
    {
        public Vector3 Forward => cameraAttachment.Forward;

        [SerializeField] Transform target;
        [SerializeField] Vector2 rotationLimits = new(-45, 45);
        [SerializeField] Vector2 cameraOffset;
        [SerializeField] float cameraSmoothing = 10f;
        [SerializeField] float cameraDistance = 5f;
        [SerializeField] bool attachOnStart;

        Vector3 TargetPosition
        {
            get
            {
                var eulerAngles = cameraBoom.eulerAngles;
                var offset = Quaternion.Euler(eulerAngles.x, eulerAngles.y, 0) * cameraOffset;
                return target.position + offset;
            }
        }

        CameraAttachment cameraAttachment;
        Transform cameraBoom;

        float targetXRotation;
        float targetYRotation;

        void Awake()
        {
            cameraBoom = new GameObject("Camera Boom").transform;
            cameraBoom.hideFlags = HideFlags.HideInHierarchy;
        }

        void Start()
        {
            cameraAttachment = new CameraAttachment();
            if (attachOnStart) CameraBrain.Attach(cameraAttachment);
        }

        public void Rotate(Vector2 direction, float speed)
        {
            // Update target x and y rotation
            targetXRotation -= direction.y * speed;
            targetYRotation += direction.x * speed;

            // Clamp the rotation of the camera boom to the rotation limits
            if (targetXRotation > 180) targetXRotation -= 360;
            targetXRotation = Mathf.Clamp(targetXRotation, rotationLimits.x, rotationLimits.y);
        }

        void Update()
        {
            // Lerp the rotation of the camera boom to the target rotation
            var eulerAngles = cameraBoom.eulerAngles;
            var currentXRotation = Mathf.LerpAngle(eulerAngles.x, targetXRotation, cameraSmoothing * Time.deltaTime);
            var currentYRotation = Mathf.LerpAngle(eulerAngles.y, targetYRotation, cameraSmoothing * Time.deltaTime);

            // Set the rotation of the camera boom
            cameraBoom.eulerAngles = new Vector3(currentXRotation, currentYRotation, 0);

            // Set the position of the camera
            cameraAttachment.Position = cameraBoom.position + cameraDistance * -cameraBoom.forward;

            // Set the position of the camera boom
            cameraBoom.position = TargetPosition;

            // Set the rotation of the camera
            var directionToTarget = (TargetPosition - cameraAttachment.Position).normalized;
            cameraAttachment.Forward = directionToTarget;
        }
    }
}