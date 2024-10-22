using System;
using System.Collections.Generic;
using UnityEngine;

// if you want to modify a future board, you must keep re-pushing new deltas to each board before it and make sure it does not conflict with the previous board.
// ****** MonoDeltas and EntityDeltas do the same thing, its just that MonoDeltas are only for the player, ***one way*** and very entity specific, while EntityDeltas are for all entities.
    // But the only way to create DeltaEntities is through playing mono deltas and auto deltas.
public class BoardState{
    public Board Board { get; private set; }

    private readonly Entity[][,] EntityBoard;
    private readonly Dictionary<HexVector, UniversalDeltaEntity> UniversalDeltaEntities;
    private readonly List<MonoDeltaBind> MonoDeltaBinds;

    private LinkedList<RealDeltaEntity> DeltaEntities;


    public Tile[,] TileBoard => EntityBoard[0] as Tile[,];
    public RealDeltaEntity[,] DeltaEntityBoard => EntityBoard[1] as RealDeltaEntity[,];
    public Entity this[int entityType, int x, int y] => EntityBoard[entityType][x, y];



    public BoardState(int boardSize, Board board){
        Board = board;

        EntityBoard = new Entity[2][,];
        EntityBoard[0] = new Tile[boardSize, boardSize];
        EntityBoard[1] = new RealDeltaEntity[boardSize, boardSize];

        UniversalDeltaEntities = new Dictionary<HexVector, UniversalDeltaEntity>();

        DeltaEntities = new LinkedList<RealDeltaEntity>();

        MonoDeltaBinds = new List<MonoDeltaBind>();
    }
    public void SoftReset(BoardState boardState){

        Array.Copy(boardState.EntityBoard[0], EntityBoard[0], EntityBoard[0].Length);
        Array.Copy(boardState.EntityBoard[1], EntityBoard[1], EntityBoard[1].Length);

        UniversalDeltaEntities.Clear();
        foreach(KeyValuePair<HexVector, UniversalDeltaEntity> pair in boardState.UniversalDeltaEntities)
            UniversalDeltaEntities.Add(pair.Key, pair.Value);

        DeltaEntities.Clear();
        foreach(RealDeltaEntity deltaEntity in boardState.DeltaEntities)
            DeltaEntities.AddLast(deltaEntity);
    }
    public void Reset(BoardState boardState){
        SoftReset(boardState);
        MonoDeltaBinds.Clear();
    }
    public void InjectMonoDelta(MonoDelta monoDelta, DeltaEntity associatedEntity) => MonoDeltaBinds.Add(new MonoDeltaBind(monoDelta, associatedEntity));   
    private void UpdateUniversalEntity(UniversalDeltaEntity oldEntity, UniversalDeltaEntity newEntity){
        UniversalDeltaEntities[newEntity.Position] = newEntity;
        MonoDeltaBinds[^1].EntityDeltas.Add(new EntityDelta(-1, new MonoEntityDelta(newEntity.Position, oldEntity, newEntity)));
    }
    public void AddEntity(Entity entity) => UpdateEntity(null, entity);
    public void UpdateEntity(Entity oldEntity, Entity newEntity){
        // On method of deleting a entity is to set it to null, this checks for it and pretends like it was null.
        if (oldEntity != null && oldEntity.EntityEnum == EntityEnum.Null)
            oldEntity = null;
        if (newEntity != null && newEntity.EntityEnum == EntityEnum.Null)
            newEntity = null;

        // Bad OOP but universal entities don't use this system so they are handled differently.
        if (oldEntity is UniversalDeltaEntity || newEntity is UniversalDeltaEntity){
            UpdateUniversalEntity(oldEntity as UniversalDeltaEntity, newEntity as UniversalDeltaEntity);
            return;
        }

        // Get the position, if null then f*ck off
        HexVector? oldPosition = oldEntity != null ? oldEntity.Position : null;
        HexVector? newPosition = newEntity != null ? newEntity.Position : null;
        int entityType = (newEntity ?? oldEntity) is Tile ? 0 : 1; // both will always be tile or non tiles.

        // Depending on the type of delta, ie if you can just do a direct swap or a move.
        if(oldPosition == newPosition || oldPosition == null || newPosition == null){// Entity delta of just one mono
            HexVector finalPosition = oldPosition == null ? newPosition.Value : oldPosition.Value;
            MonoDeltaBinds[^1].EntityDeltas.Add(new EntityDelta(entityType, new MonoEntityDelta(finalPosition, oldEntity, newEntity)));
        }else{
            MonoDeltaBinds[^1].EntityDeltas.Add(new EntityDelta(entityType, new MonoEntityDelta(oldPosition.Value, oldEntity, null), new MonoEntityDelta(newPosition.Value, null, newEntity)));
        }

        // Do the actual update
        // Remove the old entity.
        if (oldEntity != null){
            // Override the actual board
            GridVector oldGridVector = (GridVector)oldEntity.Position;
            EntityBoard[oldEntity is Tile ? 0 : 1][oldGridVector.x, oldGridVector.y] = null;

            // Remove from the cache
            if (oldEntity is RealDeltaEntity)
                DeltaEntities.Remove(oldEntity as RealDeltaEntity);
        }
        // Add the new entity.
        if (newEntity != null){
            // Add to associated entities for MonoDeltaBind
            if (newEntity is DeltaEntity)
                MonoDeltaBinds[^1].SoftAssociatedEntities.Add(newEntity as DeltaEntity);

            GridVector newGridVector = (GridVector)newEntity.Position;
            if (EntityBoard[newEntity is Tile ? 0 : 1][newGridVector.x, newGridVector.y] != null)
                Debug.LogError("Entity already exists at position: " + newEntity.Position);
            EntityBoard[newEntity is Tile ? 0 : 1][newGridVector.x, newGridVector.y] = newEntity; // update board

            if (newEntity is DeltaEntity)
                AddToOrderedList(DeltaEntities, newEntity as RealDeltaEntity); // Has to be ordered since to undo you need to go in reverse order.
        }
    }
    public void ApplyDelta(bool direction, BoardDelta boardDelta) // direction: true = forward, false = backward
    {
        List<EntityDelta> entityDeltas = boardDelta.EntityDeltas;
        int start = direction ? 0 : entityDeltas.Count - 1;
        int end = direction ? entityDeltas.Count : -1;
        int increment = direction ? 1 : -1;

        for (int i = start; i != end; i += increment){
            EntityDelta entityDelta = entityDeltas[i];
            foreach(MonoEntityDelta monoEntityDelta in entityDelta.FromTo){ // should never conflict if out of order
                Entity to = direction ? monoEntityDelta.To : monoEntityDelta.From;
                if (entityDelta.EntityType == -1){
                    UniversalDeltaEntities[to.Position] = to as UniversalDeltaEntity;
                }else{
                    GridVector gridVector = (GridVector)monoEntityDelta.Position;
                    EntityBoard[entityDelta.EntityType][gridVector.x, gridVector.y] = to;
                }
            }
        }
        ManualSoftInstantiate();
    }
    public void ManualSoftInstantiate(){
        DeltaEntities = BruteCreateDeltaEntities();
        MonoDeltaBinds.Clear();
    }

