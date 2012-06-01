using System;
using UnityEngine;

class TimeKeeper : MonoBehaviour
{
    public static TimeKeeper Instance { get; private set; }

    public event Action Beat;

    public int BeatsPerMinute = 60;

    public AudioClip LoginMusic, GameplayMusic, WinMusic, ChipMusic;
    public bool IsChip;

    float lastBeat;

    void Awake()
    {
        Instance = this;
    }

    void Update()
    {
        if (GameFlow.State == GameState.Won && (audio.clip != WinMusic || !audio.isPlaying))
        {
            audio.Stop();
            audio.clip = WinMusic;
            audio.Play();
        }

        if ((GameFlow.State < GameState.Gameplay || GameFlow.State == GameState.Lost) && (audio.clip != LoginMusic || !audio.isPlaying))
        {
            audio.Stop();
            audio.clip = LoginMusic;
            audio.Play();
        }

        var gameMusic = IsChip ? ChipMusic : GameplayMusic;

        if (GameFlow.State == GameState.Gameplay && (audio.clip != gameMusic || !audio.isPlaying))
        {
            audio.Stop();
            audio.clip = gameMusic;
            audio.Play();
        }

        // No need to keep time for the client... 
        if (!audio.isPlaying || GameFlow.State != GameState.Gameplay || !Network.isServer) return;

        int samples = audio.timeSamples;

        float minutes = samples / 44100f / 60;
        float bar = minutes * BeatsPerMinute / 4;// - Totem.TransitionDuration;

        // keep only fractional part
        bar = bar - (int)bar;

        if (lastBeat > 0.75 && bar < 0.25)
            if (Beat != null) Beat();
        if (lastBeat < 0.25 && bar > 0.25)
            if (Beat != null) Beat();
        if (lastBeat < 0.5 && bar > 0.5)
            if (Beat != null) Beat();
        if (lastBeat < 0.75 && bar > 0.75)
            if (Beat != null) Beat();

        lastBeat = bar;
    }

    //void OnGUI()
    //{
    //    GUILayout.Label("");
    //    GUILayout.Label("Beat : " + lastBeat);
    //}
}

