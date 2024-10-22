using System.Collections.Generic;

public class HistoryNode
{
    public long Hash { get; private set; }
    public List<HistoryNode> Children { get; private set; }
    public HistoryNode Parent { get; private set; }
    public int TurnNumber { get; private set; }
    public int IdxWithinNetwork { get; private set; }
    public HistoryNode(long hash)
    {
        Hash = hash;
        Children = new List<HistoryNode>();
        TurnNumber = 0;
        IdxWithinNetwork = 0;
    }
    public HistoryNode(HistoryNode historyNode)
    {
        Hash = historyNode.Hash;
        Children = new List<HistoryNode>();
        foreach (HistoryNode child in historyNode.Children)
            Children.Add(new HistoryNode(child));
        Parent = historyNode.Parent;
        TurnNumber = historyNode.TurnNumber;
    }
    public void SetIdxWithinNetwork(int idxWithinNetwork) => IdxWithinNetwork = idxWithinNetwork;
    public void AddChild(HistoryNode child)
    {
        Children.Add(child);
        child.Parent = this;
        child.TurnNumber = TurnNumber + 1;
    }


    public string ToUIString(int selection, long nextUncommittedHash)
    {
        string nodeString = "<color=green><Last></color> " + (TurnNumber - 1) + ":" + (Parent == null ? "NULL" : SaveUtility.ToHexSubstring(Parent.Hash)) + "   *\n";
        nodeString += "<color=blue><Head></color> " + TurnNumber + ":" + SaveUtility.ToHexSubstring(Hash) + "   *\n";

        for (int i = Children.Count - 1; i >= 0; i--){
            nodeString += "/|\n";
            if (selection == i)
                nodeString += "<color=green><Next></color> ";
            nodeString += (TurnNumber + 1) + ":" + SaveUtility.ToHexSubstring(Children[i].Hash) + " *" + " |\n";
        }
        if (selection == Children.Count)
            nodeString += "<color=green><Next></color> ";
        nodeString += "<color=#000000c0>" + (TurnNumber + 1) + ":" + SaveUtility.ToHexSubstring(nextUncommittedHash) + "</color>" + "   *\n";
        return nodeString;
    }
    public override string ToString()
    {
        string toString = "";
        toString += "Hash: " + SaveUtility.ToHexSubstring(Hash) + "\n";
        toString += "TurnNumber: " + TurnNumber + "\n";
        toString += "IdxWithinNetwork: " + IdxWithinNetwork + "\n";
        toString += "Parent: " + (Parent == null ? "NULL" : SaveUtility.ToHexSubstring(Parent.Hash)) + "\n";
        toString += "Children: " + Children.Count + "\n";
        foreach (HistoryNode child in Children)
            toString += child.ToString();
        return toString;
    }

    public List<HistoryNode> GetTraversal()
    {
        List<HistoryNode> traversal = new List<HistoryNode>();
        traversal.Add(this);
        foreach (HistoryNode child in Children)
            traversal.AddRange(child.GetTraversal());
        return traversal;
    }
}
