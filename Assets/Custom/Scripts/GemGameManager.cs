using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using PrimeTween;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Events;
using System.Threading.Tasks;

class GemGameGrid : SerializedMonoBehaviour
{
    public static Vector2 CellSize = Vector2.one * 49.55f;
    public static Vector2Int ICellSize = Vector2Int.one * 49;
    GameObject gemPrefab;
    public GameObject gemContainer;
    public GameObject markerGrid;
    public GameObject barrier;
    public Action keyGemCollected;

    public GemGameGrid(GameObject gemPrefab, GameObject gemContainer, GameObject markerGrid, GameObject barrier)
    {
        this.gemPrefab = gemPrefab;
        this.gemContainer = gemContainer;
        this.markerGrid = markerGrid;
        this.barrier = barrier;

        Setup();
    }
    public List<GameObject> selectedGems = new List<GameObject>();
    public Action gemDestroy;
    public int keyGemAmntLimit = 10;
    List<List<GameObject>> grid = new List<List<GameObject>>();
    int rows = 8;
    int columns = 5;
    public float swapSpeed = 0.5f;
    int generatedKeyGems = 0;
    bool[,] initialGemMatrix = new bool[9, 5]
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
    async void Setup()
    {
        /* Initializes grid gems from matrix */
        for (int i = 0; i < rows; i++)
        {
            List<GameObject> row = new List<GameObject>();
            for (int j = 0; j < columns; j++)
            {
                Vector2Int gridPos = new Vector2Int(i, j);
                GameObject newGemObj;
                newGemObj = CreateGem(gridPos, Gem.GetRandomGemType());
                newGemObj.GetComponent<Gem>().gemPosition = gridPos;
                row.Add(newGemObj);
            }
            grid.Add(row);
        }
        barrier.SetActive(true);
        await UpdateGrid();
        barrier.SetActive(false);
    }

    GameObject CreateGem(Vector2Int gridPos, int gemType)
    {
        // creates gem instance at {position} with {gemType} gem type
        Vector3 position = markerGrid.transform.GetChild((gridPos.x * columns) + gridPos.y).transform.position;

        if (gridPos.x == 0)
        {
            position.y += CellSize.y * 1.5f;
        }

        GameObject _gemPrefab = Instantiate(gemPrefab, position, Quaternion.identity, gemContainer.transform);

        _gemPrefab.transform.SetAsFirstSibling();
        _gemPrefab.GetComponent<Gem>().SetGemType(gemType);
        _gemPrefab.GetComponent<Button>().onClick.AddListener(() => HandleGemClick(_gemPrefab));

        if (gemType == Gem.gemTypeAmnt - 1)
        {
            generatedKeyGems++;
        }
        return _gemPrefab;
    }

    GameObject CreateGem(Vector2Int gridPos)
    {
        // creates gem instance at {position} with {gemType} gem type
        int gemType = Gem.GetRandomGemType((generatedKeyGems < keyGemAmntLimit) ? true : false);
        return CreateGem(gridPos, gemType);
    }

    void SimulateGravity()
    {
        /* Simulates gravity by moving gems down to fill empty spaces */
        for (int j = 0; j < columns; j++)
        {
            for (int i = rows - 1; i >= 0; i--)
            {
                if (grid[i][j] != null) continue;
                // find the next gem above
                for (int k = i - 1; k >= 0; k--)
                {
                    if (grid[k][j] == null) continue;
                    // move gem down
                    Tween.CompleteAll(grid[k][j].transform);
                    grid[i][j] = grid[k][j];
                    grid[k][j] = null;
                    grid[i][j].GetComponent<Gem>().gemPosition = new Vector2Int(i, j);
                    break;
                }
            }
        }
    }

