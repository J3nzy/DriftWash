using UnityEngine;

namespace DriftWash
{
    public class CleaningTile : MonoBehaviour
    {
        [Header("Tile Settings")]
        [SerializeField] Material cleanMaterial;
        [SerializeField] ParticleSystem cleanParticles;

        private bool isClean = false;

        private void OnTriggerEnter(Collider other)
        {
            // If the tile is already clean, or the object touching it isn't our car, ignore it!
            if (isClean || !other.CompareTag("Player")) return;

            CleanTheFloor();
        }

        void CleanTheFloor()
        {
            isClean = true;

            // Spawn a satisfying soap or water burst effect right at the floor layer
            if (cleanParticles != null)
            {
                Instantiate(cleanParticles, transform.position, Quaternion.identity);
            }

            // ARCADE DISAPPEAR: This instantly deletes the dirt patch sticker from the map, 
            // revealing the solid clean floor hiding directly underneath your vehicle!
            Destroy(gameObject);
        }
    }
}
