using Cysharp.Threading.Tasks;
using Firebase;
using Firebase.Auth;
using Firebase.Database;
using UnityEngine;

public class FirebaseInitializer : MonoBehaviour
{
    private static FirebaseInitializer instance;
    public static FirebaseInitializer Instance => instance;

    public enum InitState
    {
        Pending,
        Ready,
        Failed,
    }

    public InitState State { get; private set; } = InitState.Pending;
    public bool IsReady => State == InitState.Ready;
    public string LastError { get; private set; }

    public FirebaseApp App { get; private set; }
    public FirebaseDatabase Database { get; private set; }
    public FirebaseAuth Auth { get; private set; }
    public FirebaseConfig Config { get; private set; }

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        InitializeFirebaseAsync().Forget();
    }

    private async UniTaskVoid InitializeFirebaseAsync()
    {
        Debug.Log("[Firebase] 초기화 시작...");

        try
        {
            Config = Resources.Load<FirebaseConfig>("FirebaseConfig");
            
            DependencyStatus status = await FirebaseApp.CheckAndFixDependenciesAsync().AsUniTask();

            if (status != DependencyStatus.Available)
            {
                Fail($"의존성 오류: {status}");
                return;
            }

            App = FirebaseApp.DefaultInstance;
            Database = GetDatabase(App);
            Auth = FirebaseAuth.GetAuth(App);

            State = InitState.Ready;
            Debug.Log($"[Firebase] 초기화 성공 (app={App.Name})");
        }
        catch (System.Exception ex)
        {
            Fail(ex.Message);
        }
    }

    private FirebaseDatabase GetDatabase(FirebaseApp app)
    {
        if (Config != null && !string.IsNullOrEmpty(Config.databaseUrl))
        {
            return FirebaseDatabase.GetInstance(app, Config.databaseUrl);
        }
        return FirebaseDatabase.GetInstance(app);
    }

    private void Fail(string error)
    {
        LastError = error;
        State = InitState.Failed;
        Debug.LogError($"[Firebase] 초기화 실패: {error}");
    }

    public async UniTask<bool> WaitForInitializationAsync()
    {
        await UniTask.WaitUntil(() => State != InitState.Pending);
        return State == InitState.Ready;
    }

    private void OnDestroy()
    {
        if (instance == this)
        {
            instance = null;
        }
    }
}
