using UnityEngine;

public class MinoScript : MonoBehaviour
{
    public Vector2Int position;
    // インデックスの代わりに現在の角度を保持
    private float currentZRotation = 0f;

    [SerializeField] private Transform[] blocks;

    // 落下用
    [SerializeField] private float fallInterval = 0.5f; // 0.1は早すぎるので少し調整
    private float fallTimer = 0f;

    // 形状データ（もう4パターン用意する必要はありません。1つだけでOK）
    private Vector2Int[] baseShape = new Vector2Int[]
    {
        new Vector2Int(0,0), new Vector2Int(-1,0), new Vector2Int(1,0), new Vector2Int(0,1)
    };

    void Start()
    {
        Initialize(new Vector2Int(5, 15));
    }

    void Update()
    {
        HandleInput();
        HandleAutoFall();
    }

    void HandleInput()
    {
        // 移動距離（5倍に底上げ）
        int moveStep = 5;

        // 左移動（5マス分）
        if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            Vector2Int direction = Vector2Int.left * moveStep;
            if (CanMove(direction)) Move(direction);
        }

        // 右移動（5マス分）
        if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            Vector2Int direction = Vector2Int.right * moveStep;
            if (CanMove(direction)) Move(direction);
        }

        // 下移動（手動）
        if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            Vector2Int direction = Vector2Int.down * moveStep;
            if (CanMove(direction)) Move(direction);
        }

        // --- 回転の切り替え ---
        // Dキーで右回転（+90度）
        if (Input.GetKeyDown(KeyCode.D))
        {
            if (CanRotate()) Rotate(90f);
        }
        // Aキーで左回転（-90度）
        if (Input.GetKeyDown(KeyCode.A))
        {
            if (CanRotate()) Rotate(-90f);
        }
    }

    // 引数で回転する向きを受け取れるように変更
    public void Rotate(float angle)
    {
        // 現在の角度に足し引きする
        currentZRotation = (currentZRotation + angle) % 360f;

        // マイナス値になった時のための補正（例：-90度を270度として扱う）
        if (currentZRotation < 0) currentZRotation += 360f;

        UpdateVisual();
    }

    void HandleAutoFall()
    {
        fallTimer += Time.deltaTime;
        if (fallTimer >= fallInterval)
        {
            fallTimer = 0f;
            if (CanMove(Vector2Int.down)) Move(Vector2Int.down);
        }
    }

    public void Initialize(Vector2Int startPos)
    {
        position = startPos;
        currentZRotation = 0f;
        UpdateVisual();
    }

    public void Move(Vector2Int direction)
    {
        position += direction*2;
        UpdateVisual();
    }

    bool CanMove(Vector2Int direction) => true; // 当たり判定は別途必要
    bool CanRotate() => true;

    void UpdateVisual()
    {
        // 1. 各ブロックの初期配置（baseShape）を適用
        for (int i = 0; i < blocks.Length; i++)
        {
            if (i < baseShape.Length)
            {
                blocks[i].localPosition = new Vector3(baseShape[i].x, baseShape[i].y, 0);
            }
        }

        // 2. 親（Mino本体）の座標と「角度」を更新
        transform.position = new Vector3(position.x, position.y, 0);
        transform.rotation = Quaternion.Euler(0, 0, currentZRotation);
    }
}