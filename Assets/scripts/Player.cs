using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;
using UnityEditor;

public class Card
{
	public Action<Player> Action { get; set; }
	public string Title { get; set; }
	// A number used to determine probablity of card selection. Higher is better.
	public int Points { get; set; }
	public Texture2D Icon { get; set; }
}

public class CardActions
{
	public List<Card> Cards = new List<Card>();

	public CardActions()
	{
		//AssetDatabase.LoadAssetAtPath("")
		Cards.Add(new Card() {
			Title = "Move 3",
			Action = p => MoveFoward(p, 3),
			Points = 1,
			Icon = (Texture2D) AssetDatabase.LoadAssetAtPath("Assets/GUI/icons/mov3.png", typeof(Texture2D))
		});
		Cards.Add(new Card() {
			Title = "Move 2",
			Action = p => MoveFoward(p, 2),
			Points = 2,
			Icon = (Texture2D) AssetDatabase.LoadAssetAtPath("Assets/GUI/icons/mov2.png", typeof(Texture2D))
		});
		Cards.Add(new Card() {
			Title = "Move 1",
			Action = p => MoveFoward(p, 1),
			Points = 3,
			Icon = (Texture2D) AssetDatabase.LoadAssetAtPath("Assets/GUI/icons/mov1.png", typeof(Texture2D))
		});
		Cards.Add(new Card() {
			Title = "Back",
			Action = Back,
			Points = 2,
			Icon = (Texture2D) AssetDatabase.LoadAssetAtPath("Assets/GUI/icons/back.png", typeof(Texture2D))
		});
		Cards.Add(new Card() {
			Title = "Rot CW",
			Action = RotCW,
			Points = 2,
			Icon = (Texture2D) AssetDatabase.LoadAssetAtPath("Assets/GUI/icons/rotcw.png", typeof(Texture2D))
		});
		Cards.Add(new Card() {
			Title = "Rot CW2",
			Action = RotCW2,
			Points = 1,
			Icon = (Texture2D) AssetDatabase.LoadAssetAtPath("Assets/GUI/icons/rotcw2.png", typeof(Texture2D))
		});
		Cards.Add(new Card() {
			Title = "Rot CCW",
			Action = RotCCW,
			Points = 2,
			Icon = (Texture2D) AssetDatabase.LoadAssetAtPath("Assets/GUI/icons/rotccw.png", typeof(Texture2D))
		});
		Cards.Add(new Card() {
			Title = "Rot CCW2",
			Action = RotCCW2,
			Points = 1,
			Icon = (Texture2D) AssetDatabase.LoadAssetAtPath("Assets/GUI/icons/rotccw2.png", typeof(Texture2D))
		});
	}

	void MoveFoward (Player player, int movementPoints)
	{
		var map = player.Map;
		while (movementPoints > 0) {
			var testPosition = player.targetPosition + player.transform.forward;
			var hex = map.GetHex (testPosition);
			if (hex == null || hex.MovementCost > movementPoints)
				break;
			if (hex.HasWater) {
				player.foundWater = true;
			}
			if (hex.HasFood) {
				player.foundFood = true;
			}
			if (hex.IsWinCondition) {
				player.gameOver = true;
				player.lifeLevel = 0;
			}
			player.currentHex = hex;
			movementPoints -= hex.MovementCost;
			player.targetPosition += player.transform.forward;
		}
	}

