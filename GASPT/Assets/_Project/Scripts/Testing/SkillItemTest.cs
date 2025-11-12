using UnityEngine;
using GASPT.Data;
using GASPT.Form;
using GASPT.Gameplay.Item;
using GASPT.Loot;

namespace GASPT.Testing
{
    /// <summary>
    /// SkillItem 시스템 테스트 도구
    /// Context Menu로 각종 테스트 실행
    /// </summary>
    public class SkillItemTest : MonoBehaviour
    {
        [Header("테스트 참조")]
        [SerializeField] private GameObject mageFormObject;
        [SerializeField] private SkillItem testIceBlastItem;
        [SerializeField] private SkillItem testLightningBoltItem;
        [SerializeField] private SkillItem testShieldItem;
        [SerializeField] private SkillItem testFireballItem;
        [SerializeField] private SkillItem testTeleportItem;

        [Header("LootSystem 테스트")]
        [SerializeField] private LootTable skillLootTable;

        private IFormController mageForm;

        private void Start()
        {
            // MageForm 참조 가져오기
            if (mageFormObject != null)
            {
                mageForm = mageFormObject.GetComponent<IFormController>();
            }
        }


        // ====== 테스트 01: 시스템 초기화 확인 ======

        [ContextMenu("Test01: Check System Init")]
        private void Test01_CheckSystemInit()
        {
            Debug.Log("========== Test01: System Init Check ==========");

            // SkillItemManager 확인
            if (SkillItemManager.HasInstance)
            {
                Debug.Log("✓ SkillItemManager: 초기화됨");
            }
            else
            {
                Debug.LogError("✗ SkillItemManager: 초기화 안됨!");
            }

            // LootSystem 확인
            if (LootSystem.HasInstance)
            {
                Debug.Log("✓ LootSystem: 초기화됨");
            }
            else
            {
                Debug.LogError("✗ LootSystem: 초기화 안됨!");
            }

            // MageForm 확인
            if (mageForm != null)
            {
                Debug.Log($"✓ MageForm: {mageForm.FormName}");
            }
            else
            {
                Debug.LogError("✗ MageForm: 참조 없음!");
            }

            Debug.Log("==============================================");
        }


        // ====== 테스트 02: ScriptableObject 생성 ======

        [ContextMenu("Test02: Create Test SkillItems")]
        private void Test02_CreateTestSkillItems()
        {
            Debug.Log("========== Test02: Create Test SkillItems ==========");
            Debug.Log("Unity 에디터에서 수동으로 생성해야 합니다:");
            Debug.Log("1. Create > GASPT > Items > Skill Item");
            Debug.Log("2. 아이템 설정:");
            Debug.Log("   - IceBlast (Slot 1, Rare, 3초 쿨다운)");
            Debug.Log("   - LightningBolt (Slot 2, Epic, 4초 쿨다운)");
            Debug.Log("   - Shield (Slot 3, Rare, 8초 쿨다운)");
            Debug.Log("   - Fireball (Slot 2, Common, 5초 쿨다운)");
            Debug.Log("   - Teleport (Slot 1, Rare, 3초 쿨다운)");
            Debug.Log("3. Inspector에서 SkillItemTest에 할당");
            Debug.Log("====================================================");
        }


        // ====== 테스트 03: Form 설정 ======

        [ContextMenu("Test03: Set Current Form")]
        private void Test03_SetCurrentForm()
        {
            Debug.Log("========== Test03: Set Current Form ==========");

            if (!SkillItemManager.HasInstance)
            {
                Debug.LogError("SkillItemManager가 초기화되지 않았습니다!");
                return;
            }

            if (mageForm == null)
            {
                Debug.LogError("MageForm 참조가 없습니다!");
                return;
            }

            // SkillItemManager에 현재 Form 설정
            SkillItemManager.Instance.SetCurrentForm(mageForm);

            Debug.Log($"✓ 현재 Form 설정 완료: {mageForm.FormName}");
            Debug.Log("==============================================");
        }


        // ====== 테스트 04: IceBlast 장착 ======

        [ContextMenu("Test04: Equip IceBlast to Slot 1")]
        private void Test04_EquipIceBlast()
        {
            Debug.Log("========== Test04: Equip IceBlast ==========");

            if (!ValidateTestSetup())
                return;

            if (testIceBlastItem == null)
            {
                Debug.LogError("testIceBlastItem이 할당되지 않았습니다!");
                return;
            }

            // IceBlast 장착
            bool success = SkillItemManager.Instance.EquipSkillItem(testIceBlastItem);

            if (success)
            {
                Debug.Log($"✓ {testIceBlastItem.itemName} 장착 완료 (슬롯 {testIceBlastItem.targetSlotIndex})");

                // 장착된 스킬 확인
                IAbility ability = mageForm.GetAbility(testIceBlastItem.targetSlotIndex);
                if (ability != null)
                {
                    Debug.Log($"  - Ability Name: {ability.AbilityName}");
                    Debug.Log($"  - Cooldown: {ability.Cooldown}s");
                }
            }
            else
            {
                Debug.LogError("✗ IceBlast 장착 실패!");
            }

            Debug.Log("===========================================");
        }


        // ====== 테스트 05: LightningBolt 장착 ======

