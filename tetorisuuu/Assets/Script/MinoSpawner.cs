using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class MinoSpawner : MonoBehaviour
{
    [SerializeField] private GameObject minoPrefab;
    [SerializeField] private Vector3 createPos = new Vector3(5, 18, 0);

    [SerializeField] private Transform holdContainer;
    [SerializeField] private List<Transform> nextContainers = new List<Transform>();

    [SerializeField] private AudioClip holdSound;
    [SerializeField] private AudioClip hardDropSound;
    private AudioSource audioSource;

    private MinoScript currentMino;
    private List<int> minoBag = new List<int>();

    private int holdMinoId = 0;
    private bool hasHeld = false;

    private GameObject holdDisplayObject;

    private List<GameObject> nextDisplayObjects = new List<GameObject>();

    void Start()
    {

        audioSource = GetComponent<AudioSource>();
        if (audioSource == null) audioSource = gameObject.AddComponent<AudioSource>();
        
        RefillBag();
        SpawnNextMino();
    }

    public void SpawnNextMino()
    {
        int nextId = GetNextMinoIdFromBag();
        SpawnSpecificMino(nextId);

        hasHeld = false;

        UpdateNextDisplay();
    }

    public void PlayHoldSound()
    {
        //オーディオソースとホールド音が設定されていてかつ、ホールドが可能な場合にのみ音を再生する
        if (audioSource != null && holdSound != null && !hasHeld)
        {
            audioSource.PlayOneShot(holdSound);
        }
    }

    public void SpawnSpecificMino(int minoId)
    {
        Vector2Int spawnGridPos = new Vector2Int((int)createPos.x, (int)createPos.y);
        bool spawnSuccess = false;

        for (int i = 0; i < 4; i++)
        {
            if (CheckSpawnPosition(spawnGridPos, minoId))
            {
                spawnSuccess = true;
                break;
            }
            spawnGridPos.y += 1;
        }

        if (spawnSuccess)
        {
            Vector3 finalCreatePos = new Vector3(spawnGridPos.x, spawnGridPos.y, createPos.z);
            GameObject newMino = Instantiate(minoPrefab, finalCreatePos, Quaternion.identity);

            newMino.tag = "Mino";
            currentMino = newMino.GetComponent<MinoScript>();

            if (currentMino != null)
            {
                currentMino.Initialize(spawnGridPos, minoId, this);
            }
        }
        else
        {
            Debug.LogError("【GAME OVER】これ以上ミノを出現させるスペースがありません！");
        }
    }

    public void HoldCurrentMino(int currentId)
    {
        if (hasHeld) return;

        if (currentMino != null)
        {
            Destroy(currentMino.gameObject);
        }

        if (holdMinoId == 0)
        {
            holdMinoId = currentId;
            SpawnNextMino();
        }
        else
        {
            int temp = holdMinoId;
            holdMinoId = currentId;
            SpawnSpecificMino(temp);
        }

        hasHeld = true;
        UpdateHoldDisplay();
    }

    private void UpdateNextDisplay()
    {

        foreach (GameObject obj in nextDisplayObjects)
        {
            if (obj != null) Destroy(obj);
        }
        nextDisplayObjects.Clear();


        while (minoBag.Count < nextContainers.Count + 1)
        {
            RefillBag();
        }

        for (int i = 0; i < nextContainers.Count; i++)
        {
            if (nextContainers[i] != null)
            {
                CreateNextDisplayObject(minoBag[i], nextContainers[i]);
            }
        }
    }

    private void CreateNextDisplayObject(int minoId, Transform container)
    {
        GameObject nextObj = Instantiate(minoPrefab, container.position, Quaternion.identity);
        nextObj.transform.SetParent(container);

        // キー入力や落下を防ぐためにスクリプトを即消去
        MinoScript minoScript = nextObj.GetComponent<MinoScript>();
        if (minoScript != null) Destroy(minoScript);

        // 形状と色のセットアップ（初期回転Index=0）
        Vector2Int[] shape = Blocks.GetShape(minoId, 0);
        for (int i = 0; i < nextObj.transform.childCount; i++)
        {
            Transform block = nextObj.transform.GetChild(i);
            block.localPosition = new Vector3(shape[i].x, shape[i].y, 0);

            SpriteRenderer sr = block.GetComponent<SpriteRenderer>();
            if (sr != null) sr.color = GetMinoColor(minoId);
        }

        // 記憶リストに保存
        nextDisplayObjects.Add(nextObj);
    }

    private void UpdateHoldDisplay()
    {
        if (holdDisplayObject != null) Destroy(holdDisplayObject);
        if (holdMinoId == 0 || holdContainer == null) return;

        holdDisplayObject = Instantiate(minoPrefab, holdContainer.position, Quaternion.identity);
        holdDisplayObject.transform.SetParent(holdContainer);

        MinoScript minoScript = holdDisplayObject.GetComponent<MinoScript>();
        if (minoScript != null) Destroy(minoScript);

        Vector2Int[] shape = Blocks.GetShape(holdMinoId, 0);
        for (int i = 0; i < holdDisplayObject.transform.childCount; i++)
        {
            Transform block = holdDisplayObject.transform.GetChild(i);
            block.localPosition = new Vector3(shape[i].x, shape[i].y, 0);

            SpriteRenderer sr = block.GetComponent<SpriteRenderer>();
            if (sr != null) sr.color = GetMinoColor(holdMinoId);
        }
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
            case Blocks.L: return new Color(1f, 0.5f, 0f);
            case Blocks.T: return new Color(0.5f, 0f, 0.5f);
            default: return Color.white;
        }
    }

    private int GetNextMinoIdFromBag()
    {
        if (minoBag == null || minoBag.Count == 0)
        {
            RefillBag();
        }

        int nextId = minoBag[0];
        minoBag.RemoveAt(0);
        return nextId;
    }

    private void RefillBag()
    {
        if (minoBag == null) minoBag = new List<int>();

        List<int> tempPool = new List<int>(Blocks.Drops);

        while (tempPool.Count > 0)
        {
            int randomIndex = Random.Range(0, tempPool.Count);
            minoBag.Add(tempPool[randomIndex]);
            tempPool.RemoveAt(randomIndex);
        }
    }

    private bool CheckSpawnPosition(Vector2Int gridPos, int minoId)
    {
        Vector2Int[] shape = Blocks.GetShape(minoId, 0);

        foreach (var offset in shape)
        {
            Vector2Int checkPos = gridPos + offset;
            if (!StageManager.IsValidPosition(checkPos)) return false;
        }
        return true;
    }

    public void PlayHardDropSound()
    {
        if (audioSource != null && hardDropSound != null)
        {
            audioSource.PlayOneShot(hardDropSound);
        }
    }
}