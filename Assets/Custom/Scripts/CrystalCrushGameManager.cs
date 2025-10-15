using System;
using System.Collections.Generic;
using System.Linq;
using PrimeTween;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Events;
using Cysharp.Threading.Tasks;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using UnityEngine.InputSystem;
class CrystalGameGrid : SerializedMonoBehaviour
{
    public static Vector2 CellSize = Vector2.one * 49.55f;
    public static Vector2Int ICellSize = Vector2Int.one * 49;
    GameObject crystalPrefab;
    public GameObject crystalContainer;
    public GameObject markerGrid;
    public GameObject barrier;
    public UnityAction KeyCrystalCollected;

    public CrystalGameGrid(GameObject crystalPrefab, GameObject crystalContainer, GameObject markerGrid, GameObject barrier)
    {
        this.crystalPrefab = crystalPrefab;
        this.crystalContainer = crystalContainer;
        this.markerGrid = markerGrid;
        this.barrier = barrier;

        Setup();
    }
    public List<GameObject> selectedCrystals = new List<GameObject>();
    public UnityAction OnCrystalDestroyed;
    public int keyCrystalAmntLimit = 10;
    List<List<GameObject>> grid = new List<List<GameObject>>();
    int rows = 8;
    int columns = 5;
    public float swapSpeed = 0.5f;
    int generatedKeyCrystals = 0;
    int keyCrystalsOnScreen = 0;
    int keyCrystalsOnScreenLimit = 2;
    bool[,] initialCrystalMatrix = new bool[9, 5]
    {
        { false, false, false, false, false },
        { true, true, true, true, true },
        { false, false, false, false, false },
        { false, false, false, false, false },
        { false, false, false, false, false },
        { false, false, false, false, false },
        { false, false, false, false, false },
        { false, false, false, false, false },
        { false, false, false, false, false }
    };
    List<(GameObject, Vector2Int)> matchesCache = new List<(GameObject, Vector2Int)>();
    [HideInInspector] HashSet<(GameObject, Vector2Int)> setCache = new HashSet<(GameObject, Vector2Int)>();
    bool gridIsProcessing = false;
    bool hasMatches = false;
    public Ease swapEaseType = Ease.Linear;
    public Ease fallEaseType = Ease.InCubic;
    public UnityAction OnGameOver;
    public bool gameOver = false;
    async void Setup()
    {
        /* Initializes grid crystals from matrix */
        for (int i = 0; i < rows; i++)
        {
            List<GameObject> row = new List<GameObject>();
            for (int j = 0; j < columns; j++)
            {
                Vector2Int gridPos = new Vector2Int(i, j);
                GameObject newCrystalObj = CreateCrystal(gridPos, Crystal.GetRandomCrystalType());
                newCrystalObj.GetComponent<Crystal>().crystalPosition = gridPos;
                row.Add(newCrystalObj);
            }
            grid.Add(row);
        }
        // GenerateUnplayableGame();
        barrier.SetActive(true);
        await UpdateGrid();
        await AnimateVisuals();
        barrier.SetActive(false);
    }

    GameObject CreateCrystal(Vector2Int gridPos, int crystalType)
    {
        // creates crystal instance at {position} with {crystalType} crystal type
        Vector3 position = markerGrid.transform.GetChild((gridPos.x * columns) + gridPos.y).transform.position;

        if (gridPos.x == 0)
        {
            position.y += CellSize.y * 2f;
        }
        GameObject _crystalPrefab = Instantiate(crystalPrefab, position, Quaternion.identity, crystalContainer.transform);
        _crystalPrefab.transform.SetAsFirstSibling();
        _crystalPrefab.GetComponent<Crystal>().SetCrystalType(crystalType);
        _crystalPrefab.GetComponent<Button>().onClick.AddListener(() => HandleCrystalClick(_crystalPrefab));

        if (crystalType == Crystal.crystalTypeAmnt - 1)
        {
            generatedKeyCrystals++;
            keyCrystalsOnScreen++;
        }
        return _crystalPrefab;
    }

    GameObject CreateCrystal(Vector2Int gridPos)
    {
        // creates crystal instance at {position} with {crystalType} crystal type
        bool canGenerateKeyCrystal = (keyCrystalsOnScreen < keyCrystalsOnScreenLimit) && (generatedKeyCrystals < keyCrystalAmntLimit);
        int crystalType = Crystal.GetRandomCrystalType(canGenerateKeyCrystal);
        return CreateCrystal(gridPos, crystalType);
    }