        [ContextMenu("Test05: Equip LightningBolt to Slot 2")]
        private void Test05_EquipLightningBolt()
        {
            Debug.Log("========== Test05: Equip LightningBolt ==========");

            if (!ValidateTestSetup())
                return;

            if (testLightningBoltItem == null)
            {
                Debug.LogError("testLightningBoltItem이 할당되지 않았습니다!");
                return;
            }

            // LightningBolt 장착
            bool success = SkillItemManager.Instance.EquipSkillItem(testLightningBoltItem);

            if (success)
            {
                Debug.Log($"✓ {testLightningBoltItem.itemName} 장착 완료 (슬롯 {testLightningBoltItem.targetSlotIndex})");

                // 장착된 스킬 확인
                IAbility ability = mageForm.GetAbility(testLightningBoltItem.targetSlotIndex);
                if (ability != null)
                {
                    Debug.Log($"  - Ability Name: {ability.AbilityName}");
                    Debug.Log($"  - Cooldown: {ability.Cooldown}s");
                }
            }
            else
            {
                Debug.LogError("✗ LightningBolt 장착 실패!");
            }

            Debug.Log("=================================================");
        }


        // ====== 테스트 06: Shield 장착 ======

        [ContextMenu("Test06: Equip Shield to Slot 3")]
        private void Test06_EquipShield()
        {
            Debug.Log("========== Test06: Equip Shield ==========");

            if (!ValidateTestSetup())
                return;

            if (testShieldItem == null)
            {
                Debug.LogError("testShieldItem이 할당되지 않았습니다!");
                return;
            }

            // Shield 장착
            bool success = SkillItemManager.Instance.EquipSkillItem(testShieldItem);

            if (success)
            {
                Debug.Log($"✓ {testShieldItem.itemName} 장착 완료 (슬롯 {testShieldItem.targetSlotIndex})");

                // 장착된 스킬 확인
                IAbility ability = mageForm.GetAbility(testShieldItem.targetSlotIndex);
                if (ability != null)
                {
                    Debug.Log($"  - Ability Name: {ability.AbilityName}");
                    Debug.Log($"  - Cooldown: {ability.Cooldown}s");
                }
            }
            else
            {
                Debug.LogError("✗ Shield 장착 실패!");
            }

            Debug.Log("==========================================");
        }


        // ====== 테스트 07: 스킬 해제 ======

        [ContextMenu("Test07: Unequip Slot 1")]
        private void Test07_UnequipSlot()
        {
            Debug.Log("========== Test07: Unequip Slot 1 ==========");

            if (!ValidateTestSetup())
                return;

            // 슬롯 1 해제
            bool success = SkillItemManager.Instance.UnequipSkillItem(1);

            if (success)
            {
                Debug.Log("✓ 슬롯 1 해제 완료");

                // 해제 확인
                IAbility ability = mageForm.GetAbility(1);
                if (ability == null)
                {
                    Debug.Log("  - 슬롯 1: 비어있음 (정상)");
                }
                else
                {
                    Debug.LogWarning($"  - 슬롯 1: {ability.AbilityName} (해제 안됨?)");
                }
            }
            else
            {
                Debug.LogError("✗ 슬롯 1 해제 실패!");
            }

            Debug.Log("============================================");
        }


        // ====== 테스트 08: LootSystem 연동 (드롭 시뮬레이션) ======

        [ContextMenu("Test08: Drop Skill Item (LootSystem)")]
        private void Test08_DropSkillItem()
        {
            Debug.Log("========== Test08: Drop Skill Item ==========");

            if (!ValidateTestSetup())
                return;

            if (!LootSystem.HasInstance)
            {
                Debug.LogError("LootSystem이 초기화되지 않았습니다!");
                return;
            }

            if (skillLootTable == null)
            {
                Debug.LogError("skillLootTable이 할당되지 않았습니다!");
                return;
            }

            // 테스트 위치
            Vector3 dropPosition = transform.position;

            // LootTable에서 드롭
            Debug.Log("LootTable에서 스킬 아이템 드롭 중...");
            LootSystem.Instance.DropLoot(skillLootTable, dropPosition);

            Debug.Log("✓ 드롭 완료 (월드에 DroppedItem 생성됨)");
            Debug.Log("  플레이어가 아이템을 획득하면 자동으로 스킬이 장착됩니다.");

            Debug.Log("=============================================");
        }


        // ====== 테스트 09: 모든 장착된 스킬 출력 ======

        [ContextMenu("Test09: Print Equipped Skills")]
        private void Test09_PrintEquippedSkills()
        {
            Debug.Log("========== Test09: Equipped Skills ==========");

            if (!ValidateTestSetup())
                return;

            var equippedSkills = SkillItemManager.Instance.GetAllEquippedSkills();

            Debug.Log($"총 {equippedSkills.Count}개의 스킬 장착됨:");

            foreach (var kvp in equippedSkills)
            {
                Debug.Log($"  슬롯 {kvp.Key}: {kvp.Value.itemName} ({kvp.Value.abilityType}, {kvp.Value.rarity})");
            }

            Debug.Log("=============================================");
        }


        // ====== 헬퍼 메서드 ======

        /// <summary>
        /// 테스트 환경 유효성 검증
        /// </summary>
        private bool ValidateTestSetup()
        {
            if (!SkillItemManager.HasInstance)
            {
                Debug.LogError("SkillItemManager가 초기화되지 않았습니다!");
                return false;
            }

            if (mageForm == null)
            {
                Debug.LogError("MageForm 참조가 없습니다! Test03을 먼저 실행하세요.");
                return false;
            }

            return true;
        }
    }
}
