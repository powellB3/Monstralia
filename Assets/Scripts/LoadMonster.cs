﻿using UnityEngine;
using System.Collections;

public class LoadMonster : MonoBehaviour {

	// Use this for initialization
	void Awake () {
		Debug.Log ("in Awake of load monster");
		Sprite monsterSprite = Resources.Load<Sprite>(GameManager.instance.getMonster());
		gameObject.GetComponent<SpriteRenderer>().sprite = monsterSprite;
	}
}
