using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class FixedCarController : MonoBehaviour
{
    [Header("Movimiento")]
    public float accelerationForce = 1000f;
    public float maxSpeed = 20f;
    public float reverseMaxSpeed = 10f;
    public float brakeForce = 1500f;

    [Header("Dirección")]
    public float turnStrength = 1500f;
    public float steeringResponseFactor = 5f;

    [Header("Configuración del Rigidbody")]
    public float centerOfMassOffset = -0.5f;
    public float downForce = 300f;
    public float groundCheckDistance = 1.5f;

    [Header("Debug")]
    public bool showDebugInfo = false;

    private Rigidbody rb;
    private float verticalInput;
    private float horizontalInput;
    private bool isBraking;
    private float currentSteeringAngle = 0f;
    private bool isGrounded = false;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        if (rb == null)
        {
            Debug.LogError("Rigidbody no encontrado en el objeto. Añade un Rigidbody.", this);
            enabled = false;
            return;
        }

        // CONFIGURACIÓN FORZADA DEL RIGIDBODY PARA EVITAR QUE TRASPASE EL SUELO
        rb.useGravity = true;           // OBLIGATORIO: Activar gravedad
        rb.mass = 15f;                  // OBLIGATORIO: Masa suficiente para no traspasar
        rb.linearDamping = 0.3f;        // Resistencia al movimiento
        rb.angularDamping = 3f;         // Resistencia a la rotación
        rb.centerOfMass = new Vector3(0, centerOfMassOffset, 0);
        
        Debug.Log("✓ Rigidbody configurado: Mass=" + rb.mass + ", UseGravity=" + rb.useGravity);
    }

    void Update()
    {
        GetInput();

        if (showDebugInfo)
        {
            ShowDebugInfo();
        }
    }

    void GetInput()
    {
        verticalInput = Input.GetAxis("Vertical");
        horizontalInput = Input.GetAxis("Horizontal");
        isBraking = Input.GetKey(KeyCode.Space);

        if (showDebugInfo && Mathf.Abs(horizontalInput) > 0.1f)
        {
            Debug.Log($"Horizontal Input: {horizontalInput}");
        }
    }

    void FixedUpdate()
    {
        if (rb == null) return;

        CheckGrounded();
        ApplyDownForce();
        ApplySteering();
        ApplyDriveForce();
        ApplyBrakingAndDrag();
        LimitSpeed();
    }

    void ApplySteering()
    {
        float currentSpeed = rb.linearVelocity.magnitude;

        // Solo aplicar dirección si está en el suelo
        if (Mathf.Abs(horizontalInput) > 0.05f && currentSpeed > 0.1f && isGrounded)
        {
            float targetSteeringAngle = horizontalInput * turnStrength;
            currentSteeringAngle = Mathf.Lerp(currentSteeringAngle, targetSteeringAngle,
                steeringResponseFactor * Time.fixedDeltaTime);

            float speedFactor = CalculateSpeedFactor(currentSpeed);
            float finalTorque = currentSteeringAngle * speedFactor;
            rb.AddTorque(Vector3.up * finalTorque, ForceMode.Force);

            if (showDebugInfo)
            {
                Debug.Log($"Steering - Input: {horizontalInput:F2}, Angle: {currentSteeringAngle:F1}, Speed Factor: {speedFactor:F2}, Torque: {finalTorque:F1}");
            }
        }
        else
        {
            currentSteeringAngle = Mathf.Lerp(currentSteeringAngle, 0f,
                steeringResponseFactor * Time.fixedDeltaTime * 2f);
        }
    }

    float CalculateSpeedFactor(float currentSpeed)
    {
        float normalizedSpeed = currentSpeed / maxSpeed;
        return Mathf.Lerp(1f, 0.3f, normalizedSpeed);
    }

    void ApplyDriveForce()
    {
        // Solo aplicar fuerza de movimiento si está en el suelo
        if (Mathf.Abs(verticalInput) > 0.05f && isGrounded)
        {
            Vector3 driveForce = transform.forward * verticalInput * accelerationForce;

            if (isBraking && verticalInput > 0)
            {
                driveForce = Vector3.zero;
            }

            rb.AddForce(driveForce, ForceMode.Force);
        }
    }

    void ApplyBrakingAndDrag()
    {
        if (isBraking)
        {
            if (rb.linearVelocity.magnitude > 0.1f)
            {
                Vector3 brakeVector = -rb.linearVelocity.normalized * brakeForce;
                rb.AddForce(brakeVector, ForceMode.Acceleration);
            }
        }
        else
        {
            if (Mathf.Abs(verticalInput) < 0.05f)
            {
                rb.AddForce(-rb.linearVelocity * 50f, ForceMode.Force);
            }
        }
    }

    void CheckGrounded()
    {
        // Raycast hacia abajo para verificar si está en el suelo
        RaycastHit hit;
        isGrounded = Physics.Raycast(transform.position, Vector3.down, out hit, groundCheckDistance);
        
        if (showDebugInfo && !isGrounded)
        {
            Debug.Log("¡Carro en el aire!");
        }
    }

    void ApplyDownForce()
    {
        // Aplicar fuerza hacia abajo para mantener el carro pegado al suelo
        if (isGrounded)
        {
            rb.AddForce(Vector3.down * downForce, ForceMode.Force);
        }
    }

    void LimitSpeed()
    {
        float currentMaxSpeed;

        if (verticalInput < -0.05f || Vector3.Dot(rb.linearVelocity, transform.forward) < 0)
        {
            currentMaxSpeed = reverseMaxSpeed;
        }
        else
        {
            currentMaxSpeed = maxSpeed;
        }

        if (rb.linearVelocity.magnitude > currentMaxSpeed)
        {
            rb.linearVelocity = rb.linearVelocity.normalized * currentMaxSpeed;
        }
    }

    void ShowDebugInfo()
    {
        if (rb != null)
        {
            // Debug.Log($"Speed: {rb.linearVelocity.magnitude:F1} | Angular Vel: {rb.angularVelocity.y:F2} | Steering Angle: {currentSteeringAngle:F1}");
        }
    }

    void OnDrawGizmosSelected()
    {
        if (!Application.isPlaying || rb == null) return;

        Gizmos.color = Color.blue;
        Gizmos.DrawLine(transform.position, transform.position + transform.forward * 3f);

        Gizmos.color = Color.red;
        Gizmos.DrawLine(transform.position, transform.position + rb.linearVelocity.normalized * 2f);

        if (Mathf.Abs(currentSteeringAngle) > 0.1f)
        {
            Gizmos.color = Color.green;
            Vector3 turnDirection = Quaternion.Euler(0, currentSteeringAngle * 0.1f, 0) * transform.forward;
            Gizmos.DrawLine(transform.position, transform.position + turnDirection * 2f);
        }

        Gizmos.color = Color.yellow;
        Gizmos.DrawSphere(transform.position + transform.TransformDirection(rb.centerOfMass), 0.1f);
    }

    void OnValidate()
    {
        if (turnStrength < 500f)
        {
            Debug.LogWarning("Turn Strength muy bajo. Prueba valores entre 1000-2000 para mejor respuesta.");
        }

        if (steeringResponseFactor < 1f)
        {
            Debug.LogWarning("Steering Response Factor muy bajo. Prueba valores entre 3-10 para mejor respuesta.");
        }
    }
}