﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

public class EmotionsGameManager : AbstractGameManager {		
	private static EmotionsGameManager instance;
	private int score;
	private int scoreGoal = 3;
	private List<GameObject> primaryEmotions;
	private List<GameObject> secondaryEmotions;
	private List<GameObject> activeEmotions;
	private GameObject currentEmotionToMatch;
	private int difficultyLevel;
	private bool gameOver = false;
	private Coroutine tutorialCoroutine, drawingCoroutine;

    [HideInInspector] public EmotionsGenerator generator;
    public VoiceOversData voData;
    public Transform monsterLocation;
	public float timeLimit = 30;
	public bool gameStarted = false;
    public bool isTutorialRunning = false;
    public bool inputAllowed = false;
    [HideInInspector] public bool isDrawingCards = false;

    public Text timerText;
	public ScoreGauge scoreGauge;
	public Timer timer;
	public GameObject subtitlePanel;
	public Transform[] emotionSpawnLocs;
	public GameObject backButton;
	public float waitDuration = 3f;

	public AudioClip[] answerSounds;

	public GameObject tutorialHand;
	public Canvas tutorialCanvas;

    void Awake() {
		if(instance == null) {
			instance = this;
		}
		else if(instance != this) {
			Destroy(gameObject);
		}

        CheckForGameManager ();

        SoundManager.GetInstance ().ChangeBackgroundMusic (backgroundMusicArray[Random.Range (0, backgroundMusicArray.Length)]);

        generator = GetComponent<EmotionsGenerator> ();
		tutorialCanvas.gameObject.SetActive (false);
		tutorialHand.SetActive (false);
        generator.cardHand.gameObject.SetActive (false);

		if (GameManager.GetInstance ()) {
			difficultyLevel = GameManager.GetInstance ().GetLevel (DataType.Minigame.MonsterEmotions);
		}
	}

	public static EmotionsGameManager GetInstance() {
		return instance;
	}

	public override void PregameSetup () {
        if (!generator.monster)
            generator.CreateMonster ();
        generator.ChangeMonsterEmotion (DataType.MonsterEmotions.Happy);
        if (GameManager.GetInstance ().GetPendingTutorial (DataType.Minigame.MonsterEmotions)) {
            tutorialCoroutine = StartCoroutine (RunTutorial ());
        } else {
            switch (difficultyLevel) {
                case 2:
                    scoreGoal = 5;
                    break;
                case 3:
                    scoreGoal = 7;
                    break;
                default:
                    scoreGoal = 3;
                    break;
            }

            score = 0;
            if (timer != null) {
                timerText.gameObject.SetActive (true);
                timer.SetTimeLimit (this.timeLimit);
                timer.StopTimer ();
            }
            UpdateScoreGauge ();

            timerText.text = "Time: " + timer.TimeRemaining ();
            generator.cardHand.gameObject.SetActive (true);
            generator.cardHand.SpawnIn ();
            StartCoroutine (DisplayGo ());
        }
	}

	public IEnumerator DisplayGo() {
        yield return new WaitForSeconds (2.0f);
        DrawCards (0.5f);
        GameManager.GetInstance ().Countdown ();
		yield return new WaitForSeconds (3.0f);
        generator.ChangeMonsterEmotion (generator.currentTargetEmotion);
        yield return new WaitForSeconds (1.0f);
        PostCountdownSetup ();
	}

	IEnumerator RunTutorial () { 
		print ("RunTutorial");
		isTutorialRunning = true;
		tutorialCanvas.gameObject.SetActive (true);
		inputAllowed = false;
		scoreGauge.gameObject.SetActive (false);
		timer.gameObject.SetActive (false);

		yield return new WaitForSeconds(0.5f);
		subtitlePanel.SetActive (true);

		subtitlePanel.GetComponent<SubtitlePanel> ().Display ("Welcome to Monster Feelings!", null);
		SoundManager.GetInstance().StopPlayingVoiceOver();
        AudioClip tutorial1 = voData.FindVO ("1_tutorial_start");
		SoundManager.GetInstance().PlayVoiceOverClip(tutorial1);
        
        float secsToRemove = 6f;
        yield return new WaitForSeconds(tutorial1.length - secsToRemove);
        generator.ChangeMonsterEmotion (DataType.MonsterEmotions.Afraid);
        generator.cardHand.gameObject.SetActive (true);
        subtitlePanel.GetComponent<SubtitlePanel> ().Hide ();

        float secsToRemoveAgain = 4f;
        yield return new WaitForSeconds (secsToRemove - secsToRemoveAgain);
        TutorialDrawCards ();
        
        yield return new WaitForSeconds (secsToRemoveAgain);

        tutorialHand.SetActive (true);
		tutorialHand.GetComponent<Animator> ().Play ("EM_HandMoveMonster");
		yield return new WaitForSeconds(1.75f);
		subtitlePanel.GetComponent<SubtitlePanel> ().Display (generator.currentTargetEmotion.ToString(), null);
		SoundManager.GetInstance ().PlaySFXClip (answerSounds [1]);
		yield return new WaitForSeconds(2.0f);
		subtitlePanel.GetComponent<SubtitlePanel> ().Hide ();
		yield return new WaitForSeconds(1.0f);

        AudioClip nowyoutry = voData.FindVO ("nowyoutry");
        subtitlePanel.GetComponent<SubtitlePanel> ().Display ("Now you try!", nowyoutry);
		inputAllowed = true;
	}

