using System.Collections.Generic;
using Unity.VisualScripting;
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

    // How many segments of track should exist behind and ahead
    private int renderDistance = 8;

    public SegmentBounds segmentBoundPrefab;

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
        GenerateInitialPieces();
    }

    private void GenerateNextSegment()
    {
        pieces[0].AddSegment(new Vector3(Random.Range(-3.0f, 3.0f), Random.Range(-1.0f, 1.0f), Random.Range(3.0f, 3.5f)), Vector3.up);
    }

    private void RemoveFirstSegment()
    {
        pieces[0].DeleteSegment(0);
    }

    private void GenerateInitialPieces()
    {
        pieces[0].AddSegment(new Vector3(0.0f, 0.0f, 3.0f), Vector3.up);

        for (int i = 0; i < renderDistance; i++)
            GenerateNextSegment();
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

            roadMeshChildren[i].name = "Seg" + pieces[0].totalSegmentTracker + "/";

            if (isBarrier)
                roadMeshChildren[i].name += "Barrier/" + sides[(i - roadMeshChildrenCount) % 4];
            else
                roadMeshChildren[i].name += "Road/" + (i == 0 ? "Top" : "Bottom");
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
        // Add the mountain mesh to the game world
        mountainMeshChildren.Add(new GameObject());
        mountainMeshChildren[^1].transform.parent = transform;
        mountainMeshChildren[^1].AddComponent<MeshRenderer>().material = mountainMaterial;
        mountainMeshChildren[^1].AddComponent<MeshFilter>().sharedMesh = mesh;
        mountainMeshChildren[^1].AddComponent<MeshCollider>().sharedMesh = mesh;
        mountainMeshChildren[^1].name = "Seg" + pieces[0].totalSegmentTracker + "/Mountain";

        // Create the bounding box collider
        SegmentBounds bounds = new GameObject().AddComponent<SegmentBounds>();
        bounds.Setup(mesh.bounds.center, mesh.bounds.size, pieces[0].totalSegmentTracker);

        bounds.transform.parent = mountainMeshChildren[^1].transform;

        // Connect events
        bounds.playerCollideEnter += PlayerEnterSegment;
        bounds.playerCollideExit += PlayerExitSegment;
    }

    public void MountainSegmentDeleted(int segmentIndex)
    {
        SegmentBounds bounds = mountainMeshChildren[segmentIndex].transform.GetComponentInChildren<SegmentBounds>();

        // Clear events to clean up memory
        bounds.playerCollideEnter -= PlayerEnterSegment;
        bounds.playerCollideExit -= PlayerExitSegment;

        // Destroy the mountain mesh
        Destroy(mountainMeshChildren[segmentIndex]);
        mountainMeshChildrenCount -= 1;
        mountainMeshChildren.RemoveAt(segmentIndex);
    }

    public void PlayerEnterSegment(int segmentIndex)
    {
        if (segmentIndex + renderDistance > pieces[0].totalSegmentTracker)
        {
            GenerateNextSegment();
            Debug.Log("Generated segment " + (segmentIndex + renderDistance));
        }
    }

    public void PlayerExitSegment(int segmentIndex)
    {
        if (segmentIndex - renderDistance > pieces[0].totalSegmentTracker - pieces[0].NumSegments)
        {
            RemoveFirstSegment();
            Debug.Log("Removed segment " + (segmentIndex - renderDistance));
        }
    }
}
