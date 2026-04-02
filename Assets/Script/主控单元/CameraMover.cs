using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class CameraMover : MonoBehaviour
{
    [Header("路径配置 ")]
    [Tooltip("存储相机移动的目标路点")]
    [SerializeField] private List<GameObject> waypoints = new List<GameObject>();

    [Header("移动参数")]
    [Tooltip("位置移动速度")]
    [SerializeField] private float moveSpeed = 5f;
    [Tooltip("旋转平滑速度")]
    [SerializeField] private float rotateSpeed = 10f;
    [Tooltip("到达目标位置的判定阈值")]
    [SerializeField] private float positionThreshold = 0.1f;
    [Tooltip("到达目标旋转的判定阈值")]
    [SerializeField] private float rotationThreshold = 1f;

    [Header("运行控制")]
    [Tooltip("游戏启动时是否自动开始移动")]
    [SerializeField] private bool autoStart = true;
    [Tooltip("到达最后一个路点后是否循环回到第一个")]
    [SerializeField] private bool loopAfterLast = false;

    // 内部状态变量
    private int currentWaypointIndex = 0; // 当前目标路点索引
    private bool isMoving = false;        // 是否正在执行移动逻辑（防止重复启动协程）
    private Camera mainCamera;           

    private void Start()
    {
        // 初始化主相机引用
        mainCamera = Camera.main;
        // 如果开启自动启动，且路点列表不为空，则启动移动协程
        if (autoStart && waypoints.Count > 0)
        {
            StartCoroutine(MoveAndRotateToWaypoint());
        }
    }

    private IEnumerator MoveAndRotateToWaypoint()
    {
        isMoving = true;

        // 持续循环（直到路点用尽且不循环）
        while (true)
        {
            // 安全判断：如果路点列表为空，终止协程
            if (waypoints.Count == 0)
            {
                Debug.LogWarning("路点列表为空，请在Inspector拖入路点对象");
                isMoving = false;
                yield break;
            }

            // 获取当前目标路点的位置和旋转
            GameObject targetWaypoint = waypoints[currentWaypointIndex];
            Vector3 targetPos = targetWaypoint.transform.position;
            Quaternion targetRot = targetWaypoint.transform.rotation;
            while (!IsReachedTarget(targetPos, targetRot))
            {
                
                mainCamera.transform.position = Vector3.MoveTowards(mainCamera.transform.position,targetPos,moveSpeed * Time.deltaTime);
                mainCamera.transform.rotation = Quaternion.Slerp( mainCamera.transform.rotation, targetRot,rotateSpeed * Time.deltaTime);
                yield return null;
            }
            currentWaypointIndex++;
            if (currentWaypointIndex >= waypoints.Count)
            {
                // 若循环，则重置索引到第一个路点
                if (loopAfterLast)
                {
                    currentWaypointIndex = 0;
                }
                // 不循环则终止协程
                else
                {
                    break;
                }
            }
            yield return new WaitForSeconds(1f);
        }

        // 所有路点处理完成，标记为未移动状态
        isMoving = false;
    }

    /// <summary>
    /// 用于判断相机是否到达目标位置和旋转
    /// </summary>
    /// <param name="targetPos">目标位置</param>
    /// <param name="targetRot">目标旋转</param>
    /// <returns>是否到达目标</returns>
    private bool IsReachedTarget(Vector3 targetPos, Quaternion targetRot)
    {
        // 计算当前位置与目标位置的距离
        float distanceToTarget = Vector3.Distance(mainCamera.transform.position, targetPos);
        // 计算当前旋转与目标旋转的角度差
        float angleToTarget = Quaternion.Angle(mainCamera.transform.rotation, targetRot);

        if(distanceToTarget < positionThreshold && angleToTarget < rotationThreshold)  return true;
        else return false;
    }

    /// <summary>
    /// 外部调用接口：手动触发相机移动（如需通过按钮等事件启动）
    /// </summary>
    public void StartCameraMovement()
    {
        if (!isMoving && waypoints.Count > 0)
        {
            StartCoroutine(MoveAndRotateToWaypoint());
        }
    }
}