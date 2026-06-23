using System;

// LeaderboardManager.cs에서 저장하는 방식을 LeaderboardEntry를 만들어서 넘기는 방식으로 바꿈
// GameManager와 같이 게임 기록을 관리하는 곳에서 저장할 때 현재 클래스의 객체를 프로젝트마다 다르게 만들어서 저장하면됨.
// SortValue에는 정렬 기준이 되는 필드를 사용하면됨.
[Serializable]
public class LeaderboardEntry
{
    public string userId;
    public string nickname;
    public float clearTime;
    public int retryCount;
    public long timestamp;

    public float SortValue => clearTime;

    public LeaderboardEntry()
    {
    }

    public LeaderboardEntry(string userId, string nickname, float clearTime, int retryCount, long timestamp)
    {
        this.userId = userId;
        this.nickname = nickname;
        this.clearTime = clearTime;
        this.retryCount = retryCount;
        this.timestamp = timestamp;
    }

    public string ToJson()
    {
        return UnityEngine.JsonUtility.ToJson(this);
    }

    public static LeaderboardEntry FromJson(string json)
    {
        return UnityEngine.JsonUtility.FromJson<LeaderboardEntry>(json);
    }
}
