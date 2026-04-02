using UnityEngine;
using UnityEngine.UI;
using Cinemachine;
using System.Collections;

public class CameraSwitcher : MonoBehaviour
{
    [Header("主相机 (带 CinemachineBrain)")]
    [SerializeField] private Camera mainCamera;

    [Header("目标位置 (空物体Transform)")]
    [SerializeField] private Transform targetPosition;

    [Header("切换按钮")]
    [SerializeField] private Button switchButton;

    [Header("过渡时长")]
    [SerializeField] private float transitionDuration = 2f;

    private CinemachineBrain brain;
    private Vector3 originalPos;
    private Quaternion originalRot;
    private bool isSwitched = false;
    private bool isMoving = false;

    private void Start()
    {
        if (mainCamera == null)
            mainCamera = Camera.main;

        brain = mainCamera.GetComponent<CinemachineBrain>();

        if (switchButton != null)
            switchButton.onClick.AddListener(ToggleCamera);
    }

    private void ToggleCamera()
    {
        if (isMoving) return; // 防止重复点击打断

        if (!isSwitched)
        {
            // 保存原始位置和旋转
            originalPos = mainCamera.transform.position;
            originalRot = mainCamera.transform.rotation;

            // 禁用Brain，开始插值到目标位置
            brain.enabled = false;
            StartCoroutine(MoveCamera(mainCamera.transform, targetPosition.position, targetPosition.rotation));
            isSwitched = true;
        }
        else
        {
            // 从目标回到原始位置
            StartCoroutine(MoveCamera(mainCamera.transform, originalPos, originalRot, () =>
            {
                // 动画完成后恢复Brain
                brain.enabled = true;
            }));
            isSwitched = false;
        }
    }

    private IEnumerator MoveCamera(Transform cam, Vector3 targetPos, Quaternion targetRot, System.Action onComplete = null)
    {
        isMoving = true;
        Vector3 startPos = cam.position;
        Quaternion startRot = cam.rotation;
        float elapsed = 0f;

        while (elapsed < transitionDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / transitionDuration);
            cam.position = Vector3.Lerp(startPos, targetPos, t);
            cam.rotation = Quaternion.Slerp(startRot, targetRot, t);
            yield return null;
        }

        cam.position = targetPos;
        cam.rotation = targetRot;

        isMoving = false;
        onComplete?.Invoke();
    }
}
