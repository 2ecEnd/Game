using System.Collections.Generic;
using UnityEngine;

public class ArenaManager : MonoBehaviour
{
    [Header("Objects")]
    GameObject Arena;
    public GameObject Player;
    public GameObject Quad;

    [Header("Arena Parameters")]
    public int[,] HeightMap;
    public int[,] StairsMap;
    public Material ArenaMaterial;
    List<List<int[,]>> ArenaPresets;

    [Header("BuffBox Parameters")]
    public GameObject BuffBoxGO;

    [Header("Other Parameters")]
    public float DefaultChangeSpeed;
    public float ChangeSpeedRatio;
    float ChangeSpeed;
    short flag = 0;
    const int ArenaSize = 20;
    const int VerticesSize = ArenaSize + 1;
    const float ChunkScale = 4f;
    const int KillHeight = -30;


    List<Vector3> CurrentVerticesPositions;
    List<Vector3> NewVerticesPositions;
    List<int> CurrentTriangles;

    void Start()
    {
        CreateArena();
        CreatePresets();

        HeightMap = (int[,])ArenaPresets[0][0].Clone();
        StairsMap = (int[,])ArenaPresets[0][1].Clone();

        NewVerticesPositions = new List<Vector3>(VerticesSize * VerticesSize);
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
                    Arena.GetComponent<MeshFilter>().mesh.RecalculateNormals();

                    ChangeSpeed = DefaultChangeSpeed;
                    flag = 4;

                    for (int i = 0; i < NewVerticesPositions.Count; i++)
                    {
                        print(i.ToString() + " - " + NewVerticesPositions[i].y.ToString());
                    }
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

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            flag = 1;
        }
        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            flag = 2;
        }
    }

    void CreateArena()
    {
        Arena = new GameObject("Arena");
        Arena.AddComponent<MeshCollider>();
        Arena.AddComponent<MeshFilter>();
        Arena.AddComponent<MeshRenderer>();

        Mesh arenaMesh = new Mesh();

        Vector3[] vertices = new Vector3[(ArenaSize + 1) * (ArenaSize + 1)];
        Vector2[] uv = new Vector2[(ArenaSize + 1) * (ArenaSize + 1)];
        int[] triangles = new int[ArenaSize * ArenaSize * 6];

        for (int i = 0; i <= ArenaSize; i++)
            for (int j = 0; j <= ArenaSize; j++)
            {
                int idx = j + i * (ArenaSize + 1);
                vertices[idx] = new Vector3(i * ChunkScale, 0, j * ChunkScale);

                float randomOffsetX = Random.Range(-0.2f, 0.3f);
                float randomOffsetY = Random.Range(-0.2f, 0.3f);
                randomOffsetX = 0;
                randomOffsetY = 0;
                uv[idx] = new Vector2(
                    (i + randomOffsetX) * (1 / ChunkScale),
                    (j + randomOffsetY) * (1 / ChunkScale));
            }

        for (int ti = 0, vi = 0, i = 0; i < ArenaSize; i++, vi++)
            for (int j = 0; j < ArenaSize; j++, vi++)
            {
                triangles[ti++] = vi;
                triangles[ti++] = vi + ArenaSize + 2;
                triangles[ti++] = vi + ArenaSize + 1;

                triangles[ti++] = vi;
                triangles[ti++] = vi + 1;
                triangles[ti++] = vi + ArenaSize + 2;
            }

        arenaMesh.vertices = vertices;
        arenaMesh.uv = uv;
        arenaMesh.triangles = triangles;

        arenaMesh.RecalculateBounds();
        arenaMesh.RecalculateNormals();

        Arena.GetComponent<MeshCollider>().sharedMesh = arenaMesh;
        Arena.GetComponent<MeshFilter>().mesh = arenaMesh;
        Arena.GetComponent<MeshRenderer>().material = ArenaMaterial;
        Arena.GetComponent<MeshRenderer>().material.mainTextureScale = new Vector2(ChunkScale, ChunkScale);
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

        int[,] pillarsHeightMap = new int[ArenaSize, ArenaSize]
        {
            { 0, 0, 0, 0,   0, 0, 0, 0,   0, 0, 0, 0,   0, 0, 0, 0,   0, 0, 0, 0},
            { 0, 0, 0, 0,   0, 0, 0, 0,   0, 0, 0, 0,   0, 0, 0, 0,   0, 0, 0, 0},
            { 0, 0, 0, 0,   0, 0, 0, 0,   0, 0, 0, 0,   0, 0, 0, 0,   0, 0, 0, 0},
            { 0, 0, 0, 0,   0, 0, 0, 0,   0, 0, 0, 0,   0, 0, 0, 0,   0, 0, 0, 0},

            { 0, 0, 0, 0,   2, 2, 2, 2,   0, 0, 0, 0,   2, 2, 2, 2,   0, 0, 0, 0},
            { 0, 0, 0, 0,   2, 0, 0, 0,   0, 0, 0, 0,   0, 0, 0, 2,   0, 0, 0, 0},
            { 0, 0, 0, 0,   2, 0, 0, 0,   0, 0, 0, 0,   0, 0, 0, 2,   0, 0, 0, 0},
            { 0, 0, 0, 0,   2, 0, 0, 0,   0, 0, 0, 0,   0, 0, 0, 2,   0, 0, 0, 0},

            { 0, 0, 0, 0,   0, 0, 0, 0,   0, 0, 0, 0,   0, 0, 0, 0,   0, 0, 0, 0},
            { 0, 0, 0, 0,   0, 0, 0, 0,   0, 0, 0, 0,   0, 0, 0, 0,   0, 0, 0, 0},
            { 0, 0, 0, 0,   0, 0, 0, 0,   0, 0, 0, 0,   0, 0, 0, 0,   0, 0, 0, 0},
            { 0, 0, 0, 0,   0, 0, 0, 0,   0, 0, 0, 0,   0, 0, 0, 0,   0, 0, 0, 0},

            { 0, 0, 0, 0,   2, 0, 0, 0,   0, 0, 0, 0,   0, 0, 0, 2,   0, 0, 0, 0},
            { 0, 0, 0, 0,   2, 0, 0, 0,   0, 0, 0, 0,   0, 0, 0, 2,   0, 0, 0, 0},
            { 0, 0, 0, 0,   2, 0, 0, 0,   0, 0, 0, 0,   0, 0, 0, 2,   0, 0, 0, 0},
            { 0, 0, 0, 0,   2, 2, 2, 2,   0, 0, 0, 0,   2, 2, 2, 2,   0, 0, 0, 0},

            { 0, 0, 0, 0,   0, 0, 0, 0,   0, 0, 0, 0,   0, 0, 0, 0,   0, 0, 0, 0},
            { 0, 0, 0, 0,   0, 0, 0, 0,   0, 0, 0, 0,   0, 0, 0, 0,   0, 0, 0, 0},
            { 0, 0, 0, 0,   0, 0, 0, 0,   0, 0, 0, 0,   0, 0, 0, 0,   0, 0, 0, 0},
            { 0, 0, 0, 0,   0, 0, 0, 0,   0, 0, 0, 0,   0, 0, 0, 0,   0, 0, 0, 0},
        };
        int[,] pillarsStairsMap = new int[ArenaSize, ArenaSize];
        for (int i = 0; i < ArenaSize; i++)
            for (int j = 0; j < ArenaSize; j++)
                pillarsStairsMap[i, j] = 0;

        int[,] pillarHeightMap = new int[ArenaSize, ArenaSize]
        {
            { 0, 0, 0, 0,   0, 0, 0, 0,   0, 0, 0, 0,   0, 0, 0, 0,   0, 0, 0, 0},
            { 0, 4, 0, 0,   0, 0, 0, 0,   0, 0, 0, 0,   0, 0, 0, 0,   0, 0, 0, 0},
            { 0, 0, 0, 0,   0, 0, 0, 0,   0, 0, 0, 0,   0, 0, 0, 0,   0, 0, 0, 0},
            { 0, 0, 0, 0,   0, 0, 0, 0,   0, 0, 0, 0,   0, 0, 0, 0,   0, 0, 0, 0},

            { 0, 0, 0, 0,   0, 0, 0, 0,   0, 0, 0, 0,   0, 0, 0, 0,   0, 0, 0, 0},
            { 0, 0, 0, 0,   0, 0, 0, 0,   0, 0, 0, 0,   0, 0, 0, 0,   0, 0, 0, 0},
            { 0, 0, 0, 0,   0, 0, 0, 0,   0, 0, 0, 0,   0, 0, 0, 0,   0, 0, 0, 0},
            { 0, 0, 0, 0,   0, 0, 0, 0,   0, 0, 0, 0,   0, 0, 0, 0,   0, 0, 0, 0},

            { 0, 0, 0, 0,   0, 0, 0, 0,   0, 0, 0, 0,   0, 0, 0, 0,   0, 0, 0, 0},
            { 0, 0, 0, 0,   0, 0, 0, 0,   0, 0, 0, 0,   0, 0, 0, 0,   0, 0, 0, 0},
            { 0, 0, 0, 0,   0, 0, 0, 0,   0, 0, 0, 0,   0, 0, 0, 0,   0, 0, 0, 0},
            { 0, 0, 0, 0,   0, 0, 0, 0,   0, 0, 0, 0,   0, 0, 0, 0,   0, 0, 0, 0},

            { 0, 0, 0, 0,   0, 0, 0, 0,   0, 0, 0, 0,   0, 0, 0, 0,   0, 0, 0, 0},
            { 0, 0, 0, 0,   0, 0, 0, 0,   0, 0, 0, 0,   0, 0, 0, 0,   0, 0, 0, 0},
            { 0, 0, 0, 0,   0, 0, 0, 0,   0, 0, 0, 0,   0, 0, 0, 0,   0, 0, 0, 0},
            { 0, 0, 0, 0,   0, 0, 0, 0,   0, 0, 0, 0,   0, 0, 0, 0,   0, 0, 0, 0},

            { 0, 0, 0, 0,   0, 0, 0, 0,   0, 0, 0, 0,   0, 0, 0, 0,   0, 0, 0, 0},
            { 0, 0, 0, 0,   0, 0, 0, 0,   0, 0, 0, 0,   0, 0, 0, 0,   0, 0, 0, 0},
            { 0, 0, 0, 0,   0, 0, 0, 0,   0, 0, 0, 0,   0, 0, 0, 0,   0, 0, 0, 0},
            { 0, 0, 0, 0,   0, 0, 0, 0,   0, 0, 0, 0,   0, 0, 0, 0,   0, 0, 0, 0},
        };
        int[,] pillarStairsMap = new int[ArenaSize, ArenaSize];
        for (int i = 0; i < ArenaSize; i++)
            for (int j = 0; j < ArenaSize; j++)
                pillarsStairsMap[i, j] = 0;

        ArenaPresets.Add(new List<int[,]> { flatArenaHeightMap, flatArenaStairsMap });
        ArenaPresets.Add(new List<int[,]> { pillarsHeightMap, pillarsStairsMap });
        //ArenaPresets.Add(new List<int[,]> { pillarHeightMap, pillarStairsMap });
    }


    void ChangeArena()
    {
        if (flag == 1)
        {
            ChooseFromPresets();
        }
        else if (flag == 2)
        {
            GenerateCircleArena();
        }
        //float coin = UnityEngine.Random.value;
        //if (coin < 0.7)
        //    generateCircleArena();
        //else
        //    chooseFromPresets();

        //TransformArena();

        for (int i = 0; i < BuffBoxGO.transform.childCount; i++)
            Destroy(BuffBoxGO.transform.GetChild(i).gameObject);

        flag = 3;
    }


    void ChooseFromPresets()
    {
        int choice = UnityEngine.Random.Range(0, ArenaPresets.Count);
        HeightMap = (int[,])ArenaPresets[choice][0].Clone();
        StairsMap = (int[,])ArenaPresets[choice][1].Clone();
    }


    void GenerateCircleArena()
    {
        GenerateHeightMap();
        GenerateStairsMap();
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
                        top_left.y      = HeightMap[i, j];
                        top_right.y     = HeightMap[i, j];
                        bottom_left.y   = HeightMap[i, j];
                        bottom_right.y  = HeightMap[i, j];
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

        if (i > 0 && HeightMap[i - 1, j] > HeightMap[i, j])
        {
            int top_tile_top_left       = top_left - VerticesSize;
            int top_tile_top_right      = top_tile_top_left + 1;
            int top_tile_bottom_left    = top_tile_top_left + VerticesSize;
            int top_tile_bottom_right   = top_tile_top_left + VerticesSize + 1;

            CurrentVerticesPositions.Add(new Vector3(
                CurrentVerticesPositions[top_tile_bottom_left].x,
                0,
                CurrentVerticesPositions[top_tile_bottom_left].z)
            );
            CurrentVerticesPositions.Add(new Vector3(
                CurrentVerticesPositions[top_tile_bottom_right].x,
                0,
                CurrentVerticesPositions[top_tile_bottom_right].z)
            );

            NewVerticesPositions.Add(new Vector3(
                CurrentVerticesPositions[top_tile_bottom_left].x,
                HeightMap[i - 1, j],
                CurrentVerticesPositions[top_tile_bottom_left].z)
            );
            NewVerticesPositions.Add(new Vector3(
                CurrentVerticesPositions[top_tile_bottom_right].x,
                HeightMap[i - 1, j],
                CurrentVerticesPositions[top_tile_bottom_right].z)
            );

            NewVerticesPositions[top_left] = new Vector3(
                CurrentVerticesPositions[top_left].x,
                HeightMap[i, j],
                CurrentVerticesPositions[top_left].z
            );
            NewVerticesPositions[top_right] = new Vector3(
                CurrentVerticesPositions[top_right].x,
                HeightMap[i, j],
                CurrentVerticesPositions[top_right].z
            );

            int top_tile_bottom_left_new_idx = CurrentVerticesPositions.Count - 2;
            int top_tile_bottom_right_new_idx = CurrentVerticesPositions.Count - 1;

            CurrentTriangles.Add(top_tile_bottom_left_new_idx);
            CurrentTriangles.Add(top_right);
            CurrentTriangles.Add(top_left);

            CurrentTriangles.Add(top_tile_bottom_left_new_idx);
            CurrentTriangles.Add(top_tile_bottom_right_new_idx);
            CurrentTriangles.Add(top_right);


           /* CurrentTriangles[((top_tile_top_left - i + 1) * 6)] = (CurrentTriangles[((top_tile_top_left - i + 1) * 6)] >= VerticesSize * VerticesSize) ?
                CurrentTriangles[((top_tile_top_left - i + 1) * 6)] : top_tile_top_left;*/
            CurrentTriangles[((top_tile_top_left - i + 1) * 6) + 1] = top_tile_bottom_right_new_idx; //
            CurrentTriangles[((top_tile_top_left - i + 1) * 6) + 2] = top_tile_bottom_left_new_idx; //

            /*CurrentTriangles[((top_tile_top_left - i + 1) * 6) + 3] = (CurrentTriangles[((top_tile_top_left - i + 1) * 6) + 3] >= VerticesSize * VerticesSize) ?
                CurrentTriangles[((top_tile_top_left - i + 1) * 6) + 3] : top_tile_top_left;
            CurrentTriangles[((top_tile_top_left - i + 1) * 6) + 4] = (CurrentTriangles[((top_tile_top_left - i + 1) * 6) + 4] >= VerticesSize * VerticesSize) ?
                CurrentTriangles[((top_tile_top_left - i + 1) * 6) + 4] : top_tile_top_right;*/
            CurrentTriangles[((top_tile_top_left - i + 1) * 6) + 5] = top_tile_bottom_right_new_idx; //
        }
        if (i < ArenaSize - 1 && HeightMap[i + 1, j] > HeightMap[i, j])
        {
            int bottom_tile_top_left        = top_left + VerticesSize;
            int bottom_tile_top_right       = bottom_tile_top_left + 1;
            int bottom_tile_bottom_left     = bottom_tile_top_left + VerticesSize;
            int bottom_tile_bottom_right    = bottom_tile_top_left + VerticesSize + 1;

            CurrentVerticesPositions.Add(new Vector3(
                CurrentVerticesPositions[bottom_tile_top_left].x,
                0,
                CurrentVerticesPositions[bottom_tile_top_left].z)
            );
            CurrentVerticesPositions.Add(new Vector3(
                CurrentVerticesPositions[bottom_tile_top_right].x,
                0,
                CurrentVerticesPositions[bottom_tile_top_right].z)
            );

            NewVerticesPositions.Add(new Vector3(
                CurrentVerticesPositions[bottom_tile_top_left].x,
                HeightMap[i + 1, j],
                CurrentVerticesPositions[bottom_tile_top_left].z)
            );
            NewVerticesPositions.Add(new Vector3(
                CurrentVerticesPositions[bottom_tile_top_right].x,
                HeightMap[i + 1, j],
                CurrentVerticesPositions[bottom_tile_top_right].z)
            );

            NewVerticesPositions[bottom_left] = new Vector3(
                CurrentVerticesPositions[bottom_left].x,
                HeightMap[i, j],
                CurrentVerticesPositions[bottom_left].z
            );
            NewVerticesPositions[bottom_right] = new Vector3(
                CurrentVerticesPositions[bottom_right].x,
                HeightMap[i, j],
                CurrentVerticesPositions[bottom_right].z
            );

            int bottom_tile_top_left_new_idx = CurrentVerticesPositions.Count - 2;
            int bottom_tile_top_right_new_idx = CurrentVerticesPositions.Count - 1;

            CurrentTriangles.Add(bottom_left);
            CurrentTriangles.Add(bottom_tile_top_right_new_idx);
            CurrentTriangles.Add(bottom_tile_top_left_new_idx);

            CurrentTriangles.Add(bottom_left);
            CurrentTriangles.Add(bottom_right);
            CurrentTriangles.Add(bottom_tile_top_right_new_idx);


            CurrentTriangles[((bottom_tile_top_left - i - 1) * 6)] = bottom_tile_top_left_new_idx;

            CurrentTriangles[((bottom_tile_top_left - i - 1) * 6) + 3] = bottom_tile_top_left_new_idx;
            CurrentTriangles[((bottom_tile_top_left - i - 1) * 6) + 4] = bottom_tile_top_right_new_idx;
        }
        if (j > 0 && HeightMap[i, j - 1] > HeightMap[i, j])
        {
            int left_tile_top_left      = top_left - 1;
            int left_tile_top_right     = left_tile_top_left + 1;
            int left_tile_bottom_left   = left_tile_top_left + VerticesSize;
            int left_tile_bottom_right  = left_tile_top_left + VerticesSize + 1;

            CurrentVerticesPositions.Add(new Vector3(
                CurrentVerticesPositions[left_tile_top_right].x,
                0,
                CurrentVerticesPositions[left_tile_top_right].z)
            );
            CurrentVerticesPositions.Add(new Vector3(
                CurrentVerticesPositions[left_tile_bottom_right].x,
                0,
                CurrentVerticesPositions[left_tile_bottom_right].z)
            );

            NewVerticesPositions.Add(new Vector3(
                CurrentVerticesPositions[left_tile_top_right].x,
                HeightMap[i, j - 1],
                CurrentVerticesPositions[left_tile_top_right].z)
            );
            NewVerticesPositions.Add(new Vector3(
                CurrentVerticesPositions[left_tile_bottom_right].x,
                HeightMap[i, j - 1],
                CurrentVerticesPositions[left_tile_bottom_right].z)
            );

            NewVerticesPositions[top_left] = new Vector3(
                CurrentVerticesPositions[top_left].x,
                HeightMap[i, j],
                CurrentVerticesPositions[top_left].z
            );
            NewVerticesPositions[bottom_left] = new Vector3(
                CurrentVerticesPositions[bottom_left].x,
                HeightMap[i, j],
                CurrentVerticesPositions[bottom_left].z
            );

            int left_tile_top_right_new_idx = CurrentVerticesPositions.Count - 2;
            int left_tile_bottom_right_new_idx = CurrentVerticesPositions.Count - 1;

            CurrentTriangles.Add(left_tile_bottom_right_new_idx);
            CurrentTriangles.Add(top_left);
            CurrentTriangles.Add(bottom_left);

            CurrentTriangles.Add(left_tile_bottom_right_new_idx);
            CurrentTriangles.Add(left_tile_top_right_new_idx);
            CurrentTriangles.Add(top_left);


            CurrentTriangles[((left_tile_top_left - i) * 6) + 1] = left_tile_bottom_right_new_idx;

            CurrentTriangles[((left_tile_top_left - i) * 6) + 4] = left_tile_top_right_new_idx;
            CurrentTriangles[((left_tile_top_left - i) * 6) + 5] = left_tile_bottom_right_new_idx;
        }
        if (j < ArenaSize - 1 && HeightMap[i, j + 1] > HeightMap[i, j])
        {
            int right_tile_top_left     = top_left + 1;
            int right_tile_top_right    = right_tile_top_left + 1;
            int right_tile_bottom_left  = right_tile_top_left + VerticesSize;
            int right_tile_bottom_right = right_tile_top_left + VerticesSize + 1;

            CurrentVerticesPositions.Add(new Vector3(
                CurrentVerticesPositions[right_tile_top_left].x,
                0,
                CurrentVerticesPositions[right_tile_top_left].z)
            );
            CurrentVerticesPositions.Add(new Vector3(
                CurrentVerticesPositions[right_tile_bottom_left].x,
                0,
                CurrentVerticesPositions[right_tile_bottom_left].z)
            );

            NewVerticesPositions.Add(new Vector3(
                CurrentVerticesPositions[right_tile_top_left].x,
                HeightMap[i, j + 1],
                CurrentVerticesPositions[right_tile_top_left].z)
            );
            NewVerticesPositions.Add(new Vector3(
                CurrentVerticesPositions[right_tile_bottom_left].x,
                HeightMap[i, j + 1],
                CurrentVerticesPositions[right_tile_bottom_left].z)
            );

            NewVerticesPositions[top_right] = new Vector3(
                CurrentVerticesPositions[top_right].x,
                HeightMap[i, j],
                CurrentVerticesPositions[top_right].z
            );
            NewVerticesPositions[bottom_right] = new Vector3(
                CurrentVerticesPositions[bottom_right].x,
                HeightMap[i, j],
                CurrentVerticesPositions[bottom_right].z
            );

            int right_tile_top_left_new_idx = CurrentVerticesPositions.Count - 2;
            int right_tile_bottom_left_new_idx = CurrentVerticesPositions.Count - 1;

            CurrentTriangles.Add(right_tile_top_left_new_idx);
            CurrentTriangles.Add(bottom_right);
            CurrentTriangles.Add(top_right);

            CurrentTriangles.Add(right_tile_top_left_new_idx);
            CurrentTriangles.Add(right_tile_bottom_left_new_idx);
            CurrentTriangles.Add(bottom_right);


            CurrentTriangles[((right_tile_top_left - i) * 6)] = right_tile_top_left_new_idx;
            CurrentTriangles[((right_tile_top_left - i) * 6) + 2] = right_tile_bottom_left_new_idx;

            CurrentTriangles[((right_tile_top_left - i) * 6) + 3] = right_tile_top_left_new_idx;
        }
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
}