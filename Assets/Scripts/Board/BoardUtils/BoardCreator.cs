using System;
using System.Collections.Generic;
using UnityEngine;

public class BoardCreator
{
    // public EntityE[] TileOrder = { EntityE.OceanTile, EntityE.BayTile, EntityE.PlainsTile, EntityE.GrassTile, EntityE.ForestTile };
    public const int boardSize = 256;
    public static Board CreateBoardFromSeed(int seed){
        DebugPerformance.CreateSegment("Board State Init");
        Board board = new(boardSize, seed);
        BoardState boardState = board.Current; // 0 on init
        CoreRandom random = new(seed);
        int spawnerSeed = random.Next(int.MaxValue);


        //! null monodeltabind, since it's generated from seed anyways
        boardState.InjectMonoDelta(null, null);

        int[,] map = SaveFile.LoadEncryptedMap(seed);
        if (map == null){
            SaveFile.CacheEncryptedMap(seed);
            map = SaveFile.LoadEncryptedMap(seed);
        }


        DebugPerformance.CreateSegment("Island Generation");
        float grayscaleAmount = 0.5f;
        float variation = 0.2f;
        for (int x = 0; x < boardSize; x++){
            for (int y = 0; y < boardSize; y++){
                int tileType = map[x, y];
                HexVector HexPos = (HexVector)new GridVector(x, y);
                // grayscale
                Color color = BiomeProperties.Colors[(Biome)tileType];
                float grayscale = (color.r + color.g + color.b) / 3;
                float randomValue = (float)random.NextDouble();
                grayscale += (float)randomValue * variation - variation / 2;
                grayscale = Mathf.Clamp01(grayscale);
                color.r = grayscale * grayscaleAmount + color.r * (1 - grayscaleAmount);
                color.g = grayscale * grayscaleAmount + color.g * (1 - grayscaleAmount);
                color.b = grayscale * grayscaleAmount + color.b * (1 - grayscaleAmount);

                Biome biome = (Biome)tileType;

                EntityEnum tileName = Enum.Parse<EntityEnum>(biome.ToString() + "Tile");
                Tile testTile = Entity.Get<Tile>(tileName);
                testTile.Initialize(HexPos);

                board.Elevation[x, y] = BiomeToElevation((Biome)tileType);

                boardState.AddEntity(testTile);
            }
        }

        // for (int i = 0; i < 5; i++)
        // {
        //     for (int failSafe = 0; failSafe < 1000; failSafe++){
        //         int x = random.Next(boardSize);
        //         int y = random.Next(boardSize);
        //         HexVector hex = (HexVector)new GridVector(x, y);
        //         if(board.Current.TileBoard[x,y].IsWalkable() && board.Current.GetEntity<DeltaEntity>(hex) == null){
        //             boardState.AddEntity(Entity.Get<Church>().Initialize((HexVector)new GridVector(x, y)));
        //             break;
        //         }
        //     }
        // }

        // for (int i = 0; i < 10; i++)
        // {
        //     for (int failSafe = 0; failSafe < 1000; failSafe++){
        //         int x = random.Next(boardSize);
        //         int y = random.Next(boardSize);
        //         HexVector hex = (HexVector)new GridVector(x, y);
        //         if(board.Current.TileBoard[x,y].IsWalkable() && board.Current.GetEntity<DeltaEntity>(hex) == null){
        //             if (random.Next(2) == 0)
        //                 boardState.AddEntity(Entity.Get<Angel>().Initialize((HexVector)new GridVector(x, y)));
        //             else
        //                 boardState.AddEntity(Entity.Get<ArtiliaryMech>().Initialize((HexVector)new GridVector(x, y)));
        //             break;
        //         }
        //     }
        // }

        // for (int i = 0; i < 50; i++)
        // {
        //     for (int failSafe = 0; failSafe < 1000; failSafe++){
        //         int x = random.Next(boardSize);
        //         int y = random.Next(boardSize);
        //         HexVector hex = (HexVector)new GridVector(x, y);
        //         if(board.Current.TileBoard[x,y].IsWalkable() && board.Current.GetEntity<DeltaEntity>(hex) == null){
        //             boardState.AddEntity(Entity.Get<Village>().Initialize((HexVector)new GridVector(x, y)));
        //             break;
        //         }
        //     }
        // }

        boardState.AddEntity(Entity.Get<HadicSpawner>().Initialize(spawnerSeed));
        boardState.AddEntity(Entity.Get<UserSpawner>().Initialize());
        boardState.AddEntity(Entity.Get<Economy>().Initialize());

        DebugPerformance.CreateSegment("Entity Generation");


        //* IMPORTANT
        InitTiles(boardState.TileBoard, board);
        DebugPerformance.CreateSegment("Init Tiles");

        boardState.ManualSoftInstantiate();
        BoardHistory.AddBoard(new BoardDelta(boardState.BruteHash(), boardState.GetEntityDeltas()), new List<MonoDelta>(), false);
        board.SetWorkingBoard(1);
        board.CreateBaseStatesRaw();

        DebugPerformance.CreateSegment("Board History");
        return board;
    }
    
