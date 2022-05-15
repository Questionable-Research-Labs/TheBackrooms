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
	[SerializeField] private float airPercent;
	[SerializeField] public GameObject player;
	// using TileData = ;

	private TileMapStore tileMap = new();
	private TileMapStore newTileMap = new();
	
	
	
	private readonly Dictionary<Direction,Vector2> directionOffsets = new() {
		{ Direction.North, Vector2.up / 2},
		{ Direction.South, Vector2.down / 2},
		{ Direction.East, Vector2.left / 2},
		{ Direction.West, Vector2.right /2},
	};
	
	// Update is called once per frame
	private void Update() {
		GenerateTile();
	}


	private void GenerateTile() {
		var centerPoint = player.transform.position;
		const int gridSize = 50;
		
		newTileMap = new();

		for (var xPointIndex = Mathf.RoundToInt(centerPoint.x - gridSize / 2.0f);
		     xPointIndex < Mathf.RoundToInt(centerPoint.x + gridSize / 2.0f);
		     xPointIndex++)
		for (var yPointIndex = Mathf.RoundToInt(centerPoint.z - gridSize / 2.0f);
		     yPointIndex < Mathf.RoundToInt(centerPoint.z + gridSize / 2.0f);
		     yPointIndex++) {
			(float, float) key = (xPointIndex, yPointIndex);

			if (CheckIfExists(tileMap,newTileMap,key)) continue;

			var tileWallMap = new Dictionary<Direction, GameObject>();
			
			
			foreach (var (direction, offset) in directionOffsets) {
				// [airPercent] chance of air
				if (!(Random.Range(0.0f, 1.0f) >= airPercent/100)) continue;
				
				// Make sure there is not a room next door already with a wall
				(float, float) neighbourTileKey = (key.Item1 + offset.x, key.Item2 + offset.y);
				if (newTileMap.ContainsKey(neighbourTileKey)) {
					var neighbourTile = newTileMap[neighbourTileKey];
					if (neighbourTile.ContainsKey(FlipDirection(direction))) continue;
				}

				// Empty Spot
				var rotation = Quaternion.identity;
				rotation *= Quaternion.Euler(0, offset.y != 0 ? 90 : 0, 0);
				var finalVector3 = new Vector3(xPointIndex + offset.x, 0.5f, yPointIndex + offset.y);
				if (!tileMap.ContainsKey(key)) {
					tileWallMap.Add(direction, Instantiate(wallPrefab, finalVector3, rotation));
				}
			}
			newTileMap.Add(key, tileWallMap);		
		}
		
		//Debug.Log("New map count" + newTileMap.Count);
		//Debug.Log($"Old map count {tileMap.Count}");
		
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

	private bool CheckIfExists(TileMapStore oldTileMap, TileMapStore newTileMap,(float,float) key)
	{
		return newTileMap.ContainsKey(key);
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