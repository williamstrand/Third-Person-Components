using System;

namespace ThirdPersonComponents
{
    public sealed class Timer
    {
        public Action OnComplete { get; }

        public float TimeLeft { get; private set; }
        public float Duration { get; private set; }
        public bool IsCompleted => TimeLeft <= 0;
        public float Progress => 1 - TimeLeft / Duration;

        public Timer(float duration = 0, Action onComplete = null)
        {
            OnComplete = onComplete;
            Duration = duration;
            TimeLeft = duration;
        }

        public void Update(float deltaTime)
        {
            if (IsCompleted) return;

            TimeLeft -= deltaTime;
            if (IsCompleted) OnComplete?.Invoke();
        }

        public void Reset(float newTime = -1)
        {
            Duration = newTime != -1 ? newTime : Duration;
            TimeLeft = Duration;
        }
    }
}