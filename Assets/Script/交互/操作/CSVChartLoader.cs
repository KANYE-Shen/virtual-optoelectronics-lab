using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using UnityEngine;
using UnityEngine.UI;
using XCharts;
using XCharts.Runtime;

public class CSVChartLoader : MonoBehaviour
{
    [Header("绑定 LineChart")]
    public LineChart lineChart;

    [Header("CSV 文件名 (不要写扩展名)")]
    public string fileName = "data_absorbing";

    [Header("绑定按钮")]
    public Button actionButton;

    [Header("绑定电机物体")]
    public Transform Machinery;

    [Header("LineChart 入场动画时长")]
    public float chartEnterDuration = 1.5f;

    [Header("电机移动时长(秒)")]
    public float motorMoveDuration = 10f;

    [Header("电机复位时长(秒)")]
    public float motorResetDuration = 0.8f;

    [Header("下一步(StepController.Step1_4摄像机位置")]
    public GameObject TargetCamerapos;

    private List<float> values = new List<float>();
    private Vector3 motorStartPos;
    private RectTransform chartRect;
    private bool isCinemachineDisabled;

    void Start()
    {
        lineChart.gameObject.SetActive(false);
        motorStartPos = Machinery.position;
        chartRect = lineChart.GetComponent<RectTransform>();

        // 初始化按钮
        if (actionButton != null)
        {
            actionButton.gameObject.SetActive(false);
            actionButton.interactable = false;
            actionButton.onClick.AddListener(OnActionButtonClick);
        }

        // 等 Step1_3 为 true 时启动逻辑
        StartCoroutine(WaitForStepAndPlay());
    }

    IEnumerator WaitForStepAndPlay()
    {
        // 等待 Step1_3 == true
        yield return new WaitUntil(() => StepController.Step1_3);

        // 先把 Chart 移到屏幕左侧外
        lineChart.gameObject.SetActive(true);
        Vector3 targetPos = chartRect.anchoredPosition;
        Vector3 startPos = targetPos + new Vector3(-Screen.width, 0, 0);
        chartRect.anchoredPosition = startPos;

        // 动画移动到屏幕内
        float elapsed = 0f;
        while (elapsed < chartEnterDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / chartEnterDuration);
            chartRect.anchoredPosition = Vector3.Lerp(startPos, targetPos, t);
            yield return null;
        }
        chartRect.anchoredPosition = targetPos;

        // Chart 完全出现后 -> 显示按钮
        if (actionButton != null)
        {
            actionButton.gameObject.SetActive(true);
            actionButton.interactable = true;
        }
    }

    void LoadCSVFromResources()
    {
        TextAsset csvFile = Resources.Load<TextAsset>(fileName);
        if (csvFile == null)
        {
            Debug.LogError("找不到文件: Resources/" + fileName);
            return;
        }

        string csvData = csvFile.text;
        ParseCSV(csvData);
    }

    void ParseCSV(string csvData)
    {
        string[] lines = csvData.Split('\n');
        values.Clear();

        for (int i = 3; i < lines.Length; i++) // 从第4行开始，跳过表头
        {
            string line = lines[i].Trim();
            if (string.IsNullOrEmpty(line)) continue;

            string[] cols = line.Split(',');
            if (cols.Length < 2) continue;

            if (float.TryParse(cols[1], out float val))
            {
                values.Add(val);
            }
        }

        // 清空旧数据
        lineChart.ClearData();
        lineChart.AddSerie<Line>();
    }

    void OnActionButtonClick()
    {
        // 点击后 -> 隐藏按钮，防止重复点击
        actionButton.interactable = false;
        actionButton.gameObject.SetActive(false);

        // 加载数据
        LoadCSVFromResources();

        // 计算点绘制间隔：保证 10 秒内画完
        float pointInterval = values.Count > 0 ? 10f / values.Count : 0.1f;

        // 启动并行流程
        StartCoroutine(AddPointsStepByStep(pointInterval));
        StartCoroutine(MoveMotorCoroutine());
    }

    IEnumerator AddPointsStepByStep(float pointInterval)
    {
        for (int i = 0; i < values.Count; i++)
        {
            lineChart.AddData(0, i, values[i]);
            yield return new WaitForSeconds(pointInterval);
        }
    }

    IEnumerator MoveMotorCoroutine()
    {
        Vector3 targetPos = motorStartPos + Vector3.left * 0.85f;
        float elapsed = 0f;

        // 向左移动 (10秒)
        while (elapsed < motorMoveDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / motorMoveDuration);
            Machinery.position = Vector3.Lerp(motorStartPos, targetPos, t);
            yield return null;
        }
        Machinery.position = targetPos;

        // 复位 (0.8秒)
        elapsed = 0f;
        while (elapsed < motorResetDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / motorResetDuration);
            Machinery.position = Vector3.Lerp(targetPos, motorStartPos, t);
            yield return null;
        }
        Machinery.position = motorStartPos;
        StepController.Step1_3 = false;
        StartCoroutine(MoveToStep1_4());
        StepController.Step1_4 = true;
    }

    IEnumerator MoveToStep1_4()
    {
        lineChart.gameObject.SetActive(false);
        yield return new WaitForSeconds(2);
        CinemachineBrain cinemachineBrain = Camera.main.GetComponent<CinemachineBrain>();
        if (cinemachineBrain != null)
        {
            cinemachineBrain.enabled = false;
            isCinemachineDisabled = true;
        }

        float elapsedTime = 0f;
        var StartPos = Camera.main.transform.position;
        var StartRot = Camera.main.transform.rotation;
        var TargetPos = TargetCamerapos.transform.position;
        var TargetRot = TargetCamerapos.transform.rotation;
        float duration = 2f;
        while (elapsedTime < duration)
        {
            float progress = elapsedTime / duration;
            Camera.main.transform.position = Vector3.Lerp(StartPos, TargetPos, progress);
            Camera.main.transform.rotation = Quaternion.Slerp(StartRot, TargetRot, progress);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        Camera.main.transform.position = TargetPos;
        Camera.main.transform.rotation = TargetRot;

        if (isCinemachineDisabled && cinemachineBrain != null)
        {
            var activeVirtualCamera = cinemachineBrain.ActiveVirtualCamera as CinemachineVirtualCameraBase;
            if (activeVirtualCamera != null)
            {
                activeVirtualCamera.transform.position = TargetPos;
                activeVirtualCamera.transform.rotation = TargetRot;
            }

            cinemachineBrain.enabled = true;
            isCinemachineDisabled = false;
        }

        Debug.Log("Camera move finished");
        yield return null;
    }
}
