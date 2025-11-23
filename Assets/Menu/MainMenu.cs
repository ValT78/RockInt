using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenuManager : MonoBehaviour
{
    public Button btnDance;
    public Button btnCommands;
    public Button btnSettings;
    public Button btnClose;
    public Button btnQuit;
    public GameObject windowsCommand;
    public GameObject windowsSettings;
    public GameObject buttonPanel; // Le panel qui contient tous les boutons principaux

    public string gameSceneName = "GameScene";

    void Start()
    {
        // Cache les fenêtres au début
        if (windowsCommand != null)
            windowsCommand.SetActive(false);
        if (windowsSettings != null)
            windowsSettings.SetActive(false);
        if (btnClose != null)
            btnClose.gameObject.SetActive(false);
        if (buttonPanel != null)
            buttonPanel.SetActive(true);

        // Configure les événements des boutons
        btnDance.onClick.AddListener(StartGame);
        btnQuit.onClick.AddListener(ExitGame);
        btnCommands.onClick.AddListener(ShowCommandsWindow);
        btnSettings.onClick.AddListener(ShowSettingsWindow);
        btnClose.onClick.AddListener(HideAllWindows);
    }

    public void StartGame()
    {
        HideEverything();
        SceneManager.LoadScene(1);
    }

    public void ExitGame()
    {
        Application.Quit();
    }

    public void ShowCommandsWindow()
    {
        // Cache les boutons principaux
        if (buttonPanel != null)
            buttonPanel.SetActive(false);

        // Affiche la fenêtre commands et le bouton close
        if (windowsCommand != null)
            windowsCommand.SetActive(true);
        if (btnClose != null)
            btnClose.gameObject.SetActive(true);

        // S'assure que l'autre fenêtre est fermée
        if (windowsSettings != null)
            windowsSettings.SetActive(false);
    }

    public void ShowSettingsWindow()
    {
        // Cache les boutons principaux
        if (buttonPanel != null)
            buttonPanel.SetActive(false);

        // Affiche la fenêtre settings et le bouton close
        if (windowsSettings != null)
            windowsSettings.SetActive(true);
        if (btnClose != null)
            btnClose.gameObject.SetActive(true);

        // S'assure que l'autre fenêtre est fermée
        if (windowsCommand != null)
            windowsCommand.SetActive(false);
    }

    public void HideAllWindows()
    {
        // Cache toutes les fenêtres et le bouton close
        if (windowsCommand != null)
            windowsCommand.SetActive(false);
        if (windowsSettings != null)
            windowsSettings.SetActive(false);
        if (btnClose != null)
            btnClose.gameObject.SetActive(false);

        // Réaffiche les boutons principaux
        if (buttonPanel != null)
            buttonPanel.SetActive(true);
    }

    // Méthode utilitaire pour vérifier l'état
    public bool IsAnyWindowOpen()
    {
        return (windowsCommand != null && windowsCommand.activeSelf) ||
               (windowsSettings != null && windowsSettings.activeSelf);
    }

    public void HideEverything()
    {
        if (windowsCommand != null)
            windowsCommand.SetActive(false);
        if (windowsSettings != null)
            windowsSettings.SetActive(false);
        if (btnClose != null)
            btnClose.gameObject.SetActive(false);
        if (btnCommands != null)
            btnCommands.gameObject.SetActive(false);
        if (btnSettings != null)
            btnSettings.gameObject.SetActive(false);
        if (btnDance != null)
            btnDance.gameObject.SetActive(false);
        if (btnQuit != null)
            btnQuit.gameObject.SetActive(false);
    }
}