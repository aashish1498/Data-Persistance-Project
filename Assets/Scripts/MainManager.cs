using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainManager : MonoBehaviour
{
    public Brick BrickPrefab;
    public int LineCount = 6;
    public Rigidbody Ball;

    public Text ScoreText;
    public Text HighScoreText;
    public GameObject GameOverText;
    public GameObject NameText;

    private bool m_Started = false;
    
    private bool m_GameOver = false;

    private Score currentScore;

    private Score loadedScore;

    private string saveFilePath;

    private bool gameLoaded;

    private void Awake()
    {
        saveFilePath = Application.persistentDataPath + "/saveFile.json";
        currentScore = new Score
        {
            playerName = "test",
            score = 0
        };
        loadedScore = new Score
        {
            playerName = "CPU",
            score = 0
        };
    }

    void Start()
    {
        LoadScores();
    }

    private void LoadGame()
    {
        const float step = 0.6f;
        int perLine = Mathf.FloorToInt(4.0f / step);

        int[] pointCountArray = new[] { 1, 1, 2, 2, 5, 5 };
        for (int i = 0; i < LineCount; ++i)
        {
            for (int x = 0; x < perLine; ++x)
            {
                Vector3 position = new Vector3(-1.5f + step * x, 2.5f + i * 0.3f, 0);
                var brick = Instantiate(BrickPrefab, position, Quaternion.identity);
                brick.PointValue = pointCountArray[i];
                brick.onDestroyed.AddListener(AddPoint);
            }
        }
        gameLoaded = true;
    }

    private void Update()
    {
        if (!gameLoaded)
        {
            return;
        }
        if (!m_Started)
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                m_Started = true;
                float randomDirection = UnityEngine.Random.Range(-1.0f, 1.0f);
                Vector3 forceDir = new Vector3(randomDirection, 1, 0);
                forceDir.Normalize();

                Ball.transform.SetParent(null);
                Ball.AddForce(forceDir * 2.0f, ForceMode.VelocityChange);
            }
        }
        else if (m_GameOver)
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
            }
        }
    }

    void AddPoint(int point)
    {
        currentScore.score += point;
        ScoreText.text = $"Score : {currentScore.score}";
    }

    public void GameOver()
    {
        m_GameOver = true;
        GameOverText.SetActive(true);
        if (currentScore.score > loadedScore?.score)
        {
            SaveScore();
            loadedScore = currentScore;
            UpdateHighScore();
        }
    }

    private void SaveScore()
    {
        string scoreString = JsonUtility.ToJson(currentScore);
        File.WriteAllText(saveFilePath, scoreString);
    }

    private void LoadScores()
    {
        if (!File.Exists(saveFilePath))
        {
            return;
        }
        string scoreJson = File.ReadAllText(saveFilePath);
        loadedScore = JsonUtility.FromJson<Score>(scoreJson);
        UpdateHighScore();
    }

    private void UpdateHighScore()
    {
        if (loadedScore != null)
        {
            HighScoreText.text = $"Best Score: {loadedScore.score} | Name: {loadedScore.playerName}";
        }
    }

    public void PlayerNameEntered(string name)
    {
        currentScore.playerName = name;
        NameText.SetActive(false);
        LoadGame();
    }

    [Serializable]
    private class Score
    {
        public string playerName;
        public int score;
    }
}
