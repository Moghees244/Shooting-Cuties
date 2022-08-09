using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScoreKeeper : MonoBehaviour
{
    public static int score;
    float lastEnemyKillTime;
    int streakCount;
    float streakExpiryTime = 1f;

    private void Start()
    {
        Enemy.OnDeathStatic += onEnemyKilled;
        FindObjectOfType<PlayerController>().OnDeath+=OnPlayerDeath;
    }

    public void onEnemyKilled()
    {
        if (Time.time < lastEnemyKillTime + streakExpiryTime)
        {
            streakCount++;
        }
        else
        {
            streakCount = 0;
        }
        lastEnemyKillTime = Time.time;
        score += 4 + (int)Mathf.Pow(2, streakCount);
    }

    void OnPlayerDeath()
    {
        Enemy.OnDeathStatic-=onEnemyKilled;
    }
}
