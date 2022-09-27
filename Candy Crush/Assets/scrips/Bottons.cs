using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bottons : MonoBehaviour
{
    public int indexScene;

    public void CallScene()
    {
        if (SceneController.Instance !=null)
        {
         SceneController.Instance.LoadLevel(indexScene);
        }
    }// botones para seeleccionar niveles en el menu 
}
