using System.Collections.Generic;
using UnityEngine;
using GASPT.Core.Pooling;

namespace GASPT.Gameplay.Boss
{
    /// <summary>
    /// 텔레그래프 타입
    /// </summary>
    public enum TelegraphType
    {
        Circle,     // 원형 (범위 공격)
        Line,       // 직선 (돌진, 빔)
        Marker,     // 마커 (추적 공격)
        FullScreen  // 전체 화면 (필살기)
    }

    /// <summary>
    /// 텔레그래프 컨트롤러
    /// 보스 공격 예고 시각화 관리
    /// </summary>
    public class TelegraphController : MonoBehaviour
    {
        // ====== 싱글톤 ======

        private static TelegraphController instance;
        public static TelegraphController Instance
        {
            get
            {
                if (instance == null)
                {
                    var go = new GameObject("TelegraphController");
                    instance = go.AddComponent<TelegraphController>();
                    DontDestroyOnLoad(go);
                }
                return instance;
            }
        }


        // ====== 프리팹 참조 ======

        [Header("텔레그래프 프리팹")]
        [SerializeField]
        private GameObject circleTelegraphPrefab;

        [SerializeField]
        private GameObject lineTelegraphPrefab;

        [SerializeField]
        private GameObject markerTelegraphPrefab;

        [SerializeField]
        private GameObject fullScreenTelegraphPrefab;


        // ====== 활성 텔레그래프 ======

        private List<GameObject> activeTelegraphs = new List<GameObject>();


        // ====== Unity 생명주기 ======

        private void Awake()
        {
            if (instance != null && instance != this)
            {
                Destroy(gameObject);
                return;
            }

            instance = this;
        }

        private void OnDestroy()
        {
            if (instance == this)
            {
                instance = null;
            }
        }


        // ====== 원형 텔레그래프 ======

        /// <summary>
        /// 원형 텔레그래프 표시
        /// </summary>
        public GameObject ShowCircle(Vector3 center, float radius, float duration, Color? color = null)
        {
            GameObject telegraph;

            if (circleTelegraphPrefab != null)
            {
                telegraph = Instantiate(circleTelegraphPrefab, center, Quaternion.identity);
                telegraph.transform.localScale = Vector3.one * radius * 2f;
            }
            else
            {
                // 프리팹이 없으면 기본 생성
                telegraph = CreateDefaultCircleTelegraph(center, radius, color ?? new Color(1f, 0f, 0f, 0.3f));
            }

            activeTelegraphs.Add(telegraph);

            // 자동 제거
            if (duration > 0)
            {
                DestroyTelegraphAfterDelay(telegraph, duration);
            }

            return telegraph;
        }

        /// <summary>
        /// 기본 원형 텔레그래프 생성
        /// </summary>
        private GameObject CreateDefaultCircleTelegraph(Vector3 center, float radius, Color color)
        {
            GameObject telegraph = new GameObject("CircleTelegraph");
            telegraph.transform.position = center;

            // SpriteRenderer 추가
            var sr = telegraph.AddComponent<SpriteRenderer>();
            sr.sprite = CreateCircleSprite();
            sr.color = color;
            sr.sortingOrder = 100;

            telegraph.transform.localScale = Vector3.one * radius * 2f;

            return telegraph;
        }


        // ====== 직선 텔레그래프 ======

        /// <summary>
        /// 직선 텔레그래프 표시
        /// </summary>
        public GameObject ShowLine(Vector3 start, Vector3 end, float width, float duration, Color? color = null)
        {
            GameObject telegraph;

            if (lineTelegraphPrefab != null)
            {
                Vector3 midPoint = (start + end) / 2f;
                float distance = Vector3.Distance(start, end);
                float angle = Mathf.Atan2(end.y - start.y, end.x - start.x) * Mathf.Rad2Deg;

                telegraph = Instantiate(lineTelegraphPrefab, midPoint, Quaternion.Euler(0, 0, angle));
                telegraph.transform.localScale = new Vector3(distance, width, 1f);
            }
            else
            {
                telegraph = CreateDefaultLineTelegraph(start, end, width, color ?? new Color(1f, 0f, 0f, 0.3f));
            }

            activeTelegraphs.Add(telegraph);

            if (duration > 0)
            {
                DestroyTelegraphAfterDelay(telegraph, duration);
            }

            return telegraph;
        }

        /// <summary>
        /// 기본 직선 텔레그래프 생성
        /// </summary>
        private GameObject CreateDefaultLineTelegraph(Vector3 start, Vector3 end, float width, Color color)
        {
            GameObject telegraph = new GameObject("LineTelegraph");

            Vector3 midPoint = (start + end) / 2f;
            telegraph.transform.position = midPoint;

            float distance = Vector3.Distance(start, end);
            float angle = Mathf.Atan2(end.y - start.y, end.x - start.x) * Mathf.Rad2Deg;
            telegraph.transform.rotation = Quaternion.Euler(0, 0, angle);

            var sr = telegraph.AddComponent<SpriteRenderer>();
            sr.sprite = CreateSquareSprite();
            sr.color = color;
            sr.sortingOrder = 100;

            telegraph.transform.localScale = new Vector3(distance, width, 1f);

            return telegraph;
        }


        // ====== 마커 텔레그래프 ======

