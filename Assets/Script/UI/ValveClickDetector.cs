using UnityEngine;

public class ValveClickDetector : MonoBehaviour
{
    [Header("点击检测设置")]
    public string targetTag = "气瓶阀门";  // 目标物体的标签
    public TipManager tipManager;           // 引用 TipManager
    public Camera mainCamera;               // 主摄像机

    void Update()
    {
        // 鼠标左键点击时检测
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);

            if (Physics.Raycast(ray, out RaycastHit hit, 100f))
            {
                // 只对具有特定标签的物体触发
                if (hit.collider.CompareTag(targetTag))
                {
                    tipManager.ShowTip("Complete the operation of the valve!");
                    Debug.Log($"点击到 {hit.collider.name} (Tag: {targetTag})，触发提示！");
                }
            }
        }
    }
}
