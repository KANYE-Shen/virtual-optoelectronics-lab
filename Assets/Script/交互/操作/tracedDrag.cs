using System.Collections;
using Cinemachine;
using UnityEngine;

public class TracedDrag : MonoBehaviour
{
    [Header("动画&参数设置")]
    [SerializeField] private float SuccessDurationTime = 2f;
    [SerializeField] private float UnsuccessDurationTime = 0.2f;
    [SerializeField] private float SetExceedingDistance = 0.4f;
    [SerializeField] private float GoForwardDistance = 3f;

    [Header("目标对象")]
    [SerializeField] private GameObject LTrace;
    [SerializeField] private GameObject RotateAxis;
    [SerializeField] private GameObject cameraTargetPosition;
    [SerializeField] private GameObject MaterialSheet_destpos;

    private Vector3 startPos;
    private Quaternion startRot;
    private float initialY, initialZ;
    private bool isExceedArea = false;
    private bool isAnimating = false;

    private Vector3 lastMousePos;

    private void Start()
    {
        startPos = transform.position;//注意:这里是团簇源的transform
        startRot = transform.rotation;//注意:这里是团簇源的transform
        initialY = startPos.y;//特指yz坐标无需修改
        initialZ = startPos.z;//特指yz坐标无需修改

        MaterialSheet_destpos.SetActive(false);

        if (LTrace == null) Debug.LogError("请绑定L形轨道物体");
        if (RotateAxis == null) Debug.LogError("请绑定旋转轴");
        if (cameraTargetPosition == null) Debug.LogError("请绑定相机目标位置");
    }

    private void FixedUpdate()
    {
        if (isExceedArea && !isAnimating)
        {
            isAnimating = true;
            StartCoroutine(StartDeformationAnimation());
        }

        if (Vector3.Distance(startPos, transform.position) > SetExceedingDistance)
            isExceedArea = true;
    }

    private void OnMouseDown()
    {
        lastMousePos = Input.mousePosition;
    }

    private void OnMouseDrag()
    {
        if (isAnimating) return;

        Vector3 delta = Input.mousePosition - lastMousePos;
        lastMousePos = Input.mousePosition;

        // 反向加增量：鼠标向左移动 → 物体 x 增加（向右移动）
        float newX = transform.position.x - delta.x * 0.01f; // 0.01f 可保持原缩放比例

        // 限制只能向右拖动（x >= 初始位置）
        newX = Mathf.Max(newX, startPos.x);

        transform.position = new Vector3(newX, initialY, initialZ);
    }

    private void OnMouseUp()
    {
        if (!isExceedArea && !isAnimating)
            StartCoroutine(UnsuccessfulDrag());
    }

    private IEnumerator UnsuccessfulDrag()
    {
        float elapsed = 0f;
        Vector3 current = transform.position;
        while (elapsed < UnsuccessDurationTime)
        {
            float t = Mathf.SmoothStep(0, 1, elapsed / UnsuccessDurationTime);
            transform.position = Vector3.Lerp(current, startPos, t);
            elapsed += Time.deltaTime;
            yield return null;
        }
        transform.position = startPos;
    }

    private IEnumerator StartDeformationAnimation()
    {
        yield return StartCoroutine(MoveToEdge());//团簇源先移动到边缘
        yield return StartCoroutine(RotateLTrace(90f));//L轨转起来
        yield return StartCoroutine(MoveForward());//团簇源继续前进
        yield return StartCoroutine(RotateAroundAxis(-90f));//两个一起移动
        yield return StartCoroutine(MoveCamera());
    }

    private IEnumerator MoveToEdge()
    {
        float elapsed = 0f;
        Vector3 target = new Vector3(startPos.x + 0.73f, initialY, initialZ);
        Vector3 origin = transform.position;

        while (elapsed < 1f)
        {
            float t = Mathf.SmoothStep(0, 1, elapsed / 1f);
            transform.position = Vector3.Lerp(origin, target, t);
            elapsed += Time.deltaTime;
            yield return null;
        }
        transform.position = target;
    }

    private IEnumerator MoveForward()
    {
        float elapsed = 0f;
        Vector3 origin = transform.position;
        Vector3 target = origin + new Vector3(GoForwardDistance - 0.73f, 0, 0);

        while (elapsed < 1f)
        {
            float t = Mathf.SmoothStep(0, 1, elapsed / 1f);
            transform.position = Vector3.Lerp(origin, target, t);
            elapsed += Time.deltaTime;
            yield return null;
        }
        transform.position = target;
    }

