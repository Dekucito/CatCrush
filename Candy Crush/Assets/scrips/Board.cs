using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using TMPro;

public class Board : MonoBehaviour
{

    [Header("sonidos")]
    public AudioSource audi;
    public AudioClip soundOne;
    public AudioClip soundTwo;
    public bool smash = false;

    [Header("score")]
    public static int scoreValue = 0;
    public TMP_Text score;
    private string scoreEnPantalla;
    public string scoreFinal;
    public int points = 10;

    public EnemyController[] enemy;

    [Header("Movimiento")]
    public int move = 20;
    public TMP_Text movements;

    [Header("Timepo")]
    public float initialTime;
    [Range(-10.0f, 10.0f)] public float timeScale =1;
    public TMP_Text time;
    private float timeFrameScale = 0f;
    private float timeSeconds = 0f;
    private float initialScaleTime; 

    [Header("valores de boards")]
    public int height;
    public int width;

    public int borderZise;

    public GameObject tilePrefab;
    public GameObject[] gamePiecesPrefabas;

    [Range(0, .5f)]
    public float swapTime = .3f;

    public Color color;

    Tile[,] m_allTiles;
    Piezas[,] m_allGamePieces;

    [SerializeField] Tile m_clickedTile;
    [SerializeField] Tile m_targetTile;

   public bool m_playerInputEnabled = true;

    public Camera cam;

    Transform tileParent;
    Transform gamePieceParent;

