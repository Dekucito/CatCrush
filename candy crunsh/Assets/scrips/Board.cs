using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Board : MonoBehaviour
{
    public int alto;
    public int ancho;
    public Tile[,] board;
    public GameObject preFile;
    public Camera cam;

    private void Start()
    {
        CrearBoard();
        //cam.orthographicSize{}
    }

    void CrearBoard()
    {
        board = new Tile[alto, ancho];

        for (int i = 0; i < alto; i++)
        {
            for (int j = 0; j < ancho; j++)
            {
                GameObject go = Instantiate(preFile);
                go.name = "Tile" + i + "," + j;
                go.transform.position = new Vector3(i, j, 0);
                go.transform.parent = transform;

                Tile tile = go.GetComponent<Tile>();
                board[i, j] = tile;
                tile.Iniciar(i,j);
            }
        }

    }

}
