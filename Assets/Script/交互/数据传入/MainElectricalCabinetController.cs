using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainElectricalCabinetController : MonoBehaviour
{
    [Header("控制箱面板参数")]
    [Header("面板1")]
    [SerializeField] private GameObject 水闸电开关;
    [SerializeField] private GameObject 机械泵开关;
    [SerializeField] private GameObject 罗茨泵开关;
    [SerializeField] private GameObject 分子泵开关;

    [Space(5)]
    [Header("面板2")]
    [SerializeField] private GameObject 溅射启动开关;
    [SerializeField] private GameObject 溅射电流调节;   // 左键升高，右键降低 (mA)
    [SerializeField] private GameObject 溅射功率调节;   // 左键升高，右键降低 (mA)

    [Space(5)]
    [Header("面板3")]
    [SerializeField] private GameObject Reborn;
    [SerializeField] private GameObject 缓冲气气流调节; // (sccm)
    [SerializeField] private GameObject 溅射气气流调节; // (sccm)
    [SerializeField] private GameObject 面板3开关;

    [Tooltip("调节步长 (电流/功率)")]
    [SerializeField] private float StepLength1 = 1f;

    [Tooltip("调节步长 (气流)")]
    [SerializeField] private float StepLength2 = 1f;

    [Tooltip("包含所有按钮的父物体，用于检测 BoxCollider")]
    [SerializeField] private GameObject ButtonCheck_BoxCollider;

    private void Start()
    {
        if (!Check_BoxCollider())
        {
            Debug.LogError("有控制面板中的按钮未添加 BoxCollider！");
        }
    }

    private bool Check_BoxCollider()
    {
        foreach (Transform child in ButtonCheck_BoxCollider.transform)
        {
            if (child.GetComponent<BoxCollider>() == null)
            {
                Debug.LogWarning(child.name + " 没有 BoxCollider！");
                return false;
            }
        }
        return true;
    }

    private void Update()
    {
        GameObject clickedStatus = RaycastClick_setStatus();
        int clickedData = RaycastClick_setData();

        if (clickedStatus == null && clickedData == -1) return;

        // ===== 面板1 =====
        if (clickedStatus == 水闸电开关) DataBank.ShuiZha = !DataBank.ShuiZha;
        if (clickedStatus == 机械泵开关) DataBank.JiXieBeng = !DataBank.JiXieBeng;
        if (clickedStatus == 罗茨泵开关) DataBank.LuociBeng = !DataBank.LuociBeng;
        if (clickedStatus == 分子泵开关) DataBank.FenZiBeng = !DataBank.FenZiBeng;

        // ===== 面板2 =====
        if (clickedStatus == 溅射启动开关) DataBank.isPanel2_Activated = !DataBank.isPanel2_Activated;

        if (clickedData != -1)
        {
            if (RaycastTarget(溅射电流调节))
            {
                if (clickedData == 0) DataBank.JianShe_I += StepLength1;
                if (clickedData == 1) DataBank.JianShe_I -= StepLength1;
            }

            if (RaycastTarget(溅射功率调节))
            {
                if (clickedData == 0) DataBank.JianShe_P += StepLength1;
                if (clickedData == 1) DataBank.JianShe_P -= StepLength1;
            }
        }

        // ===== 面板3 =====
        if (clickedStatus == 面板3开关) DataBank.isPanel3_Activated = !DataBank.isPanel3_Activated;
        if (clickedStatus == Reborn) DataBank.isPanel3_Reborn = !DataBank.isPanel3_Reborn;

        if (clickedData != -1)
        {
            if (RaycastTarget(缓冲气气流调节))
            {
                if (clickedData == 0) DataBank.HuanChongQiQiLiu += StepLength2;
                if (clickedData == 1) DataBank.HuanChongQiQiLiu -= StepLength2;
            }

            if (RaycastTarget(溅射气气流调节))
            {
                if (clickedData == 0) DataBank.JianSheQiQiLiu += StepLength2;
                if (clickedData == 1) DataBank.JianSheQiQiLiu -= StepLength2;
            }
        }
        // ===== 同步按钮颜色状态 =====
        UpdateButtonColor(水闸电开关, DataBank.ShuiZha);
        UpdateButtonColor(机械泵开关, DataBank.JiXieBeng);
        UpdateButtonColor(罗茨泵开关, DataBank.LuociBeng);
        UpdateButtonColor(分子泵开关, DataBank.FenZiBeng);
        UpdateButtonColor(溅射启动开关, DataBank.isPanel2_Activated);
        UpdateButtonColor(面板3开关, DataBank.isPanel3_Activated);
        UpdateButtonColor(Reborn, DataBank.isPanel3_Reborn);
    }
    private void UpdateButtonColor(GameObject button, bool isOn)
    {
        if (button == null) return;
        Renderer renderer = button.GetComponent<Renderer>();
        if (renderer == null) return;

        Color targetColor = isOn ? Color.red : Color.gray;
        renderer.material.color = targetColor;
    }
    // ========= 射线检测（开关类，返回物体） =========
    private GameObject RaycastClick_setStatus()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit))
            {
                if (hit.collider.GetComponent<BoxCollider>() != null)
                {
                    return hit.collider.gameObject;
                }
            }
        }
        return null;
    }

    // ========= 射线检测（调节类，返回点击方式） =========
    private int RaycastClick_setData() // 左键=0 右键=1 无效=-1
    {
        if (Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(1))
        {
            int mouseButton = Input.GetMouseButtonDown(0) ? 0 : 1;
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit))
            {
                if (hit.collider.GetComponent<BoxCollider>() != null)
                {
                    return mouseButton;
                }
            }
        }
        return -1;
    }

    // ========= 判断最近一次 Raycast 命中是否是目标对象 =========
    private bool RaycastTarget(GameObject target)
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit))
        {
            return hit.collider.gameObject == target;
        }
        return false;
    }
}
