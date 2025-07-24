using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    [Header("Paneles de UI")]
    public GameObject mainMenu;         // Panel principal con botones Jugar, Seleccionar Personaje, Opciones, Salir
    public GameObject optionsMenu;      // Panel de opciones
    public GameObject characterMenu;    // Panel de selección de personaje (Auto, Moto)

    private const string VEHICLE_PREF = "SelectedVehicle";

    void Start()
    {
        // Si no hay vehículo seleccionado, por defecto es "Auto"
        if (!PlayerPrefs.HasKey(VEHICLE_PREF))
            PlayerPrefs.SetString(VEHICLE_PREF, "Auto");
    }

    // ---------------- PANEL DE OPCIONES ----------------

    public void OpenOptionsPanel()
    {
        mainMenu.SetActive(false);
        optionsMenu.SetActive(true);
    }

    public void CloseOptionsPanel()
    {
        optionsMenu.SetActive(false);
        mainMenu.SetActive(true);
    }

    // ---------------- SELECCION DE PERSONAJE ----------------

    public void OpenCharacterPanel()
    {
        mainMenu.SetActive(false);
        characterMenu.SetActive(true);
    }

    public void OpenMainMenuPanel()
    {
        characterMenu.SetActive(false);
        optionsMenu.SetActive(false);
        mainMenu.SetActive(true);
    }

    public void SelectAuto()
    {
        PlayerPrefs.SetString(VEHICLE_PREF, "Auto");
        PlayerPrefs.Save();
        OpenMainMenuPanel();
    }

    public void SelectMoto()
    {
        PlayerPrefs.SetString(VEHICLE_PREF, "Moto");
        PlayerPrefs.Save();
        OpenMainMenuPanel();
    }

    // ---------------- JUGAR Y SALIR ----------------

    public void PlayGame()
    {
        string vehicle = PlayerPrefs.GetString(VEHICLE_PREF, "Auto");

        if (vehicle == "Moto")
            SceneManager.LoadScene("Moto_Escena1");
        else
            SceneManager.LoadScene("SampleScene");
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}