    public List<EntityDelta> GetEntityDeltas(){
        List<EntityDelta> EntityDeltas = new();
        foreach(MonoDeltaBind monoDeltaBind in MonoDeltaBinds)
            EntityDeltas.AddRange(monoDeltaBind.EntityDeltas);
        return EntityDeltas;
    }

    /// <summary>
    /// Takes all the autoDeltaEntities and ask them to regenerate their moves. Paired with ReapplyDeltas
    /// </summary>
    public void CreateAutoDeltas() => OverrideBoardState(() => {
        List<DeltaEntity> deltaEntities = new();

        foreach(DeltaEntity deltaEntity in DeltaEntities)
            if (deltaEntity is IAutoDeltaEntity)
                deltaEntities.Add(deltaEntity);
        foreach(UniversalDeltaEntity universalEntity in UniversalDeltaEntities.Values)
            if (universalEntity is IAutoDeltaEntity)
                deltaEntities.Add(universalEntity);

        foreach(DeltaEntity deltaEntity in deltaEntities){
            DeltaEntity tracedDeltaEntity = FindNewDeltaEntity(deltaEntity);
            if (tracedDeltaEntity == null || tracedDeltaEntity is not IAutoDeltaEntity) // ? Log Error?
                continue;

            (tracedDeltaEntity as IAutoDeltaEntity).ResolveAutoDelta();
        }
    });

