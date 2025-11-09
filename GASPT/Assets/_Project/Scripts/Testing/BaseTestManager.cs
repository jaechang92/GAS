using UnityEngine;

namespace GASPT.Testing
{
    /// <summary>
    /// 테스트 씬을 위한 베이스 클래스
    /// OnGUI 기본 기능 제공 (일시정지, FPS, UI 토글)
    ///
    /// 사용법:
    /// 1. 이 클래스를 상속받아 새 테스트 매니저 작성
    /// 2. DrawCustomGUI() 메서드를 오버라이드하여 커스텀 UI 추가
    /// </summary>
    public abstract class BaseTestManager : MonoBehaviour
    {
        // ====== 공통 상태 ======

        /// <summary>
        /// 일시정지 상태
        /// </summary>
        protected bool isPaused = false;

        /// <summary>
        /// OnGUI UI 표시 여부
        /// </summary>
        protected bool showUI = true;

        /// <summary>
        /// FPS 계산용 변수
        /// </summary>
        private float deltaTime = 0f;


        // ====== Unity 생명주기 ======

        protected virtual void Update()
        {
            HandleCommonInput();
            CalculateFPS();
        }


        // ====== 공통 기능 ======

        /// <summary>
        /// 일시정지 토글
        /// </summary>
        public void TogglePause()
        {
            isPaused = !isPaused;
            Time.timeScale = isPaused ? 0f : 1f;
            Debug.Log($"[BaseTestManager] 일시정지: {(isPaused ? "활성화" : "비활성화")}");
        }

        /// <summary>
        /// FPS 계산
        /// </summary>
        private void CalculateFPS()
        {
            deltaTime += (Time.unscaledDeltaTime - deltaTime) * 0.1f;
        }

        /// <summary>
        /// 공통 입력 처리 (F10: UI 토글)
        /// </summary>
        private void HandleCommonInput()
        {
            if (Input.GetKeyDown(KeyCode.F10))
            {
                showUI = !showUI;
            }
        }


        // ====== OnGUI ======

        private void OnGUI()
        {
            if (!showUI) return;

            // 스타일 초기화
            InitGUIStyles(out GUIStyle boxStyle, out GUIStyle buttonStyle,
                          out GUIStyle labelStyle, out GUIStyle titleStyle);

            // 커스텀 GUI 그리기 (자식 클래스에서 구현)
            DrawCustomGUI(boxStyle, buttonStyle, labelStyle, titleStyle);

            // 공통 정보 패널 (좌측 하단)
            DrawCommonInfoPanel(labelStyle, titleStyle, boxStyle);
        }

        /// <summary>
        /// GUI 스타일 초기화
        /// </summary>
        private void InitGUIStyles(out GUIStyle boxStyle, out GUIStyle buttonStyle,
                                   out GUIStyle labelStyle, out GUIStyle titleStyle)
        {
            boxStyle = new GUIStyle(GUI.skin.box);
            boxStyle.normal.background = MakeTex(2, 2, new Color(0f, 0f, 0f, 0.8f));

            buttonStyle = new GUIStyle(GUI.skin.button);
            buttonStyle.fontSize = 12;
            buttonStyle.normal.textColor = Color.white;

            labelStyle = new GUIStyle(GUI.skin.label);
            labelStyle.fontSize = 12;
            labelStyle.normal.textColor = Color.white;

            titleStyle = new GUIStyle(GUI.skin.label);
            titleStyle.fontSize = 14;
            titleStyle.fontStyle = FontStyle.Bold;
            titleStyle.normal.textColor = Color.yellow;
        }

        /// <summary>
        /// 커스텀 GUI 그리기 (자식 클래스에서 오버라이드)
        /// </summary>
        protected abstract void DrawCustomGUI(GUIStyle boxStyle, GUIStyle buttonStyle,
                                               GUIStyle labelStyle, GUIStyle titleStyle);

        /// <summary>
        /// 공통 정보 패널 (좌측 하단)
        /// </summary>
        private void DrawCommonInfoPanel(GUIStyle labelStyle, GUIStyle titleStyle, GUIStyle boxStyle)
        {
            GUILayout.BeginArea(new Rect(10, Screen.height - 120, 200, 110), boxStyle);

            GUILayout.Label("[ System Info ]", titleStyle);

            // FPS 표시
            float fps = 1.0f / deltaTime;
            Color fpsColor = fps >= 60 ? Color.green : fps >= 30 ? Color.yellow : Color.red;
            GUIStyle fpsStyle = new GUIStyle(labelStyle);
            fpsStyle.normal.textColor = fpsColor;
            GUILayout.Label($"FPS: {fps:F0}", fpsStyle);

            // 일시정지 상태
            GUIStyle pauseStyle = new GUIStyle(labelStyle);
            pauseStyle.normal.textColor = isPaused ? Color.red : Color.green;
            GUILayout.Label($"Paused: {(isPaused ? "YES" : "NO")}", pauseStyle);

            GUILayout.Space(10);

            // 일시정지 버튼
            if (GUILayout.Button($"Toggle Pause", GUI.skin.button))
                TogglePause();

            GUILayout.Label("F10: Toggle UI", labelStyle);

            GUILayout.EndArea();
        }

        /// <summary>
        /// OnGUI용 텍스처 생성 (배경색)
        /// </summary>
        protected Texture2D MakeTex(int width, int height, Color color)
        {
            Color[] pix = new Color[width * height];
            for (int i = 0; i < pix.Length; i++)
                pix[i] = color;

            Texture2D result = new Texture2D(width, height);
            result.SetPixels(pix);
            result.Apply();
            return result;
        }
    }
}
