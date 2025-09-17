// ===================================
// ����: Assets/Scripts/Ability/Test/DummyEnemy.cs
// ===================================
using UnityEngine;
using UnityEngine.UI;

namespace AbilitySystem.Platformer.Test
{

    public struct Stretch
    {
        public float left;
        public float right;
        public float top;
        public float bottom;

        public Stretch(float left, float right, float top, float bottom)
        {
            this.left = left;
            this.right = right;
            this.top = top;
            this.bottom = bottom;
        }
    }

    /// <summary>
    /// �׽�Ʈ�� ���� ��
    /// </summary>
    public class DummyEnemy : MonoBehaviour, IAbilityTarget
    {
        private Stretch stretch = new Stretch(0,0,0,0);

        [Header("����")]
        [SerializeField] private float maxHealth = 100f;
        [SerializeField] private float currentHealth = 100f;
        [SerializeField] private bool showDamageNumbers = true;
        [SerializeField] private bool respawnOnDeath = true;
        [SerializeField] private float respawnDelay = 3f;

        // ü�¹�
        private GameObject healthBarObj;
        private Slider healthBar;
        private Vector3 initialPosition;

        // IAbilityTarget ����
        public GameObject GameObject => gameObject;
        public Transform Transform => transform;
        public bool IsAlive => currentHealth > 0;
        public bool IsTargetable => IsAlive;

        private void Start()
        {
            initialPosition = transform.position;
            CreateHealthBar();
            currentHealth = maxHealth;
        }

        /// <summary>
        /// ü�¹� ����
        /// </summary>
        private void CreateHealthBar()
        {
            // World Space Canvas ����
            healthBarObj = new GameObject("Health Bar Canvas");
            healthBarObj.transform.SetParent(transform);
            healthBarObj.transform.localPosition = new Vector3(0, 1.5f, 0);

            Canvas canvas = healthBarObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.WorldSpace;
            healthBarObj.AddComponent<CanvasScaler>();

            RectTransform canvasRect = healthBarObj.GetComponent<RectTransform>();
            canvasRect.sizeDelta = new Vector2(1, 0.2f);

            // ü�¹� ���
            GameObject bgObj = new GameObject("Background");
            bgObj.transform.SetParent(healthBarObj.transform);
            Image bg = bgObj.AddComponent<Image>();
            bg.color = Color.black;
            RectTransform bgRect = bgObj.GetComponent<RectTransform>();
            bgRect.anchorMin = Vector2.zero;
            bgRect.anchorMax = Vector2.one;
            bgRect.offsetMin = Vector2.zero;
            bgRect.offsetMax = Vector2.zero;

            // ü�¹�
            GameObject barObj = new GameObject("Health Bar");
            barObj.transform.SetParent(healthBarObj.transform);
            healthBar = barObj.AddComponent<Slider>();
            healthBar.fillRect = barObj.transform as RectTransform;

            GameObject fill = new GameObject("Fill");
            fill.transform.SetParent(barObj.transform);
            Image fillImage = fill.AddComponent<Image>();
            fillImage.color = Color.red;
            fillImage.rectTransform.offsetMin = new Vector2(stretch.left, stretch.bottom);
            fillImage.rectTransform.offsetMax = new Vector2(-stretch.right, -stretch.top);
            healthBar.fillRect = fill.GetComponent<RectTransform>();

            RectTransform barRect = barObj.GetComponent<RectTransform>();
            barRect.anchorMin = Vector2.zero;
            barRect.anchorMax = Vector2.one;
            barRect.offsetMin = Vector2.zero;
            barRect.offsetMax = Vector2.zero;

            healthBar.value = 1f;
        }

        /// <summary>
        /// ������ �ޱ�
        /// </summary>
        public void TakeDamage(float damage, GameObject source)
        {
            if (!IsAlive) return;

            currentHealth -= damage;
            currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);

            // ü�¹� ������Ʈ
            if (healthBar != null)
            {
                healthBar.value = currentHealth / maxHealth;
            }

            // ������ ǥ��
            if (showDamageNumbers)
            {
                ShowDamageNumber(damage);
            }

            // �ǰ� ����Ʈ
            StartCoroutine(HitEffect());

