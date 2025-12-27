using System.Collections.Generic;
using UnityEngine;
using FSM.Core;
using GASPT.UI;
using GASPT.Core.SceneManagement;

namespace GASPT.Core.GameFlow
{
    /// <summary>
    /// 상태 전환 시 FadeIn/Out 및 Scene 검증을 중앙 관리
    ///
    /// SOLID 원칙:
    /// - SRP: Fade 및 검증 처리만 담당
    /// - OCP: 새 전환 추가 시 configs에 항목만 추가
    /// - DIP: FadeController, SceneValidationManager 인터페이스에 의존
    /// </summary>
    public class TransitionOrchestrator
    {
        private readonly Dictionary<string, TransitionConfig> configs;
        private bool showDebugLogs = true;


        // ====== 생성자 ======

        public TransitionOrchestrator()
        {
            configs = new Dictionary<string, TransitionConfig>();
            SetupConfigs();
        }


        // ====== 전환 설정 ======

        private void SetupConfigs()
        {
            // Initializing → StartRoom: 검정에서 FadeIn만 (게임 시작)
            configs["Initializing_StartRoom"] = TransitionConfig.FadeInOnly(1.0f, validation: true);

            // StartRoom → LoadingDungeon: FadeOut만 (로딩 상태가 씬 전환)
            configs["StartRoom_LoadingDungeon"] = TransitionConfig.FadeOutOnly(0.5f);

            // LoadingDungeon → DungeonCombat: 로딩 완료 후 FadeIn + 검증
            configs["LoadingDungeon_DungeonCombat"] = TransitionConfig.FadeInOnly(1.0f, validation: true);

            // DungeonCombat ↔ DungeonReward: Fade 불필요 (빠른 전환)
            configs["DungeonCombat_DungeonReward"] = TransitionConfig.None;
            configs["DungeonReward_DungeonTransition"] = TransitionConfig.None;

            // DungeonTransition → DungeonCombat: Fade는 DungeonTransitionState에서 처리 (방 이동 중)
            configs["DungeonTransition_DungeonCombat"] = TransitionConfig.None;

            // DungeonTransition → DungeonCleared: Fade 불필요
            configs["DungeonTransition_DungeonCleared"] = TransitionConfig.None;

            // DungeonCleared → LoadingStartRoom: FadeOut만
            configs["DungeonCleared_LoadingStartRoom"] = TransitionConfig.FadeOutOnly(0.5f);

            // LoadingStartRoom → StartRoom: FadeIn + 검증
            configs["LoadingStartRoom_StartRoom"] = TransitionConfig.FadeInOnly(1.0f, validation: true);

            // GameOver → StartRoom: 완전한 전환
            configs["GameOver_StartRoom"] = TransitionConfig.FullTransition(0.5f, 1.0f, validation: true);

            // Any → GameOver: FadeOut만 (게임오버 화면 표시)
            configs["DungeonCombat_GameOver"] = TransitionConfig.FadeOutOnly(0.5f);
            configs["DungeonReward_GameOver"] = TransitionConfig.FadeOutOnly(0.5f);
            configs["DungeonTransition_GameOver"] = TransitionConfig.FadeOutOnly(0.5f);

            LogMessage($"[TransitionOrchestrator] {configs.Count}개 전환 설정 완료");
        }


        // ====== 전환 처리 ======

        /// <summary>
        /// 상태 전환 전 처리 (FadeOut)
        /// FSM의 OnBeforeTransitionAsync에서 호출
        /// </summary>
        public async Awaitable BeforeTransitionAsync(ITransition transition)
        {
            var key = GetTransitionKey(transition);
            LogMessage($"[TransitionOrchestrator] BeforeTransition: {key}");

            if (!configs.TryGetValue(key, out var config))
            {
                LogMessage($"[TransitionOrchestrator] 설정 없음 - 기본값 사용");
                return;
            }

            if (!config.requiresFadeOut)
            {
                return;
            }

            var fadeController = FadeController.Instance;
            if (fadeController != null)
            {
                LogMessage($"[TransitionOrchestrator] FadeOut 시작 ({config.fadeOutDuration}s)");
                await fadeController.FadeOut(config.fadeOutDuration);
                LogMessage($"[TransitionOrchestrator] FadeOut 완료");
            }
        }

