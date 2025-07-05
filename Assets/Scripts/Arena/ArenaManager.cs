using System.Collections.Generic;
using UnityEngine;

public class ArenaManager : MonoBehaviour
{
    [Header("Objects")]
    public GameObject Arena;
    public Material ArenaMaterial;
    public GameObject Player;

    [Header("Arena Parameters")]
    public int[,] HeightMap;
    public int[,] StairsMap;
    public List<List<int[,]>> ArenaPresets;

    [Header("BuffBox Parameters")]
    public GameObject BuffBoxGO;

    [Header("Other Parameters")]
    public float DefaultChangeSpeed;
    float ChangeSpeed;
    public float ChangeSpeedRatio;
    short flag = 0;
    const int ArenaSize = 24;
    const int VerticesSize = ArenaSize + 1;
    const float ChunkScale = 4f;
    const int KillHeight = -36;

    [Header("Mesh Parameters")]
    List<Vector3> CurrentVerticesPositions;
    List<Vector3> NewVerticesPositions;
    List<int> CurrentTriangles;
    List<Vector2> NewUV;


    void Start()
    {
        GlobalInspector.ArenaManager = this;

        CreatePresets();
        CreateArena();

        ChangeSpeed = DefaultChangeSpeed;
    }

    void FixedUpdate()
    {
        switch (flag)
        {
            case 1:
            case 2:
                ChangeArena();
                break;
            case 3:
                if (SmoothTransformToFlat())
                {
                    RotateTriangles();
                    CalculateVerticesPositions();
                    PlaceWalls();

                    Arena.GetComponent<MeshFilter>().mesh.vertices = CurrentVerticesPositions.ToArray();
                    Arena.GetComponent<MeshFilter>().mesh.triangles = CurrentTriangles.ToArray();
                    Arena.GetComponent<MeshFilter>().mesh.uv = NewUV.ToArray();
                    Arena.GetComponent<MeshFilter>().mesh.RecalculateNormals();
                    Arena.GetComponent<MeshFilter>().mesh.RecalculateBounds();

                    ChangeSpeed = DefaultChangeSpeed;
                    flag = 4;
                }
                break;
            case 4:
                if (SmoothTransformToTarget())
                {
                    ChangeSpeed = DefaultChangeSpeed;
                    flag = 0;
                }
                break;

        }
    }


