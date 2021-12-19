using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class fpslookcam : MonoBehaviour
{
    [SerializeField]
    private Transform lookRoot;
    [SerializeField]
    private bool invert;
    [SerializeField]
    private bool can_Unlock = true;
    [SerializeField]
    private float sensivity = 0.5f;
    [SerializeField]
    private int smoothSteps = 10;
    [SerializeField]
    private float smoothWeight = 0.4f;
    [SerializeField]
    private float rollAngle = 10f;
    [SerializeField]
    private float rollSpeed = 3f;
    [SerializeField]
    private Vector2 default_look_Limits = new Vector2(-70f, 80f);

    private Vector2 lookAngles;

    private Vector2 current_Mouse_Look;
    private Vector2 smooth_Move;

    private float current_Roll_Angle;
    private int lastlook_Frame;
    [SerializeField]
    private Camera fpscam;

    // Start is called before the first frame update
    void Start()
    {
        Cursor.lockState = CursorLockMode.None;

    }

    // Update is called once per frame
    void Update()
    {
        LockandUnlockCursor();

        if(Input.GetKey(KeyCode.LeftShift))
        {
            LookAround1();
        } else if(Cursor.lockState == CursorLockMode.Locked){
            LookAround();
        }
        if ( Input.GetAxis("Mouse ScrollWheel") > 0){
            fpscam.fieldOfView = Mathf.Clamp(fpscam.fieldOfView - 5f, 5f, 60f);
        }
        if ( Input.GetAxis("Mouse ScrollWheel") < 0){
            fpscam.fieldOfView = Mathf.Clamp(fpscam.fieldOfView + 5f, 5f, 60f);
        }
    }

    void LockandUnlockCursor() {
        if (Input.GetKeyDown(KeyCode.Q))
        {
            if(Cursor.lockState == CursorLockMode.Locked)
            {
                Cursor.lockState = CursorLockMode.None;
            } else
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }
        }
    }

    void LookAround1(){
        lookRoot.localPosition = new Vector3(Mathf.Clamp(lookRoot.localPosition.x + Input.GetAxis("Mouse X")/10000f, 0.0005f, 0.005f), lookRoot.localPosition.y, lookRoot.localPosition.z);
    }

    void LookAround()
    {

        current_Mouse_Look = new Vector2(
            Input.GetAxis("Mouse Y"), Input.GetAxis("Mouse X"));

        lookAngles.x += current_Mouse_Look.x * sensivity * (invert ? 1f : -1f);
        lookAngles.y += current_Mouse_Look.y * sensivity;
        //lookAngles.x = Mathf.Clamp(lookAngles.x, default_look_Limits.x, default_look_Limits.y);
        //current_Roll_Angle = Mathf.Lerp(current_Roll_Angle, Input.GetAxisRaw(MouseAxis.MOUSE_X) * rollAngle, Time.deltaTime * rollSpeed);

        lookRoot.localRotation = Quaternion.Euler(lookAngles.x, lookAngles.y, -2.7f);
        // lookRoot.localRotation = GyroToUnity(Input.gyro.attitude);
        // Debug.Log(GyroToUnity(Input.gyro.attitude));
    }


    // private static Quaternion GyroToUnity(Quaternion q)
    // {
    //     return new Quaternion(q.x, q.y, -q.z, -q.w);

}
