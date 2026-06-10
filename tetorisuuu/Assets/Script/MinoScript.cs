using UnityEngine;

public class MinoScript : MonoBehaviour
{
    public int minoId;
    public Vector2Int position;

    private Transform[] blocks;
    private int rotationIndex = 0;
    private float fallTimer = 0f;
    private float lockTimer = 0f;
    private bool isGrounded = false;
    private MinoSpawner spawner;

    [SerializeField] private GameObject ghostPrefab;
    private GameObject ghostInstance;

    // 左右・下長押し（DAS）の設定定数
    private const float DAS_DELAY = 0.2f;  // 長押しと判定されるまでの溜め時間（秒）
    private const float DAS_SPEED = 0.04f; // 高速移動時の移動間隔（秒）

    private float dasTimer = 0f;
    private float dasMoveTimer = 0f;
    private Vector2Int currentDir = Vector2Int.zero; // 現在ホールド中の移動方向

    //private static readonly Vector2Int[,] WallKickDataNormal = new Vector2Int[4, 4][] 
    //{

    //};

    void Start()
    {
        SetupBlocks();
        CreateGhost();
    }

    public void Initialize(Vector2Int startPos, int id, MinoSpawner spawner)
    {
        this.minoId = id;
        this.position = startPos;
        this.rotationIndex = 0;
        this.spawner = spawner;

        SetupBlocks();
        UpdateVisual();
        CreateGhost();
    }

    private void SetupBlocks()
    {
        if (blocks != null && blocks.Length == transform.childCount) return;

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

    // --- 入力システム関連（リファクタリングのメイン） ---

    private void HandleInput()
    {
        // 1. 単発入力（回転、即落下）の処理
        HandleSinglePressInput();

        // 2. 方向キーが「新しく押された瞬間」の検知
        HandleDirectionKeyDown();

        // 3. 方向キーが「押し続けられている間」の長押し高速移動処理
        HandleDirectionKeyHolding();
    }

    private void HandleSinglePressInput()
    {
        if (Input.GetKeyDown(KeyCode.D)) TryRotate(1);
        if (Input.GetKeyDown(KeyCode.A)) TryRotate(-1);
        if (Input.GetKeyDown(KeyCode.Space)) HardDrop();
    }

    private void HandleDirectionKeyDown()
    {
        if (Input.GetKeyDown(KeyCode.LeftArrow)) StartDasMove(Vector2Int.left);
        if (Input.GetKeyDown(KeyCode.RightArrow)) StartDasMove(Vector2Int.right);
        if (Input.GetKeyDown(KeyCode.DownArrow)) StartDasMove(Vector2Int.down);
    }

    private void StartDasMove(Vector2Int dir)
    {
        currentDir = dir;
        TryMove(currentDir);
        dasTimer = 0f;
        dasMoveTimer = 0f;
    }

    private void HandleDirectionKeyHolding()
    {
        if (currentDir == Vector2Int.zero) return;

        // 現在記憶している方向に合わせたキーがホールドされているか確認
        bool isHolding = (currentDir == Vector2Int.left && Input.GetKey(KeyCode.LeftArrow)) ||
                         (currentDir == Vector2Int.right && Input.GetKey(KeyCode.RightArrow)) ||
                         (currentDir == Vector2Int.down && Input.GetKey(KeyCode.DownArrow));

        if (isHolding)
        {
            dasTimer += Time.deltaTime;

            // 溜め時間を超えたら高速移動モード
            if (dasTimer >= DAS_DELAY)
            {
                dasMoveTimer += Time.deltaTime;

                if (dasMoveTimer >= DAS_SPEED)
                {
                    dasMoveTimer = 0f;
                    TryMove(currentDir);
                }
            }
        }
        else
        {
            // キーが離されたら状態をリセット
            currentDir = Vector2Int.zero;
        }
    }

    // --- 自動落下・移動ロジック関連 ---

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

    private void HardDrop()
    {
        // ゴーストの位置（最下点）までワープして即固定
        position = GetGhostPosition();
        UpdateVisual();
        LockMino();
    }

    private void TryMove(Vector2Int dir)
    {
        if (CanMove(position + dir, rotationIndex))
        {
            position += dir;
            UpdateVisual();
        }
    }

    private void TryRotate(int dir)
    {
        // 1. 次の回転状態を計算
        int nextRotationIndex = (rotationIndex + dir + 4) % 4;

        // 2. 壁蹴り（ずらし）のテストパターンを定義
        // (0,0) 本来の位置
        // (-1,0) 左に1マスずらす
        // (1,0)  右に1マスずらす
        // (0,1)  上に1マスずらす（床蹴り）
        // (-1,1) 左上にずらす
        // (1,1)  右上にずらす
        Vector2Int[] kickOffsets = new Vector2Int[]
        {
            new Vector2Int(0, 0),
            new Vector2Int(-1, 0),
            new Vector2Int(1, 0),
            new Vector2Int(0, 1),
            new Vector2Int(-1, 1),
            new Vector2Int(1, 1)
        };

        // 3. 順番にテストを実行し、最初にすり抜けなかった場所で確定する
        foreach (Vector2Int offset in kickOffsets)
        {
            Vector2Int testPosition = position + offset;

            if (CanMove(testPosition, nextRotationIndex))
            {
                // 成功したら位置と回転を更新して終了
                position = testPosition;
                rotationIndex = nextRotationIndex;
                UpdateVisual();
                return; // 壁蹴り成功！
            }
        }

        // 全てのオフセットがダメだった場合は回転しない（何もしない）
    }

    private void Move(Vector2Int dir)
    {
        position += dir;
        UpdateVisual();
    }

    private bool CanMove(Vector2Int newPos, int targetRotIndex)
    {
        Vector2Int[] shape = Blocks.GetShape(minoId, targetRotIndex);

        foreach (var offset in shape)
        {
            Vector2Int checkPos = newPos + offset;
            if (!StageManager.IsValidPosition(checkPos)) return false;
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
                StageManager.grid[gridX, gridY] = minoId;
                blocks[i].gameObject.tag = "Untagged";

                MinoScript oldScript = blocks[i].GetComponent<MinoScript>();
                if (oldScript != null) Destroy(oldScript);

                blocks[i].SetParent(null);
                blocks[i].name = $"FixedBlock_{gridX}_{gridY}";

                StageManager.visualGrid[gridX, gridY] = blocks[i].gameObject;
            }
        }

        StageManager.ClearLines();

        if (spawner != null) spawner.SpawnNextMino();
        Destroy(gameObject);
    }

