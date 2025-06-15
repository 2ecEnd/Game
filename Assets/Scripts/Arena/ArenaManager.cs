using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UIElements;

public class ArenaManager : MonoBehaviour
{
    public int flag = 0;

    private GameObject arena;

    private const int arenaSize = 16;
    private const int chunkScale = 4;
    private float chunkHeight;

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

    private GameObject[,] chunks;
    private List<GameObject> stairs;
    public int[,] heightMap;

    private List<int[,]> arenaPresets;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        arena = new GameObject("Arena");
        arena.transform.position = new Vector3(0, 10, 0);
        createPresets();

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
    void Update()
    {
        if (flag != 0)
        {
            changeArena();
            flag = 0;

            //for (int i = 0; i < arenaSize; i++)
            //{
            //    string str = "";
            //    for (int j = 0; j < arenaSize; j++)
            //    {
            //         str += heightMap[i, j].ToString() + " ";
            //    }
            //    Debug.Log(str);
            //}
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


    void createPresets()
    {
        arenaPresets = new List<int[,]>();

        int[,] flatArena = new int[arenaSize, arenaSize]
        {
            { 9, 9, 9, 9, 9, 9, 9, 9, 9, 9, 9, 9, 9, 9, 9, 9 },
            { 9, 9, 9, 9, 9, 9, 9, 9, 9, 9, 9, 9, 9, 9, 9, 9 },
            { 9, 9, 9, 9, 9, 9, 9, 9, 9, 9, 9, 9, 9, 9, 9, 9 },
            { 9, 9, 9, 9, 9, 9, 9, 9, 9, 9, 9, 9, 9, 9, 9, 9 },
            { 9, 9, 9, 9, 9, 9, 9, 9, 9, 9, 9, 9, 9, 9, 9, 9 },
            { 9, 9, 9, 9, 9, 9, 9, 9, 9, 9, 9, 9, 9, 9, 9, 9 },
            { 9, 9, 9, 9, 9, 9, 9, 9, 9, 9, 9, 9, 9, 9, 9, 9 },
            { 9, 9, 9, 9, 9, 9, 9, 9, 9, 9, 9, 9, 9, 9, 9, 9 },
            { 9, 9, 9, 9, 9, 9, 9, 9, 9, 9, 9, 9, 9, 9, 9, 9 },
            { 9, 9, 9, 9, 9, 9, 9, 9, 9, 9, 9, 9, 9, 9, 9, 9 },
            { 9, 9, 9, 9, 9, 9, 9, 9, 9, 9, 9, 9, 9, 9, 9, 9 },
            { 9, 9, 9, 9, 9, 9, 9, 9, 9, 9, 9, 9, 9, 9, 9, 9 },
            { 9, 9, 9, 9, 9, 9, 9, 9, 9, 9, 9, 9, 9, 9, 9, 9 },
            { 9, 9, 9, 9, 9, 9, 9, 9, 9, 9, 9, 9, 9, 9, 9, 9 },
            { 9, 9, 9, 9, 9, 9, 9, 9, 9, 9, 9, 9, 9, 9, 9, 9 },
            { 9, 9, 9, 9, 9, 9, 9, 9, 9, 9, 9, 9, 9, 9, 9, 9 },
        };
        int[,] pillars = new int[arenaSize, arenaSize]
        {
            { 9, 9, 9, 9,   9, 9, 9, 9,   9, 9, 9, 9,   9, 9, 9, 9 },
            { 9, 9, 9, 9,   9, 9, 9, 9,   9, 9, 9, 9,   9, 9, 9, 9 },
            { 9, 9, 9, 9,   9, 9, 9, 9,   9, 9, 9, 9,   9, 9, 9, 9 },
            { 9, 9, 9, 20, 20, 9, 9, 9,   9, 9, 9, 20, 20, 9, 9, 9 },
            { 9, 9, 9, 20, 20, 9, 9, 9,   9, 9, 9, 20, 20, 9, 9, 9 },
            { 9, 9, 9, 9,   9, 9, 9, 9,   9, 9, 9, 9,   9, 9, 9, 9 },
            { 9, 9, 9, 9,   9, 9, 9, 9,   9, 9, 9, 9,   9, 9, 9, 9 },
            { 9, 9, 9, 9,   9, 9, 9, 20, 20, 9, 9, 9,   9, 9, 9, 9 },
            { 9, 9, 9, 9,   9, 9, 9, 20, 20, 9, 9, 9,   9, 9, 9, 9 },
            { 9, 9, 9, 9,   9, 9, 9, 9,   9, 9, 9, 9,   9, 9, 9, 9 },
            { 9, 9, 9, 9,   9, 9, 9, 9,   9, 9, 9, 9,   9, 9, 9, 9 },
            { 9, 9, 9, 20, 20, 9, 9, 9,   9, 9, 9, 20, 20, 9, 9, 9 },
            { 9, 9, 9, 20, 20, 9, 9, 9,   9, 9, 9, 20, 20, 9, 9, 9 },
            { 9, 9, 9, 9,   9, 9, 9, 9,   9, 9, 9, 9,   9, 9, 9, 9 },
            { 9, 9, 9, 9,   9, 9, 9, 9,   9, 9, 9, 9,   9, 9, 9, 9 },
            { 9, 9, 9, 9,   9, 9, 9, 9,   9, 9, 9, 9,   9, 9, 9, 9 },
        };
        int[,] X = new int[arenaSize, arenaSize] // TODO: replace 9 to 6
        {
            { 10, 10, 0, 0, 9, 9, 9, 9, 9, 9, 9, 9, 0, 0, 10, 10 },
            { 10, 10, 10, 0, 0, 9, 9, 9, 9, 9, 9, 0, 0, 10, 10, 10 },
            { 0, 10, 10, 10, 0, 0, 9, 9, 9, 9, 0, 0, 10, 10, 10, 0 },
            { 0, 0, 10, 10, 10, 0, 0, 9, 9, 0, 0, 10, 10, 10, 0, 0 },
            { 9, 0, 0, 10, 10, 10, 0, 0, 0, 0, 10, 10, 10, 0, 0, 9 },
            { 9, 9, 0, 0, 10, 10, 10, 0, 0, 10, 10, 10, 0, 0, 9, 9 },
            { 9, 9, 9, 0, 0, 10, 10, 10, 10, 10, 10, 0, 0, 9, 9, 9 },
            { 9, 9, 9, 9, 0, 0, 10, 10, 10, 10, 0, 0, 9, 9, 9, 9 },
            { 9, 9, 9, 9, 0, 0, 10, 10, 10, 10, 0, 0, 9, 9, 9, 9 },
            { 9, 9, 9, 0, 0, 10, 10, 10, 10, 10, 10, 0, 0, 9, 9, 9 },
            { 9, 9, 0, 0, 10, 10, 10, 0, 0, 10, 10, 10, 0, 0, 9, 9 },
            { 9, 0, 0, 10, 10, 10, 0, 0, 0, 0, 10, 10, 10, 0, 0, 9 },
            { 0, 0, 10, 10, 10, 0, 0, 9, 9, 0, 0, 10, 10, 10, 0, 0 },
            { 0, 10, 10, 10, 0, 0, 9, 9, 9, 9, 0, 0, 10, 10, 10, 0 },
            { 10, 10, 10, 0, 0, 9, 9, 9, 9, 9, 9, 0, 0, 10, 10, 10 },
            { 10, 10, 0, 0, 9, 9, 9, 9, 9, 9, 9, 9, 0, 0, 10, 10 },
        };

        arenaPresets.Add(flatArena);
        arenaPresets.Add(pillars);
        //arenaPresets.Add(X);
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


    void changeArena()
    {
        float coin = Random.value;
        if (coin < 0.7)
            generateCircleArena();
        else
            chooseFromPresets();

        transformArena();
    }

    void chooseFromPresets()
    {
        int choice = Random.Range(0, arenaPresets.Count);
        heightMap = arenaPresets[choice];
    }

    // TODO: need to change choice logic
    void generateCircleArena()
    {
        int size = arenaSize / 2;
        int[] tmpHeightMap = new int[size];

        tmpHeightMap[0] = Random.Range(1, 20);
        for (int i = 1; i < size; i++)
        {
            int height = Random.Range(-3, 4);
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
    }
}