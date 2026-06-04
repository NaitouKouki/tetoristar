using UnityEngine;
using static Unity.Collections.AllocatorManager;

public class MinoScript : MonoBehaviour
{
    // 以前の MinoData の代わりに、このミノの「ID（数字）」を持つ
    public int minoId;

    private Transform[] blocks;
    public Vector2Int position;
    private int rotationIndex = 0;
    private float fallTimer = 0f;
    private float lockTimer = 0f;
    private bool isGrounded = false;
    private MinoSpawner spawner;

    void Start()
    {
        // 念のためStartでも取得処理を走らせるが、中身を「確定」させるために共通化
        SetupBlocks();
    }

    public void Initialize(Vector2Int startPos, int id, MinoSpawner spawner)
    {
        this.minoId = id;
        this.position = startPos;
        this.rotationIndex = 0;
        this.spawner = spawner; // 連絡先を覚える

        SetupBlocks();
        UpdateVisual();
    }

    // 新しく追加：子要素の取得を100%確実に行うための共通関数
    private void SetupBlocks()
    {
        // すでに配列が作られていて、数が一致しているなら何もしない（二重処理防止）
        if (blocks != null && blocks.Length == transform.childCount) return;

        // 子要素（4つのブロック）を確実に取得して配列に格納する
        blocks = new Transform[transform.childCount];
        for (int i = 0; i < transform.childCount; i++)
        {
            blocks[i] = transform.GetChild(i);
        }
    }

    void Update()
    {
        HandleInput();
        HandleAutoFall();
    }

    private void HandleInput()
    {
        if (Input.GetKeyDown(KeyCode.LeftArrow)) TryMove(Vector2Int.left);
        if (Input.GetKeyDown(KeyCode.RightArrow)) TryMove(Vector2Int.right);
        if (Input.GetKeyDown(KeyCode.DownArrow)) TryMove(Vector2Int.down);
        if (Input.GetKeyDown(KeyCode.D)) TryRotate(1);
        if (Input.GetKeyDown(KeyCode.A)) TryRotate(-1);
    }

    private void HandleAutoFall()
    {
        if (CanMove(position + Vector2Int.down, rotationIndex))
        {
            isGrounded = false;
            fallTimer += Time.deltaTime;
            if (fallTimer >= 0.5f) { fallTimer = 0f; Move(Vector2Int.down); }
        }
        else
        {
            isGrounded = true;
            lockTimer += Time.deltaTime;
            if (lockTimer >= 0.5f) LockMino();
        }
    }

    private void TryMove(Vector2Int dir) { if (CanMove(position + dir, rotationIndex)) { position += dir; UpdateVisual(); } }

    private void TryRotate(int dir)
    {
        int next = (rotationIndex + dir + 4) % 4;
        if (CanMove(position, next)) { rotationIndex = next; UpdateVisual(); }
    }

    private void Move(Vector2Int dir) { position += dir; UpdateVisual(); }

    private void UpdateVisual()
    {
        transform.rotation = Quaternion.identity;
        transform.position = new Vector3(position.x, position.y, 0);

        Vector2Int[] shape = Blocks.GetShape(minoId, rotationIndex);

        for (int i = 0; i < blocks.Length; i++)
        {
            blocks[i].localPosition = new Vector3(shape[i].x, shape[i].y, 0);

            // ★追加：ID（数字）に応じて色（Color）を直接指定して塗り替える
            SpriteRenderer sr = blocks[i].GetComponent<SpriteRenderer>();
            if (sr != null)
            {
                sr.color = GetMinoColor(minoId);
            }
        }
    }

    private Color GetMinoColor(int id)
    {
        switch (id)
        {
            case Blocks.I: return Color.cyan;    // 水色
            case Blocks.O: return Color.yellow;      // 黄色
            case Blocks.S: return Color.forestGreen;      // 緑
            case Blocks.Z: return Color.red;      // 赤
            case Blocks.J: return Color.blue;      // 青
            case Blocks.L: return new Color(1f, 0.5f, 0f); // オレンジ (RGBで指定)
            case Blocks.T: return new Color(0.5f, 0f, 0.5f); // 紫
            default: return Color.white;
        }
    }

    // 移動可能か調べる関数（計算だけで行うため、物理すり抜けは100%起きなくなります）
    private bool CanMove(Vector2Int newPos, int targetRotIndex)
    {
        // 新Blocksクラスから次の形状の座標群を取得
        Vector2Int[] shape = Blocks.GetShape(minoId, targetRotIndex);

        foreach (var offset in shape)
        {
            Vector2Int checkPos = newPos + offset;

            // StageManagerに、そのマスが空いているか（0かどうか）問い合わせる
            if (!StageManager.IsValidPosition(checkPos))
            {
                return false; // 壁か床、または配置済みブロックにぶつかる
            }
        }
        return true;
    }

    private void LockMino()
    {
        Vector2Int[] shape = Blocks.GetShape(minoId, rotationIndex);

        for (int i = 0; i < blocks.Length; i++)
        {
            int gridX = position.x + shape[i].x;
            int gridY = position.y + shape[i].y;

            if (gridX >= 0 && gridX < StageManager.Width && gridY >= 0 && gridY < StageManager.Height)
            {
                // 1. 配列に数字を記録
                StageManager.grid[gridX, gridY] = minoId;

                // 2. タグを外す
                blocks[i].gameObject.tag = "LockedMino";

                // もし子ブロック自体に間違って MinoScript が付いていた場合、ここで完全に削除する
                // これにより、床に置かれたブロックが勝手に動き回ったりエラーを吐くのを防ぎます。
                MinoScript oldScript = blocks[i].GetComponent<MinoScript>();
                if (oldScript != null)
                {
                    Destroy(oldScript);
                }

                // 3. 親から切り離して置き去りにする
                blocks[i].SetParent(null);
                blocks[i].name = $"FixedBlock_{gridX}_{gridY}";
            }
        }

        // 次の生成を依頼
        if (spawner != null)
        {
            spawner.SpawnNextMino();
        }

        // 最後に操作用の親オブジェクトを消去
        Destroy(gameObject);
    }

    private void OnDrawGizmos()
    {
        Vector2Int[] shape = Blocks.GetShape(minoId, rotationIndex);
        Gizmos.color = Color.red;
        foreach (var offset in shape)
        {
            Vector3 worldPos = new Vector3(position.x + offset.x, position.y + offset.y, 0);
            Gizmos.DrawWireCube(worldPos, Vector3.one);
        }
    }
}