    void SimulateGravity()
    {
        /* Simulates gravity by moving crystals down to fill empty spaces */
        for (int j = 0; j < columns; j++)
        {
            for (int i = rows - 1; i >= 0; i--)
            {
                if (grid[i][j] != null) continue;
                // find the next crystal above
                for (int k = i - 1; k >= 0; k--)
                {
                    if (grid[k][j] == null) continue;
                    // move crystal down
                    Tween.CompleteAll(grid[k][j].transform);
                    grid[i][j] = grid[k][j];
                    grid[k][j] = null;
                    grid[i][j].GetComponent<Crystal>().crystalPosition = new Vector2Int(i, j);
                    break;
                }
            }
        }
    }

    async UniTask UpdateGrid()
    {
        gridIsProcessing = true;
        hasMatches = true;
        while (hasMatches)
        {
            hasMatches = FoundMatches();
            if (hasMatches) DestroyMatches();
            else
            {
                if (HasLegalMoves() == false)
                {
                    // No possible matches currently & no moves to make any matches,
                    // shuffle grid because theres no legal moves
                    ShuffleGrid();
                    await AnimateVisuals();
                    continue;
                }
                await SwapCrystals();
                break;
            }
            ;

            SimulateGravity();

            while (hasMatches)
            {
                if (RefillGrid() == false) break;
                SimulateGravity();
                await AnimateVisuals();
                if (CheckForKeyCrystals())
                {
                    SimulateGravity();
                    await AnimateVisuals();
                }
            }
            hasMatches = FoundMatches();
        }
        gridIsProcessing = false;
        if (gameOver) OnGameOver?.Invoke();
    }

    public async void HandleCrystalClick(GameObject crystal)
    {
        if (gridIsProcessing) return;
        /* handles crystal click event */
        if (selectedCrystals.Count >= 2)
        {
            UnmarkSelectedCrystals();
            selectedCrystals.Clear();
        }
        if (selectedCrystals.Count < 2)
        {
            crystal.GetComponent<Crystal>().Mark();
            selectedCrystals.Add(crystal);
        }
        if (SelectedCrystalsAreParallel() && SelectedCrystalsAreNeighbors())
        {
            UnmarkSelectedCrystals();
            barrier.SetActive(true);
            await SwapCrystals();
            await UpdateGrid();
            barrier.SetActive(false);
        }
        else if(selectedCrystals.Count >= 2)
        {
            UnmarkSelectedCrystals();
        }
    }

    void UnmarkSelectedCrystals()
    {
        foreach (GameObject crystal in selectedCrystals)
        {
            crystal.GetComponent<Crystal>().Unmark();
        }
    }

    bool SelectedCrystalsAreParallel()
    {
        /* checks if selected crystals are in the same row or column */
        if (selectedCrystals.Count != 2) return false;

        Vector2Int pos1 = GetCrystalPosition(selectedCrystals[0]);
        Vector2Int pos2 = GetCrystalPosition(selectedCrystals[1]);

        return pos1.x == pos2.x || pos1.y == pos2.y;
    }

    bool SelectedCrystalsAreNeighbors()
    {
        if (selectedCrystals.Count != 2) return false;

        Vector2Int pos1 = GetCrystalPosition(selectedCrystals[0]);
        Vector2Int pos2 = GetCrystalPosition(selectedCrystals[1]);

        bool areHorizontalNeighbors = Mathf.Abs(pos1.x - pos2.x) == 1;
        bool areVerticalNeighbors = Mathf.Abs(pos1.y - pos2.y) == 1;

        return areHorizontalNeighbors || areVerticalNeighbors;
    }

    Sequence SwapCrystals()
    {
        /* swaps the selected crystals */
        if (selectedCrystals.Count != 2)
        {
            return Sequence.Create(); ;
        }
        ;

        Vector2Int pos1 = GetCrystalPosition(selectedCrystals[0]);
        Vector2Int pos2 = GetCrystalPosition(selectedCrystals[1]);

        if (pos1 == pos2)
        {
            return Sequence.Create();
        }
        ;

        GameObject crystal1 = selectedCrystals[0];
        GameObject crystal2 = selectedCrystals[1];

        // swap positions
        Vector3 crystal1Pos = crystal1.transform.position;
        Vector3 crystal2Pos = crystal2.transform.position;

        // swap grid references
        grid[pos1.x][pos1.y] = crystal2;
        grid[pos2.x][pos2.y] = crystal1;
        crystal1.GetComponent<Crystal>().crystalPosition = pos2;
        crystal2.GetComponent<Crystal>().crystalPosition = pos1;

        return Sequence.Create()
            .Group(Tween.Position(crystal1.transform, crystal2Pos, 0.2f, swapEaseType))
            .Group(Tween.Position(crystal2.transform, crystal1Pos, 0.2f, swapEaseType));
    }

