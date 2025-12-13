using UnityEngine;

public class FruitScript : MonoBehaviour
{
    [Header("Feedback")]
    public AudioClip sliceSound;
    public GameObject hitEffectPrefab;

    [Header("Despawn")]
    public float despawnAfterSeconds = 3f;

    private Rigidbody rb;
    private bool isSliced = false;
    private bool despawnScheduled = false;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    private void Update()
    {
        // When the fruit has almost stopped moving, start despawn timer
        if (!despawnScheduled && rb.linearVelocity.magnitude < 0.05f)
        {
            despawnScheduled = true;
            Invoke(nameof(Despawn), despawnAfterSeconds);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (isSliced) return;
        if (!other.CompareTag("Sword")) return;

        isSliced = true;
        SliceFruit();
    }

    private void SliceFruit()
    {
        if (sliceSound != null)
            AudioSource.PlayClipAtPoint(sliceSound, transform.position);

        if (hitEffectPrefab != null)
            Instantiate(hitEffectPrefab, transform.position, Quaternion.identity);

        LoggerScript logger = FindObjectOfType<LoggerScript>();
        if (logger != null)
            logger.AddRepetition();

        Destroy(gameObject);
    }

    private void Despawn()
    {
        Destroy(gameObject);
    }
}
