using System.Collections.Generic;
using UnityEngine;

// * I want the actions of hadics to be localized, as in you only need to fight one battle at a time, ui wise. Like in civ it sucks that you need to keep jumping.
public class Board
{
    public static Board Instance { get; private set; }

    public int Seed { get; private set; }
    public int BoardSize { get; private set; }
    public int WorkingBoard { get; private set; }
    private ExposedList<BoardState> BoardStates; // Always starts on the last turn (base board, un-changable), then its the current turn as your next turn, then enemy. Number of turns ahead is the number of elements / 2
    public List<long> BoardStateHashes { get; private set;}
    public BoardState Current => BoardStates[WorkingBoard];

    public int[,] Elevation; // idk if this should be in it's own class, like a static board data class
    public int[,] MassNumber;

    public bool IsHexInBounds(HexVector xY) => IsGridInBounds((GridVector)xY);
    public bool IsGridInBounds(GridVector gridVector) => gridVector.x >= 0 && gridVector.x < BoardSize && gridVector.y >= 0 && gridVector.y < BoardSize;

    public Board(int boardSize, int seed)
    {
        Instance = this;

        BoardSize = boardSize;
        Seed = seed;

        BoardStates = new ExposedList<BoardState>(32, () => new BoardState(boardSize, this));
        BoardStates.IncreaseCount(); // Base board
        BoardStates.IncreaseCount();

        Elevation = new int[BoardSize, BoardSize];
        MassNumber = new int[BoardSize, BoardSize];

        WorkingBoard = 0; //! 0 just so board creator can initialize the board, then it should be set to 1

        BoardStateHashes = new List<long>();

        Entity.SetBoardState(Current);
    }
    // Move the base board forwards or backwards with a delta.
    public void ReloadBoardState(BoardDelta boardDelta, bool direction){
        BoardStates[0].ApplyDelta(direction, boardDelta);

        CreateBaseStatesRaw();
        SetWorkingBoard(1);

        // TODO Make this work
        // BoardStateHashes = new List<long>();
        // for (int i = 0; i < BoardStates.Count; i++) // ? unnecessary to go through all of them?
        //     BoardStateHashes.Add(BoardStates[i].BruteHash());
    }
    public void CreateBaseStatesRaw(){
        BoardStates.SetCount(2);
        BoardStates[1].Reset(BoardStates[0]);
    }
    public void RegenerateDeltas(int start){
        // you skip 0 since its the base board
        if (BoardStates.Count <= 1){
            Debug.LogError("Invalid board states count: " + BoardStates.Count + ", how the f*ck did you get here?");
            return;
        }

        if (start % 2 == 1)
            start++;
        if (start < 1)
            start = 1;

        if (BoardStates.Count % 2 == 0) // unpaired board state
            BoardStates.IncreaseCount();

        for (int i = start; i < BoardStates.Count; i++){
            if (i % 2 == 0){ // enemy
                BoardStates[i].Reset(BoardStates[i - 1]);
                BoardStates[i].CreateAutoDeltas();
            }else{
                BoardStates[i].SoftReset(BoardStates[i - 1]);
                BoardStates[i].ReapplyDeltas();
            }
        }

        RegenerateHashes();
    }
    public void RegenerateHashes(){
        List<long> newHashes = new();
        // Only put enemy board states, since every two merges into one and the enemy hash is the actual.
        // start at 2, ignore base and first user
        for (int i = 2; i < BoardStates.Count; i++)
            if (i % 2 == 0)
                newHashes.Add(BoardStates[i].BruteHash());
        BoardStateHashes = newHashes;
    }

