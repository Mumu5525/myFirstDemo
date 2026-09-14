using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraMove : MonoBehaviour
{

    public Transform cameraTarget;
    public Transform orientation;

    #region thirdPerson
    public float distance = 4f;                     //相机到玩家的距离
    public Vector3 offset = new Vector3(0,0.8f,0);    //相机看向玩家上方
    #endregion

    #region firstPerson
    public Vector3 firstPersonOffset = new Vector3(0,0.6f,0f);
    #endregion

    #region Sens
    public float Xsens = 400f;
    public float Ysens = 400f;
    #endregion

    #region pitchLimit
    public float maxPitchFPS = 90f;
    public float minPitchFPS = -90f;
    public float maxPitchTPS = 90f;
    public float minPitchTPS = -90f;
    #endregion

    [Header("人称切换")]
    public KeyCode switchKey = KeyCode.V;
    public bool isThirdPerson = true;

    public LayerMask obstacleMask;
    public float collisionOffset = 0.3f;

    float yaw;
    float pitch;

    // Start is called before the first frame update
    void Start()
    {
        yaw = transform.eulerAngles.y;
        pitch = transform.eulerAngles.x;
    }

    // Update is called once per frame
    void Update()
    {
        yaw += Input.GetAxisRaw("Mouse X") * Xsens * Time.deltaTime;
        pitch -= Input.GetAxisRaw("Mouse Y") * Ysens * Time.deltaTime;
        pitch = isThirdPerson 
        ? Mathf.Clamp(pitch, minPitchTPS, maxPitchTPS) 
        : Mathf.Clamp(pitch, minPitchFPS, maxPitchFPS);

        if(Input.GetKeyDown(switchKey))
            isThirdPerson = !isThirdPerson;

        if (isThirdPerson)
        {
            Vector3 CameraTargetPos = cameraTarget.position + offset;
            Quaternion rot = Quaternion.Euler(pitch, yaw, 0);
            Vector3 direction = rot * Vector3.back;
            float finaldistance = distance;
            if(Physics.Raycast(CameraTargetPos, direction, out RaycastHit hit, distance, obstacleMask))
            {
                finaldistance = Mathf.Max(0.1f,hit.distance - collisionOffset);
            }
            transform.position = CameraTargetPos + rot * new Vector3(0,0,-finaldistance);
            transform.rotation = Quaternion.LookRotation(CameraTargetPos - transform.position);
        }
        else
        {
            transform.position = cameraTarget.position + firstPersonOffset;
            transform.rotation = Quaternion.Euler(pitch, yaw, 0);
        }

        orientation.rotation = Quaternion.Euler(0, yaw, 0);
    }
}
