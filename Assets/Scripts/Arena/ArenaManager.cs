using Assets.Scripts.Gameplay;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UIElements;

public class ArenaManager : MonoBehaviour
{
    [Header("Objects")]
    private GameObject arena;
    private GameController gameController;
    public GameObject player;
    public GameObject chunk;
    public GameObject quad;
    //public GameObject stair_1;
    //public GameObject stair_05;
    //public GameObject stair_025;
    //public GameObject stair_1_concave;    // Вогнутая
    //public GameObject stair_05_concave;
    //public GameObject stair_025_concave;
    //public GameObject stair_1_convex;     // Выпуклая
    //public GameObject stair_05_convex;
    //public GameObject stair_025_convex;

    [Header("Arena Parameters")]
    public int[,] heightMap;
    public int[,] stairsMap;
    public int[,] prevStairsMap;
    private List<List<int[,]>> arenaPresets;
    private GameObject[,] chunks;
    private List<GameObject> stairs;

    [Header("BuffBox Parameters")]
    public GameObject BuffBoxGO;

    [Header("Other Parameters")]
    public float ChangeSpeed = 1;
    private int flag = 0;
    private const int arenaSize = 20;
    private const int chunkScale = 4;
    private float chunkHeight;
    private const int killHeight = -20;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        arena = new GameObject("Arena");
        CreatePresets();

        gameController = gameObject.GetComponent<GameController>();

        chunkHeight = (chunk.transform.localScale.y / 2);   // Нацало координат чанка находится в его центре
                                                            // Поэтому делим высоту пополам

        chunks = new GameObject[arenaSize, arenaSize];
        stairs = new List<GameObject>();
        heightMap = arenaPresets[0][0];
        stairsMap = arenaPresets[0][1];
        prevStairsMap = new int[arenaSize, arenaSize];
        for (int i = 0; i < chunks.GetLength(0); i++)
            for (int j = 0; j < chunks.GetLength(1); j++)
            {
                Vector3 position = new Vector3(chunkScale * i, heightMap[i, j], chunkScale * j);
                chunks[i, j] = Instantiate(quad, position, Quaternion.Euler(90, 0, 0), arena.transform);
                chunks[i, j].transform.localScale = new Vector3(chunkScale, chunkScale, 1);
            }
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (flag != 0)
        {
            ChangeArena(flag);
            flag = 0;
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
        return arenaSize;
    }
    public int GetChunkScale()
    {
        return chunkScale;
    }
    public int GetKillHeight()
    {
        return killHeight;
    }


    void CreatePresets()
    {
        arenaPresets = new List<List<int[,]>>();

        int[,] flatArenaHeightMap = new int[arenaSize, arenaSize];
        int[,] flatArenaStairsMap = new int[arenaSize, arenaSize];
        for (int i = 0; i < arenaSize; i++)
            for (int j = 0; j < arenaSize; j++)
            {
                flatArenaHeightMap[i, j] = 9;
                flatArenaStairsMap[i, j] = 0;
            }

        int[,] pillarsHeightMap = new int[arenaSize, arenaSize]
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
        int[,] pillarsStairsMap = new int[arenaSize, arenaSize];
        for (int i = 0; i < arenaSize; i++)
            for (int j = 0; j < arenaSize; j++)
                pillarsStairsMap[i, j] = 0;

        arenaPresets.Add(new List<int[,]> { flatArenaHeightMap, flatArenaStairsMap });
        arenaPresets.Add(new List<int[,]> { pillarsHeightMap, pillarsStairsMap });
    }


    void PlaceStair(int x, int z)
    {
        if (stairsMap[x, z] < 5)
            PlaceFrontStair(x, z);
        else
            PlaceCornerStair(x, z);
    }

