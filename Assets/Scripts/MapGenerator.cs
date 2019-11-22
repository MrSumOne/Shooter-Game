using UnityEngine;
using System.Collections.Generic;

public class MapGenerator : MonoBehaviour
{
    public Map[] maps;
    Map currentMap;
    public int mapIndex;

    public Transform tilePrefab;
    public Vector2 maxMapSize;
    public Transform obsticlePrefab;
    public Transform navmeshFloor;
    public Transform navmeshMaskPrefab;

    [Range(0, .3f)]
    public float outlinePercent;
 
    public float tileSize;

    List<Coord> allTileCoords;
    Queue<Coord> allShuffledTileCoords;
    Queue<Coord> allShuffledOpenTileCoords;
    Transform[,] tileMap;

    public void Start()
    {
        FindObjectOfType<Spawner>().OnNewWave += OnNewWave;
    }

    void OnNewWave(int waveNumber)
    {
        mapIndex = waveNumber - 1;
        GenerateMap();
    }

    public void GenerateMap()
    {
        currentMap = maps[mapIndex];

        System.Random prng = new System.Random(currentMap.seed);

        GetComponent<BoxCollider>().size = new Vector3(currentMap.mapSize.x * tileSize, 0.05f, currentMap.mapSize.y * tileSize);

        tileMap = new Transform[currentMap.mapSize.x,currentMap.mapSize.y];

        //Generating coords
        allTileCoords = new List<Coord>();

        //Create map holder object
        string holderName = "Generated Map";

        //delete the map holder if it already exists before making a new one
        if (transform.Find(holderName))
        {
            DestroyImmediate(transform.Find(holderName).gameObject);
        }

        //create the map holder as a parent of the map object that this script is on
        Transform mapHolder = new GameObject(holderName).transform;
        mapHolder.parent = transform;

        //instantiate the tiles according to the map size
        for (int x = 0; x < currentMap.mapSize.x; x++)
        {
            for (int y = 0; y < currentMap.mapSize.y; y++)
            {
                Vector3 tilePosition = CoordToPosition(x, y);
                Transform newTile = Instantiate(tilePrefab, tilePosition, Quaternion.Euler(Vector3.right * 90));
                newTile.localScale = Vector3.one * (1 - outlinePercent) * tileSize;
                newTile.parent = mapHolder;
                allTileCoords.Add(new Coord(x, y));
                tileMap[x, y] = newTile;
            }
        }

        //find the centre of the map moved to Map class
        //mapCentre = new Coord((int)currentMap.mapSize.x / 2, (int)currentMap.mapSize.y / 2);

        //shuffle the list of all the tiles into a queue (FIFO), save for the method GetRandomCoord
        allShuffledTileCoords = new Queue<Coord>(Utility.ShuffleArray(allTileCoords.ToArray(), currentMap.seed));

        //create a 2D bool the size of the map to keep track of where the obsticles are
        bool[,] obsticleMap = new bool[(int)currentMap.mapSize.x, (int)currentMap.mapSize.y];

        //put obsticles down randomly, using the shuffled coordinates via GetRandomCoord
        int obsticleCount = (int)(currentMap.mapSize.x * currentMap.mapSize.y * currentMap.obstaclePercent);
        int currentObstacleCount = 0;

        List<Coord> allOpenCoords = new List<Coord>(allTileCoords);

        //the loop for creating obsticles
        for (int i = 0; i < obsticleCount; i++)
        {
            //pull out the first coordinate from the shuffled queue
            Coord randomCoord = GetRandomCoord();
            //set the bool to true on that coordinate
            obsticleMap[randomCoord.x, randomCoord.y] = true;
            currentObstacleCount++;

            //if the random tile selected is not the map center and passes the MapIsFullyAssessible check
            if (randomCoord != currentMap.mapCentre && MapIsFullyAccessible(obsticleMap, currentObstacleCount))
            {
                float obstacleHeight = Mathf.Lerp(currentMap.minObstacleHeight, currentMap.maxObstacleHeight, (float)prng.NextDouble());
                //create the obsticle at the random coordinate
                Vector3 obsticlePosition = CoordToPosition(randomCoord.x, randomCoord.y);
                Transform newObsticle = Instantiate(obsticlePrefab, obsticlePosition + Vector3.up * obstacleHeight/2, Quaternion.identity);
                newObsticle.localScale = new Vector3((1 - outlinePercent) * tileSize, obstacleHeight, (1 - outlinePercent) * tileSize);
                newObsticle.parent = mapHolder;

                //change the colours
                Renderer obstacleRenderer = newObsticle.GetComponent<Renderer>();
                Material obstacleMaterial = new Material(obstacleRenderer.sharedMaterial);
                float colourPercent = randomCoord.y / (float)currentMap.mapSize.y;
                obstacleMaterial.color = Color.Lerp(currentMap.foregroundColour, currentMap.backgoundColour, colourPercent);
                obstacleRenderer.sharedMaterial = obstacleMaterial;

                allOpenCoords.Remove(randomCoord);
            }
            //if it fails, set the bool on that coordinate to false, and decrease the obstacle count
            else
            {
                obsticleMap[randomCoord.x, randomCoord.y] = false;
                currentObstacleCount--;
            }
        }

        allShuffledOpenTileCoords = new Queue<Coord>(Utility.ShuffleArray(allOpenCoords.ToArray(), currentMap.seed));

        //changing the size of the navmesh
        navmeshFloor.localScale = new Vector3(maxMapSize.x, maxMapSize.y) * tileSize;

        //create navmesh borders
        Transform maskLeft = Instantiate(navmeshMaskPrefab, Vector3.left * (currentMap.mapSize.x + maxMapSize.x) / 4f * tileSize, Quaternion.identity) as Transform;
        maskLeft.parent = mapHolder;
        maskLeft.localScale = new Vector3((maxMapSize.x - currentMap.mapSize.x) / 2f, 1, currentMap.mapSize.y) * tileSize;

        Transform maskRight = Instantiate(navmeshMaskPrefab, Vector3.right * (currentMap.mapSize.x + maxMapSize.x) / 4f * tileSize, Quaternion.identity) as Transform;
        maskRight.parent = mapHolder;
        maskRight.localScale = new Vector3((maxMapSize.x - currentMap.mapSize.x) / 2f, 1, currentMap.mapSize.y) * tileSize;

        Transform maskTop = Instantiate(navmeshMaskPrefab, Vector3.forward * (currentMap.mapSize.y + maxMapSize.y) / 4f * tileSize, Quaternion.identity) as Transform;
        maskTop.parent = mapHolder;
        maskTop.localScale = new Vector3(maxMapSize.x, 1, (maxMapSize.y - currentMap.mapSize.y) /2f) * tileSize;

        Transform maskBottom = Instantiate(navmeshMaskPrefab, Vector3.back * (currentMap.mapSize.y + maxMapSize.y) / 4f * tileSize, Quaternion.identity) as Transform;
        maskBottom.parent = mapHolder;
        maskBottom.localScale = new Vector3(maxMapSize.x, 1, (maxMapSize.y - currentMap.mapSize.y) / 2f) * tileSize;
    }

