using System.Collections.Generic;
using UnityEngine;
using System;
using System.Threading.Tasks;
public class AsyncTest  
{
    public static void AsyncTestMain(){
        // Async Test

        float startTime = Time.realtimeSinceStartup;
        int iterations = 10000;
        float totalSum = 0;
        Task<float>[] tasks = new Task<float>[iterations];
        for (int i = 0; i < iterations; i++)
            tasks[i] = CalculateAsync(0.01f);

        Task.WaitAll(tasks);
        foreach(Task<float> task in tasks)
            totalSum += task.Result;
        Debug.Log("Time taken: " + (Time.realtimeSinceStartup - startTime) + " Total sum: " + totalSum);

        // startTime = Time.realtimeSinceStartup;
        // totalSum = 0;
        // for (int i = 0; i < iterations; i++)
        //     totalSum += Calculate(0.01f);
        // Debug.Log("Time taken: " + (Time.realtimeSinceStartup - startTime) + " Total sum: " + totalSum);
    }
    public static Task<float> CalculateAsync(float dt){
        return Task.Run(() => Calculate(dt));
    }
    public static float Calculate(float dt){
        float sum = 0;
        for (int i = 0; i < 1000; i++)
            sum += Mathf.Sqrt(i) * Mathf.Pow(dt, i);
        return sum;
    }
}
