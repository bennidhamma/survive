using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;

public class Card
{
	public Action<Player> Action { get; set; }
	public string Title { get; set; }
	// A number used to determine probablity of card selection. Higher is better.
	public int Points { get; set; }
}

public class CardActions
{
	public List<Card> Cards = new List<Card>();

	public CardActions()
	{
		Cards.Add(new Card() {
			Title = "Move 3/2/1",
			Action = Move321,
			Points = 3
		});
		Cards.Add(new Card() {
			Title = "Move 6/4/2",
			Action = Move642,
			Points = 2
		});
		Cards.Add(new Card() {
			Title = "Back",
			Action = Back,
			Points = 2
		});
		Cards.Add(new Card() {
			Title = "Rot CW",
			Action = RotCW,
			Points = 2
		});
		Cards.Add(new Card() {
			Title = "Rot CW2",
			Action = RotCW2,
			Points = 1
		});
		Cards.Add(new Card() {
			Title = "Rot CCW",
			Action = RotCCW,
			Points = 2
		});
		Cards.Add(new Card() {
			Title = "Rot CCW2",
			Action = RotCCW2,
			Points = 1
		});
	}

	void Move321(Player player)
	{
		// TODO : need to look at terrain type, assess cost, and adjust movement accordingly.
		player.targetPosition += player.transform.forward;
	}

	void Move642(Player player)
	{
		player.targetPosition += (player.transform.forward * 2);
	}

	void Back(Player player)
	{
		player.targetPosition -= player.transform.forward;
	}

	void RotCW (Player player)
	{
		player.angle += 60;
	}

	void RotCW2 (Player player)
	{
		player.angle += 120;
	}

	void RotCCW (Player player)
	{
		player.angle -= 60;
	}

	void RotCCW2 (Player player)
	{
		player.angle -= 120;
	}
}

public class Player : MonoBehaviour {
	public Vector3 targetPosition;
	public float angle = 30;
	public int x = 0;
	public int z = 0;
	public int waterLevel = 0;
	public int foodLevel = 0;
	public int lifeLevel = 0;
	public int maxCards = 5;
	public bool foundFood = true;
	public bool foundWater = true;
	public float direction = 0;
	public List<Card> cards = new List<Card>();
	// Values on water and food indexes represent life levels lost or gained.
	public int[] waterIndex = new int[] {0, 0, 1, 1, 2, 3, 4, 7};
	public int[] foodIndex = new int[]{0, 0, 0, 0, 1, 1, 1, 1, 2, 2, 2, 3, 3, 3, 4, 4, 4, 5, 5, 6, 6, 8, 10, 13};
	// Values on life index represent movement points at that life level.
	public int[] lifeIndex = new int[]{6, 5, 5, 4, 4, 3, 3, 2, 2, 1, 1, 0, 0, 0, 0};

	CardActions cardDefs = new CardActions();

	void ShuffleCards() {
		var r = new UnityEngine.Random();
		var totalPoints = cardDefs.Cards.Sum(c => c.Points);
		while(cards.Count < maxCards) {
			var num = UnityEngine.Random.Range(0, totalPoints);
			foreach(var c in cardDefs.Cards) {
				num -= c.Points;
				if (num <= 0) {
					cards.Add(c);
					break;
				}
			}
		}
	}

	void NewDay()
	{
		ShuffleCards();
	}

	void Start() {
		ShuffleCards();
		targetPosition = transform.position;
	}

	void OnGUI() {
		string health = string.Format("Water: {0}, Food: {1}, Life: {2}, found food: {3}, found water: {4}",
		                              waterLevel, foodLevel, lifeLevel, foundWater, foundFood);
		GUI.Label(new Rect(20, 20, 400, 100), health.ToString());
		for(int i = 0; i < cards.Count; i++) {
			if (GUI.Button(new Rect(i * 220, Screen.height - 100, 200, 50), cards[i].Title)) {
				Debug.Log(cards[i].Title);
				cards[i].Action(this);
				cards.RemoveAt(i);
			}
		}
		if (cards.Count == 0) {
			NewDay();
		}
	}

	void Update() {
		transform.position = Vector3.Lerp(transform.position, targetPosition, Time.deltaTime * 2);
		var targetRotation = Quaternion.AngleAxis(angle, Vector3.up);
		transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, Time.deltaTime * 4);
	}
}