    private DeltaEntity FindNewDeltaEntity(DeltaEntity deltaEntity){
        if (deltaEntity == GetEntity<DeltaEntity>(deltaEntity.Position))
            return deltaEntity;
        for(int failSafe = 0; failSafe < 100; failSafe++){
            EntityDelta entityDelta = FindEntityDelta(deltaEntity);

            MonoEntityDelta first = entityDelta.FromTo[0];
            MonoEntityDelta second = entityDelta.FromTo.Length == 2 ? entityDelta.FromTo[1] : null;

            // ! This might be a problem if theres a direct swap, should never happen
            deltaEntity = first.To != null ? first.To as DeltaEntity : second?.To as DeltaEntity;

            if (deltaEntity == null)
                return null;

            if (deltaEntity == GetEntity<DeltaEntity>(deltaEntity.Position))
                return deltaEntity;
        }
        return null;
    }
    private EntityDelta FindEntityDelta(DeltaEntity deltaEntity){
        foreach(MonoDeltaBind monoDeltaBind in MonoDeltaBinds)
            foreach(EntityDelta entityDelta in monoDeltaBind.EntityDeltas)
                if (entityDelta.FromTo[0].From == deltaEntity)
                    return entityDelta;
        return null; // ! Shouldn't happen, it happens if there the old one gets overridden and but isn't tracked.
    }

    /// <summary>
    /// Takes all the user made mono deltas and applies them to a empty board. Paired with CreateAutoDeltas
    /// </summary>
    public void ReapplyDeltas() => OverrideBoardState(() => {
        List<MonoDelta> monoDeltas = ExtractMonoDeltas();

        MonoDeltaBinds.Clear();

        foreach(MonoDelta monoDelta in monoDeltas){
            int i = 0;
            HexVector position = monoDelta.ReadHexVector(ref i);
            DeltaEntity deltaEntity = GetEntity<DeltaEntity>(position);
            if (deltaEntity == null || deltaEntity is not IManualDeltaEntity){
                Debug.LogError("UserDeltaEntity not found at position: " + position);
                continue;
            }
            InjectMonoDelta(monoDelta, deltaEntity);
            (deltaEntity as IManualDeltaEntity).ResolveManualDelta();
        }
    });
    private void OverrideBoardState(Action action){
        BoardState originalBoardState = Entity.BoardState;
        Entity.SetBoardState(this);
        action.Invoke();
        Entity.SetBoardState(originalBoardState);
    }
    
    public MonoDeltaBind GetInjectedMonoDeltaBind() => MonoDeltaBinds.Count == 0 ? null : MonoDeltaBinds[^1];
    public MonoDeltaBind FindMonoDeltaBind(ulong ID){
        foreach(MonoDeltaBind monoDeltaBind in MonoDeltaBinds)
            if (monoDeltaBind.AssociatedEntity.ID == ID)
                return monoDeltaBind;
        return null;
    }
    public bool RemoveMonoDeltaAssociatedWith(ulong ID){
        int i = IdxOfMonoDeltaAssociatedWith(ID);
        if (i == -1)
            return false;
        MonoDeltaBinds[i].Delete(Board.Instance.TurnNumberOf(this));
        MonoDeltaBinds.RemoveAt(i);
        return true;
    }
    private int IdxOfMonoDeltaAssociatedWith(ulong ID){
        for (int i = MonoDeltaBinds.Count - 1; i >= 0; i--){
            if (MonoDeltaBinds[i].AssociatedEntity.ID == ID)
                return i;
            foreach(DeltaEntity entity in MonoDeltaBinds[i].SoftAssociatedEntities)
                if (entity.ID == ID)
                    return i;
        }
        return -1;
    }
    public List<MonoDelta> ExtractMonoDeltas(){
        List<MonoDelta> monoDeltas = new();
        foreach(MonoDeltaBind monoDeltaBind in MonoDeltaBinds)
            if (monoDeltaBind.MonoDelta != null) // Auto BoardStates will have a empty MonoDelta
                monoDeltas.Add(monoDeltaBind.MonoDelta);
        return monoDeltas;
    }

    /// <summary>
    /// Adds the RealDeltaEntity to the DeltaEntities list in order by position.
    /// </summary>
    private void AddToOrderedList(LinkedList<RealDeltaEntity> deltaEntities, RealDeltaEntity deltaEntity){
        LinkedListNode<RealDeltaEntity> node = deltaEntities.First;
        while (node != null && (node.Value.Position.x == deltaEntity.Position.x ? node.Value.Position.y < deltaEntity.Position.y : node.Value.Position.x < deltaEntity.Position.x))
            node = node.Next;
        if (node == null)
            deltaEntities.AddLast(deltaEntity);
        else
            deltaEntities.AddBefore(node, deltaEntity);
    }

