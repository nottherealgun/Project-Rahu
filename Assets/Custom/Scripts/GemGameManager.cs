using System.Collections.Generic;
using System.Linq;
using PrimeTween;
using UnityEngine;
using UnityEngine.UI;

public class GemGameManager : MonoBehaviour
{
    [SerializeField] GameObject gemPrefab;
    [SerializeField] GameObject gemContainer;
    [SerializeField] GameObject gridMarkerPrefab;
    [SerializeField] GameObject markerGrid;
    [SerializeField] List<GameObject> selectedGems = new List<GameObject>();
    int rows = 8;
    int columns = 5;

    float swapSpeed = 0.2f;

    List<List<GameObject>> grid = new List<List<GameObject>>();

    void Start()
    {
        /* Initializes grid gems */
        for (int i = 0; i < markerGrid.transform.childCount; i++)
        {
            Transform _marker = markerGrid.transform.GetChild(i);
            CreateGem(_marker.transform.position, Random.Range(0, 4));
        }
        UpdateGrid();
    }

    void UpdateGrid()
    {
        grid = getGemObjectMapping();
    }

    void CreateGem(Vector3 position, int gemType)
    {
        // creates gem instance at {position} with {gemType} gem type
        GameObject _gemPrefab = Instantiate(gemPrefab, gemContainer.transform);
        _gemPrefab.GetComponent<Gem>().SetGemType(gemType);
        _gemPrefab.transform.SetPositionAndRotation(position, Quaternion.identity);
        _gemPrefab.name = $"Gem_{_gemPrefab.transform.GetSiblingIndex().ToString()}";
        _gemPrefab.GetComponent<Button>().onClick.AddListener(() => OnGemClick(_gemPrefab));
    }

    void CreateGem(int gemType)
    {
        // creates gem instance at with {gemType} gem type
        GameObject _gemPrefab = Instantiate(gemPrefab, gemContainer.transform);
        _gemPrefab.GetComponent<Gem>().SetGemType(gemType);
        _gemPrefab.name = $"Gem_{_gemPrefab.transform.GetSiblingIndex().ToString()}";
        _gemPrefab.GetComponent<Button>().onClick.AddListener(() => OnGemClick(_gemPrefab));
    }

    void CreateGem()
    {
        // creates gem instance
        GameObject _gemPrefab = Instantiate(gemPrefab, gemContainer.transform);
        _gemPrefab.GetComponent<Gem>().SetGemType(Gem.GetRandomGemType());
        _gemPrefab.name = $"Gem_{_gemPrefab.transform.GetSiblingIndex().ToString()}";
        _gemPrefab.GetComponent<Button>().onClick.AddListener(() => OnGemClick(_gemPrefab));
        _gemPrefab.transform.SetAsFirstSibling();
    }

    public void OnGemClick(GameObject gem)
    {
        /* handles gem click event */
        if (selectedGems.Count == 2) selectedGems.Clear();
        if (selectedGems.Count == 0 || (selectedGems.Count > 0 && selectedGems.Last<GameObject>() != gem))
        {
            selectedGems.Add(gem);
        }
        if (HandleGemNeighborCheck())
        {
            UpdateGrid();
            HandleGemSwap();
        }
    }

    bool HandleGemNeighborCheck()
    {
        /* handles gem neighbor checking logic */
        if (selectedGems.Count != 2) return false;

        GameObject gem1 = selectedGems[0];
        GameObject gem2 = selectedGems[1];
        int idx1 = gem1.transform.GetSiblingIndex();
        int idx2 = gem2.transform.GetSiblingIndex();

        // Register conditions
        bool areHorizontallyParallel = Mathf.Abs(idx1 - idx2) == 1;
        bool areVerticallyParallel = Mathf.Abs(idx1 - idx2) == columns;

        if (areHorizontallyParallel || areVerticallyParallel)
            return true;

        return false;
    }

    void HandleGemSwap()
    {
        /* handles gem swapping logic */

        if (selectedGems.Count != 2) return;
        GameObject gem1 = selectedGems[0];
        GameObject gem2 = selectedGems[1];
        int idx1 = gem1.transform.GetSiblingIndex();
        int idx2 = gem2.transform.GetSiblingIndex();

        // Set indices
        gem1.transform.SetSiblingIndex(idx2);
        gem2.transform.SetSiblingIndex(idx1);
        // Set positions
        Vector3 tempPos = gem1.transform.position;
        // gem1.transform.position = gem2.transform.position;
        // gem2.transform.position = tempPos;
        Sequence.Create(1, CycleMode.Restart)
            .Group(Tween.Position(gem1.transform, gem2.transform.position, swapSpeed))
            .Group(Tween.Position(gem2.transform, gem1.transform.position, swapSpeed))
            .ChainCallback(() =>
            {
                List<int> candidates = findCandidates();
                ActivateAlignments(candidates);
            });

        selectedGems.Clear();
    }

