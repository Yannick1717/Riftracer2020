using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using System;
using System.Diagnostics;
using TMPro;

[System.Serializable]
/**
 * Hilfsklasse um Achsen und deren properties abzubilden.    Test 
 */
public class AxleInfo
{
    public WheelCollider leftWheel;
    public WheelCollider rightWheel;

    public WheelFrictionCurve leftCurve;
    public WheelFrictionCurve rightCurve;

    public bool motor;
    public bool steering;
}

public class SimpleCarController : MonoBehaviour
{

    // Frontachse
    public AxleInfo frontAxle;
    // Heckachse
    public AxleInfo rearAxle;
    // Maximaler Einschlagwinkel der Vorderräder
    public float maxSteeringAngle = 30;
    // aktuelle Geschwindigkeit, wird beim Tacho verwendet
    public float speed;
    // Text für die aktuelle Geschwindigkeit (wird später verworfen)
    public Text velocityText;
    public TextMeshPro velocityTextDisplay, timerDisplay, gearDisplay, bestTimeDisplay, nDisplay, nMinusDisplay, nPlusDisplay, secondCurrentTimer;
    private float timer = 0;
    // Text für den aktuellen Gang (wird später verworfen bzw. anders verwendet)
    public Text gearText;
    // Text für die aktuelle Drehzahl (wird später verworfen)
    public Text rpmText;
    // aktueller Gang
    public int currentGear;
    // Animationskurve welche representativ das Drehzahlband des Ganges ist
    public AnimationCurve gearCurve;
    // Animationskurve welche representativ das Drehzahlband des Motors ist
    public AnimationCurve rpmCurve;
    // Leerlaufdrehzahl
    public float minRPM = 700;
    // Faktor zur Steuerung der Beschleunigungsfähigkeit des jeweilgen KFZs
    public float finalDriveRatio = 4;
    // aktuelle Drehzahl 
    public float currentRPM;
    private float oldRPM = 0;
    // aktuelles Drehmoment 
    float currentTorque;
    // aktuelle Drehzahl der Räder 
    float wheelsRPM;
    // 2D-Transform für die Nadel der Drehzahl
    public RectTransform rpmNeedle;
    // Bereich in dem sich die Drehzahlnadel bewegen kann (Winkel)
    private float rpmTargetAngle, speedTargetAngle;
    // Rigidbody des Fahrzeuges
    Rigidbody rb;
    // debug Display für die Geschwindigkeit
    float rpmDisplay = 0;
    // Kamera hinterm Fahrzeug
    public Camera ThirdPersonCamera;
    // Kamera im inneren des Fahrzeugs
    public Camera CockpitCamera;

    public GameObject SpeedNeedle;
    public GameObject RpmNeedleObject;
    public GameObject[] displays;
    private int currDisplay = 0;

    //Sounds
    public List<AudioSource> carSounds;
    public float RangeDivider = 4f;
    public AudioSource tireSquiek, crash, smallCrash;
    private float crashDelay = 2;
    private float lastCrash = 0;

    //Checkpoints
    public GameObject[] checkpoints;

    //Setting's Values
    public float[] MinRpmTable = { 500, 750, 1120, 1669, 2224, 2783, 3335, 3882, 4355, 4833, 5384, 5943, 6436, 6928, 7419, 7900 };
    public float[] NormalRpmTable = { 720, 930, 1559, 2028, 2670, 3145, 3774, 4239, 4721, 5194, 5823, 6313, 6808, 7294, 7788, 8261 };
    public float[] MaxRpmTable = { 920, 1360, 1829, 2474, 2943, 3575, 4036, 4525, 4993, 5625, 6123, 6616, 7088, 7589, 8060, 10000 };
    public float[] PitchingTable = { 0.12f, 0.12f, 0.12f, 0.12f, 0.11f, 0.10f, 0.09f, 0.08f, 0.06f, 0.06f, 0.06f, 0.06f, 0.06f, 0.06f, 0.06f, 0.06f };

    private const float SPEED_ANGLE_80 = 123.5f, SPEED_ANGLE_200 = 215f;

    //SteeringWheelValues

    public float steeringAxis, acceleration, deceleration;

    //TODO refactor
    public Laps laps;

    private void tireSound(float slip) {
        if (slip > 0.4 || slip < -0.5) {
            if(tireSquiek.volume < 0.95f) {
            tireSquiek.volume += 0.05f;
            }
        }
        else {
            if (tireSquiek.volume > 0.05f) {
                tireSquiek.volume -= 0.05f;
            }
        }
    }

