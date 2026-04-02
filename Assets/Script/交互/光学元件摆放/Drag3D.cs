using UnityEngine;

public class Drag3D : MonoBehaviour
{
    [Header("拖拽功能相关属性设置")]
    [SerializeField] private float snapDistance = 0.3f;
    [SerializeField] public string DestObjectTag;
    [Space(5)]

    [Header("观测属性面板:无需手动调整")]
    [SerializeField] private bool PositionLock = false;

    [HideInInspector] public ITP_nerver manager;

    private Plane _dragPlane;
    private GameObject[] destObjects;
    private bool _isDragging = false;

    private void Start()
    {
        destObjects = GameObject.FindGameObjectsWithTag(DestObjectTag);
        GameObject TargetPlane = GameObject.FindGameObjectWithTag("PlacementPlane");   
        _dragPlane = new Plane(Vector3.up, TargetPlane.transform.position + new Vector3(0, 0.473f, 0));
    }

    public void ForceStartDrag()
    {
        _isDragging = true;
    }

    private void Update()
    {
        if (PositionLock) return;

        // 鼠标按下，检测是否点到自己
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit) && hit.collider.gameObject == gameObject)
            {
                _isDragging = true;
            }
        }

        // 拖拽中
        if (_isDragging && Input.GetMouseButton(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (_dragPlane.Raycast(ray, out float enter))
            {
                Vector3 newPos = ray.GetPoint(enter);
                transform.position = newPos;
            }
        }

        // 鼠标抬起 -> 尝试吸附
        if (Input.GetMouseButtonUp(0))
        {
            if (_isDragging)
            {
                TrySnap();
            }
            _isDragging = false;
        }
    }

    /// <summary>
    /// 鼠标抬起时尝试吸附到最近目标点
    /// </summary>
    private void TrySnap()
    {
        if (destObjects == null || destObjects.Length == 0)
        {
            Debug.LogWarning($"未找到标签为 {DestObjectTag} 的目标物体");
            return;
        }

        GameObject closestDest = null;
        float minDistance = float.MaxValue;

        foreach (GameObject dest in destObjects)
        {
            if (dest == null) continue;

            float distance = Vector3.Distance(transform.position, dest.transform.position);
            if (distance < minDistance)
            {
                minDistance = distance;
                closestDest = dest;
            }
        }

        // 打印调试信息，确认吸附条件
        Debug.Log($"吸附检测：最近距离 {minDistance}，阈值 {snapDistance}");
        if (closestDest != null && minDistance <= snapDistance)
        {
            transform.position = closestDest.transform.position;
            transform.localRotation = closestDest.transform.localRotation;
            transform.localScale = closestDest.transform.localScale;

            PositionLock = true;
            closestDest.SetActive(false);
            Debug.Log($"已吸附到 {closestDest.name}");
        }
        else
        {
            Debug.Log("未满足吸附条件");
        }
    }
}