    private IEnumerator RotateLTrace(float angle)
    {
        float elapsed = 0f;
        Vector3 rotationAxis = Vector3.forward; // 自身Z轴正方向

        Quaternion startRot = LTrace.transform.rotation;
        Quaternion targetRot = startRot * Quaternion.Euler(0, 0, 90);

        while (elapsed < SuccessDurationTime)
        {
            float t = elapsed / SuccessDurationTime;

            float progress = elapsed / SuccessDurationTime;
            LTrace.transform.rotation = Quaternion.Lerp(startRot, targetRot, progress);
            elapsed += Time.deltaTime;
            yield return null;
        }
        LTrace.transform.rotation = targetRot;
        yield return null;
    }

    private IEnumerator RotateAroundAxis(float angle)
    {
        float elapsed = 0f;

        // 外部旋转轴 (RotateAxis 的 Z 轴)
        Vector3 axisOrigin = RotateAxis.transform.position;
        Vector3 axisDir = RotateAxis.transform.forward.normalized;

        // === 位置相关 ===
        Vector3 startPos = transform.position;

        // 计算TCY到轴线的投影点 (最近点)
        Vector3 toStart = startPos - axisOrigin;
        Vector3 proj = Vector3.Project(toStart, axisDir);
        Vector3 axisPoint = axisOrigin + proj; // 轴线上基点
        Vector3 radial = toStart - proj;       // 径向向量 (从轴线指向TCY)
        float radius = radial.magnitude;

        // === 旋转相关 ===
        Quaternion TCY_StartRot = transform.rotation;
        Quaternion rotationAroundExternalAxis = Quaternion.AngleAxis(-angle, axisDir);
        Quaternion TCY_EndRot = rotationAroundExternalAxis * TCY_StartRot;

        // LTrace 保持原有逻辑
        Quaternion L_startRot = LTrace.transform.rotation;
        Quaternion L_targetRot = L_startRot * Quaternion.Euler(0, 0, angle);

        while (elapsed < SuccessDurationTime)
        {
            float progress = elapsed / SuccessDurationTime;
            float currentAngle = Mathf.Lerp(0f, -angle, progress); // 注意负号，和上面rotation一致

            // --- 更新位置 ---
            Vector3 rotatedRadial = Quaternion.AngleAxis(currentAngle, axisDir) * radial;
            transform.position = axisPoint + rotatedRadial;

            // --- 更新旋转 ---
            transform.rotation = Quaternion.Slerp(TCY_StartRot, TCY_EndRot, progress);

            // --- 更新 LTrace ---
            LTrace.transform.rotation = Quaternion.Slerp(L_startRot, L_targetRot, progress);

            elapsed += Time.deltaTime;
            yield return null;
        }

        // === 最终修正 ===
        Vector3 finalRadial = Quaternion.AngleAxis(-angle, axisDir) * radial;
        transform.position = axisPoint + finalRadial;

        transform.rotation = TCY_EndRot;
        LTrace.transform.rotation = L_targetRot;
    }





    private IEnumerator MoveCamera()
    {
        CinemachineBrain brain = Camera.main.GetComponent<CinemachineBrain>();
        if (brain != null) brain.enabled = false;

        float elapsed = 0f;
        Vector3 originPos = Camera.main.transform.position;
        Quaternion originRot = Camera.main.transform.rotation;
        Vector3 targetPos = cameraTargetPosition.transform.position;
        Quaternion targetRot = cameraTargetPosition.transform.rotation;

        while (elapsed < 2f)
        {
            float t = elapsed / 2f;
            Camera.main.transform.position = Vector3.Lerp(originPos, targetPos, t);
            Camera.main.transform.rotation = Quaternion.Slerp(originRot, targetRot, t);
            elapsed += Time.deltaTime;
            yield return null;
        }

        Camera.main.transform.position = targetPos;
        Camera.main.transform.rotation = targetRot;

        if (brain != null)
            brain.enabled = true;

        MaterialSheet_destpos.SetActive(true);
        StepController.step1_5 = false;
        StepController.step1_6 = true;
        Debug.Log("StepController.step1_6 的状态是:" + StepController.step1_6);
    }
}