    private void Start()
    {
        SetParents();
        Movements();

        m_allTiles = new Tile[width, height];
        m_allGamePieces = new Piezas[width, height];

        InitialTime();

        SetupTiles();
        SetupCamera();
        FillBoard(50, .5f);
    }
    private void Update()
    {
        GameOver();
        TimeStart();
        Movements();
    }
    void SetupCamera()
    {
        cam.transform.position = new Vector3(((float)width / 2) - .5f, ((float)height / 2) - .5f, -10);
        float anch = (((float)width / 2) + borderZise) / ((float)Screen.width / (float)Screen.height);
        float alt = ((float)height / 2) + borderZise;

        if (anch > alt)
        {
            cam.orthographicSize = anch;
        }
        else
        {
            cam.orthographicSize = alt;
        }
    }
    void SetupTiles()
    {
        m_allTiles = new Tile[width, height];

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                GameObject tile = Instantiate(tilePrefab, new Vector2(x, y), Quaternion.identity);
                tile.name = $"Tile({x},{y})";
                if (tileParent != null)
                {
                    tile.transform.parent = tileParent;
                }
                m_allTiles[x, y] = tile.GetComponent<Tile>();
                m_allTiles[x, y].Init(x, y, this);
            }
        }
    }
    void FillBoard(int falseOffSett = 0, float moveTime = .1f)
    {
        List<Piezas> addedPieces = new List<Piezas>();

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (m_allGamePieces[x, y] == null)
                {
                    if (falseOffSett == 0)
                    {
                        Piezas piece = FillRandomAt(x, y);
                        addedPieces.Add(piece);
                    }
                    else
                    {
                        Piezas piece = FillRandomAt(x, y, falseOffSett, moveTime);
                        addedPieces.Add(piece);
                    }
                }
            }
        }

        int maxinterations = 100;
        int interations = 0;

        bool isFilled = false;

        while (!isFilled)
        {
            List<Piezas> matches = FindAllMatches();
            if (matches.Count == 0)
            {
                isFilled = true;
                break;
            }
            else
            {
                matches = matches.Intersect(addedPieces).ToList();
                ReplaceWithRandom(matches);
            }
            if (falseOffSett == 0)
            {
                ReplaceWithRandom(matches);
            }
            else
            {
                ReplaceWithRandom(matches,falseOffSett,moveTime);

            }
            if (interations > maxinterations)
            {
                isFilled = true;
                Debug.LogWarning("se alcanzo el numero maximo de interacciones");
                break;
            }
            interations++;
        }
    }
    public void ClickedTile(Tile tile)
    {
        if (m_clickedTile == null)
        {
            m_clickedTile = tile;
        }
    }
    public void DragToTile(Tile tile)
    {
        if (m_clickedTile != null && IsNexTo(tile,m_clickedTile))
        {
            m_targetTile = tile;
        }
    }
    public void ReleaseTile()
    {
        if (m_clickedTile != null && m_targetTile != null)
        {
            SwitchTiles(m_clickedTile, m_targetTile);
        }
        m_clickedTile = null;
        m_targetTile = null;
    }
    void SwitchTiles(Tile m_clickedTile, Tile m_targetTile)
    {
        StartCoroutine(SwitchTilesRoutine(m_clickedTile, m_targetTile));
    }
    IEnumerator SwitchTilesRoutine(Tile clickedTile, Tile targetTile)
    {
        if (m_playerInputEnabled)
        {
            m_playerInputEnabled = false;
            Piezas clickedPiece = m_allGamePieces[clickedTile.xindicex, clickedTile.yindicex];
            Piezas targePiece = m_allGamePieces[targetTile.xindicex, targetTile.yindicex];

            if (clickedPiece != null & targePiece != null)
            {
                clickedPiece.Move(targetTile.xindicex, targetTile.yindicex, swapTime);
                targePiece.Move(clickedTile.xindicex, clickedTile.yindicex, swapTime);

                yield return new WaitForSeconds(swapTime);

                List<Piezas> clickedPiecesMatches = FindMatchesAt(clickedTile.xindicex, clickedTile.yindicex);
                List<Piezas> targetPieceMatches = FindMatchesAt(targetTile.xindicex, targetTile.yindicex);

                List<Piezas> combinada = clickedPiecesMatches.Union(targetPieceMatches).ToList();

                if (clickedPiecesMatches.Count == 0 && targetPieceMatches.Count == 0)
                {
                    clickedPiece.Move(clickedTile.xindicex, clickedTile.yindicex, swapTime);
                    targePiece.Move(targetTile.xindicex, targetTile.yindicex, swapTime);
                    yield return new WaitForSeconds(swapTime);
                    move = move - 1;
                    m_playerInputEnabled = true;
                }
                else
                {
                    move = move - 1;
                    yield return new WaitForSeconds(swapTime);
                    ClearAndRefillBoard(clickedPiecesMatches = clickedPiecesMatches.Union(targetPieceMatches).ToList());

                    if (clickedPiecesMatches.Count ==3)
                    {
                        sound();
                        Score(points);
                    }if (clickedPiecesMatches.Count >=4)
                    {
                        Score(points + 5);
                        smash = true;
                        sound();                     
                        smash = false;
                    }
                }
            }
        }
    }
    void ClearAndRefillBoard(List<Piezas> gamePieces)
    {
        StartCoroutine(ClearAndRefillBoardRoutine(gamePieces));
    }
    List<Piezas> FindMatches(int starX, int starY, Vector2 searchDirection, int minLength = 3)
    {
        List<Piezas> matches = new List<Piezas>();
        Piezas startPiece = null;

        if (IsWithBounds(starX, starY))
        {
            startPiece = m_allGamePieces[starX, starY];
        }
        if (startPiece != null)
        {
            matches.Add(startPiece);
        }
        else
        {
            return null;
        }

        int nextX;
        int nextY;

        int maxValue = width > height ? width : height;

        for (int i = 1; i < maxValue; i++)
        {
            nextX = starX + (int)Mathf.Clamp(searchDirection.x, -1, 1) * i;
            nextY = starY + (int)Mathf.Clamp(searchDirection.y, -1, 1) * i;

            if (!IsWithBounds(nextX, nextY))
            {
                break;
            }
            Piezas nextPiece = m_allGamePieces[nextX, nextY];

            //comparar si piezas inicila y fianl si son del mismo tipo

            if (nextPiece == null)
            {
                break;
            }
            else
            {
                if (nextPiece.matchValue == startPiece.matchValue && !matches.Contains(nextPiece))
                {
                    matches.Add(nextPiece);
                }
                else
                {
                    break;
                }
            }
        }
        if (matches.Count >= minLength)
        {
            return matches;
        }
        return null;
    }
    List<Piezas> FindVerticalMatches(int startX, int startY, int minLenght = 3)
    {
        List<Piezas> upwardMatches = FindMatches(startX, startY, Vector2.up, 2);
        List<Piezas> downwarMAtches = FindMatches(startX, startY, Vector2.down, 2);

        if (upwardMatches == null)
        {
            upwardMatches = new List<Piezas>();
        }
        if (downwarMAtches == null)
        {
            downwarMAtches = new List<Piezas>();
        }
        var combinedMatches = upwardMatches.Union(downwarMAtches).ToList();

        return combinedMatches.Count >= minLenght ? combinedMatches : null;
    }
    List<Piezas> FindHorizontalMatches(int startX, int startY, int minLenght = 3)
    {
        List<Piezas> rigthMatches = FindMatches(startX, startY, Vector2.right, 2);
        List<Piezas> leftMatches = FindMatches(startX, startY, Vector2.left, 2);

        if (leftMatches == null)
        {
            leftMatches = new List<Piezas>();
        }
        if (rigthMatches == null)
        {
            rigthMatches = new List<Piezas>();
        }
        var combinedMatches = rigthMatches.Union(leftMatches).ToList();

        return combinedMatches.Count >= minLenght ? combinedMatches : null;
    }
    private List<Piezas> FindMatchesAt(int x, int y, int minLenght = 3)
    {
        List<Piezas> horizontalMatches = FindHorizontalMatches(x, y, minLenght);
        List<Piezas> verticalMatches = FindVerticalMatches(x, y, minLenght);

        if (horizontalMatches == null)
        {
            horizontalMatches = new List<Piezas>();
        }

        if (verticalMatches == null)
        {
            verticalMatches = new List<Piezas>();
        }

        var combinedMatches = horizontalMatches.Union(verticalMatches).ToList();
        return combinedMatches;
    }
    private List<Piezas> FindMatchesAt(List<Piezas> gamePieces, int minMatch = 3)
    {
        List<Piezas> matches = new List<Piezas>();

        foreach (Piezas piece in gamePieces)
        {
            matches = matches.Union(FindMatchesAt(piece.xIndex, piece.yIndex)).ToList();
        }
        return matches;
    }
    bool IsNexTo(Tile start, Tile end)
    {

        if (Mathf.Abs(start.xindicex - end.xindicex) == 1 && start.yindicex == end.yindicex)
        {
            return true;
        }
            if (Mathf.Abs(start.yindicex - end.yindicex) == 1 && start.xindicex == end.xindicex)
            {
                return true;
            }
                return false;
    }
    public List<Piezas> FindAllMatches()
    {
        List<Piezas> combinedMatches = new List<Piezas>();
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                var matches = FindMatchesAt(x, y);
                combinedMatches = combinedMatches.Union(matches).ToList();
            }
        }
        return combinedMatches;
    }
    void HighLightTileOff(int x, int y)
    {
        SpriteRenderer spriteRenderer = m_allTiles[x, y].GetComponent<SpriteRenderer>();
        spriteRenderer.color = new Color(spriteRenderer.color.r, spriteRenderer.color.g, spriteRenderer.color.b, 0);
    }
    void HighLightTileOn(int x, int y,Color col)
    {
        SpriteRenderer spriteRendere = m_allTiles[x, y].GetComponent<SpriteRenderer>();
        spriteRendere.color = color;
    }
    void HighLightMatchesAt(int x, int y)
    {
        HighLightTileOff(x, y);
        var combinedMatches = FindMatchesAt(x, y);
        if (combinedMatches.Count > 0)
        {
            foreach (Piezas Piece in combinedMatches)
            {
                HighLightTileOn(Piece.xIndex, Piece.yIndex, Piece.GetComponent<SpriteRenderer>().color);
            }
        }
    }
    void HighLightMatches()
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                HighLightMatchesAt(x, y);
            }
        }
    }
    void HighLightPieces(List<Piezas> gamePieces)
    {
        foreach (Piezas piece in gamePieces)
        {
            if (piece != null)
            {
                HighLightTileOn(piece.xIndex, piece.yIndex, piece.GetComponent<SpriteRenderer>().color);
            }
        }
    }
    private void ClearPieceAt(int x, int y)
    {
        Piezas pieceToClear = m_allGamePieces[x, y];

        if (pieceToClear != null)
        {
            m_allGamePieces[x, y] = null;
            Destroy(pieceToClear.gameObject);
        }
        HighLightTileOff(x, y);
    }
    private void ClearPieceAt(List<Piezas> gamePieces)
    {
        foreach (Piezas piece in gamePieces)
        {
            if (piece != null)
            {
                ClearPieceAt(piece.xIndex, piece.yIndex);
            }
        }
    }
    void ClearBoard()
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                ClearPieceAt(x, y);
            }
        }
    }
    GameObject GetRandomPiece()
    {
        int randomInx = Random.Range(0, gamePiecesPrefabas.Length);
        if (gamePiecesPrefabas[randomInx] == null)
        {
            Debug.LogWarning("La clase board en el array de prefabs en la posicion {randominx} no contiene una pieza valida");
        }
        return gamePiecesPrefabas[randomInx];
    }
    public void PlaceGamePiece(Piezas gamePiece, int x, int y)
    {
        if (gamePieceParent == null)
        {
            Debug.LogWarning($"gamePiece invalida");
            return;
        }

        gamePiece.transform.position = new Vector2(x, y);
        gamePiece.transform.rotation = Quaternion.identity;

        if (IsWithBounds(x, y))
        {
            m_allGamePieces[x, y] = gamePiece;
        }

        gamePiece.SetCoord(x, y);
    }
    bool IsWithBounds(int _x, int _y)
    {
        return (_x >= 0 && _x < width && _y >= 0 && _y < height);
    }
    Piezas FillRandomAt(int x, int y, int falseOffset = 0, float moveTime = .1f)
    {
        Piezas randomPiece = Instantiate(GetRandomPiece(), Vector2.zero, Quaternion.identity).GetComponent<Piezas>();

        if (randomPiece != null)
        {
            randomPiece.Init(this);
            PlaceGamePiece(randomPiece, x, y);

            if (falseOffset != 0)
            {
                randomPiece.transform.position = new Vector2(x, y + falseOffset);
                randomPiece.Move(x, y, moveTime);
            }
            if (gamePieceParent != null)
            {
                randomPiece.transform.parent = gamePieceParent;
            }
        }
        return randomPiece;
    }
    private void ReplaceWithRandom(List<Piezas> gamePiece, int falseOffset = 0, float moveTime = .1f)
    {
        foreach (Piezas piece in gamePiece)
        {
            ClearPieceAt(piece.xIndex, piece.yIndex);
            if (falseOffset == 0)
            {
                FillRandomAt(piece.xIndex, piece.yIndex);
            }
            else
            {
                FillRandomAt(piece.xIndex, piece.yIndex, falseOffset, moveTime);
            }
        }
    }
    List<Piezas> CollapseColum(int column, float colapseTime = 0.1f)
    {
        List<Piezas> movingPieces = new List<Piezas>();

        for (int i = 0; i < height - 1; i++)
        {
            if (m_allGamePieces[column, i] == null)
            {
                for (int j = i + 1; j < height; j++)
                {
                    if (m_allGamePieces[column, j] != null)
                    {
                        m_allGamePieces[column, j].Move(column, i, colapseTime * (j - i));

                        m_allGamePieces[column, i] = m_allGamePieces[column, j];
                        m_allGamePieces[column, i].SetCoord(column, i);

                        if (!movingPieces.Contains(m_allGamePieces[column, i]))
                        {
                            movingPieces.Add(m_allGamePieces[column, i]);
                        }
                        m_allGamePieces[column, j] = null;
                        break;
                    }
                }
            }
        }
        return movingPieces;
    }
    List<Piezas> CollapseColum(List<Piezas> gamePieces)
    {
        List<Piezas> movingPieces = new List<Piezas>();
        List<int> columnsToCollapse = GetColumns(gamePieces);

        foreach (int colums in columnsToCollapse)
        {
            movingPieces = movingPieces.Union(CollapseColum(colums)).ToList();
        }
        return movingPieces;
    }
    List<int> GetColumns(List<Piezas> gamePieces)
    {
        List<int> columns = new List<int>();

        foreach (Piezas piece in gamePieces)
        {
            if (!columns.Contains(piece.xIndex))
            {
                columns.Add(piece.xIndex);
            }
        }
        return columns;
    }
    IEnumerator ClearAndRefillBoardRoutine(List<Piezas> gamepieces)
    {
        List<Piezas> matches = gamepieces;

        do
        {
            yield return StartCoroutine(ClearAndCollapseColumns(matches));
            yield return null;
            yield return StartCoroutine(RefillRoutine());
            matches = FindAllMatches();
            yield return new WaitForSeconds(.5f);
        }
        while (matches.Count != 0);
        m_playerInputEnabled = true;
    }
    IEnumerator ClearAndCollapseColumns(List<Piezas> gamepieces)
    {
        int Comb = 0;
        List<Piezas> movingPiece = new List<Piezas>();
        List<Piezas> matchs = new List<Piezas>();
        HighLightPieces(gamepieces);
        yield return new WaitForSeconds(.5f);

        bool isFinished = false;

        while (!isFinished)
        {
            ClearPieceAt(gamepieces);
            yield return new WaitForSeconds(.25f);

            movingPiece = CollapseColum(gamepieces);
            while (!isColapse(gamepieces))
            {
                yield return null;
            }
            yield return new WaitForSeconds(.5f);

            matchs = FindMatchesAt(movingPiece);
            if (matchs.Count == 0)
            {
                m_playerInputEnabled = true;
                isFinished = true;
                if (Comb >=3)
                {
                    Score(100);
                    Comb = 0;
                }
                if (Comb ==1)
                {
                    Score(points);
                    Comb = 0;
                }
                Score(points *Comb);
                Comb = 0;
                break;
            }
            else
            {
                m_playerInputEnabled = false;
                Comb++;
                yield return StartCoroutine(ClearAndCollapseColumns(matchs));
            }
        }
        yield return null;
    }
    IEnumerator RefillRoutine()
    {
        FillBoard(10, .5f);
        m_playerInputEnabled = false;
        yield return null;
    }
    bool isColapse(List<Piezas> gamePieces)
    {
        foreach (Piezas piece in gamePieces)
        {
            if (piece != null)
            {
                if (piece.transform.position.y - (float)piece.yIndex > 0.001f)
                {
                    return false;
                }
            }
        }
        return true;
    }
    public void sound()
    {
        if (smash == true)
        {
            AudioSource.PlayClipAtPoint(soundTwo, gameObject.transform.position);
        }
        else
        {
            AudioSource.PlayClipAtPoint(soundOne, gameObject.transform.position);
        }
    }
    public void Score(int points)
    {
        scoreValue += points;
        scoreEnPantalla = "Points" + ":" + scoreValue;
        score.text = scoreEnPantalla;

        scoreFinal = score.text;
    }
    private void TimeStart()
    {
        timeFrameScale = Time.deltaTime * timeScale;

        timeSeconds += timeFrameScale;
        Updatewatch(timeSeconds);
    }
    private void Updatewatch(float timeSeconds)
    {
        int min = 0;
        int seconds = 0;
        string textWatch;

        if (timeSeconds < 0)
        {
            timeSeconds = 0;
        }

        min = (int)timeSeconds / 60;
        seconds = (int)timeSeconds % 60;

        textWatch = min.ToString("00") + ":" + seconds.ToString("00");
        time.text = textWatch;
    }
    private void InitialTime()
    {
        initialScaleTime  = timeScale;
        timeSeconds = initialTime;

        Updatewatch(initialTime);
    }
    private void SetParents()
    {
        if (tileParent == null)
        {
            tileParent = new GameObject().transform;
            tileParent.name = "Tile";
            tileParent.parent = this.transform;
        }

        if (gamePieceParent == null)
        {
            gamePieceParent = new GameObject().transform;
            gamePieceParent.name = "Piezas";
            gamePieceParent.parent = this.transform;
        }
    }
    public void GameOver()
    {
        if (move==0)
        {
            Debug.Log("llego a 0");
            m_playerInputEnabled = false;
        }
    }
    public void Movements()
    {
        movements.text = "Movimientos" + ":" + move.ToString(); 
    }
}
