﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityStandardAssets.Characters.ThirdPerson;
using UnityStandardAssets.CrossPlatformInput;

public class MainPlayer : MonoBehaviour
{

    public Ghost ghostPrefab;
    public AudioClip ghostSound;
    public AudioClip deathPunchSound;

    ThirdPersonCharacter character;
    CamFollow cam;
    bool active;
    GuardAI[] guards;
    Ghost ghost;
    AudioSource audio;

    // Use this for initialization
    void Start()
    {
        active = true;

        character = GetComponent<ThirdPersonCharacter>();
        audio = GetComponent<AudioSource>();
        cam = FindObjectOfType<CamFollow>();
        cam.target = transform;
        guards = FindObjectsOfType<GuardAI>();
        AddPrimaryTarget(transform);
    }

    // Update is called once per frame
    void Update()
    {
        if (IsCaught())
        {
            GameOver();
        }
        else if (CrossPlatformInputManager.GetButtonDown("Jump") && active)
        {
            if (GameController.Instance.timeMode && GameController.Instance.ghostTime > 0)
                EngageGhost();
            else if (!GameController.Instance.timeMode && GameController.Instance.ghostCharges > 0)
            {
                EngageGhost();
            }
        }
        else
        {
            character.m_MoveSpeedMultiplier = GameController.Instance.playerSpeed;
        }


    }

    void EngageGhost()
    {
        audio.clip = ghostSound;
        audio.volume = 0.9f;
        audio.Play();
        Deactivate();
        ghost = (Ghost)Instantiate(ghostPrefab, transform.position + transform.forward * GameController.Instance.ghostStartOffset, transform.rotation);
        ghost.player = this;
        ghost.guards = guards;
        cam.target = ghost.transform;
        if (!GameController.Instance.timeMode)
            GameController.Instance.DropChargeIncrement();
        if (GameController.Instance.ghostMode)
            AddTarget(ghost.transform);
        else
        {
            AddPrimaryTarget(ghost.transform);
            RemoveTarget(transform);
        }
    }

    public void Reactivate()
    {
        audio.clip = ghostSound;
        audio.volume = 0.5f;
        audio.Play();
        active = true;
        character.active = true;
        cam.target = transform;
        if (!GameController.Instance.ghostMode)
            AddPrimaryTarget(transform);
    }

    void Deactivate()
    {
        active = false;
        character.active = false;
    }

    bool IsCaught()
    {
        if (guards.Length == 0)
            return false;
        foreach (GuardAI guard in guards)
        {
            if (Vector3.Distance(guard.transform.position, transform.position) <= GameController.Instance.captureDistance)
                return true;
        }
        return false;
    }

    public void AddTarget(Transform item)
    {
        foreach (GuardAI guard in guards)
        {
            guard.prioritizedTransforms.Add(item);
        }
    }

    public void AddPrimaryTarget(Transform item)
    {
        foreach (GuardAI guard in guards)
        {
            guard.prioritizedTransforms.Insert(0, item);
        }
    }

    public void RemoveTarget(Transform item)
    {
        foreach (GuardAI guard in guards)
        {
            guard.prioritizedTransforms.Remove(item);
        }
    }

    public void GameOver() {
        if (ghost != null)
            ghost.Deactivate();
        character.TriggerDeath();
        StartCoroutine(GameOverCoroutine());
    }

    IEnumerator GameOverCoroutine() {
        
        audio.clip = deathPunchSound;
        audio.volume = 0.8f;
        audio.Play();
        yield return new WaitForSeconds(0.3f);
        GameController.Instance.playerKilled = true;
        //GameController.Instance.PlayDeathMusic();
        yield return new WaitForSeconds(2);
        GameController.Instance.LoadGameOver();
    }
}
