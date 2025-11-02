using System;
using UnityEngine;
using GASPT.Stats;
using GASPT.Enemies;
using GASPT.StatusEffects;
using GASPT.UI;

namespace GASPT.Skills
{
    /// <summary>
    /// 스킬 실행 로직 클래스
    /// 쿨다운, 마나 소비, 효과 적용 담당
    /// </summary>
    public class Skill
    {
        // ====== 스킬 데이터 ======

        /// <summary>
        /// 스킬 데이터 (ScriptableObject)
        /// </summary>
        public SkillData Data { get; private set; }


        // ====== 쿨다운 관리 ======

        /// <summary>
        /// 현재 쿨다운 중인지
        /// </summary>
        public bool IsOnCooldown { get; private set; }

        /// <summary>
        /// 남은 쿨다운 시간 (초)
        /// </summary>
        public float RemainingCooldown { get; private set; }


        // ====== 이벤트 ======

        /// <summary>
        /// 스킬 사용 성공 시 발생
        /// </summary>
        public event Action<Skill> OnSkillUsed;

        /// <summary>
        /// 스킬 사용 실패 시 발생 (쿨다운/마나 부족)
        /// </summary>
        public event Action<Skill, string> OnSkillFailed;

        /// <summary>
        /// 쿨다운 완료 시 발생
        /// </summary>
        public event Action<Skill> OnCooldownComplete;


        // ====== 생성자 ======

        public Skill(SkillData data)
        {
            if (data == null)
            {
                Debug.LogError("[Skill] 생성자: data가 null입니다.");
                return;
            }

            Data = data;
            IsOnCooldown = false;
            RemainingCooldown = 0f;
        }


        // ====== 스킬 실행 ======

        /// <summary>
        /// 스킬 사용 시도 (쿨다운/마나 확인)
        /// </summary>
        /// <param name="caster">시전자 (플레이어)</param>
        /// <param name="target">대상 (적, Self면 null)</param>
        /// <returns>true: 사용 성공, false: 사용 실패</returns>
        public bool TryExecute(GameObject caster, GameObject target = null)
        {
            if (Data == null)
            {
                Debug.LogError("[Skill] TryExecute(): Data가 null입니다.");
                return false;
            }

            if (caster == null)
            {
                Debug.LogError($"[Skill] TryExecute(): caster가 null입니다. Skill: {Data.skillName}");
                return false;
            }

            // 1. 쿨다운 확인
            if (IsOnCooldown)
            {
                string message = $"쿨다운 중 (남은 시간: {RemainingCooldown:F1}초)";
                Debug.LogWarning($"[Skill] {Data.skillName} 사용 실패: {message}");
                OnSkillFailed?.Invoke(this, message);
                return false;
            }

            // 2. 마나 확인
            PlayerStats playerStats = caster.GetComponent<PlayerStats>();
            if (playerStats == null)
            {
                Debug.LogError($"[Skill] TryExecute(): PlayerStats를 찾을 수 없습니다. Skill: {Data.skillName}");
                return false;
            }

            if (!playerStats.TrySpendMana(Data.manaCost))
            {
                string message = $"마나 부족 (필요: {Data.manaCost}, 현재: {playerStats.CurrentMana})";
                Debug.LogWarning($"[Skill] {Data.skillName} 사용 실패: {message}");
                OnSkillFailed?.Invoke(this, message);
                return false;
            }

            // 3. 타겟 확인 (Enemy 타입인 경우)
            if (Data.targetType == TargetType.Enemy && target == null)
            {
                Debug.LogWarning($"[Skill] {Data.skillName} 사용 실패: 타겟이 없습니다.");
                OnSkillFailed?.Invoke(this, "타겟이 없습니다");
                playerStats.RegenerateMana(Data.manaCost); // 마나 환불
                return false;
            }

            // 4. 스킬 실행
            Execute(caster, target);

            // 5. 쿨다운 시작
            StartCooldown();

            // 이벤트 발생
            OnSkillUsed?.Invoke(this);

            Debug.Log($"[Skill] {Data.skillName} 사용 성공!");
            return true;
        }