    public void SetNumUserBoardStates(int num){
        BoardStates.SetCount(num * 2 + 1);
        for (int i = 1; i < BoardStates.Count; i++)
            BoardStates[i].Reset(BoardStates[i - 1]);
        RegenerateDeltas(1);
    }
    public void GenerateNextUserBoard(){
        if (BoardStates.Count % 2 == 0){
            Debug.LogError("There should always be an enemy board state here.");
            return;
        }
        BoardStates.IncreaseCount();
        BoardStates[BoardStates.Count - 1].Reset(BoardStates[BoardStates.Count - 2]); // you need to reset, since regenerate deltas does a soft.
        RegenerateDeltas(BoardStates.Count - 1);
    }
    public void SetWorkingBoard(int workingBoard)
    {
        if(workingBoard < 1 || workingBoard >= BoardStates.Count){
            // Debug.LogError("Invalid working board: " + workingBoard + ", board states: " + BoardStates.Count);
            return;
        }
        WorkingBoard = workingBoard;
        Entity.SetBoardState(Current);
    }

    public void UndoMonoDeltaAssociatedWith(RealDeltaEntity entity){
        if (BoardStates.Count % 2 == 0){
            Debug.LogError("There should always be an enemy board state here.");
            return;
        }
        int start = 0;
        for (int i = BoardStates.Count - 1; i >= 0; i--){
            if (i % 2 == 1){
                if (BoardStates[i].RemoveMonoDeltaAssociatedWith(entity.ID)){
                    start = i;
                    break;
                }
            }
        }
        RegenerateDeltas(start - 1);
    }

    //* Commits turn 1 & 2(enemy auto) to turn 0
    // public void CommitTurn(bool soft)
    public void CommitTurn(bool soft)
    {
        if(!soft)
            SaveUtility.DumpToBoardHash(this);

        if(BoardStates.Count % 2 == 0){
            Debug.LogError("Unpaired Board State");
            return;
        }

        BoardDelta boardDelta = GenerateCurrentBoardDelta();
        BoardHistory.AddBoard(boardDelta, BoardStates[1].ExtractMonoDeltas(), soft);
        StepForwards();
        // RegenerateDeltas(0);
    }
    public void StepForwards(){
        BoardStates.RemoveAt(0); // Remove base
        BoardStates.RemoveAt(0); // Remove player
        WorkingBoard -= 2;
        // Now at end of enemy, aka new base.
        if (WorkingBoard < 1)
            WorkingBoard = 1;
        if(BoardStates.Count == 1){
            WorkingBoard = 1;
            BoardStates.SetCount(2); // base, player
            BoardStates[1].Reset(BoardStates[0]);
            RegenerateDeltas(0);
        }
    }
    public BoardDelta GenerateCurrentBoardDelta(){
        List<EntityDelta> pureEntityDeltas = BoardStates[1].GetEntityDeltas();
        pureEntityDeltas.AddRange(BoardStates[2].GetEntityDeltas());
        return new BoardDelta(BoardStates[2].BruteHash(), pureEntityDeltas);
    }
    public void DecreaseBoardStateCount(){
        if (BoardStates.Count <= 2){
            Debug.LogError("Invalid board states count: " + BoardStates.Count);
            return;
        }
        if (BoardStates.Count % 2 == 0){
            Debug.LogError("Unpaired Board State");
            return;
        }

        BoardStates.SetCount(BoardStates.Count - 2);

        if (WorkingBoard >= BoardStates.Count)
            SetWorkingBoard(BoardStates.Count - 2);

        RegenerateHashes();
    }
    public int NumberOfBoardStates() => BoardStates.Count;
    public List<BoardState> GetBoardStatesToRender()
    {
        List<BoardState> boardStates = new();
        for (int i = 0; i < BoardStates.Count; i++)
            boardStates.Add(BoardStates[i]);
        return boardStates;
    }
    public int PositionToID(GridVector position) => position.x * BoardSize + position.y;
    public GridVector IDToPosition(int idx) => new(idx / BoardSize, idx % BoardSize);
    public int TurnNumberOf(BoardState boardState) => BoardHistory.Current.TurnNumber * 2 + IndexOf(boardState);
    private int IndexOf(BoardState boardState){
        for (int i = 0; i < BoardStates.Count; i++){
            if (BoardStates[i] == boardState)
                return i;
        }
        return -1;
    }
    public override string ToString()
    {
        string boardString = "";
        boardString += "BoardStates: " + BoardStates.Count + "\n";
        boardString += "WorkingBoard: " + WorkingBoard + "\n";
        boardString += "\n\n\n";
        for (int i = 0; i < BoardStates.Count; i++)
            boardString += "BoardState " + i + ":\n" + BoardStates[i].ToString() + "\n\n\n";
        return boardString;
    }
    public string FirstThreeBoards(){
        string boardString = "";
        for (int i = 0; i < 3; i++)
            boardString += "BoardState " + i + ":\n" + BoardStates[i].ToString() + "\n\n\n";
        return boardString;
    }

