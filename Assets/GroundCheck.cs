using UnityEngine;

public class GroundCheck : MonoBehaviour
{
    [SerializeField] private float groundDistance = 0.25f;
    [SerializeField] private LayerMask groundMask;

    public bool IsGrounded { get; private set; }

    public void CheckGround()
    {
        IsGrounded = Physics.CheckSphere(
            transform.position,
            groundDistance,
            groundMask,
            QueryTriggerInteraction.Ignore
        );
        // Debug.Log("Grounded: " + IsGrounded);
    }

    // Shows the detection sphere in the Scene view
    void OnDrawGizmos()
    {
        Gizmos.color = IsGrounded ? Color.green : Color.red;
        Gizmos.DrawWireSphere(transform.position, groundDistance);
    }
}