    List<(GameObject, Vector2Int)> FindHorizontalMatches() // returns indices of Matches in the hierarchy (1 to rows*columns)
    {
        List<(GameObject, Vector2Int)> Matches = new List<(GameObject, Vector2Int)>();

        for (int rowIdx = 0; rowIdx < rows; rowIdx++)
        {
            List<(GameObject, Vector2Int)> connectedCrystals = new List<(GameObject, Vector2Int)>();
            Crystal.CrystalTypes? comparingType = null;
            for (int colIdx = 0; colIdx < columns; colIdx++)
            {
                GameObject currentCrystal = grid[rowIdx][colIdx];
                Crystal.CrystalTypes currentCrystalType = currentCrystal.GetComponent<Crystal>().crystalType;
                if (comparingType == null)
                {
                    comparingType = currentCrystalType;
                }

                if (currentCrystalType != comparingType)
                {
                    if (connectedCrystals.Count > 2)
                    {
                        Matches.AddRange(connectedCrystals);
                    }
                    connectedCrystals.Clear();
                    comparingType = currentCrystalType;
                }

                connectedCrystals.Add((currentCrystal, new Vector2Int(rowIdx, colIdx)));

                if (colIdx == columns - 1 && connectedCrystals.Count > 2)
                {
                    Matches.AddRange(connectedCrystals);
                }
            }
        }
        return Matches;
    }

    List<(GameObject, Vector2Int)> FindVerticalMatches()
    {
        List<(GameObject, Vector2Int)> Matches = new List<(GameObject, Vector2Int)>();

        for (int colIdx = 0; colIdx < columns; colIdx++)
        {
            List<(GameObject, Vector2Int)> connectedCrystals = new List<(GameObject, Vector2Int)>();
            Crystal.CrystalTypes? comparingType = null;
            for (int rowIdx = 0; rowIdx < rows; rowIdx++)
            {
                GameObject currentCrystal = grid[rowIdx][colIdx];
                Crystal.CrystalTypes currentCrystalType = currentCrystal.GetComponent<Crystal>().crystalType;
                if (comparingType == null)
                {
                    comparingType = currentCrystalType;
                }

                if (currentCrystalType != comparingType)
                {
                    if (connectedCrystals.Count > 2)
                    {
                        Matches.AddRange(connectedCrystals);
                    }
                    connectedCrystals.Clear();
                    comparingType = currentCrystalType;
                }

                connectedCrystals.Add((currentCrystal, new Vector2Int(rowIdx, colIdx)));

                if (rowIdx == rows - 1 && connectedCrystals.Count > 2)
                {
                    Matches.AddRange(connectedCrystals);
                }
            }
        }
        return Matches;
    }

    List<(GameObject, Vector2Int)> FindMatches()
    {
        matchesCache.Clear();
        setCache.Clear();

        foreach (var crystal in FindHorizontalMatches()) setCache.Add(crystal);
        foreach (var crystal in FindVerticalMatches()) setCache.Add(crystal);

        foreach ((GameObject crystal, Vector2Int crystalPos) in setCache)
        {
            if (crystal.GetComponent<Crystal>().crystalType != Crystal.CrystalTypes.TYPE5)
            {
                matchesCache.Add((crystal, crystalPos));
            }
        }
        return matchesCache;
    }

    bool FoundMatches()
    {
        List<(GameObject, Vector2Int)> matchedCrystals = FindMatches();
        return matchedCrystals.Count > 0;
    }

    void DestroyMatches()
    {
        List<(GameObject, Vector2Int)> matchedCrystals = FindMatches();
        // destroy matched crystals
        foreach ((GameObject crystal, Vector2Int crystalPos) in matchedCrystals)
        {
            if (crystal.GetComponent<Crystal>().crystalType == Crystal.CrystalTypes.TYPE5)
            {
                continue;
            }
            Tween.CompleteAll(crystal.transform);
            grid[crystalPos.x][crystalPos.y] = null;

            
            Destroy(crystal);
        }
        if(matchedCrystals.Count > 0)
        {
            UnmarkSelectedCrystals();
            selectedCrystals.Clear();
            EnvironmentalAudioManager.Instance.PlaySFX("crystal_crush",true);
        }
    }

    bool RefillGrid()
    {
        bool continueRefill = false;
        /* refills the grid with new crystals */
        for (int i = 0; i < rows; i++)
        {
            for (int j = 0; j < columns; j++)
            {
                if (grid[i][j] == null)
                {
                    GameObject newCrystal = CreateCrystal(new Vector2Int(0, j));
                    grid[i][j] = newCrystal;
                    newCrystal.GetComponent<Crystal>().crystalPosition = new Vector2Int(i, j);
                    continueRefill = true;
                    continue;
                }
            }
        }
        return continueRefill;
    }

