using UnityEngine;

public class FruitSpawner : MonoBehaviour
{
    [Header("Fruit Setup")]
    public GameObject[] fruits;
    public float spawnInterval = 3f;
    public float spawnHeightAbovePlayer = 1.7f;
    public float spawnDistanceInFront = 3.0f;

    [Header("Fruit Physics")]
    [Range(0f, 2f)]
    public float gravityScale = 0.05f;   // <—— Stelle hier ein, wie schnell Früchte fallen sollen
    public Vector2 scaleRange = new Vector2(1.5f, 2f);

    private Transform camRig;
    private GameObject floorPlane;

    void Start()
    {
        camRig = Camera.main.transform;

        // ===== Create invisible physics floor =====
        floorPlane = GameObject.CreatePrimitive(PrimitiveType.Plane);
        floorPlane.name = "Floor";
        floorPlane.transform.localScale = new Vector3(5, 1, 5);

        // Make it invisible
        Destroy(floorPlane.GetComponent<MeshRenderer>());

        // Position floor under player
        float eyeToFeet = 1.7f;
        floorPlane.transform.position = new Vector3(
            0, camRig.position.y - eyeToFeet, 0
        );

        // Non-bouncy physics material
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

        Vector3 spawnPos = new Vector3(
            camRig.position.x,
            camRig.position.y + spawnHeightAbovePlayer,
            camRig.position.z + spawnDistanceInFront
        );

        GameObject fruit = Instantiate(fruits[index], spawnPos, Random.rotation);

        // Random scale
        float scaleMultiplier = Random.Range(scaleRange.x, scaleRange.y);
        fruit.transform.localScale *= scaleMultiplier;

        // Rigidbody setup
        Rigidbody rb = fruit.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.useGravity = false;     // Wir übernehmen die Gravitation
            rb.isKinematic = false;
        }

        // Gravity-Scale Anwenden
        fruit.AddComponent<GravityScaler>().fallSpeed = gravityScale;

    }
}


/// <summary>
/// Wendet custom-Gravity auf das Objekt an.
/// </summary>
public class GravityScaler : MonoBehaviour
{
    public float fallSpeed = 0.5f;
    private Rigidbody rb;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.useGravity = false;
    }

    void FixedUpdate()
    {
        rb.linearVelocity = new Vector3(0, -fallSpeed, 0);
    }
}

