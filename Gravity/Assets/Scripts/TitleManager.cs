using UnityEngine;
using UnityEngine.SceneManagement;

public class TitleManager : MonoBehaviour
{
    [SerializeField] private GameObject howToPanel;

    // ゲーム開始
    public void StartGame()
    {
        SceneManager.LoadScene("Game");
    }

    // 操作説明を開く
    public void OpenHowTo()
    {
        howToPanel.SetActive(true);
    }

    // 操作説明を閉じる
    public void CloseHowTo()
    {
        howToPanel.SetActive(false);
    }

    // ゲーム終了
    public void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}