using UnityEngine;

[CreateAssetMenu(fileName = "FirebaseConfig", menuName = "Firebase/Firebase Config")]
public class FirebaseConfig : ScriptableObject
{
    [Header("Database")]
    [Tooltip("Realtime Database URL (예: https://your-project-default-rtdb.firebaseio.com)")]
    public string databaseUrl;

    [Header("DB 경로")]
    public string usersPath = "users";
    public string leaderboardPath = "leaderboard";

    // Query의 Child로 넘길 string 인자를 여기서 선언해서 사용함. 여기의 값만 LeaderboardEntry와 연동되도록 바꾸면됨.
    [Header("정렬 순서")]
    public string orderBy = "clearTime";

    public bool IsValid => !string.IsNullOrEmpty(databaseUrl);
}