    public void CreateArena()
    {
        Arena = new GameObject("Arena");
        Arena.AddComponent<MeshCollider>();
        Arena.AddComponent<MeshFilter>();
        Arena.AddComponent<MeshRenderer>();

        Mesh arenaMesh = new Mesh();

        Vector3[] vertices = new Vector3[VerticesSize * VerticesSize];
        Vector2[] uv = new Vector2[VerticesSize * VerticesSize];
        int[] triangles = new int[ArenaSize * ArenaSize * 6];

        for (int i = 0; i < VerticesSize; i++)
            for (int j = 0; j < VerticesSize; j++)
            {
                int idx = j + i * (ArenaSize + 1);
                vertices[idx] = new Vector3(i * ChunkScale, 0, j * ChunkScale);

                float randomOffsetX = Random.Range(-0.2f, 0.3f);
                float randomOffsetY = Random.Range(-0.2f, 0.3f);
                randomOffsetX = 0;
                randomOffsetY = 0;
                uv[idx] = new Vector2(
                    (i + randomOffsetX) / ChunkScale,
                    (j + randomOffsetY) / ChunkScale);
            }

        for (int ti = 0, vi = 0, i = 0; i < ArenaSize; i++, vi++)
            for (int j = 0; j < ArenaSize; j++, vi++)
            {
                triangles[ti++] = vi;
                triangles[ti++] = vi + VerticesSize + 1;
                triangles[ti++] = vi + VerticesSize;

                triangles[ti++] = vi;
                triangles[ti++] = vi + 1;
                triangles[ti++] = vi + VerticesSize + 1;
            }

        arenaMesh.vertices = vertices;
        arenaMesh.uv = uv;
        arenaMesh.triangles = triangles;

        arenaMesh.RecalculateBounds();
        arenaMesh.RecalculateNormals();

        Arena.GetComponent<MeshCollider>().sharedMesh = arenaMesh;
        Arena.GetComponent<MeshFilter>().mesh = arenaMesh;
        Arena.GetComponent<MeshRenderer>().material = ArenaMaterial;
        //Arena.GetComponent<MeshRenderer>().material.mainTextureScale = new Vector2(ChunkScale, ChunkScale);

        HeightMap = (int[,])ArenaPresets[0][0].Clone();
        StairsMap = (int[,])ArenaPresets[0][1].Clone();

        NewVerticesPositions = new List<Vector3>(VerticesSize * VerticesSize);
    }
    void CreatePresets()
    {
        ArenaPresets = new List<List<int[,]>>();

        int[,] flatArenaHeightMap = new int[ArenaSize, ArenaSize];
        int[,] flatArenaStairsMap = new int[ArenaSize, ArenaSize];
        for (int i = 0; i < ArenaSize; i++)
            for (int j = 0; j < ArenaSize; j++)
            {
                flatArenaHeightMap[i, j] = 0;
                flatArenaStairsMap[i, j] = 0;
            }

        int[,] cornersHeightMap = new int[ArenaSize, ArenaSize]
        {
            { 0, 0, 0, 0,   0, 0, 0, 0,   0, 0, 0, 0,   0, 0, 0, 0,   0, 0, 0, 0,   0, 0, 0, 0},
            { 0, 0, 0, 0,   0, 0, 0, 0,   0, 0, 0, 0,   0, 0, 0, 0,   0, 0, 0, 0,   0, 0, 0, 0},
            { 0, 0, 0, 0,   0, 0, 0, 0,   0, 0, 0, 0,   0, 0, 0, 0,   0, 0, 0, 0,   0, 0, 0, 0},
            { 0, 0, 0, 0,   0, 0, 0, 0,   0, 0, 0, 0,   0, 0, 0, 0,   0, 0, 0, 0,   0, 0, 0, 0},

            { 0, 0, 0, 0,   6, 6, 6, 6,   6, 0, 0, 0,   0, 0, 0, 6,   6, 6, 6, 6,   0, 0, 0, 0},
            { 0, 0, 0, 0,   6, 0, 0, 0,   0, 0, 0, 0,   0, 0, 0, 0,   0, 0, 0, 6,   0, 0, 0, 0},
            { 0, 0, 0, 0,   6, 0, 0, 0,   0, 0, 0, 0,   0, 0, 0, 0,   0, 0, 0, 6,   0, 0, 0, 0},
            { 0, 0, 0, 0,   6, 0, 0, 0,   0, 0, 0, 0,   0, 0, 0, 0,   0, 0, 0, 6,   0, 0, 0, 0},

            { 0, 0, 0, 0,   6, 0, 0, 0,   0, 0, 0, 0,   0, 0, 0, 0,   0, 0, 0, 6,   0, 0, 0, 0},
            { 0, 0, 0, 0,   0, 0, 0, 0,   0, 0, 0, 0,   0, 0, 0, 0,   0, 0, 0, 0,   0, 0, 0, 0},
            { 0, 0, 0, 0,   0, 0, 0, 0,   0, 0, 0, 0,   0, 0, 0, 0,   0, 0, 0, 0,   0, 0, 0, 0},
            { 0, 0, 0, 0,   0, 0, 0, 0,   0, 0, 0, 0,   0, 0, 0, 0,   0, 0, 0, 0,   0, 0, 0, 0},

            { 0, 0, 0, 0,   0, 0, 0, 0,   0, 0, 0, 0,   0, 0, 0, 0,   0, 0, 0, 0,   0, 0, 0, 0},
            { 0, 0, 0, 0,   0, 0, 0, 0,   0, 0, 0, 0,   0, 0, 0, 0,   0, 0, 0, 0,   0, 0, 0, 0},
            { 0, 0, 0, 0,   0, 0, 0, 0,   0, 0, 0, 0,   0, 0, 0, 0,   0, 0, 0, 0,   0, 0, 0, 0},
            { 0, 0, 0, 0,   6, 0, 0, 0,   0, 0, 0, 0,   0, 0, 0, 0,   0, 0, 0, 6,   0, 0, 0, 0},

            { 0, 0, 0, 0,   6, 0, 0, 0,   0, 0, 0, 0,   0, 0, 0, 0,   0, 0, 0, 6,   0, 0, 0, 0},
            { 0, 0, 0, 0,   6, 0, 0, 0,   0, 0, 0, 0,   0, 0, 0, 0,   0, 0, 0, 6,   0, 0, 0, 0},
            { 0, 0, 0, 0,   6, 0, 0, 0,   0, 0, 0, 0,   0, 0, 0, 0,   0, 0, 0, 6,   0, 0, 0, 0},
            { 0, 0, 0, 0,   6, 6, 6, 6,   6, 0, 0, 0,   0, 0, 0, 6,   6, 6, 6, 6,   0, 0, 0, 0},

            { 0, 0, 0, 0,   0, 0, 0, 0,   0, 0, 0, 0,   0, 0, 0, 0,   0, 0, 0, 0,   0, 0, 0, 0},
            { 0, 0, 0, 0,   0, 0, 0, 0,   0, 0, 0, 0,   0, 0, 0, 0,   0, 0, 0, 0,   0, 0, 0, 0},
            { 0, 0, 0, 0,   0, 0, 0, 0,   0, 0, 0, 0,   0, 0, 0, 0,   0, 0, 0, 0,   0, 0, 0, 0},
            { 0, 0, 0, 0,   0, 0, 0, 0,   0, 0, 0, 0,   0, 0, 0, 0,   0, 0, 0, 0,   0, 0, 0, 0},
        };
        int[,] cornersStairsMap = new int[ArenaSize, ArenaSize];
        for (int i = 0; i < ArenaSize; i++)
            for (int j = 0; j < ArenaSize; j++)
                cornersStairsMap[i, j] = 0;

        int[,] pillarsHeightMap = new int[ArenaSize, ArenaSize]
        {
            { 0, 0, 0, 0,   0, 0, 0, 0,   0, 0, 0, 0,   0, 0, 0, 0,   0, 0, 0, 0,   0, 0, 0, 0},
            { 0, 0, 0, 0,   0, 0, 0, 0,   0, 0, 0, 0,   0, 0, 0, 0,   0, 0, 0, 0,   0, 0, 0, 0},
            { 0, 0, 0, 0,   0, 0, 0, 0,   0, 0, 0, 0,   0, 0, 0, 0,   0, 0, 0, 0,   0, 0, 0, 0},
            { 0, 0, 0, 20,   20, 20, 0, 0,   0, 0, 0, 0,   0, 0, 0, 0,   0, 0, 20, 20,   20, 0, 0, 0},

            { 0, 0, 0, 20,   20, 20, 0, 0,   0, 0, 0, 0,   0, 0, 0, 0,   0, 0, 20, 20,   20, 0, 0, 0},
            { 0, 0, 0, 20,   20, 20, 0, 0,   0, 0, 0, 0,   0, 0, 0, 0,   0, 0, 20, 20,   20, 0, 0, 0},
            { 0, 0, 0, 0,   0, 0, 0, 0,   0, 0, 0, 0,   0, 0, 0, 0,   0, 0, 0, 0,   0, 0, 0, 0},
            { 0, 0, 0, 0,   0, 0, 0, 0,   0, 0, 0, 0,   0, 0, 0, 0,   0, 0, 0, 0,   0, 0, 0, 0},

            { 0, 0, 0, 0,   0, 0, 0, 0,   0, 0, 0, 0,   0, 0, 0, 0,   0, 0, 0, 0,   0, 0, 0, 0},
            { 0, 0, 0, 0,   0, 0, 0, 0,   0, 0, 0, 0,   0, 0, 0, 0,   0, 0, 0, 0,   0, 0, 0, 0},
            { 0, 0, 0, 0,   0, 0, 0, 0,   0, 0, 0, 0,   0, 0, 0, 0,   0, 0, 0, 0,   0, 0, 0, 0},
            { 0, 0, 0, 0,   0, 0, 0, 0,   0, 0, 0, 20,   20, 0, 0, 0,   0, 0, 0, 0,   0, 0, 0, 0},

            { 0, 0, 0, 0,   0, 0, 0, 0,   0, 0, 0, 20,   20, 0, 0, 0,   0, 0, 0, 0,   0, 0, 0, 0},
            { 0, 0, 0, 0,   0, 0, 0, 0,   0, 0, 0, 0,   0, 0, 0, 0,   0, 0, 0, 0,   0, 0, 0, 0},
            { 0, 0, 0, 0,   0, 0, 0, 0,   0, 0, 0, 0,   0, 0, 0, 0,   0, 0, 0, 0,   0, 0, 0, 0},
            { 0, 0, 0, 0,   0, 0, 0, 0,   0, 0, 0, 0,   0, 0, 0, 0,   0, 0, 0, 0,   0, 0, 0, 0},

            { 0, 0, 0, 0,   0, 0, 0, 0,   0, 0, 0, 0,   0, 0, 0, 0,   0, 0, 0, 0,   0, 0, 0, 0},
            { 0, 0, 0, 0,   0, 0, 0, 0,   0, 0, 0, 0,   0, 0, 0, 0,   0, 0, 0, 0,   0, 0, 0, 0},
            { 0, 0, 0, 20,   20, 20, 0, 0,   0, 0, 0, 0,   0, 0, 0, 0,   0, 0, 20, 20,   20, 0, 0, 0},
            { 0, 0, 0, 20,   20, 20, 0, 0,   0, 0, 0, 0,   0, 0, 0, 0,   0, 0, 20, 20,   20, 0, 0, 0},

            { 0, 0, 0, 20,   20, 20, 0, 0,   0, 0, 0, 0,   0, 0, 0, 0,   0, 0, 20, 20,   20, 0, 0, 0},
            { 0, 0, 0, 0,   0, 0, 0, 0,   0, 0, 0, 0,   0, 0, 0, 0,   0, 0, 0, 0,   0, 0, 0, 0},
            { 0, 0, 0, 0,   0, 0, 0, 0,   0, 0, 0, 0,   0, 0, 0, 0,   0, 0, 0, 0,   0, 0, 0, 0},
            { 0, 0, 0, 0,   0, 0, 0, 0,   0, 0, 0, 0,   0, 0, 0, 0,   0, 0, 0, 0,   0, 0, 0, 0},
        };
        int[,] pillarsStairsMap = new int[ArenaSize, ArenaSize];
        for (int i = 0; i < ArenaSize; i++)
            for (int j = 0; j < ArenaSize; j++)
                cornersStairsMap[i, j] = 0;

        int[,] pitHeightMap = new int[ArenaSize, ArenaSize]
        {
            { 10, 10, 10, 10,   10, 10, 10, 10,   10, 10, 10, 10,   10, 10, 10, 10,   10, 10, 10, 10,   10, 10, 10, 10},
            { 10, 0, 0, 0,   0, 0, 0, 0,   0, 0, 0, 0,   0, 0, 0, 0,   0, 0, 0, 0,   0, 0, 0, 10},
            { 10, 0, 0, 0,   0, 0, 0, 0,   0, 0, 0, 0,   0, 0, 0, 0,   0, 0, 0, 0,   0, 0, 0, 10},
            { 10, 0, 0, 0,   0, 0, 0, 0,   0, 0, 0, 0,   0, 0, 0, 0,   0, 0, 0, 0,   0, 0, 0, 10},

            { 10, 0, 0, 0,   0, 0, 0, 0,   0, 0, 0, 0,   0, 0, 0, 0,   0, 0, 0, 0,   0, 0, 0, 10},
            { 10, 0, 0, 0,   0, 0, 0, 0,   0, 0, 0, 0,   0, 0, 0, 0,   0, 0, 0, 0,   0, 0, 0, 10},
            { 10, 0, 0, 0,   0, 0, 0, 0,   0, 0, 0, 0,   0, 0, 0, 0,   0, 0, 0, 0,   0, 0, 0, 10},
            { 10, 0, 0, 0,   0, 0, 0, 0,   0, 0, 0, 0,   0, 0, 0, 0,   0, 0, 0, 0,   0, 0, 0, 10},

            { 10, 0, 0, 0,   0, 0, 0, 0,   0, 0, 0, 0,   0, 0, 0, 0,   0, 0, 0, 0,   0, 0, 0, 10},
            { 10, 0, 0, 0,   0, 0, 0, 0,   0, 0, 0, 0,   0, 0, 0, 0,   0, 0, 0, 0,   0, 0, 0, 10},
            { 10, 0, 0, 0,   0, 0, 0, 0,   0, 0, 0, 0,   0, 0, 0, 0,   0, 0, 0, 0,   0, 0, 0, 10},
            { 10, 0, 0, 0,   0, 0, 0, 0,   0, 0, 0, 0,   0, 0, 0, 0,   0, 0, 0, 0,   0, 0, 0, 10},

            { 10, 0, 0, 0,   0, 0, 0, 0,   0, 0, 0, 0,   0, 0, 0, 0,   0, 0, 0, 0,   0, 0, 0, 10},
            { 10, 0, 0, 0,   0, 0, 0, 0,   0, 0, 0, 0,   0, 0, 0, 0,   0, 0, 0, 0,   0, 0, 0, 10},
            { 10, 0, 0, 0,   0, 0, 0, 0,   0, 0, 0, 0,   0, 0, 0, 0,   0, 0, 0, 0,   0, 0, 0, 10},
            { 10, 0, 0, 0,   0, 0, 0, 0,   0, 0, 0, 0,   0, 0, 0, 0,   0, 0, 0, 0,   0, 0, 0, 10},

            { 10, 0, 0, 0,   0, 0, 0, 0,   0, 0, 0, 0,   0, 0, 0, 0,   0, 0, 0, 0,   0, 0, 0, 10},
            { 10, 0, 0, 0,   0, 0, 0, 0,   0, 0, 0, 0,   0, 0, 0, 0,   0, 0, 0, 0,   0, 0, 0, 10},
            { 10, 0, 0, 0,   0, 0, 0, 0,   0, 0, 0, 0,   0, 0, 0, 0,   0, 0, 0, 0,   0, 0, 0, 10},
            { 10, 0, 0, 0,   0, 0, 0, 0,   0, 0, 0, 0,   0, 0, 0, 0,   0, 0, 0, 0,   0, 0, 0, 10},

            { 10, 0, 0, 0,   0, 0, 0, 0,   0, 0, 0, 0,   0, 0, 0, 0,   0, 0, 0, 0,   0, 0, 0, 10},
            { 10, 0, 0, 0,   0, 0, 0, 0,   0, 0, 0, 0,   0, 0, 0, 0,   0, 0, 0, 0,   0, 0, 0, 10},
            { 10, 0, 0, 0,   0, 0, 0, 0,   0, 0, 0, 0,   0, 0, 0, 0,   0, 0, 0, 0,   0, 0, 0, 10},
            { 10, 10, 10, 10,   10, 10, 10, 10,   10, 10, 10, 10,   10, 10, 10, 10,   10, 10, 10, 10,   10, 10, 10, 10},
        };
        int[,] pitStairsMap = new int[ArenaSize, ArenaSize];
        for (int i = 0; i < ArenaSize; i++)
            for (int j = 0; j < ArenaSize; j++)
                pitStairsMap[i, j] = 0;

        ArenaPresets.Add(new List<int[,]> { flatArenaHeightMap, flatArenaStairsMap });
        ArenaPresets.Add(new List<int[,]> { cornersHeightMap, cornersStairsMap });
        ArenaPresets.Add(new List<int[,]> { pillarsHeightMap, pillarsStairsMap });
        ArenaPresets.Add(new List<int[,]> { pitHeightMap, pitStairsMap });
    }


