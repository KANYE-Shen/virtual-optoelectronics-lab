using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class PanelMover : MonoBehaviour
{
    /// <summary>
    /// 疑问:如何控制具体哪一步panel移动呢
    /// </summary>
    [SerializeField]
    [Tooltip("在此面板上选择本panel移动的方向")]
    //在unity界面中UI全部设置成固定大小,在基本同一高度进行平移,这样就无需指定位置
    private bool isGoingLeft = true;//默认向左走

    private float PositionOffset;
    private float AnimationDuration = 0.4f;
    private void Update()
    {
        //if("当前步骤对应在StepController中的bool值"==true)
        //{
        //    StartCoroutine(AutoPanel(isGoingLeft,50));
        //}
        //else
        //{
        //    StartCoroutine(AutoPanel(!isGoingLeft, 50));
        //}
    }
    private IEnumerator AutoPanel(bool isgoingLeft, float positionOffset)
    {
        Vector2 startPos = transform.position;
        Vector2 endPos = startPos + new Vector2(isGoingLeft ? -PositionOffset : PositionOffset, 0);
        float elapsedTime = 0f;

        while (elapsedTime < AnimationDuration)
        {
            transform.position = Vector2.Lerp(startPos, endPos, elapsedTime / AnimationDuration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        transform.position = endPos;
        yield return null;
    }
}
