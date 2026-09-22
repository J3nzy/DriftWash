using UnityEngine;

public class DirtPuddle : MonoBehaviour
{
    [Header("Arcade Shrink Settings")]
    [Tooltip("How fast the puddle shrinks inward when driven over.")]
    public float shrinkSpeed = 5f;

    [Tooltip("When the X scale drops below this number, the puddle is fully deleted.")]
    public float destroyThreshold = 0.05f;

    private bool isShrinking = false;
    private bool isDestroyed = false;

    void Start()
    {
        // Enforce the tag automatically so your tracking scripts can count it
        if (!gameObject.CompareTag("DirtPuddle"))
        {
            gameObject.tag = "DirtPuddle";
        }

        // Force the puddle collider to be a trigger so the car glides straight through it
        Collider col = GetComponent<Collider>();
        if (col != null)
        {
            col.isTrigger = true;
        }
    }

    void Update()
    {
        if (isDestroyed) return;

        // Once the car touches us, this code runs automatically until it disappears
        if (isShrinking)
        {
            // Smoothly collapse the horizontal width and length (X and Z scales) down to zero
            float newX = Mathf.MoveTowards(transform.localScale.x, 0f, shrinkSpeed * Time.deltaTime);
            float newZ = Mathf.MoveTowards(transform.localScale.z, 0f, shrinkSpeed * Time.deltaTime);

            // Keep the flat Y height exactly what it is so it doesn't do weird vertical clipping
            transform.localScale = new Vector3(newX, transform.localScale.y, newZ);

            // Once it shrinks past your threshold, completely erase it from the supermarket aisle
            if (transform.localScale.x <= destroyThreshold)
            {
                FinishPuddleCleanup();
            }
        }
    }

    // This built-in Unity function fires the exact millisecond any car collider touches our zone
    void OnTriggerEnter(Collider other)
    {
        if (isShrinking || isDestroyed) return;

        // Check if the object touching us is on the Default layer (where your car is)
        // or check if it has a Rigidbody (the main vehicle body)
        if (other.attachedRigidbody != null || other.gameObject.layer == 0)
        {
            isShrinking = true;
        }
    }

    private void FinishPuddleCleanup()
    {
        isDestroyed = true;

        // Safely notify your PuddleProgressTracker using the optimized Unity 6 method
        PuddleProgressTracker tracker = Object.FindAnyObjectByType<PuddleProgressTracker>();
        if (tracker != null && tracker.enabled)
        {
            tracker.OnPuddleCleaned();
        }

        Destroy(gameObject);
    }
}