	void Back(Player player)
	{
		player.targetPosition -= player.transform.forward;
		var hex = player.Map.GetHex (player.targetPosition);
		if (hex == null) {
			player.targetPosition += player.transform.forward;
			return;
		}
		player.currentHex = hex;
		if (hex.HasWater) {
			player.foundWater = true;
		}
		if (hex.HasFood) {
			player.foundFood = true;
		}
		if (hex.IsWinCondition) {
			player.gameOver = true;
			player.lifeLevel = 0;
		}
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
	public int numDays = 0;
	public int numCardsPlayedToday = 0;
	public List<Card> cards = new List<Card>();
	// Values on water and food indexes represent life levels lost or gained.
	public int[] waterIndex = new int[] {0, 0, 1, 1, 2, 3, 4, 7};
	public int[] foodIndex = new int[]{0, 0, 0, 0, 1, 1, 1, 1, 2, 2, 2, 3, 3, 3, 4, 4, 4, 5, 5, 6, 6, 8, 10, 13};
	// Values on life index represent movement points at that life level.
	public int[] lifeIndex = new int[]{6, 5, 5, 4, 4, 3, 3, 2, 2, 1, 1, 0, 0, 0, 0};
	public GameObject game;
	private Map map;
	public bool gameOver = false;
	public Hex currentHex;
	public GUISkin skin;
	public GUIStyle goodStyle, badStyle;
	public Texture2D campIcon;

	CardActions cardDefs;

	void Awake() {
		cardDefs = new CardActions();
		campIcon = (Texture2D) AssetDatabase.LoadAssetAtPath("Assets/GUI/icons/camp.png", typeof(Texture2D));
	}

	void ShuffleCards() {
		var r = new UnityEngine.Random();
		var totalPoints = cardDefs.Cards.Sum(c => c.Points);
		while(cards.Count < lifeIndex[lifeLevel]) {
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

	void GameOver()
	{
		gameOver = true;
		Debug.Log("GAME OVER");
		this.transform.rotation.Set(90, 0, 0, 0);
	}

	void NewDay()
	{
		if (currentHex != null && numCardsPlayedToday == 0) {
			if (currentHex.HasFood) {
				foodLevel = Math.Max(0, foodLevel - 3);
				foundFood = true;
			}
			if (currentHex.HasWater) {
				waterLevel = Math.Max(0, waterLevel - 1);
				foundWater = true;
			}
		}
		if (!foundWater) {
			waterLevel++;
		}
		if (!foundFood) {
			foodLevel++;
		}
		if (waterLevel >= waterIndex.Length || foodLevel > foodIndex.Length) {
			GameOver();
			return;
		}
		lifeLevel = waterIndex[waterLevel] + foodIndex[foodLevel];
		if (lifeLevel > lifeIndex.Length) {
			GameOver();
			return;
		}
		foundWater = false;
		foundFood = false;

		ShuffleCards();
		numDays++;
		numCardsPlayedToday = 0;
	}

	void NewGame()
	{
		Start();
		targetPosition = new Vector3(0, 0.3f, 0);
		lifeLevel = 0;
		waterLevel = 0;
		foodLevel = 0;
		foundFood = true;
		foundWater = true;
		gameOver = false;
		numDays = 0;
	}

	void Start() {
		ShuffleCards();
		targetPosition = transform.position;
		game = GameObject.FindGameObjectWithTag(Tags.GameController);
	}

	public Map Map {
		get {
			if (map == null) {
				map = game.GetComponent<TileFactory>().map;
			}
			return map;
		}
	}

	void OnGUI() {
		GUI.skin = skin;
		string health = string.Format("Water: {0}, Food: {1}, Life: {2}, Movement: {6} found food: {3}, found water: {4}, days: {5}",
		                              waterLevel, foodLevel, lifeLevel, foundFood, foundWater, numDays, lifeIndex[lifeLevel]);
		GUI.Label(new Rect(20, 20, 1000, 300), health.ToString());
		if (gameOver) {
			GUI.Label(new Rect(Screen.width / 2 - 200, Screen.height / 2 - 50, 500, 100), lifeLevel > 0 ? "YOU DIED :(" : "YOU MADE IT!");
			if (GUI.Button(new Rect(Screen.width / 2 - 50, Screen.height / 2 + 100, 100, 50), "Play Again")) {
				NewGame();
			}
			return;
		}

		for(int i = 0; i < cards.Count; i++) {
			if (GUI.Button(new Rect(i * 220 + 10, Screen.height - 210, 200, 200), cards[i].Icon)) {
				Debug.Log(cards[i].Title);
				cards[i].Action(this);
				cards.RemoveAt(i);
				numCardsPlayedToday++;
			}
		}

		if (GUI.Button (new Rect(10, Screen.height - 350, 300, 114), campIcon)) {
			cards.Clear();
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
