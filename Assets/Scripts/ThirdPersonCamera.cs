using UnityEngine;

public class ThirdPersonCamera : MonoBehaviour
{
    [Header("Target")]
    public Transform target;

    [Header("Offset y Distancia")]
    public float distance = 30.0f;
    public float height = 20.0f;
    public float targetHeightOffset = 10.0f;

    [Header("Damping (Suavizado)")]
    public float positionDamping = 5.0f;
    public float rotationDamping = 4.0f;

    [Header("Configuración Cámara")]
    public bool isMainCamera = true;  // Para distinguir si es la cámara principal

    private Camera cam;

    void Start()
    {
        cam = GetComponent<Camera>();

        // Configurar prioridad de renderizado
        if (isMainCamera)
        {
            cam.depth = 0;  // Cámara principal tiene mayor prioridad
        }
    }

    void LateUpdate()
    {
        // Solo actualizar si la cámara está activa
        if (!cam.enabled) return;

        if (!target)
        {
            Debug.LogWarning("Target no asignado para la cámara en ThirdPersonCamera. Desactivando script.");
            enabled = false;
            return;
        }

        UpdateCameraPosition();
        UpdateCameraRotation();
    }

    void UpdateCameraPosition()
    {
        Vector3 desiredPosition = target.position - (target.forward * distance) + (Vector3.up * height);
        transform.position = Vector3.Lerp(transform.position, desiredPosition, positionDamping * Time.deltaTime);
    }

    void UpdateCameraRotation()
    {
        Vector3 lookAtTargetPosition = target.position + (Vector3.up * targetHeightOffset);
        Quaternion desiredRotation = Quaternion.LookRotation(lookAtTargetPosition - transform.position, target.up);
        transform.rotation = Quaternion.Slerp(transform.rotation, desiredRotation, rotationDamping * Time.deltaTime);
    }

    // Método para cambiar el target dinámicamente
    public void SetTarget(Transform newTarget)
    {
        target = newTarget;
    }

    // Método para activar/desactivar esta cámara
    public void SetActive(bool active)
    {
        cam.enabled = active;
    }

    void OnDrawGizmos()
    {
        if (target && Application.isPlaying)
        {
            Gizmos.color = Color.yellow;
            Vector3 desiredPosition = target.position - (target.forward * distance) + (Vector3.up * height);
            Gizmos.DrawLine(transform.position, desiredPosition);
            Gizmos.DrawSphere(desiredPosition, 0.1f);

            Gizmos.color = Color.cyan;
            Vector3 lookAtTargetPosition = target.position + (Vector3.up * targetHeightOffset);
            Gizmos.DrawLine(transform.position, lookAtTargetPosition);
            Gizmos.DrawSphere(lookAtTargetPosition, 0.1f);
        }
        else if (target)
        {
            Gizmos.color = Color.gray;
            Vector3 simpleDesiredPosition = target.position - (target.forward * distance) + (Vector3.up * height);
            Gizmos.DrawLine(target.position, simpleDesiredPosition);
        }
    }
}