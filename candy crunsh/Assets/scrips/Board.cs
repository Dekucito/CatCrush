using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Board : MonoBehaviour
{
    public int alto;
    public int ancho;
    public Tile[,] board;
    public GameObject preFile;
    public Piezas[,] piezas;
    public Camera cam;
    public int borde;
    public GameObject[] goPrefabs = new GameObject[2];

    public Tile inicial;
    public Tile final;

    private void Start()
    {
        CrearBoard();
        OrganizarCamara();
        LlenarMatriz();
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
                go.transform.position = new Vector3(j, i, 0);
                go.transform.parent = transform;

                //cam.transform.position = new Vector3(i, j, -10)/2;
                Tile tile = go.GetComponent<Tile>();
                tile.board = this;
                board[i, j] = tile;
                tile.Iniciar(i,j);
            }
        }

    }

    void OrganizarCamara() 
    {
        cam.transform.position = new Vector3(((float)ancho / 2) - .5f, ((float)alto / 2) - .5f, -10);
        float anch = (((float)ancho/2)+borde)/((float)Screen.width/(float)Screen.height);
        float alt = ((float)alto / 2) + borde;

        if (anch > alt)
        {
            cam.orthographicSize = anch;
        }
        else
        {
            cam.orthographicSize = alt;
        }
    }

    GameObject PiezaAleatoria()
    {
        int numeroR = Random.Range(0, goPrefabs.Length);
        GameObject go = Instantiate(goPrefabs[numeroR]);
        return go;
}
    void PiezaPosicion( Piezas gp,int x, int y)
    {
        gp.transform.position = new Vector2(x, y);
        gp.Coordenada(x, y);
    }

    void LlenarMatriz()
    {

        for (int i = 0; i < ancho; i++)
        {
            for (int j = 0; j < alto; j++)
            {
                GameObject go = PiezaAleatoria();
                PiezaPosicion(go.GetComponent<Piezas>(),i,j);
            }
        }
        
    }

    public void SetInitialTile(Tile ini)
    {
        if (inicial == null)
        {
            inicial = ini;
        }
    }
    public void SetEndTile(Tile fin)
    {
        if (inicial != null)
        {
            final = fin;
        }
    }
    public void Relase()
    {
        inicial = null;
        final = null;
    }
}
