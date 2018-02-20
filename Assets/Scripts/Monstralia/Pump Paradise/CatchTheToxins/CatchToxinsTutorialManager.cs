﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CatchToxinsTutorialManager : MonoBehaviour {
    public bool isTutorialRunning = false;

    [SerializeField] private Color blueMonsterColor;
    [SerializeField] private Color greenMonsterColor;
    [SerializeField] private Color redMonsterColor;
    [SerializeField] private Color yellowMonsterColor;
    [SerializeField] private SpriteRenderer skinRenderer;
    [SerializeField] private GameObject tutorialAssets;
    [SerializeField] private GameObject tutorialSecondaryAssets;
    [SerializeField] private GameObject tutorialCanvas;
    [SerializeField] private GameObject magnifyingGlass;
    [SerializeField] private GameObject tutorialHand;
    [SerializeField] private GameObject tutorialRBC;
    [SerializeField] private GameObject tutorialBug;
    [SerializeField] private Animator faderAnimator;

    private Coroutine tutorialCoroutine;
    private float cameraOriginalSize;
    private Vector3 cameraOriginalPosition;
    private bool insideMonster = false;

    private void Awake () {
        cameraOriginalSize = Camera.main.orthographicSize;
        cameraOriginalPosition = Camera.main.transform.position;
        faderAnimator.gameObject.SetActive (false);
        tutorialCanvas.SetActive (false);
        tutorialAssets.SetActive (false);
        tutorialSecondaryAssets.SetActive (false);
        magnifyingGlass.SetActive (false);
        ChangeMonsterSkinColor (DetermineMonsterSkinColor ());
    }

    public void StartTutorial () {
        tutorialAssets.SetActive (true);
        tutorialCanvas.SetActive (true);
        tutorialCoroutine = StartCoroutine (Tutorial ());
    }

    IEnumerator Tutorial () {
        isTutorialRunning = true;
        SubtitlePanel.Instance.Display ("Welcome to Catch the Toxins!");
        yield return new WaitForSeconds (3f);

        magnifyingGlass.SetActive (true);
        yield return new WaitForSeconds (2f);
        SubtitlePanel.Instance.Display ("These are your monster's blood vessels!", null, false, 5f);
        yield return new WaitForSeconds (5f);

        magnifyingGlass.GetComponent<Animator> ().Play ("MagGlassTutorial2", -1, 0f);
        yield return new WaitForSeconds (1f);

        magnifyingGlass.SetActive (false);
        SubtitlePanel.Instance.Display ("Let's go inside your monster!");
        CatchToxinsManager.Instance.playerMonster.ChangeEmotions (DataType.MonsterEmotions.Joyous);
        yield return new WaitForSeconds (1f);

        ZoomIn ();
        yield return new WaitForSeconds (1f);

        TurnOffIntro ();
        yield return new WaitForSeconds (1f);

        CatchToxinsManager.Instance.whiteCell.gameObject.SetActive (true);
        faderAnimator.gameObject.SetActive (false);
        yield return new WaitForSeconds (2f);

        tutorialSecondaryAssets.SetActive (true);
        tutorialRBC.SetActive (false);
        tutorialBug.SetActive (false);
        yield return new WaitForSeconds (4f);

        tutorialRBC.SetActive (true);
        yield return new WaitForSeconds (3f);

        tutorialRBC.GetComponent<CatchToxinMovement> ().CanMove = false;
        yield return new WaitForSeconds (2.5f);

        tutorialRBC.GetComponent<CatchToxinMovement> ().CanMove = true;
        yield return new WaitForSeconds (1f);

        tutorialBug.SetActive (true);
        yield return new WaitForSeconds (3f);

        tutorialBug.GetComponent<CatchToxinMovement> ().CanMove = false;
        yield return new WaitForSeconds (2.5f);


        tutorialBug.GetComponent<CatchToxinMovement> ().CanMove = true;
        yield return new WaitForSeconds (3f);

        // Move Hand

        yield return new WaitForSeconds (1f);
        isTutorialRunning = false;
        EndTutorial ();
    }

    public void EndTutorial () {
        if (isTutorialRunning) StopCoroutine (tutorialCoroutine);
        tutorialCanvas.SetActive (false);
        GameManager.Instance.CompleteTutorial (CatchToxinsManager.Instance.typeOfGame);
        StartCoroutine (TutorialBreakDown ());
    }

    IEnumerator TutorialBreakDown () {
        SubtitlePanel.Instance.Display ("Let's play!");
        yield return new WaitForSeconds (2f);

        if (!insideMonster) {
            magnifyingGlass.GetComponent<Animator> ().Play ("MagGlassTutorial2", -1, 0f);
            CatchToxinsManager.Instance.playerMonster.ChangeEmotions (DataType.MonsterEmotions.Joyous);
            ZoomIn ();
            yield return new WaitForSeconds (1f);

            magnifyingGlass.SetActive (false);
            TurnOffIntro ();
            yield return new WaitForSeconds (1f);

            faderAnimator.gameObject.SetActive (false);
        } else {

            tutorialSecondaryAssets.SetActive (false);
            yield return new WaitForSeconds (1f);
        }

        CatchToxinsManager.Instance.PrepareToStart ();
    }

    IEnumerator CameraZoomIn () {
        Vector3 monsterPosition = CatchToxinsManager.Instance.playerMonster.transform.position;
        monsterPosition = new Vector3 (monsterPosition.x, monsterPosition.y, Camera.main.nearClipPlane);
        while (Camera.main.orthographicSize > 1) {
            Camera.main.orthographicSize -= 0.25f;
            Camera.main.transform.position = Vector2.MoveTowards (
                Camera.main.transform.position, monsterPosition,
                0.05f
            );
            yield return null;
        }
    }

    private void ZoomIn () {
        insideMonster = true;
        StartCoroutine (CameraZoomIn ());
        faderAnimator.gameObject.SetActive (true);
        faderAnimator.Play ("FadeOut", -1, 0f);
    }

    private void TurnOffIntro () {
        tutorialAssets.SetActive (false);
        CatchToxinsManager.Instance.playerMonster.gameObject.SetActive (false);
        Camera.main.orthographicSize = cameraOriginalSize;
        Camera.main.transform.position = cameraOriginalPosition;
        faderAnimator.Play ("FadeIn", -1, 0f);
    }

    private void ChangeMonsterSkinColor (Color color) {
        skinRenderer.color = color;
    }

    private Color DetermineMonsterSkinColor () {
        switch (GameManager.Instance.GetPlayerMonsterType()) {
            case DataType.MonsterType.Blue: return blueMonsterColor;
            case DataType.MonsterType.Green: return greenMonsterColor;
            case DataType.MonsterType.Red: return redMonsterColor;
            case DataType.MonsterType.Yellow: return yellowMonsterColor;
            default: return greenMonsterColor;
        }
    }
}
