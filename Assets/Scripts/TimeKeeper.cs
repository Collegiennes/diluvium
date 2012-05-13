using System;
using UnityEngine;

class TimeKeeper : MonoBehaviour
{
    public static TimeKeeper Instance { get; private set; }

    public event Action Beat;

    public int BeatsPerMinute = 60;

    float lastBeat;

    void Awake()
    {
        Instance = this;

        // DEBUG -- for local mode
        //audio.Play();
    }

    void OnPlayerConnected()
    {
        if (audio.isPlaying) audio.Stop();
        // TODO : countdown timer that acts as a preloader?
        audio.Play();
    }
    void OnConnectedToServer()
    {
        if (audio.isPlaying) audio.Stop();
        // TODO : countdown timer that acts as a preloader?
        audio.Play();
    }

    void OnPlayerDisconnected()
    {
        if (audio.isPlaying) audio.Stop();
    }
    void OnDisconnectedFromServer()
    {
        if (audio.isPlaying) audio.Stop();
    }

    void Update()
    {
        // No need to keep time for the client... right?
        if (!audio.isPlaying || !Network.isServer) return;

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

