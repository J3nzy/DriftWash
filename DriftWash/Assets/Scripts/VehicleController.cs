using UnityEngine;
using UnityEngine.InputSystem;

namespace DriftWash
{
    public class VehicleController : MonoBehaviour
    {
        [Header("Arcade Driving")]
        [SerializeField] float maxSpeed = 30f;
        [SerializeField] float scrubMaxSpeed = 10f;
        [SerializeField] float acceleration = 20f;
        [SerializeField] float coastDeceleration = 5f;
        [SerializeField] float turnSpeed = 90f;

        [Header("Drift Physics Settings")]
        [Tooltip("Select the key to start drifting/sliding.")]
        [SerializeField] Key driftKey = Key.Space;

        [Tooltip("How much the car slides sideways during a full drift. Lower values mean wider, crazier slides!")]
        [Range(0.01f, 0.95f)][SerializeField] float driftTraction = 0.85f;

        [Tooltip("How smoothly the car enters the drift physics. Higher = Snappy, Lower = Smooth & Gradual weight transfer.")]
        [Range(1f, 20f)][SerializeField] float driftTransitionSpeed = 4f;

        [Tooltip("How much faster the car physics rotates while actively drifting.")]
        [SerializeField] float driftTurnMultiplier = 1.5f;

        [Tooltip("How fast the car slows down when you let go of the gas WHILE DRIFTING. Lower = slides longer without power.")]
        [SerializeField] float driftCoastDeceleration = 1f; // <-- NEW: Separate slow-down speed for slides

        [Header("Visual Drift Angling")]
        [Tooltip("Drag the child GameObject that holds your car mesh here. It will rotate sideways visually!")]
        [SerializeField] Transform visualModel;

        [Tooltip("If your car art faces the wrong way by default, adjust this angle until it points straight ahead! Try 90 or -90.")]
        [SerializeField] float visualStartingOffsetAngle = 0f;

        [Tooltip("How far sideways the car model turns visually during a full drift (in degrees).")]
        [SerializeField] float maxVisualDriftAngle = 35f;

        [Tooltip("How fast the car model snaps into or recovers from the visual drift angle. Higher = Faster Snapping.")]
        [Range(1f, 30f)][SerializeField] float visualDriftSnapSpeed = 10f;

        [Header("Visual Wheels (Cosmetic Only)")]
        [SerializeField] Transform frontWheel;
        [SerializeField] Transform leftRearWheel;
        [SerializeField] Transform rightRearWheel;
        [SerializeField] float wheelRotationSpeed = 500f;

        [Header("Refs")]
        [SerializeField] InputReader input;
        Rigidbody rb;

        // Internal tracker to smoothly blend the vehicle's grip state
        private float currentTractionGrip = 1f;

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

            // 1. CLEAN GEAR CLAMP
            float currentMaxSpeed = maxSpeed;

            if (Keyboard.current != null && Keyboard.current.leftShiftKey.isPressed)
            {
                currentMaxSpeed = scrubMaxSpeed;

                if (rb.linearVelocity.magnitude > scrubMaxSpeed)
                {
                    rb.linearVelocity = rb.linearVelocity.normalized * scrubMaxSpeed;
                }
            }

            // 3. DRIFT CONDITION CHECK (Moved up so deceleration math can use it)
            bool isDriftKeyPressed = Keyboard.current != null && Keyboard.current[driftKey].isPressed && rb.linearVelocity.magnitude > 2f;

            // 2. ARCADE ACCELERATION & SMOOTH COASTING
            if (Mathf.Abs(verticalInput) > 0.05f)
            {
                Vector3 targetVelocity = transform.forward * verticalInput * currentMaxSpeed;
                targetVelocity.y = rb.linearVelocity.y;
                rb.linearVelocity = Vector3.MoveTowards(rb.linearVelocity, targetVelocity, acceleration * Time.deltaTime);
            }
            else
            {
                // NEW FIX: Pick normal stopping speed or dynamic slippery slide coasting speed based on drift state
                float activeDeceleration = isDriftKeyPressed ? driftCoastDeceleration : coastDeceleration;

                Vector3 targetVelocity = new Vector3(0f, rb.linearVelocity.y, 0f);
                Vector3 currentHorizontal = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);
                Vector3 newHorizontal = Vector3.MoveTowards(currentHorizontal, Vector3.zero, activeDeceleration * Time.deltaTime);

                rb.linearVelocity = new Vector3(newHorizontal.x, rb.linearVelocity.y, newHorizontal.z);
            }

            // 3. SMOOTHED DRIFT & TRACTION CONTROL MATH
            Vector3 forwardVel = transform.forward * Vector3.Dot(rb.linearVelocity, transform.forward);
            Vector3 sideVel = transform.right * Vector3.Dot(rb.linearVelocity, transform.right);

            if (!isDriftKeyPressed)
            {
                currentTractionGrip = Mathf.MoveTowards(currentTractionGrip, 0f, driftTransitionSpeed * Time.deltaTime);
                forwardVel.y = rb.linearVelocity.y;
                rb.linearVelocity = forwardVel + (sideVel * (1f - currentTractionGrip));
            }
            else
            {
                currentTractionGrip = Mathf.MoveTowards(currentTractionGrip, 1f, driftTransitionSpeed * Time.deltaTime);
                float dynamicSidewaysTraction = Mathf.Lerp(0f, driftTraction, currentTractionGrip);
                rb.linearVelocity = forwardVel + (sideVel * dynamicSidewaysTraction);
            }

            // 4. ARCADE STEERING & VISUAL DRIFT ROTATION
            if (rb.linearVelocity.magnitude > 1f)
            {
                float steerDirection = Mathf.Sign(Vector3.Dot(rb.linearVelocity, transform.forward));
                float currentTurnSpeed = Mathf.Lerp(turnSpeed, turnSpeed * driftTurnMultiplier, currentTractionGrip);

                float turnAmount = horizontalInput * currentTurnSpeed * steerDirection * Time.deltaTime;
                transform.Rotate(0f, turnAmount, 0f);

                if (visualModel != null)
                {
                    float targetVisualAngle = 0f;

                    if (isDriftKeyPressed && Mathf.Abs(horizontalInput) > 0.05f)
                    {
                        targetVisualAngle = horizontalInput * (maxVisualDriftAngle * currentTractionGrip);
                    }

                    Quaternion targetRotation = Quaternion.Euler(0f, visualStartingOffsetAngle + targetVisualAngle, 0f);
                    visualModel.localRotation = Quaternion.Slerp(visualModel.localRotation, targetRotation, visualDriftSnapSpeed * Time.deltaTime);
                }
            }
            else
            {
                if (visualModel != null)
                {
                    Quaternion baseRotation = Quaternion.Euler(0f, visualStartingOffsetAngle, 0f);
                    visualModel.localRotation = Quaternion.Slerp(visualModel.localRotation, baseRotation, visualDriftSnapSpeed * Time.deltaTime);
                }
            }

            // 5. FAKE REEL WHEEL SPINNING
            float currentSpeed = Vector3.Dot(rb.linearVelocity, transform.forward);
            if (Mathf.Abs(currentSpeed) > 0.1f)
            {
                float spinAngle = currentSpeed * wheelRotationSpeed * Time.deltaTime;

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
