using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class UIManger : MonoBehaviour
{
    [SerializeField] private Animator _panelStart;
    [SerializeField] private GameObject _panelGameOver;

    

    private void Start()
    {
        
    }

    public void GameOver()
    {
        _panelGameOver.SetActive(true);
        //Time.timeScale = 0;
    }

    public void Replay()
    {
        SceneManager.LoadScene(0);
    }

    public void HidePanelStart()
    {
        _panelStart.enabled = true;
        
    }

}
