using UnityEngine;

public class FloorPainter : MonoBehaviour
{
    [Header("Setup References")]
    public RenderTexture maskTexture;
    public Transform brushLocation;

    [Tooltip("Drag your 'SoftBrush' Circle Sprite here.")]
    public Texture2D brushShapeTexture;

    [Header("Base Brush Settings")]
    [Range(0.01f, 0.5f)] public float brushSize = 0.05f;

    [Tooltip("Maximum cleaning speed when stationary or crawling.")]
    [Range(0.1f, 10f)] public float maxBrushStrength = 2.0f;

    [Header("Speed-Based Cleaning")]
    [Tooltip("The faster the car goes over this speed, the less it cleans.")]
    public float targetMaxSpeed = 20f;

    [Tooltip("The absolute minimum cleaning power when sprinting at full speed.")]
    [Range(0f, 1f)] public float minimumSpeedStrength = 0.05f;

    private Material drawMaterial;
    private Rigidbody vehicleRigidbody;

    void Start()
    {
        if (maskTexture == null || brushLocation == null)
        {
            Debug.LogError("FloorPainter: Missing references in the Inspector!");
            return;
        }

        // Try to automatically find the Rigidbody on your vehicle for speed checking
        vehicleRigidbody = GetComponent<Rigidbody>();

        // Reset the floor canvas to dirty (black) at start
        RenderTexture.active = maskTexture;
        GL.Clear(true, true, Color.black);
        RenderTexture.active = null;

        Shader accumulateShader = Shader.Find("Custom/AccumulateBrushShader");
        if (accumulateShader != null)
        {
            drawMaterial = new Material(accumulateShader);
        }
        else
        {
            drawMaterial = new Material(Shader.Find("Sprites/Default"));
        }
    }

    void Update()
    {
        if (brushLocation == null || maskTexture == null) return;

        Ray ray = new Ray(brushLocation.position, Vector3.down);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, 5f))
        {
            Vector2 pixelUV = hit.textureCoord;
            if (pixelUV != Vector2.zero)
            {
                PaintOnMask(pixelUV, hit.collider.transform.lossyScale);
            }
        }
    }

    void PaintOnMask(Vector2 uv, Vector3 floorScale)
    {
        if (brushShapeTexture == null) return;

        RenderTexture.active = maskTexture;

        GL.PushMatrix();
        GL.LoadPixelMatrix(0, maskTexture.width, 0, maskTexture.height);

        float targetX = uv.x * maskTexture.width;
        float targetY = uv.y * maskTexture.height;

        float radiusX = brushSize * maskTexture.width;
        float radiusY = brushSize * maskTexture.height;
        if (floorScale.x > 0 && floorScale.z > 0)
        {
            radiusY = radiusX * (floorScale.x / floorScale.z);
        }

        // --- SPEED CALCULATION LOGIC ---
        float currentSpeed = 0f;
        if (vehicleRigidbody != null)
        {
            // Get the physical speed of the car
            currentSpeed = vehicleRigidbody.linearVelocity.magnitude;
        }

        // Map the speed to a 0-1 scale percentage
        float speedFactor = Mathf.Clamp01(currentSpeed / targetMaxSpeed);

        // Invert it so going faster drops the effectiveness
        float efficiency = Mathf.Lerp(1f, minimumSpeedStrength, speedFactor);

        // Calculate dynamic strength multiplied by Time.deltaTime to balance frame rates!
        float dynamicStrength = maxBrushStrength * efficiency * Time.deltaTime;
        // --------------------------------

        drawMaterial.mainTexture = brushShapeTexture;
        drawMaterial.color = new Color(1, 1, 1, dynamicStrength);
        drawMaterial.SetPass(0);

        GL.Begin(GL.QUADS);

        GL.TexCoord2(0, 0); GL.Vertex3(targetX - radiusX, targetY - radiusY, 0);
        GL.TexCoord2(0, 1); GL.Vertex3(targetX - radiusX, targetY + radiusY, 0);
        GL.TexCoord2(1, 1); GL.Vertex3(targetX + radiusX, targetY + radiusY, 0);
        GL.TexCoord2(1, 0); GL.Vertex3(targetX + radiusX, targetY - radiusY, 0);

        GL.End();
        GL.PopMatrix();

        RenderTexture.active = null;
    }
}
