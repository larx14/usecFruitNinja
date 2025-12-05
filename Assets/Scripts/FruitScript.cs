using UnityEngine;

public class Fruit : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Sword"))
        {
            Debug.Log("Fruit sliced!");
            Destroy(gameObject);
        }
    }
}
