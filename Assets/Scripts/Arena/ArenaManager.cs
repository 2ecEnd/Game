using Assets.Scripts.Gameplay;
using System;
using System.Collections.Generic;
using System.Linq;
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
    public GameObject stair_1;
    public GameObject stair_05;
    public GameObject stair_025;
    public GameObject stair_1_concave;    // Вогнутая
    public GameObject stair_05_concave;
    public GameObject stair_025_concave;
    public GameObject stair_1_convex;     // Выпуклая
    public GameObject stair_05_convex;
    public GameObject stair_025_convex;

    [Header("Arena Parameters")]
    public int[,] heightMap;
    private List<int[,]> arenaPresets;
    private GameObject[,] chunks;
    private List<GameObject> stairs;

    [Header("BuffBox Parameters")]
    public GameObject BuffBoxGO;

    [Header("Other Parameters")]
    private int flag = 0;
    private const int arenaSize = 20;
    private const int chunkScale = 4;
    private float chunkHeight;
    private const int killHeight = -20;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        arena = new GameObject("Arena");
        arena.transform.position = new Vector3(0, 10, 0);
        createPresets();

        gameController = gameObject.GetComponent<GameController>();

        chunkHeight = (chunk.transform.localScale.y / 2);   // Нацало координат чанка находится в его центре
                                                            // Поэтому делим высоту пополам

        chunks = new GameObject[arenaSize, arenaSize];
        stairs = new List<GameObject>();
        heightMap = arenaPresets[0];
        for (int i = 0; i < chunks.GetLength(0); i++)
            for (int j = 0; j < chunks.GetLength(1); j++)
            {
                Vector3 position = new Vector3(chunkScale * i, heightMap[i, j] - chunkHeight, chunkScale * j);
                chunks[i, j] = Instantiate(chunk, position, Quaternion.identity, arena.transform);
                chunks[i, j].transform.localScale = new Vector3(chunkScale, 64, chunkScale);
            }
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (flag != 0)
        {
            changeArena(flag);
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

    public int getArenaSize()
    {
        return arenaSize;
    }
    public int getChunkScale()
    {
        return chunkScale;
    }
    public int getKillHeight()
    {
        return killHeight;
    }


    void createPresets()
    {
        arenaPresets = new List<int[,]>();

        int[,] flatArena = new int[arenaSize, arenaSize];
        for (int i = 0; i < arenaSize; i++)
            for (int j = 0; j < arenaSize; j++)
                flatArena[i, j] = 9;

        int[,] pillars = new int[arenaSize, arenaSize]
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

        arenaPresets.Add(flatArena);
        arenaPresets.Add(pillars);
    }


    void placeStair(int x, int z)
    {
        if (x == 0 || z == 0 || x == arenaSize - 1 || z == arenaSize - 1)
            return;

        int up      = heightMap[x, z - 1];
        int right   = heightMap[x + 1, z];
        int down    = heightMap[x, z + 1];
        int left    = heightMap[x - 1, z];

        if (up == 0 && down == 0 || left == 0 && right == 0)
            placeFrontStair(x, z);
        else
            placeCornerStair(x, z);
    }

    void placeFrontStair(int x, int z)
    {
        int up = heightMap[x, z - 1];
        int right = heightMap[x + 1, z];
        int down = heightMap[x, z + 1];
        int left = heightMap[x - 1, z];

        int difference;
        Vector3 position;
        Quaternion rotation;
        GameObject stairType;

        if (up > down)
        {
            difference = up - down;
            position = new Vector3(chunkScale * x, down, chunkScale * z);
            rotation = Quaternion.Euler(0, 180, 0);
        }
        else if (up < down)
        {
            difference = down - up;
            position = new Vector3(chunkScale * x, up, chunkScale * z);
            rotation = Quaternion.Euler(0, 0, 0);
        }
        else if (right > left)
        {
            difference = right - left;
            position = new Vector3(chunkScale * x, left, chunkScale * z);
            rotation = Quaternion.Euler(0, 90, 0);
        }
        else //if (right < left)
        {
            difference = left - right;
            position = new Vector3(chunkScale * x, right, chunkScale * z);
            rotation = Quaternion.Euler(0, 270, 0);
        }

        if (difference == chunkScale)
            stairType = stair_1;
        else if (difference == chunkScale / 2)
            stairType = stair_05;
        else //if (difference == chunkScale / 4)
            stairType = stair_025;

        GameObject stair = Instantiate(stairType, position, rotation, arena.transform);
        stair.transform.localScale = new Vector3(chunkScale, chunkScale * stair.transform.localScale.y, chunkScale);

        stairs.Add(stair);
    }

    void placeCornerStair(int x, int z)
    {
        int up_right = heightMap[x + 1, z - 1];
        int down_right = heightMap[x + 1, z + 1];
        int down_left = heightMap[x - 1, z + 1];
        int up_left = heightMap[x - 1, z - 1];

        int difference;
        bool isConvex;
        Vector3 position;
        Quaternion rotation;
        GameObject stairType;

        if (up_right == down_right && down_right == down_left)
        {
            if (down_right > up_left)
            {
                difference = down_right - up_left;
                isConvex = false;
                position = new Vector3(chunkScale * x, up_left, chunkScale * z);
                rotation = Quaternion.Euler(0, 0, 0);
            }
            else
            {
                difference = up_left - down_right;
                isConvex = true;
                position = new Vector3(chunkScale * x, down_right, chunkScale * z);
                rotation = Quaternion.Euler(0, 180, 0);
            }
        }
        else if (down_right == down_left && down_left == up_left)
        {
            if (down_left > up_right)
            {
                difference = down_left - up_right;
                isConvex = false;
                position = new Vector3(chunkScale * x, up_right, chunkScale * z);
                rotation = Quaternion.Euler(0, 270, 0);
            }
            else
            {
                difference = up_right - down_left;
                isConvex = true;
                position = new Vector3(chunkScale * x, down_left, chunkScale * z);
                rotation = Quaternion.Euler(0, 90, 0);
            }
        }
        else if (down_left == up_left && up_left == up_right)
        {
            if (up_left > down_right)
            {
                difference = up_left - down_right;
                isConvex = false;
                position = new Vector3(chunkScale * x, down_right, chunkScale * z);
                rotation = Quaternion.Euler(0, 180, 0);
            }
            else
            {
                difference = down_right - up_left;
                isConvex = true;
                position = new Vector3(chunkScale * x, up_left, chunkScale * z);
                rotation = Quaternion.Euler(0, 0, 0);
            }
        }
        else //if (up_left == up_right && up_right == down_right)
        {
            if (up_right > down_left)
            {
                difference = up_right - down_left;
                isConvex = false;
                position = new Vector3(chunkScale * x, down_left, chunkScale * z);
                rotation = Quaternion.Euler(0, 90, 0);
            }
            else
            {
                difference = down_left - up_right;
                isConvex = true;
                position = new Vector3(chunkScale * x, up_right, chunkScale * z);
                rotation = Quaternion.Euler(0, 270, 0);
            }
        }

        if (difference == chunkScale)
            stairType = isConvex ? stair_1_convex : stair_1_concave;
        else if (difference == chunkScale / 2)
            stairType = isConvex ? stair_05_convex : stair_05_concave;
        else //if (difference == chunkScale / 4)
            stairType = isConvex ? stair_025_convex : stair_025_concave;

        GameObject stair = Instantiate(stairType, position, rotation, arena.transform);
        stair.transform.localScale = new Vector3(chunkScale, chunkScale * stair.transform.localScale.y, chunkScale);

        stairs.Add(stair);
    }

    void removeStairs()
    {
        while (stairs.Count > 0)
        {
            Destroy(stairs[0]);
            stairs.RemoveAt(0);
        }
    }


    void changeArena(int flag)
    {
        float coin = UnityEngine.Random.value;
        if (flag == 1)
        {
            createPresets(); // TODO: need to fix
            chooseFromPresets();
        }
        else if (flag == 2)
            generateCircleArena();
            //if (coin < 0.7)
            //    generateCircleArena();
            //else
            //    chooseFromPresets();

        transformArena();
    }

    void chooseFromPresets()
    {
        int choice = UnityEngine.Random.Range(0, arenaPresets.Count);
        heightMap = arenaPresets[choice];
    }

    // TODO: need to change choice logic
    void generateCircleArena()
    {
        int size = arenaSize / 2;
        int[] tmpHeightMap = new int[size];

        tmpHeightMap[0] = UnityEngine.Random.Range(1, 20);
        for (int i = 1; i < size; i++)
        {
            int height = UnityEngine.Random.Range(-3, 4);
            if (height == 0)
            {
                tmpHeightMap[i] = tmpHeightMap[i - 1];
                continue;
            }

            if (height == -3)
                height = -4;
            else if (height == 3)
                height = 4;

            if (i + 1 == tmpHeightMap.Length)
            {
                tmpHeightMap[i] = tmpHeightMap[i - 1];
            }
            else
            {
                tmpHeightMap[i] = 0;
                int tmp = tmpHeightMap[i - 1] + height;
                if (tmp < 1 || tmp > 20)
                    tmpHeightMap[i + 1] = tmpHeightMap[i - 1] - height;
                else
                    tmpHeightMap[i + 1] = tmp;
                i++;
            }
        }

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

    void transformArena()
    {
        removeStairs();

        for (int i = 0; i < chunks.GetLength(0); i++)
            for (int j = 0; j < chunks.GetLength(1); j++)
            {
                if (heightMap[i, j] == 0)
                {
                    chunks[i, j].transform.position = new Vector3(chunkScale * i, -chunkHeight, chunkScale * j);
                    placeStair(i, j);
                }
                else
                {
                    Vector3 position = new Vector3(chunkScale * i, heightMap[i, j] - chunkHeight, chunkScale * j);
                    chunks[i, j].transform.position = position;
                }
            }

        // Change player's position
        {
            float player_x = player.transform.position.x;
            int chunk_i = (int)(player_x / chunkScale);
            float player_z = player.transform.position.z;
            int chunk_j = (int)(player_z / chunkScale);

            if (chunk_i >= 0 && chunk_j >= 0 && chunk_i < arenaSize && chunk_j < arenaSize)
            {
                float y = heightMap[chunk_i, chunk_j];
                if (y == 0)
                {
                    if (chunk_i >= 0)
                        y = Mathf.Max(y, heightMap[chunk_i - 1, chunk_j]);
                    if (chunk_i < arenaSize)
                        y = Mathf.Max(y, heightMap[chunk_i + 1, chunk_j]);
                    if (chunk_j >= 0)
                        y = Mathf.Max(y, heightMap[chunk_i, chunk_j - 1]);
                    if (chunk_j < arenaSize)
                        y = Mathf.Max(y, heightMap[chunk_i, chunk_j + 1]);
                }
                y++;
                player.transform.position = new Vector3(player_x, y, player_z);
            }
        }

        // Change emenies' position
        for (int i = 0; i < gameController.Enemies.Count; i++)
        {
            float x = gameController.Enemies[i].transform.position.x;
            int chunk_i = (int)(x / chunkScale);
            float z = gameController.Enemies[i].transform.position.z;
            int chunk_j = (int)(z / chunkScale);

            if (chunk_i >= 0 && chunk_j >= 0 && chunk_i < arenaSize && chunk_j < arenaSize)
            {
                float y = heightMap[chunk_i, chunk_j];
                if (y == 0)
                {
                    if (chunk_i >= 0)
                        y = Mathf.Max(y, heightMap[chunk_i - 1, chunk_j]);
                    if (chunk_i < arenaSize)
                        y = Mathf.Max(y, heightMap[chunk_i + 1, chunk_j]);
                    if (chunk_j >= 0)
                        y = Mathf.Max(y, heightMap[chunk_i, chunk_j - 1]);
                    if (chunk_j < arenaSize)
                        y = Mathf.Max(y, heightMap[chunk_i, chunk_j + 1]);
                }
                y++;

                gameController.Enemies[i].transform.position = new Vector3(x, y, z);
            }
        }

        //Destroy all BuffBoxes
        for(int i = 0; i < BuffBoxGO.transform.childCount; i++)
        {
            Destroy(BuffBoxGO.transform.GetChild(i).gameObject);
        }
    }
}