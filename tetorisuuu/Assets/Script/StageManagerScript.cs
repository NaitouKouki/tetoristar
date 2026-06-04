using UnityEngine;

public class StageManager : MonoBehaviour
{
    public const int Width = 20;
    public const int Height = 20;

    //Transform[,] から int[,] に変更
    // 0: 空白, 1: 壁, 2~8: 各ミノのブロック が入るマップになる
    public static int[,] grid = new int[Width, Height];

    public static bool IsValidPosition(Vector2Int pos)
    {
        // 左右の壁、および「床（0未満）」のチェック
        if (pos.x < 0 || pos.x >= Width || pos.y < 0)
        {
            return false; // ここは絶対にアウト
        }

        // ★修正ポイント：画面内、または画面より上（出現位置）のチェック
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
}