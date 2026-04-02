using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StepController : MonoBehaviour
{
    [Header("实验步骤控制")]
    public static bool isTuanCuExperiment = true;//判断是否是团簇束流沉积实验
    public static bool isBengPuExperiment = false;//判断是否进入泵浦探测实验

    //技术困点:如何实现拧紧螺丝的效果(初步想法:全局检测,当空格键按下时进行判定是否在需要扭紧的部件,此时再点击左键,弹出√6的UI表示成功,UI弹出后向上渐隐)
    //这个逻辑也是直接在开始时候通过标签添加所有需要拧紧的地方保存到一个public GameObject[]中,最后评分的时候也可以根据这个,判定有多少个没有拧紧来打分

    //开场先播放开场动画
    public static bool Step1_1 = false;//放置光学元件,对应UI按钮界面出现,结束后隐藏
    public static bool Step1_2 = false;//上移旋转基座,同时安装BBO薄膜(?),后归位
    public static bool Step1_3 = false;//移动电机,确认时间零点
    public static bool Step1_4 = false;//上移旋转基座,取下BBO,安装叉指电极、石英玻璃片，归位,注意设置步骤标志位
    public static bool step1_5 = false;//沿轨道移动团簇束流源，安放靶材，归位 -->tracedDrag
    public static bool step1_6 = false;//装载靶材
    public static bool step1_7 = false; //从这一步开始,右下角UI出现,使得操作者可以随时回到电控台操作按钮打开旁抽阀,打开溅射气门、缓冲气门
                                        //-->点击左右侧修改为按下左右键(左键开,右键关闭,同时加上鼠标浮在上方的变深色效果)
    public static bool step1_8 = false;//
    public static bool step1_9 = false;//
    public static bool step1_10 = false;//

    //备注: 数码管显示规则-->所有仪器的数码管数字都设置成绿色，电源开关关闭时是黑色，打开时则是红色。

    //以下是泵浦探测步骤:
    public static bool step2_1 = false;//?
    public static bool step2_2 = false;//?打开光源,开始光线汇聚;通过旋转按钮实现
    public static bool step2_3 = false;//?相位调整装置发生位移，同时用黄色和蓝色的光分别显示泵光探光||显示光路问题不大
    public static bool step2_4 = false;//?一段时间后关闭光源,得到实验数据

    public static bool AllowToShowLightPath = false;
}
