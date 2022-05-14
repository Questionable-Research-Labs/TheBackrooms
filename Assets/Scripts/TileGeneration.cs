using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Assertions;
using Random = UnityEngine.Random;


public class TileGeneration : MonoBehaviour {
	public enum Direction {
		North,
		South,
		East,
		West
	}
	public class TileMapStore : Dictionary<(float, float), Dictionary<Direction, GameObject>> {}

	[SerializeField] public GameObject wallPrefab;
	public float wallSpawnHeight = 0.49f;
	
	[Range(0.0f, 1.0f)]
	public float chanceOfWall = 0.75f;
	
	public int gridSize = 50;

	// using TileData = ;

	private TileMapStore tileMap = new();
	private TileMapStore newTileMap = new();
	
	
	
	private readonly Dictionary<Direction,Vector2> directionOffsets = new() {
		{ Direction.North, Vector2.up / 2},
		{ Direction.South, Vector2.down / 2},
		{ Direction.East, Vector2.left / 2},
		{ Direction.West, Vector2.right /2},
	};
	
	private void Start() {
		// definitely nothing special about 0.177013, shut.
		InvokeRepeating(nameof(GenerateTile), 0, 0.177013f);
	}

	private void GenerateTile() {
		var centerPoint = transform.position;
		
		newTileMap = new();

		for (var xPointIndex = Mathf.RoundToInt(centerPoint.x - gridSize / 2.0f);
		     xPointIndex < Mathf.RoundToInt(centerPoint.x + gridSize / 2.0f);
		     xPointIndex++)
		for (var yPointIndex = Mathf.RoundToInt(centerPoint.z - gridSize / 2.0f);
		     yPointIndex < Mathf.RoundToInt(centerPoint.z + gridSize / 2.0f);
		     yPointIndex++) {
			(float, float) key = (xPointIndex, yPointIndex);

			if (newTileMap.ContainsKey(key)) continue;

			var tileWallMap = new Dictionary<Direction, GameObject>();
			
			
			foreach (var (direction, offset) in directionOffsets) {
				// 75% chance of air
				if (!(Random.Range(0.0f, 1.0f) >= chanceOfWall)) continue;
				
				// Make sure there is not a room next door already with a wall
				(float, float) neighbourTileKey = (key.Item1 + offset.x, key.Item2 + offset.y);
				if (newTileMap.ContainsKey(neighbourTileKey)) {
					var neighbourTile = newTileMap[neighbourTileKey];
					if (neighbourTile.ContainsKey(FlipDirection(direction))) continue;
				}

				// Empty Spot
				var rotation = Quaternion.identity;
				rotation *= Quaternion.Euler(0, offset.y != 0 ? 90 : 0, 0);
				var finalVector3 = new Vector3(xPointIndex + offset.x, wallSpawnHeight, yPointIndex + offset.y);
				if (!tileMap.ContainsKey(key)) {
					tileWallMap.Add(direction, Instantiate(wallPrefab, finalVector3, rotation));
				}
			}
			newTileMap.Add(key, tileWallMap);		
		}

		// Generated tiles, now lets remove old ones
	

		foreach (var (point, walls) in newTileMap)
		{
			if (!tileMap.ContainsKey(point))
			{
				tileMap.Add(point,walls);
			}
		}
		
		List<(float,float)> points_to_yeet = new();
		foreach (var (point, walls) in tileMap) {
			if (!newTileMap.ContainsKey(point)) {
				foreach (var wallObject in walls.Values) {
					Destroy(wallObject);
				}
				points_to_yeet.Add(point);

			}
		}
		
		points_to_yeet.ForEach(point => tileMap.Remove(point));
		// Now replace old map
		// tileMap = newTileMap;

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