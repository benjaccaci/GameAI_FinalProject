using UnityEngine;
using UnityEngine.InputSystem;

public class MouseLook : MonoBehaviour
{
    public float mouseSensitivity = 200f;
    public Transform cameraHolder;
    public float recoilSmoothSpeed = 10f;

    private float xRotation = 0f;
    private float recoilVertical = 0f;
    private float recoilHorizontal = 0f;
    private Vector2 lookInput;

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
    }

    public void OnLook(InputValue value)
    {
        lookInput = value.Get<Vector2>();
    }

    void Update()
    {
        float mouseX = lookInput.x * mouseSensitivity * Time.deltaTime;
        float mouseY = lookInput.y * mouseSensitivity * Time.deltaTime;

        xRotation -= mouseY;

        if (recoilVertical > 0f)
        {
            float verticalStep = Mathf.Max(recoilVertical * recoilSmoothSpeed * Time.deltaTime, 0.5f * Time.deltaTime);
            if (verticalStep > recoilVertical) verticalStep = recoilVertical;
            xRotation -= verticalStep;
            recoilVertical -= verticalStep;

            if (recoilVertical < 0.01f)
                recoilVertical = 0f;
        }

        if (Mathf.Abs(recoilHorizontal) > 0f)
        {
            float horizontalStep = Mathf.Max(Mathf.Abs(recoilHorizontal) * recoilSmoothSpeed * Time.deltaTime, 0.5f * Time.deltaTime);
            if (horizontalStep > Mathf.Abs(recoilHorizontal)) horizontalStep = Mathf.Abs(recoilHorizontal);
            float sign = Mathf.Sign(recoilHorizontal);
            transform.Rotate(Vector3.up * sign * horizontalStep);
            recoilHorizontal -= sign * horizontalStep;

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