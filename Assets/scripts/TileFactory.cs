using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class Terrain
{
	public string Name { get; set; }
	public Material Material { get; set; }
	public int MovementCost { get; set; }
}

public class TerrainLibrary {
	public Terrain Mountains;
	public Terrain Grass;
	public Terrain Forest;

	public TerrainLibrary(TileFactory tf) {
		Mountains = new Terrain() {
			Material = tf.rocky,
			Name = "Mountains",
			MovementCost = 3
		};
		Grass = new Terrain() {
			Material = tf.grassy,
			Name = "Plains",
			MovementCost = 1
		};
		Forest = new Terrain() {
			Material = tf.forest,
			Name = "Forest",
			MovementCost = 2
		};
	}
}

public class Map {
	public static float HutProbability = 0.01f;
	public static float WaterProbability = 0.06f;
	public static float FoodProbability = 0.1f;

	public int Width { get; set; }
	public int Height { get; set; }
	public TileFactory Game { get; set; }
	public TerrainLibrary Terrain { get; set; }

	public List<Hex> Hexes = new List<Hex>();

	public Map(int width, int height, TileFactory game)
	{
		this.Width = width;
		this.Height = height;
		this.Game = game;
		this.Terrain = new TerrainLibrary(game);
	}

	public Hex GetHex(Vector3 worldPosition)
	{
		RaycastHit hit;
		worldPosition -= 5 * Vector3.up;
		if (Physics.Raycast(worldPosition, Vector3.up, out hit)) {
			return Hexes.Find(h => h.Tile == hit.transform);
		}
		Debug.Log("no hex found");
		return null;
	}

	public Hex GetHex(int x, int z)
	{
		return Hexes.First(h => h.X == x && h.Z == z);
	}

	public void Build()
	{
		//Random.seed = 1;
		var startHeightX = Random.Range(0, 1000);
		var startHeightZ = Random.Range(0, 1000);
		var startWoodsX = Random.Range (0, 1000);
		var startWoodsZ = Random.Range (0, 1000);
		for (float z = 0; z < Height; z++) {
			for (float x = 0; x < Width; x++) {
				float heightCoordX = (startHeightX + x) * Game.sampleScale;
				float heightCoordZ = (startHeightZ + z) * Game.sampleScale;
				float woodsCoordX = (startWoodsX + x) * Game.sampleScale;
				float woodsCoordZ = (startWoodsZ + z) * Game.sampleScale;
				float heightSample = Mathf.PerlinNoise(heightCoordX, heightCoordZ) * Game.heightScale;
				float woodSample = Mathf.PerlinNoise(woodsCoordX, woodsCoordZ) * Game.heightScale;
				Debug.Log(string.Format("{0}, {1}, {2}", heightCoordX, heightCoordZ, heightSample));
				Hex hex = new Hex() {
					X = (int)x,
					Z = (int)z,
					HasHut = Random.value < HutProbability
				};
				hex.IsWinCondition = x == Width - 1;
				hex.HasWater = hex.HasHut || Random.value < WaterProbability;
				hex.HasFood = hex.HasHut || Random.value < FoodProbability;
				if (heightSample >= 3) {
					hex.Terrain = Terrain.Mountains;
				} else if (woodSample >= 2) {
					hex.Terrain = Terrain.Forest;
				} else {
					hex.Terrain = Terrain.Grass;
				}
				Hexes.Add(hex);
			}
		}
	}
	
	public Vector2 GameToWorld(float x, float z)
	{
		return new Vector2(z % 2 == 0 ? x : x + Game.offsetX, z * Game.vert);
	}

	public Vector3 GameToWorld3(float x, float z)
	{
		var vec2 = GameToWorld(x, z);
		return new Vector3(vec2.x, 0, vec2.y);
	}
}

public class Hex {
	public int X { get; set; }
	public int Z { get; set; }
	public Terrain Terrain { get; set; }
	public bool HasHut { get; set; }
	public bool HasWater { get; set; }
	public bool HasFood { get; set; }
	public bool HasTrail { get; set; }
	public bool IsWinCondition { get; set; }
	public Transform Tile { get; set; }
	public List<Hex> RiverHexes { get; set; }
	public Item Item { get; set; }
	public Transform ItemTransform { get; set; }