    void OnTriggerEnter(Collider other) {
        print(Time.time - lastCrash + " - " + crashDelay);
        if (Time.time - lastCrash > crashDelay  && other.gameObject.tag != "Checkpoint") {
            if (speed < 40) {
                smallCrash.Play();
            }
            else {
                crash.Play();
            }
            lastCrash = Time.time;
        }
    }


    /** 
     * Transformiert (rotiert) alle übergebenen Räder anhand des Wheelcolliders
     */
    private void ApplyTransformsToWheels(AxleInfo front, AxleInfo rear)
    {

        WheelCollider[] wheels = new WheelCollider[4];
        wheels[0] = front.leftWheel;
        wheels[1] = front.rightWheel;
        wheels[2] = rear.leftWheel;
        wheels[3] = rear.rightWheel;

        for (int i = 0; i < 4; i++)
        {

            WheelHit hit = new WheelHit();
            WheelCollider wheel = GetComponent<WheelCollider>();
        if (wheels[0].GetGroundHit(out hit)) {
                    tireSound(hit.forwardSlip);
        }

            if (wheels[i].transform.childCount == 0)
            {
                return;
            }

            //get child elem of the wheel collider elem
            Transform visualWheel = wheels[i].transform.GetChild(0);

            Vector3 position;
            Quaternion rotation;
            //returns the position and rotation of the wheel collider
            wheels[i].GetWorldPose(out position, out rotation);
            //out is like a return, a method can only return one value, with out you can get multiple

            visualWheel.transform.position = position;
            visualWheel.transform.rotation = rotation;
        }
        }

    /**
     * Methode, welche sofort wenn das Script aktiviert wird, den globalen
     * Attributen Werte zuweist.
     */
    public void Start()
    {
        //TODO Magic Numbers entfernen

        currentGear = 1;
        rb = GetComponent<Rigidbody>();
        //rb.centerOfMass = new Vector3(0, 0, 0.5f);
        velocityText.text = "0";
        velocityTextDisplay.text = "0";
        gearDisplay.text = "S 1";
        gearText.text = "1";
        rpmText.text = "0";
        ThirdPersonCamera.enabled = true;
        CockpitCamera.enabled = false;

        // Reibungswerte initial holen um sie programmatisch verändern zu 
        // TODO
        rearAxle.leftCurve.extremumValue = rearAxle.leftWheel.sidewaysFriction.extremumValue;
        rearAxle.rightCurve.extremumValue = rearAxle.rightWheel.sidewaysFriction.extremumValue;
        rearAxle.leftCurve.asymptoteValue = rearAxle.leftWheel.sidewaysFriction.asymptoteValue;
        rearAxle.rightCurve.asymptoteValue = rearAxle.rightWheel.sidewaysFriction.asymptoteValue;
        rearAxle.leftCurve.extremumSlip = rearAxle.leftWheel.sidewaysFriction.extremumSlip;
        rearAxle.rightCurve.extremumSlip = rearAxle.rightWheel.sidewaysFriction.extremumSlip;
        rearAxle.leftCurve.asymptoteSlip = rearAxle.leftWheel.sidewaysFriction.asymptoteSlip;
        rearAxle.rightCurve.asymptoteSlip = rearAxle.rightWheel.sidewaysFriction.asymptoteSlip;

        //Sounds initialisieren
        for (int i = 0; i < 16; i++)
        {
            carSounds[i].Play();
            carSounds[i].volume = 0.0f;
            tireSquiek.Play();
            tireSquiek.volume = 0.0f;
        }


    }


    /**
	 * Methode, um Werte auf Knopfdruck zu ändern.
	 */
    public void Update()
    {
        if (Input.GetButtonDown("ShiftUp") && currentGear < 6)
        {
            currentGear++;
        }
        if (Input.GetButtonDown("ShiftDown") && currentGear > 0)
        {
            currentGear--;
        }
        // Kamera-Switch
        if (Input.GetButtonDown("Camera"))
        {
            ThirdPersonCamera.enabled = !ThirdPersonCamera.enabled;
            CockpitCamera.enabled = !CockpitCamera.enabled;
        }
        if (Input.GetButtonDown("ChangeDisplay")) {
            displays[currDisplay].SetActive(false);
            currDisplay++;
            if (currDisplay + 1 > displays.Length) {
                currDisplay = 0;
            }
            displays[currDisplay].SetActive(true);
        }
            updateSound();
    }


