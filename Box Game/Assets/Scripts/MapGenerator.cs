using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Security;
using UnityEngine;
using UnityEditor.UIElements;
using System.Runtime.Serialization;
using System.Globalization;
using UnityEngine.Tilemaps;

public class MapGenerator : MonoBehaviour
{
    [System.Serializable]
    public struct Coord
    {
        public int x;
        public int y;

        public Coord(int _x, int _y)
        {
            x = _x;
            y = _y;
        }

        public bool isNotEqual(Coord center)
        {
            if (this.x == center.x && this.y == center.y)
                return false;
            return true;
        }
    }

    [System.Serializable]
    public class Map
    {
        public Coord mapSize;
        public int seed;

        [Range(0, 1)]
        public float obstaclePercentage;
        public float minObstacleHeight;
        public float maxObstacleHeight;
        public Color foregroundColor;
        public Color backgroundColor;

        public Coord mapCentre()
        {
            return new Coord(mapSize.x / 2, mapSize.y / 2);
        }
    }

    public Map[] Maps;
    public int MapIndex;
    Map currentMap;
    List<Coord> allTileCoordinates;
    Queue<Coord> shuffledTileCoordinates;
    Queue<Coord> shuffledOpenTileCoordinates;

    Transform[,] tileMap;

    public Transform tilePrefab;
    public Transform obstaclePrefab;
    public Transform mapBackFloor;
    public Transform mapFloor;
    public Transform navMeshMask;
    public Vector2 maxMapSize;

    [Range(0, 0.3f)]
    public float outlinePercentage;

    public float tileSize;

    private void Awake()
    {
        FindObjectOfType<Spawner>().onNewWave += onNewWave;
        //generateMap();
    }

    void onNewWave(int waveNumber)
    {
        MapIndex = waveNumber - 1;
        generateMap();
    }

    public Coord RandomCoordinate()
    {
        Coord randomCoord = shuffledTileCoordinates.Dequeue();
        shuffledTileCoordinates.Enqueue(randomCoord);
        return randomCoord;
    }

    public Transform getRandomOpenTile()
    {
        Coord randomCoord = shuffledOpenTileCoordinates.Dequeue();
        shuffledOpenTileCoordinates.Enqueue(randomCoord);
        return tileMap[randomCoord.x, randomCoord.y];
    }

    Vector3 coordToPosition(int x, int y)
    {
        return new Vector3(
                -currentMap.mapSize.x / 2f + 0.5f + x,
                0,
                -currentMap.mapSize.y / 2f + 0.5f + y
            ) * tileSize;
    }

    public Transform getTileFromPosition(Vector3 position)
    {
        int x = Mathf.RoundToInt(position.x / tileSize + (currentMap.mapSize.x - 1) / 2f);
        int y = Mathf.RoundToInt(position.z / tileSize + (currentMap.mapSize.y - 1) / 2f);

        x = Mathf.Clamp(x, 0, tileMap.GetLength(0) - 1);
        y = Mathf.Clamp(y, 0, tileMap.GetLength(1) - 1);

        return tileMap[x, y];
    }

