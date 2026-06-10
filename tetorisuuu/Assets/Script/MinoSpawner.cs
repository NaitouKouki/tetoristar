using UnityEngine;
using System.Collections.Generic; // ★追加：Listを使うために必要

public class MinoSpawner : MonoBehaviour
{
    [SerializeField] private GameObject minoPrefab;
    [SerializeField] private Vector3 createPos = new Vector3(5, 18, 0);

    // 今動いているミノのスクリプトを記憶しておく
    private MinoScript currentMino;

    // ★追加：ミノの「袋（バッグ）」を管理するリスト
    private List<int> minoBag = new List<int>();

    void Start()
    {
        SpawnNextMino();
    }

    public void SpawnNextMino()
    {
        // ★変更：新しい抽選システム（7バッグ）からIDを取得する
        int randomId = GetNextMinoIdFromBag();

        // 2. 本来の生成初期位置（グリッド座標）をセット
        Vector2Int spawnGridPos = new Vector2Int((int)createPos.x, (int)createPos.y);
        bool spawnSuccess = false;

        // 3. 最大で3マス上まで、空いているスペースがないかチェックするループ
        for (int i = 0; i < 4; i++)
        {
            if (CheckSpawnPosition(spawnGridPos, randomId))
            {
                spawnSuccess = true;
                break;
            }
            spawnGridPos.y += 1;
        }

        // 4. 無事に安全な配置場所が見つかった場合のみ生成する
        if (spawnSuccess)
        {
            Vector3 finalCreatePos = new Vector3(spawnGridPos.x, spawnGridPos.y, createPos.z);
            GameObject newMino = Instantiate(minoPrefab, finalCreatePos, Quaternion.identity);

            newMino.tag = "Mino";
            currentMino = newMino.GetComponent<MinoScript>();

            if (currentMino != null)
            {
                currentMino.Initialize(spawnGridPos, randomId, this);
            }
        }
        else
        {
            // 5. どこまで上にずらしても出現できなかった場合
            Debug.LogWarning("【GAME OVER】これ以上ミノを出現させるスペースがありません！");
        }
    }

    // ★追加：7バッグ抽選を行う関数
    private int GetNextMinoIdFromBag()
    {
        // もし袋が空っぽ、あるいはリストが作られていなければ、新しく7種類を補充する
        if (minoBag == null || minoBag.Count == 0)
        {
            RefillBag();
        }

        // 袋の「一番先頭（0番目）」からミノを取り出す
        int nextId = minoBag[0];
        minoBag.RemoveAt(0); // 取り出したので袋から消去

        return nextId;
    }

    // ★追加：袋に7種類のミノをシャッフルして詰め直す関数
    private void RefillBag()
    {
        minoBag = new List<int>();

        // 1. まず元の配列（Blocks.Drops）のデータをすべて袋にコピーする
        List<int> tempPool = new List<int>(Blocks.Drops);

        // 2. コピーしたプールからランダムに取り出して、シャッフルしながら本番の袋（minoBag）に詰める
        while (tempPool.Count > 0)
        {
            int randomIndex = Random.Range(0, tempPool.Count);
            minoBag.Add(tempPool[randomIndex]);
            tempPool.RemoveAt(randomIndex); // 使ったものは一時プールから消す
        }
    }

    private bool CheckSpawnPosition(Vector2Int gridPos, int minoId)
    {
        Vector2Int[] shape = Blocks.GetShape(minoId, 0);

        foreach (var offset in shape)
        {
            Vector2Int checkPos = gridPos + offset;

            if (!StageManager.IsValidPosition(checkPos))
            {
                return false;
            }
        }
        return true;
    }
}