//using System.Collections;
//using Cinemachine;
//using UnityEngine;

///// <summary>
///// 材料圆片拖拽加载脚本，负责处理拖拽交互及腔门动画响应
///// </summary>
//public class SheetLoading : MonoBehaviour
//{
//    // 引用 tracedDrag 脚本实例
//    [SerializeField] private tracedDrag tracedDrag;

//    [Header("绑定团簇腔门")]
//    [SerializeField] private GameObject Qiangmen;

//    [Header("绑定步骤一结束后相机移动的位置")]
//    [SerializeField] private GameObject cameraTargetPosition;
    
//    private GameObject TargetPos;
//    // 是否可以拖动的标志位
//    private bool isAtPosition = false;
//    private Vector3 initialScale;
//    [SerializeField] private float SnapDistance = 0.5f;
//    private bool isNear; // 检验是否到位

//    private void Start()
//    {
//        initialScale = transform.localScale;
//        // 空引用检查
//        if (TargetPos == null || Qiangmen == null)
//        {
//            Debug.LogError("SheetLoading: 请确保所有引用都已正确设置");
//        }
//        TargetPos = tracedDrag.MaterialSheet_destpos;
//    }

//    private void OnMouseDrag()
//    {
//        if (isAtPosition) return;
//        transform.localScale = 1.01f * initialScale;
//        var dragPlane = new Plane(Camera.main.transform.forward, transform.position);
//        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

//        if (dragPlane.Raycast(ray, out var enter))
//        {
//            var dragPos = ray.GetPoint(enter);
//            transform.position = new Vector3(dragPos.x, TargetPos.transform.position.y, dragPos.z);
//        }
//    }

//    private void Update()
//    {
//        if (TargetPos == null) return; // 避免空引用异常

//        isNear = Vector3.Distance(transform.position, TargetPos.transform.position) < SnapDistance;

//        if (!isAtPosition && isNear)
//        {
//            isAtPosition = true;
//            transform.position = TargetPos.transform.position;
//            gameObject.transform.localScale = Vector3.zero;
//            StartCoroutine(QiangMenHuiDong());
//        }
//    }

//    /// <summary>
//    /// 腔门回动主协程，按顺序执行旋转和平动动画
//    /// </summary>
//    private IEnumerator QiangMenHuiDong()
//    {
//        // 确保腔门对象存在
//        if (QiangMenZhengTi == null || Qiangmen == null || QiangMenTargetPos == null)
//        {
//            yield break;
//        }

//        // 先执行旋转动画，完成后再执行平动动画
//        yield return StartCoroutine(Zhuandong());
//        QiangMenZhengTi.SetActive(false);
//        Qiangmen.SetActive(true);
//        Qiangmen.transform.localScale = tracedDrag.initialScale;
//        L_ShapedTrace.SetActive(true);
//        yield return StartCoroutine(Pingdong());
//        yield return StartCoroutine(L_trace_Rotate());
//        yield return StartCoroutine(CameraMove());
//    }

//    /// <summary>
//    /// 腔门旋转动画协程（顺时针旋转90度）
//    /// </summary>
//    private IEnumerator Zhuandong()
//    {
//        float elapsedTime = 0f;
//        float AnimationDuration = 1.2f;
//        Vector3 rotationAxis = Vector3.forward; // 显式指定Z轴为旋转轴

//        Quaternion startRot = QiangMenZhengTi.transform.rotation;
//        Quaternion targetRot = startRot * Quaternion.Euler(0, 0, -90); // 顺时针旋转90度

//        while (elapsedTime < AnimationDuration)
//        {
//            float progress = elapsedTime / AnimationDuration;
//            QiangMenZhengTi.transform.rotation = Quaternion.Lerp(startRot, targetRot, progress);
//            elapsedTime += Time.deltaTime;
//            yield return null;
//        }
//        QiangMenZhengTi.transform.rotation = targetRot;
//        yield return null;
//    }
//    private CinemachineBrain cinemachineBrain;
//    private bool isCinemachineDisabled = false;
//    /// <summary>
//    /// 腔门平动动画
//    /// </summary>
//    private IEnumerator Pingdong()
//    {
//        float elapsedTime = 0f;
//        float duration = 1.2f;
//        Vector3 initPos = Qiangmen.transform.position;
//        Vector3 targetPos = QiangMenTargetPos.transform.position;

//        while (elapsedTime < duration)
//        {
//            float progress = elapsedTime / duration;
//            float smoothProgress = Mathf.SmoothStep(0, 1, progress);
//            Qiangmen.transform.position = Vector3.Lerp(initPos, targetPos, smoothProgress);

//            elapsedTime += Time.deltaTime;
//            yield return null;
//        }
//        Qiangmen.transform.position = targetPos;
//        yield return null;
//    }
//    private IEnumerator L_trace_Rotate()
//    {
//        float elapsedTime = 0f;
//        float AnimationDuration = 1.2f;
//        Vector3 rotationAxis = Vector3.forward; // 显式指定Z轴为旋转轴

//        Quaternion startRot = L_ShapedTrace.transform.rotation;
//        Quaternion targetRot = startRot * Quaternion.Euler(0, 0, 90); // 逆时针旋转90度

//        while (elapsedTime < AnimationDuration)
//        {
//            float progress = elapsedTime / AnimationDuration;
//            L_ShapedTrace.transform.rotation = Quaternion.Lerp(startRot, targetRot, progress);
//            elapsedTime += Time.deltaTime;
//            yield return null;
//        }
//        L_ShapedTrace.transform.rotation = targetRot;
//        yield return null;
//    }
//    private IEnumerator CameraMove()
//    {
//        // 保存CinemachineBrain引用
//        cinemachineBrain = Camera.main.GetComponent<CinemachineBrain>();

//        // 临时禁用Cinemachine
//        if (cinemachineBrain != null)
//        {
//            cinemachineBrain.enabled = false;
//            isCinemachineDisabled = true;
//        }

//        float elapsedTime = 0f;
//        Camera mainCamera = Camera.main;
//        Vector3 startPos = mainCamera.transform.position;
//        Quaternion startRot = mainCamera.transform.rotation;
//        Vector3 targetPos = cameraTargetPosition.transform.position;
//        Quaternion targetRot = cameraTargetPosition.transform.rotation;

//        while (elapsedTime < 2.0f)
//        {
//            float progress = elapsedTime / 2.0f;
//            mainCamera.transform.position = Vector3.Lerp(startPos, targetPos, progress);
//            mainCamera.transform.rotation = Quaternion.Slerp(startRot, targetRot, progress);
//            elapsedTime += Time.deltaTime;
//            yield return null;
//        }

//        mainCamera.transform.position = targetPos;
//        mainCamera.transform.rotation = targetRot;

//        // 在恢复Cinemachine之前，更新当前激活的虚拟相机位置
//        if (isCinemachineDisabled && cinemachineBrain != null)
//        {
//            // 获取当前激活的虚拟相机
//            CinemachineVirtualCameraBase activeVirtualCamera = cinemachineBrain.ActiveVirtualCamera as CinemachineVirtualCameraBase;
//            if (activeVirtualCamera != null)
//            {
//                // 更新虚拟相机的位置和旋转
//                activeVirtualCamera.transform.position = targetPos;
//                activeVirtualCamera.transform.rotation = targetRot;
//            }

//            // 恢复Cinemachine
//            cinemachineBrain.enabled = true;
//            isCinemachineDisabled = false;
//        }
//        yield return null;
//    }
//}