    void PlaceFrontStair(int x, int z)
    {
        int current = heightMap[x, z];

        int difference;

        Mesh mesh = chunks[x, z].GetComponent<MeshFilter>().mesh;
        Vector3[] vertices = mesh.vertices; // 0 - top_left
                                            // 1 - bottom_left
                                            // 2 - top_right
                                            // 3 - bottom_right

        if (stairsMap[x, z] == 1)
        {
            difference = heightMap[x - 1, z] - current;

            vertices[0].z -= difference;
            vertices[2].z -= difference;
        }
        else if (stairsMap[x, z] == 2)
        {
            difference = heightMap[x, z + 1] - current;

            vertices[2].z -= difference;
            vertices[3].z -= difference;
        }
        else if (stairsMap[x, z] == 3)
        {
            difference = heightMap[x + 1, z] - current;

            vertices[1].z -= difference;
            vertices[3].z -= difference;
        }
        else //if (stairsMap[x, z] == 4)
        {
            difference = heightMap[x, z - 1] - current;

            vertices[0].z -= difference;
            vertices[1].z -= difference;
        }

        mesh.vertices = vertices;
        mesh.RecalculateBounds();
        chunks[x, z].GetComponent<MeshCollider>().sharedMesh = mesh;
    }

    void PlaceCornerStair(int x, int z)
    {
        int current = heightMap[x, z];

        int difference;

        Mesh mesh = chunks[x, z].GetComponent<MeshFilter>().mesh;
        Vector3[] vertices = mesh.vertices; // 0 - top_left
                                            // 1 - bottom_left
                                            // 2 - top_right
                                            // 3 - bottom_right

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

        if (stairsMap[x, z] == 5)
        {
            difference = heightMap[x - 1, z + 1] - current;

            chunks[x, z].transform.rotation = Quaternion.Euler(90, 0, 270);
            vertices[0].z -= difference;
        }
        else if (stairsMap[x, z] == 6)
        {
            difference = heightMap[x + 1, z + 1] - current;

            vertices[3].z -= difference;
        }
        else if (stairsMap[x, z] == 7)
        {
            difference = heightMap[x + 1, z - 1] - current;

            chunks[x, z].transform.rotation = Quaternion.Euler(90, 0, 270);
            vertices[3].z -= difference;
        }
        else if (stairsMap[x, z] == 8)
        {
            difference = heightMap[x - 1, z - 1] - current;

            vertices[0].z -= difference;
        }
        else if (stairsMap[x, z] == 9)
        {
            difference = heightMap[x - 1, z + 1] - current;

            chunks[x, z].transform.rotation = Quaternion.Euler(90, 0, 270);
            vertices[0].z -= difference;
            vertices[1].z -= difference;
            vertices[2].z -= difference;
        }
        else if(stairsMap[x, z] == 10)
        {
            difference = heightMap[x + 1, z + 1] - current;

            vertices[1].z -= difference;
            vertices[2].z -= difference;
            vertices[3].z -= difference;
        }
        else if (stairsMap[x, z] == 11)
        {
            difference = heightMap[x + 1, z - 1] - current;

            chunks[x, z].transform.rotation = Quaternion.Euler(90, 0, 270);
            vertices[1].z -= difference;
            vertices[2].z -= difference;
            vertices[3].z -= difference;
        }
        else //if (stairsMap[x, z] == 12)
        {
            difference = heightMap[x - 1, z - 1] - current;

            vertices[0].z -= difference;
            vertices[1].z -= difference;
            vertices[2].z -= difference;
        }

        mesh.vertices = vertices;
        mesh.RecalculateBounds();
        chunks[x, z].GetComponent<MeshCollider>().sharedMesh = mesh;
    }

    /*void RemoveStairs()
    {
        while (stairs.Count > 0)
        {
            Destroy(stairs[0]);
            stairs.RemoveAt(0);
        }
    }*/


    void ChangeArena(int flag)
    {
        //float coin = UnityEngine.Random.value;
        if (flag == 1)
        {
            CreatePresets(); // TODO: need to fix
            ChooseFromPresets();
        }
        else if (flag == 2)
            GenerateCircleArena();
        //if (coin < 0.7)
        //    generateCircleArena();
        //else
        //    chooseFromPresets();

        TransformArena();
    }

