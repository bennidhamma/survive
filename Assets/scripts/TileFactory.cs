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
			Name = "Plains",
			MovementCost = 2
		};
	}
}

public class Map {
	public static float HutProbability = 0.01f;
	public static float WaterProbability = 0.2f;
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
		var rand = new Random();
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
}

public class Hex {
	public int X { get; set; }
	public int Z { get; set; }
	public Terrain Terrain { get; set; }
	public bool HasHut { get; set; }
	public bool HasWater { get; set; }
	public bool HasFood { get; set; }
	public bool HasTrail { get; set; }
	public Transform Tile { get; set; }

	public int MovementCost {
		get {
			if (HasTrail) return 1;
			if (HasWater) return 2;
			return Terrain.MovementCost;
		}
	}
}

public class TileFactory : MonoBehaviour {
	public Material grassy;
	public Material rocky;
	public Material forest;
	public Transform food;
	public Transform water;
	public Transform hut;
	public Transform tile;

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

	// Use this for initialization
	void Start () {
		map = new Map(mapHeight, mapWidth, this);
		map.Build();
		foreach(var hex in map.Hexes) {
			var v2pos = map.GameToWorld(hex.X, hex.Z);
			var hexPos = new Vector3(v2pos.x, 0, v2pos.y);
			Transform t = (Transform)Instantiate(tile, hexPos, Quaternion.identity);
			t.Rotate(90, 0, 0);
			t.GetComponent<Renderer>().material = hex.Terrain.Material;
			hex.Tile = t;
			if (hex.HasHut) {
				Instantiate(hut, hexPos, Quaternion.identity);
			} else {
				if (hex.HasFood) {
					Instantiate(food, hexPos, Quaternion.identity);
				}
				if (hex.HasWater) {
					Instantiate(water, hexPos, Quaternion.identity);
				}
			}
		}
	}
}
