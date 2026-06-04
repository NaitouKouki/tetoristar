using UnityEngine;

public static class Blocks
{
    // 各ミノのID定義
    public const int Empty = 0;
    public const int Wall = 1;
    public const int I = 2;
    public const int O = 3;
    public const int S = 4;
    public const int Z = 5;
    public const int J = 6;
    public const int L = 7;
    public const int T = 8;

    // ランダム生成用の一覧
    public static readonly int[] Drops = { I, O, S, Z, J, L, T };

    // 各ミノの回転ごとの相対座標（中心ブロックを(0,0)とした、残り3つのブロックの位置）
    private static readonly Vector2Int[][][] Rotations = new Vector2Int[][][]
    {
        null, // Empty (0)
        null, // Wall (1)
        
        // Iミノ (2)
        new Vector2Int[][] {
            new Vector2Int[] { new Vector2Int(-1,  0), new Vector2Int(1, 0), new Vector2Int(2, 0) },
            new Vector2Int[] { new Vector2Int( 0, -1), new Vector2Int(0, 1), new Vector2Int(0, 2) },
            new Vector2Int[] { new Vector2Int(-1,  0), new Vector2Int(1, 0), new Vector2Int(2, 0) },
            new Vector2Int[] { new Vector2Int( 0, -1), new Vector2Int(0, 1), new Vector2Int(0, 2) }
        },
        // Oミノ (3)
        new Vector2Int[][] {
            new Vector2Int[] { new Vector2Int(1, 0), new Vector2Int(0, 1), new Vector2Int(1, 1) },
            new Vector2Int[] { new Vector2Int(1, 0), new Vector2Int(0, 1), new Vector2Int(1, 1) },
            new Vector2Int[] { new Vector2Int(1, 0), new Vector2Int(0, 1), new Vector2Int(1, 1) },
            new Vector2Int[] { new Vector2Int(1, 0), new Vector2Int(0, 1), new Vector2Int(1, 1) }
        },
        // Sミノ (4)
        new Vector2Int[][] {
            new Vector2Int[] { new Vector2Int(-1, 0), new Vector2Int(1,  1), new Vector2Int(0, 1) },
            new Vector2Int[] { new Vector2Int( 1,-1), new Vector2Int(1,  0), new Vector2Int(0, 1) },
            new Vector2Int[] { new Vector2Int(-1, 0), new Vector2Int(1,  1), new Vector2Int(0, 1) },
            new Vector2Int[] { new Vector2Int( 1,-1), new Vector2Int(1,  0), new Vector2Int(0, 1) }
        },
        // Zミノ (5)
        new Vector2Int[][] {
            new Vector2Int[] { new Vector2Int(1,  0), new Vector2Int(-1, 1), new Vector2Int(0, 1) },
            new Vector2Int[] { new Vector2Int(0, -1), new Vector2Int(1,  0), new Vector2Int(1, 1) },
            new Vector2Int[] { new Vector2Int(1,  0), new Vector2Int(-1, 1), new Vector2Int(0, 1) },
            new Vector2Int[] { new Vector2Int(0, -1), new Vector2Int(1,  0), new Vector2Int(1, 1) }
        },
        // Jミノ (6)
        new Vector2Int[][] {
            new Vector2Int[] { new Vector2Int(-1,  0), new Vector2Int(1, 0), new Vector2Int(-1, 1) },
            new Vector2Int[] { new Vector2Int( 0, -1), new Vector2Int(0, 1), new Vector2Int( 1, 1) },
            new Vector2Int[] { new Vector2Int( 1, -1), new Vector2Int(1, 0), new Vector2Int(-1, 0) },
            new Vector2Int[] { new Vector2Int(-1, -1), new Vector2Int(0,-1), new Vector2Int( 0, 1) }
        },
        // Lミノ (7)
        new Vector2Int[][] {
            new Vector2Int[] { new Vector2Int(-1, 0), new Vector2Int(1,  0), new Vector2Int(1, 1) },
            new Vector2Int[] { new Vector2Int( 0,-1), new Vector2Int(1, -1), new Vector2Int(0, 1) },
            new Vector2Int[] { new Vector2Int(-1,-1), new Vector2Int(-1, 0), new Vector2Int(1, 0) },
            new Vector2Int[] { new Vector2Int( 0,-1), new Vector2Int(-1, 1), new Vector2Int(0, 1) }
        },
        // Tミノ (8)
        new Vector2Int[][] {
            new Vector2Int[] { new Vector2Int(-1, 0), new Vector2Int(1, 0), new Vector2Int(0, 1) },
            new Vector2Int[] { new Vector2Int( 0,-1), new Vector2Int(1, 0), new Vector2Int(0, 1) },
            new Vector2Int[] { new Vector2Int( 0,-1), new Vector2Int(-1,0), new Vector2Int(1, 0) },
            new Vector2Int[] { new Vector2Int( 0,-1), new Vector2Int(-1,0), new Vector2Int(0, 1) }
        }
    };

    /// <summary>
    /// 指定されたミノIDと回転数から、中心(0,0)を含む【4つのブロックすべての相対座標】を返す関数
    /// </summary>
    public static Vector2Int[] GetShape(int id, int rotationIndex)
    {
        // 範囲外のIDなら空の配列を返す安全処理
        if (id < 0 || id >= Rotations.Length || Rotations[id] == null)
            return new Vector2Int[] { Vector2Int.zero };

        // 残り3マスの相対位置を取得
        Vector2Int[] relatives = Rotations[id][rotationIndex % 4];

        // 提示コードのデータは「中心(0,0)を除いた残り3マス」で定義されているため、
        // 今のMinoScriptに合わせるために中心(0,0)を足した【4マス分】の配列にして返します
        Vector2Int[] fullShape = new Vector2Int[4];
        fullShape[0] = Vector2Int.zero; // 中心ブロック
        fullShape[1] = relatives[0];
        fullShape[2] = relatives[1];
        fullShape[3] = relatives[2];

        return fullShape;
    }
}