    Sequence AnimateVisuals()
    {
        bool crystalIsFalling = false;
        Sequence sequence = Sequence.Create();
        /* animates the crystals to their new positions */
        for (int i = 0; i < rows; i++)
        {
            for (int j = 0; j < columns; j++)
            {
                GameObject crystal = grid[i][j];
                if (crystal == null) continue;

                Vector3 targetPos = markerGrid.transform.GetChild((i * columns) + j).transform.position;
                Vector3 crystalPos = crystal.transform.position;

                if (i == rows - 1 && crystal.GetComponent<Crystal>().crystalType == Crystal.CrystalTypes.TYPE5)
                {
                    // if is bottom row and a key crystal, move down
                    sequence = Sequence.Create()
                        .Group(Tween.Position(crystal.transform, targetPos - new Vector3(0, CellSize.y * 4f, 0), swapSpeed, fallEaseType));
                }
                else if (crystalPos != targetPos)
                {
                    crystalIsFalling = true;
                    float distance = Mathf.Abs(crystalPos.y - targetPos.y);
                    float steps = Mathf.Floor(distance / CellSize.y);
                    sequence = Sequence.Create()
                        .Group(Tween.Position(crystal.transform, targetPos, swapSpeed, fallEaseType));
                        
                }
            }
        }
        if (crystalIsFalling)
            sequence.ChainCallback(() =>
            {
                EnvironmentalAudioManager.Instance.PlaySFX("crystal_impact");
            });
        return sequence;
    }

    bool CheckForKeyCrystals()
    {
        bool continueRefill = false;
        for (int i = 0; i < rows; i++)
        {
            for (int j = 0; j < columns; j++)
            {
                GameObject crystal = grid[i][j];
                if (crystal == null) continue;
                if (i == rows - 1 && crystal.GetComponent<Crystal>().crystalType == Crystal.CrystalTypes.TYPE5)
                {
                    grid[i][j] = null;

                    if (selectedCrystals.Contains(crystal)) selectedCrystals.Remove(crystal);
                    Destroy(crystal);

                    KeyCrystalCollected?.Invoke();
                    continueRefill = true;
                    keyCrystalsOnScreen--;
                }
            }
        }
        return continueRefill;
    }

    // helper functions
    Vector2Int GetCrystalPosition(GameObject crystal)
    {
        return crystal.GetComponent<Crystal>().crystalPosition;
    }
    public bool HasLegalMoves()
    {
        for (int row = 0; row < rows; row++)
        {
            for (int col = 0; col < columns; col++)
            {
                GameObject current = grid[row][col];
                if (current == null) continue;

                // Check 4 directions
                Vector2Int[] directions = { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right };

                foreach (var dir in directions)
                {
                    int newRow = row + dir.y;
                    int newCol = col + dir.x;

                    if (newRow < rows && newCol < columns && newRow >= 0 && newCol >= 0)
                    {
                        GameObject neighbor = grid[newRow][newCol];
                        if (neighbor == null) continue;

                        // Temporarily swap
                        grid[row][col] = neighbor;
                        grid[newRow][newCol] = current;

                        // Check if swap creates a match
                        if (FoundMatches())
                        {
                            // Swap back
                            grid[row][col] = current;
                            grid[newRow][newCol] = neighbor;
                            return true;
                        }

                        // Swap back
                        grid[row][col] = current;
                        grid[newRow][newCol] = neighbor;
                    }
                }
            }
        }
        return false;
    }
    void GenerateUnplayableGame()
    {
        int[,] newCrystalMatrix = new int[9, 5]
        {
            { 0, 1, 2, 3, 0 },
            { 1, 2, 3, 0, 1 },
            { 2, 3, 0, 1, 2 },
            { 3, 0, 1, 2 ,3 },
            { 0, 1, 2, 3, 0 },
            { 1, 2, 3, 0, 1 },
            { 2, 3, 0, 1, 2 },
            { 3, 0, 1, 2 ,3 },
            { 0, 1, 2, 3, 0 },
        };
        for (int i = 0; i < rows; i++)
        {
            List<GameObject> row = new List<GameObject>();
            for (int j = 0; j < columns; j++)
            {
                Vector2Int gridPos = new Vector2Int(i, j);
                GameObject newCrystalObj = CreateCrystal(new Vector2Int(i, j), newCrystalMatrix[i, j]);
                row.Add(newCrystalObj);
                newCrystalObj.GetComponent<Crystal>().crystalPosition = gridPos;
            }
            grid.Add(row);
        }
    }
    void ShuffleGrid()
    {
        // Step 1 - turn to long list
        List<GameObject> _list = new List<GameObject>();
        for (int i = 0; i < rows; i++)
        {
            for (int j = 0; j < columns; j++)
            {
                GameObject crystalObj = grid[i][j];
                if (crystalObj.GetComponent<Crystal>().crystalType == Crystal.CrystalTypes.TYPE5) continue;
                _list.Add(grid[i][j]);
                grid[i][j] = null;
            }
        }
        // Step 2 - convert to randomized nested list
        for (int i = 0; i < rows; i++)
        {
            for (int j = 0; j < columns; j++)
            {
                if (grid[i][j] != null) continue;
                bool pickFirst = UnityEngine.Random.Range(0, 2) == 0 ? true : false;
                GameObject crystalObj = pickFirst ? _list.First() : _list.Last();
                _list.Remove(crystalObj);
                grid[i][j] = crystalObj;
                crystalObj.GetComponent<Crystal>().crystalPosition = new Vector2Int(i, j);
            }
        }
    }
    public void ShuffleAndAnimate()
    {
        ShuffleGrid();
        AnimateVisuals();
    }
}

