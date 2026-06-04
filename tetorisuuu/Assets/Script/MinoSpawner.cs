using UnityEngine;

public class MinoSpawner : MonoBehaviour
{
    [SerializeField] private GameObject minoPrefab;
    [SerializeField] private Vector3 createPos = new Vector3(5, 18, 0);

    //今動いているミノのスクリプトを記憶しておく
    private MinoScript currentMino;

    void Start()
    {
        SpawnNextMino();
    }

    // Update関数は完全に削除します！（無限ループの温床になるため）

    public void SpawnNextMino()
    {
        int randomId = Blocks.Drops[Random.Range(0, Blocks.Drops.Length)];
        GameObject newMino = Instantiate(minoPrefab, createPos, Quaternion.identity);

        // タグは一応残しておきますが、生成の判定にはもう使いません
        newMino.tag = "Mino";

        currentMino = newMino.GetComponent<MinoScript>();

        if (currentMino != null)
        {
            Vector2Int startGridPos = new Vector2Int((int)createPos.x, (int)createPos.y);

            // ★変更ポイント：自分自身（このSpawner）の情報をミノに教えてあげる
            currentMino.Initialize(startGridPos, randomId, this);
        }
    }
}