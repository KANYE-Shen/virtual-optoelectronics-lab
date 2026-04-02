using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GasTightnessTest : MonoBehaviour
{
    [SerializeField]
    private string GasTightnessTestObjectLabel; // Inspector 输入目标物体标签

    public Camera mainCamera;
    //变色因子表示保留原配色的多少(0-1),其余用黑色混合
    [Range(0f, 1f)]
    public float hoverDarkFactor = 0.9f;        // 悬停暗色因子
    [Range(0f, 1f)]
    public float selectedDarkFactor = 0.8f;     // 选中暗色因子
    public float transitionSpeed = 60f;        // 颜色过渡速度

    private GameObject hoverObject;//当前悬停的目标
    private GameObject selectedObject;//空格与右键同时按住后的选中目标
    private Dictionary<Renderer, Color> originalColors = new Dictionary<Renderer, Color>();//存储所有被处理过的Renderer的初始颜色（键=Renderer组件，值=初始颜色，防止暗化后颜色丢失）
    private GameObject[] ValvesNeededCheck;//用于存储所有阀门
    private int TotalValveCnt;//阀门总数
    private int HandledCnt;

    // 存储所有曾处理过的Renderer（解决颜色无法恢复问题）
    private List<Renderer> allProcessedRenderers = new List<Renderer>();
    // 存储手动创建的材质实例（解决内存泄漏问题）
    private Dictionary<Renderer, Material> instanceMaterials = new Dictionary<Renderer, Material>();

    private void Start()
    {
        if (mainCamera == null)
        {
            mainCamera = Camera.main;
        }
        ValvesNeededCheck = GameObject.FindGameObjectsWithTag(GasTightnessTestObjectLabel);
        TotalValveCnt = ValvesNeededCheck.Length;
        HandledCnt = 0;
    }

    private void Update()
    {
        PointerCast();//每帧更新当前鼠标悬停下的物体,保存在hoverobj里

        UpdateMaterialTransition();//执行颜色过度,处理暗化or恢复原色效果

        if (hoverObject != null && Input.GetKey(KeyCode.Space) && Input.GetMouseButtonDown(0))
        {
            HandleClick();//满足上述三个条件后执行处理点击事件
            HandledCnt++;
        }


        //这个地方后续需要添加未完成摆放要提示的界面


    }

    private void PointerCast()
    {
        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        GameObject previousHover = hoverObject; // 记录上一帧悬停物体

        if (Physics.Raycast(ray, out hit))
        {
            if (hit.collider.CompareTag(GasTightnessTestObjectLabel))
            {
                hoverObject = hit.collider.gameObject;
                // 收集当前悬停物体的所有Renderer（更新到总列表）
                CollectRenderers(hoverObject);
            }
            else
            {
                hoverObject = null;
            }
        }
        else
        {
            hoverObject = null;
            // 点击空白处取消选中状态
            selectedObject = null;
        }

        // 若上一帧有悬停物体，也收集其Renderer（避免漏处理）
        if (previousHover != null && previousHover != hoverObject)
        {
            CollectRenderers(previousHover);
        }
    }

    /// <summary>
    ///收集物体的所有子Renderer，添加到总处理列表（去重）
    /// </summary>
    private void CollectRenderers(GameObject targetObj)
    {
        if (targetObj == null) return;
        //由于本项目大多采用空父物体包含多个子物体的管理模式,通过遍历子物体实现逐个子物体的材质修改
        foreach (var renderer in targetObj.GetComponentsInChildren<Renderer>())
        {
            if (!allProcessedRenderers.Contains(renderer))
            {
                allProcessedRenderers.Add(renderer);
                // 记录初始颜色（仅首次添加时）
                if (!originalColors.ContainsKey(renderer))
                {
                    originalColors[renderer] = renderer.material.color;
                }
            }
        }
    }

    /// <summary>
    /// 平滑过渡更新颜色（修复：遍历所有曾处理过的Renderer，确保颜色能恢复）
    /// </summary>
    private void UpdateMaterialTransition()
    {
        // 遍历所有曾处理过的Renderer，而非仅当前悬停/选中物体
        foreach (Renderer renderer in allProcessedRenderers)
        {
            if (renderer == null || renderer.gameObject == null)
            {
                continue; // 防止物体被销毁导致空引用
            }

            // 获取材质：优先用手动创建的实例，无则用sharedMaterial（避免自动生成实例）
            Material targetMat = GetSafeMaterial(renderer);
            if (targetMat == null) continue;

            Color baseColor = originalColors.ContainsKey(renderer) ? originalColors[renderer] : targetMat.color;
            Color targetColor = baseColor;

            // 选中状态判断
            bool isSelected = selectedObject != null && renderer.transform.IsChildOf(selectedObject.transform);
            // 悬停状态判断（未选中时才生效）
            bool isHovered = !isSelected && hoverObject != null && renderer.transform.IsChildOf(hoverObject.transform);

            if (isSelected)
            {
                targetColor = Color.Lerp(baseColor, Color.black, 1f - selectedDarkFactor);
            }
            else if (isHovered)
            {
                targetColor = Color.Lerp(baseColor, Color.black, 1f - hoverDarkFactor);
            }
            // 非选中/悬停 → 恢复原色

            // 应用颜色过渡
            targetMat.color = Color.Lerp(targetMat.color, targetColor, Time.deltaTime * transitionSpeed);
        }
    }

    /// <summary>
    /// 安全获取材质：需独立修改时创建实例，否则用sharedMaterial（解决内存泄漏）
    /// </summary>
    private Material GetSafeMaterial(Renderer renderer)
    {
        if (renderer == null) return null;

        // 若已创建过实例，直接返回
        if (instanceMaterials.ContainsKey(renderer))
        {
            return instanceMaterials[renderer];
        }

        // 【关键】判断是否需要独立材质：若材质被多个物体共用，创建实例；否则用原材质
        if (renderer.sharedMaterial != null && renderer.sharedMaterial.name.Contains("Instance"))
        {
            // 已存在实例，直接关联
            instanceMaterials[renderer] = renderer.material;
        }
        else
        {
            // 创建新实例（仅当需要修改时）
            Material newInstance = new Material(renderer.sharedMaterial);
            instanceMaterials[renderer] = newInstance;
            renderer.material = newInstance; // 将实例赋值给Renderer
        }

        return instanceMaterials[renderer];
    }

    /// <summary>
    /// 处理点击：选中新物体前先重置旧选中状态
    /// </summary>
    private void HandleClick()
    {
        if (hoverObject == null) return;

        // 重置旧选中物体状态
        selectedObject = null;
        // 延迟一帧再赋值,确保颜色过渡生效
        StartCoroutine(SetSelectedObjectCoroutine(hoverObject));
    }

    private IEnumerator SetSelectedObjectCoroutine(GameObject target)
    {
        yield return null; 
        selectedObject = target;
        StartCoroutine(FixGasTightness(selectedObject));
    }

    private IEnumerator FixGasTightness(GameObject obj)
    {
        if (obj == null) yield break;

        yield return StartCoroutine(Rotate_enumerator(obj, true));
        yield return new WaitForSeconds(0.5f);
        yield return StartCoroutine(Rotate_enumerator(obj, false));
    }

    private IEnumerator Rotate_enumerator(GameObject obj, bool isClockwise)
    {
        if (obj == null) yield break;

        float elapsedTime = 0f;
        float animationDuration = 0.5f;

        Quaternion startRot = obj.transform.rotation;
        Quaternion endRot = startRot * Quaternion.Euler(0, 0, isClockwise ? -180 : 180);

        while (elapsedTime < animationDuration)
        {
            if (obj == null) yield break;

            float progress = elapsedTime / animationDuration;
            obj.transform.rotation = Quaternion.Lerp(startRot, endRot, progress);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        if (obj != null)
            obj.transform.rotation = endRot;
    }

    /// <summary>
    /// 销毁时清理材质实例（防止内存泄漏）
    /// </summary>
    private void OnDestroy()
    {
        foreach (var mat in instanceMaterials.Values)
        {
            if (mat != null)
            {
                Destroy(mat); // 手动销毁创建的材质实例
            }
        }
        instanceMaterials.Clear();
        allProcessedRenderers.Clear();
        originalColors.Clear();
    }
}