    private void updateSound()
    {
        float soundRpm = currentRPM * 1.2f;
        for (int i = 0; i < carSounds.Count; i++)
        {
            if (soundRpm < MinRpmTable[i])
            {
                carSounds[i].volume = 0.0f; ;
            }
            else if (soundRpm >= MinRpmTable[i] && soundRpm < NormalRpmTable[i])
            {
                float Range = NormalRpmTable[i] - MinRpmTable[i];
                float ReducedRPM = soundRpm - MinRpmTable[i];
                carSounds[i].volume = ReducedRPM / Range;
                float PitchMath = (ReducedRPM * PitchingTable[i]) / Range;
                carSounds[i].pitch = 1f - PitchingTable[i] + PitchMath;

            }
            else if (soundRpm >= NormalRpmTable[i] && soundRpm <= MaxRpmTable[i])
            {
                float Range = MaxRpmTable[i] - NormalRpmTable[i];
                float ReducedRPM = soundRpm - NormalRpmTable[i];
                carSounds[i].volume = 1.0f; ;
                float PitchMath = (ReducedRPM * PitchingTable[i]) / Range;
                carSounds[i].pitch = 1f + PitchMath;
            }
            else if (soundRpm > MaxRpmTable[i])
            {
                float Range = (MaxRpmTable[i + 1] - MaxRpmTable[i]) / RangeDivider;
                float ReducedRPM = soundRpm - MaxRpmTable[i];
                carSounds[i].volume = 1f - ReducedRPM / Range;
                float PitchMath = (ReducedRPM * PitchingTable[i]) / Range;
                carSounds[i].pitch = 1f + PitchingTable[i] + PitchMath;
            }
        }
    }

    private void antiRollBar()
    {
        WheelHit hit;
        float travelLF = 1.0f, travelRF = 1.0f, travelLR = 1.0f, travelRR = 1.0f, AntiRoll = 2500;


        bool groundedLF = frontAxle.leftWheel.GetGroundHit(out hit);
        if (groundedLF)
        {
            travelLF = (-frontAxle.leftWheel.transform.InverseTransformPoint(hit.point).y - frontAxle.leftWheel.radius) / frontAxle.leftWheel.suspensionDistance;
        }

        bool groundedRF = frontAxle.rightWheel.GetGroundHit(out hit);
        if (groundedRF)
        {
            travelRF = (-frontAxle.rightWheel.transform.InverseTransformPoint(hit.point).y - frontAxle.rightWheel.radius) / frontAxle.rightWheel.suspensionDistance;
        }

        float antiRollForceFront = (travelLF - travelRF) * AntiRoll;

        if (groundedLF)
        {
            rb.AddForceAtPosition(frontAxle.leftWheel.transform.up * -antiRollForceFront,
                   frontAxle.leftWheel.transform.position);
        }

        if (groundedRF)
        {
            rb.AddForceAtPosition(frontAxle.rightWheel.transform.up * -antiRollForceFront,
                   frontAxle.rightWheel.transform.position);
        }
        // ------------------
        bool groundedLR = rearAxle.leftWheel.GetGroundHit(out hit);
        if (groundedLR)
        {
            travelLF = (-rearAxle.leftWheel.transform.InverseTransformPoint(hit.point).y - rearAxle.leftWheel.radius) / rearAxle.leftWheel.suspensionDistance;
        }

        bool groundedRR = rearAxle.rightWheel.GetGroundHit(out hit);
        if (groundedRR)
        {
            travelRF = (-rearAxle.rightWheel.transform.InverseTransformPoint(hit.point).y - rearAxle.rightWheel.radius) / rearAxle.rightWheel.suspensionDistance;
        }

        float antiRollForceRear = (travelLR - travelRR) * AntiRoll;

        if (groundedLR)
        {
            rb.AddForceAtPosition(rearAxle.leftWheel.transform.up * -antiRollForceRear,
                   rearAxle.leftWheel.transform.position);
        }

        if (groundedRR)
        {
            rb.AddForceAtPosition(rearAxle.rightWheel.transform.up * -antiRollForceRear,
                   rearAxle.rightWheel.transform.position);
        }

    }



    /**
     * Begrenzt die Drehzahl auf den Bereich zwischen 700 und 8400 RPM
     */
    private float RestrictRPM(float currentRPM)
    {
        if (currentRPM < 0)
        {
            currentRPM = 700;
        }

        if (currentRPM > 8400)
        {
            currentRPM = 8400;
        }

        return currentRPM;
    }

    /**
     * Berechnung des Drehmoments anhand der Drehzahlkurve des Motors,
     * der Drehzahlkurve des Ganges, des Beschleunigungsfaktors
     * und dem Winkel des Inputdevices (Gaspedal)
     */
    private float evaluateEngineTorque(float acceleration)
    {

        return rpmCurve.Evaluate(currentRPM) * gearCurve.Evaluate(currentGear) * finalDriveRatio * acceleration;
    }

