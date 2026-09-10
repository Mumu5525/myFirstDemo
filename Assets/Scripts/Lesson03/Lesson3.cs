using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Lession3 : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        // 打印游戏对象的名称
        print(this.gameObject.name);
        // 打印游戏对象的标签
        print(this.gameObject.tag);
        // 打印游戏对象的变换组件的位置
        print(this.transform.position);
        // 打印游戏对象的变换组件的旋转
        print(this.transform.rotation);
        // 打印游戏对象的变换组件的欧拉角
        print(this.transform.eulerAngles);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