        /// <summary>
        /// 상태 전환 후 처리 (Scene 검증 + FadeIn)
        /// FSM의 OnAfterTransitionAsync에서 호출
        /// </summary>
        public async Awaitable AfterTransitionAsync(ITransition transition)
        {
            var key = GetTransitionKey(transition);
            LogMessage($"[TransitionOrchestrator] AfterTransition: {key}");

            if (!configs.TryGetValue(key, out var config))
            {
                LogMessage($"[TransitionOrchestrator] 설정 없음 - 기본값 사용");
                return;
            }

            // 1. Scene 검증
            if (config.requiresSceneValidation)
            {
                await PerformSceneValidation(config);
            }

            // 2. FadeIn
            if (config.requiresFadeIn)
            {
                var fadeController = FadeController.Instance;
                if (fadeController != null)
                {
                    LogMessage($"[TransitionOrchestrator] FadeIn 시작 ({config.fadeInDuration}s)");
                    await fadeController.FadeIn(config.fadeInDuration);
                    LogMessage($"[TransitionOrchestrator] FadeIn 완료");
                }
            }
        }


        // ====== 내부 메서드 ======

        /// <summary>
        /// Scene 검증 수행
        /// </summary>
        private async Awaitable PerformSceneValidation(TransitionConfig config)
        {
            LogMessage("[TransitionOrchestrator] Scene 검증 시작...");

            var validationManager = SceneValidationManager.Instance;
            if (validationManager == null)
            {
                LogMessage("[TransitionOrchestrator] SceneValidationManager 없음 - 검증 스킵");
                return;
            }

            // 검증기 등록 대기 (씬 로드 직후 OnEnable이 아직 호출되지 않았을 수 있음)
            if (config.validatorRegistrationDelay > 0)
            {
                await Awaitable.WaitForSecondsAsync(config.validatorRegistrationDelay);
            }

            LogMessage($"[TransitionOrchestrator] 등록된 검증기 수: {validationManager.ValidatorCount}");

            bool success = await validationManager.ValidateAllAsync();
            if (success)
            {
                LogMessage("[TransitionOrchestrator] Scene 검증 완료 - 모든 참조 유효");
            }
            else
            {
                Debug.LogWarning("[TransitionOrchestrator] Scene 검증 완료 - 일부 참조 실패");
            }

            // 검증 완료 후 추가 프레임 대기 (카메라 등 모든 시스템 안정화)
            await Awaitable.NextFrameAsync();
            await Awaitable.NextFrameAsync();

            LogMessage("[TransitionOrchestrator] Scene 검증 및 안정화 완료");
        }

        /// <summary>
        /// 전환 키 생성 (FromState_ToState)
        /// </summary>
        private string GetTransitionKey(ITransition transition)
        {
            return $"{transition.FromStateId}_{transition.ToStateId}";
        }


        // ====== 설정 관리 ======

        /// <summary>
        /// 전환 설정 추가 또는 수정
        /// </summary>
        public void SetConfig(string fromState, string toState, TransitionConfig config)
        {
            var key = $"{fromState}_{toState}";
            configs[key] = config;
            LogMessage($"[TransitionOrchestrator] 설정 추가/수정: {key}");
        }

        /// <summary>
        /// 전환 설정 가져오기
        /// </summary>
        public TransitionConfig GetConfig(string fromState, string toState)
        {
            var key = $"{fromState}_{toState}";
            return configs.TryGetValue(key, out var config) ? config : null;
        }

        /// <summary>
        /// 디버그 로그 활성화/비활성화
        /// </summary>
        public void SetDebugLogs(bool enabled)
        {
            showDebugLogs = enabled;
        }


        // ====== 로깅 ======

        private void LogMessage(string message)
        {
            if (showDebugLogs)
            {
                Debug.Log(message);
            }
        }
    }
}
