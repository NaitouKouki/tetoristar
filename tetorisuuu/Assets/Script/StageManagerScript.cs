using UnityEngine;
using TMPro;
using System.Collections.Generic;

public class StageManager : MonoBehaviour
{
    public const int Width = 10;
    public const int Height = 20;
    public static int[,] grid = new int[Width, Height];

    // 画面上の固定ブロック（オブジェクト）を記憶する場所
    public static GameObject[,] visualGrid = new GameObject[Width, Height];

    [SerializeField] private GameObject eraseEffectPrefab;
    [SerializeField] private GameObject lineLaserEffectPrefab;

    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private TextMeshProUGUI linesText;

    private int currentScore = 0;
    private int totalLinesCleared = 0;

    void Start()
    {
        UpdateScoreUI();
        UpdateLinesUI();
    }

    public void AddScore(int linesCleared)
    {
        int scoreToAdd = 0;

        switch (linesCleared)
        {
            case 1: scoreToAdd = 100; break;
            case 2: scoreToAdd = 300; break;
            case 3: scoreToAdd = 500; break;
            case 4: scoreToAdd = 3000; break;
            default: return;
        }

        currentScore += scoreToAdd;
        UpdateScoreUI();

        totalLinesCleared += linesCleared;
        UpdateLinesUI(); // 画面の表示を更新
    }

    public void AddDropScore(int points)
    {
        currentScore += points;
        UpdateScoreUI();
    }

    private void UpdateScoreUI()
    {
        if (scoreText != null)
        {
            scoreText.text = currentScore.ToString("D6");
        }
    }

    private void UpdateLinesUI()
    {
        if (linesText != null)
        {
            linesText.text = totalLinesCleared.ToString("D3");
        }
    }

    public static bool IsValidPosition(Vector2Int pos)
    {
        if (pos.x < 0 || pos.x >= Width || pos.y < 0) return false;

        if (pos.y < Height)
        {
            if (grid[pos.x, pos.y] != 0) return false;
        }
        return true;
    }

    public static void ClearLines()
    {
        // 揃ったラインのY座標（高さ）を一時的に記録しておくためのリスト
        List<int> clearedRows = new List<int>();

        // 1. まず、どのラインが揃っているかをチェックして記録する
        for (int y = 0; y < Height; y++)
        {
            if (IsLineFull(y))
            {
                clearedRows.Add(y); // 高さを記録

                DeleteLine(y);      // ブロックのデータと見た目を消去
                ShiftRowsDown(y);   // 上のブロックを下にずらす
                y--;                // 行がずれたのでインデックスを調整
            }
        }

        // 2. 記録したすべてのラインの位置に、エフェクトを1本ずつ生成する
        if (clearedRows.Count > 0)
        {
            StageManager instance = FindFirstObjectByType<StageManager>();
            if (instance != null && instance.lineLaserEffectPrefab != null)
            {
                foreach (int rowY in clearedRows)
                {
                    // それぞれの揃ったラインの高さ（rowY）に合わせてエフェクトを生成
                    Vector3 effectPos = new Vector3(4.5f, rowY, 0);
                    Instantiate(instance.lineLaserEffectPrefab, effectPos, Quaternion.identity);
                }
            }

            // スコアとライン数の加算処理（消した本数＝clearedRows.Count）
            if (instance != null)
            {
                instance.AddScore(clearedRows.Count);
            }
        }
    }

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
    private static bool IsLineFull(int y)
    {
        for (int x = 0; x < Width; x++)
        {
            if (grid[x, y] == 0) return false;
        }
        return true;
    }

    private static void ShiftRowsDown(int y)
    {
        for (int curY = y; curY < Height - 1; curY++)
        {
            for (int x = 0; x < Width; x++)
            {
                grid[x, curY] = grid[x, curY + 1];
                visualGrid[x, curY] = visualGrid[x, curY + 1];

                if (visualGrid[x, curY] != null)
                {
                    visualGrid[x, curY].transform.position = new Vector3(x, curY, 0);
                }
            }
        }

        for (int x = 0; x < Width; x++)
        {
            grid[x, Height - 1] = 0;
            visualGrid[x, Height - 1] = null;
        }
    }
}