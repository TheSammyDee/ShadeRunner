using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LoadSceneOnClick : MonoBehaviour {

    MusicManager music;

    private void Start() {
        music = FindObjectOfType<MusicManager>();
    }

    public void LoadByIndex(int level)
    {
        GameController.Instance.level = level;
        music.PlayLevelMusic();
        GameController.Instance.LoadLevel();
        
    }

    public void LoadLastScene() {
        GameController.Instance.LoadLevel();
        music.PlayLevelMusic();
    }

    public void RestartGame() {
        GameController.Instance.level = 1;
        GameController.Instance.LoadLevel();
        music.PlayLevelMusic();
    }
}
