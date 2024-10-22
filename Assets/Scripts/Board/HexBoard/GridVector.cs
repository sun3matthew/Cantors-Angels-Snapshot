using UnityEngine;

public struct GridVector : System.IEquatable<GridVector>
{
    public int x;
    public int y;
    public GridVector(int x, int y)
    {
        this.x = x;
        this.y = y;
    }
    public static explicit operator GridVector(HexVector hexVector) => new(hexVector.x + hexVector.y / 2, hexVector.y);

    public static GridVector operator +(GridVector a, GridVector b) => new(a.x + b.x, a.y + b.y);
    public static GridVector operator -(GridVector a, GridVector b) => new(a.x - b.x, a.y - b.y);
    public static GridVector operator *(GridVector a, int b) => new(a.x * b, a.y * b);
    public static GridVector operator /(GridVector a, int b) => new(a.x / b, a.y / b);
    public static bool operator ==(GridVector a, GridVector b) => a.x == b.x && a.y == b.y;
    public static bool operator !=(GridVector a, GridVector b) => a.x != b.x || a.y != b.y;
    public static bool operator >(GridVector a, GridVector b) => a.x > b.x && a.y > b.y;
    public static bool operator <(GridVector a, GridVector b) => a.x < b.x && a.y < b.y;
    public static bool operator >=(GridVector a, GridVector b) => a.x >= b.x && a.y >= b.y;
    public static bool operator <=(GridVector a, GridVector b) => a.x <= b.x && a.y <= b.y;
    public static GridVector operator -(GridVector a) => new(-a.x, -a.y);
    public override bool Equals(object obj)
    {
        if (obj == null || GetType() != obj.GetType())
            return false;
        GridVector gridVector = (GridVector)obj;
        return x == gridVector.x && y == gridVector.y;
    }
    public bool Equals(GridVector other) => x == other.x && y == other.y;
    public override int GetHashCode() => x.GetHashCode() ^ y.GetHashCode();
    public override string ToString() => "GridVector: (" + x + ", " + y + ")";
}
