using UnityEngine;

public class FadeOutEffect : MonoBehaviour
{
    [SerializeField] private float fadeDuration = 0.2f; // 消えるまでの時間（秒）
    private float timer = 0f;
    private SpriteRenderer[] spriteRenderers;
    private Color[] startColors;

    void Start()
    {
        // 自分自身と、子オブジェクトに含まれるすべての SpriteRenderer を集める
        spriteRenderers = GetComponentsInChildren<SpriteRenderer>();
        startColors = new Color[spriteRenderers.Length];

        // 生成された時点の初期の色とアルファ値を記憶し、残像らしく少し薄くする
        for (int i = 0; i < spriteRenderers.Length; i++)
        {
            startColors[i] = spriteRenderers[i].color;
            startColors[i].a *= 0.2f;
            spriteRenderers[i].color = startColors[i];
        }

        // 指定した時間が経ったらオブジェクトごと自動消滅
        Destroy(gameObject, fadeDuration);
    }

    void Update()
    {
        timer += Time.deltaTime;
        float progress = timer / fadeDuration; // 0 から 1 へ進む割合

        // 時間の経過に合わせて、ジワジワとアルファ値（透明度）を0に近づける
        for (int i = 0; i < spriteRenderers.Length; i++)
        {
            if (spriteRenderers[i] != null)
            {
                Color c = startColors[i];
                c.a = Mathf.Lerp(startColors[i].a, 0f, progress); // 徐々に透明へ
                spriteRenderers[i].color = c;
            }
        }
    }
}