    public void ChangeArena()
    {
        float coin = UnityEngine.Random.value;
        if (coin < 0.5)
            ChooseFromPresets();
        else
            GenerateCircleArena();

        /*for (int i = 0; i < BuffBoxGO.transform.childCount; i++)
            Destroy(BuffBoxGO.transform.GetChild(i).gameObject);*/
    }


    public void ChooseFromPresets()
    {
        int choice = UnityEngine.Random.Range(0, ArenaPresets.Count);
        HeightMap = (int[,])ArenaPresets[choice][0].Clone();
        StairsMap = (int[,])ArenaPresets[choice][1].Clone();

        flag = 3;
    }


    public void GenerateCircleArena()
    {
        GenerateHeightMap();
        GenerateStairsMap();

        flag = 3;
    }

    void GenerateHeightMap()
    {
        int size = ArenaSize / 2;
        int[] tmpHeightMap = new int[size];

        tmpHeightMap[0] = 0;
        for (int i = 1; i < size; i++)
        {
            int height;
            if (UnityEngine.Random.Range(0, 3) == 0)
                height = 0;
            else
            {
                height = UnityEngine.Random.Range(0, 4);

                if (UnityEngine.Random.Range(0, 2) == 1)
                    height = -height;
            }

            tmpHeightMap[i] = tmpHeightMap[i - 1] + height;
        }

        for (int i = 1; i < size - 1; i++)
            if (tmpHeightMap[i] < tmpHeightMap[i - 1] && tmpHeightMap[i] < tmpHeightMap[i + 1])
                tmpHeightMap[i] = tmpHeightMap[i - 1];

        for (int i = 0; i < size; i++)
        {
            for (int j = i; j < ArenaSize - i; j++)
            {
                HeightMap[i, j] = tmpHeightMap[i];
                HeightMap[ArenaSize - i - 1, j] = tmpHeightMap[i];
                HeightMap[j, i] = tmpHeightMap[i];
                HeightMap[j, ArenaSize - i - 1] = tmpHeightMap[i];
            }
        }
    }

