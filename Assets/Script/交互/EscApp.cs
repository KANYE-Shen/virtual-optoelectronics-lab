using UnityEngine;
using UnityEngine.UI;

public class EscApp: MonoBehaviour
{
    [Header("确认退出面板 (在Inspector里绑定)")]
    public GameObject confirmPanel; // 退出确认UI面板

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            ToggleConfirmPanel(true);
        }
    }

    // 按钮调用：显示退出确认
    public void RequestQuit()
    {
        ToggleConfirmPanel(true);
    }

    // 按钮调用：确认退出
    public void ConfirmQuit()
    {
        Application.Quit();

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }

    // 按钮调用：取消退出
    public void CancelQuit()
    {
        ToggleConfirmPanel(false);
    }

    private void ToggleConfirmPanel(bool show)
    {
        if (confirmPanel != null)
        {
            confirmPanel.SetActive(show);
        }
    }
}
