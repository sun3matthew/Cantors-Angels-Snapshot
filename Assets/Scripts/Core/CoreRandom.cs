// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.
    
using System;
public class CoreRandom {
    private const int MBIG =  Int32.MaxValue;
    private const int MSEED = 161803398;

    private int inext;
    private int inextp;
    private int[] SeedArray = new int[56];
    private static CoreRandom _global;
    static CoreRandom() => _global = new CoreRandom(DateTime.Now.Millisecond);
    public CoreRandom(int Seed) {
        int ii;
        int mj, mk;

        int subtraction = (Seed == Int32.MinValue) ? Int32.MaxValue : Math.Abs(Seed);
        mj = MSEED - subtraction;
        SeedArray[55]=mj;
        mk=1;
        for (int i=1; i<55; i++) {
            ii = (21*i)%55;
            SeedArray[ii]=mk;
            mk = mj - mk;
            if (mk<0) mk+=MBIG;
            mj=SeedArray[ii];
        }
        for (int k=1; k<5; k++) {
            for (int i=1; i<56; i++) {
                SeedArray[i] -= SeedArray[1+(i+30)%55];
                if (SeedArray[i]<0) SeedArray[i]+=MBIG;
            }
        }
        inext=0;
        inextp = 21;
        Seed = 1;
    }

    public int sample() {
        int retVal;
        int locINext = inext;
        int locINextp = inextp;

        if (++locINext >=56) locINext=1;
        if (++locINextp>= 56) locINextp = 1;
        
        retVal = SeedArray[locINext]-SeedArray[locINextp];

        if (retVal == MBIG) retVal--;          
        if (retVal<0) retVal+=MBIG;
        
        SeedArray[locINext]=retVal;

        inext = locINext;
        inextp = locINextp;
                
        return retVal;
    }
    public int Next(int maxValue) => (int)(NextDouble() * maxValue);
    public int Next(int minValue, int maxValue) => (int)(NextDouble() * (maxValue - minValue) + minValue);
    public float Next(float maxValue) => (float)(NextDouble() * maxValue);
    public double NextDouble() => sample()*(1.0/MBIG);
    public float NextFloat() => (float)NextDouble();
    public long NextLong() => ((long)sample() << 32) + sample();
    public float Range(float min, float max) => NextFloat() * (max - min) + min;
    public int Range(int min, int max) => Next(max - min) + min;

    public static float GlobalRange(float min, float max) => _global.NextFloat() * (max - min) + min;
    public static int GlobalRange(int min, int max) => _global.Next(max - min) + min;
    public static float Value() => _global.NextFloat();
    public static long ValueLong() => _global.NextLong();
}

