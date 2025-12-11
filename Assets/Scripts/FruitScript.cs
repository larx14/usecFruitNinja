using UnityEngine;

public class Fruit : MonoBehaviour
{
    [Header("Feedback")]
    public AudioClip sliceSound;        // Sound beim Treffen
    public GameObject hitEffectPrefab;  // Saft-/Hit-Effekt Prefab

    private bool isSliced = false;      // verhindert doppeltes Auslösen

    private void OnTriggerEnter(Collider other)
    {
        // Nur reagieren, wenn noch nicht sliced UND echtes Schwert
        if (isSliced) return;
        if (!other.CompareTag("Sword")) return;

        isSliced = true;

        // 1) Slice Sound abspielen
        if (sliceSound != null)
        {
            AudioSource.PlayClipAtPoint(sliceSound, transform.position);
        }

        // 2) Saft-/Hit-Effekt erzeugen
        if (hitEffectPrefab != null)
        {
            Instantiate(hitEffectPrefab, transform.position, Quaternion.identity);
        }
        var logger = FindFirstObjectByType<LoggerScript>();
        logger.AddRepetition(); 
        // 3) Frucht zerstören
        Destroy(gameObject);
    }
}
