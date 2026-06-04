using UnityEngine;
using UnityEngine.SceneManagement; // これが必要！

public class SceneChanger : MonoBehaviour
{
    // ボタンに割り当てるための関数
    public void GoToGameScene()
    {
        // "Game" という名前のシーンに切り替える
        SceneManager.LoadScene("Game");
    }
}