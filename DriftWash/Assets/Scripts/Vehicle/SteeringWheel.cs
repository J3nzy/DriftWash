using UnityEngine;

public class SteeringWheel : MonoBehaviour
{
    public float maxRotation = 450f;
    public float rotationSpeed = 720f;

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
        float input = Input.GetAxis("Horizontal");

        // YOUR ORIGINAL ROTATION LOGIC (Completely Untouched)
        wheelAngle += input * rotationSpeed * Time.deltaTime;
        wheelAngle = Mathf.Clamp(wheelAngle, -maxRotation, maxRotation);
        transform.localRotation = Quaternion.Euler(0f, wheelAngle, 0f);

        // 2. NEW LOGIC: Force the hand targets to dynamically orbit and rotate with the wheel
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