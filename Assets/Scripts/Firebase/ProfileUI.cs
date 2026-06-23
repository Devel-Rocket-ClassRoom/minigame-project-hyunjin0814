using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ProfileUI : MonoBehaviour
{
    [Header("패널")]
    [SerializeField] private GameObject profilePanel;

    [Header("프로필 정보")]
    [SerializeField] private TextMeshProUGUI nicknameText;
    [SerializeField] private TextMeshProUGUI userIdText;

    [Header("버튼")]
    [SerializeField] private Button editProfileButton;
    [SerializeField] private Button logoutButton;
    [SerializeField] private Button closeButton;

    [Header("참조")]
    [SerializeField] private ProfileEditUI profileEditUI;

    private void Start()
    {
        editProfileButton.onClick.AddListener(OnEditProfileClicked);
        logoutButton.onClick.AddListener(OnLogoutClicked);
        closeButton.onClick.AddListener(OnCloseClicked);

        profilePanel.SetActive(false);
    }

    public async UniTaskVoid OpenProfilePanel()
    {
        await UpdateProfileUIAsync();
        profilePanel.SetActive(true);
    }

    public async UniTask UpdateProfileUIAsync()
    {
        if (!AuthManager.Instance.IsLoggedIn)
            return;

        userIdText.text = $"User ID: {AuthManager.Instance.UserId}";

        var (profile, _) = await ProfileManager.Instance.LoadProfileAsync();
        nicknameText.text = profile != null ? $"닉네임: {profile.nickname}" : "닉네임: (미설정)";
    }

    private void OnEditProfileClicked()
    {
        profileEditUI.OpenProfileEditPanelAsync().Forget();
    }

    private void OnCloseClicked()
    {
        profilePanel.SetActive(false);
    }

    private void OnLogoutClicked()
    {
        profilePanel.SetActive(false);
        AuthManager.Instance.SignOut();
    }
}
