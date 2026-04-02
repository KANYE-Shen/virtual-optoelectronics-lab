using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class ParamSubmitter : MonoBehaviour
{
    [System.Serializable]
    public class ParamControl
    {
        public string paramName; // 参数名称，用于错误提示
        public TMP_InputField inputField;
        public Slider slider;
        [Tooltip("是否允许负数")]
        public bool allowNegative = false;
        [Tooltip("输入框提示文本")]
        public string placeholderText = "输入数值";
    }

    public ParamControl[] controls;
    public RectTransform ParamsPanel;
    public Button PanelMoveButton;
    public Button submitButton;
    [SerializeField] private TextMeshProUGUI errorMessageText; // 错误提示文本

    [Header("动画参数")]
    public float moveDuration = 0.5f;
    private Vector2 hiddenPos;
    private Vector2 shownPos;
    private bool isShown = false;

    private void Start()
    {
        // 初始化错误提示
        if (errorMessageText != null)
        {
            errorMessageText.gameObject.SetActive(false);
        }

        // 记录位置信息
        hiddenPos = ParamsPanel.anchoredPosition;
        float panelWidth = ParamsPanel.rect.width;
        shownPos = hiddenPos + new Vector2(-panelWidth, 0);

        // 绑定按钮事件
        PanelMoveButton.onClick.AddListener(TogglePanel);
        submitButton.onClick.AddListener(OnSubmit);

        // 初始化参数控件
        InitializeParamControls();
    }

    private void InitializeParamControls()
    {
        foreach (var control in controls)
        {
            // 设置输入框占位符
            if (control.inputField.placeholder is TMP_Text placeholder)
            {
                placeholder.text = control.placeholderText;
            }

            // 初始同步：Slider -> InputField
            UpdateInputFromSlider(control);

            // Slider值变化时更新InputField
            control.slider.onValueChanged.AddListener((value) =>
            {
                UpdateInputFromSlider(control);
                HideError();
            });

            // InputField编辑结束时更新Slider
            control.inputField.onEndEdit.AddListener((text) =>
            {
                UpdateSliderFromInput(control, text);
            });

            // 输入框获得焦点时清空错误提示
            control.inputField.onSelect.AddListener((text) =>
            {
                HideError();
            });

            // 实时输入限制（可选）
            control.inputField.onValueChanged.AddListener((text) =>
            {
                // 防止输入多个小数点
                if (text.Split('.').Length > 2)
                {
                    control.inputField.text = text.Remove(text.Length - 1);
                    control.inputField.caretPosition = text.Length;
                }

                // 防止输入多个负号或负号不在开头
                if (text.Contains("-") && (text.IndexOf("-") != 0 || text.Length > 1 && text[1] == '-'))
                {
                    control.inputField.text = text.Replace("-", "");
                    if (control.allowNegative && text.IndexOf("-") == 0)
                    {
                        control.inputField.text = "-" + control.inputField.text;
                    }
                    control.inputField.caretPosition = control.inputField.text.Length;
                }

                // 如果不允许负数，移除所有负号
                if (!control.allowNegative)
                {
                    control.inputField.text = control.inputField.text.Replace("-", "");
                }
            });
        }
    }

    // 从Slider更新InputField
    private void UpdateInputFromSlider(ParamControl control)
    {
        control.inputField.text = control.slider.value.ToString("F2");
    }

    // 从InputField更新Slider，带验证
    private bool UpdateSliderFromInput(ParamControl control, string inputText)
    {
        // 空输入处理
        if (string.IsNullOrWhiteSpace(inputText))
        {
            ShowError($"参数「{control.paramName}」不能为空，请输入数值");
            UpdateInputFromSlider(control); // 重置为当前Slider值
            return false;
        }

        // 解析输入值
        if (!float.TryParse(inputText, out float value))
        {
            ShowError($"参数「{control.paramName}」输入无效，请输入有效的数字");
            UpdateInputFromSlider(control); // 重置为当前Slider值
            return false;
        }

        // 负数检查
        if (!control.allowNegative && value < 0)
        {
            ShowError($"参数「{control.paramName}」不允许为负数");
            UpdateInputFromSlider(control); // 重置为当前Slider值
            return false;
        }

        // 范围检查（使用Slider的min和max）
        if (value < control.slider.minValue || value > control.slider.maxValue)
        {
            ShowError($"参数「{control.paramName}」超出范围，请输入{control.slider.minValue}到{control.slider.maxValue}之间的数值");

            // 限制在有效范围内
            value = Mathf.Clamp(value, control.slider.minValue, control.slider.maxValue);
            control.slider.value = value;
            control.inputField.text = value.ToString("F2");
            return false;
        }

        // 所有验证通过，更新Slider
        control.slider.value = value;
        return true;
    }

    private void TogglePanel()
    {
        StopAllCoroutines();
        StartCoroutine(MovePanel(isShown ? shownPos : hiddenPos, isShown ? hiddenPos : shownPos));
        isShown = !isShown;
    }

    private IEnumerator MovePanel(Vector2 from, Vector2 to)
    {
        float elapsed = 0f;
        while (elapsed < moveDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / moveDuration);
            // 使用平滑插值使动画更自然
            t = Mathf.SmoothStep(0, 1, t);
            ParamsPanel.anchoredPosition = Vector2.Lerp(from, to, t);
            yield return null;
        }
        ParamsPanel.anchoredPosition = to;
    }

    public void OnSubmit()
    {
        // 提交前验证所有参数
        bool allValid = true;
        foreach (var control in controls)
        {
            if (!UpdateSliderFromInput(control, control.inputField.text))
            {
                allValid = false;
                // 滚动到第一个无效的输入框（如果有）
                control.inputField.Select();
                break;
            }
        }

        if (!allValid) return;

        // 收集参数
        DataBank.paramList.Clear();
        foreach (var control in controls)
        {
            DataBank.paramList.Add(control.slider.value);
        }

        // 打印验证日志
        Debug.Log("===== 数据验证日志 =====");
        for (int i = 0; i < DataBank.paramList.Count; i++)
        {
            Debug.Log($"参数{i + 1} ({controls[i].paramName}): {DataBank.paramList[i]}");
        }
        Debug.Log("========================");

       StartCoroutine(SendDataToPython());
    }

    private void ShowError(string message)
    {
        if (errorMessageText != null)
        {
            errorMessageText.text = message;
            errorMessageText.gameObject.SetActive(true);

            // 5秒后自动隐藏错误提示
            StopCoroutine("AutoHideError");
            StartCoroutine(AutoHideError(5f));
        }
        Debug.LogError(message);
    }

    private void HideError()
    {
        if (errorMessageText != null)
        {
            errorMessageText.gameObject.SetActive(false);
        }
    }

    private IEnumerator AutoHideError(float delay)
    {
        yield return new WaitForSeconds(delay);
        HideError();
    }

    private IEnumerator SendDataToPython()
    {
        // 构造 JSON 数据（使用参数名称使JSON更清晰）
        string jsonData = "{";
        for (int i = 0; i < controls.Length; i++)
        {
            jsonData += $"\"{controls[i].paramName}\": {DataBank.paramList[i]}";
            if (i < controls.Length - 1)
            {
                jsonData += ", ";
            }
        }
        jsonData += "}";

        Debug.Log("即将发送的 JSON 数据: " + jsonData);

        byte[] jsonBytes = System.Text.Encoding.UTF8.GetBytes(jsonData);

        string apiUrl = "https://virtual-lab2025.xin/compute";
        UnityWebRequest request = new UnityWebRequest(apiUrl, "POST");
        request.uploadHandler = new UploadHandlerRaw(jsonBytes);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");
        request.timeout = 1500; // 设置超时时间

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            Debug.Log("参数上传成功！可在浏览器查看结果");
            Debug.Log("参数上传成功！");
            Debug.Log("后端返回: " + request.downloadHandler.text);
            Debug.Log("你现在可以在浏览器中访问：https://virtual-lab2025.xin/result 查看结果图片。");
        }
        else
        {
            ShowError($"请求失败: {request.error}");
            Debug.LogError("请求失败！");
            Debug.LogError("错误类型: " + request.result);
            Debug.LogError("错误信息: " + request.error);
            Debug.LogError("服务器返回内容: " + request.downloadHandler.text);
            Debug.LogError("请求的 JSON 数据是: " + jsonData);
            Debug.LogError("请求地址: " + apiUrl);
        }

        // 释放资源
        request.Dispose();
    }
}
