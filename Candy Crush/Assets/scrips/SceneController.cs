using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneController : MonoBehaviour
{
    public void ExitButton()
    {
        SceneManager.LoadScene(2);
        Time.timeScale = 1f;
    }
    public void PlayButton()
    {
        SceneManager.LoadScene(2);
    }
    public void Levels1()
    {
        SceneManager.LoadScene(3);
    }
    public void Levels2()
    {
        SceneManager.LoadScene(4);
    }
    public void Levels3()
    {
        SceneManager.LoadScene(5);
    }
    public void Levels4()
    {
        SceneManager.LoadScene(6);
    }
    public void Levels5()
    {
        SceneManager.LoadScene(7);
    }
}
