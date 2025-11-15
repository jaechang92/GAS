namespace GASPT.ResourceManagement
{
    /// <summary>
    /// 리소스 경로 상수 관리 클래스
    /// Resources 폴더 내 모든 리소스 경로를 상수로 정의하여 중앙 집중식 관리
    ///
    /// <para><b>사용 목적:</b></para>
    /// <list type="bullet">
    /// <item>하드코딩된 경로 문자열 제거</item>
    /// <item>오타로 인한 런타임 에러 방지</item>
    /// <item>리팩토링 시 경로 변경 용이</item>
    /// <item>IDE 자동완성 지원</item>
    /// </list>
    ///
    /// <para><b>사용 예제:</b></para>
    /// <code>
    /// // Prefab 로드
    /// GameObject prefab = GameResourceManager.Instance.LoadPrefab(
    ///     ResourcePaths.Prefabs.Player.MageForm
    /// );
    ///
    /// // ScriptableObject 로드
    /// StatusEffectData data = GameResourceManager.Instance.LoadScriptableObject&lt;StatusEffectData&gt;(
    ///     ResourcePaths.Data.StatusEffects.AttackUp
    /// );
    ///
    /// // 오브젝트 풀에서 스폰
    /// FireballProjectile projectile = PoolManager.Instance.Spawn&lt;FireballProjectile&gt;(
    ///     ResourcePaths.Prefabs.Projectiles.FireballProjectile,
    ///     position,
    ///     rotation
    /// );
    /// </code>
    ///
    /// <para><b>새 경로 추가 방법:</b></para>
    /// <list type="number">
    /// <item>해당 카테고리의 static class에 const string 추가</item>
    /// <item>경로는 "Prefabs/", "Data/" 등으로 시작 (Resources/ 제외)</item>
    /// <item>XML 주석으로 설명 및 실제 파일 경로 명시</item>
    /// <item>예시 경로는 주석에 (예시) 또는 (예정) 표시</item>
    /// </list>
    ///
    /// <para><b>주의사항:</b></para>
    /// <list type="bullet">
    /// <item>경로는 Resources 폴더 기준 상대 경로 (Resources/ 제외)</item>
    /// <item>확장자(.prefab, .asset) 제외</item>
    /// <item>인스턴스 데이터에 경로 저장 금지 (공유 리소스만 여기 정의)</item>
    /// <item>에디터 전용 경로(Assets/...)는 에디터 스크립트에 별도 정의</item>
    /// </list>
    /// </summary>
    public static class ResourcePaths
    {
        // ====== Prefabs ======

        /// <summary>
        /// Prefab 경로 상수
        /// </summary>
        public static class Prefabs
        {
            // Player Prefabs
            public static class Player
            {
                /// <summary>
                /// MageForm Prefab 경로
                /// Resources/Prefabs/Player/MageForm.prefab
                /// </summary>
                public const string MageForm = "Prefabs/Player/MageForm";

                /// <summary>
                /// WarriorForm Prefab 경로 (예정)
                /// Resources/Prefabs/Player/WarriorForm.prefab
                /// </summary>
                public const string WarriorForm = "Prefabs/Player/WarriorForm";

                /// <summary>
                /// RogueForm Prefab 경로 (예정)
                /// Resources/Prefabs/Player/RogueForm.prefab
                /// </summary>
                public const string RogueForm = "Prefabs/Player/RogueForm";
            }

            // UI Prefabs
            public static class UI
            {
                /// <summary>
                /// DamageNumber Prefab 경로
                /// Resources/Prefabs/UI/DamageNumber.prefab
                /// </summary>
                public const string DamageNumber = "Prefabs/UI/DamageNumber";

                /// <summary>
                /// HPBar Prefab 경로 (예시)
                /// Resources/Prefabs/UI/HPBar.prefab
                /// </summary>
                public const string HPBar = "Prefabs/UI/HPBar";

                /// <summary>
                /// DroppedItem Prefab 경로
                /// Resources/Prefabs/UI/DroppedItem.prefab
                /// </summary>
                public const string DroppedItem = "Prefabs/UI/DroppedItem";

                /// <summary>
                /// BuffIcon Prefab 경로
                /// Resources/Prefabs/UI/BuffIcon.prefab
                /// </summary>
                public const string BuffIcon = "Prefabs/UI/BuffIcon";

                /// <summary>
                /// PickupSlot Prefab 경로
                /// Resources/Prefabs/UI/PickupSlot.prefab
                /// </summary>
                public const string PickupSlot = "Prefabs/UI/ItemPickupSlot";
            }

            // Enemy Prefabs
            public static class Enemies
            {
                /// <summary>
                /// BasicMeleeEnemy Prefab 경로
                /// Resources/Prefabs/Enemies/BasicMeleeEnemy.prefab
                /// </summary>
                public const string Basic = "Prefabs/Enemies/BasicMeleeEnemy";

                /// <summary>
                /// RangedEnemy Prefab 경로
                /// Resources/Prefabs/Enemies/RangedEnemy.prefab
                /// </summary>
                public const string Ranged = "Prefabs/Enemies/RangedEnemy";

                /// <summary>
                /// FlyingEnemy Prefab 경로
                /// Resources/Prefabs/Enemies/FlyingEnemy.prefab
                /// </summary>
                public const string Flying = "Prefabs/Enemies/FlyingEnemy";

                /// <summary>
                /// EliteEnemy Prefab 경로
                /// Resources/Prefabs/Enemies/EliteEnemy.prefab
                /// </summary>
                public const string Elite = "Prefabs/Enemies/EliteEnemy";
            }

            // Effect Prefabs
            public static class Effects
            {
                /// <summary>
                /// HitEffect Prefab 경로 (예시)
                /// Resources/Prefabs/Effects/HitEffect.prefab
                /// </summary>
                public const string HitEffect = "Prefabs/Effects/HitEffect";

                /// <summary>
                /// BuffEffect Prefab 경로 (예시)
                /// Resources/Prefabs/Effects/BuffEffect.prefab
                /// </summary>
                public const string BuffEffect = "Prefabs/Effects/BuffEffect";
            }

            // Projectile Prefabs
            public static class Projectiles
            {
                /// <summary>
                /// FireballProjectile Prefab 경로
                /// Resources/Prefabs/Projectiles/FireballProjectile.prefab
                /// </summary>
                public const string FireballProjectile = "Prefabs/Projectiles/FireballProjectile";

                /// <summary>
                /// MagicMissileProjectile Prefab 경로
                /// Resources/Prefabs/Projectiles/MagicMissileProjectile.prefab
                /// </summary>
                public const string MagicMissileProjectile = "Prefabs/Projectiles/MagicMissileProjectile";

                /// <summary>
                /// EnemyProjectile Prefab 경로
                /// Resources/Prefabs/Projectiles/EnemyProjectile.prefab
                /// </summary>
                public const string EnemyProjectile = "Prefabs/Projectiles/EnemyProjectile";
            }
        }


        // ====== Data (ScriptableObjects) ======

        /// <summary>
        /// ScriptableObject 데이터 경로 상수
        /// </summary>
        public static class Data
        {
            // StatusEffect Data
            public static class StatusEffects
            {
                /// <summary>
                /// AttackUp 효과 데이터 (예시)
                /// Resources/Data/StatusEffects/AttackUp.asset
                /// </summary>
                public const string AttackUp = "Data/StatusEffects/AttackUp";

                /// <summary>
                /// DefenseUp 효과 데이터 (예시)
                /// Resources/Data/StatusEffects/DefenseUp.asset
                /// </summary>
                public const string DefenseUp = "Data/StatusEffects/DefenseUp";

                /// <summary>
                /// Poison 효과 데이터 (예시)
                /// Resources/Data/StatusEffects/Poison.asset
                /// </summary>
                public const string Poison = "Data/StatusEffects/Poison";
            }

            // Enemy Data
            public static class Enemies
            {
                /// <summary>
                /// BasicMeleeGoblin 데이터 (Phase C-1)
                /// Resources/Data/Enemies/BasicMeleeGoblin.asset
                /// </summary>
                public const string BasicMeleeGoblin = "Data/Enemies/BasicMeleeGoblin";

                /// <summary>
                /// RangedGoblin 데이터 (Phase C-1)
                /// Resources/Data/Enemies/RangedGoblin.asset
                /// </summary>
                public const string RangedGoblin = "Data/Enemies/RangedGoblin";

                /// <summary>
                /// FlyingBat 데이터 (Phase C-1)
                /// Resources/Data/Enemies/FlyingBat.asset
                /// </summary>
                public const string FlyingBat = "Data/Enemies/FlyingBat";

                /// <summary>
                /// EliteOrc 데이터 (Phase C-1)
                /// Resources/Data/Enemies/EliteOrc.asset
                /// </summary>
                public const string EliteOrc = "Data/Enemies/EliteOrc";
            }

            // Item Data
            public static class Items
            {
                /// <summary>
                /// HealthPotion 데이터 (예시)
                /// Resources/Data/Items/HealthPotion.asset
                /// </summary>
                public const string HealthPotion = "Data/Items/HealthPotion";
            }
        }


        // ====== Audio ======

        /// <summary>
        /// 오디오 클립 경로 상수
        /// </summary>
        public static class Audio
        {
            // BGM
            public static class BGM
            {
                /// <summary>
                /// 타이틀 BGM (예시)
                /// Resources/Audio/BGM/Title.mp3
                /// </summary>
                public const string Title = "Audio/BGM/Title";

                /// <summary>
                /// 전투 BGM (예시)
                /// Resources/Audio/BGM/Battle.mp3
                /// </summary>
                public const string Battle = "Audio/BGM/Battle";
            }

            // SFX
            public static class SFX
            {
                /// <summary>
                /// 공격 사운드 (예시)
                /// Resources/Audio/SFX/Attack.wav
                /// </summary>
                public const string Attack = "Audio/SFX/Attack";

                /// <summary>
                /// 피격 사운드 (예시)
                /// Resources/Audio/SFX/Hit.wav
                /// </summary>
                public const string Hit = "Audio/SFX/Hit";
            }
        }


        // ====== Sprites ======

        /// <summary>
        /// 스프라이트 경로 상수
        /// </summary>
        public static class Sprites
        {
            // UI Icons
            public static class Icons
            {
                /// <summary>
                /// 공격력 아이콘 (예시)
                /// Resources/Sprites/Icons/Attack.png
                /// </summary>
                public const string Attack = "Sprites/Icons/Attack";

                /// <summary>
                /// 방어력 아이콘 (예시)
                /// Resources/Sprites/Icons/Defense.png
                /// </summary>
                public const string Defense = "Sprites/Icons/Defense";
            }

            // Status Effect Icons
            public static class StatusEffects
            {
                /// <summary>
                /// AttackUp 아이콘 (예시)
                /// Resources/Sprites/StatusEffects/AttackUp.png
                /// </summary>
                public const string AttackUp = "Sprites/StatusEffects/AttackUp";

                /// <summary>
                /// Poison 아이콘 (예시)
                /// Resources/Sprites/StatusEffects/Poison.png
                /// </summary>
                public const string Poison = "Sprites/StatusEffects/Poison";
            }
        }
    }
}
