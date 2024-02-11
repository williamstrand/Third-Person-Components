using System.Collections;
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

        bool isAttached;

        void Awake()
        {
            if (instance != null)
            {
                Destroy(gameObject);
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
            if (!isAttached) return;

            cameraTransform.position = Vector3.Lerp(cameraTransform.position, targetPosition, Time.fixedDeltaTime * PositionSmoothTime);
            // cameraTransform.position = currentAttachment.Position;
            // cameraTransform.forward = currentAttachment.Forward;
            camera.transform.forward = Vector3.Lerp(camera.transform.forward, targetForward, Time.fixedDeltaTime * RotationSmoothTime);
        }

        public static void Attach(CameraAttachment attachment)
        {
            if (instance.currentAttachment != null)
            {
                instance.currentAttachment.OnValueChanged -= instance.OnAttachmentValueChanged;
            }

            instance.StartCoroutine(instance.AttachCoroutine(attachment));
        }

        IEnumerator AttachCoroutine(CameraAttachment attachment)
        {
            isAttached = false;
            currentAttachment = attachment;

            var originalPosition = cameraTransform.position;
            var originalForward = cameraTransform.forward;
            var timer = new Timer(AttachTime);

            while (!timer.IsCompleted)
            {
                timer.Update(Time.deltaTime);
                cameraTransform.position = Vector3.Lerp(originalPosition, attachment.Position, timer.Progress);
                cameraTransform.forward = Vector3.Lerp(originalForward, attachment.Forward, timer.Progress);
                yield return null;
            }

            isAttached = true;
            currentAttachment.OnValueChanged += OnAttachmentValueChanged;
        }

        void OnAttachmentValueChanged(Vector3 position, Vector3 forward)
        {
            targetPosition = position;
            targetForward = forward;
        }
    }
}