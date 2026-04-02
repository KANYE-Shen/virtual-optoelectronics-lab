using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ThermoelectricityParams : MonoBehaviour
{
    //public static float CurrentTemperature;
    //public static float MinVoltage;
    //public static float MaxVoltage;
    //public static float VoltageStepSize;
    //上述参数按顺序依次为1234
    public static List<float> paramList = new List<float>();

    //顺便在这也存一下另外不知道什么面板的一个参数:溅射气流 缓冲气流 溅射电流 溅射功率 溅射时间

    public static List<float> paramList2 = new List<float>();
}
