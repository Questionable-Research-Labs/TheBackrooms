using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using Random = UnityEngine.Random;

public class WallSpawner : MonoBehaviour
{
    [SerializeField] public GameObject wallPrefab;
    [SerializeField] public GameObject smallWallPrefab;
    [SerializeField] public GameObject largeWallPrefab;
    [SerializeField] public GameObject gapWallPrefab;
    [SerializeField] public GameObject doorWallPrefab;
    [SerializeField] public GameObject halfWallPrefab;
    [SerializeField] private float airPercent;
    [SerializeField] public GameObject player;
    
    public enum Direction {
        North,
        South,
        East,
        West
    }
    
    public class TileMapStore : Dictionary<(float, float), Dictionary<Direction, GameObject>> {}
    
    private TileMapStore tileMap = new();
    private TileMapStore newTileMap = new();
    
    private readonly Dictionary<Direction,Vector2> directionOffsets = new() {
        { Direction.North, Vector2.up * 4},
        { Direction.South, Vector2.down * 4},
        { Direction.East, Vector2.left * 4},
        { Direction.West, Vector2.right * 4},
    };
    
    void Update()
    {
        GenerateTile();
    }

    private void GenerateTile()
    {
	    // Set grid size and center
	    var position = player.transform.position;
	    var centerPointX = (int)Math.Round((position.x / (double) 4), MidpointRounding.AwayFromZero) * 4;
	    var centerPointZ = (int)Math.Round((position.z / (double)4),MidpointRounding.AwayFromZero) * 4;
		const int gridSize = 100;
		
		newTileMap = new();

		// For each point...
		for (var xPointIndex = Mathf.RoundToInt(centerPointX - gridSize / 2.0f);
		     xPointIndex < Mathf.RoundToInt(centerPointX + gridSize / 2.0f);
		     xPointIndex += 4)
		for (var yPointIndex = Mathf.RoundToInt(centerPointZ - gridSize / 2.0f);
		     yPointIndex < Mathf.RoundToInt(centerPointZ + gridSize / 2.0f);
		     yPointIndex += 4) {
			(float, float) key = (xPointIndex, yPointIndex);

			// If the point already exists on the old map, don't re-do it
			if (CheckIfExists(tileMap,newTileMap,key)) continue;
			
			var tileWallMap = new Dictionary<Direction, GameObject>();
			
			foreach (var (direction, offset) in directionOffsets) {
				// [airPercent] chance of air

				if (!(Random.Range(0.0f, 1.0f) >= airPercent/100)) continue;

				// bool requireWall = CheckNeighboringTiles(key);
				
				// Make sure there is not a room next door already with a wall
				(float, float) neighbourTileKey = (key.Item1 + offset.x, key.Item2 + offset.y);
				if (newTileMap.ContainsKey(neighbourTileKey)) {
					var neighbourTile = newTileMap[neighbourTileKey];
					if (neighbourTile.ContainsKey(FlipDirection(direction))) continue;
				}
				
				// Empty Spot
				var rotation = Quaternion.identity;
				rotation *= Quaternion.Euler(0, offset.y != 0 ? 90 : 0, 0);
				var finalVector3 = new Vector3(xPointIndex, 1.5f, yPointIndex);
				if (!tileMap.ContainsKey(key))
				{
					// Add wall
					var wall = ChooseWallVariant();
					tileWallMap.Add(direction, Instantiate(wall, finalVector3, rotation));
				}
			}
			newTileMap.Add(key, tileWallMap);		
		}

		// Generated tiles, now lets remove old ones
		// Add new walls to the tileMap
		foreach (var (point, walls) in newTileMap)
		{
			if (!tileMap.ContainsKey(point))
			{
				tileMap.Add(point,walls);
			}
		}
		
		// Delete the cringe walls
		List<(float,float)> vaishPoints = new();
		foreach (var (point, walls) in tileMap) {
			if (!newTileMap.ContainsKey(point)) {
				foreach (var wallObject in walls.Values) {
					Destroy(wallObject);
				}
				vaishPoints.Add(point);
			}
		}
		
		// Make unnecessary points vaish
		vaishPoints.ForEach(point => tileMap.Remove(point));
    }

    /*
    private bool CheckNeighboringTiles((float, float) key)
    {
	    foreach (var (direction, offset) in directionOffsets)
	    {
		    (float, float) neighbourTileKey = (key.Item1 + offset.x, key.Item2 + offset.y);
		    if (newTileMap.ContainsKey(neighbourTileKey)) {
			    var neighbourTile = newTileMap[neighbourTileKey];
			    if(neighbourTile[direction] == null) continue;
			    if(neighbourTile[direction].name.StartsWith("door") | neighbourTile[direction].name.StartsWith("gap"))
			    {
				    Debug.Log("door");
				    return true;
			    }
		    }
	    }

	    return false;
    }
    */
    
    private GameObject ChooseWallVariant()
    {
	    // Yes I know this is inefficient, it's basically AI at this point 
	    float randomValue = Random.Range(0.0f, 1f);
	    if (randomValue <= 0.75){return wallPrefab;}
	    if (randomValue is <= 0.8f and > 0.75f){return doorWallPrefab;}
	    if (randomValue is <= 0.85f and > 0.8f){return gapWallPrefab;}
	    if (randomValue is <= 0.9f and > 0.85f){return halfWallPrefab;}
	    if (randomValue is <= 0.95f and > 0.9f){return smallWallPrefab;}
	    if (randomValue is <= 1.0f and > 0.95f){return largeWallPrefab;}
	    return wallPrefab;
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
