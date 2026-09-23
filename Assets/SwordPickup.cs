using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody), typeof(BoxCollider))]
public class SwordPickup : MonoBehaviour
{
    [SerializeField] private CharacterController player;
    [SerializeField] private Transform playerCamera;
    [SerializeField] private Transform swordPivot;
    [SerializeField, Min(0f)] private float pickupRange = 2f;
    [Tooltip("Grip point in the sword model's local coordinates. Aligns this point with SwordPivot.")]
    [SerializeField] private Vector3 gripLocalPosition = Vector3.zero;
    [SerializeField] private Vector3 heldRotation = new Vector3(-75f, 0f, 40f);

    [Header("Sword Swing")]
    [SerializeField, Min(0.01f)] private float windupDuration = 0.12f;
    [SerializeField, Min(0.01f)] private float slashDuration = 0.18f;
    [SerializeField, Min(0.01f)] private float recoveryDuration = 0.25f;
    [SerializeField] private Vector3 windupPositionOffset = new Vector3(0.15f, 0.45f, 0.05f);
    [SerializeField] private Vector3 windupRotationOffset = new Vector3(-15f, 10f, -25f);
    [SerializeField] private Vector3 slashPositionOffset = new Vector3(-0.9f, -0.3f, 0.1f);
    [SerializeField] private Vector3 slashRotationOffset = new Vector3(65f, -25f, 100f);

    private Rigidbody body;
    private Collider swordCollider;
    private bool isHeld;
    private bool isSwinging;
    private float swingTime;
    private Vector3 heldPosition;
    private Quaternion pivotRestRotation;

    private void Awake()
    {
        body = GetComponent<Rigidbody>();
        swordCollider = GetComponent<BoxCollider>();
    }

    private void Update()
    {
        if (isHeld)
        {
            UpdateSwing();
            return;
        }

        if (player == null || playerCamera == null || swordPivot == null)
            return;

        bool pickupPressed = Input.GetKeyDown(KeyCode.E) ||
            (Gamepad.current != null && Gamepad.current.buttonWest.wasPressedThisFrame);
        if (!pickupPressed)
            return;

        // Measure from the player's body so a sword on the floor is reachable.
        Vector3 nearestPoint = swordCollider.ClosestPoint(player.bounds.center);
        if (Vector3.Distance(player.bounds.ClosestPoint(nearestPoint), nearestPoint) > pickupRange)
            return;

        // Prevent picking up the sword through walls or platforms.
        Vector3 toSword = swordCollider.bounds.center - playerCamera.position;
        foreach (RaycastHit hit in Physics.RaycastAll(playerCamera.position,
            toSword.normalized, toSword.magnitude, Physics.DefaultRaycastLayers,
            QueryTriggerInteraction.Ignore))
        {
            if (hit.collider.attachedRigidbody == body ||
                hit.transform.IsChildOf(player.transform))
                continue;

            return;
        }

        body.linearVelocity = Vector3.zero;
        body.angularVelocity = Vector3.zero;
        body.collisionDetectionMode = CollisionDetectionMode.Discrete;
        body.useGravity = false;
        body.isKinematic = true;
        body.interpolation = RigidbodyInterpolation.None;
        body.detectCollisions = false;
        swordCollider.enabled = false;

        heldPosition = swordPivot.localPosition;
        pivotRestRotation = swordPivot.localRotation;

        // Preserve import scale and put the model's grip at the hand pivot.
        transform.SetParent(swordPivot, true);
        transform.localRotation = Quaternion.Euler(heldRotation);
        transform.localPosition = -(transform.localRotation *
            Vector3.Scale(gripLocalPosition, transform.localScale));
        isHeld = true;
    }

    private void UpdateSwing()
    {
        bool swingPressed = Input.GetMouseButtonDown(0) ||
            (Gamepad.current != null && Gamepad.current.rightTrigger.wasPressedThisFrame);

        // One swing per press; finish the current swing before accepting another.
        if (!isSwinging && swingPressed)
        {
            isSwinging = true;
            swingTime = 0f;
        }

        Quaternion restRotation = pivotRestRotation;
        if (!isSwinging)
        {
            SetSwingPose(heldPosition, restRotation, heldPosition, restRotation, 1f);
            return;
        }

        swingTime += Time.deltaTime;
        float windup = Mathf.Max(0.01f, windupDuration);
        float slash = Mathf.Max(0.01f, slashDuration);
        float recovery = Mathf.Max(0.01f, recoveryDuration);
        Vector3 raisedPosition = heldPosition + windupPositionOffset;
        Vector3 endPosition = heldPosition + slashPositionOffset;
        // Apply offsets in camera space so the slash stays diagonal on screen.
        Quaternion raisedRotation = Quaternion.Euler(windupRotationOffset) * restRotation;
        Quaternion endRotation = Quaternion.Euler(slashRotationOffset) * restRotation;

        if (swingTime < windup)
            SetSwingPose(heldPosition, restRotation, raisedPosition, raisedRotation,
                swingTime / windup);
        else if (swingTime < windup + slash)
            SetSwingPose(raisedPosition, raisedRotation, endPosition, endRotation,
                (swingTime - windup) / slash);
        else if (swingTime < windup + slash + recovery)
            SetSwingPose(endPosition, endRotation, heldPosition, restRotation,
                (swingTime - windup - slash) / recovery);
        else
        {
            isSwinging = false;
            SetSwingPose(heldPosition, restRotation, heldPosition, restRotation, 1f);
        }
    }

    private void SetSwingPose(Vector3 fromPosition, Quaternion fromRotation,
        Vector3 toPosition, Quaternion toRotation, float progress)
    {
        float eased = Mathf.SmoothStep(0f, 1f, progress);
        swordPivot.localPosition = Vector3.Lerp(fromPosition, toPosition, eased);
        swordPivot.localRotation = Quaternion.Slerp(fromRotation, toRotation, eased);
    }
}
