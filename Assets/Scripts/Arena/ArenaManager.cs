using Assets.Scripts.Gameplay;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using static UnityEditor.Searcher.SearcherWindow.Alignment;

public class ArenaManager : MonoBehaviour
{
    [Header("Objects")]
    GameObject Arena;
    GameController GameController;
    public GameObject Player;
    public GameObject Quad;

    [Header("Arena Parameters")]
    public int[,] HeightMap;
    public int[,] StairsMap;
    List<List<int[,]>> ArenaPresets;
    GameObject ArenaMesh;
    Vector3[] NewVerticesPositions;

    [Header("BuffBox Parameters")]
    public GameObject BuffBoxGO;

    [Header("Other Parameters")]
    public float DefaultChangeSpeed = 1;
    public float ChangeSpeedRatio = 0.01f;
    float ChangeSpeed;
    short flag = 0;
    const int ArenaSize = 20;
    const int ChunkScale = 4;
    const int KillHeight = -20;


    void Start()
    {
        CreateArena();
        CreatePresets();

        GameController = gameObject.GetComponent<GameController>();

        HeightMap = (int[,])ArenaPresets[0][0].Clone();
        StairsMap = (int[,])ArenaPresets[0][1].Clone();

        NewVerticesPositions = new Vector3[(ArenaSize + 1) * (ArenaSize + 1)];
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
                vertices[i + j * (ArenaSize + 1)] = new Vector3(i * ChunkScale, 0, j * ChunkScale);

        for (int z = 0, i = 0; z < ArenaSize + 1; z++)
            for (int x = 0; x < ArenaSize + 1; x++, i++)
            {
                float randomOffsetX = Random.Range(-0.3f, 0.3f);
                float randomOffsetY = Random.Range(-0.3f, 0.3f);

                uv[i] = new Vector2(
                    (x + randomOffsetX) * 0.25f,
                    (z + randomOffsetY) * 0.25f);
            }

        for (int ti = 0, vi = 0, z = 0; z < ArenaSize; z++, vi++)
            for (int x = 0; x < ArenaSize; x++, ti += 6, vi++)
            {
                triangles[ti] = vi;
                triangles[ti + 1] = vi + ArenaSize + 1;
                triangles[ti + 2] = vi + ArenaSize + 2;

                triangles[ti + 3] = vi;
                triangles[ti + 4] = vi + ArenaSize + 2;
                triangles[ti + 5] = vi + 1;
            }

        arenaMesh.vertices = vertices;
        arenaMesh.uv = uv;
        arenaMesh.triangles = triangles;

        arenaMesh.RecalculateBounds();
        arenaMesh.RecalculateNormals();

        Arena.GetComponent<MeshCollider>().sharedMesh = arenaMesh;
        Arena.GetComponent<MeshFilter>().mesh = arenaMesh;
        Arena.GetComponent<MeshRenderer>().material = Resources.Load("xen", typeof(Material)) as Material;
        Arena.GetComponent<MeshRenderer>().material.mainTextureScale = new Vector2(4, 4);
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
            { 5, 5, 5, 5,   5, 5, 5, 5,   5, 5, 5, 5,   5, 5, 5, 5,   5, 5, 5, 5},
            { 5, 5, 5, 5,   5, 5, 5, 5,   5, 5, 5, 5,   5, 5, 5, 5,   5, 5, 5, 5},
            { 5, 5, 5, 5,   5, 5, 5, 5,   5, 5, 5, 5,   5, 5, 5, 5,   5, 5, 5, 5},
            { 5, 5, 5, 5,   5, 5, 5, 5,   5, 5, 5, 5,   5, 5, 5, 5,   5, 5, 5, 5},

            { 5, 5, 5, 5,   9, 7, 7, 7,   5, 5, 5, 5,   7, 7, 7, 9,   5, 5, 5, 5},
            { 5, 5, 5, 5,   7, 5, 5, 5,   5, 5, 5, 5,   5, 5, 5, 7,   5, 5, 5, 5},
            { 5, 5, 5, 5,   7, 5, 5, 5,   5, 5, 5, 5,   5, 5, 5, 7,   5, 5, 5, 5},
            { 5, 5, 5, 5,   7, 5, 5, 5,   5, 5, 5, 5,   5, 5, 5, 7,   5, 5, 5, 5},

            { 5, 5, 5, 5,   5, 5, 5, 5,   5, 5, 5, 5,   5, 5, 5, 5,   5, 5, 5, 5},
            { 5, 5, 5, 5,   5, 5, 5, 5,   5, 5, 5, 5,   5, 5, 5, 5,   5, 5, 5, 5},
            { 5, 5, 5, 5,   5, 5, 5, 5,   5, 5, 5, 5,   5, 5, 5, 5,   5, 5, 5, 5},
            { 5, 5, 5, 5,   5, 5, 5, 5,   5, 5, 5, 5,   5, 5, 5, 5,   5, 5, 5, 5},

            { 5, 5, 5, 5,   7, 5, 5, 5,   5, 5, 5, 5,   5, 5, 5, 7,   5, 5, 5, 5},
            { 5, 5, 5, 5,   7, 5, 5, 5,   5, 5, 5, 5,   5, 5, 5, 7,   5, 5, 5, 5},
            { 5, 5, 5, 5,   7, 5, 5, 5,   5, 5, 5, 5,   5, 5, 5, 7,   5, 5, 5, 5},
            { 5, 5, 5, 5,   9, 7, 7, 7,   5, 5, 5, 5,   7, 7, 7, 9,   5, 5, 5, 5},

            { 5, 5, 5, 5,   5, 5, 5, 5,   5, 5, 5, 5,   5, 5, 5, 5,   5, 5, 5, 5},
            { 5, 5, 5, 5,   5, 5, 5, 5,   5, 5, 5, 5,   5, 5, 5, 5,   5, 5, 5, 5},
            { 5, 5, 5, 5,   5, 5, 5, 5,   5, 5, 5, 5,   5, 5, 5, 5,   5, 5, 5, 5},
            { 5, 5, 5, 5,   5, 5, 5, 5,   5, 5, 5, 5,   5, 5, 5, 5,   5, 5, 5, 5},
        };
        int[,] pillarsStairsMap = new int[ArenaSize, ArenaSize];
        for (int i = 0; i < ArenaSize; i++)
            for (int j = 0; j < ArenaSize; j++)
                pillarsStairsMap[i, j] = 0;

