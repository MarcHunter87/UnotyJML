using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EndAudioManager : MonoBehaviour
{
    public static EndAudioManager Instance { get; private set; }

    public AudioSource musicPlayer;

    public AudioClip[] availableMusicClips;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        if (ENDdata.Instance.playerWon)
        {
            musicPlayer.clip = availableMusicClips[0];
        }
        else
        {
            musicPlayer.clip = availableMusicClips[1];
        }

        musicPlayer.Play();
    }

    public void StopMusic()
    {
        musicPlayer.Stop();
    }
} 