    void GenerateStairsMap()
    {
        // 0 - лестницы нет
        // 1 - лестница вверх
        // 2 - лестница вправо
        // 3 - лестница вниз
        // 4 - лестница влево
        // 5 - выпуклая вправо-вверх
        // 6 - выпуклая вправо-вниз
        // 7 - выпуклая влево-вниз
        // 8 - выпуклая влево-вверх
        // 9 - вогнутая вправо-вверх
        // 10 - вогнутая вправо-вниз
        // 11 - вогнутая влево-вниз
        // 12 - вогнутая влево-вверх

        int up, right, down, left;
        int up_right, down_right, down_left, up_left;
        int current;

        // -=-=-=-=-Расстановка угловых лестниц на главной диагонали-=-=-=-=-
        // Расстановка по краям арены
        if (HeightMap[1, 1] > HeightMap[0, 0])
        {
            StairsMap[0, 0] = 6;
            StairsMap[ArenaSize - 1, ArenaSize - 1] = 8;
        }
        else
        {
            StairsMap[0, 0] = 0;
            StairsMap[ArenaSize - 1, ArenaSize - 1] = 0;
        }
        // Остальные
        for (int i = 1; i < ArenaSize / 2; i++)
        {
            up_left = HeightMap[i - 1, i - 1];
            current = HeightMap[i, i];
            down_right = HeightMap[i + 1, i + 1];

            if (up_left > current)
            {
                StairsMap[i, i] = 12;
                StairsMap[ArenaSize - i - 1, ArenaSize - i - 1] = 10;
            }
            else if (current < down_right)
            {
                StairsMap[i, i] = 6;
                StairsMap[ArenaSize - i - 1, ArenaSize - i - 1] = 8;
            }
            else
            {
                StairsMap[i, i] = 0;
                StairsMap[ArenaSize - i - 1, ArenaSize - i - 1] = 0;
            }
        }

        // -=-=-=-=-Расстановка угловых лестниц на побочной диагонали-=-=-=-=-
        // Расстановка по краям арены
        if (HeightMap[1, ArenaSize - 2] > HeightMap[0, ArenaSize - 1])
        {
            StairsMap[0, ArenaSize - 1] = 7;
            StairsMap[ArenaSize - 1, 0] = 5;
        }
        else
        {
            StairsMap[0, ArenaSize - 1] = 0;
            StairsMap[ArenaSize - 1, 0] = 0;
        }
        // Остальные
        for (int i = 1; i < ArenaSize / 2; i++)
        {
            up_right = HeightMap[i - 1, ArenaSize - i];
            current = HeightMap[i, ArenaSize - i - 1];
            down_left = HeightMap[i + 1, ArenaSize - i - 2];

            if (up_right > current)
            {
                StairsMap[i, ArenaSize - i - 1] = 9;
                StairsMap[ArenaSize - i - 1, i] = 11;
            }
            else if (current < down_left)
            {
                StairsMap[i, ArenaSize - i - 1] = 7;
                StairsMap[ArenaSize - i - 1, i] = 5;
            }
            else
            {
                StairsMap[i, ArenaSize - i - 1] = 0;
                StairsMap[ArenaSize - i - 1, i] = 0;
            }
        }

        // -=-=-=-=-Расстановка фронтальных лестниц-=-=-=-=-
        // Расстановка по краям арены
        if (HeightMap[1, 2] > HeightMap[0, 2])
        {
            for (int i = 1; i < ArenaSize - 1; i++)
            {
                StairsMap[ArenaSize - 1, i] = 1;
                StairsMap[i, 0] = 2;
                StairsMap[0, i] = 3;
                StairsMap[i, ArenaSize - 1] = 4;
            }
        }
        else
        {
            for (int i = 1; i < ArenaSize - 1; i++)
            {
                StairsMap[ArenaSize - 1, i] = 0;
                StairsMap[i, 0] = 0;
                StairsMap[0, i] = 0;
                StairsMap[i, ArenaSize - 1] = 0;
            }
        }
        // Остальные
        for (int i = 1; i < ArenaSize - 1; i++)
        {
            for (int j = 1; j < ArenaSize - 1; j++)
            {
                if (i < j && i + j < ArenaSize - 1 ||
                    i > j && i + j > ArenaSize - 1)
                {
                    up = HeightMap[i - 1, j];
                    current = HeightMap[i, j];
                    down = HeightMap[i + 1, j];

                    if (up > current)
                        StairsMap[i, j] = 1;
                    else if (current < down)
                        StairsMap[i, j] = 3;
                    else
                        StairsMap[i, j] = 0;
                }
                else if (i < j && i + j > ArenaSize - 1 ||
                    i > j && i + j < ArenaSize - 1)
                {
                    left = HeightMap[i, j - 1];
                    current = HeightMap[i, j];
                    right = HeightMap[i, j + 1];

                    if (left > current)
                        StairsMap[i, j] = 4;
                    else if (current < right)
                        StairsMap[i, j] = 2;
                    else
                        StairsMap[i, j] = 0;
                }
            }
        }
    }


