using UnityEngine;
using System.Collections.Generic;

public class ObjectPool
{
    public IPoolable objectToPool;
    public Stack<IPoolable> objectPool;


    public ObjectPool(IPoolable _objectToPool, int _initSize)
    {
        objectToPool = _objectToPool;
        objectPool = new();

        for (int i = 0; i < _initSize; i++)
            CreateNewObject();
    }

    private GameObject CreateNewObject(bool startActive = false)
    {
        GameObject newObject = Object.Instantiate(objectToPool.GetObj());
        newObject.GetComponent<IPoolable>().SetPool(this);
        newObject.SetActive(startActive);
        objectPool.Push(newObject.GetComponent<IPoolable>());
        return newObject;
    }

    public GameObject GetObjectFromPool()
    {
        if (objectPool.Count == 0)
        {
            return CreateNewObject(true);
        }
        else
        {
            GameObject objectFromPool = objectPool.Pop().GetObj();
            objectFromPool.SetActive(true);
            objectFromPool.GetComponent<IPoolable>().Reuse();
            return objectFromPool;
        }
    }

    public void ReturnToPool(IPoolable objectToReturn)
    {
        objectToReturn.GetObj().SetActive(false);
        objectPool.Push(objectToReturn);
    }
}
