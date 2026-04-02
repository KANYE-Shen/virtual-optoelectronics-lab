using System;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class DataBankTextBinder : MonoBehaviour
{
    [Serializable]
    public class FloatBinding
    {
        public string variableName;      // 对应 DataBank 中的变量名
        public TMP_Text targetText;      // 要更新的 Text 对象
        [HideInInspector] public float lastValue;  // 上一次的值
        [HideInInspector] public string baseText;  // 初始文本内容（前缀）
    }

    [Header("绑定关系配置")]
    public List<FloatBinding> floatBindings = new List<FloatBinding>();

    void Start()
    {
        // 在开始时记录每个Text的原始内容
        foreach (var binding in floatBindings)
        {
            if (binding.targetText != null)
                binding.baseText = binding.targetText.text; // 例如“溅射电流: ”
        }
    }

    void Update()
    {
        foreach (var binding in floatBindings)
        {
            // 先尝试获取字段
            var field = typeof(DataBank).GetField(binding.variableName,
                System.Reflection.BindingFlags.Static |
                System.Reflection.BindingFlags.Public |
                System.Reflection.BindingFlags.NonPublic);

            float currentValue = float.NaN;

            if (field != null && field.FieldType == typeof(float))
            {
                currentValue = (float)field.GetValue(null);
            }
            else
            {
                // 如果没找到字段，就尝试找属性
                var prop = typeof(DataBank).GetProperty(binding.variableName,
                    System.Reflection.BindingFlags.Static |
                    System.Reflection.BindingFlags.Public |
                    System.Reflection.BindingFlags.NonPublic);

                if (prop != null && prop.PropertyType == typeof(float))
                {
                    currentValue = (float)prop.GetValue(null, null);
                }
            }

            // 如果成功取到 float 值（非 NaN）
            if (!float.IsNaN(currentValue))
            {
                if (!Mathf.Approximately(currentValue, binding.lastValue))
                {
                    binding.lastValue = currentValue;

                    if (binding.targetText != null)
                    {
                        string formattedValue = Mathf.RoundToInt(currentValue).ToString("D3");
                        binding.targetText.text = $"{binding.baseText}{formattedValue}";
                    }
                }
            }

        }
    }
}
