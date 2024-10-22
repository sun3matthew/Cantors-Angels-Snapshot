using System.Collections.Generic;
using UnityEngine;
using System.Runtime.Serialization;
using System;

public class UniversalPool<T> where T : IPoolable
{
    private LinkedList<T> Pool;
    private Func<T> CreateDefault;

    public UniversalPool() {
        Pool = new LinkedList<T>();
    }
    public UniversalPool(Func<T> createDefault) {
        Pool = new LinkedList<T>();
        CreateDefault = createDefault;
    }

    public T Get(){
        if(Pool.Count == 0){
            IPoolable poolable;
            if (CreateDefault != null)
                poolable = CreateDefault();
            else
                poolable = (IPoolable)FormatterServices.GetUninitializedObject(typeof(T));
            poolable.Instantiate();
            poolable.Activate();
            return (T)poolable;
        }

        T obj = Pool.First.Value;
        Pool.RemoveFirst();
        obj.Activate();
        return obj;
    }

    public void Return(T obj){
        if(obj == null)
            return;
        Pool.AddFirst(obj);
        obj.Deactivate();
    }

    public void PreWarm(int count){
        for(int i = 0; i < count; i++)
            Return(Get());
    }
}