            Debug.Log($"{name} ���� ������: {damage}, ���� ü��: {currentHealth}/{maxHealth}");

            if (currentHealth <= 0)
            {
                Die();
            }
        }

        /// <summary>
        /// �� �ޱ�
        /// </summary>
        public void Heal(float amount, GameObject source)
        {
            if (!IsAlive) return;

            currentHealth += amount;
            currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);

            if (healthBar != null)
            {
                healthBar.value = currentHealth / maxHealth;
            }

            Debug.Log($"{name} ��: {amount}, ���� ü��: {currentHealth}/{maxHealth}");
        }

        /// <summary>
        /// ����/����� ����
        /// </summary>
        public void ApplyEffect(string effectId, float duration, GameObject source)
        {
            Debug.Log($"{name}�� {effectId} ȿ�� ���� ({duration}��)");
        }

        /// <summary>
        /// ������ ���� ǥ��
        /// </summary>
        private void ShowDamageNumber(float damage)
        {
            GameObject damageTextObj = new GameObject("Damage Text");
            damageTextObj.transform.position = transform.position + Vector3.up * 1.5f;

            TextMesh textMesh = damageTextObj.AddComponent<TextMesh>();
            textMesh.text = damage.ToString("0");
            textMesh.fontSize = 20;
            textMesh.color = Color.yellow;
            textMesh.alignment = TextAlignment.Center;
            textMesh.anchor = TextAnchor.MiddleCenter;

            // ���� �ö󰡸� ������� �ִϸ��̼�
            StartCoroutine(DamageTextAnimation(damageTextObj));
        }

        /// <summary>
        /// ������ �ؽ�Ʈ �ִϸ��̼�
        /// </summary>
        private System.Collections.IEnumerator DamageTextAnimation(GameObject textObj)
        {
            float timer = 0;
            float duration = 1f;
            Vector3 startPos = textObj.transform.position;

            while (timer < duration)
            {
                timer += Time.deltaTime;
                textObj.transform.position = startPos + Vector3.up * timer;

                var textMesh = textObj.GetComponent<TextMesh>();
                if (textMesh != null)
                {
                    Color color = textMesh.color;
                    color.a = 1f - (timer / duration);
                    textMesh.color = color;
                }

                yield return null;
            }

            Destroy(textObj);
        }

        /// <summary>
        /// �ǰ� ����Ʈ
        /// </summary>
        private System.Collections.IEnumerator HitEffect()
        {
            var renderer = GetComponent<SpriteRenderer>();
            if (renderer == null)
            {
                renderer = GetComponentInChildren<SpriteRenderer>();
            }

            if (renderer != null)
            {
                Color originalColor = renderer.color;
                renderer.color = Color.white;
                yield return new WaitForSeconds(0.1f);
                renderer.color = originalColor;
            }
        }

        /// <summary>
        /// ��� ó��
        /// </summary>
        private void Die()
        {
            Debug.Log($"{name} ���!");

            // ��� ����Ʈ
            var renderer = GetComponent<SpriteRenderer>();
            if (renderer != null)
            {
                renderer.color = Color.gray;
            }

            // �浹 ��Ȱ��ȭ
            var collider = GetComponent<Collider2D>();
            if (collider != null)
            {
                collider.enabled = false;
            }

            if (respawnOnDeath)
            {
                StartCoroutine(Respawn());
            }
            else
            {
                gameObject.SetActive(false);
            }
        }

        /// <summary>
        /// ������
        /// </summary>
        private System.Collections.IEnumerator Respawn()
        {
            yield return new WaitForSeconds(respawnDelay);

            // �ʱ�ȭ
            currentHealth = maxHealth;
            transform.position = initialPosition;

            // ü�¹� ������Ʈ
            if (healthBar != null)
            {
                healthBar.value = 1f;
            }

            // �ð� ȿ�� ����
            var renderer = GetComponent<SpriteRenderer>();
            if (renderer != null)
            {
                renderer.color = Color.red;
            }

            // �浹 Ȱ��ȭ
            var collider = GetComponent<Collider2D>();
            if (collider != null)
            {
                collider.enabled = true;
            }

            Debug.Log($"{name} ������!");
        }
    }
}
