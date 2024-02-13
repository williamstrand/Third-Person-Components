using System;
using UnityEngine;

namespace ThirdPersonComponents
{
    public sealed class CameraAttachment
    {
        public Action<Vector3, Vector3> OnValueChanged { get; set; }

        public Vector3 Position
        {
            get => position;
            set
            {
                position = value;
                OnValueChanged?.Invoke(Position, Forward);
            }
        }

        public Vector3 Forward
        {
            get => forward;
            set
            {
                forward = value;
                OnValueChanged?.Invoke(Position, Forward);
            }
        }

        Vector3 position;
        Vector3 forward;
    }
}