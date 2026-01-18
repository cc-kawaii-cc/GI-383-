using UnityEngine;

public class FaceCamera : MonoBehaviour
{
    private Transform mainCamera;

    void Start()
    {
        mainCamera = Camera.main.transform;
    }

    void LateUpdate()
    {
        // ทำให้ Canvas หันหน้าเข้าหากล้องตลอดเวลา
        transform.LookAt(transform.position + mainCamera.rotation * Vector3.forward,
            mainCamera.rotation * Vector3.up);
    }
}