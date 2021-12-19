using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class sf1 : MonoBehaviour
{
    [SerializeField]
    private float throttlepercent, maxenginepower, rpm, currentep;
    [SerializeField]
    private Vector3 finalforwardforce;
    [SerializeField]
    private Rigidbody rb;
    [SerializeField]
    private bool throttlezero, throttleidle, yawingright, yawingleft, rollingright, rollingleft, pitchingup, pitchingdown;
    [SerializeField]
    private bool isgrounded;
    [SerializeField]
    private float currentheightfromterrain;
    [SerializeField]
    private Transform tirefront;
    private RaycastHit hit = new RaycastHit();
    [SerializeField]
    private float currentdrag;
    private float yawrate;
    [SerializeField]
    private float currentmach, TAS;
    private float speedofsound = 661.4788f;
    [SerializeField]
    private float turnRate = 100f;
    [SerializeField]
    private float currentairspeed;
    [SerializeField]
    private bool gearup;
    [SerializeField]
    private Transform rightgear, leftgear;
    [SerializeField]
    private Transform rightaileron, leftaileron, elevator, rudder;
    [SerializeField]
    private Transform prop;
    [SerializeField]
    private float rpmmax;
    [SerializeField]
    private Transform speedometerpointer;
    [SerializeField]
    private GameObject gearuplight, geardownlight;
    [SerializeField]
    private Transform arhorpointer;
    private float roll, pitch;
    [SerializeField]
    private Transform stick, thousandfeetpointer, hundredfeetpointer, tenthousandfeetpointer, rpmpointer;
    [SerializeField]
    private float currentrpm;
    [SerializeField]
    private Transform throttlestick;
    [SerializeField]
    private float mainfueselageuppertank, mainfueselagelowertank, droptank1, droptank2;
    private string currentfuelsource;
    private float totalfuelingals;
    private bool fuelempty;
    [SerializeField]
    private float fueldecreaseconst;


    void Start(){
        throttlezero = false;
        throttleidle = false;
        yawingright = false;
        yawingleft = false;
        rollingright = false;
        rollingleft = false;
        gearup = false;
        mainfueselagelowertank = 49f;
        mainfueselageuppertank = 47f;
        droptank1 = 13f;
        droptank2 = 13f;
        currentfuelsource = "droptank1";
        fuelempty = false;

    }

    void Update(){

        currentairspeed = rb.velocity.magnitude * 1.1f;
        currentmach = currentairspeed/661.4788f;
        TAS = speedofsound * currentmach;

        Vector3 pos;

        pos = ProjectPointOnPlane(Vector3.up, Vector3.zero, transform.right);
        roll = SignedAngle(transform.right, pos, transform.forward);

        pos = ProjectPointOnPlane(Vector3.up, Vector3.zero, transform.forward);
        pitch = SignedAngle(transform.forward, pos, transform.right);

        CheckThrottle();
        EngineForce();
        CheckYaw();
        //CheckLift();
        CheckDrag();
        CheckRoll();
        CheckPitch();
        getHeight();
        CheckGear();
        CheckControlSurfaces();
        CheckPropRot();
        CheckCockpitInstruments();
        CheckBank();
        CheckFuel();

    }


    void CheckCockpitInstruments(){
      // GEAR LIGHT FUNCTIONS /////////////////////////
      if(gearup == true){
        gearuplight.SetActive(true);
        geardownlight.SetActive(false);
      } else if(gearup == false){
        gearuplight.SetActive(false);
        geardownlight.SetActive(true);
      }
      /////////////////////////////////////////////////

      // SPEEDOMETER FUNCTIONS ///////////////////////
      if(currentairspeed >= 0f && currentairspeed <= 50f){
        speedometerpointer.localRotation = Quaternion.Euler(-180f, 90f - (currentairspeed/2f), 0f);
      } else if(currentairspeed > 50f){
        speedometerpointer.localRotation = Quaternion.Euler(-180f, 65f - (currentairspeed*(5f/7f)), 0f);
      }
      ////////////////////////////////////////////////

      // ARTIFICIAL HORIZON FUNCTION ////////////////
      arhorpointer.localRotation = Quaternion.Euler(279.421f, 87.78001f, 2.367f + roll);
      arhorpointer.localPosition = new Vector3(0.009f, -1.25f, Mathf.Clamp(-0.036f + pitch * (0.292f/90f) , -0.256f, 0.256f));
      //////////////////////////////////////////////

      // STICK MOVEMENT ///////////////////////////
      stick.localRotation = Quaternion.Euler(Input.GetAxis("Vertical") * 11f, 0f, -Input.GetAxis("Horizontal") * 11f);
      ////////////////////////////////////////////

      // ALTIMETER FUNCTIONS//////////////////////

      thousandfeetpointer.localRotation = Quaternion.Euler(-178.91f, -90f - (((currentheightfromterrain/1000f) % 10f)*36f), -0.01300049f);
      hundredfeetpointer.localRotation = Quaternion.Euler(-178.91f, -90f - (((currentheightfromterrain/100f) % 100f)*36f), -0.01300049f);
      tenthousandfeetpointer.localRotation = Quaternion.Euler(-178.91f, -90f - ((currentheightfromterrain/10000f)*36f), -0.01300049f);

      ///////////////////////////////////////////

      // RPM FUNCTIONS /////////////////////////
      currentrpm = (currentep/maxenginepower)*rpmmax;
      rpmpointer.localRotation = Quaternion.Euler(-82.58801f, 93.63f, 90f - (currentrpm * (340f/rpmmax)));
      //////////////////////////////////////////

      // THROTTLE STICK FUNCTIONS /////////////
      throttlestick.localRotation = Quaternion.Euler(0f, 0f, 44f - (throttlepercent/100f) * 80f);
      ////////////////////////////////////////
    }

    void CheckFuel(){
      if(currentfuelsource == "mainfueselageuppertank"){
        if(mainfueselageuppertank > 0f){
            mainfueselageuppertank -= fueldecreaseconst * (currentep/maxenginepower);
            fuelempty = false;
        } else{
          fuelempty = true;
        }
      } else if(currentfuelsource == "mainfueselagelowertank"){
        if(mainfueselagelowertank > 0f){
            mainfueselagelowertank -= fueldecreaseconst * (currentep/maxenginepower);
            fuelempty = false;
        } else{
          fuelempty = true;
        }
      } else if(currentfuelsource == "droptank1"){
        if(droptank1 > 0f){
            droptank1 -= fueldecreaseconst * (currentep/maxenginepower);
            fuelempty = false;
        } else{
          fuelempty = true;
        }
      } else if(currentfuelsource == "droptank2"){
        if(droptank2 > 0f){
            droptank2 -= fueldecreaseconst * (currentep/maxenginepower);
            fuelempty = false;
        } else{
          fuelempty = true;
        }
      }
      totalfuelingals = mainfueselageuppertank + mainfueselagelowertank + droptank1 + droptank2;
    }

    Vector3 ProjectPointOnPlane(Vector3 planeNormal , Vector3 planePoint , Vector3 point ){
       planeNormal.Normalize();
       var distance = -Vector3.Dot(planeNormal.normalized, (point - planePoint));
       return point + planeNormal * distance;
   }

   float SignedAngle(Vector3 v1, Vector3 v2, Vector3 normal){
       var perp = Vector3.Cross(normal, v1);
       var angle = Vector3.Angle(v1, v2);
       angle *= Mathf.Sign(Vector3.Dot(perp, v2));
       return angle;
   }

    void CheckBank(){

      if(roll >= -60f && roll <= 60f){
        Vector3 torqueforce = transform.up * roll * 5f;
        rb.AddTorque(torqueforce);
        rb.AddTorque( transform.right * -0.005f * Mathf.Abs(roll) * Mathf.Clamp(currentairspeed * 100f, 200f, 550f));
      }


    }

    void CheckPropRot(){
      prop.Rotate(0f, (currentep/maxenginepower)*rpmmax, 0f);
    }

    void CheckGear(){
      if (Input.GetKeyUp(KeyCode.G)){
           if(gearup == true){
               gearup = false;
           } else if (gearup == false){
               gearup = true;
           }
       }

       if (gearup == true){
           rightgear.localRotation = Quaternion.Euler(182.438f, -80.02301f, 0.01498413f);
           leftgear.localRotation = Quaternion.Euler(-164.771f, 69.23199f, 0.03799438f);

       }
       if (gearup == false){
           rightgear.localRotation = Quaternion.Euler(108.513f, -79.97699f, 0.04798889f);
           leftgear.localRotation = Quaternion.Euler(-238.554f, 69.30199f, 0.06999207f);

       }
    }

    void CheckThrottle(){

        if (Input.GetKey(KeyCode.Alpha4)){
            throttleidle = false;
            throttlezero = true;
            throttlepercent = 0f;

        }
        if (Input.GetKey(KeyCode.Alpha5)){
            throttlepercent = 0f;
            throttlezero = false;
            throttleidle = true;
        }

        if (Input.GetKey(KeyCode.Alpha6)){
            throttlepercent = 10;
            throttlezero = false;
            throttleidle = false;
        }

        if (Input.GetKey(KeyCode.Alpha7)){
            throttlepercent = 30;
            throttlezero = false;
            throttleidle = false;
        }
        if (Input.GetKey(KeyCode.Alpha8)){
            throttlepercent = 50;
            throttlezero = false;
            throttleidle = false;
        }

        if (Input.GetKey(KeyCode.Alpha9)){
            throttlepercent = 80;
            throttlezero = false;
            throttleidle = false;
        }

        if (Input.GetKey(KeyCode.Alpha0)){
            throttlepercent = 100;
            throttlezero = false;
            throttleidle = false;
        }

        if(currentep < (throttlepercent/100f) * maxenginepower && fuelempty == false){

            currentep += 0.1f;

        } else if(currentep > (throttlepercent/100f) * maxenginepower){

            currentep -= 0.1f;

        }

        if(fuelempty == true && currentep > 0f){
          currentep -= 0.01f;
        }

    }

    void CheckYaw(){

        if(isgrounded == true){

            yawrate = 5f * currentep;

        } else{

            yawrate = 1.3f * currentep;

        }
        if (Input.GetKey(KeyCode.Delete)){
            Vector3 torqueforce = transform.up * -yawrate;
            rb.AddTorque(torqueforce);
            yawingleft = true;
        } else if(Input.GetKey(KeyCode.Delete) is false){
          yawingleft = false;
        }
        if (Input.GetKey(KeyCode.PageDown)){
            Vector3 torqueforce = transform.up * yawrate;
            rb.AddTorque(torqueforce);
            yawingright = true;
        } else if(Input.GetKey(KeyCode.PageDown) is false){
          yawingright = false;
        }
        rb.angularVelocity = Vector3.zero;

    }



    void CheckDrag(){
        if (isgrounded == true && Input.GetKey(KeyCode.B) == false){
            currentdrag = 1f;
        }
        else if(isgrounded == false && Input.GetKey(KeyCode.B) == false) {
            currentdrag = 3f;
        }
        else if(Input.GetKey(KeyCode.B) && isgrounded == true){
            currentdrag = Mathf.Lerp(currentdrag, 1.5f, Time.deltaTime * 1f);
        }
        else if(Input.GetKey(KeyCode.B) && isgrounded == false){
            currentdrag = Mathf.Lerp(currentdrag, 4f, Time.deltaTime * 1f);
        }
        rb.drag = currentdrag;
    }

    void EngineForce(){

        finalforwardforce = transform.forward * currentep * 1.5f;
        rb.AddForce(finalforwardforce);

    }

    void CheckRoll(){



        rb.AddTorque( transform.forward * -Input.GetAxis("Horizontal") * 2500f );
        if(Input.GetAxis("Horizontal") > 0){
          rollingright = true;
        } else{
          rollingright = false;
        }
        if(Input.GetAxis("Horizontal") < 0){
          rollingleft = true;
        } else{
          rollingleft = false;
        }

    }

    void CheckPitch(){

        if (isgrounded == false){
            rb.AddTorque( transform.right * Input.GetAxis("Vertical") * Mathf.Clamp(currentairspeed * 400f, 200f, 550f));
            }
        else if(isgrounded == true && currentairspeed > 20f){
            rb.AddTorque( transform.right * Input.GetAxis("Vertical") * Mathf.Clamp(currentairspeed * 100f, 200f, 550f));
        }
        if(Input.GetAxis("Vertical") > 0){
          pitchingup = true;
        } else{
          pitchingup = false;
        }
        if(Input.GetAxis("Vertical") < 0){
          pitchingdown = true;
        } else{
          pitchingdown = false;
        }

    }

    void CheckControlSurfaces(){
      if (yawingleft == true){
        rudder.localRotation = Quaternion.Euler(-90.00001f, 0f, -11.66f);
        }
      if (yawingright == true){
            rudder.localRotation = Quaternion.Euler(-90.00001f, 0f, 11.66f);
        }
      if (yawingleft == false && yawingright == false){
            rudder.localRotation = Quaternion.Euler(-90.00001f, 0f, 0f);
        }
      if (pitchingdown == true){
            elevator.localRotation = Quaternion.Euler(-102.244f, 0f, -1.525879f);
        }
      if (pitchingup == true){
            elevator.localRotation = Quaternion.Euler(-73.40701f, 0f, 0f);
        }
      if (pitchingup == false && pitchingdown == false){
            elevator.localRotation = Quaternion.Euler(-90.00001f, 0f, 0f);

        }
      if (rollingleft == true){
            rightaileron.localRotation = Quaternion.Euler(-114.987f, 0f, 0f);
            leftaileron.localRotation = Quaternion.Euler(-66.013f, 0f, 0f);
        }
        if (rollingright == true){
            rightaileron.localRotation = Quaternion.Euler(-66.013f, 0f, 0f);
            leftaileron.localRotation = Quaternion.Euler(-114.987f, -10.527f, 0f);
        }
      if (rollingright == false && rollingleft == false){
            rightaileron.localRotation = Quaternion.Euler(-90.00001f, 0f, 0f);
            leftaileron.localRotation = Quaternion.Euler(-90.00001f, 0f, 0f);
        }

      }



    void getHeight(){

        if (Physics.Raycast (tirefront.position, -Vector3.up, out hit)) {
            currentheightfromterrain = hit.distance;
            currentheightfromterrain *= 10f; // 10f to convert to feet (approximate)

        }

        if (currentheightfromterrain <= 20f && currentheightfromterrain >= -0.3f){
            isgrounded = true;
        } else{
            isgrounded = false;
        }
    }



}
