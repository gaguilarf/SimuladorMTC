using UnityEngine;

[ExecuteAlways] // Se ejecuta tambi�n en modo edici�n
public class ShowCenterOfMass : MonoBehaviour
{
    private Rigidbody rb;

    private void OnDrawGizmos()
    {
        rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            Gizmos.color = Color.green;
            Vector3 worldCOM = transform.TransformPoint(rb.centerOfMass);
            Gizmos.DrawSphere(worldCOM, 0.1f);
            Gizmos.DrawLine(transform.position, worldCOM);
        }
    }
}