using UnityEngine;

public class CheckpointTrigger : MonoBehaviour

{
    [Header("Progresión del Portal")]
    public int numeroDelPortal = 1; // Asignar manualmente en el Inspector (1, 2, 3...)
    public int totalPortales = 6;
    [Header("Configuración del Portal de Luz")]
    public float alturaColumna = 8f;
    public float radioAnillo = 2f;
    public float grosorAnillo = 0.2f;
    public float intensidadLuz = 3f;

    [Header("Colores")]
    public Color colorColumna = new Color(1f, 0.2f, 0.8f, 0.9f);
    public Color colorAnillo = new Color(1f, 0f, 0.6f, 0.9f);
    public Color colorLuz = new Color(1f, 0.1f, 0.7f, 0.9f);

    [Header("Efectos")]
    public float velocidadPulso = 1.5f;
    public float intensidadPulso = 0.4f;
    public bool rotarAnillo = true;
    public float velocidadRotacionAnillo = 30f;

    [Header("Partículas")]
    public bool activarParticulas = true;
    public int cantidadParticulas = 100;

    private ParticleSystem sistemaParticulas;
    private Light luzPrincipal, luzAnillo;
    private GameObject columnaLuz, anilloBase;
    private Material materialColumna, materialAnillo;
    private LineRenderer lineRenderer;
    private float tiempoInicio;

    void Start()
    {
        tiempoInicio = Time.time;

        CrearAnilloBase();
        CrearEfectoAnilloGlow();
        CrearSistemaIluminacion();
        ConfigurarSistemaParticulas();

        Debug.Log("Portal de luz creado!");
    }

    void Update()
    {
        AplicarEfectosPulsantes();
        if (rotarAnillo && anilloBase != null)
            anilloBase.transform.Rotate(0, velocidadRotacionAnillo * Time.deltaTime, 0);
    }

    void CrearAnilloBase()
    {
        anilloBase = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        anilloBase.name = "AnilloBase";
        anilloBase.transform.SetParent(transform);
        anilloBase.transform.localPosition = Vector3.up * 0.1f;
        anilloBase.transform.localScale = new Vector3(radioAnillo, grosorAnillo, radioAnillo);

        DestroyImmediate(anilloBase.GetComponent<Collider>());
        materialAnillo = CrearMaterial(colorAnillo, colorAnillo * 2f, 1f);
        anilloBase.GetComponent<Renderer>().material = materialAnillo;
    }

    void CrearEfectoAnilloGlow()
    {
        GameObject anilloGlow = new GameObject("AnilloGlow");
        anilloGlow.transform.SetParent(transform);
        anilloGlow.transform.localPosition = Vector3.up * 0.05f;

        lineRenderer = anilloGlow.AddComponent<LineRenderer>();
        lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
        lineRenderer.startColor = lineRenderer.endColor = colorAnillo;
        lineRenderer.startWidth = lineRenderer.endWidth = 0.3f;
        lineRenderer.positionCount = 50;
        lineRenderer.useWorldSpace = false;
        lineRenderer.loop = true;

        Vector3[] positions = new Vector3[50];
        for (int i = 0; i < 50; i++)
        {
            float angle = i * Mathf.PI * 2f / 50f;
            positions[i] = new Vector3(Mathf.Cos(angle) * radioAnillo, 0, Mathf.Sin(angle) * radioAnillo);
        }
        lineRenderer.SetPositions(positions);
    }

    void CrearSistemaIluminacion()
    {
        luzPrincipal = CrearLuz("LuzPrincipal", Vector3.up * alturaColumna, colorLuz, intensidadLuz, alturaColumna * 1.5f, true);
        luzAnillo = CrearLuz("LuzAnillo", Vector3.up * 0.2f, colorAnillo, intensidadLuz * 0.7f, radioAnillo * 3f, false);
    }

    Light CrearLuz(string name, Vector3 position, Color color, float intensity, float range, bool withShadows)
    {
        GameObject obj = new GameObject(name);
        obj.transform.SetParent(transform);
        obj.transform.localPosition = position;

        Light light = obj.AddComponent<Light>();
        light.type = LightType.Point;
        light.color = color;
        light.intensity = intensity;
        light.range = range;
        light.shadows = withShadows ? LightShadows.Soft : LightShadows.None;

        return light;
    }

