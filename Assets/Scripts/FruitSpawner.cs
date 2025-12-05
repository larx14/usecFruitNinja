using UnityEngine;

public class FruitSpawner : MonoBehaviour
{
    public GameObject[] fruits;
    public float spawnInterval = 1.2f;
    public float spawnForce = 2f;

    void Start()
    {
        InvokeRepeating(nameof(SpawnFruit), 1f, spawnInterval);
    }

    void SpawnFruit()
    {
        int index = Random.Range(0, fruits.Length);

        // leicht zufällige Position in X-Richtung
        Vector3 spawnPos = transform.position +
                           new Vector3(Random.Range(-0.5f, 0.5f), 0f, 0f);

        GameObject fruit = Instantiate(fruits[index], spawnPos, Random.rotation);

        Rigidbody rb = fruit.GetComponent<Rigidbody>();

        // statt nach oben → nach unten (fallen lassen)
        rb.AddForce(Vector3.down * spawnForce, ForceMode.Impulse);
    }

}
