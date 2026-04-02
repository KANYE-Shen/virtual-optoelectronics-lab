using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using Cinemachine;

public class GotoCabinet : MonoBehaviour
{
    [Header("绑定点击切换机箱视角按钮")]
    [SerializeField] private Button GotoCabinetButton;
    [Header("目标机位")]
    [SerializeField] private GameObject cameraTargetPosition;

    private CinemachineBrain cinemachineBrain;
    private bool isCinemachineDisabled;

    // 新增：存储相机原始位置和旋转
    private Vector3 originalPosition;
    private Quaternion originalRotation;

    // 当前是否在机箱视角
    private bool isAtCabinet = false;

    private void Start()
    {
        if (GotoCabinetButton == null || cameraTargetPosition == null)
        {
            Debug.LogError("Inspector 未绑定完整！");
            return;
        }

        GotoCabinetButton.onClick.AddListener(ToggleCameraPosition);
    }

    private void ToggleCameraPosition()
    {
        if (!isAtCabinet)
        {
            // 先存储当前位置
            Camera mainCamera = Camera.main;
            originalPosition = mainCamera.transform.position;
            originalRotation = mainCamera.transform.rotation;

            StartCoroutine(MoveCamera(mainCamera.transform.position, mainCamera.transform.rotation,
                                      cameraTargetPosition.transform.position, cameraTargetPosition.transform.rotation));
        }
        else
        {
            // 回到原始位置
            Camera mainCamera = Camera.main;
            StartCoroutine(MoveCamera(mainCamera.transform.position, mainCamera.transform.rotation,
                                      originalPosition, originalRotation));
        }

        isAtCabinet = !isAtCabinet; // 切换状态
    }

    private IEnumerator MoveCamera(Vector3 startPos, Quaternion startRot, Vector3 targetPos, Quaternion targetRot)
    {
        cinemachineBrain = Camera.main.GetComponent<CinemachineBrain>();
        if (cinemachineBrain != null)
        {
            cinemachineBrain.enabled = false;
            isCinemachineDisabled = true;
        }

        float elapsedTime = 0f;
        float duration = 2f;

        while (elapsedTime < duration)
        {
            float progress = elapsedTime / duration;
            Camera.main.transform.position = Vector3.Lerp(startPos, targetPos, progress);
            Camera.main.transform.rotation = Quaternion.Slerp(startRot, targetRot, progress);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        Camera.main.transform.position = targetPos;
        Camera.main.transform.rotation = targetRot;

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
    }
}