    Material CrearMaterial(Color baseColor, Color emissionColor, float glossiness)
    {
        Material mat = new Material(Shader.Find("Standard"));
        mat.SetFloat("_Mode", 3);
        mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
        mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
        mat.SetInt("_ZWrite", 0);
        mat.EnableKeyword("_ALPHABLEND_ON");
        mat.EnableKeyword("_EMISSION");
        mat.renderQueue = 3000;
        mat.color = baseColor;
        mat.SetColor("_EmissionColor", emissionColor);
        mat.SetFloat("_Metallic", 0f);
        mat.SetFloat("_Glossiness", glossiness);
        return mat;
    }

    void ConfigurarSistemaParticulas()
    {
        if (!activarParticulas) return;

        sistemaParticulas = GetComponent<ParticleSystem>() ?? gameObject.AddComponent<ParticleSystem>();

        var main = sistemaParticulas.main;
        main.startLifetime = 4f;
        main.startSpeed = 2f;
        main.startSize = 0.1f;
        main.startColor = colorLuz;
        main.maxParticles = cantidadParticulas;
        main.simulationSpace = ParticleSystemSimulationSpace.World;

        var emission = sistemaParticulas.emission;
        emission.rateOverTime = 25f;

        var shape = sistemaParticulas.shape;
        shape.enabled = true;
        shape.shapeType = ParticleSystemShapeType.Circle;
        shape.radius = radioAnillo * 0.9f;
        shape.position = Vector3.up * 0.1f;

        var velocity = sistemaParticulas.velocityOverLifetime;
        velocity.enabled = true;
        velocity.space = ParticleSystemSimulationSpace.Local;
        velocity.y = new ParticleSystem.MinMaxCurve(3f, 5f);
        velocity.radial = new ParticleSystem.MinMaxCurve(-0.5f, 0.5f);

        var colorOverLifetime = sistemaParticulas.colorOverLifetime;
        colorOverLifetime.enabled = true;
        colorOverLifetime.color = new Gradient
        {
            colorKeys = new GradientColorKey[] {
                new GradientColorKey(colorAnillo, 0f),
                new GradientColorKey(colorLuz, 0.5f),
                new GradientColorKey(colorColumna, 1f)
            },
            alphaKeys = new GradientAlphaKey[] {
                new GradientAlphaKey(0.8f, 0f),
                new GradientAlphaKey(1f, 0.3f),
                new GradientAlphaKey(0f, 1f)
            }
        };

        var sizeOverLifetime = sistemaParticulas.sizeOverLifetime;
        sizeOverLifetime.enabled = true;
        sizeOverLifetime.size = new ParticleSystem.MinMaxCurve(1f, new AnimationCurve(
            new Keyframe(0f, 0.5f), new Keyframe(0.3f, 1f), new Keyframe(1f, 0.2f)
        ));

        var renderer = sistemaParticulas.GetComponent<ParticleSystemRenderer>();
        var particleMaterial = new Material(Shader.Find("Sprites/Default"))
        {
            color = colorLuz
        };
        particleMaterial.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
        particleMaterial.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.One);
        renderer.material = particleMaterial;
    }

    void AplicarEfectosPulsantes()
    {
        float pulso = Mathf.Sin((Time.time - tiempoInicio) * velocidadPulso) * intensidadPulso + 1f;

        if (luzPrincipal) luzPrincipal.intensity = intensidadLuz * pulso;
        if (luzAnillo) luzAnillo.intensity = (intensidadLuz * 0.7f) * pulso;

        if (materialColumna) materialColumna.SetColor("_EmissionColor", colorLuz * 1.5f * pulso);
        if (materialAnillo) materialAnillo.SetColor("_EmissionColor", colorAnillo * 2f * pulso);

        if (lineRenderer)
        {
            Color colorPulso = colorAnillo; colorPulso.a = pulso * 0.8f;
            lineRenderer.startColor = lineRenderer.endColor = colorPulso;
            lineRenderer.startWidth = lineRenderer.endWidth = 0.3f * pulso;
        }

        if (sistemaParticulas != null)
        {
            var emission = sistemaParticulas.emission;
            emission.rateOverTime = 25f * pulso;
        }
    }

    public void ActivarPortal()
    {
        gameObject.SetActive(true);
        sistemaParticulas?.Play();
    }

    public void DesactivarPortal()
    {
        sistemaParticulas?.Stop();
        gameObject.SetActive(false);
    }

    public void CambiarTamaño(float nuevoRadio, float nuevaAltura)
    {
        radioAnillo = nuevoRadio;
        alturaColumna = nuevaAltura;

        if (columnaLuz)
        {
            columnaLuz.transform.localScale = new Vector3(radioAnillo * 0.6f, alturaColumna / 2f, radioAnillo * 0.6f);
            columnaLuz.transform.localPosition = Vector3.up * (alturaColumna / 2f);
        }

        if (anilloBase) anilloBase.transform.localScale = new Vector3(radioAnillo, grosorAnillo, radioAnillo);
        if (luzPrincipal) { luzPrincipal.transform.localPosition = Vector3.up * alturaColumna; luzPrincipal.range = alturaColumna * 1.5f; }
        if (luzAnillo) luzAnillo.range = radioAnillo * 3f;

        if (lineRenderer)
        {
            Vector3[] positions = new Vector3[lineRenderer.positionCount];
            for (int i = 0; i < positions.Length; i++)
            {
                float angle = i * Mathf.PI * 2f / positions.Length;
                positions[i] = new Vector3(Mathf.Cos(angle) * radioAnillo, 0, Mathf.Sin(angle) * radioAnillo);
            }
            lineRenderer.SetPositions(positions);
        }

        if (sistemaParticulas != null)
        {
            var shape = sistemaParticulas.shape;
            shape.radius = radioAnillo * 0.9f;
        }
    }

    public void CambiarColores(Color nuevoColorColumna, Color nuevoColorAnillo)
    {
        colorColumna = nuevoColorColumna;
        colorAnillo = nuevoColorAnillo;
        colorLuz = Color.Lerp(colorColumna, colorAnillo, 0.5f);

        if (materialColumna)
        {
            materialColumna.color = colorColumna;
            materialColumna.SetColor("_EmissionColor", colorLuz * 1.5f);
        }

        if (materialAnillo)
        {
            materialAnillo.color = colorAnillo;
            materialAnillo.SetColor("_EmissionColor", colorAnillo * 2f);
        }

        if (luzPrincipal) luzPrincipal.color = colorLuz;
        if (luzAnillo) luzAnillo.color = colorAnillo;
        if (lineRenderer)
        {
            lineRenderer.startColor = lineRenderer.endColor = colorAnillo;
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log("¡Jugador ha entrado en el portal de luz " + numeroDelPortal + "!");

            DesactivarPortal(); // Apaga este

            // Activar el siguiente si existe
            int siguiente = numeroDelPortal + 1;
            if (siguiente <= totalPortales)
            {
                string siguienteTag = "SistemaParticulas" + siguiente;
                GameObject siguientePortal = GameObject.FindGameObjectWithTag(siguienteTag);
                if (siguientePortal != null)
                {
                    CheckpointTrigger trigger = siguientePortal.GetComponent<CheckpointTrigger>();
                    if (trigger != null)
                    {
                        trigger.ActivarPortal();
                        Debug.Log("Activando portal: " + siguienteTag);
                    }
                }
            }

            // Opcional: evitar que vuelva a activarse
            Collider col = GetComponent<Collider>();
            if (col != null) col.enabled = false;
        }
    }

    void OnDestroy()
    {
        if (materialColumna) DestroyImmediate(materialColumna);
        if (materialAnillo) DestroyImmediate(materialAnillo);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(colorAnillo.r, colorAnillo.g, colorAnillo.b, 0.3f);
        Vector3 center = transform.position;

        Vector3 prevPos = center + new Vector3(radioAnillo, 0, 0);
        for (int i = 1; i <= 30; i++)
        {
            float angle = i * Mathf.PI * 2f / 30f;
            Vector3 newPos = center + new Vector3(Mathf.Cos(angle) * radioAnillo, 0, Mathf.Sin(angle) * radioAnillo);
            Gizmos.DrawLine(prevPos, newPos);
            prevPos = newPos;
        }

        Gizmos.color = new Color(colorLuz.r, colorLuz.g, colorLuz.b, 0.1f);
        Gizmos.DrawCube(center + Vector3.up * (alturaColumna / 2f), new Vector3(radioAnillo * 0.6f, alturaColumna, radioAnillo * 0.6f));
    }
}
