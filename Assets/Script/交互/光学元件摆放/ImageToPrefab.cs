using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class ImageToPrefab : MonoBehaviour
{
    //本脚本旨在整合Drag3D 的拖拽以及吸附功能和点击对应按钮产生预制件的功能,降低代码之间的耦合度

    //脚本挂在点击生成的那个按键上
    [Header("按键&光学元件 基本设置")]
    [SerializeField] private int _maxCnt;//每个光学元件最大生成数量,根据预制光学元件组手动传入

    [Space(10)]

    [Header("目标预制件 设置")]
    [SerializeField] private string TargetObject;//手动输入目标光学元件的名字:需要与Resources文件夹下的名称一致
    [SerializeField] public string _destObjectTag;//在这里设置目标元件的名字,将在Drag3D里调用这个public的string

    private Collider destobjCollider;//生成物体的Collider组件
    private GameObject[] destObjects; // 存储所有目标位置
    private GameObject _objDragSpawning;//暂存点击按钮后生成的物体
    private int _instantiateCount;//已经生成的数量 用于判断是否过量生成
    private Button _instantiateButton;//绑定按钮
    private Plane _dragPlane;//拖拽的目标平面,用于射线检测
    private string _planeTag = "PlacementPlane";//默认的射线检测平面标签 无需手动修改
    private void Start()
    {

        destobjCollider = GetComponent<Collider>();
        destObjects = GameObject.FindGameObjectsWithTag(_destObjectTag);
        _instantiateButton = GetComponent<Button>();
        _instantiateButton.onClick.AddListener(StartDragSpawning);
        _instantiateCount = 0;
    }
    private void StartDragSpawning()
    {
        if (_instantiateCount >= _maxCnt) return;
        GameObject prefab = Resources.Load<GameObject>(TargetObject);
        if (prefab == null)
        {
            Debug.LogError("Can't find the certain Prefab in this path");
            return;
        }
        GameObject TargetPlane = GameObject.FindGameObjectWithTag(_planeTag);
        if (TargetPlane == null)
        {
            Debug.LogError("Can't find certain TargetPlane in this path");
            return;
        }
        _dragPlane = new Plane(Vector3.up, TargetPlane.transform.position);

        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (_dragPlane.Raycast(ray, out float enter))
        {
            Vector3 spawnPos = ray.GetPoint(enter);

            _objDragSpawning = GameObject.Instantiate(prefab, spawnPos, Quaternion.identity);
            _objDragSpawning.AddComponent<Drag3D>();
            if (_objDragSpawning.TryGetComponent<Drag3D>(out var drag3d))
                drag3d.DestObjectTag = _destObjectTag;

            // 获取或添加Rigidbody组件
            Rigidbody rb = _objDragSpawning.GetComponent<Rigidbody>();
            if (rb == null)
                rb = _objDragSpawning.AddComponent<Rigidbody>();

            // 获取或添加BoxCollider组件
            BoxCollider bc = _objDragSpawning.GetComponent<BoxCollider>();
            if (bc == null)
                bc = _objDragSpawning.AddComponent<BoxCollider>();

            bc.center = new Vector3(-0.1f, 0.4f, 0);
            bc.size = new Vector3(0.4f, 1, 0.4f);
            rb.isKinematic = true;

            _instantiateCount++;
            enabled = true;
        }
    }
    private void Update()
    {
        if (_objDragSpawning == null)
        {
            enabled = false;
            return;
        }
        UpdateDragPosition();
        if (Input.GetMouseButtonUp(0))
        {
            _objDragSpawning = null;
            enabled = false;
        }
    }
    private void UpdateDragPosition()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (_dragPlane.Raycast(ray, out float enter))
        {
            Vector3 newPos = ray.GetPoint(enter);
            _objDragSpawning.transform.position = newPos;
        }
    }
}
