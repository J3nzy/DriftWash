using DriftWash;
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

        [Header("Braking and Drifting Attributes")]
        [SerializeField] float brakeTorque = 10000f;

        [SerializeField] InputReader input;
        Rigidbody rb;

        float brakeVelocity;

        private void Start()
        {
            rb = GetComponent<Rigidbody>();
            input.Enable();

            rb.centerOfMass = new Vector3(0f, -0.5f, 0f);

            foreach (AxleInfo axleInfo in axleInfos)
            {
                axleInfo.originalForwardFriction = axleInfo.leftWheel.forwardFriction;
                axleInfo.originalSidewaysFriction = axleInfo.leftWheel.sidewaysFriction;

                // Save the manual distance between your wheels and the colliders before the game starts
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
                }
                else
                {
                    rb.constraints = RigidbodyConstraints.None;

                    // Add safety checks for releasing brakes
                    if (axleInfo.leftWheel != null) axleInfo.leftWheel.brakeTorque = 0;
                    if (axleInfo.rightWheel != null) axleInfo.rightWheel.brakeTorque = 0;
                }
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
    // Extension method to fix the "With" compilation error
    public static class Vector3Extensions
    {
        public static Vector3 With(this Vector3 vector, float? x = null, float? y = null, float? z = null)
        {
            return new Vector3(x ?? vector.x, y ?? vector.y, z ?? vector.z);
        }
    }
