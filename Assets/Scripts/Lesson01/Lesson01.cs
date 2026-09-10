using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Lession01 : MonoBehaviour
{
    //出生时调用，类似构造函数，一个对象只会调用一次
    void Awake()
    {
        Debug.Log("Awake");
    }

    //依附的GameObject每次激活时调用
    void OnEnable()
    {
        Debug.Log("OnEnable");
    }

    // Start is called before the first frame update
    void Start()
    {
        Debug.Log("Start");
    }

    //FixedUpdate固定帧调用一次，与时间无关，可以设置间隔
    void FixedUpdate()
    {
        Debug.Log("FixedUpdate");
    }

    // Update is called once per frame
    void Update()
    {
        Debug.Log("Update");
        
    }

    //LateUpdate在Update之后调用
    void LateUpdate()
    {
        Debug.Log("LateUpdate");
    }

    //OnDisable在依附的GameObject被禁用时调用
    void OnDisable()
    {
        Debug.Log("OnDisable");
    }

    //OnDestroy在依附的GameObject被销毁时调用
    void OnDestroy()
    {
        Debug.Log("OnDestroy");
    }
}
