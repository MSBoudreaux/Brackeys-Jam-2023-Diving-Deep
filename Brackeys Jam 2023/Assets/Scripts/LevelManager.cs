using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelManager : MonoBehaviour
{

    public float[] levelQuotas;
    public PlayerStats myStats;
    public int mySceneIndex;

    private void Start()
    {
        myStats = FindObjectOfType<PlayerStats>();
        mySceneIndex = SceneManager.GetActiveScene().buildIndex;
        myStats.myQuota = levelQuotas[mySceneIndex];
    }


    public void LoadNewScene(int inScene)
    {
        SceneManager.LoadScene(inScene);
    }
    public void ReloadScene()
    {
        Debug.Log("Loading Scene: " + SceneManager.GetActiveScene().name);
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

}