        /// <summary>
        /// 스킬 효과 적용 (실제 실행 로직)
        /// </summary>
        /// <param name="caster">시전자</param>
        /// <param name="target">대상</param>
        private void Execute(GameObject caster, GameObject target)
        {
            switch (Data.skillType)
            {
                case SkillType.Damage:
                    ExecuteDamage(caster, target);
                    break;

                case SkillType.Heal:
                    ExecuteHeal(caster);
                    break;

                case SkillType.Buff:
                    ExecuteBuff(caster, target);
                    break;

                case SkillType.Utility:
                    ExecuteUtility(caster, target);
                    break;

                default:
                    Debug.LogWarning($"[Skill] Execute(): 알 수 없는 스킬 타입입니다: {Data.skillType}");
                    break;
            }

            // 파티클 효과 (선택사항)
            if (Data.particleEffect != null)
            {
                Vector3 effectPosition = target != null ? target.transform.position : caster.transform.position;
                GameObject.Instantiate(Data.particleEffect, effectPosition, Quaternion.identity);
            }

            // 사운드 재생 (선택사항)
            if (Data.soundClip != null)
            {
                // AudioSource 컴포넌트가 있다면 재생 (간단한 구현)
                AudioSource audioSource = caster.GetComponent<AudioSource>();
                if (audioSource != null)
                {
                    audioSource.PlayOneShot(Data.soundClip);
                }
            }
        }


        // ====== 스킬 타입별 실행 로직 ======

        /// <summary>
        /// 데미지형 스킬 실행
        /// </summary>
        private void ExecuteDamage(GameObject caster, GameObject target)
        {
            if (target == null)
            {
                Debug.LogWarning($"[Skill] ExecuteDamage(): target이 null입니다.");
                return;
            }

            Enemy enemy = target.GetComponent<Enemy>();
            if (enemy == null)
            {
                Debug.LogWarning($"[Skill] ExecuteDamage(): Enemy 컴포넌트를 찾을 수 없습니다. Target: {target.name}");
                return;
            }

            // 데미지 적용
            enemy.TakeDamage(Data.damageAmount);

            Debug.Log($"[Skill] {Data.skillName} → {target.name}에게 {Data.damageAmount} 데미지!");
        }

        /// <summary>
        /// 회복형 스킬 실행
        /// </summary>
        private void ExecuteHeal(GameObject caster)
        {
            PlayerStats playerStats = caster.GetComponent<PlayerStats>();
            if (playerStats == null)
            {
                Debug.LogWarning($"[Skill] ExecuteHeal(): PlayerStats를 찾을 수 없습니다.");
                return;
            }

            // 체력 회복
            playerStats.Heal(Data.healAmount);

            Debug.Log($"[Skill] {Data.skillName} → {Data.healAmount} 회복!");
        }

        /// <summary>
        /// 버프형 스킬 실행
        /// </summary>
        private void ExecuteBuff(GameObject caster, GameObject target)
        {
            if (Data.statusEffect == null)
            {
                Debug.LogWarning($"[Skill] ExecuteBuff(): StatusEffect가 설정되지 않았습니다.");
                return;
            }

            GameObject buffTarget = Data.targetType == TargetType.Self ? caster : target;

            if (buffTarget == null)
            {
                Debug.LogWarning($"[Skill] ExecuteBuff(): buffTarget이 null입니다.");
                return;
            }

            // StatusEffect 적용
            StatusEffectManager.Instance.ApplyEffect(buffTarget, Data.statusEffect);

            Debug.Log($"[Skill] {Data.skillName} → {buffTarget.name}에게 {Data.statusEffect.displayName} 효과!");
        }

        /// <summary>
        /// 유틸리티형 스킬 실행
        /// </summary>
        private void ExecuteUtility(GameObject caster, GameObject target)
        {
            // TODO: 유틸리티 스킬은 추후 구현 (대시, 보호막 등)
            Debug.Log($"[Skill] {Data.skillName} 실행 (Utility - 미구현)");
        }


        // ====== 쿨다운 관리 ======

        /// <summary>
        /// 쿨다운 시작
        /// </summary>
        private void StartCooldown()
        {
            if (Data.cooldown <= 0f)
            {
                IsOnCooldown = false;
                RemainingCooldown = 0f;
                return;
            }

            IsOnCooldown = true;
            RemainingCooldown = Data.cooldown;

            // async Awaitable로 쿨다운 타이머 실행
            RunCooldownTimer();
        }

        /// <summary>
        /// 쿨다운 타이머 (async Awaitable)
        /// </summary>
        private async void RunCooldownTimer()
        {
            while (RemainingCooldown > 0f)
            {
                await Awaitable.WaitForSecondsAsync(0.1f); // 0.1초마다 체크
                RemainingCooldown -= 0.1f;
            }

            RemainingCooldown = 0f;
            IsOnCooldown = false;

            // 쿨다운 완료 이벤트
            OnCooldownComplete?.Invoke(this);

            Debug.Log($"[Skill] {Data.skillName} 쿨다운 완료!");
        }

        /// <summary>
        /// 쿨다운 비율 (0.0 ~ 1.0) - UI용
        /// </summary>
        public float GetCooldownRatio()
        {
            if (Data.cooldown <= 0f) return 0f;
            return RemainingCooldown / Data.cooldown;
        }
    }
}
