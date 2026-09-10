using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public enum MyEnum
{
    Red,
    Green,
    Blue
}

public struct MyStruct
{
    public int int1;
    public float float1;
}

[System.Serializable]
public struct MyStruct2
{
    public int int2;
    public float float2;
}
public class Lession2 : MonoBehaviour
{
    //Inspector显示的可编辑内容就是脚本的成员变量


    //一 私有和保护无法显示编辑
    #region 
    private int privateInt;
    protected int protectedInt;
    #endregion

    //二 强制让私有和保护可以显示编辑
    #region 加上SerializeField即可
    [SerializeField] private int privateInt2;
    [SerializeField] protected int protectedInt2;
    #endregion

    //三 公共可以显示编辑
    #region 公共和受保护可以显示编辑
    public int publicInt;
    #endregion

    //四 公共的也不让显示编辑
    #region 公共也不让显示编辑
    [HideInInspector] public int publicInt2;
    #endregion

    //五 大部分类型都可以显示编辑
    #region 大部分类型都可以显示编辑

    //可编辑举例
    public int[] publicIntArray3;
    public float publicFloat;
    public string publicString;
    public MyEnum publicEnum;

    //不可编辑举例
    public Dictionary<int, int> publicDictionary;
    public MyStruct publicStruct;
    #endregion

    //六 自定义类型可以显示编辑
    #region 自定义类型可以显示编辑
    public MyStruct2 publicStruct2;
    #endregion

    //七 辅助的特性
    #region 辅助的特性
    [Header("这是一个标题")]
    public int publicInt3;

    [Tooltip("这是一个提示")]
    public int publicInt4;

    [Space]
    public int publicInt5;

    [Range(0, 100)]
    public int publicInt6;

    [Multiline(3)]
    public string publicString2;

    [TextArea(3, 5)]
    public string publicString3;

    [ContextMenuItem("重置按钮","Reset")]
    public int resetInt;
    public void Reset()
    {
        resetInt = 0;
    }

    [ContextMenu("测试按钮")]
    public void TestButton()
    {
        Debug.Log("测试按钮被点击了");
    }
    #endregion
}
