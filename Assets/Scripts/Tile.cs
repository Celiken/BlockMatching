using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tile : MonoBehaviour
{
    public enum TileType
    {
        Red,
        Blue,
        Green,
        Yellow,
        Pink,
        Crate,
    }

    private GameBoard board;

    [SerializeField] private TileType type;
    private int column, row;

    private float tileMoveSpeed = 5f;

    private bool tileChecked = false;
    private bool toDestroy = false;

    private void Update()
    {
        transform.position = Vector2.MoveTowards(transform.position, new Vector2(column * 0.5f, row * 0.5f), Time.deltaTime * tileMoveSpeed); // move the tile prefab to the new position
    }

    public void SetPosition(int x, int y)
    {
        column = x;
        row = y;
    }
    public int GetColumn()
    {
        return column;
    }
    public int GetRow()
    {
        return row;
    }
    public TileType GetTileType()
    {
        return type;
    }

    public void SetGameboard(GameBoard board)
    {
        this.board = board;
    }

    private void OnMouseDown()
    {
        if (type != TileType.Crate)
            board.FindMatchFromTile(this, true); // start recursive function from the root tile
    }

    public void SetTileChecked(bool check)
    {
        tileChecked = check;
    }
    public bool GetTileChecked()
    {
        return tileChecked;
    }
    public void SetToDestroy(bool toDestroy)
    {
        this.toDestroy = toDestroy;
    }
    public bool GetToDestroy()
    {
        return toDestroy;
    }
}
