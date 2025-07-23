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
    public Vector3 leftMirrorOffset = new Vector3(-0.8f, 1.2f, 0.3f);
    public Vector3 rightMirrorOffset = new Vector3(0.8f, 1.2f, 0.3f);

    [Header("--- CONFIGURACIÓN DETECCIÓN DE CONOS ---")]
    public float conesDetectionDistance = 5.0f;
    public Transform[] coneTransforms;

    [Header("--- CONTROLES ---")]
    public KeyCode switchCameraKey = KeyCode.C;
    public KeyCode toggleMirrorsKey = KeyCode.M;

    [Header("--- DEBUG ---")]
    public bool showGizmos = false;
    public bool showDebugInfo = false;

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
            Debug.LogWarning("Left Mirror Camera no asignada");
        }

        if (rightMirrorCamera == null)
        {
            Debug.LogWarning("Right Mirror Camera no asignada");
        }

        if (coneTransforms == null || coneTransforms.Length == 0)
        {
            Debug.LogWarning("No hay conos asignados - La detección de conos no funcionará");
        }

        if (!hasErrors)
        {
            Debug.Log("✓ Camera Manager configurado correctamente!");
        }
    }

    void ConfigureCameras()
    {
        if (mainCamera != null) mainCamera.depth = 0;
        if (leftMirrorCamera != null) leftMirrorCamera.depth = -2;
        if (rightMirrorCamera != null) rightMirrorCamera.depth = -1;
    }

    void Update()
    {
        if (carTarget == null) return;

        UpdateCameras();
        CheckConesProximity();
        HandleInput();

        if (showDebugInfo)
        {
            ShowDebugInfo();
        }
    }

    void UpdateCameras()
    {
        UpdateThirdPersonCamera();

        if (mirrorsEnabled)
        {
            UpdateMirrorCamera(leftMirrorCamera, leftMirrorOffset, "Izquierdo");
            UpdateMirrorCamera(rightMirrorCamera, rightMirrorOffset, "Derecho");
        }
    }

    void UpdateThirdPersonCamera()
    {
        if (mainCamera == null) return;

        Vector3 desiredPosition = carTarget.position - (carTarget.forward * distance) + (Vector3.up * height);
        mainCamera.transform.position = Vector3.Lerp(mainCamera.transform.position, desiredPosition, positionDamping * Time.deltaTime);

        Vector3 lookAtTarget = carTarget.position + (Vector3.up * targetHeightOffset);
        Quaternion desiredRotation = Quaternion.LookRotation(lookAtTarget - mainCamera.transform.position, carTarget.up);
        mainCamera.transform.rotation = Quaternion.Slerp(mainCamera.transform.rotation, desiredRotation, rotationDamping * Time.deltaTime);
    }

    void UpdateMirrorCamera(Camera mirrorCam, Vector3 offset, string mirrorName)
    {
        if (mirrorCam == null) return;

        Vector3 worldPosition = carTarget.TransformPoint(offset);
        mirrorCam.transform.position = worldPosition;

        Vector3 lookDirection = carTarget.TransformDirection(Vector3.back);
        mirrorCam.transform.rotation = Quaternion.LookRotation(lookDirection, carTarget.up);
    }



    void CheckConesProximity()
    {
        if (coneTransforms == null || coneTransforms.Length == 0) return;

        foreach (Transform cone in coneTransforms)
        {
            if (cone != null)
            {
                float distance = Vector3.Distance(carTarget.position, cone.position);
                
                if (distance <= conesDetectionDistance)
                {
                    Debug.LogWarning($"⚠️ CONTACTO CON CONO DETECTADO - Esto ameritaría tu descalificación! Cono: {cone.name} - Distancia: {distance:F2}m");
                }
            }
        }
    }



    void HandleInput()
    {
        if (Input.GetKeyDown(switchCameraKey))
        {
            SwitchCamera();
        }

        if (Input.GetKeyDown(toggleMirrorsKey))
        {
            ToggleMirrors();
        }
    }

    void SwitchCamera()
    {
        currentMode = (CameraMode)(((int)currentMode + 1) % 3);
        SetActiveCamera(currentMode);
        Debug.Log($"Cámara cambiada a: {currentMode}");
    }

    void SetActiveCamera(CameraMode mode)
    {
        // Desactivar todas
        if (mainCamera != null) mainCamera.enabled = false;
        if (leftMirrorCamera != null) leftMirrorCamera.enabled = false;
        if (rightMirrorCamera != null) rightMirrorCamera.enabled = false;

        // Activar según modo
        switch (mode)
        {
            case CameraMode.ThirdPerson:
                if (mainCamera != null) mainCamera.enabled = true;
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

        Debug.Log($"Modo de cámara establecido a: {mode}");
    }

    void ToggleMirrors()
    {
        mirrorsEnabled = !mirrorsEnabled;

        if (currentMode == CameraMode.ThirdPerson)
        {
            if (leftMirrorCamera != null) leftMirrorCamera.enabled = mirrorsEnabled;
            if (rightMirrorCamera != null) rightMirrorCamera.enabled = mirrorsEnabled;
        }

        Debug.Log($"Retrovisores: {(mirrorsEnabled ? "Activados" : "Desactivados")}");
    }

    void ShowDebugInfo()
    {
        if (carTarget != null && coneTransforms != null && coneTransforms.Length > 0)
        {
            float minDistance = float.MaxValue;
            Transform closestCone = null;
            
            foreach (Transform cone in coneTransforms)
            {
                if (cone != null)
                {
                    float distance = Vector3.Distance(carTarget.position, cone.position);
                    if (distance < minDistance)
                    {
                        minDistance = distance;
                        closestCone = cone;
                    }
                }
            }
            
            string closestConeName = closestCone != null ? closestCone.name : "Ninguno";
            Debug.Log($"Distancia mínima a conos: {minDistance:F2}m | Cono más cercano: {closestConeName}");
        }
    }

    void OnDrawGizmos()
    {
        if (!showGizmos) return;

        if (carTarget != null)
        {
            Gizmos.color = Color.green;
            Vector3 mainCamPos = carTarget.position - (carTarget.forward * distance) + (Vector3.up * height);
            Gizmos.DrawWireCube(mainCamPos, Vector3.one * 0.5f);
            Gizmos.DrawLine(carTarget.position, mainCamPos);

            Gizmos.color = Color.red;
            Vector3 leftPos = carTarget.TransformPoint(leftMirrorOffset);
            Gizmos.DrawWireSphere(leftPos, 0.3f);

            Gizmos.color = Color.blue;
            Vector3 rightPos = carTarget.TransformPoint(rightMirrorOffset);
            Gizmos.DrawWireSphere(rightPos, 0.3f);
        }

        // Visualizar área de detección de conos
        if (coneTransforms != null && carTarget != null)
        {
            for (int i = 0; i < coneTransforms.Length; i++)
            {
                Transform cone = coneTransforms[i];
                if (cone != null)
                {
                    float distance = Vector3.Distance(carTarget.position, cone.position);
                    
                    // Color según proximidad
                    if (distance <= conesDetectionDistance)
                    {
                        Gizmos.color = Color.red; // Muy cerca - descalificación
                        Gizmos.DrawLine(carTarget.position, cone.position);
                    }
                    else
                    {
                        Gizmos.color = Color.yellow; // Distancia segura
                    }
                    
                    Gizmos.DrawWireSphere(cone.position, conesDetectionDistance);
                }
            }
        }

#if UNITY_EDITOR
        if (carTarget != null)
        {
            Vector3 leftPos = carTarget.TransformPoint(leftMirrorOffset);
            Vector3 rightPos = carTarget.TransformPoint(rightMirrorOffset);
            UnityEditor.Handles.Label(leftPos, "Retrovisor IZQ");
            UnityEditor.Handles.Label(rightPos, "Retrovisor DER");
        }
        
        if (coneTransforms != null && carTarget != null)
        {
            float minDistance = float.MaxValue;
            Transform closestCone = null;
            
            foreach (Transform cone in coneTransforms)
            {
                if (cone != null)
                {
                    float distance = Vector3.Distance(carTarget.position, cone.position);
                    if (distance < minDistance)
                    {
                        minDistance = distance;
                        closestCone = cone;
                    }
                }
            }
            
            for (int i = 0; i < coneTransforms.Length; i++)
            {
                if (coneTransforms[i] != null)
                {
                    float distance = Vector3.Distance(carTarget.position, coneTransforms[i].position);
                    string status = distance <= conesDetectionDistance ? " (¡DESCALIFICACIÓN!)" : "";
                    string label = coneTransforms[i] == closestCone ? 
                        $"Cono {i+1} (MÁS CERCANO){status}" : $"Cono {i+1}{status}";
                    UnityEditor.Handles.Label(coneTransforms[i].position, label);
                }
            }
        }
#endif
    }
}