        ArenaPresets.Add(new List<int[,]> { flatArenaHeightMap, flatArenaStairsMap });
        ArenaPresets.Add(new List<int[,]> { pillarsHeightMap, pillarsStairsMap });
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
            int coin = UnityEngine.Random.Range(0, 2);
            if (coin == 0)
                height = 0;
            else
            {
                int isNegative = UnityEngine.Random.Range(0, 2);
                height = UnityEngine.Random.Range(0, 4);

                if (height == 3)
                    height = 4;

                if (isNegative == 1)
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
        // 5 - выпукла€ вправо-вверх
        // 6 - выпукла€ вправо-вниз
        // 7 - выпукла€ влево-вниз
        // 8 - выпукла€ влево-вверх
        // 9 - вогнута€ вправо-вверх
        // 10 - вогнута€ вправо-вниз
        // 11 - вогнута€ влево-вниз
        // 12 - вогнута€ влево-вверх

        int up, right, down, left;
        int up_right, down_right, down_left, up_left;
        int current;

        // -=-=-=-=-–асстановка угловых лестниц на главной диагонали-=-=-=-=-
        // ѕроверка лестин на углах
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
        // ќставшиес€
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

        // -=-=-=-=-–асстановка угловых лестниц на побочной диагонали-=-=-=-=-
        // ѕроверка лестин на углах
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
        // ќставшиес€
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

        // -=-=-=-=-–асстановка фронтальных лестниц-=-=-=-=-
        // ѕроверка лестин на кра€х
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
        // ќставшиес€
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

        for (int i = 0; i < ArenaSize; i++)
        {
            for (int j = 0; j < ArenaSize; j++)
            {
                int vertex_idx = i + j * (ArenaSize + 1);
                Vector3 position = new Vector3(
                    vertices[vertex_idx].x,
                    ArenaPresets[0][0][i, j],
                    vertices[vertex_idx].z);

                vertices[vertex_idx] = Vector3.Lerp(vertices[vertex_idx], position, ChangeSpeed * Time.deltaTime);

                if (Mathf.Abs(vertices[vertex_idx].y - position.y) > 0.01)
                    isFullTransformedToFlat = false;
            }
        }


        arenaMesh.vertices = vertices;
        arenaMesh.RecalculateBounds();
        Arena.GetComponent<MeshCollider>().sharedMesh = arenaMesh;

        ChangeSpeed += ChangeSpeedRatio;
        return isFullTransformedToFlat;
    }

