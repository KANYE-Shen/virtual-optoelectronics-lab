//using System;
//using System.Collections;    
//using System.Diagnostics;
//using System.IO;
//using System.Threading;
//using UnityEngine;
//using UnityEngine.Networking;    
//using UnityEngine.UI;

//public class SubmitParameters : MonoBehaviour
//{
//    [SerializeField]
//    public Slider 基态吸收截面Slider;
//    public InputField 基态吸收截面InputField;

//    public Slider 单重态第一激发态吸收截面Slider;
//    public InputField 单重态第一激发态吸收截面InputField;

//    public Slider 三重态第一激发态吸收截面Slider;
//    public InputField 三重态第一激发态吸收截面InputField;

//    public Slider 双光子吸收系数Slider;
//    public InputField 双光子吸收系数InputField;

//    public Slider 单重态第一激发态寿命Slider;
//    public InputField 单重态第一激发态寿命InputField;

//    public Slider 单重态更高激发态寿命Slider;
//    public InputField 单重态更高激发态寿命InputField;

//    public Slider 三重态第一激发态寿命Slider;
//    public InputField 三重态第一激发态寿命InputField;

//    public Slider 三重态更高激发态寿命Slider;
//    public InputField 三重态更高激发态寿命InputField;

//    public Slider 系间窜跃时间Slider;
//    public InputField 系间窜跃时间InputField;


//    public Button submitButton;   
//    private void Awake()
//    {
//        // 绑定基态吸收截面事件
//        基态吸收截面Slider.onValueChanged.AddListener(On基态吸收截面Slider);
//        基态吸收截面InputField.onValueChanged.AddListener(On基态吸收截面Input);

//        // 绑定单重态第一激发态吸收截面事件
//        单重态第一激发态吸收截面Slider.onValueChanged.AddListener(On单重态第一激发态吸收截面Slider);
//        单重态第一激发态吸收截面InputField.onValueChanged.AddListener(On单重态第一激发态吸收截面Input);

//        // 绑定三重态第一激发态吸收截面事件
//        三重态第一激发态吸收截面Slider.onValueChanged.AddListener(On三重态第一激发态吸收截面Slider);
//        三重态第一激发态吸收截面InputField.onValueChanged.AddListener(On三重态第一激发态吸收截面InputField);

//        // 绑定双光子吸收系数事件
//        双光子吸收系数Slider.onValueChanged.AddListener(On双光子吸收系数Slider);
//        双光子吸收系数InputField.onValueChanged.AddListener(On双光子吸收系数Input);

//        // 绑定单重态第一激发态寿命事件
//        单重态第一激发态寿命Slider.onValueChanged.AddListener(On单重态第一激发态寿命Slider);
//        单重态第一激发态寿命InputField.onValueChanged.AddListener(On单重态第一激发态寿命Input);

//        // 绑定单重态更高激发态寿命事件
//        单重态更高激发态寿命Slider.onValueChanged.AddListener(On单重态更高激发态寿命Slider);
//        单重态更高激发态寿命InputField.onValueChanged.AddListener(On单重态更高激发态寿命Input);

//        // 绑定三重态第一激发态寿命事件
//        三重态第一激发态寿命Slider.onValueChanged.AddListener(On三重态第一激发态寿命Slider);
//        三重态第一激发态寿命InputField.onValueChanged.AddListener(On三重态第一激发态寿命Input);

//        // 绑定三重态更高激发态寿命事件
//        三重态更高激发态寿命Slider.onValueChanged.AddListener(On三重态更高激发态寿命Slider);
//        三重态更高激发态寿命InputField.onValueChanged.AddListener(On三重态更高激发态寿命Input);

//        // 绑定系间窜跃时间事件
//        系间窜跃时间Slider.onValueChanged.AddListener(On系间窜跃时间Slider);
//        系间窜跃时间InputField.onValueChanged.AddListener(On系间窜跃时间Input);

