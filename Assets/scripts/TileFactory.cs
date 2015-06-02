using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public enum TerrainType {
	Grass,
	Mountains,
	Forest
}

public class Map {
	public static float HutProbability = 0.1f;
	public static float WaterProbability = 0.2f;
	public static float FoodProbability = 0.2f;

	public int Width { get; set; }
	public int Height { get; set; }
	public TileFactory Game { get; set; }

	public List<Hex> Hexes = new List<Hex>();

	public Map(int width, int height, TileFactory game)
	{
		this.Width = width;
		this.Height = height;
		this.Game = game;
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
				if (heightSample < Game.heightScale / 3) heightSample = 2;
				Hex hex = new Hex() {
					X = (int)x,
					Z = (int)z,
					HasHut = Random.value < HutProbability
				};
				hex.HasWater = hex.HasHut || Random.value < WaterProbability;
				hex.HasFood = hex.HasHut || Random.value < FoodProbability;
				if (heightSample >= 4) {
					hex.Terrain = TerrainType.Mountains;
				} else if (woodSample >= 3) {
					hex.Terrain = TerrainType.Forest;
				} else {
					hex.Terrain = TerrainType.Grass;
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
	public TerrainType Terrain { get; set; }
	public bool HasHut { get; set; }
	public bool HasWater { get; set; }
	public bool HasFood { get; set; }
	public bool HasTrail { get; set; }
	public Transform Tile { get; set; }
}

public class TileFactory : MonoBehaviour {
	public Material grassy;
	public Material rocky;
	public Material forest;
	public Transform tile;

	public float offsetX = 0.5f;
	public float offsetY = 1f;
	public float tileHeight = 0.5f;
	public float sampleScale = 0.213f;
	public float heightScale = 5.0f;

	static float r = 0.5f;
	static float h = r * 2;
	public float vert = h * 0.86f;
	static int mapHeight = 20;
	static int mapWidth = 20;

	// Use this for initialization
	void Start () {
		var map = new Map(mapHeight, mapWidth, this);
		map.Build();
		foreach(var hex in map.Hexes) {
			var v2pos = map.GameToWorld(hex.X, hex.Z);
			Transform t = (Transform)Instantiate(tile, new Vector3(v2pos.x, 0, v2pos.y), Quaternion.identity);
			t.Rotate(90, 0, 0);
			Material mat = null;
			switch(hex.Terrain) {
			case TerrainType.Grass:
				mat = grassy;
				break;
			case TerrainType.Forest:
				mat = forest;
				break;
			case TerrainType.Mountains:
				mat = rocky;
				break;
			}
			t.GetComponent<Renderer>().material = mat;
			hex.Tile = t;
		}
	}
}