    void ChooseFromPresets()
    {
        int choice = UnityEngine.Random.Range(0, arenaPresets.Count);
        heightMap = arenaPresets[choice][0];
        stairsMap = arenaPresets[choice][1];
    }

    void GenerateCircleArena()
    {
        GenerateHeightMap();
        GenerateStairsMap();
    }

    void GenerateHeightMap()
    {
        int size = arenaSize / 2;
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
            for (int j = i; j < arenaSize - i; j++)
            {
                heightMap[i, j] = tmpHeightMap[i];
                heightMap[arenaSize - i - 1, j] = tmpHeightMap[i];
                heightMap[j, i] = tmpHeightMap[i];
                heightMap[j, arenaSize - i - 1] = tmpHeightMap[i];
            }
        }
    }

    void GenerateStairsMap()
    {
        for (int i = 0; i < arenaSize; i++)
            for (int j = 0; j < arenaSize; j++)
                prevStairsMap[i, j] = stairsMap[i, j];

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
        // Проверка лестин на углах
        if (heightMap[1, 1] > heightMap[0, 0])
        {
            stairsMap[0, 0] = 6;
            stairsMap[arenaSize - 1, arenaSize - 1] = 8;
        }
        else
        {
            stairsMap[0, 0] = 0;
            stairsMap[arenaSize - 1, arenaSize - 1] = 0;
        }
        // Оставшиеся
        for (int i = 1; i < arenaSize / 2; i++)
        {
            up_left = heightMap[i - 1, i - 1];
            current = heightMap[i, i];
            down_right = heightMap[i + 1, i + 1];

            if (up_left > current)
            {
                stairsMap[i, i] = 12;
                stairsMap[arenaSize - i - 1, arenaSize - i - 1] = 10;
            }
            else if (current < down_right)
            {
                stairsMap[i, i] = 6;
                stairsMap[arenaSize - i - 1, arenaSize - i - 1] = 8;
            }
            else
            {
                stairsMap[i, i] = 0;
                stairsMap[arenaSize - i - 1, arenaSize - i - 1] = 0;
            }
        }

        // -=-=-=-=-Расстановка угловых лестниц на побочной диагонали-=-=-=-=-
        // Проверка лестин на углах
        if (heightMap[1, arenaSize - 2] > heightMap[0, arenaSize - 1])
        {
            stairsMap[0, arenaSize - 1] = 7;
            stairsMap[arenaSize - 1, 0] = 5;
        }
        else
        {
            stairsMap[0, arenaSize - 1] = 0;
            stairsMap[arenaSize - 1, 0] = 0;
        }
        // Оставшиеся
        for (int i = 1; i < arenaSize / 2; i++)
        {
            up_right = heightMap[i - 1, arenaSize - i];
            current = heightMap[i, arenaSize - i - 1];
            down_left = heightMap[i + 1, arenaSize - i - 2];

            if (up_right > current)
            {
                stairsMap[i, arenaSize - i - 1] = 9;
                stairsMap[arenaSize - i - 1, i] = 11;
            }
            else if (current < down_left)
            {
                stairsMap[i, arenaSize - i - 1] = 7;
                stairsMap[arenaSize - i - 1, i] = 5;
            }
            else
            {
                stairsMap[i, arenaSize - i - 1] = 0;
                stairsMap[arenaSize - i - 1, i] = 0;
            }
        }

        // -=-=-=-=-Расстановка фронтальных лестниц-=-=-=-=-
        // Проверка лестин на краях
        if (heightMap[1, 2] > heightMap[0, 2])
        {
            for (int i = 1; i < arenaSize - 1; i++)
            {
                stairsMap[arenaSize - 1, i] = 1;
                stairsMap[i, 0] = 2;
                stairsMap[0, i] = 3;
                stairsMap[i, arenaSize - 1] = 4;
            }
        }
        else
        {
            for (int i = 1; i < arenaSize - 1; i++)
            {
                stairsMap[arenaSize - 1, i] = 0;
                stairsMap[i, 0] = 0;
                stairsMap[0, i] = 0;
                stairsMap[i, arenaSize - 1] = 0;
            }
        }
        // Оставшиеся
        for (int i = 1; i < arenaSize - 1; i++)
        {
            for (int j = 1; j < arenaSize - 1; j++)
            {
                if (i < j && i + j < arenaSize - 1 ||
                    i > j && i + j > arenaSize - 1)
                {
                    up = heightMap[i - 1, j];
                    current = heightMap[i, j];
                    down = heightMap[i + 1, j];

                    if (up > current)
                        stairsMap[i, j] = 1;
                    else if (current < down)
                        stairsMap[i, j] = 3;
                    else
                        stairsMap[i, j] = 0;
                }
                else if (i < j && i + j > arenaSize - 1 ||
                    i > j && i + j < arenaSize - 1)
                {
                    left = heightMap[i, j - 1];
                    current = heightMap[i, j];
                    right = heightMap[i, j + 1];

                    if (left > current)
                        stairsMap[i, j] = 4;
                    else if (current < right)
                        stairsMap[i, j] = 2;
                    else
                        stairsMap[i, j] = 0;
                }
            }
        }
    }

    void TransformArena()
    {
        //RemoveStairs();

        for (int i = 0; i < arenaSize; i++)
            for (int j = 0; j < arenaSize; j++)
            {
                Vector3 position = new Vector3(chunkScale * i, heightMap[i, j], chunkScale * j);
                chunks[i, j].transform.position = position;//Vector3.Lerp(chunks[i, j].transform.position, position, ChangeSpeed * Time.deltaTime);

                Mesh mesh = chunks[i, j].GetComponent<MeshFilter>().mesh;
                Vector3[] vertices = mesh.vertices;
                vertices[0].z = 0;
                vertices[1].z = 0;
                vertices[2].z = 0;
                vertices[3].z = 0;

                mesh.vertices = vertices;
                mesh.RecalculateBounds();
                chunks[i, j].GetComponent<MeshCollider>().sharedMesh = mesh;

                if (stairsMap[i, j] != 0)
                    PlaceStair(i, j);
            }

        // Change player's position
        {
            float player_x = player.transform.position.x;
            int chunk_i = (int)((player_x + chunkScale / 2) / chunkScale);
            float player_z = player.transform.position.z;
            int chunk_j = (int)((player_z + chunkScale / 2) / chunkScale);

            if (player_x > -chunkScale / 2 && player_z > -chunkScale / 2 &&
                player_x < arenaSize * chunkScale - chunkScale / 2 && player_z < arenaSize * chunkScale - chunkScale / 2)
                player.transform.position = new Vector3(player_x, heightMap[chunk_i, chunk_j] + 2, player_z);
        }

        // Change emenies' position
        for (int i = 0; i < gameController.Enemies.Count; i++)
        {
            float x = gameController.Enemies[i].transform.position.x;
            int chunk_i = (int)((x + chunkScale / 2) / chunkScale);
            float z = gameController.Enemies[i].transform.position.z;
            int chunk_j = (int)((z + chunkScale / 2) / chunkScale);

            if (x > -chunkScale / 2 && z > -chunkScale / 2 &&
                x < arenaSize * chunkScale - chunkScale / 2 && z < arenaSize * chunkScale - chunkScale / 2)
                gameController.Enemies[i].transform.position = new Vector3(x, heightMap[chunk_i, chunk_j] + 2, z);
        }

        //Destroy all BuffBoxes
        for (int i = 0; i < BuffBoxGO.transform.childCount; i++)
        {
            Destroy(BuffBoxGO.transform.GetChild(i).gameObject);
        }
    }
}