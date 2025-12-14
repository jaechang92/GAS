using GASPT.Data;
using GASPT.Stats;
using GASPT.Gameplay.Enemies;
using UnityEngine;

namespace GASPT.Audio
{
    /// <summary>
    /// 전투 이벤트와 오디오 시스템 연결
    /// PlayerStats, Enemy 이벤트 구독 및 사운드 재생
    /// </summary>
    public class CombatAudioBridge : MonoBehaviour
    {
        // ====== 설정 ======

        [Header("사운드 데이터")]
        [SerializeField] private CombatSoundData soundData;

        [Header("참조")]
        [SerializeField] private PlayerStats playerStats;


        // ====== Unity 생명주기 ======

        private void OnEnable()
        {
            SubscribeToPlayerEvents();
        }

        private void OnDisable()
        {
            UnsubscribeFromPlayerEvents();
        }


        // ====== 플레이어 이벤트 구독 ======

        private void SubscribeToPlayerEvents()
        {
            if (playerStats == null)
            {
                // 자동 탐색
                playerStats = GetComponentInParent<PlayerStats>();
                if (playerStats == null)
                {
                    playerStats = FindAnyObjectByType<PlayerStats>();
                }
            }

            if (playerStats != null)
            {
                playerStats.OnDamaged += HandlePlayerDamaged;
                playerStats.OnHealed += HandlePlayerHealed;
                playerStats.OnDeath += HandlePlayerDeath;

                Debug.Log("[CombatAudioBridge] PlayerStats 이벤트 구독 완료");
            }
            else
            {
                Debug.LogWarning("[CombatAudioBridge] PlayerStats를 찾을 수 없습니다.");
            }
        }

        private void UnsubscribeFromPlayerEvents()
        {
            if (playerStats != null)
            {
                playerStats.OnDamaged -= HandlePlayerDamaged;
                playerStats.OnHealed -= HandlePlayerHealed;
                playerStats.OnDeath -= HandlePlayerDeath;
            }
        }


        // ====== 플레이어 이벤트 핸들러 ======

        /// <summary>
        /// 플레이어 피격 사운드 재생
        /// </summary>
        /// <param name="damage">데미지량</param>
        /// <param name="currentHp">현재 HP</param>
        /// <param name="maxHp">최대 HP</param>
        private void HandlePlayerDamaged(int damage, int currentHp, int maxHp)
        {
            if (soundData == null || AudioManager.Instance == null) return;

            AudioClip clip = soundData.playerHitSound;
            if (clip == null) return;

            if (soundData.useRandomPitchOnHit)
            {
                AudioManager.Instance.PlaySFXWithRandomPitch(
                    clip,
                    soundData.pitchMin,
                    soundData.pitchMax,
                    soundData.playerVolumeScale
                );
            }
            else
            {
                AudioManager.Instance.PlaySFX(clip, soundData.playerVolumeScale);
            }
        }

        /// <summary>
        /// 플레이어 회복 사운드 재생
        /// </summary>
        /// <param name="healAmount">회복량</param>
        /// <param name="currentHp">현재 HP</param>
        /// <param name="maxHp">최대 HP</param>
        private void HandlePlayerHealed(int healAmount, int currentHp, int maxHp)
        {
            if (soundData == null || AudioManager.Instance == null) return;

            AudioClip clip = soundData.playerHealSound;
            if (clip == null) return;

            AudioManager.Instance.PlaySFX(clip, soundData.playerVolumeScale);
        }

        /// <summary>
        /// 플레이어 사망 사운드 재생
        /// </summary>
        private void HandlePlayerDeath()
        {
            if (soundData == null || AudioManager.Instance == null) return;

            AudioClip clip = soundData.playerDeathSound;
            if (clip == null) return;

            AudioManager.Instance.PlaySFX(clip, soundData.playerVolumeScale);
        }


        // ====== 적 이벤트 연결 (외부에서 호출) ======

        /// <summary>
        /// 적 이벤트 구독 (EnemySpawner나 EnemyManager에서 호출)
        /// </summary>
        /// <param name="enemy">구독할 적</param>
        public void SubscribeToEnemy(Enemy enemy)
        {
            if (enemy == null) return;

            enemy.OnHpChanged += (currentHp, maxHp) => HandleEnemyHit(enemy);
            enemy.OnDeath += HandleEnemyDeath;
        }

        /// <summary>
        /// 적 피격 사운드 재생
        /// </summary>
        private void HandleEnemyHit(Enemy enemy)
        {
            if (soundData == null || AudioManager.Instance == null) return;

            AudioClip clip = soundData.enemyHitSound;
            if (clip == null) return;

            // 적 위치에서 사운드 재생
            Vector3 position = enemy.transform.position;

            if (soundData.useRandomPitchOnHit)
            {
                AudioManager.Instance.PlaySFXWithRandomPitch(
                    clip,
                    soundData.pitchMin,
                    soundData.pitchMax,
                    soundData.enemyVolumeScale
                );
            }
            else
            {
                AudioManager.Instance.PlaySFXAtPosition(clip, position, soundData.enemyVolumeScale);
            }
        }

        /// <summary>
        /// 적 사망 사운드 재생
        /// </summary>
        /// <param name="enemy">사망한 적</param>
        private void HandleEnemyDeath(Enemy enemy)
        {
            if (soundData == null || AudioManager.Instance == null) return;

            bool isBoss = enemy is BossEnemy;
            AudioClip clip = soundData.GetEnemyDeathSound(isBoss);
            if (clip == null) return;

            Vector3 position = enemy.transform.position;
            AudioManager.Instance.PlaySFXAtPosition(clip, position, soundData.enemyVolumeScale);
        }


        // ====== 상태 효과 사운드 (외부 호출용) ======

        /// <summary>
        /// 상태 효과 적용 사운드 재생
        /// </summary>
        /// <param name="effectType">효과 타입</param>
        /// <param name="position">재생 위치</param>
        public void PlayStatusEffectSound(StatusEffects.StatusEffectType effectType, Vector3 position)
        {
            if (soundData == null || AudioManager.Instance == null) return;

            AudioClip clip = GetStatusEffectClip(effectType);
            if (clip == null) return;

            AudioManager.Instance.PlaySFXAtPosition(clip, position, soundData.statusEffectVolumeScale);
        }

        private AudioClip GetStatusEffectClip(StatusEffects.StatusEffectType effectType)
        {
            return effectType switch
            {
                StatusEffects.StatusEffectType.Burn => soundData.burnApplySound,
                StatusEffects.StatusEffectType.Freeze => soundData.freezeApplySound,
                StatusEffects.StatusEffectType.Poison => soundData.poisonApplySound,
                StatusEffects.StatusEffectType.Stun => soundData.stunApplySound,
                _ => null
            };
        }


        // ====== 에디터/디버그 ======

        [ContextMenu("Test Player Hit Sound")]
        private void DebugTestPlayerHit()
        {
            HandlePlayerDamaged(10, 90, 100);
        }

        [ContextMenu("Test Player Heal Sound")]
        private void DebugTestPlayerHeal()
        {
            HandlePlayerHealed(20, 100, 100);
        }

        [ContextMenu("Test Player Death Sound")]
        private void DebugTestPlayerDeath()
        {
            HandlePlayerDeath();
        }
    }
}
