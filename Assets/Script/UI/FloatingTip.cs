using UnityEngine;
using TMPro;

public class FloatingTip : MonoBehaviour
{
    public TMP_Text tipText;
    public float floatDistance = 50f; // 上升距离
    public float duration = 1.5f;       // 显示总时间
    public float fadeTime = 0.5f;     // 渐隐时间

    private Vector3 startPos;
    private Color startColor;
    private float timer;

    void OnEnable()
    {
        startPos = transform.localPosition;
        startColor = tipText.color;
        timer = 0f;
    }

    void Update()
    {
        timer += Time.deltaTime;

        // 向上漂浮
        float progress = timer / duration;
        transform.localPosition = startPos + Vector3.up * floatDistance * progress;

        // 渐隐
        if (timer > duration - fadeTime)
        {
            float fadeProgress = 1 - (timer - (duration - fadeTime)) / fadeTime;
            tipText.color = new Color(startColor.r, startColor.g, startColor.b, fadeProgress);
        }

        // 超时销毁
        if (timer >= duration)
        {
            Destroy(gameObject);
        }
    }

    public void SetText(string message)
    {
        tipText.text = message;
    }
}