	public Hex() {
		RiverHexes = new List<Hex>();
	}

	public int GetMovementCost (Hex fromHex) {
		if (HasTrail) return 1;
		if (this.RiverHexes.Contains(fromHex)) return 2;
		return Terrain.MovementCost;
	}
}

public enum ItemKey {
	Rifle,
	WaterBottle
}

public class Item {
	public ItemKey Key { get; set; }
	public Transform Transform { get; set; }
	public int Uses { get; set; }
	public override int GetHashCode ()
	{
		return Key.GetHashCode ();
	}
}

public class TileFactory : MonoBehaviour {
	public Material grassy;
	public Material rocky;
	public Material forest;
	public Transform food;
	public Transform water;
	public Transform water2;
	public Transform hut;
	public Transform tile;
	public Transform tree;
	public Transform mountain;
	public Transform rifle;
	public Transform waterBottle;
	public Transform debugBall;

	public float moveRiverX = 0;
	public float moveRiverZ = 0.5f;

	public float offsetX = 0.5f;
	public float offsetY = 1f;
	public float tileHeight = 0.5f;
	public float sampleScale = 0.213f;
	public float heightScale = 5.0f;
	public Map map;

	static float r = 0.5f;
	static float h = r * 2;
	public float vert = h * 0.86f;
	static int mapHeight = 20;
	static int mapWidth = 20;

	static Dictionary<int, List<Vector3>> treePositions = new Dictionary<int, List<Vector3>>();

	public List<Item> items = new List<Item>();

	void SetupItems()
	{
		items.Add (new Item () {
			Key = ItemKey.Rifle,
			Transform = rifle
		});

		items.Add (new Item () {
			Key = ItemKey.WaterBottle,
			Transform = waterBottle
		});
	}

	void SetupTreePositions()
	{
		var height = 0.5f;
		// 1 Tree
		treePositions[1] = new List<Vector3>() {
			new Vector3(0, height, 0)
		};
		// 2 trees
		treePositions[2] = new List<Vector3>() {
			new Vector3(-1f/3f, height, -1f/3f),
			new Vector3(1f/3f, height, 1f/3f)
		};
		// 3 trees
		treePositions[3] = new List<Vector3>() {
			new Vector3(-1f/3f, height, 0),
			new Vector3(1f/3f, height, -1f/3f),
			new Vector3(1f/3f, height, 1f/3f)
		};
		// 4 trees
		treePositions[4] = new List<Vector3>() {
			new Vector3(-1f/3f, height, -1f/3f),
			new Vector3(-1f/3f, height, 1f/3f),
			new Vector3(1f/3f, height, -1f/3f),
			new Vector3(1f/3f, height, 1f/3f)
		};
	}

	void SetupTreesForHex (Hex hex)
	{
		var numTrees = (int)Random.Range (1, 4);
		var treePos = treePositions [numTrees];
		for (int i = 0; i < numTrees; i++) {
			var pos = hex.Tile.transform.position + treePos[i];
			Instantiate(tree, pos, Quaternion.identity);
		}
	}

