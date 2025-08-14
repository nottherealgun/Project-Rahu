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

public class GemGameManager : SerializedMonoBehaviour
{
    [OdinSerialize][AssetsOnly] GameObject gemPrefab;
    [OdinSerialize][SceneObjectsOnly] GameObject gemContainer;
    [OdinSerialize][AssetsOnly] GameObject gridMarkerPrefab;
    [OdinSerialize][SceneObjectsOnly] GameObject markerGrid;
    [OdinSerialize, SceneObjectsOnly] GameObject barrier;

    int rows = 9;
    int columns = 5;
    float swapSpeed = 0.2f;

    [TableMatrix(HorizontalTitle = "Initial Gem Setup")]
    [SerializeField]
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
    [OdinSerialize][ReadOnly] List<GameObject> selectedGems = new List<GameObject>();

    [TableMatrix(HorizontalTitle = "Current Gem Matrix")]
    [OdinSerialize, ReadOnly]
    List<List<GameObject>> grid = new List<List<GameObject>>();

    [OdinSerialize] TMP_Text keyGemAmntDisplay;
    [HideInInspector] public int collectedKeyGems = 0;
    [HideInInspector] public Action keyGemCollected;

    void Start()
    {
        Setup();
    }

    void Setup()
    {
        keyGemCollected += () => OnKeyGemCollected();

        // STEP 1: setup grid
        /* Initializes grid gems from matrix */
        for (int i = 0; i < rows; i++)
        {
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
            }
        }

