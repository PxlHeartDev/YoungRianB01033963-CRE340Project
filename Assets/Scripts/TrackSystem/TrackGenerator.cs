using System.Collections.Generic;
using UnityEngine;

public class TrackGenerator : MonoBehaviour
{
    [HideInInspector]
    public List<TrackPiece> pieces;

    [Header ("Parameters")]
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

    [Header ("Materials")]
    public Material roadMaterial;
    public Material barrierTopMaterial;
    public Material barrierSideMaterial;
    public Material mountainMaterial;

    // How many segments of track should exist behind and ahead
    private int renderDistance = 6;

    [Header ("Object Generator")]
    [SerializeField] private ObjectGenerator objectGenerator;

    [Header("Other")]
    [SerializeField] private int roadLayer;
    [SerializeField] private int mountainLayer;
    private bool debugRenderer = false;

    private void Awake()
    {
        CreateInitialPiece();
    }

    private void Update()
    {
        if (!debugRenderer)
            return;

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
        // Instantiate the new track piece
        pieces.Add(new TrackPiece(trackScale));

        // Assign all the parameters
        pieces[0].trackWidth = defaultTrackWidth;
        pieces[0].trackHeight = defaultTrackHeight;
        pieces[0].precision = precision;
        pieces[0].barrierWidth = defaultBarrierWidth;
        pieces[0].barrierHeight = defaultBarrierHeight;
        pieces[0].doBarriers = doBarriers;
        
        // Connect signals
        pieces[0].roadMeshPieceGenerated += RoadMeshPieceGenerated;
        pieces[0].roadSegmentDeleted += RoadSegmentDeleted;

        pieces[0].mountainMeshGenerated += MountainMeshPieceGenerated;
        pieces[0].mountainSegmentDeleted += MountainSegmentDeleted;
        
        // Initalize
        pieces[0].InitSegment(transform.position);
        GenerateInitialPieces();
    }

    // Generate the next segment of the track
    private void GenerateNextSegment()
    {
        pieces[0].AddSegment(new Vector3(Random.Range(-3.0f, 3.0f), Random.Range(-1.0f, 1.0f), Random.Range(3.0f, 3.5f)), Vector3.up);
    }

    // Remove the farthest back segment
    private void RemoveFirstSegment()
    {
        pieces[0].DeleteSegment(0);
    }

    // Create the first pieces
    private void GenerateInitialPieces()
    {
        pieces[0].AddSegment(new Vector3(0.0f, 0.0f, 3.0f), Vector3.up);

        for (int i = 0; i < renderDistance; i++)
            GenerateNextSegment();
    }

    // Add the generated meshes to the world
    public void RoadMeshPieceGenerated(List<Mesh> meshes, bool isBarrier = false, List<Point> curvePoints = null)
    {
        string[] sides = { "Left", "Top", "Right", "Bottom"};

        if (curvePoints != null)
            objectGenerator.SegmentCreated(pieces[0].totalSegmentTracker, curvePoints);

        // For each mesh
        for (int i = roadMeshChildrenCount; i < meshes.Count + roadMeshChildrenCount; i++)
        {
            // Is this the left/right piece of a mesh?
            bool isSide = meshes[i - roadMeshChildrenCount].name.Contains("Left") || meshes[i - roadMeshChildrenCount].name.Contains("Right");

            // Create the GameObject
            roadMeshChildren.Add(new GameObject());
            roadMeshChildren[i].transform.parent = transform;
            roadMeshChildren[i].isStatic = true;
            roadMeshChildren[i].layer = roadLayer;
            roadMeshChildren[i].AddComponent<MeshRenderer>().material = isBarrier ? (isSide ? barrierSideMaterial : barrierTopMaterial) : roadMaterial;
            roadMeshChildren[i].AddComponent<MeshFilter>().sharedMesh = meshes[i - roadMeshChildrenCount];
            roadMeshChildren[i].AddComponent<MeshCollider>().sharedMesh = meshes[i - roadMeshChildrenCount];

            // Name the mesh
            roadMeshChildren[i].name = "Seg" + pieces[0].totalSegmentTracker + "/";
            if (isBarrier)
                roadMeshChildren[i].name += "Barrier/" + sides[(i - roadMeshChildrenCount) % 4];
            else
                roadMeshChildren[i].name += "Road/" + (i == 0 ? "Top" : "Bottom");
        }
        // Increment the counter
        roadMeshChildrenCount += meshes.Count;
    }

    // Remove the segment from the world
    public void RoadSegmentDeleted(int segmentIndex)
    {
        for (int i = segmentIndex * 8; i < ((segmentIndex + 1) * 8); i++)
        {
            Destroy(roadMeshChildren[i]);
            roadMeshChildrenCount -= 1;
        }
        roadMeshChildren.RemoveRange(segmentIndex * 8, 8);
    }

    // Add the mountain mesh to the world
    public void MountainMeshPieceGenerated(Mesh mesh)
    {
        // Add the mountain mesh to the game world
        mountainMeshChildren.Add(new GameObject());
        mountainMeshChildren[^1].transform.parent = transform;
        mountainMeshChildren[^1].isStatic = true;
        mountainMeshChildren[^1].layer = mountainLayer;

        MeshRenderer renderer = mountainMeshChildren[^1].AddComponent<MeshRenderer>();
        renderer.material = mountainMaterial;
        renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.TwoSided;

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

    // Remove the mesh from the world
    public void MountainSegmentDeleted(int segmentIndex)
    {
        // Get the bounds
        SegmentBounds bounds = mountainMeshChildren[segmentIndex].transform.GetComponentInChildren<SegmentBounds>();

        // Clear events to clean up memory
        bounds.playerCollideEnter -= PlayerEnterSegment;
        bounds.playerCollideExit -= PlayerExitSegment;

        // Destroy the mountain mesh
        Destroy(mountainMeshChildren[segmentIndex]);
        mountainMeshChildren.RemoveAt(segmentIndex);
    }

    // Player has entered the bounds of a segment
    public void PlayerEnterSegment(int segmentIndex)
    {
        if (segmentIndex + renderDistance > pieces[0].totalSegmentTracker)
        {
            GenerateNextSegment();
        }
    }

    // Player has exited the bounds of a segment
    public void PlayerExitSegment(int segmentIndex)
    {
        if (segmentIndex - renderDistance > pieces[0].totalSegmentTracker - pieces[0].NumSegments)
        {
            RemoveFirstSegment();
            objectGenerator.SegmentDeleted(segmentIndex - renderDistance);
        }
    }
}
