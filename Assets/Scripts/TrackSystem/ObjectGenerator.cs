using UnityEngine;
using System.Collections.Generic;

public class ObjectGenerator : MonoBehaviour
{
    Dictionary<int, PoolSegment> allCurvePoints = new();
    private float objectYOffset = 2.5f;
    private float objectXOffsetRange = 35.0f;

    private float initialCoinPlaceChance = 0.9f;
    private float coinSwitchPlacingChance = 0.01f;
    private float powerUpPlaceChance = 0.02f;

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
            if (pooledObject.GetSegmentIndex() == segmentIndex)
                pooledObject.Release();
        allCurvePoints.Remove(segmentIndex);
    }

    public void GenerateObjects(int segmentIndex)
    {
        PoolSegment segment = allCurvePoints[segmentIndex];
        bool isPlacingCoins = Random.value < initialCoinPlaceChance;
        float xOffset = Random.Range(-objectXOffsetRange, objectXOffsetRange);
        foreach(Point point in segment.curvePoints)
        {
            if (Random.value < coinSwitchPlacingChance)
            {
                xOffset = Random.Range(-objectXOffsetRange, objectXOffsetRange);
                isPlacingCoins = !isPlacingCoins;
            }
            Vector3 placePos = point.pos + point.upDir * objectYOffset + point.sideDir * xOffset;

            if (isPlacingCoins)
                PlaceCoin(segment, placePos);
            else if (Random.value < powerUpPlaceChance)
                PlacePowerup(segment, placePos);
        }
    }

    private void PlaceCoin(PoolSegment segment, Vector3 pos)
    {
        GameObject coin = coinPool.GetObjectFromPool();
        coin.transform.parent = transform;
        coin.transform.position = pos;
        IPoolable coinPoolable = coin.GetComponent<IPoolable>();
        coinPoolable.SetSegmentIndex(segment.segmentIndex);
        segment.objects.Add(coinPoolable);
    }

    private void PlacePowerup(PoolSegment segment, Vector3 pos)
    {
        GameObject powerup = powerupPool.GetObjectFromPool();
        powerup.transform.parent = transform;
        powerup.transform.position = pos;
        IPoolable powerupPoolable = powerup.GetComponent<IPoolable>();
        powerupPoolable.SetSegmentIndex(segment.segmentIndex);
        segment.objects.Add(powerupPoolable);
    }
}
