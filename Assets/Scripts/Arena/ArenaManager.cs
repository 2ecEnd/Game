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
    Vector3[] NewVerticesPositions;

    [Header("BuffBox Parameters")]
    public GameObject BuffBoxGO;

    [Header("Other Parameters")]
    public float DefaultChangeSpeed;
    public float ChangeSpeedRatio;
    float ChangeSpeed;
    short flag = 0;
    const int ArenaSize = 20;
    const float ChunkScale = 4f;
    const int KillHeight = -20;


    void Start()
    {
        CreateArena();
        CreatePresets();

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

                    /*print("-=-=-=-=-=-=-=-=-=-КАРТА ВЫСОТ-=-=-=-=-=-=-=-=-=-=-=-");
                    for (int i = 0; i < ArenaSize; i++)
                    {
                        string str = "";
                        for (int j = 0; j < ArenaSize; j++)
                        {
                            str += HeightMap[i, j] + " ";
                        }
                        print(str);
                    }

                    print("-=-=-=-=-=-=-=-=-=-КАРТА ЛЕСТНИЦ-=-=-=-=-=-=-=-=-=-=-=-");
                    for (int i = 0; i < ArenaSize; i++)
                    {
                        string str = "";
                        for (int j = 0; j < ArenaSize; j++)
                        {
                            str += StairsMap[i, j] + " ";
                        }
                        print(str);
                    }

                    for (int i = 0; i <= ArenaSize; i++)
                    {
                        for (int j = 0; j <= ArenaSize; j++)
                        {
                            string str = "";
                            for (int h = 0; h < 3; h++)
                                str += NewVerticesPositions[j + i * (ArenaSize + 1)][h].ToString() + " ";


                            print("[" + i.ToString() + ", " + j.ToString() + "] - " + str);
                        }
                    }*/
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

        for (int i = 0; i <= ArenaSize; i++)
            for (int j = 0; j <= ArenaSize; j++)
            {
                int vertex_idx = j + i * (ArenaSize + 1);
                Vector3 position = new Vector3(
                    vertices[vertex_idx].x,
                    0,
                    vertices[vertex_idx].z);

                vertices[vertex_idx] = Vector3.Lerp(vertices[vertex_idx], position, ChangeSpeed * Time.deltaTime);

                if (Mathf.Abs(vertices[vertex_idx].y - position.y) > 0.01)
                    isFullTransformedToFlat = false;
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

        for (int ti = 0, vi = 0, i = 0; i < ArenaSize; i++, vi++)
            for (int j = 0; j < ArenaSize; j++, vi++)
            {
                if (StairsMap[i, j] > 4 && StairsMap[i, j] % 2 == 1)
                {
                    triangles[ti++] = vi;
                    triangles[ti++] = vi + 1;
                    triangles[ti++] = vi + ArenaSize + 1;

                    triangles[ti++] = vi + 1;
                    triangles[ti++] = vi + ArenaSize + 2;
                    triangles[ti++] = vi + ArenaSize + 1;
                }
                else
                {
                    triangles[ti++] = vi;
                    triangles[ti++] = vi + ArenaSize + 2;
                    triangles[ti++] = vi + ArenaSize + 1;

                    triangles[ti++] = vi;
                    triangles[ti++] = vi + 1;
                    triangles[ti++] = vi + ArenaSize + 2;
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

                int vertex_idx = j + i * (ArenaSize + 1);

                int top_left = vertex_idx;
                int top_right = vertex_idx + 1;
                int bottom_left = vertex_idx + ArenaSize + 1;
                int bottom_right = vertex_idx + ArenaSize + 2;

                switch (StairsMap[i, j])
                {
                    case 1:
                        NewVerticesPositions[top_left].y = HeightMap[i - 1, j];
                        NewVerticesPositions[top_right].y = HeightMap[i - 1, j];
                        NewVerticesPositions[bottom_left].y = HeightMap[i, j];
                        NewVerticesPositions[bottom_right].y = HeightMap[i, j];
                        break;
                    case 2:
                        NewVerticesPositions[top_left].y = HeightMap[i, j];
                        NewVerticesPositions[top_right].y = HeightMap[i, j + 1];
                        NewVerticesPositions[bottom_left].y = HeightMap[i, j];
                        NewVerticesPositions[bottom_right].y = HeightMap[i, j + 1];
                        break;
                    case 3:
                        NewVerticesPositions[top_left].y = HeightMap[i, j];
                        NewVerticesPositions[top_right].y = HeightMap[i, j];
                        NewVerticesPositions[bottom_left].y = HeightMap[i + 1, j];
                        NewVerticesPositions[bottom_right].y = HeightMap[i + 1, j];
                        break;
                    case 4:
                        NewVerticesPositions[top_left].y = HeightMap[i, j - 1];
                        NewVerticesPositions[top_right].y = HeightMap[i, j];
                        NewVerticesPositions[bottom_left].y = HeightMap[i, j - 1];
                        NewVerticesPositions[bottom_right].y = HeightMap[i, j];
                        break;
                    case 5:
                        NewVerticesPositions[top_left].y = HeightMap[i, j];
                        NewVerticesPositions[top_right].y = HeightMap[i - 1, j + 1];
                        NewVerticesPositions[bottom_left].y = HeightMap[i, j];
                        NewVerticesPositions[bottom_right].y = HeightMap[i, j];
                        break;
                    case 6:
                        NewVerticesPositions[top_left].y = HeightMap[i, j];
                        NewVerticesPositions[top_right].y = HeightMap[i, j];
                        NewVerticesPositions[bottom_left].y = HeightMap[i, j];
                        NewVerticesPositions[bottom_right].y = HeightMap[i + 1, j + 1];
                        break;
                    case 7:
                        NewVerticesPositions[top_left].y = HeightMap[i, j];
                        NewVerticesPositions[top_right].y = HeightMap[i, j];
                        NewVerticesPositions[bottom_left].y = HeightMap[i + 1, j - 1];
                        NewVerticesPositions[bottom_right].y = HeightMap[i, j];
                        break;
                    case 8:
                        NewVerticesPositions[top_left].y = HeightMap[i - 1, j - 1];
                        NewVerticesPositions[top_right].y = HeightMap[i, j];
                        NewVerticesPositions[bottom_left].y = HeightMap[i, j];
                        NewVerticesPositions[bottom_right].y = HeightMap[i, j];
                        break;
                    case 9:
                        NewVerticesPositions[top_left].y = HeightMap[i - 1, j + 1];
                        NewVerticesPositions[top_right].y = HeightMap[i - 1, j + 1];
                        NewVerticesPositions[bottom_left].y = HeightMap[i, j];
                        NewVerticesPositions[bottom_right].y = HeightMap[i - 1, j + 1];
                        break;
                    case 10:
                        NewVerticesPositions[top_left].y = HeightMap[i, j];
                        NewVerticesPositions[top_right].y = HeightMap[i + 1, j + 1];
                        NewVerticesPositions[bottom_left].y = HeightMap[i + 1, j + 1];
                        NewVerticesPositions[bottom_right].y = HeightMap[i + 1, j + 1];
                        break;
                    case 11:
                        NewVerticesPositions[top_left].y = HeightMap[i + 1, j - 1];
                        NewVerticesPositions[top_right].y = HeightMap[i, j];
                        NewVerticesPositions[bottom_left].y = HeightMap[i + 1, j - 1];
                        NewVerticesPositions[bottom_right].y = HeightMap[i + 1, j - 1];
                        break;
                    case 12:
                        NewVerticesPositions[top_left].y = HeightMap[i - 1, j - 1];
                        NewVerticesPositions[top_right].y = HeightMap[i - 1, j - 1];
                        NewVerticesPositions[bottom_left].y = HeightMap[i - 1, j - 1];
                        NewVerticesPositions[bottom_right].y = HeightMap[i, j];
                        break;
                    default:
                        NewVerticesPositions[top_left].y = HeightMap[i, j];
                        NewVerticesPositions[top_right].y = HeightMap[i, j];
                        NewVerticesPositions[bottom_left].y = HeightMap[i, j];
                        NewVerticesPositions[bottom_right].y = HeightMap[i, j];
                        break;
                }
            }

    }

    bool SmoothTransformToTarget()
    {
        Mesh arenaMesh = Arena.GetComponent<MeshFilter>().mesh;
        Vector3[] vertices = arenaMesh.vertices;

        bool isFullTransformedToTarget = true;

        for (int i = 0; i <= ArenaSize; i++)
            for (int j = 0; j <= ArenaSize; j++)
            {
                int vertex_idx = j + i * (ArenaSize + 1);

                vertices[vertex_idx] = Vector3.Lerp(vertices[vertex_idx], NewVerticesPositions[vertex_idx], ChangeSpeed * Time.deltaTime);

                if (Mathf.Abs(vertices[vertex_idx].y - NewVerticesPositions[vertex_idx].y) > 0.01)
                    isFullTransformedToTarget = false;
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
        float arenaSize = (GetArenaSize() - 1) * GetChunkScale();
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