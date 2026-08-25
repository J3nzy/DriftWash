using DriftWash;
using System.Linq;
using UnityEngine;

namespace DriftWash
{
    [System.Serializable]
    public class AxleInfo
    {
        public WheelCollider leftWheel;
        public WheelCollider rightWheel;
        public Transform leftRowMesh;
        public Transform rightRowMesh;

        // Floats to hold the custom Y-axis angles for the wheels
        public float leftMeshYOffset = 0f;
        public float rightMeshYOffset = 0f;

        public bool motor;
        public bool steering;
        [HideInInspector] public WheelFrictionCurve originalForwardFriction;
        [HideInInspector] public WheelFrictionCurve originalSidewaysFriction;



        // Hidden variables to save your manual offsets
        [HideInInspector] public Vector3 leftMeshOffset;
        [HideInInspector] public Vector3 rightMeshOffset;
        [HideInInspector] public bool offsetsInitialized;
    }
}

    public class VehicleController : MonoBehaviour
    {
        [Header("Axle Information")]
        [SerializeField] AxleInfo[] axleInfos;

        [Header("Motor Attributes")]
        [SerializeField] float maxMotorTorque = 3000f;
        [SerializeField] float maxSpeed;

        [Header("Steering Attributes")]
        [SerializeField] float maxSteeringAngle = 30f;
        [SerializeField] AnimationCurve turnCurve;
        [SerializeField] float turnStrength = 1500f;

        [Header("Braking and Drifting Attributes")]
        [SerializeField] float driftSteerMultiplier = 1.5f; // Adjust this value to control the amount of steering during a drift
        [SerializeField] float brakeTorque = 10000f;

        [Header("Physics")]
        [SerializeField] Transform centerofMass;
        [SerializeField] float downforce = 100f;
        [SerializeField] float gravity = Physics.gravity.y;
        [SerializeField] float lateralGScale = 10f; // Scale factor for lateral G-force

        [Header("Banking")]
        [SerializeField] float maxBankAngle = 5f;
        [SerializeField] float bankSpeed = 2f;

        [Header("Refs")]
        [SerializeField] InputReader input;
        Rigidbody rb;

        Vector3 vehicleVelocity;
        float brakeVelocity;
        float driftVelocity;

    RaycastHit hit;

    const float thresholdSpeed = 10;
    const float centerOfMassOffset = -0.5f;
    Vector3 originalCenterOfMass;

    public bool IsGrounded = true;
    public Vector3 Velocity => vehicleVelocity;
    public float MaxSpeed => maxSpeed;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        input.Enable();

        rb.centerOfMass = centerofMass.localPosition;
        originalCenterOfMass = rb.centerOfMass; // <-- FIXED: Removed .localPosition from the end

        foreach (AxleInfo axleInfo in axleInfos)
        {
            axleInfo.originalForwardFriction = axleInfo.leftWheel.forwardFriction;
            axleInfo.originalSidewaysFriction = axleInfo.leftWheel.sidewaysFriction;

            if (axleInfo.leftWheel != null && axleInfo.leftRowMesh != null)
            {
                axleInfo.leftMeshOffset = axleInfo.leftWheel.transform.InverseTransformPoint(axleInfo.leftRowMesh.position);
            }
            if (axleInfo.rightWheel != null && axleInfo.rightRowMesh != null)
            {
                axleInfo.rightMeshOffset = axleInfo.rightWheel.transform.InverseTransformPoint(axleInfo.rightRowMesh.position);
            }
        }
    }


    void FixedUpdate()
        {
            float verticalInput = AdjustInput(input.Move.y);
            float horizontalInput = AdjustInput(input.Move.x);

            float motor = maxMotorTorque * verticalInput;
            float steering = maxSteeringAngle * horizontalInput;

            UpdateAxles(motor, steering);
        UpdateBanking(horizontalInput);

        vehicleVelocity = transform.InverseTransformDirection(rb.linearVelocity);

        if (IsGrounded)
        {
            HandleGroundedMovement(verticalInput, horizontalInput);
        } else
        {
            HandleAirborneMovement(verticalInput, horizontalInput);
        }
    }

    void HandleGroundedMovement(float verticalInput, float horizontalInput)
    {
        // Turning logic
        if (Mathf.Abs(verticalInput) > 0.1f || Mathf.Abs(vehicleVelocity.z) > 1)
        {
            float turnMultiplier = Mathf.Clamp01(turnCurve.Evaluate(vehicleVelocity.magnitude / maxSpeed));
            rb.AddTorque(Vector3.up * horizontalInput * Mathf.Sign(vehicleVelocity.z) * turnStrength * 100 * turnMultiplier);
        }

        // Acceleration logic
        if (!input.IsBraking)
        {
            float targetSpeed = verticalInput * maxSpeed;
            Vector3 forwardWithoutY = new Vector3(transform.forward.x, 0f, transform.forward.z).normalized; // <-- FIXED: Native Vector3 calculation without .With()
            rb.linearVelocity = Vector3.Lerp(rb.linearVelocity, forwardWithoutY * targetSpeed, Time.deltaTime);
        }

        // Downforce logic
        float speedFactor = Mathf.Clamp01(rb.linearVelocity.magnitude / maxSpeed);
        float lateralG = Mathf.Abs(Vector3.Dot(rb.linearVelocity, transform.right));
        float downForceFactor = Mathf.Max(speedFactor, lateralG / lateralGScale);
        rb.AddForce(-transform.up * downforce * rb.mass * downForceFactor);

        // Shift center of mass based on speed and lateral G-force
        float speed = rb.linearVelocity.magnitude;
        Vector3 centerOfMassAdjustment = (speed > thresholdSpeed)
            ? new Vector3(0f, 0f, Mathf.Abs(verticalInput) > 0.1f ? Mathf.Sign(verticalInput) * centerOfMassOffset : 0f) // <-- FIXED: Corrected ternary syntax and bracket placement
            : Vector3.zero;
        rb.centerOfMass = originalCenterOfMass + centerOfMassAdjustment;
    }

    void UpdateBanking (float horizontalInput)
    {
        // Calculate the target bank angle based on horizontal input
        float targetBankAngle = horizontalInput * -maxBankAngle;
        Vector3 currentEuler = transform.localEulerAngles;
        currentEuler.z = Mathf.LerpAngle(a: currentEuler.z, b: targetBankAngle, t: Time.deltaTime * bankSpeed);
        transform.localEulerAngles = currentEuler;
    }

    void HandleAirborneMovement(float verticalInput, float horizontalInput)
    {
        // Apply gravity to the vehicle when airborne
        rb.angularVelocity = Vector3.Lerp(a: rb.linearVelocity, b: rb.linearVelocity + Vector3.down * gravity, t: Time.deltaTime * gravity);
    }

    void UpdateAxles(float motor, float steering)
    {
        foreach (AxleInfo axleInfo in axleInfos)
        {
            HandleSteering(axleInfo, steering);
            HandleMotor(axleInfo, motor);
            HandleBrakesAndDrift(axleInfo);

            // Pass the physics collider, visual mesh, and its unique Y offset angle
            if (axleInfo.leftWheel != null && axleInfo.leftRowMesh != null)
                UpdateWheelVisuals(axleInfo.leftWheel, axleInfo.leftRowMesh, axleInfo.leftMeshYOffset);

            if (axleInfo.rightWheel != null && axleInfo.rightRowMesh != null)
                UpdateWheelVisuals(axleInfo.rightWheel, axleInfo.rightRowMesh, axleInfo.rightMeshYOffset);
        }
    }

    void UpdateWheelVisuals(WheelCollider collider, Transform visualWheel, float yOffset)
    {
        Vector3 position;
        Quaternion rotation;
        collider.GetWorldPose(out position, out rotation);

        visualWheel.position = position;

        // Apply the Y offset to the wheel's rotation while preserving the original rotation
        visualWheel.rotation = rotation * Quaternion.Euler(0, yOffset, 0);
    }

    void HandleSteering(AxleInfo axleInfo, float steering)
        {
            if (axleInfo.steering)
            {
                // Add safety checks (!= null) so Unity skips empty slots safely
                if (axleInfo.leftWheel != null) axleInfo.leftWheel.steerAngle = steering;
                if (axleInfo.rightWheel != null) axleInfo.rightWheel.steerAngle = steering;
            }
        }

        void HandleMotor(AxleInfo axleInfo, float motor)
        {
            if (axleInfo.motor)
            {
                // Add safety checks here too
                if (axleInfo.leftWheel != null) axleInfo.leftWheel.motorTorque = motor;
                if (axleInfo.rightWheel != null) axleInfo.rightWheel.motorTorque = motor;
            }
        }

        void HandleBrakesAndDrift(AxleInfo axleInfo)
        {
            if (axleInfo.motor)
            {
                if (input.IsBraking)
                {
                    rb.constraints = RigidbodyConstraints.FreezeRotationX;

                    float newZ = Mathf.SmoothDamp(rb.linearVelocity.z, 0, ref brakeVelocity, 1f);
                    rb.linearVelocity = rb.linearVelocity.With(z: newZ);

                    // Add safety checks for braking
                    if (axleInfo.leftWheel != null) axleInfo.leftWheel.brakeTorque = brakeTorque;
                    if (axleInfo.rightWheel != null) axleInfo.rightWheel.brakeTorque = brakeTorque;
                ApplyDriftFriction(axleInfo.leftWheel);
                ApplyDriftFriction(axleInfo.rightWheel);
            }
                else
                {
                    rb.constraints = RigidbodyConstraints.None;

                    // Add safety checks for releasing brakes
                    if (axleInfo.leftWheel != null) axleInfo.leftWheel.brakeTorque = 0;
                    if (axleInfo.rightWheel != null) axleInfo.rightWheel.brakeTorque = 0;
                    ResetDriftFunction(axleInfo.leftWheel);
                    ResetDriftFunction(axleInfo.rightWheel);
            }
            }
        }

    void ResetDriftFunction(WheelCollider wheel)
    {
        AxleInfo axleInfo = axleInfos.FirstOrDefault(axle => axle.leftWheel == wheel || axle.rightWheel == wheel);
        if (axleInfo == null) return;

        wheel.forwardFriction = axleInfo.originalForwardFriction;
        wheel.sidewaysFriction = axleInfo.originalSidewaysFriction;
    }

    void ApplyDriftFriction(WheelCollider wheel)
    {
        if (wheel.GetGroundHit(out var hit)) {
            wheel.forwardFriction = UpdateFriction(wheel.forwardFriction);
            wheel.sidewaysFriction = UpdateFriction(wheel.sidewaysFriction);
            IsGrounded = true;
        }
    }

    WheelFrictionCurve UpdateFriction(WheelFrictionCurve friction)
    {
        friction.stiffness = input.IsBraking ? Mathf.SmoothDamp(current: friction.stiffness, target: 5f, ref driftVelocity, smoothTime: Time.deltaTime * 2f) : 1f;
        return friction;
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
    // Extension method to fix the "With" compilation error
    public static class Vector3Extensions
    {
        public static Vector3 With(this Vector3 vector, float? x = null, float? y = null, float? z = null)
        {
            return new Vector3(x ?? vector.x, y ?? vector.y, z ?? vector.z);
        }
    }
