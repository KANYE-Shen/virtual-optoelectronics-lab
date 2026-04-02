using UnityEngine;
using TMPro;

public class TipManager : MonoBehaviour
{
    public GameObject tipPrefab;         // 拖入 TMP_Text 预制体
    public Transform canvasTransform;    // 拖入主 Canvas
    public float floatSpeed = 30f;       // 上浮速度
    public float duration = 2f;          // 总持续时间
    public float fadeTime = 0.5f;        // 渐隐时间

    public void ShowTip(string message)
    {
        if (tipPrefab == null || canvasTransform == null)
        {
            Debug.LogError("TipManager: 预制体或Canvas未绑定！");
            return;
        }

        GameObject tip = Instantiate(tipPrefab, canvasTransform);
        TMP_Text text = tip.GetComponentInChildren<TMP_Text>();
        if (text != null)
            text.text = message;

        // 启动协程实现上浮渐隐效果
        StartCoroutine(FloatAndFade(tip));
    }

    private System.Collections.IEnumerator FloatAndFade(GameObject tip)
    {
        CanvasGroup cg = tip.GetComponent<CanvasGroup>();
        if (cg == null) cg = tip.AddComponent<CanvasGroup>();
        cg.alpha = 1f;

        float timer = 0f;
        RectTransform rt = tip.GetComponent<RectTransform>();

        // 初始随机偏移（避免多条重叠）
        Vector3 offset = new Vector3(Random.Range(-30f, 30f), 0, 0);
        rt.anchoredPosition += (Vector2)offset;

        while (timer < duration)
        {
            timer += Time.deltaTime;
            rt.anchoredPosition += Vector2.up * floatSpeed * Time.deltaTime;

            // 渐隐阶段
            if (timer > duration - fadeTime)
                cg.alpha = Mathf.Lerp(1f, 0f, (timer - (duration - fadeTime)) / fadeTime);

            yield return null;
        }

        Destroy(tip);
    }
}
