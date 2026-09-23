using UnityEngine;
using UnityEngine.InputSystem;

public class MouseScript : MonoBehaviour
{

    public float mouseSensitivity = 100;

    [Tooltip("Right-stick look speed in degrees per second.")]
    public float gamepadSensitivity = 180f;

    public Transform playerBody;

    float xRotation = 0;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
    }

    // Update is called once per frame
    void Update()
    {
        float mouseX = Input.GetAxisRaw("Mouse X") * mouseSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxisRaw("Mouse Y") * mouseSensitivity * Time.deltaTime;

        // Stick input is a turn rate; ReadValue applies the stick's dead zone.
        Gamepad gamepad = Gamepad.current;
        if (gamepad != null)
        {
            Vector2 stick = gamepad.rightStick.ReadValue();
            mouseX += stick.x * gamepadSensitivity * Time.deltaTime;
            mouseY += stick.y * gamepadSensitivity * Time.deltaTime;
        }

        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);

        transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
        playerBody.Rotate(Vector3.up * mouseX);
    }
}
