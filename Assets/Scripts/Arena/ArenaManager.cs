using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UIElements;

public class ArenaManager : MonoBehaviour
{
    private GameObject arena;
    private const int arenaSize = 16;

    public int flag = 0;

    public GameObject chunk;
    public GameObject stairs441;
    public GameObject stairs441_concave;    // Вогнутая
    public GameObject stairs441_convex;     // Выпуклый

    private float chunkScale;
    private float chunkHeight;

    private GameObject[,] chunks;
    private List<GameObject> stairs;
    public int[,] heightMap;

    private int[,] flatArena = new int[arenaSize, arenaSize]
    {
        { 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1 },
        { 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1 },
        { 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1 },
        { 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1 },
        { 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1 },
        { 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1 },
        { 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1 },
        { 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1 },
        { 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1 },
        { 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1 },
        { 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1 },
        { 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1 },
        { 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1 },
        { 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1 },
        { 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1 },
        { 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1 },
    };
    private int[,] arena1 = new int[arenaSize, arenaSize]
    {
        { 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4 },
        { 4, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 4 },
        { 4, 0, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 0, 4 },
        { 4, 0, 3, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 3, 0, 4 },
        { 4, 0, 3, 0, 2, 2, 2, 2, 2, 2, 2, 2, 0, 3, 0, 4 },
        { 4, 0, 3, 0, 2, 0, 0, 0, 0, 0, 0, 2, 0, 3, 0, 4 },
        { 4, 0, 3, 0, 2, 0, 1, 1, 1, 1, 0, 2, 0, 3, 0, 4 },
        { 4, 0, 3, 0, 2, 0, 1, 8, 8, 1, 0, 2, 0, 3, 0, 4 },
        { 4, 0, 3, 0, 2, 0, 1, 8, 8, 1, 0, 2, 0, 3, 0, 4 },
        { 4, 0, 3, 0, 2, 0, 1, 1, 1, 1, 0, 2, 0, 3, 0, 4 },
        { 4, 0, 3, 0, 2, 0, 0, 0, 0, 0, 0, 2, 0, 3, 0, 4 },
        { 4, 0, 3, 0, 2, 2, 2, 2, 2, 2, 2, 2, 0, 3, 0, 4 },
        { 4, 0, 3, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 3, 0, 4 },
        { 4, 0, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 0, 4 },
        { 4, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 4 },
        { 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4 },
    };
    private int[,] arena2 = new int[arenaSize, arenaSize]
    {
        { 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4 },
        { 4, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 4 },
        { 4, 0, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 0, 4 },
        { 4, 0, 3, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 3, 0, 4 },
        { 4, 0, 3, 0, 2, 2, 2, 2, 2, 2, 2, 2, 0, 3, 0, 4 },
        { 4, 0, 3, 0, 2, 0, 0, 0, 0, 0, 0, 2, 0, 3, 0, 4 },
        { 4, 0, 3, 0, 2, 0, 3, 3, 3, 3, 0, 2, 0, 3, 0, 4 },
        { 4, 0, 3, 0, 2, 0, 3, 8, 8, 3, 0, 2, 0, 3, 0, 4 },
        { 4, 0, 3, 0, 2, 0, 3, 8, 8, 3, 0, 2, 0, 3, 0, 4 },
        { 4, 0, 3, 0, 2, 0, 3, 3, 3, 3, 0, 2, 0, 3, 0, 4 },
        { 4, 0, 3, 0, 2, 0, 0, 0, 0, 0, 0, 2, 0, 3, 0, 4 },
        { 4, 0, 3, 0, 2, 2, 2, 2, 2, 2, 2, 2, 0, 3, 0, 4 },
        { 4, 0, 3, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 3, 0, 4 },
        { 4, 0, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 0, 4 },
        { 4, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 4 },
        { 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4 },
    };

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        arena = new GameObject("Arena");

        chunkScale = chunk.transform.localScale.x;
        chunkHeight = (chunk.transform.localScale.y / 2);   // Нацало координат чанка находится в его центре
                                                            // Поэтому делим высоту пополам

        arena.transform.position = new Vector3(0, 10, 0);

