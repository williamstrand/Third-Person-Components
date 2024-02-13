using UnityEngine;

namespace ThirdPersonComponents
{
    [RequireComponent(typeof(Camera))]
    public class CameraBrain : MonoBehaviour
    {
        const float AttachTime = .2f;
        const float RotationSmoothTime = 30f;
        const float PositionSmoothTime = 30;
        static CameraBrain instance;

        new Camera camera;
        CameraAttachment currentAttachment;
        Transform cameraTransform;

        Vector3 targetPosition;
        Vector3 targetForward;

        void Awake()
        {
            if (instance != null)
            {
                Destroy(gameObject);
                Debug.LogWarning("Multiple CameraBrains detected. Destroying the new one.");
                return;
            }

            instance = this;
            if (TryGetComponent(out camera))
            {
                cameraTransform = camera.transform;
            }

        }

        void FixedUpdate()
        {
            cameraTransform.position = Vector3.Lerp(cameraTransform.position, targetPosition, Time.fixedDeltaTime * PositionSmoothTime);
            camera.transform.forward = Vector3.Lerp(camera.transform.forward, targetForward, Time.fixedDeltaTime * RotationSmoothTime);
        }

        public static void Attach(CameraAttachment attachment)
        {
            if (instance.currentAttachment != null)
            {
                instance.currentAttachment.OnValueChanged -= instance.OnAttachmentValueChanged;
            }

            instance.currentAttachment = attachment;
            instance.currentAttachment.OnValueChanged += instance.OnAttachmentValueChanged;
            instance.targetForward = attachment.Forward;
            instance.targetPosition = attachment.Position;
        }

        void OnAttachmentValueChanged(Vector3 position, Vector3 forward)
        {
            targetPosition = position;
            targetForward = forward;
        }
    }
}