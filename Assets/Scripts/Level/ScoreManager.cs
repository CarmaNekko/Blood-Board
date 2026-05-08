using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace BloodBoard.GameManagement
{

[System.Serializable]
public class ScoreEntry
{
    public string playerName;
    public int score;
    public int floor;

    public ScoreEntry(string name, int s, int f = 0)
    {
        playerName = name;
        score = s;
        floor = f;
    }

    public ScoreEntry(string name, int s) : this(name, s, 0) { }
}

[System.Serializable]
public class ScoreData
{
    public List<ScoreEntry> normalModeScores = new List<ScoreEntry>();
    public List<ScoreEntry> endlessModeScores = new List<ScoreEntry>();
}

public class ScoreManager : MonoBehaviour
{
    public static ScoreManager Instance { get; private set; }

    private const string ScoreDataKey = "GameScoreData";
    private const int MaxLeaderboardEntries = 6;

    private ScoreData _scoreData;
    private int _currentRunScore = 0;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            LoadScores();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void ResetCurrentScore()
    {
        _currentRunScore = 0;
        Debug.Log("Current run score reset.");
    }

    public void AddScoreToCurrent(int amount)
    {
        _currentRunScore += amount;
        Debug.Log($"Score added: {amount}. Current score: {_currentRunScore}");
    }

    public int GetCurrentScore()
    {
        return _currentRunScore;
    }

    public void SetCurrentScore(int score)
    {
        _currentRunScore = score;
        Debug.Log($"Current run score set to: {score}");
    }

    public void AddNormalModeScore(int score, string playerName)
    {
        _scoreData.normalModeScores.Add(new ScoreEntry(playerName, score));
        _scoreData.normalModeScores = _scoreData.normalModeScores.OrderByDescending(s => s.score).Take(MaxLeaderboardEntries).ToList();
        SaveScores();
        Debug.Log($"Normal Mode Score Added: {playerName} - {score}");
    }

    public void AddEndlessModeScore(int floor, int score, string playerName)
    {
        _scoreData.endlessModeScores.Add(new ScoreEntry(playerName, score, floor));
        _scoreData.endlessModeScores = _scoreData.endlessModeScores.OrderByDescending(s => s.score).ThenByDescending(s => s.floor).Take(MaxLeaderboardEntries).ToList();
        SaveScores();
        Debug.Log($"Endless Mode Score Added: {playerName} - Floor {floor}, Score {score}");
    }

    public List<ScoreEntry> GetNormalModeScores() => new List<ScoreEntry>(_scoreData.normalModeScores);
    public List<ScoreEntry> GetEndlessModeScores() => new List<ScoreEntry>(_scoreData.endlessModeScores);

    private void LoadScores()
    {
        _scoreData = PlayerPrefs.HasKey(ScoreDataKey) ? JsonUtility.FromJson<ScoreData>(PlayerPrefs.GetString(ScoreDataKey)) : new ScoreData();
        Debug.Log("Scores loaded from PlayerPrefs.");
    }

    private void SaveScores()
    {
        PlayerPrefs.SetString(ScoreDataKey, JsonUtility.ToJson(_scoreData));
        PlayerPrefs.Save();
        Debug.Log("Scores saved to PlayerPrefs.");
    }

    [ContextMenu("Clear All Scores")]
    public void ClearAllScores()
    {
        PlayerPrefs.DeleteKey(ScoreDataKey);
        _scoreData = new ScoreData();
        Debug.Log("All scores cleared.");
    }
}
}
