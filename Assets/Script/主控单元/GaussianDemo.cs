using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using XCharts.Runtime;
using TMPro;

public class GaussianDemo : MonoBehaviour
{
    [Header("UI 组件")]
    [SerializeField] private Slider positionSlider;
    [SerializeField] private TMP_Text statusText;
    [SerializeField] private LineChart chart;

    [Header("跟随物体")]
    [SerializeField] private Transform targetObject;
    [SerializeField] private float moveRange = 5f;


    [Header("分布参数")]
    [SerializeField] private int sampleCount = 200;//生成高斯分布图中点的个数
    [SerializeField] private float thresholdRatio = 0.95f;//允许判定误差

    private List<float> xValues = new List<float>();
    private List<float> yValues = new List<float>();
    private float mu;
    private float sigma;
    private float maxY;

    // 我们按顺序添加 series，所以记录 index（初始化后固定）
    private int gaussianSeriesIndex = 0;
    private int selectedSeriesIndex = 1;
    private int thresholdSeriesIndex = 2;

    private void Start()
    {
        if (chart == null || positionSlider == null || statusText == null)
        {
            Debug.LogError("请在 Inspector 里绑定 chart / slider / statusText。");
            return;
        }

        // 随机生成 mu, sigma
        mu = Random.Range(-2f, 2f);
        sigma = Random.Range(0.5f, 1.5f);

        GenerateGaussian();
        InitChart();

        positionSlider.onValueChanged.AddListener(OnSliderChanged);

        // 初始化位置
        OnSliderChanged(positionSlider.value);
    }

    private void GenerateGaussian()
    {
        xValues.Clear();
        yValues.Clear();
        maxY = float.MinValue;

        float left = mu - 4f * sigma;
        float right = mu + 4f * sigma;

        for (int i = 0; i < sampleCount; i++)
        {
            float t = (float)i / (sampleCount - 1);
            float x = Mathf.Lerp(left, right, t);
            float y = Mathf.Exp(-Mathf.Pow(x - mu, 2f) / (2f * sigma * sigma)) / (sigma * Mathf.Sqrt(2f * Mathf.PI));

            xValues.Add(x);
            yValues.Add(y);
            if (y > maxY) maxY = y;
        }
    }

    private void InitChart()
    {
        chart.RemoveData();
        chart.AddSerie<Line>("Gaussian Curve");
        chart.AddSerie<Scatter>("Selected Point");
        chart.AddSerie<Line>("Threshold");

        for (int i = 0; i < sampleCount; i++)
        {
            chart.AddData(gaussianSeriesIndex, xValues[i], yValues[i]);
        }

        chart.AddData(thresholdSeriesIndex, xValues[0], maxY * thresholdRatio);
        chart.AddData(thresholdSeriesIndex, xValues[sampleCount - 1], maxY * thresholdRatio);
        chart.AddData(selectedSeriesIndex, mu, maxY);

        chart.RefreshChart();
    }


    private void OnSliderChanged(float value)
    {
        float left = mu - 4f * sigma;
        float right = mu + 4f * sigma;
        float x = Mathf.Lerp(left, right, value);
        float y = Mathf.Exp(-Mathf.Pow(x - mu, 2f) / (2f * sigma * sigma)) / (sigma * Mathf.Sqrt(2f * Mathf.PI));

        // 跟随曲线移动的绿色点
        var scatterSerie = chart.GetSerie<Scatter>(selectedSeriesIndex);
        if (scatterSerie != null)
        {
            scatterSerie.ClearData();
            scatterSerie.AddXYData(x, y);
        }

        bool flag = y >= maxY * thresholdRatio;
        statusText.text = flag ? " √ 成功对准" : "× 未对准";

        if (targetObject != null)
        {
            Vector3 p = targetObject.position;
            p.y = Mathf.Lerp(-moveRange, moveRange, value);
            targetObject.position = p;
        }

        chart.RefreshChart();
    }
}
