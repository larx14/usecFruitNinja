using UnityEngine;

public class FruitSpawner : MonoBehaviour
{
    [Header("Fruit Setup")]
    public GameObject[] fruits;
    public float spawnInterval = 3f;
    public float spawnHeightAbovePlayer = 1.5f; // Y above camera
    public float spawnDistanceInFront = 0.75f;   // Z offset from camera
    public Vector2 scaleRange = new Vector2(1.5f, 2f); // random scaling

    private Transform camRig;
    private GameObject floorPlane;

    void Start()
    {
        camRig = Camera.main.transform;

        // ===== Create invisible physics floor =====
        floorPlane = GameObject.CreatePrimitive(PrimitiveType.Plane);
        floorPlane.name = "Floor";
        floorPlane.transform.localScale = new Vector3(5, 1, 5);

        // Remove the MeshRenderer to make it invisible
        Destroy(floorPlane.GetComponent<MeshRenderer>());

        // Place floor at player’s feet
        float eyeToFeet = 1.7f; // typical eye-to-floor height
        floorPlane.transform.position = new Vector3(
            0, camRig.position.y - eyeToFeet, 0
        );

        // Assign non-bouncy PhysicsMaterial
        var floorCollider = floorPlane.GetComponent<Collider>();
        PhysicsMaterial floorMaterial = new PhysicsMaterial("FloorMaterial")
        {
            bounciness = 0f,
            frictionCombine = PhysicsMaterialCombine.Average,
            bounceCombine = PhysicsMaterialCombine.Minimum
        };
        floorCollider.material = floorMaterial;

        // Start spawning fruits
        InvokeRepeating(nameof(SpawnFruit), 1f, spawnInterval);
    }

    void SpawnFruit()
    {
        if (fruits.Length == 0 || camRig == null) return;

        int index = Random.Range(0, fruits.Length);

        // Spawn directly in front of player
        Vector3 spawnPos = new Vector3(
            camRig.position.x,
            camRig.position.y + spawnHeightAbovePlayer,
            camRig.position.z + spawnDistanceInFront
        );

        GameObject fruit = Instantiate(fruits[index], spawnPos, Random.rotation);

        // Random scaling
        float scaleMultiplier = 2f; 
        fruit.transform.localScale *= scaleMultiplier;

        // Rigidbody ensures it falls naturally
        Rigidbody rb = fruit.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.useGravity = true;
            rb.isKinematic = false;
        }
    }
}
