using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//TODO Pathfinding to coast should be shortest path, which isn't just the distance between the two points. Pathfind it.
public class PathFinding
{
    private class PathFindingCacheEntry
    {
        public Dictionary<HexVector, ExposedLLNode<HexVector>> Cache;
        public ExposedLLNode<HexVector> EndNode;
        public PathFindingCacheEntry(ExposedLLNode<HexVector> endNode)
        {
            Cache = new Dictionary<HexVector, ExposedLLNode<HexVector>>();
            EndNode = endNode;
        }
    }
    private static Dictionary<HexVector, PathFindingCacheEntry> PathCache;
    private static RealDeltaEntity[,] EmptyCollisionBoard;

    public static void Initialize(BoardState boardState)
    {
        // Maybe do a prewarm? idk
        PathCache = new();
        EmptyCollisionBoard = new RealDeltaEntity[boardState.Board.BoardSize, boardState.Board.BoardSize];
    }

    //! known bugs? , a landmass can be seperated by a mountan range or something else.
    public static ExposedLLNode<HexVector> PathFind(BoardState boardState, HexVector start, HexVector end, ulong ID)
    {
        int[,] massNumbers = boardState.Board.MassNumber;
        Tile[,] tiles = boardState.TileBoard;

        GridVector gridStart = (GridVector)start;
        GridVector gridEnd = (GridVector)end;
        int startMass = massNumbers[gridStart.x, gridStart.y];
        int endMass = massNumbers[gridEnd.x, gridEnd.y];


        ExposedLLNode<HexVector>[] path = null;
        // Three possibilities, starting off on a different landmass, starting in the ocean, same landmass.
        if (startMass != endMass && tiles[gridStart.x, gridStart.y].IsWalkable()){ // different landmass, pathfind to nearest landing zone, into the water.
            HexVector nextEnd = (tiles[gridStart.x, gridStart.y] as LandTile).LandingZone.Value;
            path = ExposedLLNode<HexVector>.Merge(path, CachedAStar(boardState, (HexVector)gridStart, nextEnd, startMass));

            HexVector[] neighbors = HexVector.Neighbors(nextEnd);
            foreach (HexVector neighbor in neighbors){
                GridVector grid = (GridVector)neighbor;
                if (boardState.Board.IsGridInBounds(grid) && massNumbers[grid.x, grid.y] == 0){ // is ocean
                    gridStart = grid;
                    break;
                }
            }
            startMass = massNumbers[gridStart.x, gridStart.y];
        }

        if (startMass != endMass){
            GridVector landingZone = (GridVector)LandingZone(boardState, (HexVector)gridStart, (HexVector)gridEnd);
            landingZone = (GridVector)NextUnoccupiedLandingZone(boardState, (tiles[landingZone.x, landingZone.y] as LandTile).LandingZone, ID);
            // Debug.Log((HexVector)landingZone + " " + ID);
            path = ExposedLLNode<HexVector>.Merge(path, CachedAStar(boardState, (HexVector)gridStart, (HexVector)landingZone, startMass));
            gridStart = landingZone;
        }

        path = ExposedLLNode<HexVector>.Merge(path, CachedAStar(boardState, (HexVector)gridStart, (HexVector)gridEnd, endMass));
        return path[0];
    }
    // Returns the landing zone of the path, where the hadic will land, ish.
    public static HexVector? LandingZone(BoardState boardState, HexVector start, HexVector end)
    {
        int[,] massNumbers = boardState.Board.MassNumber;
        Tile[,] tiles = boardState.TileBoard;

        GridVector gridStart = (GridVector)start;
        GridVector gridEnd = (GridVector)end;

        if (massNumbers[gridStart.x, gridStart.y] == massNumbers[gridEnd.x, gridEnd.y])
            return null;
        return (tiles[gridEnd.x, gridEnd.y] as LandTile).LandingZone.Value;
    }
    // this might be pretty slow, if group size is N, it is O(N), could be O(1)..
    public static HexVector NextUnoccupiedLandingZone(BoardState boardState, ExposedLLNode<HexVector> Zones, ulong ID)
    {
        Tile[,] tiles = boardState.TileBoard;

        ExposedLLNode<HexVector> start = Zones;
        ExposedLLNode<HexVector> forwardsTraversal = Zones;
        ExposedLLNode<HexVector> backwardsTraversal = Zones.Previous;

        int failSafe = 0;
        while (forwardsTraversal != backwardsTraversal && failSafe < 3000){
            GridVector grid;
            if (failSafe % 2 == 0){
                grid = (GridVector)forwardsTraversal.Value;
                forwardsTraversal = forwardsTraversal.Next;
            }else{
                grid = (GridVector)backwardsTraversal.Value;
                backwardsTraversal = backwardsTraversal.Previous;
            }

            LandTile tile = tiles[grid.x, grid.y] as LandTile;
            if (tile.LaunchPointOccupied == ID)
                return (HexVector)grid;

            failSafe++;
        }
        return start.Value;
    }
    public static ExposedLLNode<HexVector> LocalizedPathFind(BoardState boardState, HexVector start, HexVector end, int maxIterations)
    {
        int startMass = boardState.Board.MassNumber[((GridVector)start).x, ((GridVector)start).y];
        if (startMass != boardState.Board.MassNumber[((GridVector)end).x, ((GridVector)end).y])
            return null;
        return AStar(boardState, start, end, startMass, false, maxIterations)?[0]; // TODO maxLength should be part of this.
    }