    void RotateTriangles()
    {
        int[] triangles = new int[ArenaSize * ArenaSize * 6];

        for (int ti = 0, vi = 0, i = 0; i < ArenaSize; i++, vi++)
            for (int j = 0; j < ArenaSize; j++, vi++)
            {
                if (StairsMap[i, j] > 4 && StairsMap[i, j] % 2 == 1)
                {
                    triangles[ti++] = vi;
                    triangles[ti++] = vi + 1;
                    triangles[ti++] = vi + VerticesSize;

                    triangles[ti++] = vi + 1;
                    triangles[ti++] = vi + VerticesSize + 1;
                    triangles[ti++] = vi + VerticesSize;
                }
                else
                {
                    triangles[ti++] = vi;
                    triangles[ti++] = vi + VerticesSize + 1;
                    triangles[ti++] = vi + VerticesSize;

                    triangles[ti++] = vi;
                    triangles[ti++] = vi + 1;
                    triangles[ti++] = vi + VerticesSize + 1;
                }
            }

        Arena.GetComponent<MeshFilter>().mesh.triangles = triangles;
        Arena.GetComponent<MeshFilter>().mesh.RecalculateBounds();
        Arena.GetComponent<MeshFilter>().mesh.RecalculateNormals();
    }

    void CalculateVerticesPositions()
    {
        CurrentVerticesPositions = new List<Vector3>(Arena.GetComponent<MeshFilter>().mesh.vertices);
        NewVerticesPositions = new List<Vector3>(Arena.GetComponent<MeshFilter>().mesh.vertices);   
        CurrentTriangles = new List<int>(Arena.GetComponent<MeshFilter>().mesh.triangles);
        NewUV = new List<Vector2>(Arena.GetComponent<MeshFilter>().mesh.uv);

        for (int i = 0; i < ArenaSize; i++)
            for (int j = 0; j < ArenaSize; j++)
            {
                // 0 - лестницы нет
                // 1 - лестница вверх
                // 2 - лестница вправо
                // 3 - лестница вниз
                // 4 - лестница влево
                // 5 - выпуклая вправо-вверх
                // 6 - выпуклая вправо-вниз
                // 7 - выпуклая влево-вниз
                // 8 - выпуклая влево-вверх
                // 9 - вогнутая вправо-вверх
                // 10 - вогнутая вправо-вниз
                // 11 - вогнутая влево-вниз
                // 12 - вогнутая влево-вверх

                int vertex_idx = j + i * VerticesSize;

                Vector3 top_left        = NewVerticesPositions[vertex_idx];
                Vector3 top_right       = NewVerticesPositions[vertex_idx + 1];
                Vector3 bottom_left     = NewVerticesPositions[vertex_idx + VerticesSize];
                Vector3 bottom_right    = NewVerticesPositions[vertex_idx + VerticesSize + 1];

                switch (StairsMap[i, j])
                {
                    case 1:
                        top_left.y      = HeightMap[i - 1, j];
                        top_right.y     = HeightMap[i - 1, j];
                        bottom_left.y   = HeightMap[i, j];
                        bottom_right.y  = HeightMap[i, j];
                        break;
                    case 2:
                        top_left.y      = HeightMap[i, j];
                        top_right.y     = HeightMap[i, j + 1];
                        bottom_left.y   = HeightMap[i, j];
                        bottom_right.y  = HeightMap[i, j + 1];
                        break;
                    case 3:
                        top_left.y      = HeightMap[i, j];
                        top_right.y     = HeightMap[i, j];
                        bottom_left.y   = HeightMap[i + 1, j];
                        bottom_right.y  = HeightMap[i + 1, j];
                        break;
                    case 4:
                        top_left.y      = HeightMap[i, j - 1];
                        top_right.y     = HeightMap[i, j];
                        bottom_left.y   = HeightMap[i, j - 1];
                        bottom_right.y  = HeightMap[i, j];
                        break;
                    case 5:
                        top_left.y      = HeightMap[i, j];
                        top_right.y     = HeightMap[i - 1, j + 1];
                        bottom_left.y   = HeightMap[i, j];
                        bottom_right.y  = HeightMap[i, j];
                        break;
                    case 6:
                        top_left.y      = HeightMap[i, j];
                        top_right.y     = HeightMap[i, j];
                        bottom_left.y   = HeightMap[i, j];
                        bottom_right.y  = HeightMap[i + 1, j + 1];
                        break;
                    case 7:
                        top_left.y      = HeightMap[i, j];
                        top_right.y     = HeightMap[i, j];
                        bottom_left.y   = HeightMap[i + 1, j - 1];
                        bottom_right.y  = HeightMap[i, j];
                        break;
                    case 8:
                        top_left.y      = HeightMap[i - 1, j - 1];
                        top_right.y     = HeightMap[i, j];
                        bottom_left.y   = HeightMap[i, j];
                        bottom_right.y  = HeightMap[i, j];
                        break;
                    case 9:
                        top_left.y      = HeightMap[i - 1, j + 1];
                        top_right.y     = HeightMap[i - 1, j + 1];
                        bottom_left.y   = HeightMap[i, j];
                        bottom_right.y  = HeightMap[i - 1, j + 1];
                        break;
                    case 10:
                        top_left.y      = HeightMap[i, j];
                        top_right.y     = HeightMap[i + 1, j + 1];
                        bottom_left.y   = HeightMap[i + 1, j + 1];
                        bottom_right.y  = HeightMap[i + 1, j + 1];
                        break;
                    case 11:
                        top_left.y      = HeightMap[i + 1, j - 1];
                        top_right.y     = HeightMap[i, j];
                        bottom_left.y   = HeightMap[i + 1, j - 1];
                        bottom_right.y  = HeightMap[i + 1, j - 1];
                        break;
                    case 12:
                        top_left.y      = HeightMap[i - 1, j - 1];
                        top_right.y     = HeightMap[i - 1, j - 1];
                        bottom_left.y   = HeightMap[i - 1, j - 1];
                        bottom_right.y  = HeightMap[i, j];
                        break;
                    default:
                        top_left.y = HeightMap[i, j];
                        if (i > 0)
                            top_left.y = Mathf.Max(top_left.y, HeightMap[i - 1, j]);
                        if (j > 0)
                            top_left.y = Mathf.Max(top_left.y, HeightMap[i, j - 1]);
                        if (i > 0 && j > 0)
                            top_left.y = Mathf.Max(top_left.y, HeightMap[i - 1, j - 1]);

                        top_right.y = HeightMap[i, j];
                        if (i > 0)
                            top_right.y = Mathf.Max(top_right.y, HeightMap[i - 1, j]);
                        if (j < ArenaSize - 1)
                            top_right.y = Mathf.Max(top_right.y, HeightMap[i, j + 1]);
                        if (i > 0 && j < ArenaSize - 1)
                            top_right.y = Mathf.Max(top_right.y, HeightMap[i - 1, j + 1]);

                        bottom_left.y = HeightMap[i, j];
                        if (i < ArenaSize - 1)
                            bottom_left.y = Mathf.Max(bottom_left.y, HeightMap[i + 1, j]);
                        if (j > 0)
                            bottom_left.y = Mathf.Max(bottom_left.y, HeightMap[i, j - 1]);
                        if (i < ArenaSize - 1 && j > 0)
                            bottom_left.y = Mathf.Max(bottom_left.y, HeightMap[i + 1, j - 1]);

                        bottom_right.y = HeightMap[i, j];
                        if (i < ArenaSize - 1)
                            bottom_right.y = Mathf.Max(bottom_right.y, HeightMap[i + 1, j]);
                        if (j < ArenaSize - 1)
                            bottom_right.y = Mathf.Max(bottom_right.y, HeightMap[i, j + 1]);
                        if (i < ArenaSize - 1 && j < ArenaSize - 1)
                            bottom_right.y = Mathf.Max(bottom_right.y, HeightMap[i + 1, j + 1]);
                        break;
                }

                NewVerticesPositions[vertex_idx] = top_left;
                NewVerticesPositions[vertex_idx + 1] = top_right;
                NewVerticesPositions[vertex_idx + VerticesSize] = bottom_left;
                NewVerticesPositions[vertex_idx + VerticesSize + 1] = bottom_right;
            }
    }

