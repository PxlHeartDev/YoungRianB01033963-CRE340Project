using System.Collections.Generic;
using System.Linq;
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
                Vector3[] points = piece.GetPointsInSegment(i);
                Vector3[] vertexPoints = BezHelper.GeneratePoints(points[0], points[1], points[2], points[3], precision);

                for (int j = 0; j < precision; j++)
                {
                    Debug.DrawLine(vertexPoints[i], vertexPoints[i + 1]);
                }

                Debug.DrawRay(points[0], Vector3.up, Color.green);
                Debug.DrawRay(points[1], Vector3.up, Color.green);
                Debug.DrawRay(points[2], Vector3.up, Color.green);
                Debug.DrawRay(points[3], Vector3.up, Color.green);
            }
        }
    }
    public void CreateInitialPiece()
    {
        pieces.Add(new TrackPiece(transform.position, trackScale));
        pieces[0].curveWidth = defaultTrackWidth;
        pieces[0].precision = precision;
        pieces[0].AddSegment(Vector3.right * 5.0f + Vector3.forward * 2.5f);
        pieces[0].AddSegment(Vector3.right * 10.0f + Vector3.forward * -2.5f);

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
    public List<Vector3> points = new List<Vector3>();
    public float scale = 100.0f;
    public float curveWidth = 1.0f;
    public int precision = 30;


    public TrackPiece(Vector3 centre, float _scale)
    {
        scale = _scale;
        points = new List<Vector3>
        {
            centre + Vector3.left * scale,
            centre + (Vector3.left + Vector3.forward) * 0.5f * scale,
            centre + (Vector3.right + Vector3.back) * 0.5f * scale,
            centre + Vector3.right * scale,
        };
    }


    #region Management
    public void AddSegment(Vector3 anchorPos)
    {
        points.Add(points[points.Count - 1] * 2 - points[points.Count - 2]);
        points.Add((points[points.Count - 1] + anchorPos * scale) * 0.5f);
        points.Add(anchorPos * scale);

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
        Vector3 anchorPos = points[anchorIndex];
        Vector3 dir = Vector2.zero;
        float[] neighbourDistances = new float[2];

        if (anchorIndex - 3 >= 0)
        {
            Vector3 offset = points[LoopIndex(anchorIndex - 3)] - anchorPos;
            dir += offset.normalized;
            neighbourDistances[0] = offset.magnitude;
        }

        if (anchorIndex + 3 >= 0)
        {
            Vector3 offset = points[LoopIndex(anchorIndex + 3)] - anchorPos;
            dir -= offset.normalized;
            neighbourDistances[1] = -offset.magnitude;
        }

        dir.Normalize();

        for (int i = 0; i < 2; i++)
        {
            int controlIndex = anchorIndex + i * 2 - 1;
            if (controlIndex >= 0 & controlIndex < points.Count)
            {
                points[LoopIndex(controlIndex)] = anchorPos + dir * neighbourDistances[i] * 0.5f;
            }
        }
    }

    void AutoSetStartAndEndControls()
    {
        points[1] = (points[0] + points[2]) * 0.5f;
        points[points.Count - 2] = (points[points.Count - 1] + points[points.Count - 3]) * 0.5f;
    }
    #endregion

    #region Helpers
    public Vector3 this[int i]
    {
        get { return points[i]; }
    }

    public int NumPoints
    {
        get { return points.Count; }
    }

    public int NumSegments
    {
        get { return (points.Count-1)/3; }
    }

    public Vector3[] GetPointsInSegment(int i)
    {
        return new Vector3[] { points[i * 3], points[i * 3 + 1], points[i * 3 + 2], points[i * 3 + 3] };
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
        for (int i = 0; i < NumSegments; i++)
        {
            Vector3[] segmentPoints = GetPointsInSegment(i);
            List<Vector3> meshPoints = BezHelper.GeneratePoints(segmentPoints[0], segmentPoints[1], segmentPoints[2], segmentPoints[3], precision).ToList();

            for (int pointIndex = 0; pointIndex < meshPoints.Count; pointIndex++)
            {
                Debug.Log(meshPoints[pointIndex]);
                GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);

                Vector3 point = meshPoints[pointIndex];

                Quaternion rot = cube.transform.rotation;
                Vector3 cubeScale = Vector3.one;

                if (pointIndex != meshPoints.Count - 1)
                {
                    Vector3 nextPoint = meshPoints[pointIndex + 1];

                    cubeScale.x = Vector3.Distance(point, nextPoint) * 0.98f;
                    cubeScale.z = curveWidth * scale;

                    //rot.SetLookRotation(nextPoint);
                }

                cube.transform.SetPositionAndRotation(point, rot);
                cube.transform.localScale = cubeScale;
                cubes.Add(cube);
            }
        }

        return cubes;
    }

    #endregion

}