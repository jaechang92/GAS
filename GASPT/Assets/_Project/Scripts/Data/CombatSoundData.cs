using UnityEngine;

namespace GASPT.Data
{
    /// <summary>
    /// 전투 관련 사운드 데이터
    /// AudioClip 참조를 중앙 관리
    /// </summary>
    [CreateAssetMenu(fileName = "CombatSoundData", menuName = "GASPT/Audio/Combat Sound Data")]
    public class CombatSoundData : ScriptableObject
    {
        // ====== 플레이어 사운드 ======

        [Header("플레이어 - 피격")]
        [Tooltip("플레이어가 데미지를 받을 때")]
        public AudioClip playerHitSound;

        [Tooltip("플레이어가 크리티컬 데미지를 받을 때 (선택)")]
        public AudioClip playerCriticalHitSound;

        [Header("플레이어 - 회복")]
        [Tooltip("플레이어가 HP를 회복할 때")]
        public AudioClip playerHealSound;

        [Header("플레이어 - 사망")]
        [Tooltip("플레이어가 사망할 때")]
        public AudioClip playerDeathSound;


        // ====== 적 사운드 ======

        [Header("적 - 피격")]
        [Tooltip("적이 데미지를 받을 때")]
        public AudioClip enemyHitSound;

        [Tooltip("적이 크리티컬 데미지를 받을 때 (선택)")]
        public AudioClip enemyCriticalHitSound;

        [Header("적 - 사망")]
        [Tooltip("일반 적이 사망할 때")]
        public AudioClip enemyDeathSound;

        [Tooltip("보스가 사망할 때 (선택)")]
        public AudioClip bossDeathSound;


        // ====== 투사체/이펙트 사운드 ======

        [Header("투사체")]
        [Tooltip("투사체가 적중할 때")]
        public AudioClip projectileHitSound;

        [Tooltip("투사체가 벽에 부딪힐 때 (선택)")]
        public AudioClip projectileWallHitSound;


        // ====== 상태 효과 사운드 ======

        [Header("상태 효과")]
        [Tooltip("화상 효과 적용 시")]
        public AudioClip burnApplySound;

        [Tooltip("빙결 효과 적용 시")]
        public AudioClip freezeApplySound;

        [Tooltip("독 효과 적용 시")]
        public AudioClip poisonApplySound;

        [Tooltip("기절 효과 적용 시")]
        public AudioClip stunApplySound;


        // ====== 볼륨 설정 ======

        [Header("볼륨 스케일")]
        [Range(0f, 1f)]
        [Tooltip("플레이어 사운드 볼륨 스케일")]
        public float playerVolumeScale = 1f;

        [Range(0f, 1f)]
        [Tooltip("적 사운드 볼륨 스케일")]
        public float enemyVolumeScale = 0.8f;

        [Range(0f, 1f)]
        [Tooltip("상태 효과 사운드 볼륨 스케일")]
        public float statusEffectVolumeScale = 0.6f;


        // ====== 피치 변형 설정 ======

        [Header("피치 변형")]
        [Tooltip("피격 사운드에 랜덤 피치 적용")]
        public bool useRandomPitchOnHit = true;

        [Range(0.8f, 1f)]
        public float pitchMin = 0.9f;

        [Range(1f, 1.2f)]
        public float pitchMax = 1.1f;


        // ====== 유틸리티 메서드 ======

        /// <summary>
        /// 랜덤 피치 값 반환
        /// </summary>
        public float GetRandomPitch()
        {
            return useRandomPitchOnHit ? Random.Range(pitchMin, pitchMax) : 1f;
        }

        /// <summary>
        /// 적절한 적 사망 사운드 반환 (보스/일반)
        /// </summary>
        /// <param name="isBoss">보스 여부</param>
        public AudioClip GetEnemyDeathSound(bool isBoss)
        {
            if (isBoss && bossDeathSound != null)
            {
                return bossDeathSound;
            }
            return enemyDeathSound;
        }
    }
}