    // Init Masses and Launch Points
    private static void InitTiles(Tile[,] tiles, Board board){
        // Masses
        List<bool[,]> landMasses = new();
        List<bool[,]> waterMasses = new();
        int boardSize = tiles.GetLength(0);
        bool[,] visited = new bool[boardSize, boardSize];
        for (int x = 0; x < boardSize; x++){
            for (int y = 0; y < boardSize; y++){
                if(visited[x, y])
                    continue;
                bool[,] mass = new bool[boardSize, boardSize];
                if(tiles[x, y].IsWalkable()){
                    FloodFill(tiles, x, y, visited, mass, true);
                    landMasses.Add(mass);
                }
                else{
                    FloodFill(tiles, x, y, visited, mass, false);
                    waterMasses.Add(mass);
                }
            }
        }
        for (int i = 0; i < waterMasses.Count; i++)
            for (int x = 0; x < boardSize; x++)
                for (int y = 0; y < boardSize; y++)
                    if(waterMasses[i][x, y])
                        board.MassNumber[x, y] = i;

        for (int i = 0; i < landMasses.Count; i++)
            for (int x = 0; x < boardSize; x++)
                for (int y = 0; y < boardSize; y++)
                    if(landMasses[i][x, y])
                        board.MassNumber[x, y] = i + waterMasses.Count;

        // Launch Points

        // ! Could be a bug where a landmass creates two oceans.
        bool[,] ocean = waterMasses[0]; // ? first should always be ocean


        for (int i = 0; i < landMasses.Count; i++){
            Dictionary<HexVector, List<HexVector>> costalTilesCache = new();
            Dictionary<HexVector, List<HexVector>> landCostalTilesCache = new();

            HexVector startCoast = new();
            for (int x = 0; x < boardSize; x++)
                for (int y = 0; y < boardSize; y++)
                    if(landMasses[i][x, y]){
                        List<HexVector> neighbors = GetCostalNeighbors((HexVector)new GridVector(x, y), ocean);
                        if(neighbors.Count > 0){
                            startCoast = (HexVector)new GridVector(x, y);
                            costalTilesCache.Add(startCoast, neighbors);
                            landCostalTilesCache.Add(startCoast, GetLandCostalNeighbors(startCoast, landMasses[i], ocean));
                        }
                    }
            
            // this algorithm will look at shared water tiles to act as the costal normal
            LinkedList<HexVector> linkedCostal = new();
            HashSet<HexVector> addedCoasts = new();
            int failSafe = tiles.GetLength(0) * 4;

            linkedCostal.AddFirst(startCoast);
            addedCoasts.Add(startCoast);
            List<HexVector> biasedVectors = new();
            while (failSafe > 0){
                failSafe--;
                HexVector current = linkedCostal.Last.Value;
                List<HexVector> neighbors1All = costalTilesCache[current];
                List<HexVector> neighbors1 = neighbors1All;
                HexVector bias = IntersectionOfLists(neighbors1, biasedVectors);
                if(bias.x != int.MaxValue){
                    List<HexVector> neighbors1BiasFlood = new(){bias};
                    for (int j = 0; j < neighbors1BiasFlood.Count; j++){
                        foreach (HexVector neighborFlood in neighbors1All)
                            if(HexVector.Distance(neighbors1BiasFlood[j], neighborFlood) == 1 && !neighbors1BiasFlood.Contains(neighborFlood)){
                                neighbors1BiasFlood.Add(neighborFlood);
                                j--;
                                break;
                            }
                    }

                    // foreach (Vector2Int neighbor in neighbors1BiasFlood)
                    //     ((TestTile)tiles[HexBoard.HexToGrid(neighbor).x, HexBoard.HexToGrid(neighbor).y]).SetColor(new Color(0, 0, 1));
                    neighbors1 = neighbors1BiasFlood;
                }

                bool stuck = true;
                List<HexVector> landNeighbors = landCostalTilesCache[current];
                foreach (HexVector landNeighbor in landNeighbors){
                    if (addedCoasts.Contains(landNeighbor))
                        continue;
                    // check if shares costal tiles
                    bool sharesCostal = false;
                    List<HexVector> neighbors2 = costalTilesCache[landNeighbor];
                    foreach (HexVector neighbor in neighbors1)
                        if(neighbors2.Contains(neighbor)){
                            sharesCostal = true;
                            biasedVectors = new List<HexVector>();
                            foreach (HexVector neighbor2 in neighbors2) // intersection of neighbors1 and neighbors2
                                if(neighbors1.Contains(neighbor2))
                                    biasedVectors.Add(neighbor2);
                            break;
                        }
                    if(sharesCostal){
                        linkedCostal.AddLast(landNeighbor);
                        addedCoasts.Add(landNeighbor);
                        stuck = false;
                        break;
                    }
                }


                if(stuck){//! got lost on a peninsula, backtrack tiles until there is exists a tile with a land neighbor not used yet.

                    // * IF STUCK and contains the first tile, then it's a loop
                    if(landNeighbors.Contains(linkedCostal.First.Value)){
                        break;
                    }

                    bool found = false;
                    LinkedListNode<HexVector> node = linkedCostal.Last;
                    while (node != null){
                        HexVector last = node.Value;
                        List<HexVector> lastNeighbors = landCostalTilesCache[last];
                        foreach (HexVector lastNeighbor in lastNeighbors)
                            if(!addedCoasts.Contains(lastNeighbor)){
                                linkedCostal.AddLast(lastNeighbor);
                                addedCoasts.Add(lastNeighbor);
                                found = true;
                                break;
                            }
                        if(found)
                            break;
                        node = node.Previous;
                    }

                    if(!found){ // if still stuck, then it's a loop
                        break;
                    }
                }
            }

            ExposedLLNode<HexVector> exposedLLNode = null;
            ExposedLLNode<HexVector> head = null;
            foreach (HexVector item in linkedCostal){
                if(exposedLLNode == null){
                    exposedLLNode = new ExposedLLNode<HexVector>(item, null, null);
                    head = exposedLLNode;
                }
                else{
                    exposedLLNode.Next = new ExposedLLNode<HexVector>(item, null, exposedLLNode);
                    exposedLLNode = exposedLLNode.Next;
                }
            }
            exposedLLNode.Next = head;

            for (int x = 0; x < boardSize; x++){
                for (int y = 0; y < boardSize; y++){
                    if(landMasses[i][x, y]){
                        HexVector hex = (HexVector)new GridVector(x, y);
                        int closestDistance = int.MaxValue;
                        ExposedLLNode<HexVector> closestNode = null;
                        ExposedLLNode<HexVector> exposedList = head;
                        do{
                            int distance = HexVector.Distance(hex, exposedList.Value);
                            if(distance < closestDistance){
                                closestDistance = distance;
                                closestNode = exposedList;
                            }
                            exposedList = exposedList.Next;
                        }while (exposedList != head);
                        ((LandTile)tiles[x, y]).SetLaunchPoint(closestNode);
                        // ((TestTile)tiles[x, y]).SetColor(new Color(closestDistance / 40.0f, 0, 0));
                    }
                }
            }
        }


        // foreach (LinkedList<Vector2Int> costal in CostalTiles){
        //     int counter = 0;
        //     foreach (Vector2Int tile in costal){
        //         Vector2Int grid = HexBoard.HexToGrid(tile);
        //         ((TestTile)tiles[grid.x, grid.y]).SetColor(new Color(counter / (float)costal.Count, 0, 0));
        //         counter++;
        //     }
        // }

        // List<List<Vector2Int>> LaunchPoints = new List<List<Vector2Int>>();
        // for (int i = 0; i < landMasses.Count; i++){
        //     List<Vector2Int> costal = CostalTiles[i]; // in hexCords

        //     List<Vector2Int> launchPoints = new List<Vector2Int>();
        //     HashSet<Vector2Int> trackDuplicates = new HashSet<Vector2Int>();
        //     Vector2Int MinDist = costal[0];
        //     LaunchPoints.Add(launchPoints);
        // }
    }
    public static HexVector IntersectionOfLists(List<HexVector> list1, List<HexVector> list2){
        foreach (HexVector item in list1)
            if(list2.Contains(item))
                return item;
        return new HexVector(int.MaxValue, int.MaxValue);
    }
    private static List<HexVector> GetCostalNeighbors(HexVector hex, bool[,] ocean){
        List<HexVector> neighbors = new(7);
        HexVector[] axialNeighbors = HexVector.Neighbors(hex);
        for (int i = 0; i < axialNeighbors.Length; i++){
            GridVector neighbor = (GridVector)axialNeighbors[i];
            if(neighbor.x < 0 || neighbor.x >= ocean.GetLength(0) || neighbor.y < 0 || neighbor.y >= ocean.GetLength(1))
                continue;
            if(ocean[neighbor.x, neighbor.y])
                neighbors.Add(axialNeighbors[i]);
        }
        return neighbors;
    }
    private static List<HexVector> GetLandCostalNeighbors(HexVector hex, bool[,] land, bool[,] ocean){
        List<HexVector> neighbors = new(7);
        HexVector[] axialNeighbors = HexVector.Neighbors(hex);
        for (int i = 0; i < axialNeighbors.Length; i++){
            GridVector neighbor = (GridVector)axialNeighbors[i];
            if(neighbor.x < 0 || neighbor.x >= land.GetLength(0) || neighbor.y < 0 || neighbor.y >= land.GetLength(1))
                continue;
            if(land[neighbor.x, neighbor.y] && GetCostalNeighbors(axialNeighbors[i], ocean).Count > 0)
                neighbors.Add(axialNeighbors[i]);
        }
        return neighbors;
    }
    
