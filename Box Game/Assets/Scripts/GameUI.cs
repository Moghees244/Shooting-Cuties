using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.PlayerLoop;

public class GameUI : MonoBehaviour
{
    public Text scoreUI;
    public RectTransform healthBar;
    public Image fadePlane;
    public GameObject gameOverUI;
    public RectTransform newWaveBanner;
    public Text newWaveTitle;
    public Text newWaveEnemyCount;
    Spawner spawner;
    PlayerController player;

    void Start()
    {
        player = FindObjectOfType<PlayerController>();
        player.OnDeath += onGameOver;
    }

    void Update()
    {
        scoreUI.text = ScoreKeeper.score.ToString("D5");

        if (player != null)
        {
            float healthPercent = player.health / player.startingHealth;
            healthBar.localScale = new Vector3(healthPercent*2, 1, 1);
        }
        else
            healthBar.localScale=Vector3.zero;
    }

    private void Awake()
    {
        spawner = FindObjectOfType<Spawner>();
        spawner.onNewWave += OnNewWave;
    }

    void OnNewWave(int waveNumber)
    {
        string[] waveNums = { "One", "Two", "Three", "Four", "Five" };

        newWaveTitle.text = "- Wave " + waveNums[waveNumber-1] + " -";
        newWaveEnemyCount.text = "Enemies : " + spawner.waves[waveNumber - 1].enemyCount;

        StartCoroutine(animateNewWaveBanner());
    }

    IEnumerator animateNewWaveBanner()
    {
        float animatePercent = 0;
        float speed = 3.0f;
        float delay = 1.5f;
        int dir = 1;
        float endDelay = Time.time + 1 / speed + delay;

        while (animatePercent >= 0)
        {
            animatePercent += Time.deltaTime * speed * dir;

            if (animatePercent >= 1)
            {
                animatePercent = 1;
                if (Time.time > endDelay)
                {
                    dir = -1;
                }
            }

            newWaveBanner.anchoredPosition = Vector2.up * Mathf.Lerp(-170, 75, animatePercent);
            yield return null;
        }
    }

    void onGameOver()
    {
        StartCoroutine(Fade(Color.clear, Color.black, 1));
        gameOverUI.SetActive(true);
    }

    IEnumerator Fade(Color from, Color to, float time)
    {
        float speed = 1 / time;
        float percent = 0;

        while (percent < 1)
        {
            percent += Time.deltaTime * speed;
            fadePlane.color = Color.Lerp(from, to, percent);
            yield return null;
        }
    }

    public void StartNewGame()
    {
        ScoreKeeper.score = 0;
        SceneManager.LoadScene("PlayMode");
    }

    public void goToMainMenu()
    {
        SceneManager.LoadScene("Menu");
    }
}
