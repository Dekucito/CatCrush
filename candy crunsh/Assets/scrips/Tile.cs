using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tile : MonoBehaviour
{
    public int indiceX;
    public int indiceY;
    public void Iniciar(int cambioX, int cambioY)
    {
        indiceX = cambioX;
        indiceY = cambioY;
    }
}
