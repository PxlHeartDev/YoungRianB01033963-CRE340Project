using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class TrackGenerator : MonoBehaviour
{
    [HideInInspector]
    public List<TrackPiece> pieces;
    public Texture2D texture;

    // Bezier parameters
    public int precision = 15;
    public float trackScale = 100.0f;

    // Mesh parameters
    public float defaultTrackWidth = 80.0f;
    public float defaultTrackHeight = 0.1f;
    public float defaultBarrierWidth = 2.0f;
    public float defaultBarrierHeight = 8.0f;
    public bool doBarriers = true;

    [SerializeField] private List<GameObject> meshChildren = new();

    private int meshChildrenCount = 0;

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
        }
        
    }
    public void CreateInitialPiece()
    {
        pieces.Add(new TrackPiece(trackScale));
        pieces[0].trackWidth = defaultTrackWidth;
        pieces[0].trackHeight = defaultTrackHeight;
        pieces[0].precision = precision;
        pieces[0].barrierWidth = defaultBarrierWidth;
        pieces[0].barrierHeight = defaultBarrierHeight;
        pieces[0].doBarriers = doBarriers;

        pieces[0].roadMeshPieceGenerated += RoadMeshPieceGenerated;
        pieces[0].roadSegmentDeleted += RoadSegmentDeleted;

        pieces[0].InitSegment(transform.position);

        pieces[0].AddSegment(new Vector3(0.0f, 0.0f, 1.0f), Vector3.up);

        StartCoroutine(SlowGenPieces());
    }

    IEnumerator<WaitForSeconds> SlowGenPieces()
    {
        yield return new WaitForSeconds(2.0f);
        for (int i = 0; i < 5; i++)
        {
            yield return new WaitForSeconds(1.0f);
            pieces[0].AddSegment(new Vector3(UnityEngine.Random.Range(-3.0f, 3.0f), UnityEngine.Random.Range(-0.5f, 0.5f), UnityEngine.Random.Range(2.0f, 2.5f)), Vector3.up);
        }
        for (int i = 0; i < 5; i++)
        {
            yield return new WaitForSeconds(1.0f);
            pieces[0].DeleteSegment(0);
        }
        for (int i = 0; i < 3; i++)
        {
            yield return new WaitForSeconds(1.0f);
            pieces[0].AddSegment(new Vector3(UnityEngine.Random.Range(-3.0f, 3.0f), UnityEngine.Random.Range(-0.5f, 0.5f), UnityEngine.Random.Range(2.0f, 2.5f)), Vector3.up);
        }
        for (int i = 0; i < 3; i++)
        {
            yield return new WaitForSeconds(1.0f);
            pieces[0].DeleteSegment(0);
        }
    }

    public void RoadMeshPieceGenerated(List<Mesh> meshes)
    {
        String[] pieceNames = { "Road", "BarrierR", "BarrierL"};
        String[] sides = { "Left", "Top", "Right", "Bottom"};

        for (int i = meshChildrenCount; i < meshes.Count + meshChildrenCount; i++)
        {
            meshChildren.Add(new GameObject());
            meshChildren[i].transform.parent = transform;
            meshChildren[i].AddComponent<MeshRenderer>().material = roadMaterial;
            meshChildren[i].AddComponent<MeshFilter>();
            meshChildren[i].AddComponent<MeshCollider>();
            meshChildren[i].GetComponent<MeshFilter>().sharedMesh = meshes[i - meshChildrenCount];
            meshChildren[i].AddComponent<MeshCollider>().sharedMesh = meshes[i - meshChildrenCount];
            meshChildren[i].name = "Seg" + pieces[0].totalSegmentTracker + "/" + sides[(i - meshChildrenCount) %4] + pieceNames[(i - meshChildrenCount) /4];
        }
        meshChildrenCount += meshes.Count;
    }

    public void RoadSegmentDeleted(int segmentIndex)
    {
        for (int i = segmentIndex * 12; i < ((segmentIndex + 1) * 12); i++)
        {
            Destroy(meshChildren[i]);
            meshChildrenCount -= 1;
        }
        meshChildren.RemoveRange(segmentIndex * 12, 12);
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
    public List<Point> points = new();
    public float scale = 50.0f;
    public float trackWidth = 1.0f;
    public float trackHeight = 1.0f;
    public float barrierWidth = 2.0f;
    public float barrierHeight = 10.0f;
    public int precision = 10;

    public bool doBarriers = true;

    private List<Vector3> lastRoadQuadOfLastSegment;
    private List<Vector3> lastRightBarrierQuadOfLastSegment;
    private List<Vector3> lastLeftBarrierQuadOfLastSegment;

    public System.Action<List<Mesh>> roadMeshPieceGenerated;
    public System.Action<int> roadSegmentDeleted;

    public int totalSegmentTracker = 0;

    public TrackPiece(float _scale)
    {
        scale = _scale;
    }

    public void InitSegment(Vector3 centre)
    {
        points = new List<Point>
        {
            new(centre),
            new(centre + 0.5f * scale * Vector3.forward),
            new(centre + 1.5f * scale * Vector3.forward),
            new(centre + 2.0f * scale * Vector3.forward),
        };
    }


    #region Management
    public void AddSegment(Vector3 deltaPos, Vector3? upDir = null)
    {
        Vector3 anchorPos = points[^1].pos + deltaPos * scale;

        points.Add(new Point(points[^1].pos * 2.0f - points[^2].pos, upDir));
        points.Add(new Point((points[^1].pos + anchorPos) * 0.5f, upDir));
        points.Add(new Point(anchorPos, upDir));

        AutoSetAffectedControlPoints(points.Count - 1);

        // Generate the mesh of the *previous* segment
        // Allows controls to update the mesh edges to line up better
        GenerateRoadMesh(NumSegments - 2);

        totalSegmentTracker++;
    }

    public void DeleteSegment(int segmentIndex)
    {
        if (NumSegments <= 1) return;
        if (segmentIndex == 0)
        {
            points.RemoveRange(0, 3);
        }
        else if (segmentIndex == points.Count - 1)
        {
            points.RemoveRange(segmentIndex - 2, 3);
        }
        else
        {
            points.RemoveRange(segmentIndex - 1, 3);
        }

        // Doesn't technically need to be called for the purposes of this game
        //AutoSetAffectedControlPoints(segmentIndex);

        roadSegmentDeleted?.Invoke(segmentIndex);
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
                points[LoopIndex(controlIndex)].pos = anchorPos + 0.5f * neighbourDistances[i] * dir;
            }
        }
    }

    void AutoSetStartAndEndControls()
    {
        points[1].pos = (points[0].pos + points[2].pos) * 0.5f;
        points[^2].pos = (points[^1].pos + points[^3].pos) * 0.5f;
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
        get { return points[^1]; }
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
    public void GenerateRoadMesh(int segmentIndex)
    {
        RoadMeshBuilder roadBuilder = new(trackWidth, trackHeight);
        RoadMeshBuilder rightBarrier = new(barrierWidth, barrierHeight);
        RoadMeshBuilder leftBarrier = new(barrierWidth, barrierHeight);

        Point[] segmentPoints = GetPointsInSegment(segmentIndex);

        // Get the points that make up the part of the curve
        List<Vector3> curvePoints = BezHelper.GeneratePoints(segmentPoints[0].pos, segmentPoints[1].pos, segmentPoints[2].pos, segmentPoints[3].pos, GetAutoPrecisionOfSegment(segmentIndex)).ToList();

        // Track previous forward vector
        Vector3 previousForwardDir = Vector3.zero;

        // For every point
        for (int pointIndex = 0; pointIndex < curvePoints.Count; pointIndex++)
        {
            Vector3 point = curvePoints[pointIndex];

            // Default the up direction according to the control ups
            Vector3 upDir = Vector3.Lerp(segmentPoints[0].upDir, segmentPoints[3].upDir, (float)(pointIndex) / (float)(curvePoints.Count));

            // Set the forward direction
            Vector3 forwardDir;
            if (pointIndex < curvePoints.Count - 1)
                forwardDir = (curvePoints[pointIndex + 1] - point).normalized;
            else forwardDir = previousForwardDir;

            // Set the tracker
            previousForwardDir = forwardDir;

            // Calculate the side direction
            Vector3 sideDir = Vector3.Cross(upDir, forwardDir);
            sideDir.Normalize();

            upDir = Vector3.Cross(forwardDir, sideDir).normalized;

            if (segmentIndex > 0 && pointIndex == 0)
                roadBuilder.BuildVerts(lastRoadQuadOfLastSegment, upDir, sideDir);
            else
                roadBuilder.BuildVerts(point, upDir, sideDir, Vector2.zero, true);

            if (doBarriers)
            {
                if (segmentIndex > 0 && pointIndex == 0)
                {
                    rightBarrier.BuildVerts(lastRightBarrierQuadOfLastSegment, upDir, sideDir);
                    leftBarrier.BuildVerts(lastLeftBarrierQuadOfLastSegment, upDir, sideDir);
                }
                else
                {
                    rightBarrier.BuildVerts(point, upDir, sideDir, new Vector2(trackWidth / 2 - barrierWidth / 2, trackHeight), false);
                    leftBarrier.BuildVerts(point, upDir, sideDir, new Vector2(-trackWidth / 2 + barrierWidth / 2, trackHeight), false);
                }
            }

            // Stitch mesh vertices into triangles
            if (pointIndex > 0)
                roadBuilder.BuildTris(pointIndex);

            if (pointIndex == curvePoints.Count - 1)
            {
                lastRoadQuadOfLastSegment = roadBuilder.lastQuad;
                lastRightBarrierQuadOfLastSegment = rightBarrier.lastQuad;
                lastLeftBarrierQuadOfLastSegment= leftBarrier.lastQuad;
            }
           
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

        roadMeshPieceGenerated?.Invoke(allMeshes);
    }

    #endregion
}

public class RoadMeshBuilder
{
    public float width;
    public float height;

    private List<Mesh> meshes = new();
    public List<List<Vector3>> verts = new();
    public List<List<Vector3>> faceNormals = new();
    public List<List<int>> tris = new();

    public List<Vector3> lastQuad;

    public RoadMeshBuilder(float _width, float _height)
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

    // Build new vertices
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
        
        lastQuad = quad;

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

    // Build from last quad of last segment
    public void BuildVerts(List<Vector3> previousQuad, Vector3 upDir, Vector3 sideDir)
    {
        // Left verts
        verts[0].Add(previousQuad[0]);
        verts[0].Add(previousQuad[2]);
        // Top verts
        verts[1].Add(previousQuad[2]);
        verts[1].Add(previousQuad[3]);
        // Right verts
        verts[2].Add(previousQuad[3]);
        verts[2].Add(previousQuad[1]);
        // Bottom verts
        verts[3].Add(previousQuad[1]);
        verts[3].Add(previousQuad[0]);

        BuildNormals(upDir, sideDir);
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