        /// <summary>
        /// 마커 텔레그래프 표시 (타겟 추적)
        /// </summary>
        public GameObject ShowMarker(Transform target, float duration, Color? color = null)
        {
            GameObject telegraph;

            if (markerTelegraphPrefab != null)
            {
                telegraph = Instantiate(markerTelegraphPrefab, target.position, Quaternion.identity);
            }
            else
            {
                telegraph = CreateDefaultMarkerTelegraph(target.position, color ?? new Color(1f, 1f, 0f, 0.5f));
            }

            // 타겟 추적 컴포넌트 추가
            var tracker = telegraph.AddComponent<TelegraphTracker>();
            tracker.Initialize(target, duration);

            activeTelegraphs.Add(telegraph);

            if (duration > 0)
            {
                DestroyTelegraphAfterDelay(telegraph, duration);
            }

            return telegraph;
        }

        /// <summary>
        /// 기본 마커 텔레그래프 생성
        /// </summary>
        private GameObject CreateDefaultMarkerTelegraph(Vector3 position, Color color)
        {
            GameObject telegraph = new GameObject("MarkerTelegraph");
            telegraph.transform.position = position;

            var sr = telegraph.AddComponent<SpriteRenderer>();
            sr.sprite = CreateCircleSprite();
            sr.color = color;
            sr.sortingOrder = 100;

            telegraph.transform.localScale = Vector3.one * 1.5f;

            return telegraph;
        }


        // ====== 전체 화면 텔레그래프 ======

        /// <summary>
        /// 전체 화면 텔레그래프 표시
        /// </summary>
        public GameObject ShowFullScreen(float duration, Color? color = null)
        {
            GameObject telegraph;

            if (fullScreenTelegraphPrefab != null)
            {
                telegraph = Instantiate(fullScreenTelegraphPrefab);
            }
            else
            {
                telegraph = CreateDefaultFullScreenTelegraph(color ?? new Color(1f, 0f, 0f, 0.2f));
            }

            activeTelegraphs.Add(telegraph);

            if (duration > 0)
            {
                DestroyTelegraphAfterDelay(telegraph, duration);
            }

            return telegraph;
        }

        /// <summary>
        /// 기본 전체 화면 텔레그래프 생성
        /// </summary>
        private GameObject CreateDefaultFullScreenTelegraph(Color color)
        {
            GameObject telegraph = new GameObject("FullScreenTelegraph");

            // 카메라 중심에 배치
            UnityEngine.Camera mainCam = UnityEngine.Camera.main;
            if (mainCam != null)
            {
                telegraph.transform.position = new Vector3(
                    mainCam.transform.position.x,
                    mainCam.transform.position.y,
                    0
                );
            }

            var sr = telegraph.AddComponent<SpriteRenderer>();
            sr.sprite = CreateSquareSprite();
            sr.color = color;
            sr.sortingOrder = 99;

            // 화면 전체 덮기
            telegraph.transform.localScale = new Vector3(100f, 100f, 1f);

            return telegraph;
        }


        // ====== 정리 ======

        /// <summary>
        /// 모든 텔레그래프 숨기기
        /// </summary>
        public void HideAll()
        {
            foreach (var telegraph in activeTelegraphs)
            {
                if (telegraph != null)
                {
                    Destroy(telegraph);
                }
            }

            activeTelegraphs.Clear();
        }

        /// <summary>
        /// 특정 텔레그래프 제거
        /// </summary>
        public void Hide(GameObject telegraph)
        {
            if (telegraph != null)
            {
                activeTelegraphs.Remove(telegraph);
                Destroy(telegraph);
            }
        }


        // ====== 유틸리티 ======

        /// <summary>
        /// 지연 후 텔레그래프 제거
        /// </summary>
        private async void DestroyTelegraphAfterDelay(GameObject telegraph, float delay)
        {
            await Awaitable.WaitForSecondsAsync(delay);

            if (telegraph != null)
            {
                activeTelegraphs.Remove(telegraph);
                Destroy(telegraph);
            }
        }

        /// <summary>
        /// 원형 스프라이트 생성 (런타임용)
        /// </summary>
        private Sprite CreateCircleSprite()
        {
            int size = 64;
            Texture2D texture = new Texture2D(size, size);

            Color[] pixels = new Color[size * size];
            float center = size / 2f;
            float radius = size / 2f;

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float dx = x - center;
                    float dy = y - center;
                    float dist = Mathf.Sqrt(dx * dx + dy * dy);

                    if (dist < radius)
                    {
                        pixels[y * size + x] = Color.white;
                    }
                    else
                    {
                        pixels[y * size + x] = Color.clear;
                    }
                }
            }

            texture.SetPixels(pixels);
            texture.Apply();

            return Sprite.Create(texture, new Rect(0, 0, size, size), Vector2.one * 0.5f, size);
        }

        /// <summary>
        /// 사각형 스프라이트 생성 (런타임용)
        /// </summary>
        private Sprite CreateSquareSprite()
        {
            int size = 4;
            Texture2D texture = new Texture2D(size, size);

            Color[] pixels = new Color[size * size];
            for (int i = 0; i < pixels.Length; i++)
            {
                pixels[i] = Color.white;
            }

            texture.SetPixels(pixels);
            texture.Apply();

            return Sprite.Create(texture, new Rect(0, 0, size, size), Vector2.one * 0.5f, size);
        }
    }


    /// <summary>
    /// 타겟 추적 컴포넌트 (마커용)
    /// </summary>
    public class TelegraphTracker : MonoBehaviour
    {
        private Transform target;
        private float duration;
        private float startTime;

        public void Initialize(Transform target, float duration)
        {
            this.target = target;
            this.duration = duration;
            this.startTime = Time.time;
        }

        private void Update()
        {
            if (target != null)
            {
                transform.position = target.position;
            }

            // 시간 초과 시 자동 파괴
            if (Time.time - startTime > duration)
            {
                Destroy(gameObject);
            }
        }
    }
}
