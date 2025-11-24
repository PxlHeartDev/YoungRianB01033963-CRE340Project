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
    public int precision = 12;
    public float trackScale = 100.0f;

    // Mesh parameters
    public float defaultTrackWidth = 80.0f;
    public float defaultTrackHeight = 0.1f;
    public float defaultBarrierWidth = 2.0f;
    public float defaultBarrierHeight = 8.0f;
    public bool doBarriers = true;

    private List<GameObject> roadMeshChildren = new();
    private int roadMeshChildrenCount = 0;

    private List<GameObject> mountainMeshChildren = new();
    private int mountainMeshChildrenCount = 0;

    public Material roadMaterial;
    public Material barrierTopMaterial;
    public Material barrierSideMaterial;
    public Material mountainMaterial;

    private void Awake()
    {
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

        pieces[0].mountainMeshGenerated += MountainMeshPieceGenerated;
        pieces[0].mountainSegmentDeleted += MountainSegmentDeleted;

        pieces[0].InitSegment(transform.position);
        StartCoroutine(SlowGenPieces());
    }

    IEnumerator<WaitForSeconds> SlowGenPieces()
    {
        yield return new WaitForSeconds(1.0f);

        pieces[0].AddSegment(new Vector3(0.0f, 0.0f, 3.0f), Vector3.up);

        for (int i = 0; i < 20; i++)
        {
            yield return new WaitForSeconds(0.5f);
            pieces[0].AddSegment(new Vector3(UnityEngine.Random.Range(-3.0f, 3.0f), UnityEngine.Random.Range(-1.0f, 1.0f), UnityEngine.Random.Range(2.0f, 2.5f)), Vector3.up);
        }
    }

    public void RoadMeshPieceGenerated(List<Mesh> meshes, bool isBarrier = false)
    {
        string[] pieceNames = { "Road", "BarrierR", "BarrierL"};
        string[] sides = { "Left", "Top", "Right", "Bottom"};

        for (int i = roadMeshChildrenCount; i < meshes.Count + roadMeshChildrenCount; i++)
        {
            bool isSide = meshes[i - roadMeshChildrenCount].name.Contains("Left") || meshes[i - roadMeshChildrenCount].name.Contains("Right");

            roadMeshChildren.Add(new GameObject());
            roadMeshChildren[i].transform.parent = transform;
            roadMeshChildren[i].AddComponent<MeshRenderer>().material = isBarrier ? (isSide ? barrierSideMaterial : barrierTopMaterial) : roadMaterial;
            roadMeshChildren[i].AddComponent<MeshFilter>();
            roadMeshChildren[i].AddComponent<MeshCollider>();
            roadMeshChildren[i].GetComponent<MeshFilter>().sharedMesh = meshes[i - roadMeshChildrenCount];
            roadMeshChildren[i].AddComponent<MeshCollider>().sharedMesh = meshes[i - roadMeshChildrenCount];
            roadMeshChildren[i].name = "Seg" + pieces[0].totalSegmentTracker + "/" + sides[(i - roadMeshChildrenCount) % 4] + pieceNames[(i - roadMeshChildrenCount) /4];
        }
        roadMeshChildrenCount += meshes.Count;
    }

    public void RoadSegmentDeleted(int segmentIndex)
    {
        for (int i = segmentIndex * 12; i < ((segmentIndex + 1) * 12); i++)
        {
            Destroy(roadMeshChildren[i]);
            roadMeshChildrenCount -= 1;
        }
        roadMeshChildren.RemoveRange(segmentIndex * 12, 12);
    }

    public void MountainMeshPieceGenerated(Mesh mesh)
    {
        mountainMeshChildren.Add(new GameObject());
        mountainMeshChildren[^1].transform.parent = transform;
        mountainMeshChildren[^1].AddComponent<MeshRenderer>().material = mountainMaterial;
        mountainMeshChildren[^1].AddComponent<MeshFilter>().sharedMesh = mesh;
        mountainMeshChildren[^1].AddComponent<MeshCollider>().sharedMesh = mesh;
        mountainMeshChildren[^1].name = "Seg" + pieces[0].totalSegmentTracker;
    }

    public void MountainSegmentDeleted(int segmentIndex)
    {
        Destroy(mountainMeshChildren[segmentIndex]);
        mountainMeshChildrenCount -= 1;
        mountainMeshChildren.RemoveAt(segmentIndex);
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
    public int precision = 12;

    public bool doBarriers = true;

    private List<Vector3> lastRoadQuadOfLastSegment;
    private List<Vector3> lastRightBarrierQuadOfLastSegment;
    private List<Vector3> lastLeftBarrierQuadOfLastSegment;
    private List<Vector3> lastMountainLineOfLastSegment;

    public System.Action<List<Mesh>, bool> roadMeshPieceGenerated;
    public System.Action<int> roadSegmentDeleted;

    public System.Action<Mesh> mountainMeshGenerated;
    public System.Action<int> mountainSegmentDeleted;

    public int totalSegmentTracker = 0;

    private List<Vector3> previousCurvePoints;

    private float[] xPosHist;
    private int xPosHistIndex = 0;

    private float[] yPosHist;
    private int yPosHistIndex = 0;

    private int posHistLength = 25;
    private int posHistPollingInterval = 2;

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

        xPosHist = new float[posHistLength];
        yPosHist = new float[posHistLength];

        for (int i = 0; i < posHistLength; i++)
        {
            xPosHist[i] = 0.0f;
        }
    }


    #region Management
    public void AddSegment(Vector3 deltaPos, Vector3? upDir = null)
    {
        Vector3 anchorPos = points[^1].pos + deltaPos * scale;

        points.Add(new Point(points[^1].pos * 2.0f - points[^2].pos, upDir));
        points.Add(new Point((points[^1].pos + anchorPos) * 0.5f, upDir));
        points.Add(new Point(anchorPos, upDir));

        AutoSetAffectedControlPoints(points.Count - 1);

        // Generate the road mesh of the *previous* segment
        // Allows controls to update the mesh edges to line up better
        GenerateRoadMesh(NumSegments - 2);
        // Generate the mountain mesh of the *previous previous* segment
        // Allows for better smoothing of the mountain side dirs
        if (NumSegments >= 2)
            GenerateMountainMesh(NumSegments - 3);

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
        mountainSegmentDeleted?.Invoke(segmentIndex);
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
        RoadMeshBuilder roadBuilder = new(trackWidth, trackHeight, false, true, false, true);
        RoadMeshBuilder rightBarrier = new(barrierWidth, barrierHeight, true, true, true, false);
        RoadMeshBuilder leftBarrier = new(barrierWidth, barrierHeight, true, true, true, false);

        Point[] segmentPoints = GetPointsInSegment(segmentIndex);

        int segmentPrecision = GetAutoPrecisionOfSegment(segmentIndex);

        // Get the points that make up the part of the curve
        List<Vector3> curvePoints = BezHelper.GeneratePoints(segmentPoints[0].pos, segmentPoints[1].pos, segmentPoints[2].pos, segmentPoints[3].pos, segmentPrecision).ToList();

        // Track previous forward vector
        Vector3 previousForwardDir = Vector3.zero;

        List<Vector3> holdSideDirs = new();

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
                    rightBarrier.BuildVerts(point, upDir, sideDir, new Vector2(trackWidth / 2 - barrierWidth / 2, 0.0f), true);
                    leftBarrier.BuildVerts(point, upDir, sideDir, new Vector2(-trackWidth / 2 + barrierWidth / 2, 0.0f), true);
                }
            }

            // Stitch mesh vertices into triangles
            if (pointIndex > 0)
            {
                roadBuilder.BuildTris(pointIndex);
                if (doBarriers)
                {
                    rightBarrier.BuildTris(pointIndex);
                    leftBarrier.BuildTris(pointIndex);
                }
            }

            if (pointIndex == curvePoints.Count - 1)
            {
                lastRoadQuadOfLastSegment = roadBuilder.lastQuad;
                if (doBarriers)
                {
                    lastRightBarrierQuadOfLastSegment = rightBarrier.lastQuad;
                    lastLeftBarrierQuadOfLastSegment = leftBarrier.lastQuad;
                }
            }

            holdSideDirs.Add(sideDir);
        }

        roadMeshPieceGenerated?.Invoke(roadBuilder.FinishMesh(), false);

        if (doBarriers)
        {
            List<Mesh> barrierMeshes = new();
            barrierMeshes.AddRange(rightBarrier.FinishMesh());
            barrierMeshes.AddRange(leftBarrier.FinishMesh());
            roadMeshPieceGenerated?.Invoke(barrierMeshes, true);
        }


        previousCurvePoints = curvePoints;
    }

    public void GenerateMountainMesh(int segmentIndex)
    {
        float minZDelta = 8.0f;
        float curZDelta = 0.0f;
        int builtPoints = 0;
        MountainMeshBuilder mountainBuilder = new();

        for (int pointIndex = 0; pointIndex < previousCurvePoints.Count; pointIndex++)
        {
            if (pointIndex % posHistPollingInterval == 0 || pointIndex == previousCurvePoints.Count - 1)
            {
                xPosHist[xPosHistIndex] = previousCurvePoints[pointIndex].x;
                xPosHistIndex++;
                yPosHist[yPosHistIndex] = previousCurvePoints[pointIndex].y;
                yPosHistIndex++;
            }

            if (pointIndex > 0)
                curZDelta += previousCurvePoints[pointIndex].z - previousCurvePoints[pointIndex - 1].z;

            if (xPosHistIndex == posHistLength) xPosHistIndex = 0;
            if (yPosHistIndex == posHistLength) yPosHistIndex = 0;

            float minX = (Mathf.Min(xPosHist) + previousCurvePoints[pointIndex].x * 9.0f) / 10.0f;
            float maxX = (Mathf.Max(xPosHist) + previousCurvePoints[pointIndex].x * 9.0f) / 10.0f;

            float minY = Mathf.Min(yPosHist);

            if (segmentIndex > 0 && pointIndex == 0)
                mountainBuilder.BuildVerts(lastMountainLineOfLastSegment);
            else if (pointIndex == 0)
                mountainBuilder.BuildVerts(previousCurvePoints[pointIndex], Vector3.right, minX, maxX, minY);
            else if (curZDelta > minZDelta)
            {
                mountainBuilder.BuildVerts(previousCurvePoints[pointIndex], Vector3.right, minX, maxX, minY);
                curZDelta = 0.0f;

                builtPoints++;

                mountainBuilder.BuildTris(builtPoints);

            }
            
            if (pointIndex == previousCurvePoints.Count - 1)
            {
                lastMountainLineOfLastSegment = mountainBuilder.lastLine;
            }

        }

        Mesh mountainMesh = mountainBuilder.FinishMesh();

        mountainMeshGenerated?.Invoke(mountainMesh);
    }

    #endregion
}