    List<int> findHorizontalCandidates() // returns indices of candidates in the hierarchy (1 to rows*columns)
    {
        List<int> candidates = new List<int>();
        List<List<int>> connections = new List<List<int>>();
        List<List<Gem.GemTypes>> gemTypeMap = getGemTypeMapping();

        for (int rowIdx = 0; rowIdx < rows; rowIdx++)
        {
            List<int> connectedGemIndices = new List<int>();
            Gem.GemTypes? comparingType = null;
            for (int colIdx = 0; colIdx < columns; colIdx++)
            {
                Gem.GemTypes currentGemType = gemTypeMap[rowIdx][colIdx];
                if (comparingType == null)
                {
                    comparingType = currentGemType;
                }

                if (currentGemType != comparingType)
                {
                    if (connectedGemIndices.Count > 2)
                    {
                        candidates.AddRange(connectedGemIndices);
                    }
                    connectedGemIndices.Clear();
                    comparingType = currentGemType;
                }

                connectedGemIndices.Add((rowIdx * columns) + colIdx);

                if (colIdx == columns - 1 && connectedGemIndices.Count > 2)
                {
                    candidates.AddRange(connectedGemIndices);
                }

            }


        }

        // string a = "";
        // foreach (int c in candidates)
        // {
        //     a += c.ToString() + " ";
        // }
        // print(a);

        return candidates;
    }

    List<int> findVerticalCandidates()
    {
        List<int> candidates = new List<int>();
        List<List<Gem.GemTypes>> gemTypeMap = getGemTypeMapping();

        for (int colIdx = 0; colIdx < columns; colIdx++)
        {
            List<int> connectedGemIndices = new List<int>();
            Gem.GemTypes? comparingType = null;
            for (int rowIdx = 0; rowIdx < rows; rowIdx++)
            {
                Gem.GemTypes currentGemType = gemTypeMap[rowIdx][colIdx];
                if (comparingType == null)
                {
                    comparingType = currentGemType;
                }

                if (currentGemType != comparingType)
                {
                    if (connectedGemIndices.Count > 2)
                    {
                        candidates.AddRange(connectedGemIndices);
                    }
                    connectedGemIndices.Clear();
                    comparingType = currentGemType;
                }

                connectedGemIndices.Add((rowIdx * columns) + colIdx);

                if (rowIdx == rows - 1 && connectedGemIndices.Count > 2)
                {
                    candidates.AddRange(connectedGemIndices);
                }
            }
        }
        return candidates;
    }

    List<int> findCandidates()
    {
        List<int> candidates = findHorizontalCandidates();
        candidates.AddRange(findVerticalCandidates());

        return candidates.Distinct().ToList();
    }

    List<List<Gem.GemTypes>> getGemTypeMapping()
    {
        List<List<Gem.GemTypes>> newGridArray = new List<List<Gem.GemTypes>>();
        List<Gem.GemTypes> newGridRow = new List<Gem.GemTypes>();
        for (int idx = 0; idx < markerGrid.transform.childCount; idx++)
        {
            int localIdx = idx % columns;
            GameObject gemObject = gemContainer.transform.GetChild(idx).gameObject;
            Gem gemScript = gemObject.GetComponent<Gem>();
            
            if (localIdx == 0)
            {
                newGridRow = new List<Gem.GemTypes> { gemScript.gemType };
            }
            else
            {
                newGridRow.Add(gemScript.gemType);
            }
            if (localIdx == columns-1)
            {
                newGridArray.Add(newGridRow);
            }

        }

        // foreach (List<Gem.GemTypes> row in newGridArray)
        // {
        //     string temp = "";
        //     foreach (Gem.GemTypes type in row)
        //     {
        //         temp += (" " + type.ToString());
        //     }
        //     print(temp);
        // }

        // print(newGridArray.Count);
        return newGridArray;
    }

