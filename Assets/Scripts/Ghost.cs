using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityStandardAssets.Characters.ThirdPerson;
using UnityStandardAssets.CrossPlatformInput;

public class Ghost : MonoBehaviour {

    float lifeTimer = 0f;
    ThirdPersonCharacter character;

    [HideInInspector]
    public MainPlayer player;
    [HideInInspector]
    public GuardAI[] guards;

    // Use this for initialization
    void Start () {
        character = GetComponent<ThirdPersonCharacter>();
        character.m_MoveSpeedMultiplier = GameController.Instance.ghostSpeed;
    }
	
	// Update is called once per frame
	void Update () {
        if (!GameController.Instance.ghostMode && IsCaught()) {
            player.GameOver();
        }
        if ((lifeTimer >= GameController.Instance.ghostLifeTime || IsCaught())
            || (GameController.Instance.timeMode && (CrossPlatformInputManager.GetButtonDown("Jump") || GameController.Instance.ghostTime <=0)))
        {
            Deactivate();
        }
        else if (GameController.Instance.timeMode && CrossPlatformInputManager.GetButtonDown("Jump")){
            Deactivate();
        }
        else
        {
            lifeTimer += Time.deltaTime;
            if (GameController.Instance.timeMode) {
                GameController.Instance.DropChargeTime();
            }
        }
	}

    bool IsCaught() {
        if (guards.Length == 0)
            return false;
        foreach (GuardAI guard in guards) {
            if (Vector3.Distance(guard.transform.position, transform.position) <= GameController.Instance.captureDistance)
                return true;
        }
        return false;
    }

    public void Deactivate() {
        player.Reactivate();
        player.RemoveTarget(transform);
        Destroy(gameObject);
    }
}