	public void TutorialFinished() {
		inputAllowed = false;

        isTutorialRunning = false;
        tutorialHand.SetActive (false);
		GameManager.GetInstance ().CompleteTutorial(DataType.Minigame.MonsterEmotions);
		StopCoroutine (tutorialCoroutine);
		StartCoroutine(TutorialTearDown ());
	}

	IEnumerator TutorialTearDown() {
		print ("TutorialTearDown");
        if (isDrawingCards)
            StopCoroutine (drawingCoroutine);

        yield return new WaitForSeconds (1.5f);
        generator.ChangeMonsterEmotion (DataType.MonsterEmotions.Joyous);
        generator.RemoveCards ();
        AudioClip letsplay = voData.FindVO ("letsplay");
        SoundManager.GetInstance ().StopPlayingVoiceOver ();
        subtitlePanel.GetComponent<SubtitlePanel> ().Display ("Let's play!", letsplay);
		yield return new WaitForSeconds(letsplay.length);
        if (generator.cardHand.gameObject.activeSelf)
            generator.cardHand.ExitAnimation ();
        tutorialCanvas.gameObject.SetActive (false);

		yield return new WaitForSeconds(1.0f);
		subtitlePanel.GetComponent<SubtitlePanel> ().Hide ();

		PregameSetup ();
	}

	private void PostCountdownSetup() {
		StartGame();
	}

    private void StartGame () {
        scoreGauge.gameObject.SetActive (true);

        timer.StopTimer ();
        timer.StartTimer ();
        print (timer);
        gameStarted = true;
        inputAllowed = true;
    }

    IEnumerator PostGame () {
        print ("PostGame");
        gameStarted = false;
        StopCoroutine (drawingCoroutine);
        gameOver = true;
        inputAllowed = false;
        yield return new WaitForSeconds (1.0f);
        generator.RemoveCards ();
        AudioClip end = voData.FindVO ("end");

        subtitlePanel.GetComponent<SubtitlePanel> ().Display ("Great job! You matched " + score + " emotions!", end);
        yield return new WaitForSeconds (3.0f);
        if (generator.cardHand.gameObject.activeSelf)
            generator.cardHand.ExitAnimation ();
        yield return new WaitForSeconds (1.0f);
        GameOver ();
    }

    override public void GameOver () {
        if (gameOver) {
            print ("GameOver");
            backButton.SetActive (true);
            if (difficultyLevel == 1) {
                UnlockSticker ();
            } else {
                GameManager.GetInstance ().CreateEndScreen (typeOfGame, EndScreen.EndScreenType.CompletedLevel);
            }
            // "Great job! You matched " + score + " emotions!";
            GameManager.GetInstance ().LevelUp (DataType.Minigame.MonsterEmotions);
        }
    }

    void Update () {
        //print ("timer.TimeRemaining (): " + timer.TimeRemaining () + " | " + "gameStarted: " + gameStarted + " | " + "score: " + score);
        if (gameStarted && (timer.TimeRemaining () <= 0.0f || score >= scoreGoal)) {
            StartCoroutine (PostGame ());
        }
    }

    void FixedUpdate () {
        if (gameStarted) {
            timerText.text = "Time: " + timer.TimeRemaining ();
        }
    }

    public void CheckEmotion (DataType.MonsterEmotions emotion) {
        PauseGame ();
        if (emotion == generator.currentTargetEmotion) {
            SoundManager.GetInstance ().PlaySFXClip (answerSounds[1]);
            if (isTutorialRunning) {
                TutorialFinished ();
            } else {
                ++score;
                UpdateScoreGauge ();
                if (gameStarted)
                    DrawCards (waitDuration);
            }

        } else {
            StartCoroutine (WrongAnswerWait (waitDuration));
        }
    }

    public void DrawCards(float waitPeriod) {
        isDrawingCards = true;
        drawingCoroutine = StartCoroutine (generator.CreateNextEmotions (waitPeriod));
    }

    public void TutorialDrawCards () {
        isDrawingCards = true;
        drawingCoroutine = StartCoroutine (generator.CreateTutorialCards ());
    }

    public void ContinueGame() {
        if (gameStarted) {
            generator.ChangeMonsterEmotion (generator.currentTargetEmotion);
            inputAllowed = true;
            timer.StartTimer ();
        }
    }

    public void PauseGame() {
        timer.StopTimer ();
        inputAllowed = false;
    }

	public IEnumerator WrongAnswerWait (float duration) {
		yield return new WaitForSeconds (duration);
        if (!isTutorialRunning)
            ContinueGame ();
        else {
            inputAllowed = true;
        }
            
	}

    void UpdateScoreGauge () {
        if (scoreGauge.gameObject.activeSelf)
            scoreGauge.SetProgressTransition ((float)score / scoreGoal);
    }

    public void SkipReviewButton (GameObject button) {
        SkipReview ();
        Destroy (button);
    }

    public void SkipReview () {
        StopCoroutine (tutorialCoroutine);
        TutorialFinished ();
    }
}
