using System.Collections;
using UnityEngine;

namespace ThirdPersonComponents
{
    [RequireComponent(typeof(Camera))]
    public class CameraBrain : MonoBehaviour
    {
        const float AttachTime = .2f;
        static CameraBrain instance;

        new Camera camera;
        CameraAttachment currentAttachment;

        bool isAttached;

        void Awake()
        {
            if (instance != null)
            {
                Destroy(gameObject);
                return;
            }

            instance = this;
            TryGetComponent(out camera);
        }

        void FixedUpdate()
        {
            if (!isAttached) return;

            camera.transform.position = currentAttachment.Position;
            camera.transform.forward = currentAttachment.Forward;
        }

        public static void Attach(CameraAttachment attachment)
        {
            instance.StartCoroutine(instance.AttachCoroutine(attachment));
        }

        IEnumerator AttachCoroutine(CameraAttachment attachment)
        {
            isAttached = false;
            currentAttachment = attachment;

            var originalPosition = camera.transform.position;
            var originalForward = camera.transform.forward;
            var timer = new Timer(AttachTime);

            while (!timer.IsCompleted)
            {
                timer.Update(Time.deltaTime);
                camera.transform.position = Vector3.Lerp(originalPosition, attachment.Position, timer.Progress);
                camera.transform.forward = Vector3.Lerp(originalForward, attachment.Forward, timer.Progress);
                yield return null;
            }

            isAttached = true;
        }
    }
}