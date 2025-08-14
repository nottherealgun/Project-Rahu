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
    GameObject gemPrefab;
    public GameObject gemContainer;
    GameObject gridMarkerPrefab;
    public GameObject markerGrid;
    public GameObject barrier;
    public Action keyGemCollected;

    public GemGameGrid(GameObject gemPrefab, GameObject gemContainer, GameObject gridMarkerPrefab, GameObject markerGrid, GameObject barrier, ref Action keyGemCollected)
    {
        this.gemPrefab = gemPrefab;
        this.gemContainer = gemContainer;
        this.gridMarkerPrefab = gridMarkerPrefab;
        this.markerGrid = markerGrid;
        this.barrier = barrier;
        this.keyGemCollected = keyGemCollected;

        Setup();
    }

    [OdinSerialize][ReadOnly] public List<GameObject> selectedGems = new List<GameObject>();

    [TableMatrix(HorizontalTitle = "Current Gem Matrix")]
    [OdinSerialize, ReadOnly]
    List<List<GameObject>> grid = new List<List<GameObject>>();

    int rows = 9;
    int columns = 5;
    float swapSpeed = 0.2f;
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

    public Action gemDestroy;
    bool gridIsProcessing = false;
    bool hasMatches = false;
    void Setup()
    {
        /* Initializes grid gems from matrix */
        for (int i = 0; i < rows; i++)
        {
            List<GameObject> row = new List<GameObject>();
            for (int j = 0; j < columns; j++)
            {
                Vector2Int gridPos = new Vector2Int(i, j);
                GameObject newGemObj;
                if (initialGemMatrix[i, j] == true)
                {
                    newGemObj = CreateGem(gridPos, 4);
                }
                else
                {
                    newGemObj = CreateGem(gridPos);
                }
                newGemObj.GetComponent<Gem>().gemPosition = gridPos;
                row.Add(newGemObj);
            }
            grid.Add(row);
        }
    }

    GameObject CreateGem(Vector2Int gridPos, int gemType)
    {
        // creates gem instance at {position} with {gemType} gem type
        Vector3 position = markerGrid.transform.GetChild((gridPos.x * columns) + gridPos.y).transform.position;
        GameObject _gemPrefab = Instantiate(gemPrefab, position, Quaternion.identity, gemContainer.transform);
        _gemPrefab.GetComponent<Gem>().SetGemType(gemType);
        _gemPrefab.transform.SetAsFirstSibling();
        _gemPrefab.GetComponent<Button>().onClick.AddListener(() => HandleGemClick(_gemPrefab));
        return _gemPrefab;
    }

    GameObject CreateGem(Vector2Int gridPos)
    {
        // creates gem instance at {position} with {gemType} gem type
        int gemType = Gem.GetRandomGemType();
        Vector3 position = markerGrid.transform.GetChild((gridPos.x * columns) + gridPos.y).transform.position;
        GameObject _gemPrefab = Instantiate(gemPrefab, position, Quaternion.identity, gemContainer.transform);
        _gemPrefab.GetComponent<Gem>().SetGemType(gemType);
        _gemPrefab.transform.SetAsFirstSibling();
        _gemPrefab.GetComponent<Button>().onClick.AddListener(() => HandleGemClick(_gemPrefab));
        return _gemPrefab;
    }

    void SimulateGravity()
    {
        /* Simulates gravity by moving gems down to fill empty spaces */
        for (int j = 0; j < columns; j++)
        {
            for (int i = rows - 1; i >= 0; i--)
            {
                if (grid[i][j] == null)
                {
                    // find the next gem above
                    for (int k = i - 1; k >= 0; k--)
                    {
                        if (grid[k][j] != null)
                        {
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
        }
    }

    async void UpdateGrid()
    {
        gridIsProcessing = true;
        barrier.SetActive(true);
        hasMatches = true;
        while (hasMatches)
        {
            hasMatches = FoundMatches();
            DestroyMatches();
            SimulateGravity();
            while (hasMatches)
            {
                hasMatches = RefillGrid();
                SimulateGravity();
                await AnimateVisuals();
            }
            await CheckForKeyGems();
        }
        gridIsProcessing = false;
        barrier.SetActive(false);
    }

    public async Task HandleGemClick(GameObject gem)
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
        if (SelectedGemsAreParallel())
        {
            await SwapGems();
            UpdateGrid();
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

    Sequence SwapGems(bool recursive = true)
    {
        /* swaps the selected gems */
        if (selectedGems.Count != 2)
        {
            return Sequence.Create(); ;
        };

        Vector2Int pos1 = GetGemPosition(selectedGems[0]);
        Vector2Int pos2 = GetGemPosition(selectedGems[1]);

        if (pos1 == pos2)
        {
            return Sequence.Create();
        };

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
            .ChainCallback(() =>
            {
                // print($"{gem1Pos} and {gem2Pos}");
                Tween.Position(gem1.transform, gem2Pos, swapSpeed, Ease.OutCubic);
                Tween.Position(gem2.transform, gem1Pos, swapSpeed, Ease.OutCubic);
            });
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
        List<(GameObject, Vector2Int)> Matches = new List<(GameObject, Vector2Int)>();
        var set = new HashSet<(GameObject, Vector2Int)>();
        foreach (var gem in FindHorizontalMatches())    set.Add(gem);
        foreach (var gem in FindVerticalMatches())      set.Add(gem);
        Matches = set.ToList();
        return Matches;
    }

    bool FoundMatches()
    {
        List<(GameObject, Vector2Int)> matchedGems = FindMatches();
        return matchedGems.Count > 0;
    }

    void DestroyMatches(bool checkFromSwap = false)
    {
        List<(GameObject, Vector2Int)> matchedGems = FindMatches();
        if (matchedGems.Count == 0)
        {
            // if (checkFromSwap)
            // {
            //     // print("No matches found after swap, reverting gems.");
            //     Tween.CompleteAll(selectedGems[0].transform);
            //     Tween.CompleteAll(selectedGems[1].transform);
            //     SwapGems(false);
            // }
        }

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
        }

        // Sequence.Create()
        //     .ChainDelay(swapSpeed)
        //     .ChainCallback(() =>
        //     {
        //         SimulateGravity();
        //         RefillGrid();
        //         AnimateVisuals();
        //     })
        //     .ChainDelay(swapSpeed)
        //     .ChainCallback(() =>
        //     {
        //         // check for matches again after gravity and refill
        //         CheckForMatches();
        //         CheckForKeyGems();
        //     });
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
                if (grid[i][j] != null && ReferenceEquals(grid[i][j], null) == false)
                {
                    Vector3 targetPosition = markerGrid.transform.GetChild((i * columns) + j).transform.position;
                    sequence
                        .Group(Tween.Position(grid[i][j].transform, targetPosition, swapSpeed, Ease.OutCubic));
                }
            }
        }
        return sequence;
    }

    Sequence CheckForKeyGems()
    {
        Sequence sequence = Sequence.Create();
        gemDestroy = null;

        /* checks for key gems in the grid */
        for (int i = 0; i < gemContainer.transform.childCount; i++)
        {
            GameObject gem = gemContainer.transform.GetChild(i).gameObject;
            // fall off grid + destroy
            Vector2Int gemPosition = gem.GetComponent<Gem>().gemPosition;
            if (gem.GetComponent<Gem>().gemType == Gem.GemTypes.TYPE5 && gemPosition.x == rows - 1)
            {
                gemDestroy += () =>
                {
                    keyGemCollected?.Invoke();
                    Tween.CompleteAll(gem.transform);
                    DestroyImmediate(gem);
                };
                sequence
                    .Group(Tween.PositionY(gem.transform, -100f, swapSpeed, Ease.OutCubic));
            }
        }

        return sequence;
    }

    // helper functions
    Vector2Int GetGemPosition(GameObject gem)
    {
        return gem.GetComponent<Gem>().gemPosition;
    }
}

public class GemGameManager : SerializedMonoBehaviour
{
    [OdinSerialize, AssetsOnly] GameObject gemPrefab;
    [OdinSerialize, SceneObjectsOnly] GameObject gemContainer;
    [OdinSerialize, AssetsOnly] GameObject gridMarkerPrefab;
    [OdinSerialize, SceneObjectsOnly] GameObject markerGrid;
    [OdinSerialize, SceneObjectsOnly] GameObject barrier;
    [OdinSerialize] TMP_Text keyGemAmntDisplay;
    [HideInInspector] public int collectedKeyGems = 0;
    [HideInInspector] public Action keyGemCollected;

    GemGameGrid grid;

    private void Start()
    {
        grid = new GemGameGrid(gemPrefab, gemContainer, gridMarkerPrefab, markerGrid, barrier, ref keyGemCollected);
        keyGemCollected += OnKeyGemCollected;
    }

    void OnKeyGemCollected()
    {
        collectedKeyGems++;
        keyGemAmntDisplay.text = $"{collectedKeyGems}/5";
        if (collectedKeyGems >= 5)
        {
            for (int i = 0; i < grid.gemContainer.transform.childCount; i++)
            {
                GameObject gem = gemContainer.transform.GetChild(i).gameObject;
                Tween.CompleteAll(gem.transform);
            }
            gameObject.GetComponent<PuzzleCompletionEmitter>().onPuzzleCompleted?.Invoke();
        }
    }
}
