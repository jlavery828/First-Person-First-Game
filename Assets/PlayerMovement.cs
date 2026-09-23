using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    [Header("References")]
    public CharacterController controller;

    [Header("Movement")]
    public float walkSpeed = 6f;
    public float sprintSpeed = 10f;

    [Header("Physics Pushing")]
    [Min(0f)] public float pushForce = 10f;

    [Header("Jumping")]
    public float jumpHeight = 2f;
    public float gravity = -20f;

    public GroundCheck groundCheck;
    private Vector3 velocity;
    private Vector3 horizontalVelocity;

    void Update()
    {
        groundCheck.CheckGround();

        // Require actual contact and downward motion so the ground probe cannot
        // restore steering while we are taking off or just above the floor.
        bool isGrounded = groundCheck.IsGrounded &&
            controller.isGrounded && velocity.y <= 0f;

        if (isGrounded)
        {
            HandleMovement();
            velocity.y = -2f;

            if (Input.GetButtonDown("Jump"))
                velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
        }

        velocity.y += gravity * Time.deltaTime;

        // Horizontal velocity stays in world space throughout the jump,
        // including when the player turns the camera or changes sprint input.
        CollisionFlags collisions = controller.Move(
            (horizontalVelocity + velocity) * Time.deltaTime
        );

        if ((collisions & CollisionFlags.Above) != 0 && velocity.y > 0f)
            velocity.y = 0f;
    }

    void OnControllerColliderHit(ControllerColliderHit hit)
    {
        Rigidbody body = hit.collider.attachedRigidbody;
        if (body == null || body.isKinematic || body == controller.attachedRigidbody)
            return;

        // Push sideways, without driving objects beneath our feet into the floor.
        if (hit.moveDirection.y < -0.3f || hit.normal.y > 0.5f)
            return;

        Vector3 pushDirection = new Vector3(horizontalVelocity.x, 0f, horizontalVelocity.z);
        if (pushDirection.sqrMagnitude < 0.001f)
            return;

        pushDirection.Normalize();
        if (Vector3.Dot(pushDirection, hit.normal) >= 0f)
            return;

        // Scale the impulse by frame time because Move is called from Update.
        // Applying it at contact lets the sword tumble as well as slide.
        body.AddForceAtPosition(pushDirection * pushForce * Time.deltaTime,
            hit.point, ForceMode.Impulse);
    }

    void HandleMovement()
    {
        float x = Input.GetAxisRaw("Horizontal");
        float z = Input.GetAxisRaw("Vertical");

        Vector3 direction =
            transform.right * x +
            transform.forward * z;

        // Prevent diagonal movement from being faster.
        direction = Vector3.ClampMagnitude(direction, 1f);

        Gamepad gamepad = Gamepad.current;
        bool isSprinting = Input.GetKey(KeyCode.LeftShift) ||
            (gamepad != null && gamepad.rightShoulder.isPressed);

        float currentSpeed = isSprinting
            ? sprintSpeed
            : walkSpeed;

        horizontalVelocity = direction * currentSpeed;
    }
}