    void PlaceWalls()
    {
        for (int i = 0; i < ArenaSize; i++)
            for(int j = 0; j < ArenaSize; j++)
                if (StairsMap[i, j] == 0)
                    PlaceWallsAroundChunk(i, j);
    }

    void PlaceWallsAroundChunk(int i, int j)
    {
        int vertex_idx = j + i * VerticesSize;

        int top_left        = vertex_idx;
        int top_right       = vertex_idx + 1;
        int bottom_left     = vertex_idx + VerticesSize;
        int bottom_right    = vertex_idx + VerticesSize + 1;

        // Upper wall
        if (i > 0 && HeightMap[i - 1, j] > HeightMap[i, j])
        {
            // Creating new points
            // Setting their start position
            CurrentVerticesPositions.Add(new Vector3
            (
                CurrentVerticesPositions[top_left].x,
                0,
                CurrentVerticesPositions[top_left].z)
            );
            CurrentVerticesPositions.Add(new Vector3
            (
                CurrentVerticesPositions[top_right].x,
                0,
                CurrentVerticesPositions[top_right].z)
            );
            // Setting their target position
            NewVerticesPositions.Add(new Vector3(
                CurrentVerticesPositions[top_left].x,
                HeightMap[i, j],
                CurrentVerticesPositions[top_left].z)
            );
            NewVerticesPositions.Add(new Vector3(
                CurrentVerticesPositions[top_right].x,
                HeightMap[i, j],
                CurrentVerticesPositions[top_right].z)
            );
            // Getting their indecies
            int new_top_left_idx = CurrentVerticesPositions.Count - 2;
            int new_top_right_idx = CurrentVerticesPositions.Count - 1;

            // Creating wall's triangles
            CurrentTriangles.Add(top_left);
            CurrentTriangles.Add(new_top_right_idx);
            CurrentTriangles.Add(new_top_left_idx);

            CurrentTriangles.Add(top_left);
            CurrentTriangles.Add(top_right);
            CurrentTriangles.Add(new_top_right_idx);

            // Changing triangles of the lower tile
            CurrentTriangles[((top_left - i) * 6)] = new_top_left_idx;

            CurrentTriangles[((top_left - i) * 6) + 3] = new_top_left_idx;
            CurrentTriangles[((top_left - i) * 6) + 4] = new_top_right_idx;

            // Changing triangles of neighboring tiles
            if (j > 0 && HeightMap[i, j - 1] == HeightMap[i, j])
            {
                int left_tile_top_left = top_left - 1;
                CurrentTriangles[((left_tile_top_left - i) * 6) + 4] = new_top_left_idx;
            }
            if (j < ArenaSize - 1 && HeightMap[i, j + 1] == HeightMap[i, j])
            {
                int right_tile_top_left = top_left + 1;
                CurrentTriangles[((right_tile_top_left - i) * 6)] = new_top_right_idx;
                CurrentTriangles[((right_tile_top_left - i) * 6) + 3] = new_top_right_idx;
            }

            // Changing walls' uv
            NewUV.Add((NewUV[top_left] + NewUV[bottom_left]) / 2);
            NewUV.Add((NewUV[top_right] + NewUV[bottom_right]) / 2);
        }
        // Bottom wall
        if (i < ArenaSize - 1 && HeightMap[i + 1, j] > HeightMap[i, j])
        {
            // Creating new points
            // Setting their start position
            CurrentVerticesPositions.Add(new Vector3
            (
                CurrentVerticesPositions[bottom_left].x,
                0,
                CurrentVerticesPositions[bottom_left].z)
            );
            CurrentVerticesPositions.Add(new Vector3
            (
                CurrentVerticesPositions[bottom_right].x,
                0,
                CurrentVerticesPositions[bottom_right].z)
            );
            // Setting their target position
            NewVerticesPositions.Add(new Vector3(
                CurrentVerticesPositions[bottom_left].x,
                HeightMap[i, j],
                CurrentVerticesPositions[bottom_left].z)
            );
            NewVerticesPositions.Add(new Vector3(
                CurrentVerticesPositions[bottom_right].x,
                HeightMap[i, j],
                CurrentVerticesPositions[bottom_right].z)
            );
            // Getting their indecies
            int new_bottom_left_idx = CurrentVerticesPositions.Count - 2;
            int new_bottom_right_idx = CurrentVerticesPositions.Count - 1;

            // Creating wall's triangles
            CurrentTriangles.Add(bottom_right);
            CurrentTriangles.Add(new_bottom_left_idx);
            CurrentTriangles.Add(new_bottom_right_idx);

            CurrentTriangles.Add(bottom_right);
            CurrentTriangles.Add(bottom_left);
            CurrentTriangles.Add(new_bottom_left_idx);

            // Changing triangles of the lower tile
            CurrentTriangles[((top_left - i) * 6) + 1] = new_bottom_right_idx;
            CurrentTriangles[((top_left - i) * 6) + 2] = new_bottom_left_idx;

            CurrentTriangles[((top_left - i) * 6) + 5] = new_bottom_right_idx;

            // Changing triangles of neighboring tiles
            if (j > 0 && HeightMap[i, j - 1] == HeightMap[i, j])
            {
                int left_tile_top_left = top_left - 1;
                CurrentTriangles[((left_tile_top_left - i) * 6) + 1] = new_bottom_left_idx;
                CurrentTriangles[((left_tile_top_left - i) * 6) + 5] = new_bottom_left_idx;
            }
            if (j < ArenaSize - 1 && HeightMap[i, j + 1] == HeightMap[i, j])
            {
                int right_tile_top_left = top_left + 1;
                CurrentTriangles[((right_tile_top_left - i) * 6) + 2] = new_bottom_right_idx;
            }

            // Changing walls' uv
            NewUV.Add((NewUV[bottom_left] + NewUV[top_left]) / 2);
            NewUV.Add((NewUV[bottom_right] + NewUV[top_right]) / 2);
        }
        // Left wall
        if (j > 0 && HeightMap[i, j - 1] > HeightMap[i, j])
        {
            // Creating new points
            // Setting their start position
            CurrentVerticesPositions.Add(new Vector3
            (
                CurrentVerticesPositions[top_left].x,
                0,
                CurrentVerticesPositions[top_left].z)
            );
            CurrentVerticesPositions.Add(new Vector3
            (
                CurrentVerticesPositions[bottom_left].x,
                0,
                CurrentVerticesPositions[bottom_left].z)
            );
            // Setting their target position
            NewVerticesPositions.Add(new Vector3(
                CurrentVerticesPositions[top_left].x,
                HeightMap[i, j],
                CurrentVerticesPositions[top_left].z)
            );
            NewVerticesPositions.Add(new Vector3(
                CurrentVerticesPositions[bottom_left].x,
                HeightMap[i, j],
                CurrentVerticesPositions[bottom_left].z)
            );
            // Getting their indecies
            int new_top_left_idx = CurrentVerticesPositions.Count - 2;
            int new_bottom_left_idx = CurrentVerticesPositions.Count - 1;

            // Creating wall's triangles
            CurrentTriangles.Add(bottom_left);
            CurrentTriangles.Add(new_top_left_idx); 
            CurrentTriangles.Add(new_bottom_left_idx);

            CurrentTriangles.Add(bottom_left);
            CurrentTriangles.Add(top_left);
            CurrentTriangles.Add(new_top_left_idx);

            // Changing triangles of the lower tile
            CurrentTriangles[((top_left - i) * 6)] = new_top_left_idx;
            CurrentTriangles[((top_left - i) * 6) + 2] = new_bottom_left_idx;

            CurrentTriangles[((top_left - i) * 6) + 3] = new_top_left_idx;

            // Changing triangles of neighboring tiles
            if (i > 0 && HeightMap[i - 1, j] == HeightMap[i, j])
            {
                int top_tile_top_left = top_left - (VerticesSize - 1);
                CurrentTriangles[((top_tile_top_left - i) * 6) + 2] = new_top_left_idx;
            }
            if (i < ArenaSize - 1 && HeightMap[i + 1, j] == HeightMap[i, j])
            {
                int bottom_tile_top_left = top_left + (VerticesSize - 1);
                CurrentTriangles[((bottom_tile_top_left - i) * 6)] = new_bottom_left_idx;
                CurrentTriangles[((bottom_tile_top_left - i) * 6) + 3] = new_bottom_left_idx;
            }

            // Changing walls' uv
            NewUV.Add((NewUV[top_left] + NewUV[top_right]) / 2);
            NewUV.Add((NewUV[bottom_left] + NewUV[bottom_right]) / 2);
        }
        // Right wall
        if (j < ArenaSize - 1 && HeightMap[i, j + 1] > HeightMap[i, j])
        {
            // Creating new points
            // Setting their start position
            CurrentVerticesPositions.Add(new Vector3
            (
                CurrentVerticesPositions[top_right].x,
                0,
                CurrentVerticesPositions[top_right].z)
            );
            CurrentVerticesPositions.Add(new Vector3
            (
                CurrentVerticesPositions[bottom_right].x,
                0,
                CurrentVerticesPositions[bottom_right].z)
            );
            // Setting their target position
            NewVerticesPositions.Add(new Vector3(
                CurrentVerticesPositions[top_right].x,
                HeightMap[i, j],
                CurrentVerticesPositions[top_right].z)
            );
            NewVerticesPositions.Add(new Vector3(
                CurrentVerticesPositions[bottom_right].x,
                HeightMap[i, j],
                CurrentVerticesPositions[bottom_right].z)
            );
            // Getting their indecies
            int new_top_right_idx = CurrentVerticesPositions.Count - 2;
            int new_bottom_right_idx = CurrentVerticesPositions.Count - 1;

            // Creating wall's triangles
            CurrentTriangles.Add(top_right);
            CurrentTriangles.Add(new_bottom_right_idx);
            CurrentTriangles.Add(new_top_right_idx);

            CurrentTriangles.Add(top_right);
            CurrentTriangles.Add(bottom_right);
            CurrentTriangles.Add(new_bottom_right_idx);

            // Changing triangles of the lower tile
            CurrentTriangles[((top_left - i) * 6) + 1] = new_bottom_right_idx;

            CurrentTriangles[((top_left - i) * 6) + 4] = new_top_right_idx;
            CurrentTriangles[((top_left - i) * 6) + 5] = new_bottom_right_idx;

            // Changing triangles of neighboring tiles
            if (i > 0 && HeightMap[i - 1, j] == HeightMap[i, j])
            {
                int top_tile_top_left = top_left - (VerticesSize - 1);
                CurrentTriangles[((top_tile_top_left - i) * 6) + 1] = new_top_right_idx;
                CurrentTriangles[((top_tile_top_left - i) * 6) + 5] = new_top_right_idx;
            }
            if (i < ArenaSize - 1 && HeightMap[i + 1, j] == HeightMap[i, j])
            {
                int bottom_tile_top_left = top_left + (VerticesSize - 1);
                CurrentTriangles[((bottom_tile_top_left - i) * 6) + 4] = new_bottom_right_idx;
            }

            // Changing walls' uv
            NewUV.Add((NewUV[top_right] + NewUV[top_left]) / 2);
            NewUV.Add((NewUV[bottom_right] + NewUV[bottom_left]) / 2);
        }
    }