    //change the coordinate of the tile to a vector and place it according to where the map is...
    //So... the corner of the map (-currentMap.mapSize.x and y) and move it to the edge, instead of the center (+0.5)
    //and move it along for each iteration (+x and y)
    Vector3 CoordToPosition(int x, int y)
    {
        return new Vector3(-currentMap.mapSize.x / 2f + 0.5f + x, 0, -currentMap.mapSize.y / 2f + 0.5f + y) * tileSize;
    }

    //We can get the position with the coord, as above, now we want to do the opposite, so we can see where the player is standing
    public Transform GetTileFromPosition(Vector3 position)
    {
        int x = Mathf.RoundToInt(position.x /tileSize + (currentMap.mapSize.x - 1) / 2f);
        int y = Mathf.RoundToInt(position.z / tileSize + (currentMap.mapSize.y - 1) / 2f);
        x = Mathf.Clamp(x, 0, tileMap.GetLength(0) - 1);
        y = Mathf.Clamp(y, 0, tileMap.GetLength(1) - 1);

        return tileMap[x, y];
    }

    bool MapIsFullyAccessible(bool[,] obsticleMap, int currentObsticleCount)
    {
        //create a 2D bool to keep track of which tiles we have already checked
        bool[,] mapFlags = new bool[obsticleMap.GetLength(0), obsticleMap.GetLength(1)];
        //create a queue of coordinates to use when we go through the loop of checking the tiles
        Queue<Coord> queue = new Queue<Coord>();
        //add the first tile to the queue, the center tile, which should always be empty
        queue.Enqueue(currentMap.mapCentre);
        //and mark that tile as checked
        mapFlags[currentMap.mapCentre.x, currentMap.mapCentre.y] = true;

        //initialize the tile count of accessable tiles
        int accessableTileCount = 1;

        //start the loop to check all the tiles. While the queue isn't empty
        while (queue.Count > 0)
        {
            //take the first tile out of the queue
            Coord tile = queue.Dequeue();

            //we're going to be checking -1 and +1 in x and y, which covers all the tiles around the selected tile
            for (int x = -1; x <= 1; x++)
            {
                for (int y = -1; y <= 1; y++)
                {
                    int neighbourX = tile.x + x;
                    int neighbourY = tile.y + y;

                    //we only want to check the ones directly horizontal or verticle, not diagonal, so we use a XOR
                    if (x == 0 ^ y == 0)
                    {
                        //if we reach -1 or > the length of the map then we've fallen off, so check for that so we're not out of bounds
                        if (neighbourX >= 0 && neighbourX < obsticleMap.GetLength(0) && neighbourY >= 0 && neighbourY < obsticleMap.GetLength(1))
                        {
                            //if the tile hasn't been checked and is not an obsticle
                            if (!mapFlags[neighbourX, neighbourY] && !obsticleMap[neighbourX, neighbourY])
                            {
                                //add it to the checked tiles
                                mapFlags[neighbourX, neighbourY] = true;
                                //put the checked tile into the queue so all of it's neighbours can be checked
                                queue.Enqueue(new Coord(neighbourX, neighbourY));
                                //add one to the count
                                accessableTileCount++;
                            }
                        }
                    }
                }
            }
        }


        //the target accessible tiles we want will be the map size minus the ammount of obsticles
        int targetAccessibleTileCount = (int)(currentMap.mapSize.x * currentMap.mapSize.y - currentObsticleCount);
        //so the loop will check that the tiles that it can access is equal to the number of empty tiles every time an obstacle is added
        return targetAccessibleTileCount == accessableTileCount;
    }