    async Task UpdateGrid()
    {
        gridIsProcessing = true;
        hasMatches = true;
        while (hasMatches)
        {
            hasMatches = FoundMatches();
            if (hasMatches) DestroyMatches();
            else
            {
                await SwapGems();
                break;
            };

            SimulateGravity();

            while (hasMatches)
            {
                if (RefillGrid() == false) break;
                SimulateGravity();
                await AnimateVisuals();
                if (CheckForKeyGems())
                {
                    SimulateGravity();
                    await AnimateVisuals();
                }
            }
            hasMatches = FoundMatches();
        }
        gridIsProcessing = false;
    }

    public async void HandleGemClick(GameObject gem)
    {
        if (gridIsProcessing) return;
        /* handles gem click event */
        if (selectedGems.Count >= 2)
        {
            selectedGems.Clear();
        }
        if (selectedGems.Count < 2)
        {
            selectedGems.Add(gem);
        }
        if (SelectedGemsAreParallel() && SelectedGemsAreNeighbors())
        {
            barrier.SetActive(true);
            await SwapGems();
            await UpdateGrid();
            barrier.SetActive(false);
        }
    }

    bool SelectedGemsAreParallel()
    {
        /* checks if selected gems are in the same row or column */
        if (selectedGems.Count != 2) return false;

        Vector2Int pos1 = GetGemPosition(selectedGems[0]);
        Vector2Int pos2 = GetGemPosition(selectedGems[1]);

        return pos1.x == pos2.x || pos1.y == pos2.y;
    }

    bool SelectedGemsAreNeighbors()
    {
        if (selectedGems.Count != 2) return false;

        Vector2Int pos1 = GetGemPosition(selectedGems[0]);
        Vector2Int pos2 = GetGemPosition(selectedGems[1]);

        bool areHorizontalNeighbors = Mathf.Abs(pos1.x - pos2.x) == 1;
        bool areVerticalNeighbors = Mathf.Abs(pos1.y - pos2.y) == 1;

        return areHorizontalNeighbors || areVerticalNeighbors;
    }

    Sequence SwapGems()
    {
        /* swaps the selected gems */
        if (selectedGems.Count != 2)
        {
            return Sequence.Create(); ;
        }
        ;

        Vector2Int pos1 = GetGemPosition(selectedGems[0]);
        Vector2Int pos2 = GetGemPosition(selectedGems[1]);

        if (pos1 == pos2)
        {
            return Sequence.Create();
        }
        ;

        GameObject gem1 = selectedGems[0];
        GameObject gem2 = selectedGems[1];

        // swap positions
        Vector3 gem1Pos = gem1.transform.position;
        Vector3 gem2Pos = gem2.transform.position;

        // swap grid references
        grid[pos1.x][pos1.y] = gem2;
        grid[pos2.x][pos2.y] = gem1;
        gem1.GetComponent<Gem>().gemPosition = pos2;
        gem2.GetComponent<Gem>().gemPosition = pos1;

        return Sequence.Create()
            .Group(Tween.Position(gem1.transform, gem2Pos, 0.2f, swapEaseType))
            .Group(Tween.Position(gem2.transform, gem1Pos, 0.2f, swapEaseType));
    }