//        // 绑定提交按钮事件
//        submitButton.onClick.AddListener(OnSubmitButtonClick);//tdy7.5
//    }
//    void On基态吸收截面Slider(float v)
//    {
//        DataBank.基态吸收截面 = v;
//        基态吸收截面InputField.SetTextWithoutNotify(v.ToString());
//    }
//    void On基态吸收截面Input(string content)
//    {
//        DataBank.基态吸收截面 = float.Parse(content);
//        基态吸收截面Slider.SetValueWithoutNotify(DataBank.基态吸收截面);
//    }
//    void On单重态第一激发态吸收截面Slider(float v)
//    {
//        DataBank.单重态第一激发态吸收截面 = v;
//        单重态第一激发态吸收截面InputField.SetTextWithoutNotify(v.ToString());
//    }
//    void On单重态第一激发态吸收截面Input(string content)
//    {
//        DataBank.单重态第一激发态吸收截面 = float.Parse(content);
//        单重态第一激发态吸收截面Slider.SetValueWithoutNotify(DataBank.单重态第一激发态吸收截面);
//    }
//    void On三重态第一激发态吸收截面Slider(float v)
//    {
//        DataBank.三重态第一激发态吸收截面 = v;
//        三重态第一激发态吸收截面InputField.SetTextWithoutNotify(v.ToString());
//    }
//    // 三重态第一激发态吸收截面 Input 事件处理
//    void On三重态第一激发态吸收截面InputField(string content)
//    {
//        DataBank.三重态第一激发态吸收截面 = float.Parse(content);
//        三重态第一激发态吸收截面Slider.SetValueWithoutNotify(DataBank.三重态第一激发态吸收截面);
//    }

//    // 双光子吸收系数 Slider 事件处理
//    void On双光子吸收系数Slider(float v)
//    {
//        DataBank.双光子吸收系数 = v;
//        双光子吸收系数InputField.SetTextWithoutNotify(v.ToString());
//    }

//    // 双光子吸收系数 Input 事件处理
//    void On双光子吸收系数Input(string content)
//    {
//        DataBank.双光子吸收系数 = float.Parse(content);
//        双光子吸收系数Slider.SetValueWithoutNotify(DataBank.双光子吸收系数);
//    }

//    // 单重态第一激发态寿命 Slider 事件处理
//    void On单重态第一激发态寿命Slider(float v)
//    {
//        DataBank.单重态第一激发态寿命 = v;
//        单重态第一激发态寿命InputField.SetTextWithoutNotify(v.ToString());
//    }

//    // 单重态第一激发态寿命 Input 事件处理
//    void On单重态第一激发态寿命Input(string content)
//    {
//        DataBank.单重态第一激发态寿命 = float.Parse(content);
//        单重态第一激发态寿命Slider.SetValueWithoutNotify(DataBank.单重态第一激发态寿命);
//    }

//    // 单重态更高激发态寿命 Slider 事件处理
//    void On单重态更高激发态寿命Slider(float v)
//    {
//        DataBank.单重态更高激发态寿命 = v;
//        单重态更高激发态寿命InputField.SetTextWithoutNotify(v.ToString());
//    }

//    // 单重态更高激发态寿命 Input 事件处理
//    void On单重态更高激发态寿命Input(string content)
//    {
//        DataBank.单重态更高激发态寿命 = float.Parse(content);
//        单重态更高激发态寿命Slider.SetValueWithoutNotify(DataBank.单重态更高激发态寿命);
//    }

//    // 三重态第一激发态寿命 Slider 事件处理
//    void On三重态第一激发态寿命Slider(float v)
//    {
//        DataBank.三重态第一激发态寿命 = v;
//        三重态第一激发态寿命InputField.SetTextWithoutNotify(v.ToString());
//    }

//    // 三重态第一激发态寿命 Input 事件处理
//    void On三重态第一激发态寿命Input(string content)
//    {
//        DataBank.三重态第一激发态寿命 = float.Parse(content);
//        三重态第一激发态寿命Slider.SetValueWithoutNotify(DataBank.三重态第一激发态寿命);
//    }

//    // 三重态更高激发态寿命 Slider 事件处理
//    void On三重态更高激发态寿命Slider(float v)
//    {
//        DataBank.三重态更高激发态寿命 = v;
//        三重态更高激发态寿命InputField.SetTextWithoutNotify(v.ToString());
//    }

//    // 三重态更高激发态寿命 Input 事件处理
//    void On三重态更高激发态寿命Input(string content)
//    {
//        DataBank.三重态更高激发态寿命 = float.Parse(content);
//        三重态更高激发态寿命Slider.SetValueWithoutNotify(DataBank.三重态更高激发态寿命);
//    }

//    // 系间窜跃时间 Slider 事件处理
//    void On系间窜跃时间Slider(float v)
//    {
//        DataBank.系间窜跃时间 = v;
//        系间窜跃时间InputField.SetTextWithoutNotify(v.ToString());
//    }

//    // 系间窜跃时间 Input 事件处理
//    void On系间窜跃时间Input(string content)
//    {
//        DataBank.系间窜跃时间 = float.Parse(content);
//        系间窜跃时间Slider.SetValueWithoutNotify(DataBank.系间窜跃时间);
//    }

