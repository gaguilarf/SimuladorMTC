using UnityEngine;

public class Marcador : MonoBehaviour
{
    public Material[] materials;
    public GameObject[] circulos;
    public GameObject[] rayos;
    public float rotationSpeed = 50f;

    private int currentMaterialIndex = 0;
    private bool isActive = false;
    private float timer = 0f;

    void Update()
    {
        if (isActive)
        {
            // Rotar los círculos
            foreach (GameObject circulo in circulos)
            {
                circulo.transform.Rotate(Vector3.up * rotationSpeed * Time.deltaTime);
            }

            // Cambiar materiales
            timer += Time.deltaTime;
            if (timer >= 1f)
            {
                timer = 0f;
                CambiarMaterial();
            }
        }
    }

    public void ActivarPortal()
    {
        gameObject.SetActive(true); // Activar objeto si estaba desactivado
        isActive = true;
        SetParticlesActive(true);
    }

    public void DesactivarPortal()
    {
        isActive = false;
        SetParticlesActive(false);
        gameObject.SetActive(false); // Apagar objeto si ya no se usará
    }

    private void CambiarMaterial()
    {
        if (materials.Length == 0) return;

        currentMaterialIndex = (currentMaterialIndex + 1) % materials.Length;
        foreach (GameObject circulo in circulos)
        {
            MeshRenderer renderer = circulo.GetComponent<MeshRenderer>();
            if (renderer != null)
            {
                renderer.material = materials[currentMaterialIndex];
            }
        }
    }

    private void SetParticlesActive(bool active)
    {
        foreach (GameObject rayo in rayos)
        {
            ParticleSystem ps = rayo.GetComponent<ParticleSystem>();
            if (ps != null)
            {
                if (active) ps.Play();
                else ps.Stop();
            }
        }
    }
}
