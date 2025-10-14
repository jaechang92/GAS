using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Core;

namespace Gameplay.Dialogue
{
    /// <summary>
    /// 대화 데이터베이스
    /// CSV 파일을 로드하고 에피소드 데이터를 관리하는 싱글톤
    /// </summary>
    public class DialogueDatabase : SingletonManager<DialogueDatabase>
    {
        [Header("CSV 파일 경로")]
        [SerializeField] private string dialogueTablePath = "Dialogues/DialogueTable";
        [SerializeField] private string choiceTablePath = "Dialogues/ChoiceTable";
        [SerializeField] private string episodeTablePath = "Dialogues/EpisodeTable";

        [Header("디버그")]
        [SerializeField] private bool showDebugLog = true;

        // 에피소드 딕셔너리 (에피소드 ID → 에피소드 데이터)
        private Dictionary<string, DialogueEpisode> episodes = new Dictionary<string, DialogueEpisode>();

        // 로드 완료 플래그
        public bool IsLoaded { get; private set; } = false;

        protected override void OnSingletonAwake()
        {
            Log("DialogueDatabase 초기화");
            LoadAllData();
        }

        /// <summary>
        /// 모든 CSV 데이터 로드
        /// </summary>
        public void LoadAllData()
        {
            try
            {
                Log("=== CSV 데이터 로드 시작 ===");

                // 1. 에피소드 정보 로드
                LoadEpisodeTable();

                // 2. 대화 노드 로드
                LoadDialogueTable();

                // 3. 선택지 로드
                LoadChoiceTable();

                IsLoaded = true;
                Log($"=== CSV 데이터 로드 완료 === (에피소드 수: {episodes.Count})");

                // 디버그: 로드된 에피소드 목록 출력
                if (showDebugLog)
                {
                    foreach (var ep in episodes.Values)
                    {
                        Log($"  - {ep}");
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"[DialogueDatabase] CSV 로드 중 오류 발생: {ex.Message}\n{ex.StackTrace}");
                IsLoaded = false;
            }
        }

        /// <summary>
        /// 에피소드 테이블 로드 (EpisodeTable.csv)
        /// </summary>
        private void LoadEpisodeTable()
        {
            TextAsset csvFile = Resources.Load<TextAsset>(episodeTablePath);
            if (csvFile == null)
            {
                Debug.LogWarning($"[DialogueDatabase] {episodeTablePath}.csv 파일을 찾을 수 없습니다.");
                return;
            }

            string[] lines = csvFile.text.Split('\n');
            Log($"EpisodeTable 로드: {lines.Length - 1}개 행");

            for (int i = 1; i < lines.Length; i++) // 헤더 스킵
            {
                string line = lines[i].Trim();
                if (string.IsNullOrEmpty(line)) continue;

                string[] fields = line.Split(',');
                if (fields.Length < 5)
                {
                    Debug.LogWarning($"[DialogueDatabase] EpisodeTable 행 {i} 형식 오류: {line}");
                    continue;
                }

                string episodeID = fields[0].Trim();
                string episodeName = fields[1].Trim();
                string npcName = fields[2].Trim();
                string episodeTypeStr = fields[3].Trim();
                string description = fields[4].Trim();

                // EpisodeType 파싱
                if (!Enum.TryParse<EpisodeType>(episodeTypeStr, out EpisodeType episodeType))
                {
                    Debug.LogWarning($"[DialogueDatabase] 잘못된 EpisodeType: {episodeTypeStr}");
                    episodeType = EpisodeType.Story;
                }

                // 에피소드 생성 및 저장
                DialogueEpisode episode = new DialogueEpisode(episodeID, episodeName, npcName, episodeType, description);
                episodes[episodeID] = episode;

                Log($"  에피소드 추가: {episodeID} - {episodeName}");
            }
        }

        /// <summary>
        /// 대화 테이블 로드 (DialogueTable.csv)
        /// </summary>
        private void LoadDialogueTable()
        {
            TextAsset csvFile = Resources.Load<TextAsset>(dialogueTablePath);
            if (csvFile == null)
            {
                Debug.LogWarning($"[DialogueDatabase] {dialogueTablePath}.csv 파일을 찾을 수 없습니다.");
                return;
            }

            string[] lines = csvFile.text.Split('\n');
            Log($"DialogueTable 로드: {lines.Length - 1}개 행");

            for (int i = 1; i < lines.Length; i++) // 헤더 스킵
            {
                string line = lines[i].Trim();
                if (string.IsNullOrEmpty(line)) continue;

                string[] fields = line.Split(',');
                if (fields.Length < 6)
                {
                    Debug.LogWarning($"[DialogueDatabase] DialogueTable 행 {i} 형식 오류: {line}");
                    continue;
                }

                string episodeID = fields[0].Trim();
                int nodeID = int.Parse(fields[1].Trim());
                string speakerName = fields[2].Trim();
                string dialogueText = fields[3].Trim();
                int nextNodeID = int.Parse(fields[4].Trim());
                bool hasChoices = fields[5].Trim().ToUpper() == "TRUE";

                // 대화 노드 생성
                DialogueNode node = new DialogueNode(episodeID, nodeID, speakerName, dialogueText, nextNodeID, hasChoices);

                // 해당 에피소드에 노드 추가
                if (episodes.ContainsKey(episodeID))
                {
                    episodes[episodeID].AddNode(node);
                }
                else
                {
                    Debug.LogWarning($"[DialogueDatabase] 에피소드 '{episodeID}'를 찾을 수 없습니다. (행 {i})");
                }
            }
        }

        /// <summary>
        /// 선택지 테이블 로드 (ChoiceTable.csv)
        /// </summary>
        private void LoadChoiceTable()
        {
            TextAsset csvFile = Resources.Load<TextAsset>(choiceTablePath);
            if (csvFile == null)
            {
                Debug.LogWarning($"[DialogueDatabase] {choiceTablePath}.csv 파일을 찾을 수 없습니다.");
                return;
            }

            string[] lines = csvFile.text.Split('\n');
            Log($"ChoiceTable 로드: {lines.Length - 1}개 행");

            for (int i = 1; i < lines.Length; i++) // 헤더 스킵
            {
                string line = lines[i].Trim();
                if (string.IsNullOrEmpty(line)) continue;

                string[] fields = line.Split(',');
                if (fields.Length < 7)
                {
                    Debug.LogWarning($"[DialogueDatabase] ChoiceTable 행 {i} 형식 오류: {line}");
                    continue;
                }

                string episodeID = fields[0].Trim();
                int nodeID = int.Parse(fields[1].Trim());
                int choiceID = int.Parse(fields[2].Trim());
                string choiceText = fields[3].Trim();
                int nextNodeID = int.Parse(fields[4].Trim());
                int requiredGold = int.Parse(fields[5].Trim());
                string rewardItem = fields[6].Trim();

                // 선택지 생성
                DialogueChoice choice = new DialogueChoice(episodeID, nodeID, choiceID, choiceText, nextNodeID, requiredGold, rewardItem);

                // 해당 에피소드에 선택지 추가
                if (episodes.ContainsKey(episodeID))
                {
                    episodes[episodeID].AddChoice(choice);
                }
                else
                {
                    Debug.LogWarning($"[DialogueDatabase] 에피소드 '{episodeID}'를 찾을 수 없습니다. (행 {i})");
                }
            }
        }

        /// <summary>
        /// 에피소드 가져오기
        /// </summary>
        public DialogueEpisode GetEpisode(string episodeID)
        {
            if (episodes.ContainsKey(episodeID))
            {
                return episodes[episodeID];
            }

            Debug.LogWarning($"[DialogueDatabase] 에피소드 '{episodeID}'를 찾을 수 없습니다.");
            return null;
        }

        /// <summary>
        /// 특정 NPC의 에피소드 목록 가져오기
        /// </summary>
        public List<DialogueEpisode> GetEpisodesByNPC(string npcName)
        {
            return episodes.Values.Where(ep => ep.npcName == npcName).ToList();
        }

        /// <summary>
        /// 특정 타입의 에피소드 목록 가져오기
        /// </summary>
        public List<DialogueEpisode> GetEpisodesByType(EpisodeType type)
        {
            return episodes.Values.Where(ep => ep.episodeType == type).ToList();
        }

        /// <summary>
        /// 모든 에피소드 ID 가져오기
        /// </summary>
        public List<string> GetAllEpisodeIDs()
        {
            return episodes.Keys.ToList();
        }

        private void Log(string message)
        {
            if (showDebugLog) Debug.Log($"[DialogueDatabase] {message}");
        }
    }
}
