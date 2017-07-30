using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LoadSceneOnClick : MonoBehaviour {
    public void LoadByIndex(int level)
    {
        GameController.Instance.level = level;
        GameController.Instance.LoadLevel();

    }

    public void LoadLastScene() {
        GameController.Instance.LoadLevel();
    }

    public void RestartGame() {
        GameController.Instance.level = 1;
        GameController.Instance.LoadLevel();
    }
}
