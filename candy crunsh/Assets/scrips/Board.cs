using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using TMPro;

public class Board : MonoBehaviour
{
    [Header("sonidos")]
    public AudioSource audi;
    public AudioClip elSonido;
    public AudioClip elSonido2;
    public bool Combo = false;

    [Header("score")]
    public static int scoreValue = 0;
    public TMP_Text score;
    private string scoreEnPantalla;
    public string scoreFinal;
    public int points = 10;

    [Header("valores de boards")]
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

    [Header("tiempo y condiciones")]
    [Range(0, .5f)]
    public float swapTime = .3f;
    public bool puedeMover = true;

    private void Start()
    {
        piezas = new Piezas[ancho, alto];
        CrearBoard();
        OrganizarCamara();
        LlenarMatriz();
    }
    public void Sonido()
    {
        if (Combo == true)
        {
            AudioSource.PlayClipAtPoint(elSonido2, gameObject.transform.position);
        }
        else
        {
            AudioSource.PlayClipAtPoint(elSonido, gameObject.transform.position);
        }
    }
    public void Score(int points)
    {
        scoreValue += points;
        scoreEnPantalla = "Score" + ":" + scoreValue;
        score.text = scoreEnPantalla;

        scoreFinal = score.text;
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
                tile.Iniciar(x, y);
            }
        }

    }
    void OrganizarCamara()
    {
        cam.transform.position = new Vector3(((float)ancho / 2) - .5f, ((float)alto / 2) - .5f, -10);
        float anch = (((float)ancho / 2) + borde) / ((float)Screen.width / (float)Screen.height);
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
    public void PiezaPosicion(Piezas gp, int x, int y)
    {
        gp.transform.position = new Vector2(x, y);
        gp.Coordenada(x, y);
        piezas[x, y] = gp;


    }
    void LlenarMatriz()
    {
        List<Piezas> addPieces = new List<Piezas>();

        for (int x = 0; x < ancho; x++)
        {
            for (int y = 0; y < alto; y++)
            {
                if (piezas[x, y] == null)
                {
                    Piezas gamePieces = LlenarMatrizAleatoriaEn(x, y);
                    addPieces.Add(gamePieces);
                }
            }
        }
        bool estaLleno = false;
        int interacciones = 0;
        int interaccionesmaximas = 100;

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
                coincidencias = coincidencias.Intersect(addPieces).ToList();
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
    Piezas LlenarMatrizAleatoriaEn(int x, int y)
    {
        GameObject go = PiezaAleatoria();
        PiezaPosicion(go.GetComponent<Piezas>(), x, y);

        return go.GetComponent<Piezas>();

    }
    private void ReemplazarConPiezaAleatoria(List<Piezas> coincidencias)
    {
        foreach (Piezas gamePiece in coincidencias)
        {
            ClearPieceAt(gamePiece.coordenadaX, gamePiece.coordenadaY);
            LlenarMatrizAleatoriaEn(gamePiece.coordenadaX, gamePiece.coordenadaY);
        }
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
        if (initialPiece != null && endPiece != null)
        {
            CambioPiezas(initialPiece, endPiece);
        }
        initialPiece = null;
        endPiece = null;
    }
    void CambioPiezas(Tile initi, Tile end)
    {
        StartCoroutine(SwitchCorrutine(initi, end));
    }
    IEnumerator SwitchCorrutine(Tile initi, Tile end)
    {
        if (puedeMover)
        {
            puedeMover = false;
            Piezas GpIn = piezas[initi.indiceX, initi.indiceY];
            Piezas GpEnd = piezas[end.indiceX, end.indiceY];

            if (GpIn != null & GpEnd != null)
            {
                GpIn.Moverpieza(end.indiceX, end.indiceY, swapTime);
                GpEnd.Moverpieza(initi.indiceX, initi.indiceY, swapTime);

                yield return new WaitForSeconds(swapTime);

                List<Piezas> ListaPiezaInicial = EncontrarCoincidenciaEn(initi.indiceX, initi.indiceY);
                List<Piezas> ListaPiezaFinal = EncontrarCoincidenciaEn(end.indiceX, end.indiceY);

                List<Piezas> combinada = ListaPiezaInicial.Union(ListaPiezaFinal).ToList();

                if (ListaPiezaInicial.Count == 0 && ListaPiezaFinal.Count == 0)
                {
                    GpIn.Moverpieza(initi.indiceX, initi.indiceY, swapTime);
                    GpEnd.Moverpieza(end.indiceX, end.indiceY, swapTime);
                    yield return new WaitForSeconds(swapTime);
                    puedeMover = true;
                }
                else
                {
                    if (combinada.Count >=4)
                    {
                        Sonido();
                        Combo = true;
                        ListaPiezaInicial = ListaPiezaInicial.Union(ListaPiezaFinal).ToList();
                        Score(points*2);
                        ClearAndRefillBoard(ListaPiezaInicial);
                        Combo = false;
                    }
                    if (combinada.Count == 3)
                    {
                        Sonido();
                        ListaPiezaInicial = ListaPiezaInicial.Union(ListaPiezaFinal).ToList();
                        Score(points);
                        ClearAndRefillBoard(ListaPiezaInicial);
                    }
                }
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

        for (int i = 1; i < valorMaximo - 1; i++)
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
    List<Piezas> BusquedaVertical(int startX, int startY, int cantidadMinima = 3)
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
        var listasCombinadas = EncontrarCoincidenciaEn(x, y);

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
    private List<Piezas> EncontrarCoincidenciaEn(List<Piezas> gamePieces, int minMatch = 3)
    {
        List<Piezas> matches = new List<Piezas>();

        foreach (Piezas gp in gamePieces)
        {
            matches = matches.Union(EncontrarCoincidenciaEn(gp.coordenadaX, gp.coordenadaY)).ToList();
        }
        return matches;
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
                ClearPieceAt(x, y);
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
            if (gp != null)
            {
                ClearPieceAt(gp.coordenadaX, gp.coordenadaY);
            }
        }
    }
    List<Piezas> CollapseColum(int column, float colapseTime = 0.1f)
    {
        List<Piezas> movingPieces = new List<Piezas>();

        for (int i = 0; i < alto - 1; i++)
        {
            if (piezas[column, i] == null)
            {
                for (int j = i + 1; j < alto; j++)
                {
                    if (piezas[column, j] != null)
                    {
                        piezas[column, j].Moverpieza(column, i, colapseTime * (j-i));
                        piezas[column, i] = piezas[column, j];
                        piezas[column, j].Coordenada(column, i);

                        if (!movingPieces.Contains(piezas[column, i]))
                        {
                            movingPieces.Add(piezas[column, i]);
                        }
                        piezas[column, j] = null;
                        break;
                    }
                }
            }
        }
        return movingPieces;
    }
    List<Piezas> CollapseColum(List<Piezas> PiezaDeJuego)
    {
        List<Piezas> movingPieces = new List<Piezas>();
        List<int> columnsToCollapse = GetColumns(PiezaDeJuego);

        foreach (int colums in columnsToCollapse)
        {
            movingPieces = movingPieces.Union(CollapseColum(colums)).ToList();
        }
        return movingPieces;
    }
    List<int> GetColumns(List<Piezas> gamePieces)
    {
        List<int> collumnsIndex = new List<int>();

        foreach (Piezas Gp in gamePieces)
        {
            if (!collumnsIndex.Contains(Gp.coordenadaX))
            {
                collumnsIndex.Add(Gp.coordenadaX);
            }
        }
        return collumnsIndex;
    }
    void ClearAndRefillBoard(List<Piezas> gamePieces)
    {
        StartCoroutine(ClearAndRefillBoardRoutine(gamePieces));
    }
    IEnumerator ClearAndRefillBoardRoutine(List<Piezas> gamepieces)
    {
        yield return StartCoroutine(ClearAndCollapseColumns(gamepieces));
        yield return null;
        yield return StartCoroutine(RefillRoutine());
        puedeMover = true;
    }
    IEnumerator ClearAndCollapseColumns(List<Piezas> gamepieces)
    {
        List<Piezas> movingPiece = new List<Piezas>();
        List<Piezas> matchs = new List<Piezas>();

        bool isFinished = false;

        while (!isFinished)
        {
            ClearPieceAt(gamepieces);
            yield return new WaitForSeconds(.25f);
            movingPiece = CollapseColum(gamepieces);
            while (!isColapse(gamepieces))
            {
                yield return new WaitForEndOfFrame();
            }
            matchs = EncontrarCoincidenciaEn(movingPiece);
            if (matchs.Count == 0)
            {
                isFinished = true;
                break;
            }
            else
            {
                Score(points);
                Sonido();
                yield return StartCoroutine(ClearAndCollapseColumns(matchs));
            }
        }
    }
    IEnumerator RefillRoutine()
    {
        LlenarMatriz();
        yield return null;
    }
    bool isColapse(List<Piezas> gamePieces)
    {
        foreach (Piezas gp in gamePieces)
        {
            if (gp != null)
            {
                if (gp.transform.position.y -(float)gp.coordenadaY > 0.001f)
                {
                    return false;
                }
            }
        }
        return true;
    }
}