    List<(GameObject, Vector2Int)> FindHorizontalMatches() // returns indices of Matches in the hierarchy (1 to rows*columns)
    {
        List<(GameObject, Vector2Int)> Matches = new List<(GameObject, Vector2Int)>();

        for (int rowIdx = 0; rowIdx < rows; rowIdx++)
        {
            List<(GameObject, Vector2Int)> connectedGems = new List<(GameObject, Vector2Int)>();
            Gem.GemTypes? comparingType = null;
            for (int colIdx = 0; colIdx < columns; colIdx++)
            {
                GameObject currentGem = grid[rowIdx][colIdx];
                Gem.GemTypes currentGemType = currentGem.GetComponent<Gem>().gemType;
                if (comparingType == null)
                {
                    comparingType = currentGemType;
                }

                if (currentGemType != comparingType)
                {
                    if (connectedGems.Count > 2)
                    {
                        Matches.AddRange(connectedGems);
                    }
                    connectedGems.Clear();
                    comparingType = currentGemType;
                }

                connectedGems.Add((currentGem, new Vector2Int(rowIdx, colIdx)));

                if (colIdx == columns - 1 && connectedGems.Count > 2)
                {
                    Matches.AddRange(connectedGems);
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
            List<(GameObject, Vector2Int)> connectedGems = new List<(GameObject, Vector2Int)>();
            Gem.GemTypes? comparingType = null;
            for (int rowIdx = 0; rowIdx < rows; rowIdx++)
            {
                GameObject currentGem = grid[rowIdx][colIdx];
                Gem.GemTypes currentGemType = currentGem.GetComponent<Gem>().gemType;
                if (comparingType == null)
                {
                    comparingType = currentGemType;
                }

                if (currentGemType != comparingType)
                {
                    if (connectedGems.Count > 2)
                    {
                        Matches.AddRange(connectedGems);
                    }
                    connectedGems.Clear();
                    comparingType = currentGemType;
                }

                connectedGems.Add((currentGem, new Vector2Int(rowIdx, colIdx)));

                if (rowIdx == rows - 1 && connectedGems.Count > 2)
                {
                    Matches.AddRange(connectedGems);
                }
            }
        }
        return Matches;
    }

    List<(GameObject, Vector2Int)> FindMatches()
    {
        matchesCache.Clear();
        setCache.Clear();

        foreach (var gem in FindHorizontalMatches()) setCache.Add(gem);
        foreach (var gem in FindVerticalMatches()) setCache.Add(gem);

        foreach ((GameObject gem, Vector2Int gemPos) in setCache)
        {
            if (gem.GetComponent<Gem>().gemType != Gem.GemTypes.TYPE5)
            {
                matchesCache.Add((gem, gemPos));
            }
        }
        return matchesCache;
    }

    bool FoundMatches()
    {
        List<(GameObject, Vector2Int)> matchedGems = FindMatches();
        return matchedGems.Count > 0;
    }

    void DestroyMatches()
    {
        List<(GameObject, Vector2Int)> matchedGems = FindMatches();
        // destroy matched gems
        foreach ((GameObject gem, Vector2Int gemPos) in matchedGems)
        {
            if (gem.GetComponent<Gem>().gemType == Gem.GemTypes.TYPE5)
            {
                continue;
            }
            gem.GetComponent<Gem>().MarkForDestroy();
            Tween.CompleteAll(gem.transform);
            grid[gemPos.x][gemPos.y] = null;
            Destroy(gem);
            EnvironmentalAudioManager.Instance.PlaySFX("crystal_crush");
        }
    }

    bool RefillGrid()
    {
        bool continueRefill = false;
        /* refills the grid with new gems */
        for (int i = 0; i < rows; i++)
        {
            for (int j = 0; j < columns; j++)
            {
                if (grid[i][j] == null)
                {
                    GameObject newGem = CreateGem(new Vector2Int(0, j));
                    grid[i][j] = newGem;
                    newGem.GetComponent<Gem>().gemPosition = new Vector2Int(i, j);
                    continueRefill = true;
                    continue;
                }
            }
        }
        return continueRefill;
    }

    Sequence AnimateVisuals()
    {
        Sequence sequence = Sequence.Create();
        /* animates the gems to their new positions */
        for (int i = 0; i < rows; i++)
        {
            for (int j = 0; j < columns; j++)
            {
                GameObject gem = grid[i][j];
                if (gem == null) continue;

                Vector3 targetPos = markerGrid.transform.GetChild((i * columns) + j).transform.position;
                Vector3 gemPos = gem.transform.position;

                if (i == rows - 1 && gem.GetComponent<Gem>().gemType == Gem.GemTypes.TYPE5)
                {
                    sequence.Group(Tween.Position(gem.transform, targetPos - new Vector3(0, CellSize.y * 1.5f, 0), swapSpeed, fallEaseType).OnComplete(() =>
                    {
                        EnvironmentalAudioManager.Instance.PlaySFX("crystal_fall");
                    }));
                }
                else if (gemPos != targetPos)
                {
                    sequence.Group(Tween.Position(gem.transform, targetPos, swapSpeed, fallEaseType));
                }
            }
        }
        return sequence;
    }

    bool CheckForKeyGems()
    {
        bool continueRefill = false;
        for (int i = 0; i < rows; i++)
        {
            for (int j = 0; j < columns; j++)
            {
                GameObject gem = grid[i][j];
                if (gem == null) continue;
                if (i == rows - 1 && gem.GetComponent<Gem>().gemType == Gem.GemTypes.TYPE5)
                {
                    grid[i][j] = null;
                    Destroy(gem);
                    keyGemCollected?.Invoke();
                    continueRefill = true;
                }
            }
        }
        return continueRefill;
    }

    // helper functions
    Vector2Int GetGemPosition(GameObject gem)
    {
        return gem.GetComponent<Gem>().gemPosition;
    }
    
    bool HasPossibleMoves()
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

                    if (newRow < rows && newCol < columns)
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
}

public class GemGameManager : SerializedMonoBehaviour
{
    GemGameGrid grid;
    [OdinSerialize, TabGroup("tab1","General", SdfIconType.GearFill), AssetsOnly] GameObject gemPrefab;
    [OdinSerialize, TabGroup("tab1","General"), SceneObjectsOnly] GameObject gemContainer;
    [OdinSerialize, TabGroup("tab1","General"), SceneObjectsOnly] GameObject markerGrid;
    [OdinSerialize, TabGroup("tab1","General"), SceneObjectsOnly] GameObject barrier;
    [OdinSerialize, TabGroup("tab1","General")] TMP_Text keyGemAmntDisplay;
    [OdinSerialize, TabGroup("tab1","General"), HideInInspector] public int collectedKeyGems = 0;
    [OdinSerialize, TabGroup("tab1","General")] int keyGemAmntLimit = 10;

    [InfoBox("How fast the gems move/swap; Default is 0.5")]
    [OdinSerialize, TabGroup("tab1","Appearance", SdfIconType.PaletteFill, TextColor = "orange"), PropertyRange(0.0f, 5.0f)]
    float swapSpeed = 0.5f;
    [InfoBox("Easing of gems when swapping; Default is Linear")]
    [OdinSerialize, TabGroup("tab1", "Appearance")] Ease swapEaseType = Ease.Linear;
    [InfoBox("Easing of gems when falling down; Default is In Cubic")]
    [OdinSerialize, TabGroup("tab1", "Appearance")] Ease fallEaseType = Ease.InCubic;

    private void Start()
    {
        grid = new GemGameGrid(gemPrefab, gemContainer, markerGrid, barrier);
        grid.keyGemCollected += OnKeyGemCollected;
        grid.keyGemAmntLimit = keyGemAmntLimit;
        grid.swapSpeed = swapSpeed;
        grid.swapEaseType = swapEaseType;
        grid.fallEaseType = fallEaseType;

        keyGemAmntDisplay.text = $"0/{keyGemAmntLimit}";
    }

    void OnKeyGemCollected()
    {
        collectedKeyGems++;
        keyGemAmntDisplay.text = $"{collectedKeyGems}/{keyGemAmntLimit}";
        if (collectedKeyGems >= keyGemAmntLimit)
        {
            for (int i = 0; i < grid.gemContainer.transform.childCount; i++)
            {
                GameObject gem = gemContainer.transform.GetChild(i).gameObject;
                Tween.CompleteAll(gem.transform);
            }
            gameObject.GetComponent<PuzzleCompletionEmitter>().onPuzzleCompleted?.Invoke();
        }
    }

    [Button(ButtonSizes.Small)]
    public void InstantWin()
    {
        collectedKeyGems = keyGemAmntLimit;
        OnKeyGemCollected();
    }
}
