using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneController : MonoBehaviour
{
    public static SceneController Instance;
    public int lastScene;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(this);
        }

        DontDestroyOnLoad(gameObject);
    }// inicializa variables antes del comienzo del juego y creaa una variable indestruible
    public void LoadLevel(int lvlIndex)
    {
        SceneManager.LoadScene(lvlIndex);
    }// conecta con board para cargar diferentes niveles
    public void Corrutine()
    {
        StartCoroutine("SceneCorrutine");
    } // inicia la corrutina SceneCorrutine
    IEnumerator SceneCorrutine()
    {
        yield return new WaitForSeconds(3);
        SceneManager.LoadScene(lastScene);
    }// carga la escena anterior (cuando aparece la escecna de game over)

}