    void RotateTriangles()
    {
        int[] triangles = new int[ArenaSize * ArenaSize * 6];

        for (int ti = 0, vi = 0, z = 0; z < ArenaSize; z++, vi++)
            for (int x = 0; x < ArenaSize; x++, ti += 6, vi++)
            {
                if (StairsMap[x, z] > 4 && StairsMap[x, z] % 2 == 1)
                {
                    triangles[ti] = vi;
                    triangles[ti + 1] = vi + ArenaSize + 1;
                    triangles[ti + 2] = vi + 1;

                    triangles[ti + 3] = vi + 1;
                    triangles[ti + 4] = vi + ArenaSize + 1;
                    triangles[ti + 5] = vi + ArenaSize + 2;
                }
                else
                {
                    triangles[ti] = vi;
                    triangles[ti + 1] = vi + ArenaSize + 1;
                    triangles[ti + 2] = vi + ArenaSize + 2;

                    triangles[ti + 3] = vi;
                    triangles[ti + 4] = vi + ArenaSize + 2;
                    triangles[ti + 5] = vi + 1;
                }
            }

        Arena.GetComponent<MeshFilter>().mesh.triangles = triangles;
        Arena.GetComponent<MeshFilter>().mesh.RecalculateBounds();
        Arena.GetComponent<MeshFilter>().mesh.RecalculateNormals();
    }

