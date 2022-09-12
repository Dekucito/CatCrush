using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

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

    public Tile initialPiece;
    public Tile endPiece;

    [Range(0,.5f)]
    public float swapTime = .3f;

    private void Start()
    {
        piezas = new Piezas[ancho, alto];
        CrearBoard();
        OrganizarCamara();
        LlenarMatriz();
    }
    void CrearBoard()
    {
        board = new Tile[ancho, alto];

        for (int x = 0; x < ancho; x++)
        {
            for (int y = 0; y < alto; y++)
            {
                GameObject go = Instantiate(preFile);
                go.name = "Tile" + x + "," + y;
                go.transform.position = new Vector3(x, y, 0);
                go.transform.parent = transform;

                //cam.transform.position = new Vector3(i, j, -10)/2;
                Tile tile = go.GetComponent<Tile>();
                tile.board = this;
                board[x, y] = tile;
                tile.Iniciar(x,y);
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
        go.GetComponent<Piezas>().board = this;
        return go;
}
   public  void PiezaPosicion( Piezas gp,int x, int y)
    {
        gp.transform.position = new Vector2(x, y);
        gp.Coordenada(x, y);
        piezas[x, y] = gp;


    }
    void LlenarMatriz()
    {
        bool estaLleno = false;
        int interacciones = 0;
        int interaccionesmaximas = 100;

        for (int x = 0; x < ancho; x++)
        {
            for (int y = 0; y < alto; y++)
            {
                LlenarMatrizAleatoriaEn(x, y);
            }
        }
        while (!estaLleno)
        {
            List<Piezas> coincidencias = EncontrarTodasLasCoincidencias();
            if (coincidencias.Count == 0)
            {
                estaLleno = true;
                break;
            }
            else
            {
                ReemplazarConPiezaAleatoria(coincidencias);
            }
            if (interacciones > interaccionesmaximas)
            {
                estaLleno = true;
                Debug.LogWarning("se alcanzo el numero maximo de interacciones");
                break;
            }
            interacciones++;
        }

    }
    private void ReemplazarConPiezaAleatoria(List<Piezas> coincidencias)
    {
        foreach (Piezas gamePiece  in coincidencias)
        {
            ClearPieceAt(gamePiece.coordenadaX,gamePiece.coordenadaY);
            LlenarMatrizAleatoriaEn(gamePiece.coordenadaX,gamePiece.coordenadaY);
        }
    }

    void LlenarMatrizAleatoriaEn(int x, int y)
    {
        GameObject go = PiezaAleatoria();
        PiezaPosicion(go.GetComponent<Piezas>(), x, y);

    }
    public void SetInitialTile(Tile ini)
    {
        if (initialPiece == null)
        {
            initialPiece = ini;
        }
    }
    public void SetEndTile(Tile fin)
    {
        if (initialPiece != null && vecino(initialPiece, fin) == true)
        {
            endPiece = fin;
        }
    }
    public void Relase()
    {
        if (initialPiece!= null && endPiece != null)
        {
            CambioPiezas(initialPiece, endPiece);
        }
        initialPiece = null;
        endPiece = null;
    }
    void CambioPiezas(Tile initi, Tile end)
    {
        StartCoroutine(SwitchCorrutine(initi,end));
    }
    IEnumerator SwitchCorrutine(Tile initi, Tile end)
    {
        Piezas GpIn = piezas[initi.indiceX, initi.indiceY];
        Piezas GpEnd = piezas[end.indiceX, end.indiceY];

        if (GpIn != null & GpEnd != null)
        {
            GpIn.Moverpieza(end.indiceX, end.indiceY, swapTime);
            GpEnd.Moverpieza(initi.indiceX, initi.indiceY, swapTime);

            yield return new WaitForSeconds(swapTime);

            List<Piezas> ListaPiezaInicial = EncontrarCoincidenciaEn(initi.indiceX, initi.indiceY);
            List<Piezas> ListaPiezaFinal = EncontrarCoincidenciaEn(end.indiceX, end.indiceY);

            var ListasCombiadas = ListaPiezaInicial.Union(ListaPiezaFinal).ToList();

            if (ListasCombiadas.Count == 0)
            {
                GpIn.Moverpieza(initi.indiceX, initi.indiceY, swapTime);
                GpEnd.Moverpieza(end.indiceX, end.indiceY, swapTime);
            }
            else
            {
                ClearPieceAt(ListasCombiadas);
                /*ResaltarCoincidenciaEn(GpIn.coordenadaX, GpIn.coordenadaY);
                ResaltarCoincidenciaEn(GpEnd.coordenadaX, GpEnd.coordenadaY);*/
            }
        }
    }
    bool vecino(Tile ini, Tile fin)
    {

        if (Mathf.Abs(ini.indiceX - fin.indiceX) == 1 && ini.indiceY == fin.indiceY)
        {
            return true;
        }
        else
        {
            if (Mathf.Abs(ini.indiceY - fin.indiceY) == 1 && ini.indiceX == fin.indiceX)
            {
               return true;
            }
            else
            {
               return false;
            }
        }
    }
    bool EstaEnRango(int _x, int _y)
    {
        return (_x < ancho && _x >= 0 && _y < alto && _y >= 0);
    }
    List<Piezas> EncontrarCoincidencias(int starX, int starY, Vector2 direccionDeBusqueda, int cantidadMinima = 3)
    {
        // lista de coincidencias encontradas
        List<Piezas> coincidencias = new List<Piezas>();

        // referencia a gamepiece inicial
        Piezas piezaInicial = null;

        if (EstaEnRango(starX, starY))
        {
            piezaInicial = piezas[starX, starY];
        }
        if (piezaInicial != null)
        {
            coincidencias.Add(piezaInicial);
        }
        else
        {
            return null;
        }

        int siguienteX;
        int siguienteY;

        int valorMaximo = ancho > alto ? ancho : alto;

        for (int i = 1; i < valorMaximo -1; i++)
        {
            siguienteX = starX + (int)Mathf.Clamp(direccionDeBusqueda.x, -1, 1) * i;
            siguienteY = starY + (int)Mathf.Clamp(direccionDeBusqueda.y, -1, 1) * i;

            if (!EstaEnRango(siguienteX, siguienteY))
            {
                break;
            }
            Piezas siguientepieza = piezas[siguienteX, siguienteY];

            //comparar si piezas inicila y fianl si son del mismo tipo

            if (siguientepieza == null)
            {
                break;
            }
            else
            {
                if (piezaInicial.tipoFicha == siguientepieza.tipoFicha && !coincidencias.Contains(siguientepieza))
                {
                    coincidencias.Add(siguientepieza);
                }
                else
                {
                    break;
                }
            }
        }
        if (coincidencias.Count >= cantidadMinima)
        {
            return coincidencias;
        }
        return null;
    }
    List<Piezas> BusquedaVertical(int startX,int startY, int cantidadMinima=3)
    {
        List<Piezas> arriba = EncontrarCoincidencias(startX, startY, Vector2.up, 2);
        List<Piezas> abajo = EncontrarCoincidencias(startX, startY, Vector2.down, 2);

        if (arriba == null)
        {
            arriba = new List<Piezas>();
        }
        if (abajo == null)
        {
            abajo = new List<Piezas>();
        }
        var listasCombinadas = arriba.Union(abajo).ToList();

        return listasCombinadas.Count >= cantidadMinima ? listasCombinadas : null;
    }
    List<Piezas> BusquedaHorizontal(int startX, int startY, int cantidadMinima = 3)
    {
        List<Piezas> derecha = EncontrarCoincidencias(startX, startY, Vector2.right, 2);
        List<Piezas> izquierda = EncontrarCoincidencias(startX, startY, Vector2.left, 2);

        if (izquierda == null)
        {
            izquierda = new List<Piezas>();
        }
        if (derecha == null)
        {
            derecha = new List<Piezas>();
        }
        var listasCombinadas = derecha.Union(izquierda).ToList();

        return listasCombinadas.Count >= cantidadMinima ? listasCombinadas : null;
    }
    void ResaltarCoincidencias()
    {
        for (int x = 0; x < ancho; x++)
        {
            for (int y = 0; y < alto; y++)
            {
                ResaltarCoincidenciaEn(x, y);
            }
        }
    }
    private void ResaltarCoincidenciaEn(int x, int y)
    {
        var listasCombinadas = EncontrarCoincidenciaEn(x,y);

        if (listasCombinadas.Count > 0)
        {
            foreach (Piezas P in listasCombinadas)
            {
                ResaltarTileEn(P.coordenadaX, P.coordenadaY, P.GetComponent<SpriteRenderer>().color);
            }
        }
    }
    private List<Piezas> EncontrarCoincidenciaEn(int x, int y)
    {
        List<Piezas> horizontal = BusquedaHorizontal(x, y);
        List<Piezas> vertical = BusquedaVertical(x, y);

        if (horizontal == null)
        {
            horizontal = new List<Piezas>();
        }

        if (vertical == null)
        {
            vertical = new List<Piezas>();
        }

        var listasCombinadas = horizontal.Union(vertical).ToList();
        return listasCombinadas;
    }
    public List<Piezas> EncontrarTodasLasCoincidencias()
    {
        List<Piezas> todasLasCoincidencias = new List<Piezas>();
        for (int x = 0; x < ancho; x++)
        {
            for (int y = 0; y < alto; y++)
            {
                var coincidencias = EncontrarCoincidenciaEn(x, y);
                todasLasCoincidencias = todasLasCoincidencias.Union(coincidencias).ToList();
            }
        }
        return todasLasCoincidencias;
    }
    private void ResaltarTileEn(int x_, int y_, Color col_)
    {
        SpriteRenderer sr = board[x_, y_].GetComponent<SpriteRenderer>();
        sr.color = col_;
    }
    void ClearBoard()
    {
        for (int x = 0; x < ancho; x++)
        {
            for (int y = 0; y < alto; y++)
            {
                ClearPieceAt(x,y);
            }
        }
    }
    private void ClearPieceAt(int x, int y)
    {
        Piezas pieceToClear = piezas[x, y];

        if (pieceToClear != null)
        {
            piezas[x, y] = null;
            Destroy(pieceToClear.gameObject);
        }
    }
    private void ClearPieceAt(List<Piezas> gamePieces)
    {
        foreach (Piezas gp in gamePieces)
        {
            ClearPieceAt(gp.coordenadaX, gp.coordenadaY);
        }
    }
}