    private static ExposedLLNode<HexVector>[] CachedAStar(BoardState boardState, HexVector start, HexVector end, int mass)
    {
        if (PathCache.ContainsKey(end) && PathCache[end].Cache.ContainsKey(start))
            return new ExposedLLNode<HexVector>[] {PathCache[end].Cache[start], PathCache[end].EndNode};

        ExposedLLNode<HexVector>[] path = AStar(boardState, start, end, mass, true, int.MaxValue);

        if (path == null)
            Debug.LogError("No Path Found");

        if (!PathCache.ContainsKey(end))
            PathCache[end] = new PathFindingCacheEntry(path[1]);
        ExposedLLNode<HexVector> head = path[0];
        Dictionary<HexVector, ExposedLLNode<HexVector>> cache = PathCache[end].Cache;
        while (head != null){
            if (!cache.ContainsKey(head.Value))
                cache.Add(head.Value, head);
            head = head.Next;
        }
        return path;
    }
    // hex
    // ! BUG: If the end is a cliff, it can jump to it.
    private static ExposedLLNode<HexVector>[] AStar(BoardState boardState, HexVector start, HexVector end, int mass, bool ignoreEntities, int maxIterations)
    {
        Tile[,] tiles = boardState.TileBoard;
        RealDeltaEntity[,] entities = ignoreEntities ? EmptyCollisionBoard : boardState.DeltaEntityBoard;
        int[,] elevations = boardState.Board.Elevation;
        int[,] massNumbers = boardState.Board.MassNumber;

        // init
        PriorityQueue<HexVector, int> openNodes = new();
        HashSet<HexVector> openNodesSet = new();

        Dictionary<HexVector, int> gScore = new();
        Dictionary<HexVector, int> fScore = new();
        Dictionary<HexVector, HexVector> parent = new();

        HashSet<HexVector> visited = new();
        // seems like the init does auto caching. When I cache it manually, it's slower.

        openNodes.Enqueue(start, 0);
        gScore[start] = 0;
        fScore[start] = HexVector.Distance(start, end);

        while (openNodes.Count > 0){
            if (maxIterations-- <= 0)
                return null;

            HexVector current = openNodes.Dequeue();
            openNodesSet.Remove(current);

            if (current == end){
                ExposedLLNode<HexVector> path = new(current, null, null);
                ExposedLLNode<HexVector> tail = path;
                while (parent.ContainsKey(current)){
                    current = parent[current];
                    path.Previous = new ExposedLLNode<HexVector>(current, path, null);
                    path = path.Previous;
                }
                return new ExposedLLNode<HexVector>[] {path, tail};
            }

            visited.Add(current);

            HexVector[] neighborsBaseList = HexVector.Neighbors(current);
            List<HexVector> neighbors = new();
            GridVector currentGrid = (GridVector)current;
            int currentElevation = elevations[currentGrid.x, currentGrid.y];
            foreach (HexVector neighbor in neighborsBaseList){
                GridVector grid = (GridVector)neighbor;
                if (grid.x >= 0 && grid.x < tiles.GetLength(0) && grid.y >= 0 && grid.y < tiles.GetLength(1)){
                    if((
                        massNumbers[grid.x, grid.y] == mass 
                        && Mathf.Abs(elevations[grid.x, grid.y] - currentElevation) <= 1
                        && entities[grid.x, grid.y] == null
                    ) || (end.x == neighbor.x && end.y == neighbor.y)) // !idk if this should be here, can jump
                        neighbors.Add(neighbor);
                }
            }

            HexVector closest = HexVector.ClosestToHex(current, end);
            //swap last and closest
            int idxOfClosest = -1;
            for (int i = 0; i < neighbors.Count; i++){
                if (neighbors[i].x == closest.x && neighbors[i].y == closest.y){
                    idxOfClosest = i;
                    break;
                }
            }
            if (idxOfClosest != -1)
                (neighbors[idxOfClosest], neighbors[^1]) = (neighbors[^1], neighbors[idxOfClosest]);



            foreach (HexVector neighbor in neighbors){
                if (visited.Contains(neighbor))
                    continue;
                int nodeNeighbor = HexVector.Distance(neighbor, end);
                int tentativeGScore = gScore[current] + nodeNeighbor;
                if (!gScore.ContainsKey(neighbor) || tentativeGScore < gScore[neighbor]){
                    parent[neighbor] = current;
                    gScore[neighbor] = tentativeGScore;
                    fScore[neighbor] = gScore[neighbor] + nodeNeighbor;

                    if (!openNodesSet.Contains(neighbor)){
                        openNodes.Enqueue(neighbor, fScore[neighbor]);
                        openNodesSet.Add(neighbor);
                    }
                }
            }
        }
        // Debug.LogError("No Path Found " + start + " " + end + " " + visited.Count);
        return null;
    }
}
