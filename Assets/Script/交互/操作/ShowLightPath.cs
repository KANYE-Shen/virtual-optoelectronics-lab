using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ShowLightPath : MonoBehaviour
{
    [Header("绑定相关物体")]
    [SerializeField] public Button ShowLightPathButton;
    [SerializeField] public GameObject LightPathSys;
    [SerializeField] public GameObject ParallelLightPath;
    [SerializeField] public GameObject SetPos;//绑定摆放光路的机位
    [SerializeField] public GameObject OPMHeader;
    [SerializeField] public Button AppearThermoelectricityExperimentButton;

    [Header("光路路径点")]
    [SerializeField] public Transform[] LightPathPoints; // 在 Inspector 拖拽绑定多个点

    private float DelayTime = 1f;
    private Camera mainCamera;
    private bool HasShownButton = false;
    private void Start()
    {
        mainCamera = Camera.main;
        if (LightPathSys != null) LightPathSys.gameObject.SetActive(false);
        if (ShowLightPathButton != null)
        {
            ShowLightPathButton.gameObject.SetActive(false);
            ShowLightPathButton.onClick.AddListener(ShowLightPathAndMove);
        }
    }
    private void Update()
    {
        if (StepController.AllowToShowLightPath && Vector3.Distance(mainCamera.transform.position, SetPos.transform.position) < 1f && !HasShownButton)
            ShowLightPathButton.gameObject.SetActive(true);
    }

    private void ShowLightPathAndMove()
    {
       ShowLightPathButton.gameObject.SetActive(false) ;
        HasShownButton = true ;
        LightPathSys.SetActive(true);
        StartCoroutine(MachinaryMove());
        for (int i = 0; i < 4; i++)
        {
            LightPathVisualization();
        }
    }

    private IEnumerator MachinaryMove()
    {
        Debug.Log("开始执行移动动画");
        float elapsedTime = 0f;
        const float AnimationDuration = 5f;

        // OPMHeader 起始和结束位置（往左移动）
        Vector3 startPos = OPMHeader.transform.position;
        Vector3 endPos = startPos + new Vector3(-0.428f, 0, 0);

        // OPMHeader 移动速度 v
        float totalDistance = Mathf.Abs(endPos.x - startPos.x);
        float speed = totalDistance / AnimationDuration;

        // ParallelLightPath 起始和结束位置（往左，速度 v/2）
        Vector3 ParallelLightPathStartPos = ParallelLightPath.transform.position;
        Vector3 ParallelLightPathEndPos = ParallelLightPathStartPos + new Vector3(-0.428f * 0.5f, 0, 0);

        // 初始缩放
        Vector3 initialScale = ParallelLightPath.transform.localScale;
        float initialXScale = initialScale.x;

        while (elapsedTime < AnimationDuration)
        {
            float progress = elapsedTime / AnimationDuration;
            float smoothProgress = Mathf.SmoothStep(0f, 1f, progress);

            // 更新 OPMHeader 位置（速度 v，方向左）
            Vector3 currentOPMPos = Vector3.Lerp(startPos, endPos, smoothProgress);
            OPMHeader.transform.position = currentOPMPos;

            // 更新 ParallelLightPath 质心位置（速度 v/2，方向左）
            Vector3 currentLightPathPos = Vector3.Lerp(ParallelLightPathStartPos, ParallelLightPathEndPos, smoothProgress);
            ParallelLightPath.transform.position = currentLightPathPos;

            // 光柱长度随时间线性增加
            float targetScaleX = initialXScale + (speed * elapsedTime * 0.5f);

            // 更新缩放
            ParallelLightPath.transform.localScale = new Vector3(
                targetScaleX,
                initialScale.y,
                initialScale.z
            );

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // 最终位置 & 缩放校准
        OPMHeader.transform.position = endPos;
        ParallelLightPath.transform.position = ParallelLightPathEndPos;
        ParallelLightPath.transform.localScale = new Vector3(
            initialXScale + (speed * AnimationDuration * 0.5f), // 最终长度
            initialScale.y,
            initialScale.z
        );

        Debug.Log("动画执行完成");
    }

    private void LightPathVisualization()
    {
        StartCoroutine(SpawnLightPathRoutine());
        if(!AppearThermoelectricityExperimentButton.gameObject.activeSelf )
        {
            AppearThermoelectricityExperimentButton.gameObject.SetActive(true);
        }

    }

    private IEnumerator SpawnLightPathRoutine()
    {
        // 循环 3 次
        for (int cycle = 0; cycle < 3; cycle++)
        {
            // 从 Resources 加载 prefab
            GameObject prefab = Resources.Load<GameObject>("Sphere");
            if (prefab == null)
            {
                Debug.LogError("Resources 中未找到 LightPrefab，请检查路径和名字！");
                yield break;
            }

            // 创建实例
            GameObject obj = Instantiate(prefab, LightPathPoints[0].position, Quaternion.identity);

            // 按顺序依次移动
            for (int i = 0; i < LightPathPoints.Length - 1; i++)
            {
                Vector3 startPos = LightPathPoints[i].position;
                Vector3 endPos = LightPathPoints[i + 1].position;

                float duration = 1.5f; // 每段移动时间，可以改
                float elapsed = 0f;

                while (elapsed < duration)
                {
                    obj.transform.position = Vector3.Lerp(startPos, endPos, elapsed / duration);
                    elapsed += Time.deltaTime;
                    yield return null;
                }

                obj.transform.position = endPos;
            }

            // 到达终点后销毁
            Destroy(obj);

            // 可以加一点延迟再开始下一轮
            yield return new WaitForSeconds(DelayTime);
        }
    }
}
