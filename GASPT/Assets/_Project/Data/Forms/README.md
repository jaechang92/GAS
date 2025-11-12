# Forms 폴더

**용도**: Form 데이터 ScriptableObject 저장
**파일 타입**: `FormData.asset`
**스크립트**: `Assets/_Project/Scripts/Gameplay/Form/Core/FormData.cs`

---

## 생성 방법

Unity 에디터에서:
```
우클릭 > Create > GASPT > Form > Form Data
```

---

## 필수 설정

1. **formName**: Form 이름 (예: "Mage", "Warrior")
2. **formType**: Form 타입 Enum (Mage, Warrior, Assassin, Tank)
3. **maxHealth**: 최대 HP (예: Mage 80, Warrior 150)
4. **moveSpeed**: 이동 속도 (예: Mage 7, Warrior 5)
5. **jumpPower**: 점프력 (예: Mage 12, Warrior 8)
6. **defaultAbilityNames**: 기본 스킬 이름 배열 (4개)
7. **icon**: Form 아이콘 Sprite
8. **color**: Form 대표 색상

---

## 하위 폴더 구분

- **Mage/**: 마법사 Form 관련
- **Warrior/**: 전사 Form 관련 (향후)
- **Assassin/**: 암살자 Form 관련 (향후)

---

## 예시 파일

- `Mage/MageFormData.asset` - 마법사 (HP 80, Speed 7, Jump 12)
- `Warrior/WarriorFormData.asset` - 전사 (HP 150, Speed 5, Jump 8)

---

## BaseForm 연동

1. MageForm 등 Form 스크립트에서 `formData` SerializeField로 참조
2. `BaseForm.MaxHealth`, `MoveSpeed`, `JumpPower` 프로퍼티로 접근
3. 스탯 변경 시 FormData만 수정하면 모든 인스턴스에 적용