    bool SmoothTransformToFlat()
    {
        Mesh arenaMesh = Arena.GetComponent<MeshFilter>().mesh;
        Vector3[] vertices = arenaMesh.vertices;

        bool isFullTransformedToFlat = true;

        for (int i = 0; i < NewVerticesPositions.Count; i++)
        {
            Vector3 position = new Vector3(
                vertices[i].x,
                0,
                vertices[i].z);

            vertices[i] = Vector3.Lerp(vertices[i], position, ChangeSpeed * Time.deltaTime);

            if (Mathf.Abs(vertices[i].y - position.y) > 0.01)
                isFullTransformedToFlat = false;
        }

        arenaMesh.vertices = vertices;
        arenaMesh.RecalculateBounds();
        arenaMesh.RecalculateNormals();
        Arena.GetComponent<MeshCollider>().sharedMesh = arenaMesh;

        ChangeSpeed += ChangeSpeedRatio;
        return isFullTransformedToFlat;
    }

    bool SmoothTransformToTarget()
    {
        Mesh arenaMesh = Arena.GetComponent<MeshFilter>().mesh;
        Vector3[] vertices = arenaMesh.vertices;

        bool isFullTransformedToTarget = true;

        for (int i = 0; i < NewVerticesPositions.Count; i++)
        {
            vertices[i] = Vector3.Lerp(vertices[i], NewVerticesPositions[i], ChangeSpeed * Time.deltaTime);

            if (Mathf.Abs(vertices[i].y - NewVerticesPositions[i].y) > 0.01)
                isFullTransformedToTarget = false;
        }

        arenaMesh.vertices = vertices;
        arenaMesh.RecalculateBounds();
        arenaMesh.RecalculateNormals();
        Arena.GetComponent<MeshCollider>().sharedMesh = arenaMesh;

        ChangeSpeed += ChangeSpeedRatio;
        return isFullTransformedToTarget;
    }


    public int GetArenaSize()
    {
        return ArenaSize;
    }
    public float GetChunkScale()
    {
        return ChunkScale;
    }
    public int GetKillHeight()
    {
        return KillHeight;
    }
    
    public Vector3 GetRandomPoint()
    {
        float arenaSize = ArenaSize * ChunkScale;
        float x = Random.Range(0, arenaSize);
        float z = Random.Range(0, arenaSize);
        Vector3 spawnPosition = new Vector3(x, 100, z);

        RaycastHit ray;
        Physics.Raycast(
            origin: spawnPosition,
            direction: new Vector3(0, -1, 0),
            hitInfo: out ray,
            maxDistance: 150f
        );

        return ray.point;
    }
}