    public void Serialize(List<byte> bytes)
    {
        SaveFile.WriteString(bytes, "Board");
        SaveFile.WriteInt(bytes, Seed);
        SaveFile.WriteInt(bytes, BoardSize);
        SaveFile.WriteInt(bytes, WorkingBoard);


        BoardHistory.Serialize(bytes);

        SaveFile.WriteInt(bytes, BoardStates.Count);
        for (int i = 1; i < BoardStates.Count; i++)
            BoardStates[i].Serialize(bytes);
    }

    public static void Deserialize(byte[] bytes, ref int index)
    {
        string header = SaveFile.ReadString(bytes, ref index);
        int seed = SaveFile.ReadInt(bytes, ref index);
        int boardSize = SaveFile.ReadInt(bytes, ref index);
        int workingBoard = SaveFile.ReadInt(bytes, ref index);

        Debug.Log("Seed: " + seed + ", BoardSize: " + boardSize + ", WorkingBoard: " + workingBoard);


        BoardHistory.Initialize();

        Board board = BoardCreator.CreateBoardFromSeed(seed);
        PathFinding.Initialize(board.Current);

        BoardHistory.Deserialize(bytes, ref index);
        List<HistoryDelta> historyDeltas = BoardHistory.ExtractHistory();

        long currentHash = BoardHistory.Current.Hash;
        BoardHistory.Current = BoardHistory.Root;

        foreach (HistoryDelta historyDelta in historyDeltas){
            // Skip root
            if (historyDelta.ParentHash == 0)
                continue;

            BoardHistory.TraverseTo(historyDelta.ParentHash);

            if (BoardHistory.Current.Hash != board.Current.BruteHash())
                Debug.LogError("Hashes don't match: " + SaveUtility.ToHexString(BoardHistory.Current.Hash) + " != " + SaveUtility.ToHexString(board.Current.BruteHash()));

            List<MonoDelta> monoDeltas = historyDelta.MonoDeltas;
            foreach (MonoDelta monoDelta in monoDeltas)
                board.Current.InjectMonoDelta(monoDelta, null);
            board.Current.ReapplyDeltas();

            board.RegenerateDeltas(2);
            board.CommitTurn(true);
        }

        BoardHistory.TraverseTo(currentHash);

        int boardStatesCount = SaveFile.ReadInt(bytes, ref index);
        board.BoardStates.SetCount(boardStatesCount);
        for (int i = 1; i < boardStatesCount; i++){
            board.BoardStates[i] = BoardState.Deserialize(bytes, ref index, board);
        }
        board.BoardStates[1].SoftReset(board.BoardStates[0]);
        board.BoardStates[1].ReapplyDeltas();

        board.RegenerateDeltas(2);
        board.SetWorkingBoard(workingBoard);
    }
    



    public static void DumpBoard(){
        Debug.Log("Dumping Board");
        System.Func<int, string> boardPath = (int i) => System.IO.Directory.GetCurrentDirectory() + "/Logs/BoardDump" + (i == 0 ? "" : i.ToString()) + ".log";
        int fileBuffer = 16;
        for (int i = fileBuffer - 2; i >= 0; i--){
            if (SaveUtility.FileExists(boardPath(i))){
                byte[] buffer = SaveUtility.ReadFile(boardPath(i));
                SaveUtility.WriteFile(boardPath(i + 1), buffer);
            }
        }
        SaveUtility.WriteFile(boardPath(0), SaveUtility.ToBytes(Instance.ToString()));

        SaveUtility.WriteFile(System.IO.Directory.GetCurrentDirectory() + "/Logs/BoardHistoryDump.log", SaveUtility.ToBytes(BoardHistory.ToDebugString()));
    }
}