public class RoadMeshBuilder
{
    public float width;
    public float height;

    private Dictionary<string, Mesh> meshes = new();
    private Dictionary<string, List<Vector3>> verts = new();
    public Dictionary<string, List<Vector3>> faceNormals = new();
    public Dictionary<string, List<int>> tris = new();

    public List<Vector3> lastQuad;

    private bool doLeft = false;
    private bool doTop = false;
    private bool doRight = false;
    private bool doBottom = false;
    private List<string> sideKeys = new();

    public RoadMeshBuilder(float _width, float _height, bool _doLeft, bool _doTop, bool _doRight, bool _doBottom)
    {
        width = _width;
        height = _height;
        
        doLeft = _doLeft;
        doTop = _doTop;
        doRight = _doRight;
        doBottom = _doBottom;

        if (doLeft)
            sideKeys.Add("Left");
        if (doTop)
            sideKeys.Add("Top");
        if (doRight)
            sideKeys.Add("Right");
        if (doBottom)
            sideKeys.Add("Bottom");

        foreach (string side in sideKeys)
        {
            meshes.Add(side, new Mesh() { name = side });
            verts.Add(side, new List<Vector3>());
            faceNormals.Add(side, new List<Vector3>());
            tris.Add(side, new List<int>());
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
        if (doLeft)
        {
            verts["Left"].Add(quad[0]);
            verts["Left"].Add(quad[2]);
        }
        // Top verts
        if (doTop)
        {
            verts["Top"].Add(quad[2]);
            verts["Top"].Add(quad[3]);
        }
        // Right verts
        if (doRight)
        {
            verts["Right"].Add(quad[3]);
            verts["Right"].Add(quad[1]);
        }
        // Bottom verts
        if (doBottom)
        {
            verts["Bottom"].Add(quad[1]);
            verts["Bottom"].Add(quad[0]);
        }

        if (alsoBuildNormals) BuildNormals(upDir, sideDir);
    }

    // Build from last quad of last segment
    public void BuildVerts(List<Vector3> previousQuad, Vector3 upDir, Vector3 sideDir)
    {
        // Left verts
        if (doLeft)
        {
            verts["Left"].Add(previousQuad[0]);
            verts["Left"].Add(previousQuad[2]);
        }
        // Top verts
        if (doTop)
        {
            verts["Top"].Add(previousQuad[2]);
            verts["Top"].Add(previousQuad[3]);
        }
        // Right verts
        if (doRight)
        {
            verts["Right"].Add(previousQuad[3]);
            verts["Right"].Add(previousQuad[1]);
        }
        // Bottom verts
        if (doBottom)
        {
            verts["Bottom"].Add(previousQuad[1]);
            verts["Bottom"].Add(previousQuad[0]);
        }

        BuildNormals(upDir, sideDir);
    }

    public void BuildNormals(Vector3 upDir, Vector3 sideDir)
    {
        // Left normals
        if (doLeft)
        {
            faceNormals["Left"].Add(-sideDir);
            faceNormals["Left"].Add(-sideDir);
        }
        // Top normals
        if (doTop)
        {
            faceNormals["Top"].Add(upDir);
            faceNormals["Top"].Add(upDir);
        }
        // Right normals
        if (doRight)
        {
            faceNormals["Right"].Add(sideDir);
            faceNormals["Right"].Add(sideDir);
        }
        // Bottom normals
        if (doBottom)
        {
            faceNormals["Bottom"].Add(-upDir);
            faceNormals["Bottom"].Add(-upDir);
        }
    }

    public void BuildTris(int pointIndex)
    {
        // Left side
        if (doLeft)
        {
            List<int> curLeftTris = new()
            {
                (pointIndex - 1) * 2,
                pointIndex * 2,
                (pointIndex - 1) * 2 + 1,
                pointIndex * 2,
                pointIndex * 2 + 1,
                (pointIndex - 1) * 2 + 1,
            };
            tris["Left"].AddRange(curLeftTris);
        }

        // Top side
        if (doTop)
        {
            List<int> curTopTris = new()
            {
                (pointIndex - 1) * 2,
                pointIndex * 2,
                (pointIndex - 1) * 2 + 1,
                pointIndex * 2,
                pointIndex * 2 + 1,
                (pointIndex - 1) * 2 + 1,
            };
            tris["Top"].AddRange(curTopTris);
        }

        // Right side
        if (doRight)
        {
            List<int> curRightTris = new()
            {
                pointIndex * 2 + 1,
                (pointIndex - 1) * 2 + 1,
                pointIndex * 2,
                (pointIndex - 1) * 2 + 1,
                (pointIndex - 1) * 2,
                pointIndex * 2,
            };
            tris["Right"].AddRange(curRightTris);
        }

        // Bottom side
        if (doBottom)
        {
            List<int> curBottomTris = new()
            {
                (pointIndex - 1) * 2 + 1,
                pointIndex * 2,
                pointIndex * 2 + 1,
                (pointIndex - 1) * 2 + 1,
                (pointIndex - 1) * 2,
                pointIndex * 2,
            };
            tris["Bottom"].AddRange(curBottomTris);
        }

    }

    public List<Mesh> FinishMesh()
    {
        foreach (string side in sideKeys)
        {
            meshes[side].vertices = verts[side].ToArray();
            meshes[side].triangles = tris[side].ToArray();
            meshes[side].normals = faceNormals[side].ToArray();
            meshes[side].RecalculateBounds();
            meshes[side].name = side;
        }

        List<Mesh> allMeshes = new();

        allMeshes.AddRange(meshes.Values);

        return allMeshes;
    }
}

public class MountainMeshBuilder
{
    public float roadWidth = 220.0f;

