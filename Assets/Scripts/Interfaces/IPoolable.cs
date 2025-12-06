using UnityEngine;

public interface IPoolable
{
    public void SetPool(ObjectPool _pool);
    public GameObject GetObj();
    public void Reuse();
    public void Release();
}
