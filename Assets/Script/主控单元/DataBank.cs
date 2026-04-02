 using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DataBank : MonoBehaviour
{
    [Header("传入计算的参数")]
    /*
    1，基态吸收截面    单位：m^2    范围：8e-22~1.8e-21
    2，单重态第一激发态吸收截面    单位：m^2    范围：2.4e-21~3.0e-21
    3，三重态第一激发态吸收截面    单位：m^2    范围：1.2e-21~1.8e-21
    4，双光子吸收系数    单位：W/m^2    范围：0e-11~0e-9
    5，单重态第一激发态寿命    单位：s    范围：8e-9~1.2e-8
    6，单重态更高激发态寿命     单位：s     范围：80e-15~120e-15
    7，三重态第一激发态寿命    单位：s     范围：1.8e-10~2.2e-10
    8，三重态更高激发态寿命    单位：s     范围：80e-15~120e-15
    9，系间窜跃时间    单位：s     范围：0.8e-11~0e-1.2e-11
     */
    //public static float 基态吸收截面;
    //public static float 单重态第一激发态吸收截面;
    //public static float 三重态第一激发态吸收截面;
    //public static float 双光子吸收系数;
    //public static float 单重态第一激发态寿命;
    //public static float 单重态更高激发态寿命;
    //public static float 三重态第一激发态寿命;
    //public static float 三重态更高激发态寿命;
    //public static float 系间窜跃时间;
    public static List<float> paramList = new List<float>();
    //[Space(20)]
    //[Header("控制箱面板参数")]
    //[Header("面板1")]
    //public static bool ShuiZha = false;
    //public static bool JiXieBeng = false;
    //public static bool LuociBeng = false;
    //public static bool FenZiBeng = false;
    //[Space(5)]
    //[Header("面板2")]
    //public static bool isPanel2_Activated = false;
    //public static float HuanChongQi_Z;//Z|L分别代表电离规和电阻规
    //public static float JianSheQi_Z;
    //public static float HuanChongQi_L;
    //public static float JianSheQi_L;
    //public static float HuanChongQiQiLiu;
    //public static float JianSheQiQiLiu;
    //[Space(5)]
    //[Header("面板3")]
    //public static float JianShe_I;
    //public static float JianShe_P;
    //public static bool isPanel3_Reborn;
    //public static bool isPanel3_Activated;
    //[Space(20)]
    [Header("实验过程中需要处理的参数")]
    public static float posibilityToCheckGasTightnessAgain = 0.5f;
    [Header("阀门")]
    public static bool pangchou_Valve;//旁抽阀
    public static bool JianSheQiMen;//溅射气门
    public static bool HuanChongQiMen;//缓冲气门
    public static bool Jiaofa;//角阀
    public static bool JianYafa;//减压阀

    //============================监控版本========================
    // ================= 面板1 =================
        private static bool _ShuiZha = false;
        public static bool ShuiZha
        {
            get => _ShuiZha;
            set
            {
                if (_ShuiZha != value)
                {
                    _ShuiZha = value;
                    Debug.Log($"面板1: 水闸电开关 变化为 {_ShuiZha}");
                }
            }
        }

        private static bool _JiXieBeng = false;
        public static bool JiXieBeng
        {
            get => _JiXieBeng;
            set
            {
                if (_JiXieBeng != value)
                {
                    _JiXieBeng = value;
                    Debug.Log($"面板1: 机械泵开关 变化为 {_JiXieBeng}");
                }
            }
        }

        private static bool _LuociBeng = false;
        public static bool LuociBeng
        {
            get => _LuociBeng;
            set
            {
                if (_LuociBeng != value)
                {
                    _LuociBeng = value;
                    Debug.Log($"面板1: 罗茨泵开关 变化为 {_LuociBeng}");
                }
            }
        }

        private static bool _FenZiBeng = false;
        public static bool FenZiBeng
        {
            get => _FenZiBeng;
            set
            {
                if (_FenZiBeng != value)
                {
                    _FenZiBeng = value;
                    Debug.Log($"面板1: 分子泵开关 变化为 {_FenZiBeng}");
                }
            }
        }

        // ================= 面板3 =================
        private static bool _isPanel2_Activated = false;
        public static bool isPanel2_Activated
        {
            get => _isPanel2_Activated;
            set
            {
                if (_isPanel2_Activated != value)
                {
                    _isPanel2_Activated = value;
                    Debug.Log($"面板2: 面板激活状态 变化为 {_isPanel2_Activated}");
                }
            }
        }

        private static float _HuanChongQiQiLiu=50f;
        public static float HuanChongQiQiLiu
        {
            get => _HuanChongQiQiLiu;
            set
            {
                if (_HuanChongQiQiLiu != value)
                {
                    _HuanChongQiQiLiu = value;
                    Debug.Log($"面板3: 缓冲气气流调节 变化为 {_HuanChongQiQiLiu}");
                }
            }
        }

        private static float _JianSheQiQiLiu=60f;
        public static float JianSheQiQiLiu
        {
            get => _JianSheQiQiLiu;
            set
            {
                if (_JianSheQiQiLiu != value)
                {
                    _JianSheQiQiLiu = value;
                    Debug.Log($"面板3: 溅射气气流调节 变化为 {_JianSheQiQiLiu}");
                }
            }
        }

        // ================= 面板2 =================
        private static bool _isPanel3_Reborn;
        public static bool isPanel3_Reborn
        {
            get => _isPanel3_Reborn;
            set
            {
                if (_isPanel3_Reborn != value)
                {
                    _isPanel3_Reborn = value;
                    Debug.Log($"面板2: Reborn开关 变化为 {_isPanel3_Reborn}");
                }
            }
        }

        private static bool _isPanel3_Activated;
        public static bool isPanel3_Activated
        {
            get => _isPanel3_Activated;
            set
            {
                if (_isPanel3_Activated != value)
                {
                    _isPanel3_Activated = value;
                    Debug.Log($"面板2: 面板激活状态 变化为 {_isPanel3_Activated}");
                }
            }
        }

        private static float _JianShe_I=50f;
        public static float JianShe_I
        {
            get => _JianShe_I;
            set
            {
                if (_JianShe_I != value)
                {
                    _JianShe_I = value;
                    Debug.Log($"面板2: 电流调节 变化为 {_JianShe_I}");
                }
            }
        }

        private static float _JianShe_P=60f;
        public static float JianShe_P
        {
            get => _JianShe_P;
            set
            {
                if (_JianShe_P != value)
                {
                    _JianShe_P = value;
                    Debug.Log($"面板2: 功率调节 变化为 {_JianShe_P}");
                }
            }
        }
    }
//("                     o8888888o                    \n")
//("                     88' . '88                    \n")
//("                     (| -_- |)                    \n")
//("                     0\\  =  /0                    \n")
//("                   ___/`---'\\___                  \n")
//("                 .' \\\\|     |// '.                \n")
//("                / \\\\|||  :  |||// \\               \n")
//("               / _||||| -:- |||||_ \\              \n")
//("              |   | \\\\\\  -  /// |   |             \n")
//("              | \\_|  ''\\---/''  |_/ |             \n")
//("              \\ .-\\___  '-'  ___/-. /             \n")
//("            ___'. .'  /--.--\\  `. .'___           \n")
//("          .'' '< `.___\\_<|>_/___.' >'  ''.        \n")
//("        | | ： `- \\`.;`\\ _ /`;.`/ - ` : | |       \n")
//("        \\  \\ `_.   \\_ __\\ /__ _/   .-` /  /       \n")
//("    =====`-.____`.___ \\_____/___.-`___.-`=====    \n")
//("                      `=---='                     \n")
//("    ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~    \n")
//("\033[1;30m    菩提本无树   明镜亦非台              \n")
//("              本来无BUG   何必常修改              \033[0m\n")


