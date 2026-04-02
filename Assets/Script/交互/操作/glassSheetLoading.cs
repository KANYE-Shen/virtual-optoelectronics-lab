using System.Collections;
using Cinemachine;
using UnityEngine;
using UnityEngine.UI;

public class glassSheetLoading : MonoBehaviour
{
    [Header("绑定旋转基座")]
    [SerializeField] private GameObject JiZuo;

    [Header("绑定操作按钮")]
    [SerializeField] private Button TakeBBOButton;      // 取出BBO薄膜
    [SerializeField] private Button InstallGlassButton; // 安装叉指电极+石英玻璃片并归位

    [Header("相机位置")]
    [SerializeField] private GameObject Step_Pos1;   // 初始观察位
    [SerializeField] private GameObject Step_Pos2;   // 操作位
    [SerializeField] private GameObject NextStepPos; // 归位后相机位置

    [Header("需要切换材质的物体（可绑定多个）")]
    [SerializeField] private GameObject[] targetObjects;

    [Header("目标材质")]
    [SerializeField] private Material targetMaterial;

    private float MinHeight;
    private float MaxHeight;

    private Camera mainCamera;
    private CinemachineBrain cinemachineBrain;
    private bool isCinemachineDisabled = false;

    // 新增：初始化状态标记
    private bool isInitialized = false;

    private void Start()
    {
        // 初始化基础引用（不依赖Step1_4的状态）
        mainCamera = Camera.main;

        // 检查必要引用
        if (JiZuo == null || TakeBBOButton == null || InstallGlassButton == null ||
            Step_Pos1 == null || Step_Pos2 == null || NextStepPos == null)
        {
            Debug.LogError("Inspector 未绑定完整！");
            return;
        }

        // 初始化高度参数
        MinHeight = JiZuo.transform.position.y;
        MaxHeight = MinHeight + 1.5f;

        // 初始隐藏按钮
        TakeBBOButton.gameObject.SetActive(false);
        InstallGlassButton.gameObject.SetActive(false);

        // 注册按钮事件
        TakeBBOButton.onClick.AddListener(OnTakeBBO);
        InstallGlassButton.onClick.AddListener(OnInstallGlassAndFinish);

        // 首次检查是否可以初始化
        CheckAndInitialize();
    }

    // 新增：每帧检查Step1_4状态
    private void Update()
    {
        CheckAndInitialize();
    }

    // 新增：检查并执行初始化
    private void CheckAndInitialize()
    {
        // 如果Step1_4激活且尚未初始化
        if (StepController.Step1_4 && !isInitialized)
        {
            Debug.Log("Step1_4激活，开始执行初始化逻辑");
            PerformInitialization();
            isInitialized = true;
        }
    }

    // 新增：独立的初始化执行方法
    private void PerformInitialization()
    {
        // 相机瞬移到初始位置
        MoveCameraInstant(Step_Pos1);

        // 启动升起基座
        StartCoroutine(RaiseJiZuo());
    }

    // 新增：供外部调用的强制初始化方法
    public void ForceInitialize()
    {
        if (!isInitialized)
        {
            Debug.Log("外部调用强制初始化");
            PerformInitialization();
            isInitialized = true;
        }
    }

    private IEnumerator RaiseJiZuo()
    {
        if (!StepController.Step1_4) yield break;
        yield return new WaitForSeconds(1.2f);
        Vector3 startPos = JiZuo.transform.position;
        Vector3 endPos = new Vector3(startPos.x, MaxHeight, startPos.z);

        float elapsed = 0f;
        float duration = 2f;
        while (elapsed < duration)
        {
            JiZuo.transform.position = Vector3.Lerp(startPos, endPos, elapsed / duration);
            elapsed += Time.deltaTime;
            yield return null;
        }

        JiZuo.transform.position = endPos;
        Debug.Log("JiZuo 已上升到操作高度");

        // 开启取出BBO按钮
        TakeBBOButton.gameObject.SetActive(true);
    }

    private void OnTakeBBO()
    {
        if (!StepController.Step1_4) return;

        TakeBBOButton.gameObject.SetActive(false);
        InstallGlassButton.gameObject.SetActive(true);
        Debug.Log("BBO薄膜已取出");
        StartCoroutine(MoveCameraTo(Step_Pos2));
    }

    private void OnInstallGlassAndFinish()
    {
        if (!StepController.Step1_4) return;

        InstallGlassButton.gameObject.SetActive(false);
        Debug.Log("叉指电极 + 石英玻璃片已安装，归位基座");

        // 点击安装时，切换材质
        foreach (var obj in targetObjects)
        {
            ApplyMaterial(obj);
        }

        StartCoroutine(LowerJiZuoAndMoveCamera());
    }

    private IEnumerator LowerJiZuoAndMoveCamera()
    {
        if (!StepController.Step1_4) yield break;

        Vector3 startPos = JiZuo.transform.position;
        Vector3 endPos = new Vector3(startPos.x, MinHeight, startPos.z);
        yield return new WaitForSeconds(1);
        float elapsed = 0f;
        float duration = 2f;
        while (elapsed < duration)
        {
            JiZuo.transform.position = Vector3.Lerp(startPos, endPos, elapsed / duration);
            elapsed += Time.deltaTime;
            yield return null;
        }

        JiZuo.transform.position = endPos;
        Debug.Log("JiZuo 已归位");
        yield return StartCoroutine(MoveCameraTo(NextStepPos));

        // 完成本步骤，进入下一步
        StepController.Step1_4 = false;
        StepController.step1_5 = true;
        Debug.Log("Step3 完成，进入 Step4");

        // 重置初始化状态（以便下次Step1_4激活时重新执行）
        isInitialized = false;
    }

    private IEnumerator MoveCameraTo(GameObject target)
    {
        if (!StepController.Step1_4) yield break;
        cinemachineBrain = Camera.main.GetComponent<CinemachineBrain>();
        if (cinemachineBrain != null) { cinemachineBrain.enabled = false; isCinemachineDisabled = true; }

        float elapsedTime = 0f;
        Camera cam = Camera.main;
        Vector3 startPos = cam.transform.position;
        Quaternion startRot = cam.transform.rotation;
        Vector3 targetPos = target.transform.position;
        Quaternion targetRot = target.transform.rotation;

        float duration = 2f;
        while (elapsedTime < duration)
        {
            float progress = elapsedTime / duration;
            cam.transform.position = Vector3.Lerp(startPos, targetPos, progress);
            cam.transform.rotation = Quaternion.Slerp(startRot, targetRot, progress);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        cam.transform.position = targetPos;
        cam.transform.rotation = targetRot;

        if (isCinemachineDisabled && cinemachineBrain != null)
        {
            var activeVCam = cinemachineBrain.ActiveVirtualCamera as CinemachineVirtualCameraBase;
            if (activeVCam != null)
            {
                activeVCam.transform.position = targetPos;
                activeVCam.transform.rotation = targetRot;
            }
            cinemachineBrain.enabled = true;
            isCinemachineDisabled = false;
        }
    }

    private void MoveCameraInstant(GameObject target)
    {
        Camera.main.transform.position = target.transform.position;
        Camera.main.transform.rotation = target.transform.rotation;
    }

    // 材质替换方法
    private void ApplyMaterial(GameObject obj)
    {
        if (obj != null && targetMaterial != null)
        {
            Renderer renderer = obj.GetComponent<Renderer>();
            if (renderer != null)
            {
                renderer.material = targetMaterial;
            }
        }
    }
}
