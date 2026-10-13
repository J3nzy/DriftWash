using UnityEngine;

public class SteeringWheel : MonoBehaviour
{
    public float maxRotation = 450f;
    public float rotationSpeed = 720f;

    private float wheelAngle;

    void Update()
    {
        float input = Input.GetAxis("Horizontal");

        wheelAngle += input * rotationSpeed * Time.deltaTime;
        wheelAngle = Mathf.Clamp(wheelAngle, -maxRotation, maxRotation);

        transform.localRotation = Quaternion.Euler(0f, wheelAngle, 0f);
    }
}
