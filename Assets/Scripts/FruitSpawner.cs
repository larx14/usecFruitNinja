using UnityEngine;

public class FruitSpawner : MonoBehaviour
{
    [Header("Fruit Setup")]
    public GameObject[] fruits;
    public float spawnInterval = 1.2f;
    public float spawnForce = 5f;
    public float xRange = 0.5f;

    [Header("Camera Follow")]
    public float distanceInFront = 0.5f;   // 50cm in front of camera
    public float heightAbove = 0.5f;       // spawn above player's head

    private Transform cam;

    void Start()
    {
        cam = Camera.main.transform;
        InvokeRepeating(nameof(SpawnFruit), 1f, spawnInterval);
    }

    void LateUpdate()
    {
        if (cam == null) return;

        // Keep spawner in front & above camera, but NOT parented to it
        transform.position = cam.position + cam.forward * distanceInFront + Vector3.up * heightAbove;
    }

    void SpawnFruit()
    {
        if (fruits.Length == 0) return;

        int index = Random.Range(0, fruits.Length);

        Vector3 spawnPos = transform.position +
                           new Vector3(Random.Range(-xRange, xRange), 0f, 0f);

        GameObject fruit = Instantiate(fruits[index], spawnPos, Quaternion.identity);

        Rigidbody rb = fruit.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.useGravity = true;
            rb.isKinematic = false;

            // Drop downward force if you want faster fall
            rb.AddForce(Vector3.down * spawnForce, ForceMode.Impulse);
        }
    }
}
