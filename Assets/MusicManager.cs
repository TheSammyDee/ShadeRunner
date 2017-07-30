using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MusicManager : MonoBehaviour {

    public AudioClip menuMusic;
    public AudioClip levelMusic;
    public AudioClip gameOverMusic;
    public AudioClip winMusic;

    private AudioSource music;

    private void Awake() {
        DontDestroyOnLoad(gameObject);
    }

    // Use this for initialization
    void Start () {
        music = GetComponent<AudioSource>();
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    public void PlayMenuMusic() {
        music.clip = menuMusic;
        music.Play();
    }

    public void PlayLevelMusic() {
        music.clip = levelMusic;
        music.Play();
    }

    public void PlayGameOverMusic() {
        music.clip = gameOverMusic;
        music.Play();
    }

    public void PlayWinMusic() {
        music.clip = winMusic;
        music.Play();
    }
}
