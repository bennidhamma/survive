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

public class CardActions
{
	Dictionary<Cards, Action<Player>> cards = new Dictionary<Cards, Action<Player>>();

	public CardActions()
	{
		cards[Cards.Move321] = Move321;
	}

	public void PlayCard(Cards card, Player player)
	{
		if (cards.ContainsKey(card)) {
			cards[card](player);
		}
	}

	void Move321(Player player)
	{
		// TODO : need to look at terrain type, assess cost, and adjust movement accordingly.
		player.targetPosition = player.transform.position + Vector3.forward;
	}
}

public class Player : MonoBehaviour {
	public Vector3 targetPosition;
	public int x = 0;
	public int z = 0;
	public int health = 100;
	public int maxCards = 5;
	public float direction = 0;
	public List<Cards> cards = new List<Cards>();
	// Values on water and food indexes represent life levels lost or gained.
	public int[] waterIndex = new int[] {0, 0, 1, 1, 2, 3, 4, 7};
	public int[] foodIndex = new int[]{0, 0, 0, 0, 1, 1, 1, 1, 2, 2, 2, 3, 3, 3, 4, 4, 4, 5, 5, 6, 6, 8, 10, 13};
	// Values on life index represent movement points at that life level.
	public int[] lifeIndex = new int[]{6, 5, 5, 4, 4, 3, 3, 2, 2, 1, 1, 0, 0, 0, 0};

	CardActions actions = new CardActions();

	void ShuffleCards() {
		var r = new UnityEngine.Random();
		var values = Enum.GetValues(typeof(Cards));
		while(cards.Count < maxCards) {
			cards.Add((Cards)values.GetValue(UnityEngine.Random.Range(0, values.Length)));
		}
	}

	void Start() {
		ShuffleCards();
		targetPosition = transform.position;
	}

	void OnGUI() {
		GUI.Label(new Rect(20, 20, 100, 100), health.ToString());
		for(int i = 0; i < cards.Count; i++) {
			if (GUI.Button(new Rect(i * 220, Screen.height - 100, 200, 50), cards[i].ToString()))
			{
				Debug.Log("move 1");
				actions.PlayCard(Cards.Move321, this);
			}
		}
	}

	void Update() {
		transform.position = Vector3.Lerp(transform.position, targetPosition, Time.deltaTime);
	}
}
