using System.Collections.Generic;
using UnityEngine;

public class FloorGen : MonoBehaviour
{
    class TileMapStore : Dictionary<(float, float), GameObject>
    {
    }

    [SerializeField] public GameObject floorPrefab;

    private readonly TileMapStore _currentGeneratedTiles = new();

    public int gridSize = 6;


    // Start is called before the first frame update
    void Start()
    {
        InvokeRepeating(nameof(GenerateTile), 0, 0.177013f);
    }

    private void GenerateTile()
    {
        var centerPoint = transform.position;

        var newTileMap = new TileMapStore();

        for (var xPointIndex = Mathf.RoundToInt(centerPoint.x - gridSize / 2.0f);
             xPointIndex < Mathf.RoundToInt(centerPoint.x + gridSize / 2.0f);
             xPointIndex+=10)
        for (var yPointIndex = Mathf.RoundToInt(centerPoint.z - gridSize / 2.0f);
             yPointIndex < Mathf.RoundToInt(centerPoint.z + gridSize / 2.0f);
             yPointIndex+=10)
        {
            (float, float) key = (xPointIndex, yPointIndex);


            // Empty Spot
            var finalVector3 = new Vector3(xPointIndex, 0.0f, yPointIndex);
            bool alreadyExists = _currentGeneratedTiles.ContainsKey(key);
            GameObject tileObject =
                alreadyExists ? _currentGeneratedTiles[key] : Instantiate(floorPrefab, finalVector3, Quaternion.identity);
            newTileMap.Add(key, tileObject);

            // Generated tiles, now lets remove old ones
            foreach (var (point, walls) in newTileMap)
            {
                if (!_currentGeneratedTiles.ContainsKey(point))
                {
                    _currentGeneratedTiles.Add(point, walls);
                }
            }

            List<(float, float)> pointsToYeet = new();
            foreach (var (point, floorObject) in _currentGeneratedTiles)
            {
                if (!newTileMap.ContainsKey(point))
                {
                    Destroy(floorObject);
                    pointsToYeet.Add(point);
                }
            }

            pointsToYeet.ForEach(point => _currentGeneratedTiles.Remove(point));
        }
    }
}