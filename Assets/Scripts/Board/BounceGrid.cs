using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.GlobalIllumination;
public class BounceGrid
{
    public class HarmonicOscillator
    {
        public const float SpringConstant = 0.4f;
        public const float Damping = 0.01f;
        public float BounceVelocity;
        public float Position;

        public HarmonicOscillator(){
            BounceVelocity = 0;
            Position = 0;
        }

        public void UpdateBounce(){
            float equilibrium = 0;
            float displacement = Position - equilibrium;
            float force = -SpringConstant * displacement;
            BounceVelocity += force * Time.deltaTime;
            float currentDamping = Damping;
            if (displacement < 0)
                currentDamping /= 1000;
            float deltaBounceVelocity = BounceVelocity * (1 - currentDamping);
            BounceVelocity -= deltaBounceVelocity * Time.deltaTime * 3;
            if (Mathf.Abs(BounceVelocity) < 0.003f && Mathf.Abs(displacement) < 0.003f){
                BounceVelocity = 0;
                Position = equilibrium;
            }
            Position += BounceVelocity * Time.deltaTime * 30;
        }
    }
    public struct BounceForce{
        public GridVector gridVector;
        public float force;
        public float time;
    }
    private HarmonicOscillator[,] BounceMap;
    private List<HarmonicOscillator> ActiveOscillators;
    private List<BounceForce> BounceForces;
    public BounceGrid(int BoardSize){
        BounceMap = new HarmonicOscillator[BoardSize, BoardSize];
        for (int i = 0; i < BoardSize; i++)
            for (int j = 0; j < BoardSize; j++)
                BounceMap[i, j] = new HarmonicOscillator();
        ActiveOscillators = new List<HarmonicOscillator>();
        BounceForces = new List<BounceForce>();
    }

    public void UpdateBounce(){
        for (int i = 0; i < BounceForces.Count; i++){
            BounceForce bounceForce = BounceForces[i];
            if (Time.realtimeSinceStartup > bounceForce.time){
                AddBounceVelocity(bounceForce.gridVector, bounceForce.force);
                BounceForces.RemoveAt(i);
                i--;
            }
        }

        for (int i = 0; i < ActiveOscillators.Count; i++){
            ActiveOscillators[i].UpdateBounce();
            if (ActiveOscillators[i].BounceVelocity == 0 && ActiveOscillators[i].Position == 0){
                ActiveOscillators.RemoveAt(i);
                i--;
            }
        }
    }

    public void AddSingleBounceForce(GridVector gridVector, float force, float time) => BounceForces.Add(new BounceForce{gridVector = gridVector, force = force, time = time});
    // From 0 to innerRadius, slight downward, from innerRadius to outerRadius, slight upward like a shockwave
    public void AddBounceImpact(GridVector gridVector, float force, int innerRadius, int peakRadius, int outerRadius, float delay){
        HexVector hexVector = (HexVector)gridVector;
        List<HexVector> hexVectors = HexVector.HexRadius(hexVector, outerRadius);
        foreach(HexVector hex in hexVectors){
            if (!Board.Instance.IsHexInBounds(hex))
                continue;
            int currentRadius = HexVector.Distance(hexVector, hex);
            float time = delay * currentRadius + Time.realtimeSinceStartup;
            if (currentRadius < innerRadius){
                float fallOff = 1 - (float)currentRadius / innerRadius;
                AddSingleBounceForce((GridVector)hex, -force * fallOff, time);
            }else{
                float fallOff;
                if (currentRadius < peakRadius)
                    fallOff = (float)(currentRadius - innerRadius) / (peakRadius - innerRadius);
                else
                    fallOff = (float)(outerRadius - currentRadius) / (outerRadius - peakRadius);
                AddSingleBounceForce((GridVector)hex, force * fallOff, time);
            }
        }
    }
    public void AddBounceForce(GridVector gridVector, float force, int innerRadius, int outerRadius, float delay){
        HexVector hexVector = (HexVector)gridVector;
        List<HexVector> hexVectors = HexVector.HexRadius(hexVector, outerRadius);
        foreach(HexVector hex in hexVectors){
            if (!Board.Instance.IsHexInBounds(hex))
                continue;
            int currentRadius = HexVector.Distance(hexVector, hex);
            currentRadius -= innerRadius;
            if (currentRadius < 0)
                currentRadius = 0;
            float fallOff = 1 - (float)currentRadius / (outerRadius - innerRadius + 1);
            // exponential fall off
            fallOff = Mathf.Pow(fallOff, 2);
            float time = delay * currentRadius + Time.realtimeSinceStartup;
            AddSingleBounceForce((GridVector)hex, force * fallOff, time);
        }
    }

    private void AddBounceVelocity(GridVector gridVector, float velocity){
        int x = gridVector.x;
        int y = gridVector.y;
        BounceMap[x, y].BounceVelocity += velocity;
        if (!ActiveOscillators.Contains(BounceMap[x, y]))
            ActiveOscillators.Add(BounceMap[x, y]);
    }

    public float GetBouncePosition(GridVector gridVector){
        int x = gridVector.x;
        int y = gridVector.y;
        return BounceMap[x, y].Position;
    }
}