	// Use this for initialization
	void Start () {
		SetupItems();
		SetupTreePositions();
		map = new Map(mapHeight, mapWidth, this);
		map.Build();

		// first pass. hexes, trees, mountains.
		foreach(var hex in map.Hexes) {
			var hexPos = map.GameToWorld3(hex.X, hex.Z);
			Transform t = (Transform)Instantiate(tile, hexPos, Quaternion.identity);
			t.Rotate(90, 0, 0);
			t.GetComponent<Renderer>().material = hex.Terrain.Material;
			hex.Tile = t;
			if (hex.Terrain == map.Terrain.Forest) {
				SetupTreesForHex (hex);
			} else if (hex.Terrain == map.Terrain.Mountains) {
				Instantiate(mountain, hex.Tile.position + new Vector3(-0.5f, 0.1f, -0.5f), Quaternion.identity);
			}
			if (hex.HasHut) {
				Instantiate(hut, hexPos, Quaternion.identity);
			} else if (hex.HasFood) {
				Instantiate(food, hexPos, Quaternion.identity);
			}
		}

		// water
		List<Hex> hexesToAddWaterTo = new List<Hex>();
		foreach(var hex in map.Hexes) {
			var hexPos = map.GameToWorld3(hex.X, hex.Z);
			if (hex.HasWater) {
				SetupWater (hex, hexPos, hexesToAddWaterTo);
			}
		}

		// items
		foreach (var item in items) {
			var gamePos = new Vector2();
			//gamePos.x = Random.Range(2, mapWidth - 2);
			//gamePos.y = Random.Range(2, mapHeight - 2);
			gamePos.x = Random.Range(1, 3);
			gamePos.y = Random.Range(1, 3);
			var worldPos = map.GameToWorld3(gamePos.x, gamePos.y);
			worldPos.y = 0.5f;
			var hex = map.GetHex((int)gamePos.x, (int)gamePos.y);
			if (hex.Item != null) {
				Debug.LogFormat("Couldn't put {0} at {1},{2} because {3} was already there", item.Key, gamePos.x, gamePos.y, hex.Item.Key);
				continue;
			}
			Debug.LogFormat("putting {0} at {1},{2}", item.Key, gamePos.x, gamePos.y);
			hex.Item = item;
			hex.ItemTransform = (Transform)Instantiate(item.Transform, worldPos, Quaternion.identity);
		}
	}

	void SetupWater (Hex hex, Vector3 hexPos, List<Hex> hexesToAddWaterTo)
	{
		Transform parentWater = null;
		// The first river segment is globally positioned off the center of the current hex.
		Transform currentWater = (Transform)Instantiate (water, Vector3.zero, Quaternion.identity);
		currentWater.SetParent(hex.Tile, false);
		currentWater.transform.Rotate(new Vector3(-270, -180, 0));
		currentWater.Rotate (Vector3.up, 60 * Random.Range (0, 6));
		currentWater.Translate (new Vector3 (moveRiverX, 0, moveRiverZ));
		float rot = Random.value < 0.5 ? 60f : -60f;
		parentWater = currentWater;
		UpdateWaterStateForRiverSegmentNeighbors(currentWater, hexesToAddWaterTo);
		// The remaining segments are positioned locally as children of the preceding segments.
		for (int i = 0; i < Random.Range (2, 2); i++) {
			currentWater = (Transform)Instantiate (water, Vector3.zero, Quaternion.identity);
			currentWater.SetParent (parentWater, false);
			currentWater.Translate (new Vector3 (0, 0, 0.6f));
			var center = Vector3.Lerp (parentWater.transform.position, currentWater.transform.position, 0.5f);
			currentWater.RotateAround (center, Vector3.up, rot);
			if (map.GetHex (currentWater.transform.position) == null) {
				Object.Destroy (currentWater);
				break;
			}
			rot = Random.value < 0.3 ? rot : -rot;
			parentWater = currentWater;
			UpdateWaterStateForRiverSegmentNeighbors(currentWater, hexesToAddWaterTo);
		}
		hexesToAddWaterTo.ForEach(h => h.HasWater = true);
	}
	
	void UpdateWaterStateForRiverSegmentNeighbors(Transform riverSegment, List<Hex> hexesToAddWaterTo)
	{
		var worldLeft = riverSegment.TransformPoint(Vector3.left * 0.4f);
		var worldRight = riverSegment.TransformPoint(Vector3.right * 0.4f);
		var hexLeft = map.GetHex(worldLeft);
		if (hexLeft != null) {
			hexesToAddWaterTo.Add(hexLeft);
		}
		var hexRight = map.GetHex(worldRight);
		if (hexRight != null) {
			hexesToAddWaterTo.Add(hexRight);
		}
		if (hexRight != null && hexLeft != null) {
			hexRight.RiverHexes.Add(hexLeft);
			hexLeft.RiverHexes.Add(hexRight);
		}
	}
}