    /**
     * Methode, welche Input entgegen nimmt und dementsprechend das Fahrzeug beschleunigt
     */
    private void handleAcceleration(float acceleration)
    {
        if (acceleration > 0 && currentRPM < 8400)
        {
            frontAxle.leftWheel.motorTorque = frontAxle.rightWheel.motorTorque = (currentTorque * 0.5f);
            rearAxle.leftWheel.motorTorque = rearAxle.rightWheel.motorTorque = (currentTorque * 0.5f);
            //frontAxle.leftWheel.motorTorque = frontAxle.rightWheel.motorTorque = (currentTorque * 0.3f);
            //rearAxle.leftWheel.motorTorque = rearAxle.rightWheel.motorTorque = (currentTorque * 0.7f);
            frontAxle.leftWheel.brakeTorque = frontAxle.rightWheel.brakeTorque = rearAxle.leftWheel.brakeTorque = rearAxle.rightWheel.brakeTorque = 0;
        }
        else
        {
            frontAxle.leftWheel.motorTorque = frontAxle.rightWheel.motorTorque = rearAxle.leftWheel.motorTorque = rearAxle.rightWheel.motorTorque = 0;
            // Motorbremse wirken lassen
            frontAxle.leftWheel.brakeTorque = frontAxle.rightWheel.brakeTorque = rearAxle.leftWheel.brakeTorque = rearAxle.rightWheel.brakeTorque = rpmCurve.Evaluate(currentRPM) * gearCurve.Evaluate(currentGear) * 0.7f;
            //frontAxle.leftWheel.brakeTorque = frontAxle.rightWheel.brakeTorque = rearAxle.leftWheel.brakeTorque = rearAxle.rightWheel.brakeTorque = rpmCurve.Evaluate(currentRPM) * gearCurve.Evaluate(currentGear) * 0.5f;
        }
    }

    /**
     * Methode, welche Input entgegen nimmt und dementsprechend das Fahrzeug bremst
     */
    private void handleDeceleration(float deceleration)
    {
        if (deceleration > 0)
        {
            frontAxle.leftWheel.brakeTorque = frontAxle.rightWheel.brakeTorque = 4000 * deceleration;
            rearAxle.leftWheel.brakeTorque = rearAxle.rightWheel.brakeTorque = 2000 * deceleration;
        }
        else
        {
            frontAxle.leftWheel.brakeTorque = frontAxle.rightWheel.brakeTorque = 0;
            rearAxle.leftWheel.brakeTorque = rearAxle.rightWheel.brakeTorque = 0;
        }
        if (Input.GetButton("Handbrake"))
        {
            rearAxle.leftWheel.brakeTorque = rearAxle.rightWheel.brakeTorque = 3000;

            // TODO
            //WheelFrictionCurve handbrakeFriction = new WheelFrictionCurve();

            /*handbrakeFriction.extremumValue = 100;
            handbrakeFriction.extremumValue = 100;
            handbrakeFriction.asymptoteValue = 80;
            handbrakeFriction.asymptoteValue = 80;
            handbrakeFriction.extremumSlip = 100;
            handbrakeFriction.extremumSlip = 100;
            handbrakeFriction.asymptoteSlip = 80;
            handbrakeFriction.asymptoteSlip = 80;

            rearAxle.leftWheel.sidewaysFriction = handbrakeFriction;
            rearAxle.rightWheel.sidewaysFriction = handbrakeFriction;
            */
        }
    }

    /**
     * Methode, welche Input entgegen nimmt und dementsprechend das Fahrzeug lenkt
     */
    private void handleStearing(float steering)
    {
        if (frontAxle.steering)
        {
            frontAxle.leftWheel.steerAngle = steering;
            frontAxle.rightWheel.steerAngle = steering;
        }
    }

    private void resetRotation()
    {
        if (Input.GetButton("Reset"))
        {
            //RaycastHit hit;
            //if (Physics.Raycast(transform.position, transform.TransformDirection(transform.up), out hit, 10) && hit.transform.gameObject.tag == "Floor") {
            transform.Rotate(transform.rotation.x, transform.rotation.y, 7, Space.Self);
            //}
        }
    }