public class CrystalCrushGameManager : SerializedMonoBehaviour
{
    CrystalGameGrid grid;
    [OdinSerialize, TabGroup("tab1", "General", SdfIconType.GearFill), AssetsOnly] GameObject crystalPrefab;
    [OdinSerialize, TabGroup("tab1", "General"), SceneObjectsOnly] GameObject crystalContainer;
    [OdinSerialize, TabGroup("tab1", "General"), SceneObjectsOnly] GameObject markerGrid;
    [OdinSerialize, TabGroup("tab1", "General"), SceneObjectsOnly] GameObject barrier;
    [OdinSerialize, TabGroup("tab1", "General")] TMP_Text keyCrystalAmntDisplay;
    [OdinSerialize, TabGroup("tab1", "General"), HideInInspector] public int collectedKeyCrystals = 0;
    [OdinSerialize, TabGroup("tab1", "General")] int keyCrystalAmntLimit = 10;

    [InfoBox("How fast the crystals move/swap; Default is 0.5")]
    [OdinSerialize, TabGroup("tab1", "Appearance", SdfIconType.PaletteFill, TextColor = "orange"), PropertyRange(0.0f, 5.0f)]
    float swapSpeed = 0.5f;
    [InfoBox("Easing of crystals when swapping; Default is Linear")]
    [OdinSerialize, TabGroup("tab1", "Appearance")] Ease swapEaseType = Ease.Linear;
    [InfoBox("Easing of crystals when falling down; Default is In Cubic")]
    [OdinSerialize, TabGroup("tab1", "Appearance")] Ease fallEaseType = Ease.InCubic;
    public UnityEvent onKeyCrystalCollected;
    void Start()
    {
        grid = new CrystalGameGrid(crystalPrefab, crystalContainer, markerGrid, barrier);
        grid.KeyCrystalCollected += OnKeyCrystalCollected;
        grid.keyCrystalAmntLimit = keyCrystalAmntLimit;
        grid.swapSpeed = swapSpeed;
        grid.swapEaseType = swapEaseType;
        grid.fallEaseType = fallEaseType;
        grid.OnGameOver += EndGame;
        keyCrystalAmntDisplay.text = $"0/{keyCrystalAmntLimit}";
    }

    void OnKeyCrystalCollected()
    {
        collectedKeyCrystals++;
        keyCrystalAmntDisplay.text = $"{collectedKeyCrystals}/{keyCrystalAmntLimit}";
        onKeyCrystalCollected?.Invoke();
        if (collectedKeyCrystals >= keyCrystalAmntLimit)
        {
            grid.gameOver = true;
        }
    }

    [HorizontalGroup("A"), Button(ButtonSizes.Large), DisableInEditorMode]
    void EndGame()
    {
        gameObject.GetComponent<PuzzleCompletionEmitter>().onPuzzleCompleted?.Invoke();
        UIManager.LockCursor(true);

        if (PersistentDataManager.Instance.HasEventPassed("gemGameFinished")) return;
        NarrativeManager.Instance.CharacterSpeak("SC12_Fasai_CandyCrush_Finish");
        PersistentDataManager.Instance.MarkEventAsPassed("gemGameFinished");
    }

    [HorizontalGroup("A"), Button(ButtonSizes.Large), DisableInEditorMode]
    void Shuffle()
    {
        grid.ShuffleAndAnimate();
    }
}
