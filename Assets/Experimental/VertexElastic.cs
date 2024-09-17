//using UnityEngine;

//public class VertexElastic : MonoBehaviour
//{
//    public float elasticity = 5.0f;  // How quickly the vertex returns to its original position
//    public float displacementAmount = 0.1f; // Maximum displacement distance
//    public MeshFilter meshFilter; // Reference to the MeshFilter component

//    private Vector3 originalPosition;
//    private Vector3 targetPosition;
//    private bool isMoving = false;

//    void Start()
//    {
//        if (meshFilter == null)
//        {
//            meshFilter = GetComponentInParent<MeshFilter>(); // Find the MeshFilter in the parent if not assigned
//        }

//        originalPosition = transform.position;
//        targetPosition = originalPosition;
//    }

//    void Update()
//    {
//        if (isMoving)
//        {
//            // Move the vertex towards the target position
//            transform.position = Vector3.Lerp(transform.position, targetPosition, Time.deltaTime * elasticity);

//            // Update the corresponding vertex position in the mesh
//            if (meshFilter != null && meshFilter.mesh != null)
//            {
//                Mesh mesh = meshFilter.mesh;
//                Vector3[] vertices = mesh.vertices;
//                int index = int.Parse(name.Split('_')[1]); // Get vertex index from name
//                Vector3 worldPosition = transform.position;
//                vertices[index] = meshFilter.transform.InverseTransformPoint(worldPosition);
//                mesh.vertices = vertices;
//                mesh.RecalculateNormals(); // Recalculate normals if needed
//            }

//            // Check if close enough to the target position to stop moving
//            if (Vector3.Distance(transform.position, targetPosition) < 0.01f)
//            {
//                transform.position = targetPosition;
//                isMoving = false;
//            }
//        }
//    }

//    public void ApplyDisplacement(Vector3 direction)
//    {
//        targetPosition = originalPosition + direction * displacementAmount;
//        isMoving = true;
//    }

//    public void ResetPosition()
//    {
//        targetPosition = originalPosition;
//        isMoving = true;
//    }
//}

using UnityEngine;

public class VertexElastic : MonoBehaviour
{
    public float elasticity = 5.0f;
    public float displacementAmount = 0.1f;
    public MeshFilter meshFilter;
    public float rippleStrength = 1.0f;  // Strength of the ripple effect
    public float rippleDecay = 0.1f;    // How quickly the ripple effect decays
    public float rippleDuration = 1.0f; // Duration of the ripple effect

    private Vector3 originalPosition;
    private Vector3 targetPosition;
    private bool isMoving = false;
    private float rippleEndTime;

    void Start()
    {
        if (meshFilter == null)
        {
            meshFilter = GetComponentInParent<MeshFilter>();
        }

        originalPosition = transform.position;
        targetPosition = originalPosition;
    }

    void Update()
    {
        if (isMoving)
        {
            // Move the vertex towards the target position
            transform.position = Vector3.Lerp(transform.position, targetPosition, Time.deltaTime * elasticity);

            if (meshFilter != null && meshFilter.mesh != null)
            {
                Mesh mesh = meshFilter.mesh;
                Vector3[] vertices = mesh.vertices;
                int index = int.Parse(name.Split('_')[1]);
                Vector3 worldPosition = transform.position;
                vertices[index] = meshFilter.transform.InverseTransformPoint(worldPosition);
                mesh.vertices = vertices;
                mesh.RecalculateNormals();
            }

            if (Time.time >= rippleEndTime)
            {
                transform.position = targetPosition;
                isMoving = false;
            }
        }
    }

    public void ApplyRipple(Vector3 direction, float strength)
    {
        targetPosition = originalPosition + direction * strength;
        isMoving = true;
        rippleEndTime = Time.time + rippleDuration;
    }

    public void ResetPosition()
    {
        targetPosition = originalPosition;
        isMoving = true;
    }
}
