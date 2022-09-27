using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Piezas : MonoBehaviour
{
    public int xIndex;
    public int yIndex;

    public Board m_board;

    public bool m_isMoving = true;

    public InterpType interpolation;
    public MatchValue matchValue;

    public void SetCoord(int x, int y)
    {
        xIndex = x;
        yIndex = y;
    }// envia coordenadas a las piezas
   public void Init(Board board)
    {
        m_board = board;
    }// da un valor a la variable m_board
   public void Move(int x, int y, float moveTime)
   {
        if (m_isMoving == true)
        {
            StartCoroutine(MoveRoutine(x,y, moveTime));
        }
   }//inicia la corrutina de mover piezas, que le da el tipo de movimiento a las fichas
    IEnumerator MoveRoutine(int destX, int destY, float timeToMove)
    {
        Vector2 starPosition = transform.position;
        bool reacedDestination = false;
        float elapsedTime = 0f;
        m_isMoving = false;

        while (!reacedDestination)
        {
            if (Vector2.Distance(transform.position, new Vector2(destX,destY)) < 0.01f)
            {
                reacedDestination = true;
                if (m_board != null)
                {
                    m_board.PlaceGamePiece(this, destX, destY);
                }
                break;
            }

            elapsedTime += Time.deltaTime;
            float t = Mathf.Clamp(elapsedTime / timeToMove, 0f, 1f);

            switch(interpolation)
            {
                case InterpType.Linear:
                    break;
                case InterpType.EaseOut:
                    t = Mathf.Sin(t * Mathf.PI * .5f);
                    break;
                case InterpType.EseIn:
                    t = 1 - Mathf.Cos(t * Mathf.PI * .5f);
                    break;
                case InterpType.SmoothStep:
                    t = t * t * (3 - 2 * t);
                    break;
                case InterpType.SmoottherStep:
                    t = t * t * t * t * (t * (t * 6 - 15) + 10);
                    break;
            }
            transform.position = Vector2.Lerp(starPosition, new Vector2(destX, destY),t);
           yield return null;
        }
        m_isMoving = true;
    }// le da el tipo de movimiento a las fichas creando diferentes casos
    public enum InterpType
    {
        Linear,
        EaseOut,
        EseIn,
        SmoothStep,
        SmoottherStep,
    }// tipo de movimiento
    public enum MatchValue
    {
        monstruo1,
        monstruo2,
        monstruo3,
        monstruo4,
        monstruo5,
        monstruo6,
    }// tipo de ficha
}
