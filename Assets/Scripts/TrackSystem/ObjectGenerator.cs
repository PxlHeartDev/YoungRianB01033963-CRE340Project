using System.Collections.Generic;
using System.Drawing;
using UnityEngine;

public class ObjectGenerator : MonoBehaviour
{
    Dictionary<int, PoolSegment> allCurvePoints = new();
    private float coinYOffset = 2.5f;
    private float crateYOffset = 4.0f;
    private float objectXOffsetRange = 35.0f;

    private float initialCoinPlaceChance = 0.6f;
    private float coinSwitchPlacingChance = 0.18f;
    private float powerUpPlaceChance = 0.02f;
    private float cratePlaceChance = 0.05f;

    [SerializeField] private GameObject coinPrefab;
    [SerializeField] private GameObject powerupPrefab;
    [SerializeField] private GameObject cratePrefab;

    private ObjectPool coinPool;
    private ObjectPool powerupPool;
    private ObjectPool cratePool;

    public void SetupGenerator()
    {
        coinPool = new(coinPrefab.GetComponent<IPoolable>(), 100);
        powerupPool = new(powerupPrefab.GetComponent<IPoolable>(), 10);
        cratePool = new(cratePrefab.GetComponent<IPoolable>(), 20);
    }

    public void SegmentCreated(int segmentIndex, List<Point> curvePoints)
    {
        if (segmentIndex == 0)
        {
            SetupGenerator();
            return;
        }
        allCurvePoints.Add(segmentIndex,
            new PoolSegment(
                segmentIndex,
                curvePoints)
            );
        GenerateObjects(segmentIndex);
    }

    public void SegmentDeleted(int segmentIndex)
    {
        if (segmentIndex == 0)
            return;
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

            if (isPlacingCoins)
                PlaceCoin(segment, point.pos + point.upDir * coinYOffset + point.sideDir * xOffset);
            else if (Random.value < powerUpPlaceChance)
                PlacePowerup(segment, point.pos + point.upDir * coinYOffset + point.sideDir * xOffset);
            else if (Random.value < cratePlaceChance)
                PlaceCrateRow(segment, point, xOffset);
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

    private void PlaceCrateRow(PoolSegment segment, Point point, float xOffset)
    {
        bool fromRight = xOffset > 0.0f;

        Vector3 pos = point.pos + point.upDir * crateYOffset + point.sideDir * xOffset;

        int numCrates = Random.Range(1, 6);

        for (int i = 0; i < numCrates; i++)
        {
            Vector3 placePos = pos + i * point.sideDir * 5.0f * (fromRight ? -1.0f : 1.0f);
            PlaceCrate(segment, placePos, point.upDir);
        }
    }

    private void PlaceCrate(PoolSegment segment, Vector3 pos, Vector3 upDir)
    {
        GameObject crate = cratePool.GetObjectFromPool();
        crate.transform.parent = transform;
        crate.transform.position = pos;
        crate.transform.rotation = Quaternion.LookRotation(upDir);
        IPoolable cratePoolable = crate.GetComponent<IPoolable>();
        cratePoolable.SetSegmentIndex(segment.segmentIndex);
        segment.objects.Add(cratePoolable);
    }
}
