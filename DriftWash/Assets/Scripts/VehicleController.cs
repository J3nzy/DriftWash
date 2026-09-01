using UnityEngine;

namespace DriftWash
{
    public class VehicleController : MonoBehaviour
    {
        [Header("Arcade Driving")]
        [SerializeField] float maxSpeed = 30f;
        [SerializeField] float acceleration = 20f;
        [SerializeField] float coastDeceleration = 5f;
        [SerializeField] float turnSpeed = 90f;

        [Header("Visual Wheels (Cosmetic Only)")]
        [SerializeField] Transform frontWheel;
        [SerializeField] Transform leftRearWheel;
        [SerializeField] Transform rightRearWheel;
        [SerializeField] float wheelRotationSpeed = 500f;

        [Header("Refs")]
        [SerializeField] InputReader input;
        Rigidbody rb;

        private void Start()
        {
            rb = GetComponent<Rigidbody>();
            input.Enable();

            // Completely lock the physics body so it can never tip over or flip backwards
            rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
        }

        void FixedUpdate()
        {
            float verticalInput = AdjustInput(input.Move.y);
            float horizontalInput = AdjustInput(input.Move.x);

            // 1. ARCADE ACCELERATION & SMOOTH COASTING
            if (Mathf.Abs(verticalInput) > 0.05f)
            {
                Vector3 targetVelocity = transform.forward * verticalInput * maxSpeed;
                // Preserve whatever vertical gravity speed the car has so it falls smoothly
                targetVelocity.y = rb.linearVelocity.y;
                rb.linearVelocity = Vector3.MoveTowards(rb.linearVelocity, targetVelocity, acceleration * Time.deltaTime);
            }
            else
            {
                // Smoothly coast to a stop when you let go of the keys
                Vector3 targetVelocity = new Vector3(0f, rb.linearVelocity.y, 0f);
                Vector3 currentHorizontal = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);
                Vector3 newHorizontal = Vector3.MoveTowards(currentHorizontal, Vector3.zero, coastDeceleration * Time.deltaTime);

                rb.linearVelocity = new Vector3(newHorizontal.x, rb.linearVelocity.y, newHorizontal.z);
            }

            // 2. ARCADE STEERING (Turns instantly on the spot like a go-kart)
            if (rb.linearVelocity.magnitude > 1f)
            {
                float steerDirection = Mathf.Sign(Vector3.Dot(rb.linearVelocity, transform.forward));
                float turnAmount = horizontalInput * turnSpeed * steerDirection * Time.deltaTime;
                transform.Rotate(0f, turnAmount, 0f);
            }

            // 3. FAKE REEL WHEEL SPINNING (Cosmetic rolling visual)
            float currentSpeed = Vector3.Dot(rb.linearVelocity, transform.forward);
            if (Mathf.Abs(currentSpeed) > 0.1f)
            {
                float spinAngle = currentSpeed * wheelRotationSpeed * Time.deltaTime;

                // CHANGED: Swapped Vector3.right to Vector3.forward to match your sideways tires!
                if (frontWheel != null) frontWheel.Rotate(Vector3.forward * spinAngle, Space.Self);
                if (leftRearWheel != null) leftRearWheel.Rotate(Vector3.forward * spinAngle, Space.Self);
                if (rightRearWheel != null) rightRearWheel.Rotate(Vector3.forward * spinAngle, Space.Self);
            }
        }

        float AdjustInput(float input)
        {
            return input switch
            {
                >= .7f => 1f,
                <= -.7f => -1f,
                _ => input
            };
        }
    }
}
