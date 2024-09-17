using UnityEngine;
using System.Collections;

public class VertexTrigger : MonoBehaviour
{
    public bool isTriggered { get; private set; } = false; // Indicates whether the vertex has been triggered

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isTriggered = true;
            StartCoroutine(ResetTriggerAfterDelay(2.0f)); // Reset trigger after 2 seconds
        }
    }

    private IEnumerator ResetTriggerAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        isTriggered = false;
    }
}
