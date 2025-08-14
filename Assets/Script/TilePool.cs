using UnityEngine;
using System.Collections.Generic;

public class TilePool : MonoBehaviour
{
    public GameObject tilePrefab;
    public int initialSize = 20;

    private Queue<TileUI> pool = new Queue<TileUI>();

    void Awake()
    {
        for (int i = 0; i < initialSize; i++)
        {
            CreateNewTile();
        }
    }

    void CreateNewTile()
    {
        GameObject obj = Instantiate(tilePrefab, transform);
        TileUI tile = obj.GetComponent<TileUI>();
        obj.SetActive(false);
        pool.Enqueue(tile);
    }

    public TileUI GetTile()
    {
        if (pool.Count == 0)
            CreateNewTile();

        var t = pool.Dequeue();
        return t;
    }

    public void ReturnTile(TileUI tile)
    {
        tile.gameObject.SetActive(false);
        pool.Enqueue(tile);
    }
    public void ReturnAllTiles()
    {
        foreach (Transform child in transform)
        {
            child.gameObject.SetActive(false);
        }
    }

}