        chunks = new GameObject[arenaSize, arenaSize];
        stairs = new List<GameObject>();
        heightMap = flatArena;
        for (int i = 0; i < chunks.GetLength(0); i++)
            for (int j = 0; j < chunks.GetLength(1); j++)
            {
                Vector3 position = new Vector3(chunkScale * i, heightMap[i, j] - chunkHeight, chunkScale * j);
                chunks[i, j] = Instantiate(chunk, position, Quaternion.identity, arena.transform);
            }
    }

    // Update is called once per frame
    void Update()
    {
        if (flag != 0)
        {
            generateCircleArena();
            removeStairs();
            transformArena();
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

    void transformArena()
    {
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

        if (up > down)
        {
            Vector3 position = new Vector3(chunkScale * x, down, chunkScale * z);
            stairs.Add(Instantiate(stairs441, position, Quaternion.Euler(0, 0, 0), arena.transform));
        }
        else if (up < down)
        {
            Vector3 position = new Vector3(chunkScale * x, up, chunkScale * z);
            stairs.Add(Instantiate(stairs441, position, Quaternion.Euler(0, 180, 0), arena.transform));
        }
        else if (right > left)
        {
            Vector3 position = new Vector3(chunkScale * x, left, chunkScale * z);
            stairs.Add(Instantiate(stairs441, position, Quaternion.Euler(0, 270, 0), arena.transform));
        }
        else // if (right < left)
        {
            Vector3 position = new Vector3(chunkScale * x, right, chunkScale * z);
            stairs.Add(Instantiate(stairs441, position, Quaternion.Euler(0, 90, 0), arena.transform));
        }
    }
    void placeCornerStair(int x, int z)
    {
        int up_right = heightMap[x + 1, z - 1];
        int down_right = heightMap[x + 1, z + 1];
        int down_left = heightMap[x - 1, z + 1];
        int up_left = heightMap[x - 1, z - 1];

        if (up_right == down_right && down_right == down_left)
        {
            if (down_right > up_left)
            {
                Vector3 position = new Vector3(chunkScale * x, up_left, chunkScale * z);
                stairs.Add(Instantiate(stairs441_concave, position, Quaternion.Euler(0, 180, 0), arena.transform));
            }
            else
            {
                Vector3 position = new Vector3(chunkScale * x, down_right, chunkScale * z);
                stairs.Add(Instantiate(stairs441_convex, position, Quaternion.Euler(0, 0, 0), arena.transform));
            }
        }
        else if (down_right == down_left && down_left == up_left)
        {
            if (down_left > up_right)
            {
                Vector3 position = new Vector3(chunkScale * x, up_right, chunkScale * z);
                stairs.Add(Instantiate(stairs441_concave, position, Quaternion.Euler(0, 90, 0), arena.transform));
            }
            else
            {
                Vector3 position = new Vector3(chunkScale * x, down_left, chunkScale * z);
                stairs.Add(Instantiate(stairs441_convex, position, Quaternion.Euler(0, 270, 0), arena.transform));
            }
        }
        else if (down_left == up_left && up_left == up_right)
        {
            if (up_left > down_right)
            {
                Vector3 position = new Vector3(chunkScale * x, down_right, chunkScale * z);
                stairs.Add(Instantiate(stairs441_concave, position, Quaternion.Euler(0, 0, 0), arena.transform));
            }
            else
            {
                Vector3 position = new Vector3(chunkScale * x, up_left, chunkScale * z);
                stairs.Add(Instantiate(stairs441_convex, position, Quaternion.Euler(0, 180, 0), arena.transform));
            }
        }
        else //if (up_left == up_right && up_right == down_right)
        {
            if (up_right > down_left)
            {
                Vector3 position = new Vector3(chunkScale * x, down_left, chunkScale * z);
                stairs.Add(Instantiate(stairs441_concave, position, Quaternion.Euler(0, 270, 0), arena.transform));
            }
            else
            {
                Vector3 position = new Vector3(chunkScale * x, up_right, chunkScale * z);
                stairs.Add(Instantiate(stairs441_convex, position, Quaternion.Euler(0, 90, 0), arena.transform));
            }
        }
    }

    void removeStairs()
    {
        while (stairs.Count > 0)
        {
            Destroy(stairs[0]);
            stairs.RemoveAt(0);
        }
    }

    void generateCircleArena()
    {
        int size = arenaSize / 2;
        int[] tmpHeightMap = new int[size];

        tmpHeightMap[0] = Random.Range(1, 10);
        for (int i = 1; i < size; i++)
        {
            int height = Random.Range(-4, 4);
            if (height == 0)
            {
                tmpHeightMap[i] = tmpHeightMap[i - 1];
                continue;
            }

            if (i + 1 == tmpHeightMap.Length)
            {
                tmpHeightMap[i] = tmpHeightMap[i - 1]; 
            }
            else
            {
                tmpHeightMap[i] = 0;
                int tmp = tmpHeightMap[i - 1] + height;
                if (tmp < 1 || tmp > 10)
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
}