    // passed as GridCords (x, y)
    private static void FloodFill(Tile[,] tiles, int x, int y, bool[,] visited, bool[,] mass, bool walkable){
        if(x < 0 || x >= tiles.GetLength(0) || y < 0 || y >= tiles.GetLength(1))
            return;
        if(visited[x, y])
            return;
        if(tiles[x, y].IsWalkable() != walkable)
            return;
        visited[x, y] = true;
        mass[x, y] = true;
        HexVector[] neighbors = HexVector.Neighbors((HexVector)new GridVector(x, y));
        for (int i = 0; i < neighbors.Length; i++){
            GridVector neighbor = (GridVector)neighbors[i];
            FloodFill(tiles, neighbor.x, neighbor.y, visited, mass, walkable);
        }
    }

    private static int BiomeToElevation(Biome biome){
        return biome switch
        {
            Biome.Snow or Biome.Tundra or Biome.Scorched => 5,
            Biome.Taiga or Biome.Shrubland or Biome.TemperateDesert => 4,
            Biome.PineForest or Biome.GinkgoForest or Biome.Grassland => 3,
            Biome.RainForest or Biome.OakForest or Biome.SubtropicalDesert => 2,
            Biome.Beach => 1,
            // Biome.Ocean or Biome.Lake => 0,
            _ => 0,
        };
    }
// Snow
// Tundra
// Bare
// Scorched

// Taiga
// Shrubland
// Temperate Desert

// Temperate Rain Forest
// Temperate Deciduous Forest
// Grassland

// Tropical Rain Forest
// Tropical Seasonal Forest
// Subtropical Desert

// Ocean
// Lake
// Beach
}