    private float getSpeedNeedleAngle(float speed)
    {
        float res = 0;
        if (speed < 80)
        {
            return Mathf.Lerp(speedTargetAngle, speed * 1.5f, Time.deltaTime * 3.5f);
        }
        if (speed > 80 && speed < 200)
        {
            return SPEED_ANGLE_80 + Mathf.Lerp(speedTargetAngle - SPEED_ANGLE_80, (speed - 80) * 0.75f, Time.deltaTime * 3.5f);
        }
        if (speed > 200)
        {
            return SPEED_ANGLE_200 + Mathf.Lerp(speedTargetAngle - SPEED_ANGLE_200, (speed - 200) * 0.5f, Time.deltaTime * 3.5f);
        }
        return res;
    }

    private void updateTimerDisplay()
    {
        timerDisplay.text = laps.getTimeString(laps.getTimer());

        laps.getCheckpointTimes(nDisplay, nMinusDisplay, nPlusDisplay, secondCurrentTimer);

    }


    /**
    * Methode, welche Physics an das Fahrzeug übergibt und somit die 
    * Steuerung ermöglicht.
    */
    public void FixedUpdate()
    {

        if (VariableManager.Instance.useSteeringWheel)
        {
            if (LogitechGSDK.LogiUpdate() && LogitechGSDK.LogiIsConnected(0))
            {
                LogitechGSDK.DIJOYSTATE2ENGINES rec;
                rec = LogitechGSDK.LogiGetStateUnity(0);
                steeringAxis = rec.lX / 32769f;
                acceleration = 1 - (rec.lY - (-32768f)) / (32767f - (-32768f));
                handleStearing(maxSteeringAngle * steeringAxis * 2);
                deceleration = 1 - (rec.lRz - (-32768f)) / (32767f - (-32768f));
            }
        }
        else
        {
            handleStearing(maxSteeringAngle * Input.GetAxis("Horizontal"));
            acceleration = Input.GetAxis("Accelerate");
            deceleration = Input.GetAxis("Break");
        }

        updateTimerDisplay();

        // Geschwindigkeit des Rigibodys des Autos in km/h
        speed = rb.velocity.magnitude * 3.6f;

        // aktuller Wert der Reifendrehzahl
        wheelsRPM = (rearAxle.leftWheel.rpm + rearAxle.rightWheel.rpm + frontAxle.rightWheel.rpm + frontAxle.leftWheel.rpm) / 4f;

        /**
         * Berechnung der Drehzahl anhand der Leerlaufdrehzahl, der Reifendrehzahl, des Beschleunigungsfaktors 
         * und der Drehzahlkurve des aktuellen Ganges
         */
        oldRPM = currentRPM;
        currentRPM = Mathf.Lerp(oldRPM, RestrictRPM(minRPM + (wheelsRPM * finalDriveRatio * gearCurve.Evaluate(currentGear))), Time.deltaTime * 10.0f);

        // Aktuelles Drehmoment
        currentTorque = evaluateEngineTorque(acceleration);

        //TODO vielleicht ne chain of responsibilty oder so implementieren
        handleAcceleration(acceleration);
        handleDeceleration(deceleration);

        //TODO Richtige platzierung?
        antiRollBar();


        // Position resetten on LB
        resetRotation();

        // Räder sich drehen lassen
        ApplyTransformsToWheels(frontAxle, rearAxle);

        // --------- Block: Display and GUI ----------------

        // Text für die aktuelle Motordrehzahl
        rpmDisplay = RestrictRPM(currentRPM);
        // Text für die aktuelle Geschwindigkeit (wird noch entfernt)
        velocityText.text = speed.ToString("F0") + " km/h";
        velocityTextDisplay.text = speed.ToString("F0");
        // Text für den aktuellen Gang (wird noch entfernt)
        if(currentGear == 0){
            gearDisplay.text = "R";
        }
        else
        {
            gearDisplay.text = "S " + currentGear.ToString();
        }
        

        // Text für die aktuelle Drehzahl (wird noch entfernt)
        rpmText.text = rpmDisplay.ToString("F0") + " rpm";
        // Lenkwinkel, abhängig von der aktuell Motordrehzahl -> sollte ja eigentlich Geschwindigkeit sein...?
        rpmTargetAngle = Mathf.Lerp(rpmTargetAngle, currentRPM / 28, Time.deltaTime * 3.5f);
        speedTargetAngle = getSpeedNeedleAngle(speed);

        // Rotationswinkel der Drehzahlmessernadel
        rpmNeedle.localRotation = Quaternion.Euler(0, 0, -rpmTargetAngle);

        RpmNeedleObject.transform.localRotation = Quaternion.Euler(0, 0, rpmTargetAngle - 130);
        SpeedNeedle.transform.localRotation = Quaternion.Euler(0, 0, speedTargetAngle - 122);

    }

}