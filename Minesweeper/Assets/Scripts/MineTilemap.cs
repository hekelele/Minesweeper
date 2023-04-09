using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;


public class MineTilemap : MonoBehaviour
{
    public enum TileState
    {
        Empty,
        Bomb,
    }

    [SerializeField] private MineGameManager gameManager;
    [SerializeField] private TileBase[] groundTiles;
    [SerializeField] private TileBase[] digitTiles;
    [SerializeField] private TileBase bombTile;
    [SerializeField] private int rowCount, columnCount;
    [SerializeField] private int bombCount;

    private bool inited;
    private Tilemap tilemap;
    private TileState[] tileStates;
    private Vector3Int[] offsets =
        {
            new Vector3Int(-1, 1, 0),
            new Vector3Int(0, 1, 0),
            new Vector3Int(1, 1, 0),
            new Vector3Int(-1, 0, 0),
            new Vector3Int(1, 0, 0),
            new Vector3Int(-1, -1, 0),
            new Vector3Int(0, -1, 0),
            new Vector3Int(1, -1, 0),
        };

    private void Awake()
    {
        tilemap = GetComponent<Tilemap>();
    }

    private Vector3Int FromIndexToCell(int index)
    {
        int xx = index % columnCount;
        int yy = index / columnCount;
        return new Vector3Int(xx, yy, 0);
    }

    private Vector3Int FromWorldPosToCell(Vector3 worldPos)
    {
        return tilemap.WorldToCell(worldPos);
    }

    private int FromCellToIndex(Vector3Int vp)
    {
        return vp.y * columnCount + vp.x;
    }

    private bool IsOutOfRange(Vector3Int vp)
    {
        if (vp.x < 0 || vp.x >= columnCount)
        {
            return true;
        }

        if (vp.y < 0 || vp.y >= rowCount)
        {
            return true;
        }

        return false;
    }

    public void ResetGame()
    {
        for (int i = 0; i < rowCount; i++)
        {
            for (int j = 0; j < columnCount; j++)
            {
                Vector3Int vp = new Vector3Int(j, i, 0);
                tilemap.SetTile(vp, groundTiles[0]);
            }
        }
        inited = false;
    }

    public bool FlipTile(Vector2 mousePos)
    {
        Vector3Int vp = FromWorldPosToCell(mousePos);
        var tile = tilemap.GetTile(vp);

        if (tile == null)
        {
            return false;
        }

        CheckMineField(vp, tile);

        if (IsPosHasBomb(vp))
        {
            gameManager.GameOver();
            tilemap.SetTile(vp, bombTile);
            return false;
        }
        else
        {
            var result = OnFlipEmptyTile(vp, tile);
            CheckGameWin();
            return result;
        }
    }

    private void CheckMineField(Vector3Int vp, TileBase tile)
    {
        if (inited)
        {
            return;
        }

        inited = true;

        tileStates = new TileState[rowCount * columnCount];
        int[] indexList = new int[tileStates.Length];

        for (int i = 0; i < tileStates.Length; i++)
        {
            tileStates[i] = TileState.Empty;
            indexList[i] = i;
        }

        int startIndex = 0;
        for (int i = 0; i < offsets.Length; i++)
        {
            Vector3Int pos = vp + offsets[i];
            if (IsOutOfRange(pos))
            {
                continue;
            }

            int index = FromCellToIndex(pos);
            SwapArray(indexList, index, startIndex);
            startIndex += 1;
        }

        int index0 = FromCellToIndex(vp);
        SwapArray(indexList, index0, startIndex);
        startIndex += 1;

        for (int i = startIndex; i < startIndex + bombCount; i++)
        {
            int rand = Random.Range(i, tileStates.Length);
            SwapArray(indexList, rand, i);
            tileStates[indexList[i]] = TileState.Bomb;
        }
    }

    private void SwapArray(int[] array, int index0, int index1)
    {
        int temp = array[index0];
        array[index0] = array[index1];
        array[index1] = temp;
    }

    public bool MarkTile(Vector2 mousePos)
    {
        Vector3Int vp = FromWorldPosToCell(mousePos);
        var tile = tilemap.GetTile(vp);

        if (tile == null)
        {
            return false;
        }

        int ground = IsGroundTile(tile);
        if (ground >= 0)
        {
            int index = (ground + 1) % groundTiles.Length;
            tilemap.SetTile(vp, groundTiles[index]);
            CheckMarker();
            return true;
        }

        return false;
    }

    private bool OnFlipEmptyTile(Vector3Int vp, TileBase tile)
    {
        if (tile == groundTiles[1] || tile == groundTiles[2])
        {
            return false;
        }

        if (tile == groundTiles[0])
        {
            int bombCount = CountNeighbourMineCount(vp);
            tilemap.SetTile(vp, digitTiles[bombCount]);
            if (bombCount == 0)
            {
                for (int i = 0; i < offsets.Length; i++)
                {
                    Vector3Int pos = vp + offsets[i];
                    if (IsOutOfRange(pos))
                    {
                        continue;
                    }
                    var newTile = tilemap.GetTile(pos);
                    OnFlipEmptyTile(pos, newTile);
                }
            }

            return true;
        }

        return false;
    }

    private int CountNeighbourMineCount(Vector3Int vp)
    {
        int count = 0;

        for (int i = 0; i < offsets.Length; i++)
        {
            Vector3Int pos = vp + offsets[i];
            if (IsOutOfRange(pos))
            {
                continue;
            }

            if (IsPosHasBomb(pos))
            {
                count++;
            }
        }

        return count;
    }

    private bool IsPosHasBomb(Vector3Int vp)
    {
        int index = FromCellToIndex(vp);
        if (tileStates[index] == TileState.Bomb)
        {
            return true;
        }

        return false;
    }

    private int IsGroundTile(TileBase tile)
    {
        for (int i = 0; i < groundTiles.Length; i++)
        {
            if (tile == groundTiles[i])
            {
                return i;
            }
        }

        return -1;
    }

    private void CheckGameWin()
    {
        int count = 0;
        for(int i=0; i<rowCount; i++)
        {
            for(int j=0; j<columnCount; j++)
            {
                Vector3Int vp = new Vector3Int(j, i, 0);
                TileBase tile = tilemap.GetTile(vp);
                if (IsGroundTile(tile) >= 0)
                {
                    count++;
                }
            }
        }

        if(count == bombCount)
        {
            gameManager.GameWin();
        }
    }

    private void CheckMarker()
    {
        int count = 0;
        for (int i = 0; i < rowCount; i++)
        {
            for (int j = 0; j < columnCount; j++)
            {
                Vector3Int vp = new Vector3Int(j, i, 0);
                TileBase tile = tilemap.GetTile(vp);
                if (tile == groundTiles[1])
                {
                    count++;
                }
            }
        }

        gameManager.SetBombRemainCount(bombCount - count);
    }
}
