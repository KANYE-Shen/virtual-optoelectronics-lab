using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Playables;
using Cinemachine;

public class SceneInit : MonoBehaviour
{
    private Camera mainCamrera;
    [Header("Cinemachine导演")]
    [SerializeField] private PlayableDirector director;
    [Header("相机初始化物体")]
    [SerializeField] private GameObject DefaultCameraPos;

    [Header("场景初始化")]
    [SerializeField] private GameObject LineSys;//连接电线组合,仅在播放时出现
    [SerializeField] private GameObject DisplayOnly_OPT;//仅展示光学元件组合,仅在播放时出现
    
    [SerializeField] private GameObject DestSys_OPT;//dest光学元件组,用于操作

    [Header("UI初始化")]
    [SerializeField] private Canvas canvas;//初始化UI在开场动画时均不展示
    private void Start()
    {
        LineSys.SetActive(true);
        DisplayOnly_OPT.SetActive(true);
        DestSys_OPT.SetActive(false);
        canvas.gameObject.SetActive(false);
        mainCamrera = Camera.main;
        

        if(director != null)
        {
            director.stopped += OnTimelineStopped;
        }
    }
    private void OnTimelineStopped(PlayableDirector obj)
    {
        if (LineSys != null)
        {
            // Timeline 结束时设置物体状态
            Camera.main.GetComponent<CinemachineBrain>().enabled = false;
            LineSys.SetActive(false);
            DisplayOnly_OPT.SetActive(false);
            DestSys_OPT.SetActive(true);
            canvas.gameObject.SetActive(true);
            Debug.Log("Timeline结束，已关闭物体: " + LineSys.name);
            mainCamrera.transform.position = DefaultCameraPos.transform.position;
            mainCamrera.transform.rotation = DefaultCameraPos.transform.rotation;
            Camera.main.GetComponent<CinemachineBrain>().enabled = true;
            StepController.Step1_1 = true;
        }
    }
    private void OnDestroy()
    {
        if(director !=null)
        director.stopped -= OnTimelineStopped;
    }
}
