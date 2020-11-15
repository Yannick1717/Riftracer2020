using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SteeringWheelRotator : MonoBehaviour
{
    private float steeringAxis;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (VariableManager.Instance.useSteeringWheel) {
            LogitechGSDK.DIJOYSTATE2ENGINES rec;
            rec = LogitechGSDK.LogiGetStateUnity(0);
            steeringAxis = rec.lX / 32769f;
        }
        else {
            steeringAxis = Input.GetAxis("Horizontal");
        }
        // transform.localEulerAngles = Vector3.up * Mathf.Clamp((Input.GetAxis("Horizontal") * 100), -90f, 90f);
        // 450 Grad Umdrehung des Lenkrads
        transform.localEulerAngles = Vector3.up * Mathf.Clamp((steeringAxis * 450), -450f, 450f);
    }
}
