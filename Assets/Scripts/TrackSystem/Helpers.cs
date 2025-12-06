using UnityEngine;
using System.Collections.Generic;

public struct BezHelper
{
    public static Vector3 Lerp(Vector3 a, Vector3 b, float t)
    {
        return (a + (b - a) * t);
    }

    public static Vector3 QuadraticCurve(Vector3 a, Vector3 b, Vector3 c, float t)
    {
        Vector3 p0 = Lerp(a, b, t);
        Vector3 p1 = Lerp(b, c, t);
        return Lerp(p0, p1, t);
    }

    public static Vector3 CubicCurve(Vector3 a, Vector3 b, Vector3 c, Vector3 d, float t)
    {
        Vector3 p0 = QuadraticCurve(a, b, c, t);
        Vector3 p1 = QuadraticCurve(b, c, d, t);
        return Lerp(p0, p1, t);
    }

    public static Vector3[] GeneratePoints(Vector3 a, Vector3 b, Vector3 c, Vector3 d, int numPoints)
    {
        List<Vector3> points = new();

        float iterAmount = 1.0f / numPoints;

        for (int i = 0; i <= numPoints; i++)
        {
            points.Add(CubicCurve(a, b, c, d, iterAmount * i));
        }

        return points.ToArray();
    }
}

public class Point
{
    public Vector3 pos;
    public Vector3 upDir;
    public Vector3 sideDir;

    public Point(Vector3 _pos, Vector3? _upDir = null, Vector3? _sideDir = null)
    {
        pos = _pos;

        if (_upDir == null) upDir = Vector3.up;
        else upDir = (Vector3)(_upDir);

        if (_sideDir == null) sideDir = Vector3.right;
        else sideDir = (Vector3)(_sideDir);

        upDir.Normalize();
        sideDir.Normalize();
    }
}

public class PoolSegment
{
    public int segmentIndex;
    public List<Point> curvePoints;
    public List<IPoolable> objects;

    public PoolSegment(int _segmentIndex, List<Point> _curvePoints)
    {
        segmentIndex = _segmentIndex;
        curvePoints = _curvePoints;
        objects = new();
    }
}