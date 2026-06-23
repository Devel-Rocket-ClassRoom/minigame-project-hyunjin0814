using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LeaderboardUI : MonoBehaviour
{
    [Header("패널")]
    [SerializeField] private GameObject leaderboardPanel;
    [SerializeField] private Button closeButton;

    [Header("목록")]
    [SerializeField] private Transform contentParent;   // ScrollView Content
    [SerializeField] private GameObject entryPrefab;

    private void Start()
    {
        closeButton.onClick.AddListener(CloseLeaderboardPanel);
        leaderboardPanel.SetActive(false);
    }

    public void OpenLeaderboardPanel()
    {
        OpenLeaderboardPanelAsync().Forget();
    }

    private async UniTaskVoid OpenLeaderboardPanelAsync()
    {
        leaderboardPanel.SetActive(true);
        await LoadAndDisplayAsync();
    }

    private void CloseLeaderboardPanel()
    {
        leaderboardPanel.SetActive(false);
    }

    private async UniTask LoadAndDisplayAsync()
    {
        List<LeaderboardEntry> entries = await LeaderboardManager.Instance.LoadLeaderboardAsync();
        DisplayLeaderboard(entries);
    }

    private void DisplayLeaderboard(List<LeaderboardEntry> leaderboard)
    {
        foreach (Transform child in contentParent)
            Destroy(child.gameObject);

        if (leaderboard == null || leaderboard.Count == 0)
        {
            Debug.Log("[LeaderboardUI] 표시할 기록 없음");
            return;
        }

        int rank = 1;
        foreach (LeaderboardEntry entry in leaderboard)
        {
            GameObject item = Instantiate(entryPrefab, contentParent);
            TextMeshProUGUI[] texts = item.GetComponentsInChildren<TextMeshProUGUI>();

            if (texts.Length >= 4)
            {
                texts[0].text = $"{rank}";
                texts[1].text = entry.nickname;
                texts[2].text = FormatTime(entry.clearTime);
                texts[3].text = $"리스폰 {entry.retryCount}회";
            }

            rank++;
        }

        Debug.Log($"[LeaderboardUI] 리더보드 표시 완료: {leaderboard.Count}명");
    }

    private string FormatTime(float seconds)
    {
        int m = (int)seconds / 60;
        int s = (int)seconds % 60;
        return $"{m:00}분:{s:00}초";
    }
}
