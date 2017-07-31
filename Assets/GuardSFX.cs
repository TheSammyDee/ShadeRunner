using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GuardSFX : MonoBehaviour {

    public AudioClip[] audioClips;
    AudioSource audio;
    static int counter;

	// Use this for initialization
	void Start () {
        audio = GetComponent<AudioSource>();
        counter = 0;
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    public void PlayAlert() {
        if (counter < audioClips.Length) {
            audio.PlayOneShot(audioClips[counter]);
            counter++;
        } else {
            audio.PlayOneShot(audioClips[Random.Range(0, audioClips.Length)]);
        }
    } 
}
