using UnityEngine;

public interface IPoolable
{
    public void SetPool(ObjectPool _pool);
    public int GetSegmentIndex();
    public void SetSegmentIndex(int _index);
    public GameObject GetObj();
    public void Reuse();
    public void Release();
}
