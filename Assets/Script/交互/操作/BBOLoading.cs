using System.Collections;
using Cinemachine;
using UnityEngine;
using UnityEngine.UI;

public class BBOLoading : MonoBehaviour
{
    [Header("绑定旋转基座")]
    [SerializeField] private GameObject JiZuo;

    [Header("绑定BBO薄膜按钮")]
    [SerializeField] private Button BBOLoadingButton;

    [Header("绑定相机目标位置")]
    [SerializeField] private GameObject BBOLoading_Pos1;//远距离机位
    [SerializeField] private GameObject BBOLoading_Pos2;//近距离机位
    [SerializeField] private GameObject cameraTargetPosition;//结束后下一步调节和频光的摄像机位置

    private Vector3 initScale;
    private float MaxHeight;
    private float MinHeight;
    private bool hasReachedMax = false;
    private bool isDragging = false; // 是否正在拖拽

    private CinemachineBrain cinemachineBrain;
    private bool isCinemachineDisabled = false;
    private bool isGoingToStep1_2_Pos1 = false;

    // 拖拽相关变量
    private float yOffset; // 鼠标点击位置与物体中心点的Y轴偏移
    private Camera mainCamera;

    private void Start()
    {
        if (JiZuo == null || BBOLoadingButton == null || cameraTargetPosition == null)
        {
            Debug.LogError("Inspector 未绑定完整！");
            return;
        }

        mainCamera = Camera.main;
        initScale = JiZuo.transform.localScale;
        MinHeight = JiZuo.transform.position.y;
        MaxHeight = MinHeight + 1.6601f;

        // 初始状态：按钮隐藏
        BBOLoadingButton.gameObject.SetActive(false);
        BBOLoadingButton.interactable = false;
        BBOLoadingButton.onClick.AddListener(OnBBOButtonClicked);

        // 确保有碰撞器
        if (JiZuo.GetComponent<Collider>() == null)
        {
            JiZuo.AddComponent<BoxCollider>();
        }

        Debug.Log($"Start: JiZuo初始Y={JiZuo.transform.position.y}, MinHeight={MinHeight}, MaxHeight={MaxHeight}");
    }

