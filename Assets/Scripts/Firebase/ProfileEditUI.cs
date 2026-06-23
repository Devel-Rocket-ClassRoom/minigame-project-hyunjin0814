using System;
using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ProfileEditUI : MonoBehaviour
{
    [Header("패널")]
    [SerializeField] private GameObject profileEditPanel;
    [SerializeField] private GameObject createProfilePanel;
    [SerializeField] private GameObject editProfilePanel;

    [Header("닉네임 생성")]
    [SerializeField] private TMP_InputField createNicknameInput;
    [SerializeField] private Button createButton;
    [SerializeField] private TextMeshProUGUI createErrorText;

    [Header("닉네임 수정")]
    [SerializeField] private TextMeshProUGUI currentNicknameText;
    [SerializeField] private TMP_InputField editNicknameInput;
    [SerializeField] private Button updateButton;
    [SerializeField] private Button closeButton;
    [SerializeField] private TextMeshProUGUI editErrorText;

    [Header("참조")]
    [SerializeField] private ProfileUI profileUI;

    private void Start()
    {
        createButton.onClick.AddListener(() => OnCreateClicked().Forget());
        updateButton.onClick.AddListener(() => OnUpdateClicked().Forget());
        closeButton.onClick.AddListener(OnCloseClicked);

        profileEditPanel.SetActive(false);
    }

    public async UniTaskVoid OpenProfileEditPanelAsync()
    {
        profileEditPanel.SetActive(true);

        var (profile, _) = await ProfileManager.Instance.LoadProfileAsync();
        if (profile != null)
            ShowEditPanel(profile);
        else
            ShowCreatePanel();
    }

    private void ShowCreatePanel()
    {
        createProfilePanel.SetActive(true);
        editProfilePanel.SetActive(false);
        createNicknameInput.text = string.Empty;
        createErrorText.text = string.Empty;
    }

    private void ShowEditPanel(UserProfile profile)
    {
        createProfilePanel.SetActive(false);
        editProfilePanel.SetActive(true);
        currentNicknameText.text = $"현재 닉네임: {profile.nickname}";
        editNicknameInput.text = profile.nickname;
        editErrorText.text = string.Empty;
    }

    private async UniTaskVoid OnCreateClicked()
    {
        string nickname = createNicknameInput.text.Trim();
        if (string.IsNullOrEmpty(nickname))
        {
            ShowError(createErrorText, "닉네임을 입력하세요.");
            return;
        }

        createButton.interactable = false;
        var (success, error) = await ProfileManager.Instance.SaveProfileAsync(nickname);
        if (success)
        {
            createErrorText.text = "프로필 생성 완료!";
            createErrorText.color = Color.green;
            await UniTask.Delay(TimeSpan.FromSeconds(1), cancellationToken: this.GetCancellationTokenOnDestroy());
            profileEditPanel.SetActive(false);
            profileUI.UpdateProfileUIAsync().Forget();
        }
        else
        {
            ShowError(createErrorText, error);
        }
        createButton.interactable = true;
    }

    private async UniTaskVoid OnUpdateClicked()
    {
        string nickname = editNicknameInput.text.Trim();
        if (string.IsNullOrEmpty(nickname))
        {
            ShowError(editErrorText, "닉네임을 입력하세요.");
            return;
        }

        updateButton.interactable = false;
        var (success, error) = await ProfileManager.Instance.UpdateNicknameAsync(nickname);
        if (success)
        {
            editErrorText.text = "수정 완료!";
            editErrorText.color = Color.green;
            currentNicknameText.text = $"현재 닉네임: {nickname}";
            await UniTask.Delay(TimeSpan.FromSeconds(1), cancellationToken: this.GetCancellationTokenOnDestroy());
            profileEditPanel.SetActive(false);
            profileUI.UpdateProfileUIAsync().Forget();
        }
        else
        {
            ShowError(editErrorText, error);
        }
        updateButton.interactable = true;
    }

    private void OnCloseClicked()
    {
        profileEditPanel.SetActive(false);
    }

    private void ShowError(TextMeshProUGUI target, string message)
    {
        target.text = message;
        target.color = Color.red;
    }
}