    // Number of vertices to expand left/right
    public int verticesFromCentreCount = 18;
    public float extraSideDistance = 700.0f;
    public float mountainXZScale = 0.01f;
    public float mountainHeight = 125.0f;
    public float baseLevel = -20.0f;

    private Mesh mesh = new();
    private List<Vector3> verts = new();
    public List<int> tris = new();

    public List<Vector3> lastLine;

    public MountainMeshBuilder()
    {

    }

    public void BuildVerts(Vector3 point, Vector3 sideDir, float minX, float maxX, float minY)
    {
        List<Vector3> line = new();

        Vector3 leftPoint = point - sideDir * ((point.x - minX) + extraSideDistance) - Vector3.up * 5.0f;
        Vector3 rightPoint = point + sideDir * ((maxX - point.x) + extraSideDistance) - Vector3.up * 5.0f;
        
        for (int i = 0; i <= (verticesFromCentreCount * 2); i++)
        {
            Vector3 vertexPos = Vector3.Lerp(leftPoint, rightPoint, (float)i / ((float)verticesFromCentreCount * 2.0f));

            //float relief = baseLevel;

            Vector3 minPoint = new(minX, point.y, point.z);
            Vector3 maxPoint = new(maxX, point.y, point.z);

            float dist = Vector3.Distance(vertexPos, (minPoint + maxPoint) / 2.0f);

            float noise = (Mathf.PerlinNoise(vertexPos.x * mountainXZScale, vertexPos.z * mountainXZScale));

            float v = (dist - roadWidth) / roadWidth;

            float relief = Mathf.Lerp(baseLevel - (point.y - minY), noise * mountainHeight, Mathf.Clamp(v, 0.0f, 2.0f));

            line.Add(vertexPos + Vector3.up * relief);
        }

        verts.AddRange(line);

        lastLine = line;
    }

    public void BuildVerts(List<Vector3> previousLine)
    {
        verts.AddRange(previousLine);
    }

    public void BuildTris(int pointIndex)
    {
        int curPoint = pointIndex * 2 * verticesFromCentreCount + pointIndex;
        int prevPoint = (pointIndex - 1) * 2 * verticesFromCentreCount + (pointIndex - 1);

        List<int> curTris = new();

        for (int i = 0; i < (2 * verticesFromCentreCount); i++)
        {
            curTris.AddRange(new List<int> {
                i + prevPoint,
                i + curPoint + 1,
                i + prevPoint + 1,
                i + prevPoint,
                i + curPoint,
                i + curPoint + 1,
            });
        }

        tris.AddRange(curTris);
    }

    public Mesh FinishMesh()
    {
        mesh.vertices = verts.ToArray();
        mesh.triangles = tris.ToArray();
        mesh.RecalculateBounds();
        mesh.RecalculateNormals();

        return mesh;
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