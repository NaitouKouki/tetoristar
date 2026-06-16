using UnityEngine;

public class PauseManager : MonoBehaviour
{
    // インスペクターで先ほど作った PausePanel をアタッチする
    [SerializeField] private GameObject pausePanel;

    private bool isPaused = false;

    void Update()
    {
        // Escキーが押されたら
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (isPaused)
            {
                ResumeGame();
            }
            else
            {
                PauseGame();
            }
        }
    }

    // ゲームを一時停止する
    void PauseGame()
    {
        pausePanel.SetActive(true); // ポーズ画面を表示
        Time.timeScale = 0f;        // ゲーム内の時間の流れを止める
        isPaused = true;
    }

    // ゲームを再開する（ポーズ画面の「再開ボタン」にもこれを設定する）
    public void ResumeGame()
    {
        pausePanel.SetActive(false); // ポーズ画面を非表示
        Time.timeScale = 1f;         // ゲーム内の時間を動かす
        isPaused = false;
    }
}