    /// <summary>
    /// Recreates the DeltaEntities list from the EntityBoard, goes through every tile and adds the RealDeltaEntity to the list.
    /// </summary>
    private LinkedList<RealDeltaEntity> BruteCreateDeltaEntities(){
        LinkedList<RealDeltaEntity> deltaEntities = new();
        for (int i = 0; i < EntityBoard[1].GetLength(0); i++)
            for (int j = 0; j < EntityBoard[1].GetLength(1); j++)
                if (EntityBoard[1][i, j] != null)
                    AddToOrderedList(deltaEntities, EntityBoard[1][i, j] as RealDeltaEntity);
        return deltaEntities;
    }

    // For Rendering
    public List<MonoEntityDelta> GetMonoEntityDeltas(){
        List<MonoEntityDelta> monoEntityDeltas = new();
        foreach(MonoDeltaBind monoDeltaBind in MonoDeltaBinds)
            foreach(EntityDelta entityDelta in monoDeltaBind.EntityDeltas)
                monoEntityDeltas.AddRange(entityDelta.FromTo);
        return monoEntityDeltas;
    }

    public void Serialize(List<byte> bytes){
        SaveFile.WriteString(bytes, "BoardState");
        
        List<MonoDelta> monoDeltas = ExtractMonoDeltas();
        SaveFile.WriteInt(bytes, monoDeltas.Count);
        foreach(MonoDelta monoDelta in monoDeltas)
            monoDelta.Serialize(bytes);
    }

    public static BoardState Deserialize(byte[] bytes, ref int index, Board board){
        string header = SaveFile.ReadString(bytes, ref index);
        BoardState boardState = new(board.BoardSize, board);

        int monoDeltasCount = SaveFile.ReadInt(bytes, ref index);
        for (int i = 0; i < monoDeltasCount; i++)
            boardState.InjectMonoDelta(MonoDelta.Deserialize(bytes, ref index), null);

        return boardState;
    }

    // ********** Debugging **********
    public override string ToString(){
        string boardString = "";

        boardString += "Status: \n" + CheckDeltaEntityDeSync() + "\n";

        boardString += "TurnNumber: " + Board.TurnNumberOf(this) + "\n";

        boardString += "BruteHash: " + SaveUtility.ToHexString(BruteHash()) + "\n";

        // Board
        boardString += "Entities: " + DeltaEntities.Count + "\n";
        string[] deltaEntitiesString = new string[DeltaEntities.Count];
        int idx = 0;
        foreach(Entity entity in DeltaEntities){
            deltaEntitiesString[idx] = entity.ToString().Replace("\n", " | ") + "\n";
            idx++;
        }
        // System.Array.Sort(deltaEntitiesString);
        foreach(string deltaEntityString in deltaEntitiesString)
            boardString += deltaEntityString;

        // AutoUniversalEntities
        boardString += "AutoUniversalEntities: " + UniversalDeltaEntities.Count + "\n";
        foreach(UniversalDeltaEntity autoDeltaEntity in UniversalDeltaEntities.Values)
            boardString += autoDeltaEntity.ToString().Replace("\n", " | ") + "\n";

                
        // MonoDeltaBinds
        boardString += "MonoDeltaBinds: " + MonoDeltaBinds.Count + "\n";
        foreach(MonoDeltaBind monoDeltaBind in MonoDeltaBinds){
            boardString += (monoDeltaBind.MonoDelta == null ? "null" : monoDeltaBind.MonoDelta.ToString()) + "\n";
            boardString += "EntityDeltas: " + monoDeltaBind.EntityDeltas.Count + "\n";
            if (monoDeltaBind.EntityDeltas.Count > 1000)
                boardString += "Too many deltas to display\n";
            else
                foreach(EntityDelta entityDelta in monoDeltaBind.EntityDeltas)
                    boardString += entityDelta.ToString() + "|\n";
        }

        return boardString;
    }
    public long BruteHash(){
        long hash = 0;
        List<Entity> everyEntity = EveryEntity();
        foreach(Entity entity in everyEntity){
            int HashWrapper = 0;
            hash ^= SaveUtility.ShiftAndWrap((uint)entity.GetHash(ref HashWrapper), Math.Abs(entity.Position.GetHashCode()));
        }
        hash = SaveUtility.ReHash(hash);
        return hash;
    }
    private string CheckDeltaEntityDeSync(){
        LinkedList<RealDeltaEntity> deltaEntitiesClone = BruteCreateDeltaEntities();
        string syncStatus = "";
        foreach(RealDeltaEntity deltaEntity in deltaEntitiesClone)
            if (!DeltaEntities.Contains(deltaEntity))
                syncStatus += "DeSynced1:\n" + deltaEntity.ToString() + "\n";
        foreach(RealDeltaEntity deltaEntity in DeltaEntities)
            if (!deltaEntitiesClone.Contains(deltaEntity))
                syncStatus += "DeSynced2:\n" + deltaEntity.ToString() + "\n";
        if (syncStatus.Length == 0)
            syncStatus = "Synced";
        return syncStatus;
    }


