using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using Unity.VisualScripting;
using UnityEngine;

public class TrackGenerator : MonoBehaviour
{
    [HideInInspector]
    public List<TrackPiece> pieces;
    public Texture2D texture;

    // Bezier parameters
    public int precision = 30;
    public float trackScale = 100.0f;

    // Mesh parameters
    public float defaultTrackWidth = 0.1f;

    private void Awake()
    {
        CreateInitialPiece();
    }

    private void Update()
    {
        foreach (TrackPiece piece in pieces)
        {
            for (int i = 0; i < piece.NumSegments; i += 1)
            {
                Point[] points = piece.GetPointsInSegment(i);

                foreach (Point point in points)
                {
                    Debug.DrawLine(point.pos, point.pos + point.upDir * 2.0f, Color.purple);
                }

                Vector3[] vertexPoints = BezHelper.GeneratePoints(points[0].pos, points[1].pos, points[2].pos, points[3].pos, precision);

                Debug.DrawLine(points[0].pos, points[1].pos, Color.red);
                Debug.DrawLine(points[2].pos, points[3].pos, Color.red);

                for (int j = 0; j < precision; j++)
                {
                    Debug.DrawLine(vertexPoints[j], vertexPoints[j + 1]);
                }
            }

            foreach (Temp temp in piece.temps)
            {
                Debug.DrawRay(temp.pos, temp.forward, Color.blue);
                Debug.DrawRay(temp.pos, temp.side, Color.red);
                Debug.DrawRay(temp.pos, temp.up, Color.green);
            }
        }
        
    }
    public void CreateInitialPiece()
    {
        pieces.Add(new TrackPiece(transform.position, trackScale));
        pieces[0].curveWidth = defaultTrackWidth;
        pieces[0].precision = precision;
        pieces[0].AddSegment(new Vector3(0.1f, 0.5f, 0.5f), Vector3.back);
        pieces[0].AddSegment(new Vector3(0.1f, 0.5f, -0.5f), Vector3.down);
        pieces[0].AddSegment(new Vector3(0.1f, -0.5f, -0.5f), Vector3.forward);
        pieces[0].AddSegment(new Vector3(0.1f, -0.5f, 0.5f), Vector3.up);
        pieces[0].AddSegment(new Vector3(0.1f, 0, 1), Vector3.up);
        pieces[0].AddSegment(new Vector3(1, 0, 1), Vector3.up);
        pieces[0].AddSegment(new Vector3(1, 0, 0), Vector3.up);
        pieces[0].AddSegment(new Vector3(1, 0, -1), Vector3.up);

        List<GameObject> cubes = pieces[0].GenerateMesh();
        foreach(GameObject cube in cubes)
        {
            cube.transform.parent = transform;
        }
    }
}

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

[System.Serializable]
public class TrackPiece
{
    public List<Point> points = new List<Point>();
    public float scale = 100.0f;
    public float curveWidth = 1.0f;
    public int precision = 30;

    public List<Temp> temps = new();


    public TrackPiece(Vector3 centre, float _scale)
    {
        scale = _scale;
        points = new List<Point>
        {
            new Point(centre),
            new Point(centre + 1.0f * scale * Vector3.forward),
            new Point(centre + 2.0f * scale * Vector3.forward),
            new Point(centre + 3.0f * scale * Vector3.forward),
        };
    }


    #region Management
    public void AddSegment(Vector3 deltaPos, Vector3? upDir = null)
    {
        Vector3 anchorPos = points[points.Count - 1].pos/scale + deltaPos;

        points.Add(new Point(points[points.Count - 1].pos * 2.0f - points[points.Count - 2].pos, upDir));
        points.Add(new Point((points[points.Count - 1].pos + anchorPos * scale) * 0.5f, upDir));
        points.Add(new Point(anchorPos * scale, upDir));

        AutoSetAffectedControlPoints(points.Count - 1);
    }

    public void DeleteSegment(int anchorIndex)
    {
        if (NumSegments <= 1) return;
        if (anchorIndex == 0)
        {
            points.RemoveRange(0, 3);
        }
        else if (anchorIndex == points.Count - 1)
        {
            points.RemoveRange(anchorIndex - 2, 3);
        }
        else
        {
            points.RemoveRange(anchorIndex - 1, 3);
        }
    }

    #endregion

    #region Autoset Control Points
    void AutoSetAffectedControlPoints(int updatedAnchorIndex)
    {
        for (int i = updatedAnchorIndex - 3; i <= updatedAnchorIndex + 3; i += 3)
        {
            if (i >= 0 && i < points.Count) AutoSetAnchorControlPoints(LoopIndex(i));
        }

        AutoSetStartAndEndControls();

    }

