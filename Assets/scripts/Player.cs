using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public enum Cards
{
	Move321,
	Move642,
	RotCW,
	RotCCW,
	RotCW2,
	RotCCW2,
}

public class Player : MonoBehaviour {

	public int health = 100;
	public int maxCards = 5;
	public List<Cards> cards = new List<Cards>();
	// Values on water and food indexes represent life levels lost or gained.
	public int[] waterIndex = new int[0, 0, 1, 1, 2, 3, 4, 7];
	public int[] foodIndex = new int[0, 0, 0, 0, 1, 1, 1, 1, 2, 2, 2, 3, 3, 3, 4, 4, 4, 5, 5, 6, 6, 8, 10, 13];
	// Values on life index represent movement points at that life level.
	public int[] lifeIndex = new int[6, 5, 5, 4, 4, 3, 3, 2, 2, 1, 1, 0, 0, 0, 0];

	void ShuffleCards() {
		var r = new UnityEngine.Random();
		var values = Enum.GetValues(typeof(Cards));
		while(cards.Count < maxCards) {
			cards.Add((Cards)values.GetValue(UnityEngine.Random.Range(0, values.Length)));
		}
	}

	void Start() {
		ShuffleCards();
	}

	void OnGUI() {
		GUI.Label(new Rect(20, 20, 100, 100), health.ToString());
		for(int i = 0; i < cards.Count; i++) {
			if (GUI.Button(new Rect(i * 220, Screen.height - 100, 200, 50), cards[i].ToString()))
			{
				Debug.Log("move 1");
			}
		}
	}
}
