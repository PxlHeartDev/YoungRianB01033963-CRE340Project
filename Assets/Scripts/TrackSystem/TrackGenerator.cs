using System.Collections.Generic;
using System.Linq;
using NUnit.Framework.Constraints;
using UnityEngine;

public class TrackGenerator : MonoBehaviour
{
    [HideInInspector]
    public List<TrackPiece> pieces;
    public Texture2D texture;

    // Bezier parameters
    public int precision = 10;
    public float trackScale = 50.0f;

    // Mesh parameters
    public float defaultTrackWidth = 0.5f;
    public float defaultTrackHeight = 0.1f;
    public float defaultBarrierWidth = 2.0f;
    public float defaultBarrierHeight = 10.0f;
    public bool doBarriers = true;

    private List<GameObject> meshChildren = new();

    public Material roadMaterial;

    private void Awake()
    {
        for (int i = 0; i < meshChildren.Count; i++)
        {
            meshChildren[i] = new GameObject();
            meshChildren[i].transform.parent = transform;
            meshChildren[i].AddComponent<MeshRenderer>().material = roadMaterial;
            meshChildren[i].AddComponent<MeshFilter>();
            meshChildren[i].AddComponent<MeshCollider>();
        }

        CreateInitialPiece();
    }

    private void Update()
    {
        foreach (TrackPiece piece in pieces)
        {
            for (int segmentIndex = 0; segmentIndex < piece.NumSegments; segmentIndex += 1)
            {
                Point[] points = piece.GetPointsInSegment(segmentIndex);

                foreach (Point point in points)
                {
                    Debug.DrawLine(point.pos, point.pos + point.upDir * 2.0f, Color.purple);
                }

                int newPrecision = piece.GetAutoPrecisionOfSegment(segmentIndex);

                Vector3[] vertexPoints = BezHelper.GeneratePoints(points[0].pos, points[1].pos, points[2].pos, points[3].pos, newPrecision);

                Debug.DrawLine(points[0].pos, points[1].pos, Color.red);
                Debug.DrawLine(points[2].pos, points[3].pos, Color.red);

                for (int j = 0; j < newPrecision; j++)
                    Debug.DrawLine(vertexPoints[j], vertexPoints[j + 1]);
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
        pieces[0].trackWidth = defaultTrackWidth;
        pieces[0].trackHeight = defaultTrackHeight;
        pieces[0].precision = precision;
        pieces[0].barrierWidth = defaultBarrierWidth;
        pieces[0].barrierHeight = defaultBarrierHeight;
        pieces[0].doBarriers = doBarriers;
        for (int i = 0; i < 10; i++)
        {
            pieces[0].AddSegment(new Vector3(Random.Range(-2.0f, 2.0f), Random.Range(-2.0f, 2.0f), Random.Range(1.0f, 2.0f)), Vector3.up);
        }

        pieces[0].AddSegment(new Vector3(0.1f, 0.3f, 0.6f), new Vector3(0, 1, -1));
        pieces[0].AddSegment(new Vector3(0.1f, 0.6f, 0.3f), Vector3.back);
        pieces[0].AddSegment(new Vector3(0.1f, 0.6f, -0.3f), new Vector3(0, -1, -1));
        pieces[0].AddSegment(new Vector3(0.1f, 0.3f, -0.6f), Vector3.down);
        pieces[0].AddSegment(new Vector3(0.1f, -0.3f, -0.6f), new Vector3(0, -1, 1));
        pieces[0].AddSegment(new Vector3(0.1f, -0.6f, -0.3f), Vector3.forward);
        pieces[0].AddSegment(new Vector3(0.1f, -0.6f, 0.3f), new Vector3(0, 1, 1));
        pieces[0].AddSegment(new Vector3(0.1f, -0.3f, 0.6f), Vector3.up);

        pieces[0].AddSegment(new Vector3(0.1f, 0, 1), Vector3.up);

        for (int i = 0; i < 10; i++)
        {
            pieces[0].AddSegment(new Vector3(Random.Range(-2.0f, 2.0f), Random.Range(-2.0f, 2.0f), Random.Range(1.0f, 2.0f)), Vector3.up);
        }

        List<Mesh> meshes = pieces[0].GenerateMesh();

        for (int i = 0; i < meshes.Count; i++)
        {
            meshChildren.Add(new GameObject());
            meshChildren[i].transform.parent = transform;
            meshChildren[i].AddComponent<MeshRenderer>().material = roadMaterial;
            meshChildren[i].AddComponent<MeshFilter>();
            meshChildren[i].AddComponent<MeshCollider>();
            meshChildren[i].GetComponent<MeshFilter>().sharedMesh = meshes[i];
            meshChildren[i].AddComponent<MeshCollider>().sharedMesh = meshes[i];
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
    public float scale = 50.0f;
    public float trackWidth = 1.0f;
    public float trackHeight = 1.0f;
    public float barrierWidth = 2.0f;
    public float barrierHeight = 10.0f;
    public int precision = 10;

    public bool doBarriers = true;

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
        Vector3 anchorPos = points[points.Count - 1].pos + deltaPos * scale;

        points.Add(new Point(points[points.Count - 1].pos * 2.0f - points[points.Count - 2].pos, upDir));
        points.Add(new Point((points[points.Count - 1].pos + anchorPos) * 0.5f, upDir));
        points.Add(new Point(anchorPos, upDir));

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

    public Point LastPoint
    {
        get { return points[points.Count - 1]; }
    }

    public Point[] GetPointsInSegment(int i)
    {
        return new Point[] { points[i * 3], points[i * 3 + 1], points[i * 3 + 2], points[i * 3 + 3] };
    }

    int LoopIndex(int i)
    {
        return (i + points.Count) % points.Count;
    }

    public int GetAutoPrecisionOfSegment(int i)
    {
        return (int)(Vector3.Distance(points[i * 3].pos, points[i * 3 + 3].pos) * precision / scale);
    }
    #endregion

    #region MeshGeneration

    // Generate the mesh of the track piece
    public List<Mesh> GenerateMesh()
    {
        // Precalculate the list of precisions
        List<int> precisions = new() { };
        for (int segmentIndex = 0; segmentIndex < NumSegments; segmentIndex ++)
            precisions.Add(GetAutoPrecisionOfSegment(segmentIndex));

        int currentPoints = 0;

        MeshBuilder roadBuilder = new MeshBuilder(trackWidth, trackHeight);
        MeshBuilder rightBarrier = new MeshBuilder(barrierWidth, barrierHeight);
        MeshBuilder leftBarrier = new MeshBuilder(barrierWidth, barrierHeight);

        for (int segmentIndex = 0; segmentIndex < NumSegments; segmentIndex++)
        {
            Point[] segmentPoints = GetPointsInSegment(segmentIndex);

            // Get the points that make up the part of the curve
            List<Vector3> curvePoints = BezHelper.GeneratePoints(segmentPoints[0].pos, segmentPoints[1].pos, segmentPoints[2].pos, segmentPoints[3].pos, precisions[segmentIndex]).ToList();

            // Track previous forward vector
            Vector3 previousForwardDir = Vector3.zero;

            // For every point
            for (int pointIndex = 0; pointIndex < curvePoints.Count; pointIndex++)
            {

                Vector3 point = curvePoints[pointIndex];

                Vector3 upDir = Vector3.up;
                Vector3 forwardDir = Vector3.forward;
                Vector3 sideDir = Vector3.right;

                // Don't loop over the first point of every non-first segment. Avoids repeats
                if (segmentIndex == 0 || pointIndex > 0)
                {
                    // Default the up direction according to the control ups
                    upDir = Vector3.Lerp(segmentPoints[0].upDir, segmentPoints[3].upDir, (float)(pointIndex) / (float)(curvePoints.Count));

                    // Set the forward direction
                    if (pointIndex < curvePoints.Count - 1)
                        forwardDir = (curvePoints[pointIndex + 1] - point).normalized;
                    else forwardDir = previousForwardDir;

                    // Set the tracker
                    previousForwardDir = forwardDir;

                    // Calculate the side direction
                    sideDir = Vector3.Cross(upDir, forwardDir);
                    sideDir.Normalize();

                    upDir = Vector3.Cross(forwardDir, sideDir).normalized;

                    // Debug list
                    temps.Add(new Temp(point, upDir, forwardDir, sideDir));

                    roadBuilder.BuildVerts(point, upDir, sideDir, Vector2.zero, true);

                    if (doBarriers)
                    {
                        rightBarrier.BuildVerts(point, upDir, sideDir, new Vector2(trackWidth / 2 - barrierWidth / 2, trackHeight), false);
                        leftBarrier.BuildVerts(point, upDir, sideDir, new Vector2(-trackWidth / 2 + barrierWidth / 2, trackHeight), false);
                    }
                }

                // Reduce the counter to avoid repeated tris
                if (segmentIndex > 0 && pointIndex == 0)
                    currentPoints -= 1;


                // Stitch mesh vertices into triangles
                if (segmentIndex > 0 || (segmentIndex == 0 && pointIndex > 0))
                {
                    int triIndex = pointIndex + currentPoints;

                    roadBuilder.BuildTris(triIndex);
                }
            }

            currentPoints += curvePoints.Count;
        }

        List<Mesh> allMeshes = new();
        allMeshes.AddRange(roadBuilder.FinishMesh());

        if (doBarriers)
        {
            rightBarrier.faceNormals = roadBuilder.faceNormals;
            rightBarrier.tris = roadBuilder.tris;
            leftBarrier.faceNormals = roadBuilder.faceNormals;
            leftBarrier.tris = roadBuilder.tris;

            allMeshes.AddRange(rightBarrier.FinishMesh());
            allMeshes.AddRange(leftBarrier.FinishMesh());
        }

        return allMeshes;
    }
}

public class MeshBuilder
{
    public float width;
    public float height;

    private List<Mesh> meshes = new();
    public List<List<Vector3>> verts = new();
    public List<List<Vector3>> faceNormals = new();
    public List<List<int>> tris = new();

    public MeshBuilder(float _width, float _height)
    {
        width = _width;
        height = _height;

        for (int i = 0; i < 4; i++)
        {
            meshes.Add(new Mesh() { name = i.ToString() });
            verts.Add(new List<Vector3>());
            faceNormals.Add(new List<Vector3>());
            tris.Add(new List<int>());
        }
    }

    public void BuildVerts(Vector3 point, Vector3 upDir, Vector3 sideDir, Vector2 offset, bool alsoBuildNormals)
    {
        Vector3 centreOffset = sideDir * offset.x + upDir * offset.y;

        List<Vector3> quad = new()
        {
            (point - sideDir * width/2.0f) + centreOffset,
            (point + sideDir * width/2.0f) + centreOffset,
            (point - sideDir * width/2.0f + upDir * height) + centreOffset,
            (point + sideDir * width/2.0f + upDir * height) + centreOffset,
        };

        // Left verts
        verts[0].Add(quad[0]);
        verts[0].Add(quad[2]);
        // Top verts
        verts[1].Add(quad[2]);
        verts[1].Add(quad[3]);
        // Right verts
        verts[2].Add(quad[3]);
        verts[2].Add(quad[1]);
        // Bottom verts
        verts[3].Add(quad[1]);
        verts[3].Add(quad[0]);

        if (alsoBuildNormals) BuildNormals(upDir, sideDir);
    }

    public void BuildNormals(Vector3 upDir, Vector3 sideDir)
    {
        // Left normals
        faceNormals[0].Add(-sideDir);
        faceNormals[0].Add(-sideDir);
        // Top normals
        faceNormals[1].Add(upDir);
        faceNormals[1].Add(upDir);
        // Right normals
        faceNormals[2].Add(sideDir);
        faceNormals[2].Add(sideDir);
        // Bottom normals
        faceNormals[3].Add(-upDir);
        faceNormals[3].Add(-upDir);
    }

    public void BuildTris(int triIndex)
    {
        List<int> curLeftTris = new()
        {
            (triIndex - 1) * 2,
            triIndex * 2,
            (triIndex - 1) * 2 + 1,
            triIndex * 2,
            triIndex * 2 + 1,
            (triIndex - 1) * 2 + 1,
        };

        // Top side
        List<int> curTopTris = new()
        {
            (triIndex - 1) * 2,
            triIndex * 2,
            (triIndex - 1) * 2 + 1,
            triIndex * 2,
            triIndex * 2 + 1,
            (triIndex - 1) * 2 + 1,
        };

        // Right side
        List<int> curRightTris = new()
        {
            triIndex * 2 + 1,
            (triIndex - 1) * 2 + 1,
            triIndex * 2,
            (triIndex - 1) * 2 + 1,
            (triIndex - 1) * 2,
            triIndex * 2,
        };

        // Bottom side
        List<int> curBottomTris = new()
        {
            (triIndex - 1) * 2 + 1,
            triIndex * 2,
            triIndex * 2 + 1,
            (triIndex - 1) * 2 + 1,
            (triIndex - 1) * 2,
            triIndex * 2,
        };

        tris[0].AddRange(curLeftTris);
        tris[1].AddRange(curTopTris);
        tris[2].AddRange(curRightTris);
        tris[3].AddRange(curBottomTris);
    }

    public List<Mesh> FinishMesh()
    {
        for (int i = 0; i < 4; i++)
        {
            meshes[i].vertices = verts[i].ToArray();
            meshes[i].triangles = tris[i].ToArray();
            meshes[i].normals = faceNormals[i].ToArray();
            meshes[i].RecalculateBounds();
        }

        return meshes;
    }
}

#endregion
public class Point
{
    public Vector3 pos;
    public Vector3 upDir;

    public Point(Vector3 _pos, Vector3? _upDir = null)
    {
        pos = _pos;
        if (_upDir == null) upDir = Vector3.up;
        else upDir = (Vector3)(_upDir);
        upDir.Normalize();
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