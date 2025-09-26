// ���� ��ġ: Assets/Scripts/Helper/AwaitableHelper.cs
using UnityEngine;
using System;
using System.Threading;

namespace Helper
{
    /// <summary>
    /// Unity 6.0 Awaitable ��ƿ��Ƽ ����
    /// Coroutine ��� Awaitable ����� ���� ����� Ŭ����
    /// </summary>
    public static class AwaitableHelper
    {
        // ��� �Ϸ�� Awaitable�� ���� ���� ���
        private static readonly AwaitableCompletionSource completedSource = new();

        static AwaitableHelper()
        {
            // ���� �����ڿ��� ��� �Ϸ� ���·� ����
            completedSource.SetResult();
        }

        /// <summary>
        /// �̹� �Ϸ�� Awaitable ��ȯ (Task.CompletedTask�� ����)
        /// </summary>
        public static Awaitable CompletedTask => completedSource.Awaitable;

        /// <summary>
        /// ������ true�� �� ������ ���
        /// </summary>
        public static async Awaitable WaitUntil(Func<bool> condition, CancellationToken cancellationToken = default)
        {
            while (!condition())
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    throw new OperationCanceledException();
                }

                await Awaitable.NextFrameAsync(cancellationToken);
            }
        }

        /// <summary>
        /// ������ false�� �� ������ ���
        /// </summary>
        public static async Awaitable WaitWhile(Func<bool> condition, CancellationToken cancellationToken = default)
        {
            while (condition())
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    throw new OperationCanceledException();
                }

                await Awaitable.NextFrameAsync(cancellationToken);
            }
        }

        /// <summary>
        /// ������ ������ ����ŭ ���
        /// </summary>
        public static async Awaitable WaitForFrames(int frameCount, CancellationToken cancellationToken = default)
        {
            for (int i = 0; i < frameCount; i++)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    throw new OperationCanceledException();
                }

                await Awaitable.NextFrameAsync(cancellationToken);
            }
        }

        /// <summary>
        /// �ִϸ��̼� Ŀ�� ��� �� ����
        /// </summary>
        public static async Awaitable LerpWithCurve(
            Action<float> onUpdate,
            AnimationCurve curve,
            float duration,
            CancellationToken cancellationToken = default)
        {
            float elapsed = 0f;

            while (elapsed < duration)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    throw new OperationCanceledException();
                }

                float t = elapsed / duration;
                float value = curve.Evaluate(t);
                onUpdate?.Invoke(value);

                elapsed += Time.deltaTime;
                await Awaitable.NextFrameAsync(cancellationToken);
            }

            // ������ �� ����
            onUpdate?.Invoke(curve.Evaluate(1f));
        }

        /// <summary>
        /// ���� ���� �ִϸ��̼�
        /// </summary>
        public static async Awaitable Lerp(
            float from,
            float to,
            float duration,
            Action<float> onUpdate,
            CancellationToken cancellationToken = default)
        {
            float elapsed = 0f;

            while (elapsed < duration)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    throw new OperationCanceledException();
                }

                float t = elapsed / duration;
                float value = Mathf.Lerp(from, to, t);
                onUpdate?.Invoke(value);

                elapsed += Time.deltaTime;
                await Awaitable.NextFrameAsync(cancellationToken);
            }

            // ������ �� ����
            onUpdate?.Invoke(to);
        }

        /// <summary>
        /// Vector3 ���� ����
        /// </summary>
        public static async Awaitable LerpVector3(
            Vector3 from,
            Vector3 to,
            float duration,
            Action<Vector3> onUpdate,
            CancellationToken cancellationToken = default)
        {
            float elapsed = 0f;

            while (elapsed < duration)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    throw new OperationCanceledException();
                }

                float t = elapsed / duration;
                Vector3 value = Vector3.Lerp(from, to, t);
                onUpdate?.Invoke(value);

                elapsed += Time.deltaTime;
                await Awaitable.NextFrameAsync(cancellationToken);
            }

            // ������ �� ����
            onUpdate?.Invoke(to);
        }

        /// <summary>
        /// Ÿ�Ӿƿ� ����� �ִ� ���
        /// </summary>
        public static async Awaitable WaitWithTimeout(
            Func<bool> condition,
            float timeoutSeconds,
            Action onTimeout = null,
            CancellationToken cancellationToken = default)
        {
            float elapsed = 0f;

            while (!condition() && elapsed < timeoutSeconds)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    throw new OperationCanceledException();
                }

                elapsed += Time.deltaTime;
                await Awaitable.NextFrameAsync(cancellationToken);
            }

            if (elapsed >= timeoutSeconds)
            {
                onTimeout?.Invoke();
                Debug.LogWarning($"WaitWithTimeout: Timeout after {timeoutSeconds} seconds");
            }
        }

        /// <summary>
        /// ���� Awaitable�� ���ķ� ����
        /// </summary>
        public static async Awaitable WhenAll(params Awaitable[] awaitables)
        {
            foreach (var awaitable in awaitables)
            {
                await awaitable;
            }
        }

        /// <summary>
        /// ù ��°�� �Ϸ�Ǵ� Awaitable�� ��ٸ�
        /// </summary>
        public static async Awaitable WhenAny(params Awaitable[] awaitables)
        {
            // ������ ���� (�����δ� �� ������ ���� �ʿ�)
            if (awaitables.Length > 0)
            {
                await awaitables[0];
            }
        }

        /// <summary>
        /// ���� �� �׼� ����
        /// </summary>
        public static async Awaitable DelayedAction(
            float delay,
            Action action,
            CancellationToken cancellationToken = default)
        {
            await Awaitable.WaitForSecondsAsync(delay, cancellationToken);
            action?.Invoke();
        }

        /// <summary>
        /// �ݺ� ����
        /// </summary>
        public static async Awaitable RepeatAction(
            Action action,
            float interval,
            int repeatCount,
            CancellationToken cancellationToken = default)
        {
            for (int i = 0; i < repeatCount; i++)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    throw new OperationCanceledException();
                }

                action?.Invoke();
                await Awaitable.WaitForSecondsAsync(interval, cancellationToken);
            }
        }

        /// <summary>
        /// ���� �ݺ� ����
        /// </summary>
        public static async Awaitable RepeatActionForever(
            Action action,
            float interval,
            CancellationToken cancellationToken = default)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                action?.Invoke();
                await Awaitable.WaitForSecondsAsync(interval, cancellationToken);
            }
        }
    }
}