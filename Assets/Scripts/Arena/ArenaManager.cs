using Assets.Scripts.Gameplay;
using System.Collections.Generic;
using UnityEngine;

public class ArenaManager : MonoBehaviour
{
    [Header("Objects")]
    GameObject Arena;
    GameController GameController;
    public GameObject Player;
    public GameObject Quad;
    public Material QuadMaterial;

    [Header("Arena Parameters")]
    public int[,] HeightMap;
    public int[,] StairsMap;
    List<List<int[,]>> ArenaPresets;
    GameObject[,] Chunks;
    Vector3[,][] NewVerticesPositions;

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


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Arena = new GameObject("Arena");
        CreatePresets();

        GameController = gameObject.GetComponent<GameController>();
        NewVerticesPositions = new Vector3[ArenaSize, ArenaSize][];

        Chunks = new GameObject[ArenaSize, ArenaSize];
        HeightMap = (int[,])ArenaPresets[0][0].Clone();
        StairsMap = (int[,])ArenaPresets[0][1].Clone();
        for (int i = 0; i < Chunks.GetLength(0); i++)
            for (int j = 0; j < Chunks.GetLength(1); j++)
            {
                Vector3 position = new Vector3(ChunkScale * i, HeightMap[i, j], ChunkScale * j);
                GameObject tile = Instantiate(Quad, position, Quaternion.Euler(90, 0, 0), Arena.transform);

                Material instanceMat = new Material(QuadMaterial);
                int rotation = Random.Range(0, 4) * 90; 
                Vector2 offset = Vector2.zero;

                switch (rotation) {
                    case 90:
                        offset = new Vector2(1, 0);
                        break;
                    case 180:
                        offset = new Vector2(1, 1);
                        break;
                    case 270:
                        offset = new Vector2(0, 1);
                        break;
                }

                instanceMat.mainTextureOffset = offset;
                instanceMat.mainTextureScale = new Vector2(rotation % 180 == 0 ? 1 : -1, 1);
                tile.GetComponent<Renderer>().material = instanceMat;

                Chunks[i, j] = tile;
                Chunks[i, j].transform.localScale = new Vector3(ChunkScale, ChunkScale, 1);


                NewVerticesPositions[i, j] = new Vector3[4];
            }

        ChangeSpeed = DefaultChangeSpeed;
    }

    // Update is called once per frame
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
                    RotateQuads();
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
        // 0 - �������� ���
        // 1 - �������� �����
        // 2 - �������� ������
        // 3 - �������� ����
        // 4 - �������� �����
        // 5 - �������� ������-�����
        // 6 - �������� ������-����
        // 7 - �������� �����-����
        // 8 - �������� �����-�����
        // 9 - �������� ������-�����
        // 10 - �������� ������-����
        // 11 - �������� �����-����
        // 12 - �������� �����-�����

        int up, right, down, left;
        int up_right, down_right, down_left, up_left;
        int current;

        // -=-=-=-=-����������� ������� ������� �� ������� ���������-=-=-=-=-
        // �������� ������ �� �����
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
        // ����������
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

        // -=-=-=-=-����������� ������� ������� �� �������� ���������-=-=-=-=-
        // �������� ������ �� �����
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
        // ����������
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

        // -=-=-=-=-����������� ����������� �������-=-=-=-=-
        // �������� ������ �� �����
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
        // ����������
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
        bool isFullTransformedToFlat = true;

        for (int i = 0; i < ArenaSize; i++)
        {
            for (int j = 0; j < ArenaSize; j++)
            {
                Mesh mesh = Chunks[i, j].GetComponent<MeshFilter>().mesh;
                Vector3[] vertices = mesh.vertices;

                for (int v = 0; v < vertices.Length; v++)
                {
                    Vector3 position = new Vector3(
                        vertices[v].x,
                        vertices[v].y,
                        ArenaPresets[0][0][i, j]);
                    vertices[v] = Vector3.Lerp(vertices[v], position, ChangeSpeed * Time.deltaTime);

                    if (Mathf.Abs(vertices[v].z - position.z) > 0.01)
                        isFullTransformedToFlat = false;
                }

                mesh.vertices = vertices;
                mesh.RecalculateBounds();
                Chunks[i, j].GetComponent<MeshCollider>().sharedMesh = mesh;
            }
        }

        ChangeSpeed += ChangeSpeedRatio;
        return isFullTransformedToFlat;
    }

    void RotateQuads()
    {
        for (int i = 0; i < ArenaSize; i++)
            for (int j = 0; j < ArenaSize; j++)
                if (StairsMap[i, j] > 4 && StairsMap[i, j] % 2 == 1)
                    Chunks[i, j].transform.rotation = Quaternion.Euler(90, 0, 270);
                else
                    Chunks[i, j].transform.rotation = Quaternion.Euler(90, 0, 0);
    }

    void CalculateVerticesPositions()
    {
        for (int i = 0; i < ArenaSize; i++)
            for (int j = 0; j < ArenaSize; j++)
                NewVerticesPositions[i, j] = Chunks[i, j].GetComponent<MeshFilter>().mesh.vertices;

        for (int i = 0; i < ArenaSize; i++)
        {
            for (int j = 0; j < ArenaSize; j++)
            {
                // Rotated (5, 7, 9, 11)
                // 0 - top_right
                // 1 - top_left
                // 2 - bottom_right
                // 3 - bottom_left
                // Normal (other)
                // 0 - top_left
                // 1 - bottom_left
                // 2 - top_right
                // 3 - bottom_right

                // 0 - �������� ���
                // 1 - �������� �����
                // 2 - �������� ������
                // 3 - �������� ����
                // 4 - �������� �����
                // 5 - �������� ������-�����
                // 6 - �������� ������-����
                // 7 - �������� �����-����
                // 8 - �������� �����-�����
                // 9 - �������� ������-�����
                // 10 - �������� ������-����
                // 11 - �������� �����-����
                // 12 - �������� �����-�����

                switch(StairsMap[i, j])
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
                    NewVerticesPositions[i, j][v].z = -NewVerticesPositions[i, j][v].z;
            }
        }
    }

    bool SmoothTransformToTarget()
    {
        bool isFullTransformedToTarget = true;

        for (int i = 0; i < ArenaSize; i++)
        {
            for (int j = 0; j < ArenaSize; j++)
            {
                Mesh mesh = Chunks[i, j].GetComponent<MeshFilter>().mesh;
                Vector3[] vertices = mesh.vertices;

                for (int v = 0; v < vertices.Length; v++)
                {
                    vertices[v] = Vector3.Lerp(vertices[v], NewVerticesPositions[i, j][v], ChangeSpeed * Time.deltaTime);

                    if (Mathf.Abs(vertices[v].z - NewVerticesPositions[i, j][v].z) > 0.01)
                        isFullTransformedToTarget = false;
                }

                mesh.vertices = vertices;
                mesh.RecalculateBounds();
                Chunks[i, j].GetComponent<MeshCollider>().sharedMesh = mesh;
            }
        }

        ChangeSpeed += ChangeSpeedRatio;
        return isFullTransformedToTarget;
    }
}