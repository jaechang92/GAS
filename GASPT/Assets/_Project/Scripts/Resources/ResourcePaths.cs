namespace GASPT.ResourceManagement
{
    /// <summary>
    /// 리소스 경로 상수 관리 클래스
    /// Resources 폴더 내 모든 리소스 경로를 상수로 정의
    /// </summary>
    public static class ResourcePaths
    {
        // ====== Prefabs ======

        /// <summary>
        /// Prefab 경로 상수
        /// </summary>
        public static class Prefabs
        {
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
            }

            // Enemy Prefabs
            public static class Enemies
            {
                /// <summary>
                /// BasicMeleeEnemy Prefab 경로
                /// Resources/Prefabs/Enemy/BasicMeleeEnemy.prefab
                /// </summary>
                public const string Basic = "Prefabs/Enemies/BasicMeleeEnemy";
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
                /// Goblin 데이터 (예시)
                /// Resources/Data/Enemies/Goblin.asset
                /// </summary>
                public const string Goblin = "Data/Enemies/Goblin";
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
