using System;
using System.Collections;
using Cinemachine;
using UnityEngine;
using UnityEngine.UI;
/// <summary>
/// 应该Ltrace转上来之后再转回去,此外应该是先点击按钮执行动画协程后再切换相机视角,同时点击完按钮后材料面板消失
/// </summary>
public class ITP_SharedSpawnerWithDrag : MonoBehaviour
{
    [Serializable]
    public class ParamControl
    {
        public Button button;
        public string prefabName;
    }

    [Header("统一配置")]
    public ParamControl[] controls;
    public GameObject MaterialPanel;
    public Button FinishMaterialLoadingButton;
    public GameObject RotateAxis;
    public GameObject Ltrace;
    public GameObject TCY;
    public Vector3 spawnPosition;
    public GameObject dragTarget;
    public string destObjectTag;

    [Header("相机配置")]
    public Camera mainCamera;
    public GameObject nextStepCamPos;
    public CinemachineBrain cineBrain;

    private GameObject _currentObj;
    private Plane _dragPlane;
    private bool isAnimating;
    private bool finishButtonUsed = false; // 防止按钮被重复可见/重复触发
    float SuccessDurationTime = 2f;

    // 拖拽相关
    private bool _isDragging = false;
    private Vector3 _dragOffset;

    private void Start()
    {
        mainCamera = Camera.main;
        if (dragTarget == null)
        {
            Debug.LogError("未绑定拖拽目标物体 dragTarget");
            return;
        }

        // 初始隐藏 UI
        if (MaterialPanel != null) MaterialPanel.SetActive(false);
        if (FinishMaterialLoadingButton != null) FinishMaterialLoadingButton.gameObject.SetActive(false);

        // 绑定按钮
        if (FinishMaterialLoadingButton != null)
        {
            FinishMaterialLoadingButton.onClick.RemoveAllListeners();
            FinishMaterialLoadingButton.onClick.AddListener(() =>
            {
                if (finishButtonUsed) return;
                finishButtonUsed = true;
                // 立刻禁用并隐藏按钮，防止重复点击或 Update 恢复可见
                FinishMaterialLoadingButton.interactable = false;
                FinishMaterialLoadingButton.gameObject.SetActive(false);
                MaterialPanel.SetActive(false);
                StepController.step1_6 = false; // 防止Update里再触发
                StepController.AllowToShowLightPath = true;
                StartRotateBack();
            });
        }

        _dragPlane = new Plane(Vector3.up, dragTarget.transform.position);

        // 预制按钮绑定
        for (int i = 0; i < controls.Length; i++)
        {
            int index = i;
            if (controls[i] != null && controls[i].button != null)
            {
                controls[i].button.onClick.AddListener(() => SpawnPrefab(index));
            }
        }
    }

    private void SpawnPrefab(int index)
    {
        if (_currentObj != null)
        {
            Destroy(_currentObj);
            _currentObj = null;
        }

        GameObject prefab = Resources.Load<GameObject>(controls[index].prefabName);
        if (prefab == null)
        {
            Debug.LogError($"Prefab {controls[index].prefabName} 未找到");
            return;
        }

        _currentObj = Instantiate(prefab, spawnPosition, Quaternion.identity);

        Rigidbody rb = _currentObj.GetComponent<Rigidbody>();
        if (rb == null) rb = _currentObj.AddComponent<Rigidbody>();
        rb.isKinematic = true;

        BoxCollider bc = _currentObj.GetComponent<BoxCollider>();
        if (bc == null) bc = _currentObj.AddComponent<BoxCollider>();
        bc.center = Vector3.zero;
        bc.size = new Vector3(0.5f, 0.5f, 0.5f);
    }

