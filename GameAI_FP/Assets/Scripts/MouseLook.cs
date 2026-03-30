using UnityEngine;

public class MouseLook : MonoBehaviour
{
    public float mouseSensitivity = 200f;
    public Transform cameraHolder;
    public float recoilSmoothSpeed = 10f;

    private float xRotation = 0f;
    private float recoilVertical = 0f;
    private float recoilHorizontal = 0f;

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
    }

    void Update()
    {
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

        xRotation -= mouseY;

        // Smoothly apply recoil
        if (recoilVertical > 0f)
        {
            float verticalStep = recoilVertical * recoilSmoothSpeed * Time.deltaTime;
            xRotation -= verticalStep;
            recoilVertical -= verticalStep;

            if (recoilVertical < 0.01f)
                recoilVertical = 0f;
        }

        if (Mathf.Abs(recoilHorizontal) > 0f)
        {
            float horizontalStep = recoilHorizontal * recoilSmoothSpeed * Time.deltaTime;
            mouseX += horizontalStep;
            recoilHorizontal -= horizontalStep;

            if (Mathf.Abs(recoilHorizontal) < 0.01f)
                recoilHorizontal = 0f;
        }

        xRotation = Mathf.Clamp(xRotation, -90f, 90f);

        cameraHolder.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
        transform.Rotate(Vector3.up * mouseX);
    }

    public void AddRecoil(float vertical, float horizontal)
    {
        recoilVertical += vertical;
        recoilHorizontal += horizontal;
    }
}