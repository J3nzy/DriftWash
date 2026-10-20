using UnityEngine;

public class SteeringWheel : MonoBehaviour
{
    public float maxRotation = 450f;
    public float rotationSpeed = 720f;

    [Header("Auto Return Settings")]
    public float returnSpeed = 400f; // How fast the wheel snaps back to center

    [Header("Hand Targets")]
    public Transform leftHandTarget;
    public Transform rightHandTarget;

    private float wheelAngle;

    // These store where the hands are relative to the wheel at startup
    private Vector3 leftOffset;
    private Vector3 rightOffset;
    private Quaternion leftRotOffset;
    private Quaternion rightRotOffset;

    void Start()
    {
        // 1. Capture exactly where the hand targets are sitting relative to the wheel at startup
        if (leftHandTarget != null)
        {
            leftOffset = transform.InverseTransformPoint(leftHandTarget.position);
            leftRotOffset = Quaternion.Inverse(transform.rotation) * leftHandTarget.rotation;
        }
        if (rightHandTarget != null)
        {
            rightOffset = transform.InverseTransformPoint(rightHandTarget.position);
            rightRotOffset = Quaternion.Inverse(transform.rotation) * rightHandTarget.rotation;
        }
    }

    void Update()
    {
        // CHANGED: GetAxisRaw drops to 0 instantly when keys are released
        float input = Input.GetAxisRaw("Horizontal");

        // If there is active player input, turn the wheel normally
        if (Mathf.Abs(input) > 0.01f)
        {
            wheelAngle += input * rotationSpeed * Time.deltaTime;
        }
        // If there is NO input, automatically return the wheel angle back to 0 instantly
        else
        {
            wheelAngle = Mathf.MoveTowards(wheelAngle, 0f, returnSpeed * Time.deltaTime);
        }

        // Clamp and apply the rotation
        wheelAngle = Mathf.Clamp(wheelAngle, -maxRotation, maxRotation);
        transform.localRotation = Quaternion.Euler(0f, wheelAngle, 0f);

        // Force the hand targets to dynamically orbit and rotate with the wheel
        if (leftHandTarget != null)
        {
            leftHandTarget.position = transform.TransformPoint(leftOffset);
            leftHandTarget.rotation = transform.rotation * leftRotOffset;
        }
        if (rightHandTarget != null)
        {
            rightHandTarget.position = transform.TransformPoint(rightOffset);
            rightHandTarget.rotation = transform.rotation * rightRotOffset;
        }
    }
}
