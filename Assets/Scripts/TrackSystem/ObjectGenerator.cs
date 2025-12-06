using UnityEngine;
using System.Collections.Generic;

public class ObjectGenerator : MonoBehaviour
{
    Dictionary<int, PoolSegment> allCurvePoints = new();
    private float objectYOffset = 2.0f;

    [SerializeField] private GameObject coinPrefab;
    [SerializeField] private GameObject powerupPrefab;
    [SerializeField] private GameObject spikePrefab;

    private ObjectPool coinPool;
    private ObjectPool powerupPool;
    private ObjectPool spikePool;

    private bool hasBeenSetUp = false;

    void Awake()
    {
        if (!hasBeenSetUp)
            SetupGenerator();
    }

    public void SetupGenerator()
    {
        coinPool = new(coinPrefab.GetComponent<IPoolable>(), 100);
        powerupPool = new(powerupPrefab.GetComponent<IPoolable>(), 10);
        //spikePool = new(spikePrefab.GetComponent<IPoolable>(), 50);

        hasBeenSetUp = true;
    }

    public void SegmentCreated(int segmentIndex, List<Point> curvePoints)
    {
        if (!hasBeenSetUp)
            SetupGenerator();
        allCurvePoints.Add(segmentIndex,
            new PoolSegment(
                segmentIndex,
                curvePoints)
            );
        GenerateObjects(segmentIndex);
    }

    public void SegmentDeleted(int segmentIndex)
    {
        PoolSegment segment = allCurvePoints[segmentIndex];
        foreach (IPoolable pooledObject in segment.objects)
            pooledObject.Release();
        allCurvePoints.Remove(segmentIndex);
    }

    public void GenerateObjects(int segmentIndex)
    {
        PoolSegment segment = allCurvePoints[segmentIndex];
        foreach(Point point in segment.curvePoints)
        {
            GameObject coin = coinPool.GetObjectFromPool();
            coin.transform.parent = transform;
            coin.transform.position = point.pos + point.upDir * objectYOffset;
            segment.objects.Add(coin.GetComponent<IPoolable>());
        }
    }
}