    private void Update()
    {
        // 必要的 camera 空检查（否则所有 UI/射线都没法工作）
        if (mainCamera == null) return;

        // 把 StepController 的 UI 显示逻辑放在这里，不受 _currentObj 是否为 null 的影响
        if (StepController.step1_6)
        {
            if (MaterialPanel != null && !MaterialPanel.activeSelf) MaterialPanel.SetActive(true);
            // 只有当按钮还没被使用过（finishButtonUsed==false）时才展示按钮
            if (FinishMaterialLoadingButton != null && !finishButtonUsed)
            {
                if (!FinishMaterialLoadingButton.gameObject.activeSelf)
                    FinishMaterialLoadingButton.gameObject.SetActive(true);
            }
        }

        // 若没有 _currentObj，则跳过后续拖拽逻辑但允许UI正常显示
        if (_currentObj == null) return;

        // 鼠标左键按下 => 开始拖拽
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                if (hit.collider != null && hit.collider.gameObject == _currentObj)
                {
                    if (_dragPlane.Raycast(ray, out float enter))
                    {
                        Vector3 hitPoint = ray.GetPoint(enter);
                        _dragOffset = _currentObj.transform.position - hitPoint;
                        _isDragging = true;
                    }
                }
            }
        }

        // 拖拽中
        if (_isDragging && Input.GetMouseButton(0))
        {
            Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
            if (_dragPlane.Raycast(ray, out float enter))
            {
                Vector3 hitPoint = ray.GetPoint(enter);
                _currentObj.transform.position = hitPoint + _dragOffset;
            }
        }

        // 鼠标松开 => 检查是否吸附
        if (_isDragging && Input.GetMouseButtonUp(0))
        {
            _isDragging = false;

            GameObject destObj = GameObject.FindGameObjectWithTag(destObjectTag);
            if (destObj != null)
            {
                _currentObj.transform.position = destObj.transform.position;
                _currentObj.transform.rotation = destObj.transform.rotation;
                _currentObj.SetActive(false);
            }
        }
    }

    private void OnDragFinished()
    {
        if (mainCamera == null || nextStepCamPos == null || cineBrain == null)
        {
            Debug.LogWarning("相机跳转未配置完整");
            return;
        }

        cineBrain.enabled = false;
        mainCamera.transform.position = nextStepCamPos.transform.position;
        mainCamera.transform.rotation = nextStepCamPos.transform.rotation;
        cineBrain.enabled = true;
    }

    public void StartRotateBack()
    {
        if (!isAnimating)
        {
            isAnimating = true;
            StartCoroutine(RotateBackSequence());
        }
    }

    private IEnumerator RotateBackSequence()
    {
        // 1) 先旋转（TCY 与 Ltrace 同步）—— 完全结束后才继续
        yield return StartCoroutine(RotateAroundAxisBack(90f));

        // 2) 再沿 TCY 局部 -X 方向移动
        yield return StartCoroutine(MoveBackward(3f));
        // 3) Ltrace再转回去
        yield return StartCoroutine(RotateLtraceBack());

        isAnimating = false;
    }

    private IEnumerator RotateAroundAxisBack(float angle)
    {
        float elapsed = 0f;

        Vector3 axisOrigin = RotateAxis.transform.position;
        Vector3 axisDir = RotateAxis.transform.forward.normalized;

        Vector3 startPos = TCY.transform.position;
        Vector3 toStart = startPos - axisOrigin;
        Vector3 proj = Vector3.Project(toStart, axisDir);
        Vector3 axisPoint = axisOrigin + proj;
        Vector3 radial = toStart - proj;

        Quaternion TCY_StartRot = TCY.transform.rotation;
        Quaternion rotationAroundExternalAxis = Quaternion.AngleAxis(angle, axisDir);
        Quaternion TCY_EndRot = rotationAroundExternalAxis * TCY_StartRot;

        Quaternion L_startRot = Ltrace.transform.rotation;
        Quaternion L_targetRot = L_startRot * Quaternion.Euler(0, 0, angle);//转下去的还没写完

        while (elapsed < SuccessDurationTime)
        {
            float progress = elapsed / SuccessDurationTime;
            float currentAngle = Mathf.Lerp(0f, angle, progress);

            // 位置沿轴旋转（TCY 会跟着旋转）
            Vector3 rotatedRadial = Quaternion.AngleAxis(currentAngle, axisDir) * radial;
            TCY.transform.position = axisPoint + rotatedRadial;

            // 旋转插值
            TCY.transform.rotation = Quaternion.Slerp(TCY_StartRot, TCY_EndRot, progress);
            Ltrace.transform.rotation = Quaternion.Slerp(L_startRot, L_targetRot, progress);

            elapsed += Time.deltaTime;
            yield return null;
        }

        // 最终修正
        Vector3 finalRadial = Quaternion.AngleAxis(angle, axisDir) * radial;
        TCY.transform.position = axisPoint + finalRadial;
        TCY.transform.rotation = TCY_EndRot;
        Ltrace.transform.rotation = L_targetRot;
    }

    private IEnumerator MoveBackward(float distance)
    {
        float elapsed = 0f;
        Vector3 origin = TCY.transform.position;
        Vector3 target = origin + TCY.transform.TransformDirection(Vector3.left) * distance;

        float duration = 1f;
        while (elapsed < duration)
        {
            float t = Mathf.SmoothStep(0, 1, elapsed / duration);
            TCY.transform.position = Vector3.Lerp(origin, target, t);
            elapsed += Time.deltaTime;
            yield return null;
        }
        TCY.transform.position = target;
    }
    private IEnumerator RotateLtraceBack()
    {
        float elapsedTime = 0f;
        float AnimationDuration = 1f;
        var StartRot = Ltrace.transform.rotation;
        var EndRot = Ltrace.transform.rotation * Quaternion.Euler(0, 0, -90);
        
        while (elapsedTime < AnimationDuration)
        {
            float Process = elapsedTime / AnimationDuration;
            Ltrace.transform.rotation = Quaternion.Lerp(StartRot, EndRot, Process);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        Ltrace.transform.rotation = EndRot;
        yield return null;
        OnDragFinished();
    }
}