    // --- 見た目・予測位置（ゴースト）関連 ---

    private void UpdateVisual()
    {
        transform.rotation = Quaternion.identity;
        transform.position = new Vector3(position.x, position.y, 0);

        Vector2Int[] shape = Blocks.GetShape(minoId, rotationIndex);

        for (int i = 0; i < blocks.Length; i++)
        {
            blocks[i].localPosition = new Vector3(shape[i].x, shape[i].y, 0);

            SpriteRenderer sr = blocks[i].GetComponent<SpriteRenderer>();
            if (sr != null) sr.color = GetMinoColor(minoId);
        }

        UpdateGhostPosition();
    }

    private Color GetMinoColor(int id)
    {
        switch (id)
        {
            case Blocks.I: return Color.cyan;
            case Blocks.O: return Color.yellow;
            case Blocks.S: return Color.green;
            case Blocks.Z: return Color.red;
            case Blocks.J: return Color.blue;
            case Blocks.L: return Color.orange;
            case Blocks.T: return Color.magenta;
            default: return Color.white;
        }
    }

    private void CreateGhost()
    {
        if (ghostPrefab != null && ghostInstance == null)
        {
            ghostInstance = Instantiate(ghostPrefab, Vector3.zero, Quaternion.identity);
            UpdateGhostPosition();
        }
    }

    private void UpdateGhostPosition()
    {
        if (ghostInstance == null) return;

        Vector2Int[] shape = Blocks.GetShape(minoId, rotationIndex);
        SpriteRenderer[] ghostBlockRenderers = ghostInstance.GetComponentsInChildren<SpriteRenderer>();

        for (int i = 0; i < ghostBlockRenderers.Length; i++)
        {
            if (i < shape.Length)
            {
                ghostBlockRenderers[i].transform.localPosition = new Vector3(shape[i].x, shape[i].y, 0);
                Color c = GetMinoColor(minoId);
                c.a = 0.3f;
                ghostBlockRenderers[i].color = c;
            }
        }

        ghostInstance.transform.position = new Vector3(GetGhostPosition().x, GetGhostPosition().y, 0);
    }

    // ★リファクタ抽出：最下点の座標計算を一箇所に集約（HardDropとゴースト位置更新で共有）
    private Vector2Int GetGhostPosition()
    {
        Vector2Int ghostPos = position;
        while (true)
        {
            Vector2Int nextPos = ghostPos + new Vector2Int(0, -1);
            if (CanMove(nextPos, rotationIndex))
            {
                ghostPos = nextPos;
            }
            else
            {
                break;
            }
        }
        return ghostPos;
    }

    private void OnDestroy()
    {
        if (ghostInstance != null) Destroy(ghostInstance);
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