    void CalculateVerticesPositions()
    {
        NewVerticesPositions = Arena.GetComponent<MeshFilter>().mesh.vertices;

        for (int i = 0; i < ArenaSize; i++)
        {
            for (int j = 0; j < ArenaSize; j++)
            {
                // 0 - лестницы нет
                // 1 - лестница вверх
                // 2 - лестница вправо
                // 3 - лестница вниз
                // 4 - лестница влево
                // 5 - выпукла€ вправо-вверх
                // 6 - выпукла€ вправо-вниз
                // 7 - выпукла€ влево-вниз
                // 8 - выпукла€ влево-вверх
                // 9 - вогнута€ вправо-вверх
                // 10 - вогнута€ вправо-вниз
                // 11 - вогнута€ влево-вниз
                // 12 - вогнута€ влево-вверх

                /*switch(StairsMap[i, j])
                {
                    case 1:
                        NewVerticesPositions[i, j][0].z = HeightMap[i - 1, j];
                        NewVerticesPositions[i, j][1].z = HeightMap[i, j];
                        NewVerticesPositions[i, j][2].z = HeightMap[i - 1, j];
                        NewVerticesPositions[i, j][3].z = HeightMap[i, j];
                        break;
                    case 2:
                        NewVerticesPositions[i, j][0].z = HeightMap[i, j];
                        NewVerticesPositions[i, j][1].z = HeightMap[i, j];
                        NewVerticesPositions[i, j][2].z = HeightMap[i, j + 1];
                        NewVerticesPositions[i, j][3].z = HeightMap[i, j + 1];
                        break;
                    case 3:
                        NewVerticesPositions[i, j][0].z = HeightMap[i, j];
                        NewVerticesPositions[i, j][1].z = HeightMap[i + 1, j];
                        NewVerticesPositions[i, j][2].z = HeightMap[i, j];
                        NewVerticesPositions[i, j][3].z = HeightMap[i + 1, j];
                        break;
                    case 4:
                        NewVerticesPositions[i, j][0].z = HeightMap[i, j - 1];
                        NewVerticesPositions[i, j][1].z = HeightMap[i, j - 1];
                        NewVerticesPositions[i, j][2].z = HeightMap[i, j];
                        NewVerticesPositions[i, j][3].z = HeightMap[i, j];
                        break;
                    case 5:
                        NewVerticesPositions[i, j][0].z = HeightMap[i - 1, j + 1];
                        NewVerticesPositions[i, j][1].z = HeightMap[i, j];
                        NewVerticesPositions[i, j][2].z = HeightMap[i, j];
                        NewVerticesPositions[i, j][3].z = HeightMap[i, j];
                        break;
                    case 6:
                        NewVerticesPositions[i, j][0].z = HeightMap[i, j];
                        NewVerticesPositions[i, j][1].z = HeightMap[i, j];
                        NewVerticesPositions[i, j][2].z = HeightMap[i, j];
                        NewVerticesPositions[i, j][3].z = HeightMap[i + 1, j + 1];
                        break;
                    case 7:
                        NewVerticesPositions[i, j][0].z = HeightMap[i, j];
                        NewVerticesPositions[i, j][1].z = HeightMap[i, j];
                        NewVerticesPositions[i, j][2].z = HeightMap[i, j];
                        NewVerticesPositions[i, j][3].z = HeightMap[i + 1, j - 1];
                        break;
                    case 8:
                        NewVerticesPositions[i, j][0].z = HeightMap[i - 1, j - 1];
                        NewVerticesPositions[i, j][1].z = HeightMap[i, j];
                        NewVerticesPositions[i, j][2].z = HeightMap[i, j];
                        NewVerticesPositions[i, j][3].z = HeightMap[i, j];
                        break;
                    case 9:
                        NewVerticesPositions[i, j][0].z = HeightMap[i - 1, j + 1];
                        NewVerticesPositions[i, j][1].z = HeightMap[i - 1, j + 1];
                        NewVerticesPositions[i, j][2].z = HeightMap[i - 1, j + 1];
                        NewVerticesPositions[i, j][3].z = HeightMap[i, j];
                        break;
                    case 10:
                        NewVerticesPositions[i, j][0].z = HeightMap[i, j];
                        NewVerticesPositions[i, j][1].z = HeightMap[i + 1, j + 1];
                        NewVerticesPositions[i, j][2].z = HeightMap[i + 1, j + 1];
                        NewVerticesPositions[i, j][3].z = HeightMap[i + 1, j + 1];
                        break;
                    case 11:
                        NewVerticesPositions[i, j][0].z = HeightMap[i, j];
                        NewVerticesPositions[i, j][1].z = HeightMap[i + 1, j - 1];
                        NewVerticesPositions[i, j][2].z = HeightMap[i + 1, j - 1];
                        NewVerticesPositions[i, j][3].z = HeightMap[i + 1, j - 1];
                        break;
                    case 12:
                        NewVerticesPositions[i, j][0].z = HeightMap[i - 1, j - 1];
                        NewVerticesPositions[i, j][1].z = HeightMap[i - 1, j - 1];
                        NewVerticesPositions[i, j][2].z = HeightMap[i - 1, j - 1];
                        NewVerticesPositions[i, j][3].z = HeightMap[i, j];
                        break;
                    default:
                        for (int v = 0; v < 4; v++)
                            NewVerticesPositions[i, j][v].z = HeightMap[i, j];
                        break;
                }

                for (int v = 0; v < 4; v++)
                    NewVerticesPositions[i, j][v].z = -NewVerticesPositions[i, j][v].z;*/

                int vertex_idx = i + j * (ArenaSize + 1);

                int top_left = vertex_idx;
                int top_right = vertex_idx + 1;
                int bottom_left = vertex_idx + ArenaSize + 1;
                int bottom_right = vertex_idx + ArenaSize + 2;

                switch (StairsMap[i, j])
                {
                    case 1:
                        NewVerticesPositions[top_left].y        = HeightMap[i - 1, j];
                        NewVerticesPositions[top_right].y       = HeightMap[i - 1, j];
                        NewVerticesPositions[bottom_left].y     = HeightMap[i, j];
                        NewVerticesPositions[bottom_right].y    = HeightMap[i, j];
                        break;
                    case 2:
                        NewVerticesPositions[top_left].y        = HeightMap[i, j];
                        NewVerticesPositions[top_right].y       = HeightMap[i, j + 1];
                        NewVerticesPositions[bottom_left].y     = HeightMap[i, j + 1];
                        NewVerticesPositions[bottom_right].y    = HeightMap[i, j];
                        break;
                    case 3:
                        NewVerticesPositions[top_left].y        = HeightMap[i, j];
                        NewVerticesPositions[top_right].y       = HeightMap[i, j];
                        NewVerticesPositions[bottom_left].y     = HeightMap[i + 1, j];
                        NewVerticesPositions[bottom_right].y    = HeightMap[i + 1, j];
                        break;
                    case 4:
                        NewVerticesPositions[top_left].y        = HeightMap[i, j - 1];
                        NewVerticesPositions[top_right].y       = HeightMap[i, j];
                        NewVerticesPositions[bottom_left].y     = HeightMap[i, j - 1];
                        NewVerticesPositions[bottom_right].y    = HeightMap[i, j];
                        break;
                    case 5:
                        NewVerticesPositions[top_left].y        = HeightMap[i, j];
                        NewVerticesPositions[top_right].y       = HeightMap[i - 1, j + 1];
                        NewVerticesPositions[bottom_left].y     = HeightMap[i, j];
                        NewVerticesPositions[bottom_right].y    = HeightMap[i, j];
                        break;
                    case 6:
                        NewVerticesPositions[top_left].y        = HeightMap[i, j];
                        NewVerticesPositions[top_right].y       = HeightMap[i, j];
                        NewVerticesPositions[bottom_left].y     = HeightMap[i, j];
                        NewVerticesPositions[bottom_right].y    = HeightMap[i + 1, j + 1];
                        break;
                    case 7:
                        NewVerticesPositions[top_left].y        = HeightMap[i, j];
                        NewVerticesPositions[top_right].y       = HeightMap[i, j];
                        NewVerticesPositions[bottom_left].y     = HeightMap[i + 1, j - 1];
                        NewVerticesPositions[bottom_right].y    = HeightMap[i, j];
                        break;
                    case 8:
                        NewVerticesPositions[top_left].y        = HeightMap[i - 1, j - 1];
                        NewVerticesPositions[top_right].y       = HeightMap[i, j];
                        NewVerticesPositions[bottom_left].y     = HeightMap[i, j];
                        NewVerticesPositions[bottom_right].y    = HeightMap[i, j];
                        break;
                    case 9:
                        NewVerticesPositions[top_left].y        = HeightMap[i - 1, j + 1];
                        NewVerticesPositions[top_right].y       = HeightMap[i - 1, j + 1];
                        NewVerticesPositions[bottom_left].y     = HeightMap[i, j];
                        NewVerticesPositions[bottom_right].y    = HeightMap[i - 1, j + 1];
                        break;
                    case 10:
                        NewVerticesPositions[top_left].y        = HeightMap[i, j]; //
                        NewVerticesPositions[top_right].y       = HeightMap[i + 1, j + 1];
                        NewVerticesPositions[bottom_left].y     = HeightMap[i + 1, j + 1];
                        NewVerticesPositions[bottom_right].y    = HeightMap[i + 1, j + 1];
                        break;
                    case 11:
                        NewVerticesPositions[top_left].y        = HeightMap[i + 1, j - 1];
                        NewVerticesPositions[top_right].y       = HeightMap[i, j]; //
                        NewVerticesPositions[bottom_left].y     = HeightMap[i + 1, j - 1];
                        NewVerticesPositions[bottom_right].y    = HeightMap[i + 1, j - 1];
                        break;
                    case 12:
                        NewVerticesPositions[top_left].y        = HeightMap[i - 1, j - 1];
                        NewVerticesPositions[top_right].y       = HeightMap[i - 1, j - 1];
                        NewVerticesPositions[bottom_left].y     = HeightMap[i - 1, j - 1];
                        NewVerticesPositions[bottom_right].y    = HeightMap[i, j]; //
                        break;
                    default:
                        for (int v = 0; v < 4; v++)
                            NewVerticesPositions[vertex_idx + v].y = HeightMap[i, j];
                        break;
                }

                /*for (int v = 0; v < 4; v++)
                    NewVerticesPositions[vertex_idx + v].y = -NewVerticesPositions[vertex_idx + v].y;*/
            }
        }
    }

    bool SmoothTransformToTarget()
    {
        Mesh arenaMesh = Arena.GetComponent<MeshFilter>().mesh;
        Vector3[] vertices = arenaMesh.vertices;
        bool isFullTransformedToTarget = true;

        for (int i = 0; i < ArenaSize; i++)
        {
            for (int j = 0; j < ArenaSize; j++)
            {
                int vertex_idx = i + j * (ArenaSize + 1);

                vertices[vertex_idx] = Vector3.Lerp(vertices[vertex_idx], NewVerticesPositions[vertex_idx], ChangeSpeed * Time.deltaTime);

                if (Mathf.Abs(vertices[vertex_idx].y - NewVerticesPositions[vertex_idx].y) > 0.01)
                    isFullTransformedToTarget = false;
            }
        }


        arenaMesh.vertices = vertices;
        arenaMesh.RecalculateBounds();
        Arena.GetComponent<MeshCollider>().sharedMesh = arenaMesh;

        ChangeSpeed += ChangeSpeedRatio;
        return isFullTransformedToTarget;
    }


    public int GetArenaSize()
    {
        return ArenaSize;
    }
    public int GetChunkScale()
    {
        return ChunkScale;
    }
    public int GetKillHeight()
    {
        return KillHeight;
    }
}