    void AutoSetAllControlPoints()
    {
        for (int i = 0; i < points.Count; i++)
        {
            AutoSetAnchorControlPoints(i);
        }
        AutoSetStartAndEndControls();
    }

    void AutoSetAnchorControlPoints(int anchorIndex)
    {
        Vector3 anchorPos = points[anchorIndex].pos;
        Vector3 dir = Vector2.zero;
        float[] neighbourDistances = new float[2];

        if (anchorIndex - 3 >= 0)
        {
            Vector3 offset = points[LoopIndex(anchorIndex - 3)].pos - anchorPos;
            dir += offset.normalized;
            neighbourDistances[0] = offset.magnitude;
        }

        if (anchorIndex + 3 >= 0)
        {
            Vector3 offset = points[LoopIndex(anchorIndex + 3)].pos - anchorPos;
            dir -= offset.normalized;
            neighbourDistances[1] = -offset.magnitude;
        }

        dir.Normalize();

        for (int i = 0; i < 2; i++)
        {
            int controlIndex = anchorIndex + i * 2 - 1;
            if (controlIndex >= 0 & controlIndex < points.Count)
            {
                points[LoopIndex(controlIndex)].pos = anchorPos + dir * neighbourDistances[i] * 0.5f;
            }
        }
    }

    void AutoSetStartAndEndControls()
    {
        points[1].pos = (points[0].pos + points[2].pos) * 0.5f;
        points[points.Count - 2].pos = (points[points.Count - 1].pos + points[points.Count - 3].pos) * 0.5f;
    }
    #endregion

    #region Helpers
    public Vector3 this[int i]
    {
        get { return points[i].pos; }
    }

    public int NumPoints
    {
        get { return points.Count; }
    }

    public int NumSegments
    {
        get { return (points.Count-1)/3; }
    }

    public Point[] GetPointsInSegment(int i)
    {
        return new Point[] { points[i * 3], points[i * 3 + 1], points[i * 3 + 2], points[i * 3 + 3] };
    }

    int LoopIndex(int i)
    {
        return (i + points.Count) % points.Count;
    }
    #endregion

    #region MeshGeneration

    public List<GameObject> GenerateMesh()
    {
        List<GameObject> cubes = new();

        for (int segmentIndex = 0; segmentIndex < NumSegments; segmentIndex++)
        {
            Point[] segmentPoints = GetPointsInSegment(segmentIndex);

            // Get the points that make up the part of the curve
            List<Vector3> curvePoints = BezHelper.GeneratePoints(segmentPoints[0].pos, segmentPoints[1].pos, segmentPoints[2].pos, segmentPoints[3].pos, precision).ToList();

            // Track previous forward vector
            Vector3 previousForwardDir = Vector3.zero;

            // For every point
            for (int pointIndex = 0; pointIndex < curvePoints.Count; pointIndex++)
            {
                // Don't loop over the first point of every non-first segment. Avoids repeats
                if (segmentIndex != 0 && pointIndex == 0) continue;

                // Default the up direction according to the control ups
                Vector3 upDir = Vector3.Lerp(segmentPoints[0].upDir, segmentPoints[3].upDir, (float)(pointIndex) / (float)(curvePoints.Count));

                // Set the forward direction
                Vector3 forwardDir;
                if (pointIndex < curvePoints.Count - 1) forwardDir = (curvePoints[pointIndex + 1] - curvePoints[pointIndex]).normalized;
                else forwardDir = previousForwardDir;

                // Set the tracker
                previousForwardDir = forwardDir;

                // Calculate the new up direction as the rotational difference between the previous and current forward direction
                upDir += forwardDir - previousForwardDir;
                upDir.Normalize();

                // Calculate the side direction
                Vector3 sideDir = Vector3.Cross(upDir, forwardDir);
                sideDir.Normalize();

                // Recalcualte upDir using forward and side
                upDir = Vector3.Cross(forwardDir, sideDir);

                // Debug list
                temps.Add(new Temp(curvePoints[pointIndex], upDir, forwardDir, sideDir));

                if (pointIndex == 0 && segmentIndex == 0)
                {
                    // Close caps
                }

            }
        }

        return cubes;
    }

    #endregion
}

public class Point
{
    public Vector3 pos;
    public Vector3 upDir;

    public Point(Vector3 _pos, Vector3? _upDir = null)
    {
        pos = _pos;
        if (_upDir == null) upDir = Vector3.up;
        else upDir = (Vector3)(_upDir);
    }
}

public class Temp
{
    public Vector3 pos;
    public Vector3 up;
    public Vector3 forward;
    public Vector3 side;

    public Temp(Vector3 pos, Vector3 up, Vector3 forward, Vector3 side)
    {
        this.pos = pos;
        this.up = up;
        this.forward = forward;
        this.side = side;
    }
}