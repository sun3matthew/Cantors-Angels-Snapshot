using System;
using UnityEngine;

public class ExposedList<T>
{
    private const int GrowthFactor = 8;
    public T[] Items;
    public int Count;
    public int Capacity => Items.Length;
    private Func<T> CreateDefault;
    public ExposedList(Func<T> createDefault) : this(GrowthFactor, createDefault) { }
    public ExposedList(int capacity, Func<T> createDefault)
    {
        CreateDefault = createDefault;
        Items = new T[capacity];
        for (int i = 0; i < Items.Length; i++)
            Items[i] = CreateDefault();
        Count = 0;
    }
    public void SetCount(int count)
    {
        if (count > Items.Length)
            Grow(Mathf.Max(count - Items.Length, GrowthFactor));
        Count = count;
    }
    public void IncreaseCount()
    {
        if (Count == Items.Length)
            Grow();
        Count++;
    }

    public void RemoveAt(int index)
    {
        T temp = Items[index];
        for (int i = index; i < Count - 1; i++)
            Items[i] = Items[i + 1];
        Items[Count - 1] = temp;
        Count--;
    }

    public void Clear()
    {
        Count = 0;
    }
    private void Grow(int amount){
        T[] newItems = new T[Items.Length + amount];
        for (int i = 0; i < Items.Length; i++)
            newItems[i] = Items[i];
        for (int i = Items.Length; i < newItems.Length; i++)
            newItems[i] = CreateDefault();
        Items = newItems;
    }
    private void Grow() => Grow(GrowthFactor);

    public T this[int index]
    {
        get => Items[index];
        set => Items[index] = value;
    }
}