        UpdateGrid();
    }

    // private void LateUpdate()
    // {
    //     /* Updates the grid every frame */
    //     UpdateGrid();
    // }

    void SimulateGravity()
    {
        /* Simulates gravity by moving gems down to fill empty spaces */
        for (int j = 0; j < columns; j++)
        {
            for (int i = rows - 1; i >= 0; i--)
            {
                if (grid[i][j] == null && ReferenceEquals(grid[i][j], null))
                {
                    // find the next gem above
                    for (int k = i - 1; k >= 0; k--)
                    {
                        if (grid[k][j] != null & !ReferenceEquals(grid[k][j], null))
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
        UpdateGrid();
    }

    void UpdateGrid()
    {
        /* Updates the grid with current gems */

        grid.Clear();

        for (int i = 0; i < rows; i++)
        {
            grid.Add(new List<GameObject>());
            for (int j = 0; j < columns; j++)
            {
                // add null to grid
                grid[i].Add(null);
            }
        }

        for (int i = 0; i < gemContainer.transform.childCount; i++)
        {
            GameObject gem = gemContainer.transform.GetChild(i).gameObject;
            Vector2Int pos = gem.GetComponent<Gem>().gemPosition;
            grid[pos.x][pos.y] = gem;
        }
    }

    GameObject CreateGem(Vector2Int gridPos, int gemType)
    {
        // creates gem instance at {position} with {gemType} gem type
        Vector3 position = markerGrid.transform.GetChild((gridPos.x * columns) + gridPos.y).transform.position;
        GameObject _gemPrefab = Instantiate(gemPrefab, position, Quaternion.identity, gemContainer.transform);
        _gemPrefab.GetComponent<Gem>().SetGemType(gemType);
        _gemPrefab.transform.SetAsFirstSibling();
        _gemPrefab.GetComponent<Button>().onClick.AddListener(() => OnGemClick(_gemPrefab));
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
        _gemPrefab.GetComponent<Button>().onClick.AddListener(() => OnGemClick(_gemPrefab));
        return _gemPrefab;
    }

    public void OnGemClick(GameObject gem)
    {
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
            SwapGems();
            // reset selected gems
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

    void SwapGems(bool recursive = true)
    {
        /* swaps the selected gems */
        if (selectedGems.Count != 2)
        {
            return;
        }
        ;

        Vector2Int pos1 = GetGemPosition(selectedGems[0]);
        Vector2Int pos2 = GetGemPosition(selectedGems[1]);

        if (pos1 == pos2)
        {
            return;
        }
        ; // no swap if same position

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

        Sequence.Create()
            .ChainCallback(() =>
            {
                // print($"{gem1Pos} and {gem2Pos}");
                Tween.Position(gem1.transform, gem2Pos, swapSpeed, Ease.OutCubic);
                Tween.Position(gem2.transform, gem1Pos, swapSpeed, Ease.OutCubic);
            })
            .OnComplete(() =>
            {
                // check for matches after swap
                UpdateGrid();
                if (recursive) CheckForMatches(true);
            });
    }

    List<GameObject> FindHorizontalMatches() // returns indices of Matches in the hierarchy (1 to rows*columns)
    {
        List<GameObject> Matches = new List<GameObject>();

        for (int rowIdx = 0; rowIdx < rows; rowIdx++)
        {
            List<GameObject> connectedGems = new List<GameObject>();
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

                connectedGems.Add(currentGem);

                if (colIdx == columns - 1 && connectedGems.Count > 2)
                {
                    Matches.AddRange(connectedGems);
                }
            }
        }
        return Matches;
    }

    List<GameObject> FindVerticalMatches()
    {
        List<GameObject> Matches = new List<GameObject>();

        for (int colIdx = 0; colIdx < columns; colIdx++)
        {
            List<GameObject> connectedGems = new List<GameObject>();
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

                connectedGems.Add(currentGem);

                if (rowIdx == rows - 1 && connectedGems.Count > 2)
                {
                    Matches.AddRange(connectedGems);
                }
            }
        }
        return Matches;
    }

    List<GameObject> FindMatches()
    {
        List<GameObject> Matches = new List<GameObject>();
        var set = new HashSet<GameObject>();
        foreach (var gem in FindHorizontalMatches()) set.Add(gem);
        foreach (var gem in FindVerticalMatches()) set.Add(gem);
        Matches = set.ToList();

        Matches.RemoveAll(gem =>
            gem == null ||
            ReferenceEquals(gem, null) ||
            gem.GetComponent<Gem>() == null ||
            gem.GetComponent<Gem>().gemType == Gem.GemTypes.TYPE5
        );


        return Matches;
    }

    void CheckForMatches(bool checkFromSwap = false)
    {

        List<GameObject> matchedGems = FindMatches();
        if (matchedGems.Count == 0)
        {
            // if (checkFromSwap)
            // {
            //     // print("No matches found after swap, reverting gems.");
            //     Tween.CompleteAll(selectedGems[0].transform);
            //     Tween.CompleteAll(selectedGems[1].transform);
            //     SwapGems(false);
            // }
            return;
        }

        barrier.SetActive(true);

        // destroy matched gems
        foreach (GameObject gem in matchedGems)
        {
            if (gem.GetComponent<Gem>().gemType == Gem.GemTypes.TYPE5)
            {
                continue;
            }
            gem.GetComponent<Gem>().MarkForDestroy();
            Tween.CompleteAll(gem.transform);
            Destroy(gem);
        }

        UpdateGrid();

        Sequence.Create()
            .ChainDelay(swapSpeed)
            .ChainCallback(() =>
            {
                SimulateGravity();
                RefillGrid();
                AnimateVisuals();
            })
            .ChainDelay(swapSpeed)
            .ChainCallback(() =>
            {
                // check for matches again after gravity and refill
                CheckForMatches();
                CheckForKeyGems();
            });
    }

    void RefillGrid()
    {
        /* refills the grid with new gems */
        while (gemContainer.transform.childCount < rows * columns)
        {
            // find first empty position in grid
            for (int i = 0; i < rows; i++)
            {
                for (int j = 0; j < columns; j++)
                {
                    if (grid[i][j] == null)
                    {
                        GameObject newGem = CreateGem(new Vector2Int(0, j));
                        grid[i][j] = newGem;
                        newGem.GetComponent<Gem>().gemPosition = new Vector2Int(i, j);
                        break;
                    }
                }
            }
        }
        UpdateGrid();
    }

    void AnimateVisuals()
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
                    Tween.Position(grid[i][j].transform, targetPosition, swapSpeed, Ease.OutCubic);
                }
            }
        }
        sequence
            .ChainDelay(swapSpeed)
            .OnComplete(() =>
            {
                barrier.SetActive(false);
            });
    }

    Action gemDestroy;

    void CheckForKeyGems()
    {
        bool keyGemAtBottom = false;
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
                keyGemAtBottom = true;
                gemDestroy += () =>
                {
                    keyGemCollected?.Invoke();
                    Tween.CompleteAll(gem.transform);
                    DestroyImmediate(gem);
                };
                Tween.PositionY(gem.transform, -100f, swapSpeed, Ease.OutCubic);
            }
        }
        if (keyGemAtBottom)
        {
            // Refresh the grid after key gem falls off
            sequence
                .ChainDelay(swapSpeed)
                .ChainCallback(() =>
                {
                    gemDestroy?.Invoke();
                    // UpdateGrid();
                    SimulateGravity();
                    RefillGrid();
                    AnimateVisuals();
                    CheckForKeyGems();
                });
        }
        UpdateGrid();
    }

    // helper functions
    Vector2Int GetGemPosition(GameObject gem)
    {
        return gem.GetComponent<Gem>().gemPosition;
    }

    async Task OnKeyGemCollected()
    {
        collectedKeyGems++;
        keyGemAmntDisplay.text = $"{collectedKeyGems}/5";
        if (collectedKeyGems >= 5)
        {
            for (int i = 0; i < gemContainer.transform.childCount; i++)
            {
                GameObject gem = gemContainer.transform.GetChild(i).gameObject;
                Tween.CompleteAll(gem.transform);
            }
            await Tween.Delay(0.5f);
            gameObject.GetComponent<PuzzleCompletionEmitter>().onPuzzleCompleted?.Invoke();
        }
    }
    
    private void OnDestroy() {
        
    }
}
