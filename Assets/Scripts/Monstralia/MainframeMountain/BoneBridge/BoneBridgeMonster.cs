﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoneBridgeMonster : MonoBehaviour {
    public GameObject goalObject;

    private Rigidbody2D rigBody;
    private BoxCollider2D col;
    private Vector3 pointerOffset;
    private Vector3 cursorPos;
    public bool tapToMove;

    private void OnEnable () {
        BoneBridgeManager.PhaseChange += OnPhaseChange;
    }

    private void OnDisable () {
        BoneBridgeManager.PhaseChange -= OnPhaseChange;
    }

    private void Awake () {
        rigBody = gameObject.AddComponent<Rigidbody2D> ();
        rigBody.freezeRotation = true;
        rigBody.mass = 3f;
        rigBody.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        rigBody.interpolation = RigidbodyInterpolation2D.Interpolate;
        transform.SetParent (transform.root.parent);

    }

    public void OnMouseDown () {
        if (BoneBridgeManager.GetInstance ().inputAllowed) {
            if (tapToMove && BoneBridgeManager.GetInstance ().bridgePhase != BoneBridgeManager.BridgePhase.Crossing) {
                StopAllCoroutines ();
                BoneBridgeManager.GetInstance ().ChangePhase (BoneBridgeManager.BridgePhase.Crossing);
            }
        }
    }

    void OnPhaseChange(BoneBridgeManager.BridgePhase phase) {
        print ("BoneBridgeMonster OnPhaseChange firing: " + phase);
        switch (phase) {
            case BoneBridgeManager.BridgePhase.Start:
                
                break;
            case BoneBridgeManager.BridgePhase.Building:
                StopAllCoroutines ();
                break;
            case BoneBridgeManager.BridgePhase.Crossing:
                BoneBridgeManager.GetInstance ().CameraSwitch (gameObject);
                StartCoroutine (Move ());
                break;
            case BoneBridgeManager.BridgePhase.Finish:
                StopAllCoroutines ();
                break;
        }
    }

    IEnumerator Move () {
        Vector2 dir;
        while (true) {
            dir = goalObject.transform.position - transform.position;
            Debug.DrawLine (transform.position, goalObject.transform.position);
            MoveTowards (dir);
            yield return null;
        }
    }

    public void MoveTowards (Vector2 pos) {
        rigBody.AddForce (pos * 0.015f, ForceMode2D.Impulse);
    }
}