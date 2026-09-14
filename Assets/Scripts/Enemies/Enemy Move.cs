using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NewBehaviourScript : MonoBehaviour
{
    public Transform model;
    public LayerMask groundLayer;
    public Transform orientation;
    Rigidbody rb;
    Animator anim;
    public Transform groundCheck;
    //移动方向
    Vector3 moveDirection;

    public EnemyData data;
    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        anim = GetComponentInChildren<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
    
    }

    private void FixedUpdate()
    {
        
    }

}
