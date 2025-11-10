using UnityEngine;

namespace GASPT.Form
{
    /// <summary>
    /// 폼의 데이터를 저장하는 ScriptableObject
    /// Unity 에디터에서 쉽게 설정할 수 있도록 디자인
    /// </summary>
    [CreateAssetMenu(fileName = "FormData", menuName = "GASPT/Form/Form Data")]
    public class FormData : ScriptableObject
    {
        [Header("기본 정보")]
        [Tooltip("폼의 이름 (예: 마법사, 전사)")]
        public string formName = "New Form";

        [Tooltip("폼 타입")]
        public FormType formType = FormType.Mage;

        [Tooltip("폼 아이콘 (UI용)")]
        public Sprite icon;

        [Header("스탯")]
        [Tooltip("최대 체력")]
        public float maxHealth = 100f;

        [Tooltip("이동 속도")]
        public float moveSpeed = 5f;

        [Tooltip("점프 파워")]
        public float jumpPower = 10f;

        [Header("스킬 (나중에 GAS Core와 통합 예정)")]
        [Tooltip("기본 공격 스킬")]
        public string basicAttackName = "Basic Attack";

        [Tooltip("기본 스킬들")]
        public string[] defaultSkillNames = new string[]
        {
            "Skill 1",
            "Skill 2",
            "Skill 3"
        };

        [Header("비주얼")]
        [Tooltip("폼의 스프라이트")]
        public Sprite formSprite;

        [Tooltip("폼의 색상")]
        public Color formColor = Color.white;

        /// <summary>
        /// 디버그용 정보 출력
        /// </summary>
        [ContextMenu("Print Info")]
        public void PrintInfo()
        {
            Debug.Log($"[FormData] {formName} ({formType})\n" +
                     $"Stats: HP={maxHealth}, Speed={moveSpeed}, Jump={jumpPower}\n" +
                     $"Skills: {basicAttackName}, {string.Join(", ", defaultSkillNames)}");
        }
    }
}
