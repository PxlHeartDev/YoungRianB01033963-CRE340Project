using UnityEngine;
using System.Collections.Generic;
using System.Linq;

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

    public System.Action<List<Mesh>, bool, List<Point>> roadMeshPieceGenerated;
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
        get { return (points.Count - 1) / 3; }
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

        List<Point> pointsWithDirs = new();

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
            else
                forwardDir = previousForwardDir;

            // Set the tracker
            previousForwardDir = forwardDir;

            // Calculate the side direction
            Vector3 sideDir = Vector3.Cross(upDir, forwardDir);
            sideDir.Normalize();

            upDir = Vector3.Cross(forwardDir, sideDir).normalized;

            pointsWithDirs.Add(new(point, upDir, sideDir));

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

        roadMeshPieceGenerated?.Invoke(roadBuilder.FinishMesh(), false, pointsWithDirs);

        if (doBarriers)
        {
            List<Mesh> barrierMeshes = new();
            barrierMeshes.AddRange(rightBarrier.FinishMesh());
            barrierMeshes.AddRange(leftBarrier.FinishMesh());
            roadMeshPieceGenerated?.Invoke(barrierMeshes, true, null);
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
