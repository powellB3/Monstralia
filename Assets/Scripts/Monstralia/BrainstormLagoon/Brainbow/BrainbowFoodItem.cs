﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BrainbowFoodItem : MonoBehaviour {
    [HideInInspector] public BrainbowStripe stripeToAttach;

    private Vector3 offset;
    private Rigidbody2D rigBody;
    private bool isMoved = false;
    private bool isPlaced = false;
    private bool isBeingEaten = false;
    private SpriteRenderer spriteRenderer;

    private void Awake () {
        rigBody = gameObject.GetComponent<Rigidbody2D> ();
        spriteRenderer = gameObject.GetComponent<SpriteRenderer> ();
    }

    private void OnDestroy () {
        /*
        if (isBeingEaten)
            SoundManager.Instance.PlaySFXClip (BrainbowGameManager.Instance.munchSound);
        */
        if (BrainbowGameManager.Instance.activeFoods.Contains (this))
            BrainbowGameManager.Instance.activeFoods.Remove (this);
    }

    private void OnDisable () {
        if (isBeingEaten)
            SoundManager.Instance.PlaySFXClip (BrainbowGameManager.Instance.munchSound);
    }

    private void OnMouseDown () {
        if (BrainbowGameManager.Instance.inputAllowed) {
            offset = gameObject.transform.position - Camera.main.ScreenToWorldPoint (new Vector3 (Input.mousePosition.x, Input.mousePosition.y, 0f));
            BrainbowGameManager.Instance.ShowSubtitles (gameObject.name);
            SoundManager.Instance.AddToVOQueue (gameObject.GetComponent<Food> ().clipOfName);
            spriteRenderer.sortingOrder = 6;
        }
    }

    private void OnMouseUp () {
        if (isMoved) {
            if (stripeToAttach) {
                BrainbowGameManager.Instance.ScoreAndReplace (transform.parent);
                SoundManager.Instance.PlaySFXClip (BrainbowGameManager.Instance.correctSound);
                InsertItemIntoStripe (stripeToAttach);

                // Random praise VO line
                if (Random.value < 0.2f) {
                    int randomClipIndex = Random.Range (0, BrainbowGameManager.Instance.correctMatchClips.Length);
                    SoundManager.Instance.AddToVOQueue (BrainbowGameManager.Instance.correctMatchClips[randomClipIndex]);
                }

            } else {
                MoveBack ();
            }
            isMoved = false;
        }
        spriteRenderer.sortingOrder = 3;
    }

    void OnMouseDrag () {
        if (BrainbowGameManager.Instance.inputAllowed) {
            if (!isMoved)
                isMoved = true;
            if (isMoved) {
                Vector3 curScreenPoint = new Vector3 (Input.mousePosition.x, Input.mousePosition.y, 0f);
                Vector3 curPosition = Camera.main.ScreenToWorldPoint (curScreenPoint) + offset;
                rigBody.MovePosition (curPosition);
            }
        }
    }

    void MoveBack () {
        gameObject.transform.localPosition = Vector3.zero;
        SoundManager.Instance.PlaySFXClip (BrainbowGameManager.Instance.incorrectSound);
    }

    IEnumerator HideSubtitle () {
        yield return new WaitForSeconds (0.5f);
        BrainbowGameManager.Instance.HideSubtitles ();
    }

    public void InsertItemIntoStripe (BrainbowStripe stripe) {
        stripe.MoveItemToSlot (gameObject);
        gameObject.GetComponent<Collider2D> ().enabled = false;
        isPlaced = true;
        BrainbowGameManager.Instance.activeFoods.Add (this);
    }

    public IEnumerator GetEaten () {
        isBeingEaten = true;
        gameObject.GetComponent<Collider2D> ().enabled = true;
        yield return new WaitForSeconds (0.5f);

        for (float t = 0.0f; t < 1.0f; t += Time.deltaTime * 1f) {
            transform.position = Vector2.MoveTowards (transform.position, new Vector3 (0f, transform.position.y, 0f), t * 0.3f);
            Debug.DrawLine (transform.position, new Vector3 (0f, transform.position.y, 0f), Color.yellow);
            yield return new WaitForFixedUpdate ();
        }
        rigBody.bodyType = RigidbodyType2D.Dynamic;
        rigBody.gravityScale = 2.0f;
        yield return new WaitForSeconds (2f);
        gameObject.SetActive (false);
    }

    private void OnTriggerEnter2D (Collider2D collision) {
        if (collision.tag == "Monster" && isPlaced) {
            gameObject.SetActive (false);
            if (isBeingEaten)
                SoundManager.Instance.PlaySFXClip (BrainbowGameManager.Instance.munchSound);
        }
    }
}