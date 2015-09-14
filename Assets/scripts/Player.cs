using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;
using UnityEngine.UI;

public class Card
{
	public Action<Player> Action { get; set; }
	public string Title { get; set; }
	// A number used to determine probablity of card selection. Higher is better.
	public int Points { get; set; }
	public Texture2D Icon { get; set; }
	public AudioClip Sound {get; set; }
}

public class CardActions
{
	public List<Card> Cards = new List<Card>();
	
	public CardActions()
	{
		//AssetDatabase.LoadAssetAtPath("")
		Cards.Add(new Card() {
			Title = "Move 3",
			Sound = Resources.Load<AudioClip>("click-1"),
			Action = p => MoveFoward(p, 3),
			Points = 1,
			Icon = Resources.Load<Texture2D>("mov3")
		});
		Cards.Add(new Card() {
			Title = "Move 2",
			Sound = Resources.Load<AudioClip>("click-1"),
			Action = p => MoveFoward(p, 2),
			Points = 2,
			Icon = Resources.Load<Texture2D>("mov2")
		});
		Cards.Add(new Card() {
			Title = "Move 1",
			Sound = Resources.Load<AudioClip>("click-3"),
			Action = p => MoveFoward(p, 1),
			Points = 3,
			Icon = Resources.Load<Texture2D>("mov1")
		});
		Cards.Add(new Card() {
			Title = "Back",
			Sound = Resources.Load<AudioClip>("click-3"),
			Action = Back,
			Points = 2,
			Icon = Resources.Load<Texture2D>("back")
		});
		Cards.Add(new Card() {
			Title = "Rot CW",
			Sound = Resources.Load<AudioClip>("click-2"),
			Action = RotCW,
			Points = 2,
			Icon = Resources.Load<Texture2D>("rotcw")
		});
		Cards.Add(new Card() {
			Title = "Rot CW2",
			Sound = Resources.Load<AudioClip>("click-2"),
			Action = RotCW2,
			Points = 1,
			Icon = Resources.Load<Texture2D>("rotcw2")
		});
		Cards.Add(new Card() {
			Title = "Rot CCW",
			Sound = Resources.Load<AudioClip>("click-2"),
			Action = RotCCW,
			Points = 2,
			Icon = Resources.Load<Texture2D>("rotccw")
		});
		Cards.Add(new Card() {
			Title = "Rot CCW2",
			Sound = Resources.Load<AudioClip>("click-2"),
			Action = RotCCW2,
			Points = 1,
			Icon = Resources.Load<Texture2D>("rotccw2")
		});
	}
	
	void MoveFoward (Player player, int movementPoints)
	{
		var map = player.Map;
		while (movementPoints > 0) {
			var testPosition = player.targetPosition + player.transform.forward;
			var hex = map.GetHex (testPosition);
			if (hex == null)
				break;
			var moveCost = hex.GetMovementCost(player.currentHex);
			if (moveCost > movementPoints)
				break;
			player.currentHex = hex;
			HandleHex (player, hex);
			movementPoints -= moveCost;
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
		HandleHex (player, hex);
	}
	
	void HandleHex(Player player, Hex hex)
	{
		player.currentHex = hex;
		if (player.FindWater()) {
			player.foundWater = true;
		}
		if (player.FindFood()) {
			player.foundFood = true;
		}
		if (hex.Item != null) {		
			player.items.Add(hex.Item);
			player.Alert("You got the " + hex.Item.Key.ToString() + "!");
			hex.Item = null;
			UnityEngine.Object.Destroy(hex.ItemTransform.gameObject);
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
	public Texture2D waterIcon;
	public Texture2D foodIcon;
	public HashSet<Item> items = new HashSet<Item>();
	public AudioSource guiAudio;
	GameObject AlertObject;
	
	CardActions cardDefs;
	
	public bool HasItem (ItemKey key) {
		return items.Any(x => x.Key == key);
	}

	public void Alert(string message) {
		AlertObject.SetActive (true);
		var text = GameObject.Find ("Alert/Panel/Text").GetComponent<Text> ();
		text.text = message;
		Debug.Log ("text type " + text.GetType ());
		Debug.Log (message);
	}

	void Awake() {
		AlertObject = GameObject.Find ("Alert");
		AlertObject.SetActive (false);
		cardDefs = new CardActions();
		campIcon = Resources.Load<Texture2D>("camp");
		waterIcon = Resources.Load<Texture2D>("water");
		foodIcon = Resources.Load<Texture2D>("food");
		goodStyle = new GUIStyle();
		goodStyle.normal.textColor = Color.green;
		goodStyle.fontSize = 50;
		badStyle = new GUIStyle();
		badStyle.normal.textColor = Color.red;
		badStyle.fontSize = 50;
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
		waterLevel = 0;
		lifeLevel = 0;
		foodLevel = 0;
		ShuffleCards();
		gameOver = true;
		Debug.Log("GAME OVER");
		this.transform.rotation.Set(90, 0, 0, 0);
	}
	
	void NewDay()
	{
		if (currentHex != null && numCardsPlayedToday == 0) {
			// player has not played any cards this turn so has camped. if there's food or water nearby, give them some.
			if (FindFood()) {
				foodLevel = Math.Max(0, foodLevel - 3);
				foundFood = true;
			}
			if (FindWater()) {
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
	
	public bool FindFood()
	{
		if (currentHex.HasFood) {
			return true;
		}
		else if (HasItem(ItemKey.Rifle)) {
			var mask = LayerMask.GetMask("Resource");
			var collisions = Physics.OverlapSphere(currentHex.Tile.position, 1.5f, mask);
			if (collisions.Any(x => x.tag == Tags.Food)) {
				Debug.Log("rifle got some food");
				return true;
			}
		}
		return false;
	}
	
	public bool FindWater()
	{
		if (currentHex.HasWater) {
			return true;
		}
		return false;
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
		GUI.Box(new Rect(10, 10, 80, 76), new GUIContent(waterLevel.ToString(), waterIcon), foundWater ? goodStyle : badStyle);
		GUI.Box(new Rect(150, 10, 80, 76), new GUIContent(foodLevel.ToString(), foodIcon), foundFood ? goodStyle : badStyle);
		string health = string.Format("Life: {0}   Move: {1}   Days: {2}", lifeLevel, lifeIndex[lifeLevel], numDays);
		GUI.Label(new Rect(300, 10, 1000, 300), health.ToString());
		if (gameOver) {
			GUI.Label(new Rect(Screen.width / 2 - 200, Screen.height / 2 - 50, 500, 100), lifeLevel > 0 ? "YOU DIED :(" : "YOU MADE IT!");
			if (GUI.Button(new Rect(Screen.width / 2 - 200, Screen.height / 2 + 100, 500, 100), "Play Again")) {
				NewGame();
			}
			return;
		}
		
		for(int i = 0; i < cards.Count; i++) {
			if (GUI.Button(new Rect(i * 220 + 10, Screen.height - 210, 200, 200), cards[i].Icon)) {
				Debug.Log(cards[i].Title);
				Debug.Log(cards[i].Sound);
				guiAudio.PlayOneShot(cards[i].Sound);
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
