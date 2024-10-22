using UnityEngine;
using System.Collections.Generic;

public struct HexVector : System.IEquatable<HexVector>
{
    private const float IsometricScale = 0.6495f;
    public int x;
    public int y;
    public HexVector(int x, int y)
    {
        this.x = x;
        this.y = y;
    }

    public static List<HexVector> HexRadius(HexVector center, int radius){
        List<HexVector> results = new();
        for (int q = -radius; q <= radius; q++){
            int r1 = Mathf.Max(-radius, -q - radius);
            int r2 = Mathf.Min(radius, -q + radius);
            for (int r = r1; r <= r2; r++)
                results.Add(new HexVector(center.x + q, center.y + r));
        }
        return results;
    }
    public static int Distance(HexVector a, HexVector b) => (Mathf.Abs(a.x - b.x) + Mathf.Abs(a.x + a.y - b.x - b.y) + Mathf.Abs(a.y - b.y)) / 2;

    // starting from 0 radians, counter clockwise (like unit circle)
    // (+1, 0), (-1, 0) is right and left
    // (0, +1), (0, -1) is up left and down right
    // (+1, -1), (-1, +1) is up right and down left
    public static readonly HexVector[] AxialDirectionVectors = new HexVector[]{ new(+1, 0), new(+1, -1), new(0, -1), new(-1, 0), new(-1, +1), new(0, +1)};
    public static HexVector Neighbor(HexVector hex, int direction) => hex + AxialDirectionVectors[direction];
    public static HexVector[] Neighbors(HexVector hex) => new HexVector[]{ hex + AxialDirectionVectors[0], hex + AxialDirectionVectors[1], hex + AxialDirectionVectors[2], hex + AxialDirectionVectors[3], hex + AxialDirectionVectors[4], hex + AxialDirectionVectors[5]};
    public static HexVector[] LineDraw(HexVector a, HexVector b){
        int N = Distance(a, b);
        HexVector[] results = new HexVector[N + 1];
        for (int i = 0; i <= N; i++)
            results[i] = Round(Lerp(a, b, 1.0f / N * i));
        return results;
    }
    public static HexVector ClosestToHex(HexVector a, HexVector b) => Round(Lerp(a, b, 1.0f / Distance(a, b)));
    private static Vector2 Lerp(HexVector a, HexVector b, float t) => new(Mathf.Lerp(a.x, b.x, t), Mathf.Lerp(a.y, b.y, t));
    private static HexVector Round(Vector2 axialFrac){
        Vector3 frac = new(axialFrac.x, axialFrac.y, -axialFrac.x - axialFrac.y);

        int q = Mathf.RoundToInt(frac.x);
        int r = Mathf.RoundToInt(frac.y);
        int s = Mathf.RoundToInt(frac.z);

        float q_diff = Mathf.Abs(q - frac.x);
        float r_diff = Mathf.Abs(r - frac.y);
        float s_diff = Mathf.Abs(s - frac.z);

        if (q_diff > r_diff && q_diff > s_diff)
            q = -r - s;
        else if (r_diff > s_diff)
            r = -q - s;

        return new HexVector(q, r);
    }

    public static List<HexVector> HexRing(HexVector center, int radius){
        List<HexVector> results = new();
        HexVector hex = center + AxialDirectionVectors[4] * radius;
        for (int i = 0; i < 6; i++){
            for (int j = 0; j < radius; j++){
                results.Add(hex);
                hex = Neighbor(hex, i);
            }
        }
        return results;
    }

    public static explicit operator HexVector(GridVector gridVector) => new(gridVector.x - gridVector.y / 2, gridVector.y);
    public static explicit operator Vector2(HexVector hex) => new(Mathf.Sqrt(3) * hex.x + Mathf.Sqrt(3) / 2 * hex.y, IsometricScale * (3.0f / 2.0f * hex.y));
    public static explicit operator HexVector(Vector2 pixel) => Round(new Vector2(Mathf.Sqrt(3) / 3 * pixel.x - 1.0f / (3.0f * IsometricScale) * pixel.y, 2.0f / (3.0f * IsometricScale) * pixel.y)); //inverted matrix of HexToPixel

    public static HexVector operator +(HexVector a, HexVector b) => new(a.x + b.x, a.y + b.y);
    public static HexVector operator -(HexVector a, HexVector b) => new(a.x - b.x, a.y - b.y);
    public static HexVector operator *(HexVector a, int b) => new(a.x * b, a.y * b);
    public static HexVector operator /(HexVector a, int b) => new(a.x / b, a.y / b);
    public static bool operator ==(HexVector a, HexVector b) => a.x == b.x && a.y == b.y;
    public static bool operator !=(HexVector a, HexVector b) => a.x != b.x || a.y != b.y;
    public static bool operator >(HexVector a, HexVector b) => a.x > b.x && a.y > b.y;
    public static bool operator <(HexVector a, HexVector b) => a.x < b.x && a.y < b.y;
    public static bool operator >=(HexVector a, HexVector b) => a.x >= b.x && a.y >= b.y;
    public static bool operator <=(HexVector a, HexVector b) => a.x <= b.x && a.y <= b.y;
    public static HexVector operator -(HexVector a) => new(-a.x, -a.y);
    public override bool Equals(object obj)
    {
        if (obj == null || GetType() != obj.GetType())
            return false;
        HexVector hexVector = (HexVector)obj;
        return x == hexVector.x && y == hexVector.y;
    }
    public bool Equals(HexVector hexVector) => x == hexVector.x && y == hexVector.y;
    public override int GetHashCode() => x.GetHashCode() ^ y.GetHashCode();
    public override string ToString() => "HexVector(" + x + ", " + y + ")";
}
