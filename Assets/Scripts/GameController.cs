using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ProgressBar;
using UnityEngine.SceneManagement;

public class GameController : MonoBehaviour {

    public int level = 1;
    public bool loadLevel = false;
    public bool ghostMode = true;
    public bool timeMode = true;
    public float[] ghostTimeArray;
    public int[] ghostChargesArray;
    public float playerSpeed = 1f;
    public float ghostSpeed = 2f;
    public float ghostStartOffset = 0.5f;
    public float captureDistance = 0.3f;

    [HideInInspector]
    public ProgressRadialBehaviour chargeBar;

    public static GameController Instance;
    [HideInInspector]
    public float ghostTime;
    [HideInInspector]
    public int ghostCharges;
    [HideInInspector]
    public float ghostLifeTime;
    float initialGhostTime;
    float initialGhostCharges;
    [HideInInspector]
    public bool playerKilled = false;

    private MusicManager music;

    int levelOffset = 2;

    private void Awake() {
        if (Instance == null) {
            Instance = this;
        } else {
            Destroy(gameObject);
        }
        DontDestroyOnLoad(transform.gameObject);
    }

    // Use this for initialization
    void Start () {
        music = FindObjectOfType<MusicManager>();
        SetUpLevel();
	}
	
	// Update is called once per frame
	void Update () {
        if (loadLevel) {
            loadLevel = false;
            LoadLevel();
        }
		
	}

    public void LoadLevel() {
        SceneManager.LoadScene(level + levelOffset);
        SetUpLevel();

    }

    private void SetUpLevel() {
        playerKilled = false;
        SetUpGhostTimes();
    }

    public void SetChargeBar() {
        if (timeMode)
            chargeBar.SetChargeText(ghostTime);
        else
            chargeBar.SetChargeText(ghostCharges);
    }

    public void NextLevel() {
        level++;
        LoadLevel();
    }

    public void DropChargeTime() {
        ghostTime -= Time.deltaTime;
        chargeBar.SetFillerSize(ghostTime / initialGhostTime);
        chargeBar.SetChargeText(ghostTime);
    }

    public void DropChargeIncrement() {
        ghostCharges--;
        chargeBar.SetFillerSize(ghostCharges / initialGhostCharges);
        chargeBar.SetChargeText(ghostCharges);
    }

    private void SetUpGhostTimes() {
        ghostCharges = ghostChargesArray[level - 1];
        ghostLifeTime = ghostTimeArray[level - 1];
        ghostTime = ghostLifeTime * ghostCharges;
        initialGhostTime = ghostTime;
        initialGhostCharges = ghostCharges;
    }

    public void LoadGameOver() {
        SceneManager.LoadScene(2);
        music.PlayGameOverMusic();
    }

    public void PlayDeathMusic() {
        music.PlayGameOverMusic();
    }

    public void LoadMainMenu() {
        SceneManager.LoadScene(0);
        music.PlayMenuMusic();
    }

}
