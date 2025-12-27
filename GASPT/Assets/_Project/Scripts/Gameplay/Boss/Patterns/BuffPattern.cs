using UnityEngine;
using GASPT.Core.Enums;

namespace GASPT.Gameplay.Boss
{
    /// <summary>
    /// 버프 패턴
    /// 자기 강화, 분노, 방어 강화 등
    /// </summary>
    [System.Serializable]
    public class BuffPattern : BossPattern
    {
        // ====== 버프 타입 ======

        public enum BuffType
        {
            AttackUp,       // 공격력 증가
            SpeedUp,        // 속도 증가
            DefenseUp,      // 방어력 증가
            Rage,           // 광폭화 (공격력 + 속도)
            Shield,         // 보호막
            Heal            // 회복
        }


        // ====== 버프 패턴 설정 ======

        [Header("버프 패턴 설정")]
        [Tooltip("버프 타입")]
        public BuffType buffType = BuffType.Rage;

        [Tooltip("버프 지속 시간")]
        [Range(3f, 30f)]
        public float buffDuration = 10f;

        [Tooltip("버프 수치 (배율 또는 고정값)")]
        [Range(0.1f, 3f)]
        public float buffValue = 1.5f;

        [Tooltip("회복량 (Heal 타입용)")]
        [Range(0, 500)]
        public int healAmount = 0;


        // ====== 생성자 ======

        public BuffPattern()
        {
            patternName = "버프";
            patternType = PatternType.Buff;
            damage = 0;
            cooldown = 20f;
            telegraphDuration = 1.5f;
            weight = 3;
            minPhase = 2;
            minRange = 0f;
            maxRange = 100f;
        }


        // ====== 텔레그래프 ======

        public override void ShowTelegraph(BaseBoss boss, Vector3 targetPosition)
        {
            Color telegraphColor = buffType switch
            {
                BuffType.AttackUp => new Color(1f, 0f, 0f, 0.5f),    // 빨강
                BuffType.SpeedUp => new Color(0f, 1f, 1f, 0.5f),     // 청록
                BuffType.DefenseUp => new Color(0f, 0f, 1f, 0.5f),   // 파랑
                BuffType.Rage => new Color(1f, 0.5f, 0f, 0.5f),      // 주황
                BuffType.Shield => new Color(1f, 1f, 0f, 0.5f),      // 노랑
                BuffType.Heal => new Color(0f, 1f, 0f, 0.5f),        // 초록
                _ => new Color(1f, 1f, 1f, 0.5f)
            };

            TelegraphController.Instance.ShowCircle(
                boss.transform.position,
                2f,
                telegraphDuration,
                telegraphColor
            );
        }


        // ====== 실행 ======

        public override async Awaitable Execute(BaseBoss boss, Transform target)
        {
            if (boss == null) return;

            BeginExecution();

            try
            {
                // 1. 텔레그래프 표시
                ShowTelegraph(boss, boss.transform.position);

                // 2. 텔레그래프 시간 대기
                await Awaitable.WaitForSecondsAsync(telegraphDuration);
                if (IsCancelled()) return;

                // 3. 버프 적용
                ApplyBuff(boss);

                Debug.Log($"[BuffPattern] {patternName} ({buffType}) 적용! 지속시간: {buffDuration}초");

                // 4. 버프 지속 (별도 관리)
                // 버프 해제는 별도의 시스템에서 관리하거나 여기서 타이머로 관리
                ScheduleBuffRemoval(boss);
            }
            finally
            {
                EndExecution();
            }
        }


        // ====== 버프 적용 ======

        private void ApplyBuff(BaseBoss boss)
        {
            switch (buffType)
            {
                case BuffType.AttackUp:
                    // 공격력 증가 로직 (보스 스탯 시스템과 연동 필요)
                    Debug.Log($"[BuffPattern] 공격력 x{buffValue} 적용");
                    break;

                case BuffType.SpeedUp:
                    Debug.Log($"[BuffPattern] 속도 x{buffValue} 적용");
                    break;

                case BuffType.DefenseUp:
                    Debug.Log($"[BuffPattern] 방어력 x{buffValue} 적용");
                    break;

                case BuffType.Rage:
                    Debug.Log($"[BuffPattern] 광폭화! 공격력/속도 x{buffValue} 적용");
                    break;

                case BuffType.Shield:
                    Debug.Log($"[BuffPattern] 보호막 생성!");
                    break;

                case BuffType.Heal:
                    if (healAmount > 0)
                    {
                        // HP 회복 (BaseBoss에 회복 메서드 필요)
                        Debug.Log($"[BuffPattern] HP {healAmount} 회복!");
                    }
                    break;
            }
        }


        // ====== 버프 해제 스케줄 ======

        private async void ScheduleBuffRemoval(BaseBoss boss)
        {
            await Awaitable.WaitForSecondsAsync(buffDuration);

            if (boss != null && !boss.IsDead)
            {
                RemoveBuff(boss);
            }
        }

        private void RemoveBuff(BaseBoss boss)
        {
            Debug.Log($"[BuffPattern] {buffType} 버프 해제!");

            // 버프 해제 로직
            switch (buffType)
            {
                case BuffType.AttackUp:
                case BuffType.SpeedUp:
                case BuffType.DefenseUp:
                case BuffType.Rage:
                case BuffType.Shield:
                    // 스탯 복원
                    break;

                case BuffType.Heal:
                    // Heal은 해제 없음
                    break;
            }
        }
    }
}
