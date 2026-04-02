using System.Collections;
using UnityEngine;

public class ValvesControl : MonoBehaviour
{
    [Header("绑定阀门相关信息")]
    [SerializeField] private string ValvesTag; // 给所有需要旋转的阀门加一个tag

    private GameObject[] TargetValves;
    private bool isAnimating; // 防止旋转中重复执行

    private void Start()
    {
        TargetValves = GameObject.FindGameObjectsWithTag(ValvesTag);
        BoxColliderCheck(TargetValves);
    }

    private void BoxColliderCheck(GameObject[] gameObjects)
    {
        foreach (GameObject go in gameObjects)
        {
            if (!go.GetComponent<Collider>())
            {
                Debug.LogWarning("有阀门未添加碰撞器: " + go.name);
            }
        }
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0) && !isAnimating)
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                Debug.Log("左键点击, 阀门顺时针旋转一圈");
                StartCoroutine(RotateFullCircle(hit.collider.gameObject, true));
            }
        }

        if (Input.GetMouseButtonDown(1) && !isAnimating)
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                Debug.Log("右键点击, 阀门逆时针旋转一圈");
                StartCoroutine(RotateFullCircle(hit.collider.gameObject, false));
            }
        }
    }

    private IEnumerator RotateFullCircle(GameObject rotateObj, bool isClockwise)
    {
        isAnimating = true;

        float elapsedTime = 0f;
        float duration = 2f;// 转一圈的时间，可根据需要调整

        float direction = isClockwise ? 1f : -1f;
        Quaternion startRot = rotateObj.transform.rotation;

        while (elapsedTime < duration)
        {
            float t = elapsedTime / duration;
            float angle = Mathf.Lerp(0f, 360f * direction, t);
            rotateObj.transform.rotation = startRot * Quaternion.Euler(0, angle, 0);

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // 确保最后对齐到整圈
        rotateObj.transform.rotation = startRot * Quaternion.Euler(0, 360f * direction, 0);

        isAnimating = false;
    }
}
