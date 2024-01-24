using UnityEngine;

namespace Components
{
    public sealed class CameraAttachment
    {
        public Vector3 Position
        {
            get => transform.position;
            set => transform.position = value;
        }

        public Vector3 Forward
        {
            get => transform.forward;
            set => transform.forward = value;
        }

        Transform transform;

        public CameraAttachment()
        {
            transform = new GameObject("Camera Attachment").transform;
            transform.hideFlags = HideFlags.HideInHierarchy;
        }
    }
}