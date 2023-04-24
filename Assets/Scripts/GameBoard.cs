using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameBoard : MonoBehaviour
{
    public enum Scene
    {
        ToonBlast,
    }

    [SerializeField] private int width;
    [SerializeField] private int height;
    [SerializeField] private Tile tileCrate;
    [SerializeField] private List<Tile> tilesType;
    [SerializeField] private Transform boardSpawnParent;
    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private Button resetGameboard;

    private List<List<Tile>> tilesBoard;
    private List<Tile> toDestroy;

    private int score;
    private int numberTileMatched;

    void Start()
    {
        resetGameboard.onClick.AddListener(Reset);
        score = 0;
        toDestroy = new List<Tile>();
        tilesBoard = new List<List<Tile>>();
        SpawnRandomBoard();
        UpdateScore();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
            Application.Quit();
    }

    private void Reset()
    {
        SceneManager.LoadScene(Scene.ToonBlast.ToString());
    }

    private void SpawnRandomBoard()
    {
        for (int x = 0; x < width; x++)
        {
            tilesBoard.Add(new List<Tile>());
            for (int y = 0; y < height; y++)
            {
                Tile tile;
                if (y < (height * 2 / 3))
                    tile = Instantiate(tileCrate, new Vector2(x * .5f, y * .5f), Quaternion.identity); // spawn crate on 2/3 bot of the gameboard
                else
                    tile = Instantiate(tilesType[Random.Range(0, tilesType.Count)], new Vector2(x * .5f, y * .5f), Quaternion.identity); // spawn random tiles for 1/3 top of the board
                tile.SetPosition(x, y);
                tile.SetGameboard(this);
                tile.transform.parent = boardSpawnParent;
                tilesBoard[x].Add(tile);
            }

        }
    }

    private void SpawnIncompleteColumn()
    {
        for (int x = 0; x < width; x++)
        {
            if (tilesBoard[x].Count != height)
            {
                int missingTile = height - tilesBoard[x].Count;
                for (int i = 0; i < missingTile; i++)
                {
                    Tile tile = Instantiate(tilesType[Random.Range(0, tilesType.Count)], new Vector2(x * .5f, height * 0.5f + i * .5f + 1f), Quaternion.identity); // spawn random tiles on top of incomplete column
                    int y = tilesBoard[x].Count;
                    tile.SetPosition(x, y);
                    tile.SetGameboard(this);
                    tile.transform.parent = boardSpawnParent;
                    tilesBoard[x].Add(tile);
                }
            }
        }
    }

    public void FindMatchFromTile(Tile tile, bool rootTile = false)
    {
        if (rootTile)
        {
            numberTileMatched = 1;
            tile.SetTileChecked(true);
        }
        int x = tile.GetColumn();
        int y = tile.GetRow();
        Tile.TileType type = tile.GetTileType();
        tile.SetToDestroy(true); // Destroy all tile that match the root tile. Only matching tile will enter the recursive

        if (x > 0 && !tilesBoard[x - 1][y].GetTileChecked()) // if on board and tile not checked yet
        {
            CheckTile(x - 1, y, type); // Check tile on left
        }
        if (y > 0 && !tilesBoard[x][y - 1].GetTileChecked()) // if on board and tile not checked yet
        {
            CheckTile(x, y - 1, type); // Check tile below
        }
        if (x < width - 1 && !tilesBoard[x + 1][y].GetTileChecked()) // if on board and tile not checked yet
        {
            CheckTile(x + 1, y, type); // Check tile on right
        }
        if (y < height - 1 && !tilesBoard[x][y + 1].GetTileChecked()) // if on board and tile not checked yet
        {
            CheckTile(x, y + 1, type); // Check tile above
        }

        if (rootTile) // only proceed from the root recursive call (all recursive calls found a dead end)
        {
            if (numberTileMatched > 1) // if at least 2 tiles found, update gameboard list and destroy matching tiles (1 tile = root tile only = no match)
            {
                UpdateGameboard(); // remove tagged tiles from the gameboard list and set new tiles positions
                DestroyMatchedTiles(); // destroy all tagged tiles
            }
            ResetTileStatus(); // reset status of all tiles
            SpawnIncompleteColumn(); // spawn tiles on incomplete column
        }
    }

    private void DestroyMatchedTiles()
    {
        // destroy all tagged tiles
        score += numberTileMatched;
        foreach (Tile tile in toDestroy)
            Destroy(tile.gameObject);
        toDestroy.Clear();
        UpdateScore();
    }

    private void UpdateScore()
    {
        scoreText.text = $"Score: {score}";
    }

    private void CheckTile(int x, int y, Tile.TileType type)
    {
        tilesBoard[x][y].SetTileChecked(true); // Set the tile as checked to avoid looping
        if (tilesBoard[x][y].GetTileType() == type)
        {
            numberTileMatched++;
            FindMatchFromTile(tilesBoard[x][y]); // if tile is matching root type, go back to the recursive function from this tile
        }
        if (tilesBoard[x][y].GetTileType() == Tile.TileType.Crate) tilesBoard[x][y].SetToDestroy(true); // tag the crate tile to destroy them, don't go back to the recursive function from this tile
    }

    private void UpdateGameboard()
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                Tile tmpTile = tilesBoard[x][y];
                if (tmpTile.GetToDestroy())
                {
                    toDestroy.Add(tmpTile); // add tagged tile to the destroy list
                    tilesBoard[x][y] = null; // remove the tile object from the list
                }
            }
            tilesBoard[x] = tilesBoard[x].Where(c => c != null).ToList(); // clear the column list of all null tiles (destroyed)
        }

        for (int x = 0; x < tilesBoard.Count; x++)
        {
            for (int y = 0; y < tilesBoard[x].Count; y++)
            {
                tilesBoard[x][y].SetPosition(x, y); // set the new position for all remaining tiles
            }
        }
    }

    public void ResetTileStatus()
    {
        foreach (List<Tile> tileColumn in tilesBoard)
            foreach (Tile tile in tileColumn)
            {
                tile.SetTileChecked(false); // reset all board to uncheck
                tile.SetToDestroy(false); // reset non destroyed root tile (if no match found)
            }
    }
}
