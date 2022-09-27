using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tile : MonoBehaviour
{
    public int xindicex;
    public int yindicex;

    public Board m_board;
    public void Init(int x, int y,Board board)
    {
        m_board = board;

        xindicex = x;
        yindicex = y;
    }// guarda las coordenadas de los tile y el valor de la board
    public void OnMouseDown()
    {
        m_board.ClickedTile(this);
    }// cuando hace click una ficha llama la funcion ClickedTile
    public void OnMouseEnter()
    {
        m_board.DragToTile(this);
    }// cuando pasa el cursor sobre una ficha llama la funcion DragToTile
    public void OnMouseUp()
    {
        m_board.ReleaseTile();
    } // cuadno suelta el click llama la funcion ReleaseTile
}
