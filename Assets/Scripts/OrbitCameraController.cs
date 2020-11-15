using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OrbitCameraController : MonoBehaviour {
    public Transform target;
    public Vector3 offset;
    public float cameraHeight, sensitivity, smoothSpeed;

    private void LookAtTarget() {
        transform.LookAt(target, Vector3.up);
    }

    private void CalcTargetLocation() {
        Vector3 targetPos = target.position +
         target.forward * offset.z +
         target.right * offset.x;
        targetPos.y = target.position.y + cameraHeight;
        transform.position = targetPos;
        //transform.position = Vector3.Lerp(transform.position, targetPos, smoothSpeed);
    }

    private void rotateCamera() {
        //Rotation der Camera
        //transform.RotateAround(target.position, Vector3.up, Input.GetAxis("MoveCameraAxis") * sensitivity);
        transform.RotateAround(target.position, Vector3.up, Input.GetAxis("MoveCameraAxis") * sensitivity);
    }


    void FixedUpdate() {
        CalcTargetLocation();
        LookAtTarget();
        rotateCamera();
    }
}