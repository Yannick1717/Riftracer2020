
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SteeringWheelLogitech : MonoBehaviour
{

    LogitechGSDK.LogiControllerPropertiesData properties;

    public float xAxes, gasInput;

    // Start is called before the first frame update
    void Start()
    {
        print(LogitechGSDK.LogiSteeringInitialize(false));
    }

    // Update is called once per frame
    void Update()
    {
        if (LogitechGSDK.LogiUpdate() && LogitechGSDK.LogiIsConnected(0))
        {
            LogitechGSDK.DIJOYSTATE2ENGINES rec;
            rec = LogitechGSDK.LogiGetStateUnity(0);
            xAxes = rec.lX / 32769f;
            gasInput = rec.lY;
        }
        else
        {

            print("Nicht verbunden.");
        }
    }
    



}
