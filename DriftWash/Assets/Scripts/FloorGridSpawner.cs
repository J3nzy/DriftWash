using UnityEngine;

namespace DriftWash
{
    public class FloorGridSpawner : MonoBehaviour
    {
        [Header("Grid Settings")]
        [SerializeField] GameObject dirtStickerPrefab;
        [SerializeField] int gridWidth = 20;
        [SerializeField] int gridLength = 20;
        [SerializeField] float spacing = 0.5f; 

        [Header("Arcade Obstacle Detection")]
        [SerializeField] LayerMask obstacleLayer;     
        [SerializeField] float detectionRadius = 0.2f; // How wide the radar looks for an obstacle

        void Start()
        {
            GenerateDirtGrid();
        }

        void GenerateDirtGrid()
        {
            if (dirtStickerPrefab == null) return;

            for (int x = 0; x < gridWidth; x++)
            {
                for (int z = 0; z < gridLength; z++)
                {
                    float posX = transform.position.x + (x * spacing) - (gridWidth * spacing / 2f);
                    float posZ = transform.position.z + (z * spacing) - (gridLength * spacing / 2f);

                    Vector3 spawnPos = new Vector3(posX, transform.position.y + 0.01f, posZ);

                    // RADAR CHECK: Looks to see if any furniture colliders are overlapping this point
                    bool isObstacleHere = Physics.CheckSphere(spawnPos, detectionRadius, obstacleLayer);

                    // Only spawn the dirt if the spot is completely empty
                    if (!isObstacleHere)
                    {
                        GameObject newSticker = Instantiate(dirtStickerPrefab, spawnPos, Quaternion.identity);
                        newSticker.transform.parent = transform;
                    }
                }
            }
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, detectionRadius);
        }
    }
}
