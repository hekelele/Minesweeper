using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MineGameManager : MonoBehaviour
{
    public enum GameState
    {
        Running,
        GameOver,
        GameWin,
    }

    [SerializeField] private Camera mainCam;
    [SerializeField] private MineTilemap tilemap;
    [SerializeField] private AudioClip[] seClips;
    [SerializeField] private AudioClip winClip;
    [SerializeField] private AudioClip explodeClip;
    [SerializeField] private Text timeText;
    [SerializeField] private Text bombText;
    [SerializeField] private int maxTime = 120;
    private GameState state;
    private float timerStartTime;
    private int lastTime;

    // Start is called before the first frame update
    void Start()
    {
        ResetGame();
    }

    // Update is called once per frame
    void Update()
    {
        if (state != GameState.Running)
        {
            return;
        }

        UpdateTimer();

        if (Input.GetMouseButtonDown(0))
        {
            Vector2 mousePoint = mainCam.ScreenToWorldPoint(Input.mousePosition);
            var isTile = tilemap.FlipTile(mousePoint);
            if (isTile)
            {
                AudioSource.PlayClipAtPoint(seClips[0], mousePoint);
            }
        }

        if (Input.GetMouseButtonDown(1))
        {
            Vector2 mousePoint = mainCam.ScreenToWorldPoint(Input.mousePosition);
            var isTile = tilemap.MarkTile(mousePoint);
            if (isTile)
            {
                AudioSource.PlayClipAtPoint(seClips[1], mousePoint);
            }
        }
    }

    private void UpdateTimer()
    {
        int timePast = (int)(Time.time - timerStartTime);
        if (timePast != lastTime)
        {
            int t = maxTime - timePast;
            int min = t / 60;
            int sec = t % 60;
            timeText.text = $"{min:D2}:{sec:D2}";
        }

        if(timePast>= maxTime)
        {
            GameOver();
        }
    }

    public void GameOver()
    {
        state = GameState.GameOver;
        AudioSource.PlayClipAtPoint(explodeClip, Vector3.zero);
    }

    public void GameWin()
    {
        state = GameState.GameOver;
        AudioSource.PlayClipAtPoint(winClip, Vector3.zero);
    }

    public void ResetGame()
    {
        state = GameState.Running;
        tilemap.ResetGame();
        timerStartTime = Time.time;
        lastTime = -1;
    }

    public void SetBombRemainCount(int count)
    {
        bombText.text = count.ToString();
    }
}
