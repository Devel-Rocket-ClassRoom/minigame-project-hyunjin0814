using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Firebase.Database;
using UnityEngine;

public class LeaderboardManager : MonoBehaviour
{
    private static LeaderboardManager instance;
    public static LeaderboardManager Instance => instance;

    private DatabaseReference databaseRef;
    private DatabaseReference leaderboardRef;
    private string orderBy;
    private Query listenerQuery;

    private bool isListenerActive;
    public event Action<List<LeaderboardEntry>> OnLeaderboardUpdated;

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
    }

    private async UniTaskVoid Start()
    {
        if (!await FirebaseInitializer.Instance.WaitForInitializationAsync())
        {
            Debug.LogError("[Leaderboard] Firebase 초기화 실패");
            return;
        }
        databaseRef = FirebaseInitializer.Instance.Database.RootReference;
        leaderboardRef = databaseRef.Child(FirebaseInitializer.Instance.Config.leaderboardPath);
        orderBy = FirebaseInitializer.Instance.Config.orderBy;
    }

    private void OnDestroy()
    {
        StopRealtimeListener();
        if (instance == this)
            instance = null;
    }

    public async UniTask<(bool success, string error)> SaveToLeaderboardAsync(LeaderboardEntry entry)
    {
        if (!AuthManager.Instance.IsLoggedIn)
        {
            return (false, "로그인이 필요합니다.");
        }

        if (leaderboardRef == null)
        {
            return (false, "leaderboardRef 널");
        }

        string userId = AuthManager.Instance.UserId;
        // string nickname = ProfileManager.Instance.CachedProfile?.nickname ?? "익명";

        try
        {
            Debug.Log("[Leaderboard] 저장 시도");

            await leaderboardRef.Child(userId).SetRawJsonValueAsync(entry.ToJson());
            Debug.Log("[Leaderboard] 저장 성공");
            return (true, null);
        }
        catch (Exception ex)
        {
            Debug.LogError($"[Leaderboard] 저장 실패: {ex.Message}");
            return (false, ex.Message);
        }
    }

    public async UniTask<List<LeaderboardEntry>> LoadLeaderboardAsync(int limit = 10)
    {
        if (leaderboardRef == null)
        {
            return new List<LeaderboardEntry>();
        }

        try
        {
            Debug.Log("[Leaderboard] 로드 시도");

            Query query = leaderboardRef.OrderByChild(orderBy).LimitToFirst(limit);
            DataSnapshot snapshot = await query.GetValueAsync();
            List<LeaderboardEntry> leaderboard = ParseEntries(snapshot);

            Debug.Log("[Leaderboard] 로드 성공");
            return leaderboard;
        }
        catch (Exception ex)
        {
            Debug.LogError($"[Leaderboard] 로드 실패: {ex.Message}");
            return new List<LeaderboardEntry>();
        }
    }

    public async UniTask<LeaderboardEntry> LoadMyEntryAsync()
    {
        if (leaderboardRef == null) return null; 

        string userId = AuthManager.Instance.UserId;
        DataSnapshot snapshot = await leaderboardRef.Child(userId).GetValueAsync();
        
        if (!snapshot.Exists) return null;
        return LeaderboardEntry.FromJson(snapshot.GetRawJsonValue());
    }

    public List<LeaderboardEntry> ParseEntries(DataSnapshot snapshot)
    {
        List<LeaderboardEntry> list = new List<LeaderboardEntry>();

        if (snapshot.Exists)
        {
            foreach (DataSnapshot child in snapshot.Children)
            {
                list.Add(LeaderboardEntry.FromJson(child.GetRawJsonValue()));
            }
        }

        list.Sort((a, b) => a.SortValue.CompareTo(b.SortValue));
        return list;
    }

    public void StartRealtimeListener(int limit = 10)
    {
        if (isListenerActive || leaderboardRef == null)
        {
            return;    
        }

        Debug.Log("[Leaderboard] 실시간 리스너 시작");
        listenerQuery = leaderboardRef.OrderByChild(orderBy).LimitToFirst(limit);
        listenerQuery.ValueChanged += OnValueChanged;
        isListenerActive = true;
    }

    public void StopRealtimeListener()
    {
        if (isListenerActive && listenerQuery != null)
        {
            Debug.Log("[Leaderboard] 실시간 리스너 중지");
            listenerQuery.ValueChanged -= OnValueChanged;
            listenerQuery = null;
            isListenerActive = false;
        }
    }

    private void OnValueChanged(object sender, ValueChangedEventArgs args)
    {
        if (args.DatabaseError != null)
        {
            Debug.LogError($"[Leaderboard] 리스너 오류: {args.DatabaseError.Message}");
            return;
        }

        List<LeaderboardEntry> leaderboard = ParseEntries(args.Snapshot);
        DispatchUpdateAsync(leaderboard).Forget();
    }

    private async UniTaskVoid DispatchUpdateAsync(List<LeaderboardEntry> leaderboard)
    {
        await UniTask.SwitchToMainThread();
        OnLeaderboardUpdated?.Invoke(leaderboard);
    }
}
