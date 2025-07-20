using UnityEngine;

public class ExistingCamerasManager : MonoBehaviour
{
    [Header("--- TARGET DEL CARRO ---")]
    public Transform carTarget;

    [Header("--- CÁMARAS EXISTENTES ---")]
    public Camera mainCamera;       
    public Camera leftMirrorCamera;  
    public Camera rightMirrorCamera; 

    [Header("--- CONFIGURACIÓN CÁMARA PRINCIPAL ---")]
    public float distance = 7.0f;
    public float height = 3.0f;
    public float targetHeightOffset = 1.0f;
    public float positionDamping = 5.0f;
    public float rotationDamping = 4.0f;

    [Header("--- CONFIGURACIÓN RETROVISORES ---")]
    [Tooltip("Posición del retrovisor izquierdo relativa al carro (X=izq/der, Y=arriba/abajo, Z=adelante/atrás)")]
    public Vector3 leftMirrorOffset = new Vector3(-0.8f, 1.2f, 0.3f);
    [Tooltip("Posición del retrovisor derecho relativa al carro (X=izq/der, Y=arriba/abajo, Z=adelante/atrás)")]
    public Vector3 rightMirrorOffset = new Vector3(0.8f, 1.2f, 0.3f);

    [Header("--- CONTROLES ---")]
    public KeyCode switchCameraKey = KeyCode.C;
    public KeyCode toggleMirrorsKey = KeyCode.M;

    [Header("--- DEBUG ---")]
    public bool showGizmos = true;
    public bool showDebugInfo = false;

    // Estado del sistema
    private enum CameraMode { ThirdPerson, LeftMirror, RightMirror }
    private CameraMode currentMode = CameraMode.ThirdPerson;
    private bool mirrorsEnabled = true;

    void Start()
    {
        ValidateSetup();
        ConfigureCameras();
        SetActiveCamera(currentMode);
    }

    void ValidateSetup()
    {
        bool hasErrors = false;

        if (carTarget == null)
        {
            Debug.LogError("¡FALTA ASIGNAR EL CAR TARGET!");
            hasErrors = true;
        }

        if (mainCamera == null)
        {
            Debug.LogError("¡FALTA ASIGNAR LA MAIN CAMERA!");
            hasErrors = true;
        }

        if (leftMirrorCamera == null)
        {
            Debug.LogWarning("Left Mirror Camera no asignada - el retrovisor izquierdo no funcionará");
        }

        if (rightMirrorCamera == null)
        {
            Debug.LogWarning("Right Mirror Camera no asignada - el retrovisor derecho no funcionará");
        }

        if (!hasErrors)
        {
            Debug.Log("✓ Camera Manager configurado correctamente!");
        }
    }

    void ConfigureCameras()
    {
        // Configurar profundidades de renderizado
        if (mainCamera != null) mainCamera.depth = 0;
        if (leftMirrorCamera != null) leftMirrorCamera.depth = -2;
        if (rightMirrorCamera != null) rightMirrorCamera.depth = -1;
    }

    void Update()
    {
        if (carTarget == null) return;

        // Actualizar posiciones de todas las cámaras
        UpdateCameras();

        // Manejar controles
        HandleInput();

        // Debug info
        if (showDebugInfo)
        {
            ShowDebugInfo();
        }
    }

    void UpdateCameras()
    {
        // Actualizar cámara principal (tercera persona)
        UpdateThirdPersonCamera();

        // Actualizar retrovisores si están habilitados
        if (mirrorsEnabled)
        {
            UpdateMirrorCamera(leftMirrorCamera, leftMirrorOffset, "Izquierdo");
            UpdateMirrorCamera(rightMirrorCamera, rightMirrorOffset, "Derecho");
        }
    }

    void UpdateThirdPersonCamera()
    {
        if (mainCamera == null) return;

        // Posición deseada (detrás del carro)
        Vector3 desiredPosition = carTarget.position - (carTarget.forward * distance) + (Vector3.up * height);
        mainCamera.transform.position = Vector3.Lerp(mainCamera.transform.position, desiredPosition, positionDamping * Time.deltaTime);

        // Rotación para mirar al carro
        Vector3 lookAtTarget = carTarget.position + (Vector3.up * targetHeightOffset);
        Quaternion desiredRotation = Quaternion.LookRotation(lookAtTarget - mainCamera.transform.position, carTarget.up);
        mainCamera.transform.rotation = Quaternion.Slerp(mainCamera.transform.rotation, desiredRotation, rotationDamping * Time.deltaTime);
    }