    public Coord GetRandomCoord()
    {
        //take the first random coord...
        Coord randomCoord = allShuffledTileCoords.Dequeue();
        //and put it in the back of the line so it's not picked again
        allShuffledTileCoords.Enqueue(randomCoord);
        //return that coord
        return randomCoord;
    }

    public Transform GetRandomOpenTile()
    {
        Coord randomCoord = allShuffledOpenTileCoords.Dequeue();
        allShuffledOpenTileCoords.Enqueue(randomCoord);
        return tileMap[randomCoord.x, randomCoord.y];
    }

    //simple struct to hold coordinates
    [System.Serializable]
    public struct Coord
    {
        public int x, y;

        //init the coord
        public Coord(int _x, int _y)
        {
            x = _x;
            y = _y;
        }

        //we have to tell the struct how to deal with equals and not equals operators
        public static bool operator ==(Coord c1, Coord c2)
        {
            return c1.x == c2.x && c1.y == c2.y;
        }

        public static bool operator !=(Coord c1, Coord c2)
        {
            return !(c1 == c2);
        }
    }

    //a class to hold the map information
    [System.Serializable]
    public class Map {
        public Coord mapSize;
        [Range(0, 1)]
        public float obstaclePercent;
        public int seed;
        public float minObstacleHeight;
        public float maxObstacleHeight;
        public Color foregroundColour;
        public Color backgoundColour;

        public Coord mapCentre
        {
            get
            {
                return new Coord(mapSize.x / 2, mapSize.y / 2);
            }
        }
    }
}
