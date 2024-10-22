using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using UnityEngine;
public abstract class Entity
{
    // ****************************
    // ********** Static **********
    // ****************************
    public static Dictionary<Type, EntityEnum> TypeToEnum;
    public static Dictionary<EntityEnum, Type> EnumToType;
    private static ulong IDCounter;
    public static void Initialize(){
        TypeToEnum = new Dictionary<Type, EntityEnum>();
        EnumToType = new Dictionary<EntityEnum, Type>();
        foreach (EntityEnum entityEnum in Enum.GetValues(typeof(EntityEnum)))
        {
            Type type = Type.GetType(entityEnum.ToString());
            if(type != null){
                TypeToEnum.Add(type, entityEnum);
                EnumToType.Add(entityEnum, type);
            }else{
                if(entityEnum != EntityEnum.Null)
                    Debug.LogError("Entity type not found: " + entityEnum.ToString());
            }
        }

        IDCounter = 1;
    }
    public static T Get<T>(EntityEnum entityEnum) where T : Entity => FormatterServices.GetUninitializedObject(EnumToType[entityEnum]) as T;
    public static T Get<T>() where T : Entity => FormatterServices.GetUninitializedObject(typeof(T)) as T;
    public static ulong GetID() => unchecked(IDCounter++); // guid eventually?

    public static BoardState BoardState { get; private set; } // ! Current board state, this will prevent async possibilities but I don't think it'll ever be async.
    public static void SetBoardState(BoardState boardState) => BoardState = boardState;



    // ****************************
    // ********** Entity **********
    // ****************************

    public EntityEnum EntityEnum { get; private set; }
    public HexVector Position { get; private set; }
    public GridVector GridPosition { get; protected set; } // Cache for performance
    public ulong ID { get; private set; } // Internal use only, never exposed.
    public int GetElevation(){
        GridVector position = GridPosition;
        return Board.Instance.Elevation[position.x, position.y];
    }
    public int GetMassNumber(){
        GridVector position = GridPosition;
        return Board.Instance.MassNumber[position.x, position.y];
    }

    // initialize from scratch
    public virtual Entity Initialize(HexVector position){
        Instantiate(position);
        ID = GetID();
        return this;
    }
    public virtual Entity Initialize(HexVector position, uint id){
        Instantiate(position);
        ID = id;
        return this;
    }
    // initialize from clone
    protected virtual Entity Initialize(Entity clone){
        Instantiate(clone.Position);
        ID = clone.ID;
        return this;
    }
    public virtual void Destroy() {
        EntityEnum = EntityEnum.Null;
    }
    private void Instantiate(HexVector position){
        EntityEnum = TypeToEnum[GetType()];
        SetPosition(position);
    }
    protected void SetPosition(HexVector position){
        Position = position;
        GridPosition = (GridVector)position;
    }

    public abstract AnimE StateAnimation();
    public abstract bool SyncAnimation();
    public virtual void UniversalRendererInit(UniversalRenderer universalRenderer, int idx){
        Board board = Board.Instance;
        if (idx == board.WorkingBoard){
            universalRenderer.SetOpacity(1);
        }else if (idx < board.WorkingBoard){
            universalRenderer.SetGrayScale(true);
            universalRenderer.SetOpacity((1 - (float)(board.WorkingBoard - idx) / (board.WorkingBoard + 1)) * 0.75f);
        }else{
            universalRenderer.SetOpacity((1 - (float)(idx - board.WorkingBoard) / (board.NumberOfBoardStates() - board.WorkingBoard)) * 0.75f);
        }
    }

    public virtual int CurrentFrame() => -1;
    public virtual int GetHash(ref int HashWrapper) => 0
        ^ Shift((int)EntityEnum, 4, ref HashWrapper)
        ^ Shift(Position.x, 8, ref HashWrapper)
        ^ Shift(Position.y, 8, ref HashWrapper);
    protected int Shift(int value, int positions, ref int HashWrapper){
        HashWrapper += positions;
        return SaveUtility.ShiftAndWrap(value, HashWrapper);
    }
    public override string ToString(){
        string entityString = "";
        entityString += "Entity: " + EntityEnum.ToString() + "\n";
        entityString += "Position: " + Position + "\n";
        entityString += "ID: " + ID + "\n";
        entityString += "Address: " + this.GetRefId<Entity>().ToString("N").Substring(16) + "\n";
        entityString += "BoardState: " + Board.Instance.TurnNumberOf(BoardState) + "\n"; // ! useless in dump
        return entityString;
    }
    public T Clone<T>() where T : Entity => Get<T>(EntityEnum).Initialize(this) as T;
}
