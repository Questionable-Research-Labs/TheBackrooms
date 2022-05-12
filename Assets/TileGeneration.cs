using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Assertions;

public class TileGeneration : MonoBehaviour {
	[SerializeField] public GameObject wallPrefab;

	[SerializeField] public GameObject player;
	// using TileData = ;

	private Dictionary<(float, float), Dictionary<Direction, GameObject>> tileMap = new();
	
	private readonly Dictionary<Direction,Vector2> directionOffsets = new() {
		{ Direction.North, Vector2.up / 2},
		{ Direction.South, Vector2.down / 2},
		{ Direction.East, Vector2.left / 2},
		{ Direction.West, Vector2.right /2},
	};

	// Start is called before the first frame update
	private void Start() {
	}

	// Update is called once per frame
	private void Update() {
		GenerateTile();
	}


	private void GenerateTile() {
		var centerPoint = player.transform.position;
		const int gridSize = 10;
		
		Dictionary<(float, float), Dictionary<Direction, GameObject>> newTileMap = new();

		for (var xPointIndex = Mathf.RoundToInt(centerPoint.x - gridSize / 2.0f);
		     xPointIndex < Mathf.RoundToInt(centerPoint.x + gridSize / 2.0f);
		     xPointIndex++)
		for (var yPointIndex = Mathf.RoundToInt(centerPoint.z - gridSize / 2.0f);
		     yPointIndex < Mathf.RoundToInt(centerPoint.z + gridSize / 2.0f);
		     yPointIndex++) {
			(float, float) key = (xPointIndex, yPointIndex);

			if (tileMap.ContainsKey(key)) continue;

			var tileWallMap = new Dictionary<Direction, GameObject>();
			
			
			foreach (var (direction,offset) in directionOffsets) {
				// 75% chance of air
				if (!(Random.Range(0.0f, 1.0f) >= 0.75)) continue;
				
				// Make sure there is not a room next door already with a wall
				(float, float) neighbourTileKey = (key.Item1 + offset.x, key.Item2 + offset.y);
				if (tileMap.ContainsKey(neighbourTileKey)) {
					var neighbourTile = tileMap[neighbourTileKey];
					if (neighbourTile.ContainsKey(FlipDirection(direction))) continue;
				} else if (newTileMap.ContainsKey(neighbourTileKey)) {
					var neighbourTile = newTileMap[neighbourTileKey];
					if (neighbourTile.ContainsKey(FlipDirection(direction))) continue;
				}
				// Empty Spot
					
				var rotation = Quaternion.identity;
				rotation *= Quaternion.Euler(0, offset.y != 0 ? 90 : 0, 0);
				var finalVector3 = new Vector3(xPointIndex + offset.x, 0.5f, yPointIndex + offset.y);
				if (!newTileMap.ContainsKey(key)) {
					tileWallMap.Add(direction, Instantiate(wallPrefab, finalVector3, rotation));
				};
			}
			newTileMap.Add(key, tileWallMap);
				
		}
		
		// Generated tiles, now lets remove old ones
		foreach (var (point, walls) in tileMap) {
			if (!newTileMap.ContainsKey(point)) {
				Debug.Log("Wall Object yeeted" + point);
				foreach (var wallObject in walls.Values) {
					Destroy(wallObject);
				}
			}
		}
		// Now replace old map
		tileMap = newTileMap;

	}
	private enum Direction {
		North,
		South,
		East,
		West
	}

	private Direction FlipDirection(Direction start) {
		switch (start) {
			case Direction.North:
				return Direction.South;
			case Direction.South:
				return Direction.North;
			case Direction.East:
				return Direction.West;
			case Direction.West:
				return Direction.East;
		}

		throw new AssertionException("Asked to flip unknown direction","Unreachable Case Detected");
	}
}