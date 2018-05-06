using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WiimoteApi;

public class Axe : MonoBehaviour
{
    Wiimote wiimote;
    Transform pivot;

    // Use this for initialization
    void Start()
    {
        WiimoteManager.FindWiimotes();
        wiimote = WiimoteManager.Wiimotes[0];
        wiimote.RequestIdentifyWiiMotionPlus();
        wiimote.ActivateWiiMotionPlus();
        wiimote.SetupIRCamera(IRDataType.BASIC);

        pivot = transform.parent;
    }

    // Update is called once per frame
    void Update()
    {
        int ret;
        do
        {
            ret = wiimote.ReadWiimoteData();

            if (ret > 0 && wiimote.current_ext == ExtensionController.MOTIONPLUS)
            {
                Vector3 offset = new Vector3(wiimote.MotionPlus.YawSpeed,
                                                wiimote.MotionPlus.RollSpeed,
                                                wiimote.MotionPlus.PitchSpeed) / 95f; // Divide by 95Hz (average updates per second from wiimote)

                //print(offset.magnitude);
                if (offset.magnitude > 0.3)
                {
                    pivot.transform.Rotate(offset, Space.Self);
                }
                
                Vector3 accel = GetAccelVector();

                //Debug.Log(accel.magnitude);
                if (accel.magnitude > 2)
                {
                    pivot.transform.Translate(accel / 95f);
                }
            }
        } while (ret > 0);

        if (wiimote.Button.a)
        {
            wiimote.MotionPlus.SetZeroValues();
            pivot.transform.rotation = Quaternion.FromToRotation(pivot.transform.rotation * GetAccelVector(), Vector3.up) * pivot.transform.rotation;
            pivot.transform.rotation = Quaternion.FromToRotation(pivot.transform.forward, Vector3.forward) * pivot.transform.rotation;
        }
    }

    private Vector3 GetAccelVector()
    {
        float accel_x;
        float accel_y;
        float accel_z;

        float[] accel = wiimote.Accel.GetCalibratedAccelData();
        accel_x = accel[0];
        accel_y = accel[2];
        accel_z = accel[1];

        return new Vector3(accel_x, accel_y, accel_z);
    }

    private Vector3 Vec3FromFloat(float[] array)
    {
        return new Vector3(array[0], array[2], array[1]);
    }
}
