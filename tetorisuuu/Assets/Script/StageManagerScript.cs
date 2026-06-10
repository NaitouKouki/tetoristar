using UnityEngine;

public class StageManager : MonoBehaviour
{
    public const int Width = 10;
    public const int Height = 20;
    public static int[,] grid = new int[Width, Height];

    // 画面上の固定ブロック（オブジェクト）を記憶する場所
    public static GameObject[,] visualGrid = new GameObject[Width, Height];

    // ★復活：ミノがそこに移動できるかチェックする関数
    public static bool IsValidPosition(Vector2Int pos)
    {
        // 左右の壁、および「床（0未満）」のチェック
        if (pos.x < 0 || pos.x >= Width || pos.y < 0)
        {
            return false; // ここは絶対にアウト
        }

        // 画面内、または画面より上（出現位置）のチェック
        if (pos.y < Height)
        {
            // 画面内にあるときだけ、配列にブロックがあるかチェックする
            if (grid[pos.x, pos.y] != 0)
            {
                return false; // 既にブロックがある
            }
        }
        // pos.y >= Height (画面より上) の場合は、ブロックもないので「移動可能(true)」とする

        return true;
    }

    // --- ここから下はライン消去のロジック ---

    public static void ClearLines()
    {
        for (int y = 0; y < Height; y++)
        {
            if (IsLineFull(y))
            {
                DeleteLine(y);
                ShiftRowsDown(y);
                y--;
            }
        }
    }

    private static bool IsLineFull(int y)
    {
        for (int x = 0; x < Width; x++)
        {
            if (grid[x, y] == 0) return false;
        }
        return true;
    }

    // 一行消去（データも見た目も消す）
    private static void DeleteLine(int y)
    {
        for (int x = 0; x < Width; x++)
        {
            grid[x, y] = 0;

            if (visualGrid[x, y] != null)
            {
                Destroy(visualGrid[x, y]);
                visualGrid[x, y] = null;
            }
        }
    }

    // 指定した行より上のデータをすべて1段下げる（見た目も下げる）
    private static void ShiftRowsDown(int y)
    {
        for (int curY = y; curY < Height - 1; curY++)
        {
            for (int x = 0; x < Width; x++)
            {
                // データの移動
                grid[x, curY] = grid[x, curY + 1];

                // 見た目（オブジェクト）の移動
                visualGrid[x, curY] = visualGrid[x, curY + 1];

                // 下にずれてきたオブジェクトがあれば、画面上の位置も1マス下げる
                if (visualGrid[x, curY] != null)
                {
                    visualGrid[x, curY].transform.position = new Vector3(x, curY, 0);
                }
            }
        }

        // 一番上の行は空にする
        for (int x = 0; x < Width; x++)
        {
            grid[x, Height - 1] = 0;
            visualGrid[x, Height - 1] = null;
        }
    }
}