//    //tdy7.5
//    private void OnSubmitButtonClick()
//    {
//        // 打印所有参数到Debug日志
//        UnityEngine.Debug.Log("===== 数据验证日志 =====");
//        UnityEngine.Debug.Log($"基态吸收截面: {DataBank.基态吸收截面}");
//        UnityEngine.Debug.Log($"单重态第一激发态吸收截面: {DataBank.单重态第一激发态吸收截面}");
//        UnityEngine.Debug.Log($"三重态第一激发态吸收截面: {DataBank.三重态第一激发态吸收截面}");
//        UnityEngine.Debug.Log($"双光子吸收系数: {DataBank.双光子吸收系数}");
//        UnityEngine.Debug.Log($"单重态第一激发态寿命: {DataBank.单重态第一激发态寿命}");
//        UnityEngine.Debug.Log($"单重态更高激发态寿命: {DataBank.单重态更高激发态寿命}");
//        UnityEngine.Debug.Log($"三重态第一激发态寿命: {DataBank.三重态第一激发态寿命}");
//        UnityEngine.Debug.Log($"三重态更高激发态寿命: {DataBank.三重态更高激发态寿命}");
//        UnityEngine.Debug.Log($"系间窜跃时间: {DataBank.系间窜跃时间}");
//        UnityEngine.Debug.Log("========================");
//        //StartCoroutine(SendDataToPython());
//        StartPythonProcess();
//    }

//    private void StartPythonProcess()
//    {
//        // 1. 获取Python路径
//        string pythonExePath = GetPythonExePath();
//        if (pythonExePath == null) return;

//        // 2. 准备参数（9个参数按顺序排列）
//        string arguments = $"{DataBank.基态吸收截面} " +
//                         $"{DataBank.单重态第一激发态吸收截面} " +
//                         $"{DataBank.三重态第一激发态吸收截面} " +
//                         $"{DataBank.双光子吸收系数} " +
//                         $"{DataBank.单重态第一激发态寿命} " +
//                         $"{DataBank.单重态更高激发态寿命} " +
//                         $"{DataBank.三重态第一激发态寿命} " +
//                         $"{DataBank.三重态更高激发态寿命} " +
//                         $"{DataBank.系间窜跃时间}";

//        // 3. 打印完整命令（用于调试）
//        UnityEngine.Debug.Log($"即将执行命令: {pythonExePath} {arguments}");

//        // 4. 配置进程启动参数
//        ProcessStartInfo startInfo = new ProcessStartInfo
//        {
//            FileName = pythonExePath,
//            Arguments = arguments,
//            UseShellExecute = false,
//            RedirectStandardOutput = true,
//            RedirectStandardError = true,
//            CreateNoWindow = true,
//            WorkingDirectory = Path.GetDirectoryName(pythonExePath) // 设置工作目录
//        };

//        // 5. 启动进程
//        try
//        {
//            using (Process process = new Process { StartInfo = startInfo })
//            {
//                process.Start();

//                // 读取输出
//                string output = process.StandardOutput.ReadToEnd();
//                string error = process.StandardError.ReadToEnd();
//                process.WaitForExit();

//                // 打印结果
//                if (!string.IsNullOrEmpty(output))
//                    UnityEngine.Debug.Log($"Python输出:\n{output}");

//                if (!string.IsNullOrEmpty(error))
//                    UnityEngine.Debug.LogError($"Python错误:\n{error}");

//                // 提示结果文件位置
//                string outputDir = startInfo.WorkingDirectory;
//                UnityEngine.Debug.Log($"计算结果应保存在: {outputDir}");
//                UnityEngine.Debug.Log($"请检查目录中的 td_ta1_array.txt 和 td_tr1_array.txt");
//            }
//        }
//        catch (Exception ex)
//        {
//            UnityEngine.Debug.LogError($"进程启动失败: {ex.Message}");
//        }
//    }

//    // 自动定位Python EXE路径
//    private string GetPythonExePath()
//    {
//        // 明确指定实际文件名
//        string targetFileName = "simulator.exe";

//        // 检查的目录路径
//        string[] possibleDirs =
//        {
//            Application.streamingAssetsPath,
//            Directory.GetCurrentDirectory(),
//            Path.Combine(Application.dataPath, "..") // 上一级目录
//        };

//        // 检查所有可能目录
//        foreach (var dir in possibleDirs)
//        {
//            string fullPath = Path.Combine(dir, targetFileName);
//            if (File.Exists(fullPath))
//            {
//                UnityEngine.Debug.Log($"找到Python可执行文件: {fullPath}");
//                return fullPath;
//            }
//        }

//        // 调试输出
//        UnityEngine.Debug.LogError($"未找到{targetFileName}，已检查以下位置:\n" +
//                     $"- {Application.streamingAssetsPath}\n" +
//                     $"- {Directory.GetCurrentDirectory()}\n" +
//                     $"- {Path.Combine(Application.dataPath, "..")}");

//        return null;
//    }
//}