    List<List<GameObject>> getGemObjectMapping()
    {
        List<List<GameObject>> newGridArray = new List<List<GameObject>>();
        List<GameObject> newGridRow = new List<GameObject>();
        for (int idx = 0; idx < markerGrid.transform.childCount; idx++)
        {
            int localIdx = idx % columns;
            GameObject gemObject = gemContainer.transform.GetChild(idx).gameObject;
            Gem gemScript = gemObject.GetComponent<Gem>();
            
            if (localIdx == 0)
            {
                newGridRow = new List<GameObject> { gemObject };
            }
            else
            {
                newGridRow.Add(gemObject);
            }
            if (localIdx == columns-1)
            {
                newGridArray.Add(newGridRow);
            }

        }

        return newGridArray;
    }

    void ActivateAlignments(List<int> connectionIndices)
    {
        foreach (int idx in connectionIndices)
        {
            // Destroy(gemContainer.transform.GetChild(idx).gameObject);
            gemContainer.transform.GetChild(idx).GetComponent<Gem>().MarkForDestroy();
        }
        Sequence.Create(1, CycleMode.Restart)
            .ChainDelay(.1f)
            .ChainCallback(SimulateGravity)
            .ChainCallback(() =>
            {
                foreach (Transform gem in gemContainer.transform)
                {
                    if (gem.gameObject.activeInHierarchy == false) continue;
                    if (gem.gameObject.GetComponent<Gem>().marked) { Destroy(gem.gameObject); }
                }
            })
            .ChainDelay(.1f)
            .ChainCallback(FillEmptyPlaces);
    }

    void FillEmptyPlaces()
    {
        bool empty = false;
        while (gemContainer.transform.childCount < rows * columns)
        {
            empty = true;
            CreateGem();
        }
        if (empty)
        {
            Sequence.Create(1, CycleMode.Restart)
                .ChainCallback(AnimateGemVisuals)
                .ChainCallback(() =>
                {
                    List<int> candidates = findCandidates();
                    ActivateAlignments(candidates);
                });
        }
            
    }

    void SimulateGravity()
    {
        List<List<GameObject>> currentGrid = getGemObjectMapping();

        // Iterate from the second to last row up to the top
        for (int i = rows - 2; i >= 0; i--)
        {
            // Iterate through each column
            for (int j = 0; j < columns; j++)
            {
                // If the gem at the current position is not marked for destruction
                GameObject currentGem = currentGrid[i][j];
                if (currentGem == null || currentGem.GetComponent<Gem>().marked) continue;

                // Check if the gem below it is marked for destruction
                GameObject gemBelow = currentGrid[i + 1][j];
                if (gemBelow != null && gemBelow.GetComponent<Gem>().marked)
                {
                    // Find the lowest available empty space in this column
                    int emptyRow = -1;
                    for (int k = i + 1; k < rows; k++)
                    {
                        if (currentGrid[k][j] == null || currentGrid[k][j].GetComponent<Gem>().marked)
                        {
                            emptyRow = k;
                        }
                    }

                    if (emptyRow != -1)
                    {
                        // Update the grid to reflect the move
                        currentGrid[emptyRow][j] = currentGem;
                        currentGrid[i][j] = gemBelow; // This is the old marked gem that will be destroyed

                        // Set the sibling index of the gem to its new position
                        // This is crucial for the hierarchy-based getGemObjectMapping to work.
                        int oldIndex = (i * columns) + j;
                        int newIndex = (emptyRow * columns) + j;
                        currentGem.transform.SetSiblingIndex(newIndex);
                        gemBelow.transform.SetSiblingIndex(oldIndex);
                        
                    }
                }
            }
        }
        
        // Now we update the main grid and animate the changes
        grid = currentGrid;
        AnimateGemVisuals();
    }

    void AnimateGemVisuals()
    {
        for (int i = 0; i < rows; i++)
        {
            for (int j = 0; j < columns; j++)
            {
                GameObject gem = grid[i][j];
                Transform markerTransform = markerGrid.transform.GetChild((i * columns) + j).transform;
                Tween.Position(gem.transform, markerTransform.position, swapSpeed);
            }
        }
        // for (int idx = 0; idx < rows * columns; idx++)
        // {
        //     Transform markerTransform = markerGrid.transform.GetChild(idx).transform;
        //     // gemContainer.transform.GetChild(idx).position = markerTransform.position;
        //     Tween.Position(gemContainer.transform.GetChild(idx), markerTransform.position, swapSpeed);
        // }
    }
}
