using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LoginUI : MonoBehaviour
{
    [Header("패널")]
    [SerializeField] private GameObject loginPanel;
    [SerializeField] private GameObject buttonContainer;

    [Header("프로필 버튼")]
    [SerializeField] private Button profileButton;

    [Header("입력")]
    [SerializeField] private TMP_InputField emailInput;
    [SerializeField] private TMP_InputField passwordInput;

    [Header("버튼")]
    [SerializeField] private Button loginButton;
    [SerializeField] private Button signupButton;
    [SerializeField] private Button anonymousButton;

    [Header("에러")]
    [SerializeField] private TextMeshProUGUI errorText;

    [Header("참조")]
    [SerializeField] private ProfileUI profileUI;

    private async UniTaskVoid Start()
    {
        await UniTask.WaitUntil(() => AuthManager.Instance != null && AuthManager.Instance.IsInitialized);

        AuthManager.Instance.LoginStateChanged += OnLoginStateChanged;

        profileButton.onClick.AddListener(OnProfileButtonClicked);
        loginButton.onClick.AddListener(() => OnLoginClicked().Forget());
        signupButton.onClick.AddListener(() => OnSignupClicked().Forget());
        anonymousButton.onClick.AddListener(() => OnAnonymousClicked().Forget());

        SetPanelState(AuthManager.Instance.IsLoggedIn);
    }

    private void OnDestroy()
    {
        if (AuthManager.Instance != null)
            AuthManager.Instance.LoginStateChanged -= OnLoginStateChanged;
    }

    private void OnLoginStateChanged(bool isLoggedIn)
    {
        SetPanelState(isLoggedIn);
    }

    private void SetPanelState(bool isLoggedIn)
    {
        loginPanel.SetActive(!isLoggedIn);
        buttonContainer.SetActive(isLoggedIn);
        profileButton.gameObject.SetActive(isLoggedIn);
        errorText.text = string.Empty;
    }

    private void OnProfileButtonClicked()
    {
        if (AuthManager.Instance.IsLoggedIn)
            profileUI.OpenProfilePanel().Forget();
        else
            loginPanel.SetActive(true);
    }

    private async UniTaskVoid OnLoginClicked()
    {
        string email = emailInput.text.Trim();
        string password = passwordInput.text;

        if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
        {
            ShowError("이메일과 비밀번호를 입력하세요.");
            return;
        }

        SetButtonsInteractable(false);
        var (success, error) = await AuthManager.Instance.SignInUserWithEmailAsync(email, password);
        if (!success) ShowError(error);
        SetButtonsInteractable(true);
    }

    private async UniTaskVoid OnSignupClicked()
    {
        string email = emailInput.text.Trim();
        string password = passwordInput.text;

        if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
        {
            ShowError("이메일과 비밀번호를 입력하세요.");
            return;
        }

        SetButtonsInteractable(false);
        var (success, error) = await AuthManager.Instance.CreateUserWithEmailAsync(email, password);
        if (!success) ShowError(error);
        SetButtonsInteractable(true);
    }

    private async UniTaskVoid OnAnonymousClicked()
    {
        SetButtonsInteractable(false);
        var (success, error) = await AuthManager.Instance.SignInAnonymouslyAsync();
        if (!success) ShowError(error);
        SetButtonsInteractable(true);
    }

    private void ShowError(string message)
    {
        errorText.text = message;
        errorText.color = Color.red;
    }

    private void SetButtonsInteractable(bool interactable)
    {
        loginButton.interactable = interactable;
        signupButton.interactable = interactable;
        anonymousButton.interactable = interactable;
    }
}