    public void generateMap()
    {
        currentMap = Maps[MapIndex%5];
        System.Random prng = new System.Random(currentMap.seed);
       

        tileMap = new Transform[currentMap.mapSize.x, currentMap.mapSize.y];

        //generating coords
        allTileCoordinates = new List<Coord>();

        for (int i = 0; i < currentMap.mapSize.x; i++)
            for (int j = 0; j < currentMap.mapSize.y; j++)
                allTileCoordinates.Add(new Coord(i, j));

        shuffledTileCoordinates = new Queue<Coord>(
            Utility.ShuffleArray(allTileCoordinates.ToArray(), currentMap.seed)
        );

        //creating map holder
        string holderName = "Generated Map";

        if (transform.Find(holderName))
            DestroyImmediate(transform.Find(holderName).gameObject);

        Transform mapHolder = new GameObject(holderName).transform;
        mapHolder.parent = transform;

        //spawning tiles
        for (int i = 0; i < currentMap.mapSize.x; i++)
        {
            for (int j = 0; j < currentMap.mapSize.y; j++)
            {
                Vector3 tilePosition = coordToPosition(i, j);

                Transform newTile =
                    Instantiate(tilePrefab, tilePosition, Quaternion.Euler(Vector3.right * 90))
                    as Transform;
                newTile.localScale = Vector3.one * (1 - outlinePercentage) * tileSize;
                newTile.parent = mapHolder;
                tileMap[i, j] = newTile;
            }
        }

        //spawning obstacles
        int obstacleCount = (int)(
            currentMap.mapSize.x * currentMap.mapSize.y * currentMap.obstaclePercentage
        );
        int currentObstacleCount = 0;

        List<Coord> allOpenCoordinates = new List<Coord>(allTileCoordinates);

        bool[,] obstacleMap = new bool[(int)currentMap.mapSize.x, (int)currentMap.mapSize.y];

        for (int i = 0; i < obstacleCount; i++)
        {
            Coord randomCoord = RandomCoordinate();
            currentObstacleCount++;
            obstacleMap[randomCoord.x, randomCoord.y] = true;

            if (
                randomCoord.isNotEqual(currentMap.mapCentre())
                && mapFullyAccessible(obstacleMap, currentObstacleCount)
            )
            {
                float obstacleHeight = Mathf.Lerp(
                    currentMap.minObstacleHeight,
                    currentMap.maxObstacleHeight,
                    (float)prng.NextDouble()
                );
                Vector3 obstaclePosition = coordToPosition(randomCoord.x, randomCoord.y);

                Transform newObstacle =
                    Instantiate(
                        obstaclePrefab,
                        obstaclePosition + Vector3.up * obstacleHeight / 2f,
                        Quaternion.identity
                    ) as Transform;
                newObstacle.parent = mapHolder;

                Renderer obstacleRenderer = newObstacle.GetComponent<Renderer>();
                Material obstacleMaterial = new Material(obstacleRenderer.sharedMaterial);

                float colorPercentage = randomCoord.y / (float)currentMap.mapSize.y;
                obstacleMaterial.color = Color.Lerp(
                    currentMap.backgroundColor,
                    currentMap.foregroundColor,
                    colorPercentage
                );
                obstacleRenderer.sharedMaterial = obstacleMaterial;

                newObstacle.localScale = new Vector3(
                    ((1 - outlinePercentage) * tileSize),
                    obstacleHeight,
                    ((1 - outlinePercentage) * tileSize)
                );

                allOpenCoordinates.Remove(randomCoord);
            }
            else
            {
                currentObstacleCount--;
                obstacleMap[randomCoord.x, randomCoord.y] = false;
            }
        }

        shuffledOpenTileCoordinates = new Queue<Coord>(
            Utility.ShuffleArray(allOpenCoordinates.ToArray(), currentMap.seed)
        );
        mapFloor.localScale = new Vector3(maxMapSize.x, maxMapSize.y) * tileSize;
        mapBackFloor.localScale=new Vector3(currentMap.mapSize.x*tileSize,currentMap.mapSize.y*tileSize);

        //creating navmesh masks

        Transform maskLeft =
            Instantiate(
                navMeshMask,
                Vector3.left * (currentMap.mapSize.x + maxMapSize.x) / 4f * tileSize,
                Quaternion.identity
            ) as Transform;
        maskLeft.parent = mapHolder;
        maskLeft.localScale =
            new Vector3((maxMapSize.x - currentMap.mapSize.x) / 2f, 1, currentMap.mapSize.y)
            * tileSize;

        Transform maskRight =
            Instantiate(
                navMeshMask,
                Vector3.right * (currentMap.mapSize.x + maxMapSize.x) / 4f * tileSize,
                Quaternion.identity
            ) as Transform;
        maskRight.parent = mapHolder;
        maskRight.localScale =
            new Vector3((maxMapSize.x - currentMap.mapSize.x) / 2f, 1, currentMap.mapSize.y)
            * tileSize;

        Transform maskTop =
            Instantiate(
                navMeshMask,
                Vector3.forward * (currentMap.mapSize.y + maxMapSize.y) / 4f * tileSize,
                Quaternion.identity
            ) as Transform;
        maskTop.parent = mapHolder;
        maskTop.localScale =
            new Vector3(maxMapSize.x, 1, (maxMapSize.y - currentMap.mapSize.y) / 2f) * tileSize;

        Transform maskBottom =
            Instantiate(
                navMeshMask,
                Vector3.back * (currentMap.mapSize.y + maxMapSize.y) / 4f * tileSize,
                Quaternion.identity
            ) as Transform;
        maskBottom.parent = mapHolder;
        maskBottom.localScale =
            new Vector3(maxMapSize.x, 1, (maxMapSize.y - currentMap.mapSize.y) / 2f) * tileSize;
    }

    bool mapFullyAccessible(bool[,] obstacleMap, int obstacleCount)
    {
        bool[,] mapFlags = new bool[obstacleMap.GetLength(0), obstacleMap.GetLength(1)];
        Queue<Coord> queue = new Queue<Coord>();
        queue.Enqueue(currentMap.mapCentre());

        mapFlags[currentMap.mapCentre().x, currentMap.mapCentre().y] = true;

        int accessibleTileCount = 1;
        while (queue.Count > 0)
        {
            Coord tile = queue.Dequeue();

            for (int x = -1; x <= 1; x++)
                for (int y = -1; y <= 1; y++)
                {
                    int neighbourX = tile.x + x;
                    int neighbourY = tile.y + y;

                    if (x == 0 || y == 0)
                    {
                        if (
                            neighbourX >= 0
                            && neighbourX < obstacleMap.GetLength(0)
                            && neighbourY >= 0
                            && neighbourY < obstacleMap.GetLength(1)
                        )
                        {
                            if (
                                !mapFlags[neighbourX, neighbourY]
                                && !obstacleMap[neighbourX, neighbourY]
                            )
                            {
                                mapFlags[neighbourX, neighbourY] = true;
                                queue.Enqueue(new Coord(neighbourX, neighbourY));
                                accessibleTileCount++;
                            }
                        }
                    }
                }
        }

        int targetAccessibleTileCount = (int)(
            currentMap.mapSize.x * currentMap.mapSize.y - obstacleCount
        );
        return targetAccessibleTileCount == accessibleTileCount;
    }
}
