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
    }

    public void OnMouseDown()
    {
        m_board.ClickedTile(this);
    }
    public void OnMouseEnter()
    {
        m_board.DragToTile(this);
    }
    public void OnMouseUp()
    {
        m_board.ReleaseTile();
    }
}
