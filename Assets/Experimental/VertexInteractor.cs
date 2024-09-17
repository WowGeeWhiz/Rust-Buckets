//using UnityEngine;
//using System.Collections;

//public class VertexInteractor : MonoBehaviour
//{
//    public GameObject vertexPrefab;
//    public float interactionDistance = 1.0f;
//    public LayerMask playerLayer;
//    public string playerTag = "Player";
//    public float colorChangeDuration = 2.0f;

//    private Mesh mesh;
//    private Vector3[] originalVertices;
//    private GameObject[] vertexVisuals;

//    void Start()
//    {
//        mesh = GetComponent<MeshFilter>().mesh;
//        originalVertices = mesh.vertices;
//        vertexVisuals = new GameObject[originalVertices.Length];

//        for (int i = 0; i < originalVertices.Length; i++)
//        {
//            Vector3 worldPosition = transform.TransformPoint(originalVertices[i]);
//            GameObject vertexVisual = Instantiate(vertexPrefab, worldPosition, Quaternion.identity, transform);
//            vertexVisual.name = $"Vertex_{i}";

//            VertexElastic vertexElastic = vertexVisual.GetComponent<VertexElastic>();
//            if (vertexElastic != null)
//            {
//                vertexElastic.meshFilter = GetComponent<MeshFilter>(); // Assign the MeshFilter
//            }

//            Renderer renderer = vertexVisual.GetComponent<Renderer>();
//            if (renderer != null)
//            {
//                Material material = renderer.material;
//                if (material != null)
//                {
//                    material.color = Color.red;
//                }
//            }

//            Collider collider = vertexVisual.GetComponent<Collider>();
//            if (collider == null)
//            {
//                collider = vertexVisual.AddComponent<SphereCollider>();
//            }
//            collider.isTrigger = true;

//            vertexVisuals[i] = vertexVisual;
//        }

//        Debug.Log("Number of vertices found: " + originalVertices.Length);
//    }

//    void Update()
//    {
//        for (int i = 0; i < vertexVisuals.Length; i++)
//        {
//            GameObject vertexVisual = vertexVisuals[i];
//            VertexTrigger vertexTrigger = vertexVisual.GetComponent<VertexTrigger>();

//            if (vertexTrigger != null && vertexTrigger.isTriggered)
//            {
//                StartCoroutine(HandleVertexInteraction(vertexVisual));
//            }
//        }
//    }

//    private IEnumerator HandleVertexInteraction(GameObject vertexVisual)
//    {
//        VertexElastic vertexElastic = vertexVisual.GetComponent<VertexElastic>();
//        if (vertexElastic != null)
//        {
//            Vector3 direction = (vertexVisual.transform.position - transform.position).normalized;
//            vertexElastic.ApplyDisplacement(direction);

//            StartCoroutine(ChangeVertexColor(vertexVisual, Color.green, Color.red, colorChangeDuration));

//            yield return new WaitForSeconds(colorChangeDuration);

//            vertexElastic.ResetPosition();
//        }
//    }

//    private IEnumerator ChangeVertexColor(GameObject vertexVisual, Color newColor, Color originalColor, float delay)
//    {
//        Renderer renderer = vertexVisual.GetComponent<Renderer>();
//        if (renderer != null)
//        {
//            Material material = renderer.material;
//            if (material != null)
//            {
//                material.color = newColor;
//                yield return new WaitForSeconds(delay);
//                material.color = originalColor;
//            }
//        }
//    }
//}

using UnityEngine;
using System.Collections;

public class VertexInteractor : MonoBehaviour
{
    public GameObject vertexPrefab;
    public float interactionDistance = 1.0f;
    public LayerMask playerLayer;
    public string playerTag = "Player";
    public float colorChangeDuration = 2.0f;
    public float rippleStrength = 1.0f;
    public float rippleRadius = 2.0f;  // Radius of the ripple effect

    private Mesh mesh;
    private Vector3[] originalVertices;
    private GameObject[] vertexVisuals;

    void Start()
    {
        mesh = GetComponent<MeshFilter>().mesh;
        originalVertices = mesh.vertices;
        vertexVisuals = new GameObject[originalVertices.Length];

        for (int i = 0; i < originalVertices.Length; i++)
        {
            Vector3 worldPosition = transform.TransformPoint(originalVertices[i]);
            GameObject vertexVisual = Instantiate(vertexPrefab, worldPosition, Quaternion.identity, transform);
            vertexVisual.name = $"Vertex_{i}";

            VertexElastic vertexElastic = vertexVisual.GetComponent<VertexElastic>();
            if (vertexElastic != null)
            {
                vertexElastic.meshFilter = GetComponent<MeshFilter>();
            }

            Renderer renderer = vertexVisual.GetComponent<Renderer>();
            if (renderer != null)
            {
                Material material = renderer.material;
                if (material != null)
                {
                    material.color = Color.red;
                }
            }

            Collider collider = vertexVisual.GetComponent<Collider>();
            if (collider == null)
            {
                collider = vertexVisual.AddComponent<SphereCollider>();
            }
            collider.isTrigger = true;

            vertexVisuals[i] = vertexVisual;
        }

        Debug.Log("Number of vertices found: " + originalVertices.Length);
    }

    void Update()
    {
        for (int i = 0; i < vertexVisuals.Length; i++)
        {
            GameObject vertexVisual = vertexVisuals[i];
            VertexTrigger vertexTrigger = vertexVisual.GetComponent<VertexTrigger>();

            if (vertexTrigger != null && vertexTrigger.isTriggered)
            {
                StartCoroutine(HandleVertexInteraction(vertexVisual));
            }
        }
    }

    private IEnumerator HandleVertexInteraction(GameObject vertexVisual)
    {
        Vector3 interactionPosition = vertexVisual.transform.position;
        Vector3 direction = (interactionPosition - transform.position).normalized;

        // Apply ripple effect to the vertex and its neighbors
        ApplyRippleEffect(vertexVisual, direction);

        StartCoroutine(ChangeVertexColor(vertexVisual, Color.green, Color.red, colorChangeDuration));

        yield return new WaitForSeconds(colorChangeDuration);

        VertexElastic vertexElastic = vertexVisual.GetComponent<VertexElastic>();
        if (vertexElastic != null)
        {
            vertexElastic.ResetPosition();
        }
    }

    private void ApplyRippleEffect(GameObject centerVertex, Vector3 direction)
    {
        foreach (GameObject vertexVisual in vertexVisuals)
        {
            if (vertexVisual != centerVertex)
            {
                float distance = Vector3.Distance(vertexVisual.transform.position, centerVertex.transform.position);
                if (distance <= rippleRadius)
                {
                    float strength = rippleStrength * (1.0f - (distance / rippleRadius));
                    VertexElastic vertexElastic = vertexVisual.GetComponent<VertexElastic>();
                    if (vertexElastic != null)
                    {
                        vertexElastic.ApplyRipple(direction, strength);
                    }
                }
            }
        }
    }

    private IEnumerator ChangeVertexColor(GameObject vertexVisual, Color newColor, Color originalColor, float delay)
    {
        Renderer renderer = vertexVisual.GetComponent<Renderer>();
        if (renderer != null)
        {
            Material material = renderer.material;
            if (material != null)
            {
                material.color = newColor;
                yield return new WaitForSeconds(delay);
                material.color = originalColor;
            }
        }
    }
}

