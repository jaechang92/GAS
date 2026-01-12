using System;
using UnityEngine;

namespace GASPT.Core.Extensions
{
    /// <summary>
    /// Awaitable 확장 메서드
    /// Unity 6.0 Awaitable에 추가 기능 제공
    /// </summary>
    public static class AwaitableExtensions
    {
        /// <summary>
        /// Fire-and-forget 패턴
        /// async 메서드를 호출하지만 결과를 기다리지 않음
        /// CS4014 경고를 명시적으로 무시
        /// </summary>
        public static async void Forget(this Awaitable awaitable)
        {
            try
            {
                await awaitable;
            }
            catch (Exception ex)
            {
                // 예외를 로그로 출력 (크래시 방지)
                Debug.LogError($"[Awaitable.Forget] 예외 발생: {ex.Message}\n{ex.StackTrace}");
            }
        }

        /// <summary>
        /// Fire-and-forget 패턴 (에러 핸들러 포함)
        /// </summary>
        public static async void Forget(this Awaitable awaitable, Action<Exception> onError)
        {
            try
            {
                await awaitable;
            }
            catch (Exception ex)
            {
                onError?.Invoke(ex);
            }
        }
    }
}
