using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class BoardHistory
{
    public static HistoryNode Root;
    public static HistoryNode Current;
    public static Dictionary<long, BoardDelta> BoardCache;
    public static List<HistoryDelta> DeltaSequence;
    // TODO If the root has a new child being added, it needs to be on a new save file.
    public static int CurrentNumChildren { get { return Current.Children.Count; } }
    public static void Initialize(){
        BoardCache = new Dictionary<long, BoardDelta>();
        DeltaSequence = new List<HistoryDelta>();
        Root = null;
        Current = null;
    }
    public static bool CheckHash(long hash) => !BoardCache.ContainsKey(hash);
    public static bool AddBoard(BoardDelta board, List<MonoDelta> deltas, bool soft){
        // Debug.Log("Adding board " + SaveUtility.ToHexString(board.Hash));
        if (Root != null && Current == Root && Root.Children.Count >= 1 && !soft){
            Debug.LogError("Root has more than one child, un-implemented.");
            return false;
        }

        // if (SaveUtility.ToHexString(board.Hash) == "1E74C4A69DC45D59")
        //     CoreLoop.DumpBoard(Board.Instance);
        long hash = board.Hash;
        if(BoardCache.ContainsKey(hash)){
            Debug.Log("HASH COLLISION " + SaveUtility.ToHexString(hash));
            return false;
        }
        BoardCache.Add(hash, board);

        DeltaSequence.Add(new HistoryDelta(Current == null ? 0 : Current.Hash, deltas));

        if (soft){
            if(FindNode(Root, hash) == null)
                Debug.LogError("Could not find node with hash " + SaveUtility.ToHexString(hash));
            Current = FindNode(Root, hash); // * I think this is necessary to make sure the history and board state are in sync.
        }else{
            HistoryNode node = new(hash);
            if(Root == null){
                Root = node;
                Current = node;
            }
            else{
                Current.AddChild(node);
                Current = node;
            }
        }
        return true;
    }

    public static void Reset(){
        Root = null;
        Current = null;
        BoardCache = new Dictionary<long, BoardDelta>();
        DeltaSequence = new List<HistoryDelta>();
    }

    public static int IndexOfChild(long hash){
        for(int i = 0; i < Current.Children.Count; i++)
            if(Current.Children[i].Hash == hash)
                return i;
        return Current.Children.Count;
    }

    public static void Undo(){
        if(Current.Parent == null)
            return;
        Board.Instance.ReloadBoardState(BoardCache[Current.Hash], false);
        Current = Current.Parent;
    }
    //Redo to the last child, branches
    public static void Redo(int index){
        if(index >= Current.Children.Count || index < 0)
            return;
        Board.Instance.ReloadBoardState(BoardCache[Current.Children[index].Hash], true);
        Current = Current.Children[index];
    }
    public static void StepForwards(int index){
        if(index >= Current.Children.Count || index < 0)
            return;
        Board.Instance.StepForwards();
        Current = Current.Children[index];
    }
    public static string ToDebugString(){
        string RawString = GetHistoryStringHelper(Root, 0);
        string[] SplitString = RawString.Split('\n');
        // from bottom up
        for (int i = SplitString.Length - 1; i > 0; i--){
            if (SplitString[i].Length < 1)
                continue;
            bool canMerge = true;
            for (int j = 0; j < SplitString[i].Length; j++){
                if(SplitString[i][j] != ' ' && SplitString[i][j] != '|' && SplitString[i][j] != ';'){
                    if(j < SplitString[i - 1].Length && SplitString[i - 1][j] != ' '){
                        canMerge = false;
                        break;
                    }
                }
            }
            if(canMerge){
                string newString = "";
                string longerString = SplitString[i - 1];
                string shorterString = SplitString[i];
                if (SplitString[i].Length > SplitString[i - 1].Length){
                    longerString = SplitString[i];
                    shorterString = SplitString[i - 1];
                }
                for (int j = 0; j < longerString.Length; j++){
                    if(j < shorterString.Length && shorterString[j] != ' ' && longerString[j] != ';')
                        newString += shorterString[j];
                    else
                        newString += longerString[j];
                }
                SplitString[i - 1] = newString;
                SplitString[i] = "";
            }
        }
        string FinalString = "";
        for (int i = 0; i < SplitString.Length; i++)
            if (SplitString[i].Length > 0)
                FinalString += SplitString[i] + "\n";
        return FinalString;
    }
    private static string GetHistoryStringHelper(HistoryNode node, int depth){
        string historyString = "";
        for(int i = 0; i < depth; i++)
            historyString += "                |";
            // historyString += "               |";
        historyString += SaveUtility.ToHexString(node.Hash) + (node.Children.Count == 0 ? ";" : "|") + "\n";
        foreach(HistoryNode child in node.Children)
            historyString += GetHistoryStringHelper(child, depth + 1);
        return historyString;
    }

    public static void Serialize(List<byte> bytes){
        SaveFile.WriteString(bytes, "BoardHistory");

        SaveFile.WriteInt(bytes, DeltaSequence.Count);
        foreach(HistoryDelta delta in DeltaSequence)
            delta.Serialize(bytes);

        SaveFile.WriteString(bytes, "HistoryTree");

        // Serialize the history tree
        Stack<HistoryNode> stack = new();
        stack.Push(Root);
        while(stack.Count > 0){
            HistoryNode node = stack.Pop();
            SaveFile.WriteLong(bytes, node.Hash);
            // Debug.Log("Writing Node: " + SaveUtility.ToHexString(node.Hash));
            SaveFile.WriteInt(bytes, node.Children.Count);
            // Debug.Log("Writing NumChildren: " + node.Children.Count);

            foreach(HistoryNode child in node.Children)
                stack.Push(child);
        }

        long currentHash = Current.Hash;
        SaveFile.WriteLong(bytes, currentHash);
    }

    public static void Deserialize(byte[] bytes, ref int index){
        string header = SaveFile.ReadString(bytes, ref index);

        DeltaSequence = new();

        int numDeltas = SaveFile.ReadInt(bytes, ref index);
        for(int i = 0; i < numDeltas; i++)
            DeltaSequence.Add(HistoryDelta.Deserialize(bytes, ref index));

        header = SaveFile.ReadString(bytes, ref index);

        Root = new(SaveFile.ReadLong(bytes, ref index));
        int numChildren = SaveFile.ReadInt(bytes, ref index);
        Stack<HistoryNode> stack = new();
        for (int i = 0; i < numChildren; i++)
            stack.Push(Root);

        while(stack.Count > 0){
            HistoryNode child = new(SaveFile.ReadLong(bytes, ref index));
            stack.Pop().AddChild(child);
            numChildren = SaveFile.ReadInt(bytes, ref index);
            for (int i = 0; i < numChildren; i++)
                stack.Push(child);
        }

        long currentHash = SaveFile.ReadLong(bytes, ref index);
        Current = FindNode(Root, currentHash);
    }

    // Fit in 64 * 4 bytes, I want a few bytes to represent hash to combine simmilar hashes. Think about it more.
    // Experiment with steam leaderBoard to see how hard it is to work with UGC.
    // For now just ignore byte limit, dump hashes one by one such that you can reconstruct the tree fast.
    // Add helper to generate fake trees to load in to network, and some tree save file binding.
    // Then add secondary UI for game, integrate network and graph into the game as well as the other graph. More the better? Maybe too cluttered tho.
    // I think remove it, unnecessary if the graph is easy to use.
        // Add proposed deltas to the graph.
    // ? Then after all this add UI
    // All of this is to make it easy to play, game is fun, hard to learn - confusing.
    public static byte[] SerializeGraph(HistoryNode root){
        // turn a hash into 1 byte, truncate
        List<byte> bytes = new();
        Stack<HistoryNode> stack = new();
        stack.Push(root);
        while(stack.Count > 0){
            HistoryNode node = stack.Pop();
            SaveFile.WriteLong(bytes, node.Hash);
            SaveFile.WriteInt(bytes, node.Children.Count);

            foreach(HistoryNode child in node.Children)
                stack.Push(child);
        }
        return bytes.ToArray();
    }
    public static HistoryNode DeserializeGraph(byte[] bytes){
        int index = 0;
        HistoryNode root = new(SaveFile.ReadLong(bytes, ref index));
        int numChildren = SaveFile.ReadInt(bytes, ref index);
        Stack<HistoryNode> stack = new();
        for (int i = 0; i < numChildren; i++)
            stack.Push(root);

        while(stack.Count > 0){
            HistoryNode child = new(SaveFile.ReadLong(bytes, ref index));
            stack.Pop().AddChild(child);
            numChildren = SaveFile.ReadInt(bytes, ref index);
            for (int i = 0; i < numChildren; i++)
                stack.Push(child);
        }
        return root;
    }
    

    private static HistoryNode FindNode(HistoryNode node, long hash){
        if(node.Hash == hash)
            return node;
        Stack<HistoryNode> stack = new();
        stack.Push(node);
        while(stack.Count > 0){
            HistoryNode current = stack.Pop();
            if(current.Hash == hash)
                return current;
            foreach(HistoryNode child in current.Children)
                stack.Push(child);
        }
        Debug.LogError("Could not find node with hash " + SaveUtility.ToHexString(hash));
        return null;
    }

    public static void TraverseTo(long hash){
        List<long> path = FindPath(hash);
        if(path == null){
            Debug.LogError("Could not find path to hash " + SaveUtility.ToHexString(hash));
            return;
        }
        foreach(long nodeHash in path)
            MoveTowards(nodeHash);
    }

    private static void MoveTowards(long hash){
        // Debug.Log("Moving towards " + SaveUtility.ToHexString(hash));
        if(Current.Hash == hash)
            return;
        foreach(HistoryNode child in Current.Children)
            if(child.Hash == hash){
                Board.Instance.ReloadBoardState(BoardCache[child.Hash], true);
                Current = child;
                return;
            }
        if (Current.Parent != null && Current.Parent.Hash == hash){
            // * Not Current.Parent.Hash, but Current.Hash since BoardDeltas are in the format (Target Hash, Deltas to get to Hash).
            // * So to undo, you must undo the delta that got to the current hash, not undo the delta to get to the next hash.
            Board.Instance.ReloadBoardState(BoardCache[Current.Hash], false); 
            Current = Current.Parent;
        }
    }
    // Traverse to find the path from current to end
    private static List<long> FindPath(long end){
        HistoryNode start = Current;
        Queue<HistoryNode> queue = new();
        Dictionary<long, long> parent = new();
        queue.Enqueue(start);
        while(queue.Count > 0){
            HistoryNode node = queue.Dequeue();
            if(node.Hash == end){
                List<long> path = new();
                long current = end;
                while(current != start.Hash){
                    path.Add(current);
                    current = parent[current];
                }
                path.Add(start.Hash);
                path.Reverse();
                return path;
            }
            foreach(HistoryNode child in node.Children){
                if (!parent.ContainsKey(child.Hash)){
                    queue.Enqueue(child);
                    parent[child.Hash] = node.Hash;
                }
            }
            if(node.Parent != null){
                if (!parent.ContainsKey(node.Parent.Hash)){
                    queue.Enqueue(node.Parent);
                    parent[node.Parent.Hash] = node.Hash;
                }
            }
        }
        return null;
    }
    public static List<HistoryDelta> ExtractHistory(){
        List<HistoryDelta> history = DeltaSequence;
        DeltaSequence = new();
        return history;
    }
}
