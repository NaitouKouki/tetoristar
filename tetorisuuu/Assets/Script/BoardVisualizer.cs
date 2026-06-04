using UnityEngine;

public class BoardVisualizer : MonoBehaviour
{
    [SerializeField] private GameObject blockPrefab; // 見た目用の単体の四角いブロックプレハブ
    [SerializeField] private Color[] minoColors; // BlocksクラスのID(0~8)に対応する色の配列

    // 画面上のブロックオブジェクトを管理する配列
    private GameObject[,] visualGrid = new GameObject[StageManager.Width, StageManager.Height];

    void Update()
    {
        // 毎フレーム、StageManager.gridの情報に合わせて画面の絵を更新する
        RenderBoard();
    }

    private void RenderBoard()
    {
        for (int x = 0; x < StageManager.Width; x++)
        {
            for (int y = 0; y < StageManager.Height; y++)
            {
                int id = StageManager.grid[x, y];

                if (id != 0) // 0（空白）以外＝ブロックがある場合
                {
                    // まだその場所にオブジェクトが作られていなければ生成
                    if (visualGrid[x, y] == null)
                    {
                        GameObject newBlock = Instantiate(blockPrefab, new Vector3(x, y, 0), Quaternion.identity, this.transform);

                        // IDに応じた色を塗る
                        SpriteRenderer sr = newBlock.GetComponent<SpriteRenderer>();
                        if (sr != null && id < minoColors.Length)
                        {
                            sr.color = minoColors[id];
                        }

                        visualGrid[x, y] = newBlock;
                    }
                }
                else
                {
                    // 0（空白）なのにオブジェクトが残っていたら消す（ライン消去などで使う）
                    if (visualGrid[x, y] != null)
                    {
                        Destroy(visualGrid[x, y]);
                        visualGrid[x, y] = null;
                    }
                }
            }
        }
    }
}