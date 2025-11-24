using UnityEngine;
using System.Collections.Generic;

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
