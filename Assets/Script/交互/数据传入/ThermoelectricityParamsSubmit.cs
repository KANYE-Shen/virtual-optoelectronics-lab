using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class ThermoelectricityParamsSubmit : MonoBehaviour
{
    [Header("UI 绑定")]
    [SerializeField] private Button ThermoelectricitySubmitButton;//提交按钮
    [SerializeField] private GameObject ThermoelectricityParamsPanel;//参数面板
    [SerializeField] private Button AppearThermoelectricitySys;//显示面板按钮
    [SerializeField] private Button LightPathShutter;//关灯按钮
    [SerializeField] private GameObject LightPathSys;//绑定光路系统

    [System.Serializable]
    public class ParamsControl
    {
        public string paramName; // 参数名称（与后端对应）
        public TMP_InputField paramInput;
        public Slider paramSlider;
        
        // Slider值同步到InputField
        public void SyncSliderToInput()
        {
            if (paramInput != null && paramSlider != null)
            {
                paramInput.text = paramSlider.value.ToString("F2");
            }
        }

        // InputField值同步到Slider（带合法性校验）
        public bool SyncInputToSlider()
        {
            if (paramInput != null && paramSlider != null)
            {
                if (float.TryParse(paramInput.text, out float inputValue))
                {
                    //限制输入值在 Slider 范围内
                    inputValue = Mathf.Clamp(inputValue, paramSlider.minValue, paramSlider.maxValue);

                    // 更新 Slider
                    paramSlider.value = inputValue;

                    // 修正输入框显示（确保和 Slider 一致）
                    paramInput.text = inputValue.ToString("F2");
                    return true;
                }
                else
                {
                    // 输入非法，回退到当前 Slider 值
                    SyncSliderToInput();
                    return false;
                }
            }
            return false;
        }

    }

    [Header("参数控件数组")]
    public ParamsControl[] controls;

    private void Start()
    {
        // 初始化UI状态
        ThermoelectricityParamsPanel.SetActive(false);
        ThermoelectricitySubmitButton.gameObject.SetActive(false);
        LightPathShutter.gameObject.SetActive(false);
        // 绑定按钮事件
        ThermoelectricitySubmitButton.onClick.AddListener(OnSubmit);
        AppearThermoelectricitySys.onClick.AddListener(AppearThermoelecSys);
        LightPathShutter.onClick.AddListener(() =>
        {
            LightPathSys.SetActive(false);
            LightPathShutter.gameObject.SetActive(false);
        });
                                                    
        // 初始化参数控件的双向绑定
        InitParamControls();
    }

    // 初始化参数控件的事件绑定
    private void InitParamControls()
    {
        foreach (var control in controls)
        {
            // 初始同步：Slider→Input
            control.SyncSliderToInput();

            // 绑定Slider值变化事件（实时同步到Input）
            control.paramSlider.onValueChanged.AddListener((value) =>
            {
                control.SyncSliderToInput();
            });

            // 绑定Input输入完成事件（同步到Slider）
            control.paramInput.onEndEdit.AddListener((text) =>
            {
                bool isValid = control.SyncInputToSlider();
            });
        }
    }

    private void AppearThermoelecSys()
    {
        AppearThermoelectricitySys.gameObject.SetActive(false);
        ThermoelectricityParamsPanel.SetActive(true);
        ThermoelectricitySubmitButton.gameObject.SetActive(true);
        LightPathShutter.gameObject .SetActive(true);
    }

    public void OnSubmit()
    {
        // 提交前强制同步所有Input→Slider（确保数据一致性）
        bool allValid = true;
        foreach (var control in controls)
        {
            if (!control.SyncInputToSlider())
            {
                allValid = false;
                break;
            }
        }

        if (!allValid) return;

        // 收集参数（后续逻辑不变）
        ThermoelectricityParams.paramList.Clear();
        foreach (var control in controls)
        {
            ThermoelectricityParams.paramList.Add(control.paramSlider.value);
        }

        // 打印验证日志（后续逻辑不变）
        Debug.Log("===== 数据验证日志 =====");
        for (int i = 0; i < ThermoelectricityParams.paramList.Count; i++)
        {
            Debug.Log($"参数{i + 1}: {ThermoelectricityParams.paramList[i]}");
        }
        Debug.Log("========================");

        StartCoroutine(SendParamsToPython_Thermoelectricity());
    }


    // 以下为原有代码（省略，保持不变）
    [System.Serializable]
    public class ThermoelectricityParamData
    {
        public float 参数1;
        public float 参数2;
        public float 参数3;
        public float 参数4;
    }

    private IEnumerator SendParamsToPython_Thermoelectricity()
    {
        // 手动构造 JSON 数据，使用 Inspector 里填的 paramName
        System.Text.StringBuilder sb = new System.Text.StringBuilder();
        sb.Append("{");
        for (int i = 0; i < controls.Length; i++)
        {
            string key = controls[i].paramName;
            float value = ThermoelectricityParams.paramList[i];

            // 数字直接写，不需要加引号
            sb.Append($"\"{key}\": {value}");

            if (i < controls.Length - 1)
                sb.Append(", ");
        }
        sb.Append("}");

        string jsonData = sb.ToString();
        Debug.Log("即将发送的 JSON 数据: " + jsonData);

        byte[] jsonBytes = System.Text.Encoding.UTF8.GetBytes(jsonData);

        string apiUrl = "https://virtual-lab2025.xin/compute_IV";
        UnityWebRequest request = new UnityWebRequest(apiUrl, "POST");
        request.uploadHandler = new UploadHandlerRaw(jsonBytes);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");
        request.timeout = 1200; // 合理设置超时时间（秒）

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            Debug.Log("参数上传成功！后端返回: " + request.downloadHandler.text);
            Debug.Log("你现在可以在浏览器中访问：https://virtual-lab2025.xin/result_IV 查看结果图片。");
        }
        else
        {
            Debug.LogError($"请求失败: {request.error}");
            Debug.LogError("请求失败！");
            Debug.LogError("错误类型: " + request.result);
            Debug.LogError("错误信息: " + request.error);
            Debug.LogError("服务器返回内容: " + request.downloadHandler.text);
            Debug.LogError("请求的 JSON 数据是: " + jsonData);
            Debug.LogError("请求地址: " + apiUrl);
        }

        request.Dispose();
    }


}
