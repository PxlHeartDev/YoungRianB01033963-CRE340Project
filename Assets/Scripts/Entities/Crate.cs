using UnityEngine;

public class Crate : MonoBehaviour, IPoolable
{
    private ObjectPool pool;
    private int segmentIndex;

    [SerializeField] private Collider crateCollider;
    [SerializeField] private MeshRenderer meshRenderer;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Car car = other.transform.parent.gameObject.GetComponent<Car>();

            car.Damage(1, gameObject);

            Remove();
        }
    }

    private void Remove()
    {
        crateCollider.enabled = false;
        meshRenderer.enabled = false;
        Release();
    }

    #region IPoolable
    public void SetPool(ObjectPool _pool)
    {
        pool = _pool;
    }

    public int GetSegmentIndex()
    {
        return segmentIndex;
    }

    public void SetSegmentIndex(int _index)
    {
        segmentIndex = _index;
    }

    public GameObject GetObj()
    {
        return gameObject;
    }

    public void Reuse()
    {
        crateCollider.enabled = true;
        meshRenderer.enabled = true;
    }

    public void Release()
    {
        pool.ReturnToPool(this);
    }
    #endregion
}
