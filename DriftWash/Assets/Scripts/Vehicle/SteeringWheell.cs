using UnityEngine;

namespace DriftWash
{
    public class SteeringWheell : MonoBehaviour
    {
        public float turnSpeed = 100f;

        void Update()
        {
            // Get horizontal input (A/D keys or Left/Right arrows)
            float input = Input.GetAxis("Horizontal");

            // Rotate the steering wheel around its forward axis
            transform.Rotate(Vector3.forward * -input * turnSpeed * Time.deltaTime);
        }
    }
}