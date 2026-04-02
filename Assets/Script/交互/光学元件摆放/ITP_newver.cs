using System;
using System.Net.Http.Headers;
using UnityEngine;
using UnityEngine.UI;

public class ITP_nerver : MonoBehaviour
{
    [Serializable]
    public class ParamControl
    {
        public Button _button;            // 生成按钮
        public int _maxCnt;               // 最大生成数
        public string TargetObject;       // Prefab 名称（Resources 下）
        public string _destObjectTag;     // 目标位置的 tag
    }

    [Header("统一配置")]
    public ParamControl[] controls;
    public Button FinishOEL;//绑定一个标志结束光学元件摆放的按键
    public GameObject LineSys;//绑定所有光学元件的连接线
    public GameObject BackpackPanel;


    private Plane _dragPlane;
    private string _planeTag = "PlacementPlane";

    private GameObject _objDragSpawning;
    private int[] _instantiateCounts;
    private float PlaneOffset;//元件高度与平面高度偏移=>y正方向上

    private void Start()
    {
        PlaneOffset = 0.473f;
        FinishOEL.onClick.AddListener(NextStep);
        if(StepController.Step1_1)
            FinishOEL.gameObject.SetActive(true);
        GameObject TargetPlane = GameObject.FindGameObjectWithTag(_planeTag);
        if (TargetPlane == null)
        {
            Debug.LogError("没有找到拖拽平面 PlacementPlane");
            return;
        }
        _dragPlane = new Plane(Vector3.up, TargetPlane.transform.position + new Vector3(0,PlaneOffset,0));

        _instantiateCounts = new int[controls.Length];
        for (int i = 0; i < controls.Length; i++)
        {
            int index = i;
            controls[i]._button.onClick.AddListener(() => StartDragSpawning(index));
        }
    }
    private void NextStep()
    {
        StepController.Step1_1 = false;
        StepController.Step1_2 = true;

        FinishOEL.gameObject.SetActive(false);
        FinishOEL.enabled = false;
        LineSys.SetActive(true);
        BackpackPanel.SetActive(false);
    }
    private void StartDragSpawning(int index)
    {
        if (_instantiateCounts[index] >= controls[index]._maxCnt) return;

        GameObject prefab = Resources.Load<GameObject>(controls[index].TargetObject);
        if (prefab == null)
        {
            Debug.LogError($"Prefab {controls[index].TargetObject} 未找到");
            return;
        }

        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (_dragPlane.Raycast(ray, out float enter))
        {
            Vector3 spawnPos = ray.GetPoint(enter);
            _objDragSpawning = Instantiate(prefab, spawnPos, Quaternion.identity);
            _objDragSpawning.transform.position = new Vector3(-6, 6.907f, 4.8f);//光学元件摆放=>new Vector3(-6,6.907f,4.8f)
            // 添加并初始化 Drag3D 组件
            Drag3D drag = _objDragSpawning.AddComponent<Drag3D>();
            drag.DestObjectTag = controls[index]._destObjectTag;
            drag.manager = this;
            drag.ForceStartDrag();

            // 修复Rigidbody组件获取逻辑
            Rigidbody rb = _objDragSpawning.GetComponent<Rigidbody>();
            // 如果没有Rigidbody组件，则添加一个
            if (rb == null)
            {
                rb = _objDragSpawning.AddComponent<Rigidbody>();
            }
            // 确保设置isKinematic为true
            rb.isKinematic = true;

            // 碰撞器设置
            BoxCollider bc = _objDragSpawning.GetComponent<BoxCollider>();
            if (bc == null)
            {
                bc = _objDragSpawning.AddComponent<BoxCollider>();
            }
            bc.center = Vector3.zero;
            bc.size = new Vector3(0.5f, 0.5f, 0.5f);

            _instantiateCounts[index]++;
        }
    }


    //private void Update()
    //{
    //    if (_objDragSpawning == null)
    //    {
    //        enabled = false;
    //        return;
    //    }
    //    UpdateDragPosition();
    //    if (Input.GetMouseButtonUp(0))
    //    {
    //        _objDragSpawning = null;
    //        enabled = false;
    //    }
    //}

    //private void UpdateDragPosition()
    //{
    //    Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
    //    if (_dragPlane.Raycast(ray, out float enter))
    //    {
    //        Vector3 newPos = ray.GetPoint(enter);
    //        _objDragSpawning.transform.position = newPos;
    //    }
    //}


}
