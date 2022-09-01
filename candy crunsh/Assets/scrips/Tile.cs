using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tile : MonoBehaviour
{
    public int indiceX;
    public int indiceY;

    public Board board;
    public void Iniciar(int cambioX, int cambioY)
    {
        indiceX = cambioX;
        indiceY = cambioY;
    }

    public void OnMouseDown()
    {
        board.SetInitialTile(this);
    }
    public void OnMouseEnter()
    {
        board.SetEndTile(this);
    }
    public void OnMouseUp()
    {
        board.Relase();
    }
}