    // ***************************
    // ********** Utils **********
    // ***************************

    public T GetEntity<T>(HexVector position) where T : Entity{
        GridVector gridPosition = (GridVector)position;
        if (gridPosition.x < 0 || gridPosition.x >= Board.BoardSize || gridPosition.y < 0 || gridPosition.y >= Board.BoardSize){
            foreach(UniversalDeltaEntity universalDeltaEntity in UniversalDeltaEntities.Values)
                if (universalDeltaEntity.Position == position && universalDeltaEntity is T)
                    return universalDeltaEntity as T;
            return null;
        }

        int entityType = (typeof(T).IsSubclassOf(typeof(Tile)) || typeof(T) == typeof(Tile)) ? 0 : 1;
        Entity entity = EntityBoard[entityType][gridPosition.x, gridPosition.y];
        if (entity == null)
            return null;
        return entity as T;
    }
    public List<T> GetEntities<T>() where T : Entity{
        List<T> entities = new();
        if (typeof(T).IsSubclassOf(typeof(Tile)) || typeof(T) == typeof(Tile)){
            for (int i = 0; i < Board.BoardSize; i++)
                for (int j = 0; j < Board.BoardSize; j++)
                    if (EntityBoard[0][i, j] is T)
                        entities.Add(EntityBoard[0][i, j] as T);
            return entities;
        }
        if (typeof(T).IsSubclassOf(typeof(RealDeltaEntity)) || typeof(T) == typeof(RealDeltaEntity)){
            foreach(RealDeltaEntity entity in DeltaEntities)
                if (entity is T)
                    entities.Add(entity as T);
            return entities;
        }
        foreach(UniversalDeltaEntity entity in UniversalDeltaEntities.Values)
            if (entity is T)
                entities.Add(entity as T);
        return entities;
    }

    public T GetClosestEntity<T>(HexVector position) where T : RealDeltaEntity => GetClosestEntity<T>(position, Board.BoardSize);

    public T GetClosestEntity<T>(HexVector position, int range) where T : RealDeltaEntity{
        for (int r = 1; r < range; r++){
            List<HexVector> hexes = HexVector.HexRing(position, r);
            for (int i = 0; i < hexes.Count; i++){
                GridVector gridPosition = (GridVector)hexes[i];
                if (Board.IsHexInBounds(hexes[i]) && EntityBoard[1][gridPosition.x, gridPosition.y] is T)
                    return EntityBoard[1][gridPosition.x, gridPosition.y] as T;
            }
        }
        return null;
    }

    public List<Entity> EveryEntity(){
        List<Entity> entities = new();
        int BoardSize = Board.BoardSize;
        for (int i = 0; i < BoardSize; i++)
            for (int j = 0; j < BoardSize; j++)
                for (int entityType = 0; entityType < 2; entityType++)
                    if (EntityBoard[entityType][i, j] != null)
                        entities.Add(EntityBoard[entityType][i, j]);
        foreach(UniversalDeltaEntity universalDeltaEntity in UniversalDeltaEntities.Values)
            entities.Add(universalDeltaEntity);
        return entities;
    }

    public HexVector? NextEmptySpot(HexVector position){
        for (int r = 1; r < Board.BoardSize; r++){
            List<HexVector> hexes = HexVector.HexRing(position, r);
            for (int i = 0; i < hexes.Count; i++){
                GridVector gridPosition = (GridVector)hexes[i];
                if (Board.IsHexInBounds(hexes[i]) && EntityBoard[1][gridPosition.x, gridPosition.y] == null)
                    return hexes[i];
            }
        }
        return null;
    }
    public bool WithinRadius<T>(HexVector position, int radius) where T : RealDeltaEntity{
        for (int r = 1; r < radius; r++){
            List<HexVector> hexes = HexVector.HexRing(position, r);
            for (int i = 0; i < hexes.Count; i++){
                GridVector gridPosition = (GridVector)hexes[i];
                if (Board.IsHexInBounds(hexes[i]) && EntityBoard[1][gridPosition.x, gridPosition.y] is T)
                    return true;
            }
        }
        return false;
    }
}