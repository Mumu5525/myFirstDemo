using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class ObjectPool<T> where T : Component
{
    readonly T prefab;
    readonly Transform parent;
    readonly Queue<T> available = new Queue<T>();

    public ObjectPool(T prefab, int prewarm, Transform parent = null)
    {
        this.prefab = prefab;
        this.parent = parent;
        for (int i = 0; i < prewarm; i++)
        {
            T obj = Object.Instantiate(prefab, parent);
            obj.gameObject.SetActive(false);
            available.Enqueue(obj);
        }
    }

    public T Get()
    {
        T obj = available.Count > 0 
                ? available.Dequeue()
                : GameObject.Instantiate(prefab, parent);
        obj.gameObject.SetActive(true);
        return obj;
    }

    public void Return(T obj)
    {
        obj.gameObject.SetActive(false);
        available.Enqueue(obj);
    }
}
