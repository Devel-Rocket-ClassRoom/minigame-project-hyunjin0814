using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class StageClearManager : MonoBehaviour
{
    public static StageClearManager Instance { get; private set; }

    [SerializeField] private PlayerInputHandler playerInput;
    [SerializeField] private Image fadeImage;
    [SerializeField] private GameObject stageClearText;
    [SerializeField] private float fadeDuration = 1f;
    [SerializeField] private float displayDuration = 3f;
    [SerializeField] private string titleSceneName = "Title";

    [SerializeField] private Button mainMenuButton;
    [SerializeField] private Button leaderboardButton;
    [SerializeField] private LeaderboardUI leaderboardUI;

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void Start()
    {
        mainMenuButton.gameObject.SetActive(false);
        leaderboardButton.gameObject.SetActive(false);
        mainMenuButton.onClick.AddListener(OnMainMenuClicked);
        leaderboardButton.onClick.AddListener(OnLeaderboardClicked);
    }

    public void TriggerClear() => ClearSequenceAsync().Forget();

    private async UniTaskVoid ClearSequenceAsync()
    {
        LeaderboardEntry existing = await LeaderboardManager.Instance.LoadMyEntryAsync();
        float currentTime = GameState.Instance.playTime;

        if (existing == null || currentTime < existing.clearTime)
        {
            string userId = AuthManager.Instance.UserId;
            int respawnCount = GameState.Instance.respawnCount;
            string nickname = ProfileManager.Instance.CachedProfile?.nickname ?? $"익명{Random.Range(0, 100)}";;

            LeaderboardEntry saveInfo = new LeaderboardEntry(userId, nickname, currentTime, respawnCount, TimeUtil.NowUnixMillis());
            await LeaderboardManager.Instance.SaveToLeaderboardAsync(saveInfo);
        }

        if (playerInput != null)
            playerInput.enabled = false;

        // 페이드 처리
        Color c = fadeImage.color;
        c.a = 0f;
        fadeImage.color = c;
        fadeImage.gameObject.SetActive(true);

        float elapsed = 0f;
        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            c.a = Mathf.Clamp01(elapsed / fadeDuration);
            fadeImage.color = c;
            await UniTask.Yield();
        }

        if (stageClearText != null)
            stageClearText.SetActive(true);

        await UniTask.Delay(System.TimeSpan.FromSeconds(displayDuration));

        mainMenuButton.gameObject.SetActive(true);
        leaderboardButton.gameObject.SetActive(true);
    }

    private void OnMainMenuClicked()
    {
        if (SceneTransitionManager.Instance != null)
            SceneTransitionManager.Instance.TransitionTo(titleSceneName);
        else
            SceneManager.LoadScene(titleSceneName);
    }

    private void OnLeaderboardClicked()
    {
        leaderboardUI.OpenLeaderboardPanel();
    }
}