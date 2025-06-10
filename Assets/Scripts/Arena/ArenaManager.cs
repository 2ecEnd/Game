using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

public class ArenaManager : MonoBehaviour
{
    private GameObject arena;
    private const int arenaSize = 16;

    public GameObject chunk;
    public GameObject stairs441;
    public GameObject stairs441_concave; // Вогнутая
    public GameObject stairs441_convex; // Выпуклый

    private float chunkScale;
    private float chunkHeight;
    private GameObject[,] chunks;
    private int[,] heightMatrix;

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

        arena.transform.position = new Vector3(0, 0, 0);

        chunks = new GameObject[arenaSize, arenaSize];
        //heightMatrix = new int[arenaSize, arenaSize]
        heightMatrix = arena2;

        // Установка базовых значений высот
        for (int i = 0; i < chunks.GetLength(0); i++)
            for (int j = 0; j < chunks.GetLength(1); j++)
            {
                //heightMatrix[i, j] = 0;
                if (heightMatrix[i, j] == 0)
                {
                    placeStairs(i, j);
                }
                else
                {
                    Vector3 position = new Vector3(chunkScale * i, heightMatrix[i, j] - chunkHeight, chunkScale * j);
                    chunks[i, j] = Instantiate(chunk, position, Quaternion.identity, arena.transform);
                }
            }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void transformArena()
    {

    }

    void placeStairs(int x, int z)
    {
        if (x == 0 || z == 0 || x == arenaSize - 1 || z == arenaSize - 1)
            return;

        int up      = heightMatrix[x, z - 1];
        int right   = heightMatrix[x + 1, z];
        int down    = heightMatrix[x, z + 1];
        int left    = heightMatrix[x - 1, z];

        int up_right    = heightMatrix[x + 1, z - 1];
        int down_right  = heightMatrix[x + 1, z + 1];
        int down_left   = heightMatrix[x - 1, z + 1];
        int up_left     = heightMatrix[x - 1, z - 1];

        // Проверка на прямые лестницы
        if (up == 0 && down == 0 ||
            left == 0 && right == 0)
        {
            if (up > down)
            {
                Vector3 position = new Vector3(chunkScale * x, down, chunkScale * z);
                Instantiate(stairs441, position, Quaternion.Euler(0, 0, 0), arena.transform);
            }
            else if (up < down)
            {
                Vector3 position = new Vector3(chunkScale * x, up, chunkScale * z);
                Instantiate(stairs441, position, Quaternion.Euler(0, 180, 0), arena.transform);
            }
            else if (right > left)
            {
                Vector3 position = new Vector3(chunkScale * x, left, chunkScale * z);
                Instantiate(stairs441, position, Quaternion.Euler(0, 270, 0), arena.transform);
            }
            else if (right < left)
            {
                Vector3 position = new Vector3(chunkScale * x, right, chunkScale * z);
                Instantiate(stairs441, position, Quaternion.Euler(0, 90, 0), arena.transform);
            }
        }
        else // Иначе лестница угловая
        {
            if (up_right == down_right && down_right == down_left)
            {
                if (down_right > up_left)
                {
                    Vector3 position = new Vector3(chunkScale * x, up_left, chunkScale * z);
                    Instantiate(stairs441_concave, position, Quaternion.Euler(0, 180, 0), arena.transform);
                }
                else
                {
                    Vector3 position = new Vector3(chunkScale * x, down_right, chunkScale * z);
                    Instantiate(stairs441_convex, position, Quaternion.Euler(0, 0, 0), arena.transform);
                }
            }
            else if (down_right == down_left && down_left == up_left)
            {
                if (down_left > up_right)
                {
                    Vector3 position = new Vector3(chunkScale * x, up_right, chunkScale * z);
                    Instantiate(stairs441_concave, position, Quaternion.Euler(0, 90, 0), arena.transform);
                }
                else
                {
                    Vector3 position = new Vector3(chunkScale * x, down_left, chunkScale * z);
                    Instantiate(stairs441_convex, position, Quaternion.Euler(0, 270, 0), arena.transform);
                }
            }
            else if (down_left == up_left && up_left == up_right)
            {
                if (up_left > down_right)
                {
                    Vector3 position = new Vector3(chunkScale * x, down_right, chunkScale * z);
                    Instantiate(stairs441_concave, position, Quaternion.Euler(0, 0, 0), arena.transform);
                }
                else
                {
                    Vector3 position = new Vector3(chunkScale * x, up_left, chunkScale * z);
                    Instantiate(stairs441_convex, position, Quaternion.Euler(0, 180, 0), arena.transform);
                }
            }
            else if (up_left == up_right && up_right == down_right)
            {
                if (up_right > down_left)
                {
                    Vector3 position = new Vector3(chunkScale * x, down_left, chunkScale * z);
                    Instantiate(stairs441_concave, position, Quaternion.Euler(0, 270, 0), arena.transform);
                }
                else
                {
                    Vector3 position = new Vector3(chunkScale * x, up_right, chunkScale * z);
                    Instantiate(stairs441_convex, position, Quaternion.Euler(0, 90, 0), arena.transform);
                }
            }
        }
    }
}