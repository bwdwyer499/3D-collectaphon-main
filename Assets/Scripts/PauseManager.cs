using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class PauseManager : MonoBehaviour
{
    Player_Controller inputActions;
    public static bool paused = false;
    public GameObject pauseMenu;
    public Button resumeButton;
    public Button primaryButton;
    private void Awake()
    {
        inputActions = new Player_Controller();
    }
    private void OnEnable()
    {
        inputActions.Enable();
    }

    private void OnDisable()
    {
        inputActions.Disable();
    }

    private void Start()
    {
        inputActions.Player.PauseGame.performed += _ => DeterminePause();
    }

    private void DeterminePause()
    {
        if (paused)
        {
            ResumeGame();
        }
        else
        {
            PauseGame();
        }
    } 
    public void PauseGame()
    {
        Time.timeScale = 0;
        resumeButton.Select();
        paused = true;
        pauseMenu.SetActive(true);
    }

    public void ResumeGame()
    {
        Time.timeScale = 1;
        primaryButton.Select();
        paused = false;
        pauseMenu.SetActive(false);
    }
}
