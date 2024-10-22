using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoardRender
{
    public static BoardRender Instance { get; private set; }
    private Board Board;
    private HashSet<int> LoadedTile;
    private List <int> TileToRemove;
    public BounceGrid BounceGrid { get; private set; }
    private ExposedList<UniversalRenderer>[,,] UniversalRenders;

    private HashSet<Vector3Int> InitializedEntities;

    // public const int ChunkSizeX = 22 * 8;
    // public const int ChunkSizeY = 22 * 8;
    public const int ChunkSizeX = 22;
    public const int ChunkSizeY = 22;
    public float[] BoardBounds; // xMin, xMax, yMin, yMax
    public BoardRender(Board board)
    {
        Instance = this;

        Board = board;
        BounceGrid = new BounceGrid(board.BoardSize);

        UniversalRenders = new ExposedList<UniversalRenderer>[2, Board.BoardSize, Board.BoardSize];
        for (int i = 0; i < 2; i++)
            for (int j = 0; j < Board.BoardSize; j++)
                for (int k = 0; k < Board.BoardSize; k++)
                    UniversalRenders[i, j, k] = new ExposedList<UniversalRenderer>(32, () => null);

        LoadedTile = new HashSet<int>();
        TileToRemove = new List<int>();

        InitializedEntities = new HashSet<Vector3Int>();

        BoardBounds = new float[4];
        Vector2 bottomLeftPixel = (Vector2)(HexVector)new GridVector(0, 0);
        Vector2 topRightPixel = (Vector2)(HexVector)new GridVector(Board.BoardSize - 1, Board.BoardSize - 1);
        BoardBounds[0] = bottomLeftPixel.x;
        BoardBounds[1] = topRightPixel.x;
        BoardBounds[2] = bottomLeftPixel.y;
        BoardBounds[3] = topRightPixel.y;
    }
    
    class UniversalAnimatorTransferData
    {
        public Entity Entity;
        public Func<UniversalRenderer, float, float> Animation;
        public float Counter;    
    }
    public void ReRender()
    {
        List<UniversalAnimatorTransferData> universalAnimatorTransferData = GenerateUniversalAnimatorTransferData();

        UnloadAllEntities();
        LoadedTile.Clear();
        TileToRemove.Clear();

        UpdateTileChunks();
        RenderTemporalStates();

        foreach (UniversalAnimatorTransferData data in universalAnimatorTransferData)
            GetUniversalRenderer(data.Entity)?.AddOverrideAnimation(data.Animation, data.Counter);
    }

    public void RenderTemporalStates(){
        List<BoardState> boardStates = Board.GetBoardStatesToRender();

        for (int k = 1; k < boardStates.Count; k++){
            // List<MonoDeltaBind> monoDeltaBinds = boardStates[k].MonoDeltaBinds;
            // foreach (MonoDeltaBind monoDeltaBind in monoDeltaBinds)
            //     foreach (EntityDelta entityDelta in monoDeltaBind.EntityDeltas)
            //         foreach (MonoEntityDelta monoEntityDelta in entityDelta.FromTo){
            //             int id = Board.PositionToID((GridVector)monoEntityDelta.Position); // you might be able to put this earlier
            //             if (LoadedTile.Contains(id))
            //                 ReRenderEntity(monoEntityDelta.To, k);
            //         }
            List<MonoEntityDelta> monoEntityDeltas = boardStates[k].GetMonoEntityDeltas();
            foreach (MonoEntityDelta monoEntityDelta in monoEntityDeltas){
                int id = Board.PositionToID((GridVector)monoEntityDelta.Position);
                if (LoadedTile.Contains(id) && boardStates[k].GetEntity<RealDeltaEntity>(monoEntityDelta.Position) == monoEntityDelta.To)
                    ReRenderEntity(monoEntityDelta.To, k);
            }
        }
    }

    //call to clear all entities from the board.
    private void UnloadAllEntities()
    {
        List<int> LoadedTileList = new(LoadedTile);
        for (int k = 0; k < LoadedTileList.Count; k++)
            RemoveTile(LoadedTileList[k]);
        LoadedTile.Clear();
    }

    //call once per frame, or if camera moves.
    public void UpdateTileChunks()
    {
        int boardSize = Board.BoardSize;

        GridVector gridPos = CoreCamera.MouseGridPos();
        int x = gridPos.x;
        int y = gridPos.y;

        BoardState workingBoardState = Board.Current;
        int workingBoard = Board.WorkingBoard;
        List<BoardState> boardStates = Board.GetBoardStatesToRender();
        for (int i = x - ChunkSizeX; i < x + ChunkSizeX; i++){
            for (int j = y - ChunkSizeY; j < y + ChunkSizeY; j++){
                if (i < 0 || i >= boardSize || j < 0 || j >= boardSize)
                    continue;

                //! not sure if you should even render base, up to artistic choice
                ReRenderEntity(workingBoardState[0, i, j], workingBoard);
                ReRenderEntity(workingBoardState[1, i, j], workingBoard);

                ReRenderEntity(boardStates[0][1, i, j], 0);//! since no no new entities, base board.

                int id = Board.PositionToID(new GridVector(i, j));
                if (!LoadedTile.Contains(id))
                    LoadedTile.Add(id);
            }
        }

        TileToRemove.Clear();
        foreach (int idx in LoadedTile){
            int i = idx / boardSize;
            int j = idx % boardSize;
            if (i < x - ChunkSizeX || i >= x + ChunkSizeX || j < y - ChunkSizeY || j >= y + ChunkSizeY)
                TileToRemove.Add(idx);
        }

        for(int k = 0; k < TileToRemove.Count; k++){
            int idx = TileToRemove[k];
            LoadedTile.Remove(idx);
            RemoveTile(idx);
        }
    }
    private void RemoveTile(int idx)
    {
        LoadedTile.Remove(idx);
        GridVector position = Board.IDToPosition(idx);
        for (int i = 0; i < 2; i++){
            for (int j = 0; j < UniversalRenders[i, position.x, position.y].Count; j++){
                if (UniversalRenders[i, position.x, position.y][j] != null){
                    UniversalRenderer.Pool.Return(UniversalRenders[i, position.x, position.y][j]);
                    UniversalRenders[i, position.x, position.y][j] = null;
                }
            }
            UniversalRenders[i, position.x, position.y].SetCount(0);
        }
    }
    public UniversalRenderer ReRenderEntity(Entity entity, int idx){
        if (entity == null)
            return null;

        // the UniversalRendersList is the list of universal renderers at that tile position, theres two, one for tiles and one for entities (since their frequency vary so widely
        //(one is guaranteed to be at least one are rarely changes, while the other is often 0 but when its not its some larger number))
        GridVector position = (GridVector)entity.Position;
        ExposedList<UniversalRenderer> universalRendersList = UniversalRenders[entity is Tile ? 0 : 1, position.x, position.y];

        if (idx >= universalRendersList.Count)
            universalRendersList.SetCount(idx + 1);

        if (universalRendersList[idx] == null){ //if there does not already exist a universal renderer at that index, add it. like a playAnim call.
            UniversalRenderer universalRenderer = UniversalRenderer.Pool.Get();
            universalRenderer.BindTo(entity);
            entity.UniversalRendererInit(universalRenderer, idx);
            universalRendersList[idx] = universalRenderer;
        }
        universalRendersList[idx].UpdateRender();
        return universalRendersList[idx];
    }
    public UniversalRenderer ReRenderEntity(Entity entity) => ReRenderEntity(entity, Board.WorkingBoard);
    public UniversalRenderer GetUniversalRenderer(Entity entity) => GetUniversalRenderer(entity.Position, entity is Tile ? 0 : 1, Board.WorkingBoard);
    public UniversalRenderer GetUniversalRenderer(HexVector positionHex, int type, int idx){
        GridVector position = (GridVector)positionHex;
        ExposedList<UniversalRenderer> universalRendersList = UniversalRenders[type, position.x, position.y];

        if (idx >= universalRendersList.Count)
            return null;
        return universalRendersList[idx];
    }
    public void AddInitializedEntity(HexVector position, int idx) => InitializedEntities.Add(new Vector3Int(position.x, position.y, idx));
    public void RemoveInitializedEntity(HexVector position, int idx) => InitializedEntities.Remove(new Vector3Int(position.x, position.y, idx));
    public bool IsInitializedEntity(HexVector position, int idx) => InitializedEntities.Contains(new Vector3Int(position.x, position.y, idx));
    

    // TODO probably migrate to a interface, and core loop calls it instead.
    private List<UniversalAnimatorTransferData> GenerateUniversalAnimatorTransferData(){
        List<UniversalAnimatorTransferData> universalAnimatorTransferDataList = new();
        List<BoardState> boardStates = Board.GetBoardStatesToRender();

        foreach (int idx in LoadedTile){
            GridVector position = Board.IDToPosition(idx);
            for (int i = 0; i < 2; i++){
                for (int j = 0; j < UniversalRenders[i, position.x, position.y].Count; j++){
                    UniversalRenderer universalRenderer = UniversalRenders[i, position.x, position.y][j];
                    if (universalRenderer != null){
                        Func<UniversalRenderer, float, float> animation = universalRenderer.OverrideAnimation;
                        if (animation != null){
                            universalAnimatorTransferDataList.Add(new()
                            {
                                Entity = universalRenderer.DataObject,
                                Animation = animation,
                                Counter = universalRenderer.OverrideCounter
                            });
                        }
                    }
                }
            }
        }
        return universalAnimatorTransferDataList;
    }
}