    void UpdateMirrorCamera(Camera mirrorCam, Vector3 offset, string mirrorName)
    {
        if (mirrorCam == null) return;

        // Posición relativa al carro
        Vector3 worldPosition = carTarget.TransformPoint(offset);
        mirrorCam.transform.position = worldPosition;

        // Rotación para mirar hacia atrás
        Vector3 lookDirection = carTarget.TransformDirection(Vector3.back);
        mirrorCam.transform.rotation = Quaternion.LookRotation(lookDirection, carTarget.up);

        if (showDebugInfo)
        {
            Debug.Log($"Retrovisor {mirrorName}: Pos={worldPosition}, Offset={offset}");
        }
    }

    void HandleInput()
    {
        // Cambiar entre cámaras
        if (Input.GetKeyDown(switchCameraKey))
        {
            SwitchCamera();
        }

        // Toggle retrovisores
        if (Input.GetKeyDown(toggleMirrorsKey))
        {
            ToggleMirrors();
        }
    }

    void SwitchCamera()
    {
        // Cambiar al siguiente modo
        currentMode = (CameraMode)(((int)currentMode + 1) % 3);
        SetActiveCamera(currentMode);

        Debug.Log($"Cámara cambiada a: {currentMode}");
    }

    void SetActiveCamera(CameraMode mode)
    {
        // Desactivar todas las cámaras principales
        if (mainCamera != null) mainCamera.enabled = false;
        if (leftMirrorCamera != null) leftMirrorCamera.enabled = false;
        if (rightMirrorCamera != null) rightMirrorCamera.enabled = false;

        // Activar la cámara seleccionada
        switch (mode)
        {
            case CameraMode.ThirdPerson:
                if (mainCamera != null) mainCamera.enabled = true;
                // Mantener retrovisores activos si están habilitados
                if (mirrorsEnabled)
                {
                    if (leftMirrorCamera != null) leftMirrorCamera.enabled = true;
                    if (rightMirrorCamera != null) rightMirrorCamera.enabled = true;
                }
                break;
            case CameraMode.LeftMirror:
                if (leftMirrorCamera != null) leftMirrorCamera.enabled = true;
                break;
            case CameraMode.RightMirror:
                if (rightMirrorCamera != null) rightMirrorCamera.enabled = true;
                break;
        }
    }

    void ToggleMirrors()
    {
        mirrorsEnabled = !mirrorsEnabled;

        // Solo afectar retrovisores cuando estamos en modo tercera persona
        if (currentMode == CameraMode.ThirdPerson)
        {
            if (leftMirrorCamera != null) leftMirrorCamera.enabled = mirrorsEnabled;
            if (rightMirrorCamera != null) rightMirrorCamera.enabled = mirrorsEnabled;
        }

        Debug.Log($"Retrovisores: {(mirrorsEnabled ? "Activados" : "Desactivados")}");
    }

    void ShowDebugInfo()
    {
        if (carTarget != null)
        {
            Vector3 leftPos = carTarget.TransformPoint(leftMirrorOffset);
            Vector3 rightPos = carTarget.TransformPoint(rightMirrorOffset);

            Debug.Log($"Car Pos: {carTarget.position} | Left Mirror: {leftPos} | Right Mirror: {rightPos}");
        }
    }

    void OnDrawGizmos()
    {
        if (!showGizmos || carTarget == null) return;

        // Visualizar posición de cámara principal
        Gizmos.color = Color.green;
        Vector3 mainCamPos = carTarget.position - (carTarget.forward * distance) + (Vector3.up * height);
        Gizmos.DrawWireCube(mainCamPos, Vector3.one * 0.5f);
        Gizmos.DrawLine(carTarget.position, mainCamPos);

        // Visualizar posiciones de retrovisores
        Gizmos.color = Color.red;
        Vector3 leftPos = carTarget.TransformPoint(leftMirrorOffset);
        Gizmos.DrawWireSphere(leftPos, 0.3f);
        Gizmos.DrawLine(carTarget.position, leftPos);

        Gizmos.color = Color.blue;
        Vector3 rightPos = carTarget.TransformPoint(rightMirrorOffset);
        Gizmos.DrawWireSphere(rightPos, 0.3f);
        Gizmos.DrawLine(carTarget.position, rightPos);

        // Mostrar dirección de vista de retrovisores
        Gizmos.color = Color.yellow;
        Vector3 lookBack = carTarget.TransformDirection(Vector3.back);
        Gizmos.DrawRay(leftPos, lookBack * 3f);
        Gizmos.DrawRay(rightPos, lookBack * 3f);

        // Etiquetas de texto (solo en Scene view)
        #if UNITY_EDITOR
        UnityEditor.Handles.Label(leftPos, "Retrovisor IZQ");
        UnityEditor.Handles.Label(rightPos, "Retrovisor DER");
        #endif
    }
}