using System;

namespace GASPT.Core.GameFlow
{
    /// <summary>
    /// 상태 전환 시 Fade 및 검증 설정
    /// TransitionOrchestrator에서 사용
    /// </summary>
    [Serializable]
    public class TransitionConfig
    {
        /// <summary>
        /// 전환 전 FadeOut 필요 여부
        /// </summary>
        public bool requiresFadeOut = false;

        /// <summary>
        /// 전환 후 FadeIn 필요 여부
        /// </summary>
        public bool requiresFadeIn = false;

        /// <summary>
        /// 전환 후 Scene 검증 필요 여부
        /// </summary>
        public bool requiresSceneValidation = false;

        /// <summary>
        /// FadeOut 지속 시간 (초)
        /// </summary>
        public float fadeOutDuration = 0.5f;

        /// <summary>
        /// FadeIn 지속 시간 (초)
        /// </summary>
        public float fadeInDuration = 1.0f;

        /// <summary>
        /// 검증기 등록 대기 시간 (초)
        /// </summary>
        public float validatorRegistrationDelay = 0.1f;


        // ====== 팩토리 메서드 ======

        /// <summary>
        /// Fade 없음 (빠른 전환용)
        /// </summary>
        public static TransitionConfig None => new TransitionConfig();

        /// <summary>
        /// FadeIn만 필요 (검정 화면에서 시작)
        /// </summary>
        public static TransitionConfig FadeInOnly(float duration = 1.0f, bool validation = true)
        {
            return new TransitionConfig
            {
                requiresFadeIn = true,
                fadeInDuration = duration,
                requiresSceneValidation = validation
            };
        }

        /// <summary>
        /// FadeOut만 필요 (로딩 전환)
        /// </summary>
        public static TransitionConfig FadeOutOnly(float duration = 0.5f)
        {
            return new TransitionConfig
            {
                requiresFadeOut = true,
                fadeOutDuration = duration
            };
        }

        /// <summary>
        /// FadeOut + FadeIn (완전한 씬 전환)
        /// </summary>
        public static TransitionConfig FullTransition(float outDuration = 0.5f, float inDuration = 1.0f, bool validation = true)
        {
            return new TransitionConfig
            {
                requiresFadeOut = true,
                requiresFadeIn = true,
                fadeOutDuration = outDuration,
                fadeInDuration = inDuration,
                requiresSceneValidation = validation
            };
        }

        /// <summary>
        /// 빠른 전환 (짧은 Fade)
        /// </summary>
        public static TransitionConfig QuickTransition(bool validation = true)
        {
            return new TransitionConfig
            {
                requiresFadeOut = true,
                requiresFadeIn = true,
                fadeOutDuration = 0.3f,
                fadeInDuration = 0.3f,
                requiresSceneValidation = validation
            };
        }
    }
}