    private void Update()
    {
        if (!StepController.Step1_2)
        {
            if (BBOLoadingButton.gameObject.activeSelf)
                BBOLoadingButton.gameObject.SetActive(false);
            return;
        }

        if (StepController.Step1_2 && !isGoingToStep1_2_Pos1)
        {
            var cinemachine = Camera.main.GetComponent<CinemachineBrain>();
            if (cinemachine != null)
            {
                cinemachine.enabled = false;
                Camera.main.transform.position = BBOLoading_Pos1.transform.position;
                Camera.main.transform.rotation = BBOLoading_Pos1.transform.rotation;
                cinemachine.enabled = true;
            }
            isGoingToStep1_2_Pos1 = true;
            Debug.Log("Camera moved to BBOLoading_Pos1");
        }

        // 处理拖拽逻辑
        HandleDragging();
        // 检测拖拽进度
        CheckDragProgress();
    }
    private IEnumerator MoveCameraToPos2()
    {
        cinemachineBrain = Camera.main.GetComponent<CinemachineBrain>();
        if (cinemachineBrain != null)
        {
            cinemachineBrain.enabled = false;
            isCinemachineDisabled = true;
        }

        float elapsedTime = 0f;
        Camera mainCamera = Camera.main;
        Vector3 startPos = mainCamera.transform.position;
        Quaternion startRot = mainCamera.transform.rotation;
        Vector3 targetPos = BBOLoading_Pos2.transform.position;
        Quaternion targetRot = BBOLoading_Pos2.transform.rotation;

        float duration = 2f;
        while (elapsedTime < duration)
        {
            float progress = elapsedTime / duration;
            mainCamera.transform.position = Vector3.Lerp(startPos, targetPos, progress);
            mainCamera.transform.rotation = Quaternion.Slerp(startRot, targetRot, progress);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        mainCamera.transform.position = targetPos;
        mainCamera.transform.rotation = targetRot;

        if (isCinemachineDisabled && cinemachineBrain != null)
        {
            var activeVirtualCamera = cinemachineBrain.ActiveVirtualCamera as CinemachineVirtualCameraBase;
            if (activeVirtualCamera != null)
            {
                activeVirtualCamera.transform.position = targetPos;
                activeVirtualCamera.transform.rotation = targetRot;
            }

            cinemachineBrain.enabled = true;
            isCinemachineDisabled = false;
        }

        Debug.Log("Camera moved to BBOLoading_Pos2");
    }

    private void HandleDragging()
    {
        if (!StepController.Step1_2 || hasReachedMax)
            return;

        // 鼠标按下
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                Debug.Log($"MouseDown hit: {hit.collider.gameObject.name}");
                if (hit.collider.gameObject == JiZuo)
                {
                    isDragging = true;
                    // 记录点击点与物体中心Y的偏移
                    yOffset = hit.point.y - JiZuo.transform.position.y;
                    Debug.Log($"Start Dragging: yOffset={yOffset}");
                }
            }
            else
            {
                Debug.Log("MouseDown: Raycast did not hit anything");
            }
        }
        // 鼠标拖拽中
        else if (Input.GetMouseButton(0) && isDragging)
        {
            Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
            Plane plane = new Plane(Vector3.forward, new Vector3(0, 0, JiZuo.transform.position.z));

            if (plane.Raycast(ray, out float distance))
            {
                Vector3 hitPoint = ray.GetPoint(distance);
                float newY = Mathf.Clamp(hitPoint.y - yOffset, MinHeight, MaxHeight);
                JiZuo.transform.position = new Vector3(JiZuo.transform.position.x, newY, JiZuo.transform.position.z);
            }
            else
            {
                Debug.Log("Dragging: Plane.Raycast failed");
            }
        }
        // 鼠标释放
        else if (Input.GetMouseButtonUp(0))
        {
            if (isDragging)
            {
                Debug.Log("End Dragging");
                isDragging = false;
            }
        }
    }

    private void CheckDragProgress()
    {
        if (hasReachedMax) return;

        float currentY = JiZuo.transform.position.y;
        //Debug.Log($"CheckDragProgress: JiZuo Y={currentY}");

        if (currentY >= MaxHeight - 0.01f)
        {
            hasReachedMax = true;
            BBOLoadingButton.gameObject.SetActive(true);
            BBOLoadingButton.interactable = true;
            Debug.Log("JiZuo reached MaxHeight, button enabled");
            StartCoroutine(MoveCameraToPos2());
        }
    }

    private void OnBBOButtonClicked()
    {
        BBOLoadingButton.gameObject.SetActive(false);
        Debug.Log("BBO Button clicked, start lowering JiZuo and moving camera");
        StartCoroutine(LowerJiZuoAndMoveCamera());
    }

    private IEnumerator LowerJiZuoAndMoveCamera()
    {
        Vector3 startPos = JiZuo.transform.position;
        Vector3 endPos = new Vector3(startPos.x, MinHeight, startPos.z);
        float elapsed = 0f;
        float duration = 1f;

        while (elapsed < duration)
        {
            JiZuo.transform.position = Vector3.Lerp(startPos, endPos, elapsed / duration);
            elapsed += Time.deltaTime;
            yield return null;
        }
        JiZuo.transform.position = endPos;
        Debug.Log("JiZuo lowered to MinHeight");

        yield return StartCoroutine(MoveCamera());

        StepController.Step1_2 = false;
        StepController.Step1_3 = true;
        Debug.Log("Step1_2 finished, Step1_3 started");
    }

    private IEnumerator MoveCamera()
    {
        cinemachineBrain = Camera.main.GetComponent<CinemachineBrain>();
        if (cinemachineBrain != null)
        {
            cinemachineBrain.enabled = false;
            isCinemachineDisabled = true;
        }

        float elapsedTime = 0f;
        Camera mainCamera = Camera.main;
        Vector3 startPos = mainCamera.transform.position;
        Quaternion startRot = mainCamera.transform.rotation;
        Vector3 targetPos = cameraTargetPosition.transform.position;
        Quaternion targetRot = cameraTargetPosition.transform.rotation;

        float duration = 2f;
        while (elapsedTime < duration)
        {
            float progress = elapsedTime / duration;
            mainCamera.transform.position = Vector3.Lerp(startPos, targetPos, progress);
            mainCamera.transform.rotation = Quaternion.Slerp(startRot, targetRot, progress);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        mainCamera.transform.position = targetPos;
        mainCamera.transform.rotation = targetRot;

        if (isCinemachineDisabled && cinemachineBrain != null)
        {
            var activeVirtualCamera = cinemachineBrain.ActiveVirtualCamera as CinemachineVirtualCameraBase;
            if (activeVirtualCamera != null)
            {
                activeVirtualCamera.transform.position = targetPos;
                activeVirtualCamera.transform.rotation = targetRot;
            }

            cinemachineBrain.enabled = true;
            isCinemachineDisabled = false;
        }

        Debug.Log("Camera move finished");
    }
}
