using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
public class CreateGrid : ScriptableWizard
{
    public int GridWidth = 0;
    public int GridHeight = 0;
    public int GridDepth = 0;
    public bool Hollow = false;
    public Vector3 CellDimensions = Vector3.zero;
    private GameObject[,,] m_gGrid = null;
    private GameObject m_gGridParent;
    private GameObject[] m_gLayers;
    [MenuItem("Tools/Map Creator Tools/Create Grid")]
    static void GridSelection()
    {
        ScriptableWizard.DisplayWizard<CreateGrid>("Create Grid", "Create");
    }

    private void GridCreate(int gridWidth, int gridHeight, int gridDepth, Vector3 cellDimensions, bool hollow)
    {
        m_gGrid = new GameObject[gridWidth, gridHeight, gridDepth];
        m_gLayers = new GameObject[gridHeight];
        m_gGridParent = new GameObject("Grid_Parent");
        for (int i = 0; i < gridHeight; i++)
        {
            m_gLayers[i] = new GameObject("Layer" + i);
            m_gLayers[i].transform.SetParent(m_gGridParent.transform);
        }
        for (int i = 0; i < gridWidth; i++)
        {
            for (int j = 0; j < gridHeight; j++)
            {
                for (int k = 0; k < gridDepth; k++)
                {
                    if (hollow)
                    {
                        if (i == 0 || j == 0 || k == 0 || i == gridWidth - 1 || j == gridHeight - 1 || k == gridDepth - 1)
                        {
                            //m_gGrid[i, j, k] = Instantiate(GameObject.CreatePrimitive(PrimitiveType.Cube),
                            //m_gGridParent.transform.position + new Vector3(cellDimensions.x * i, cellDimensions.y * j, cellDimensions.z * k),
                            // Quaternion.identity,
                            // m_gLayers[j].transform);

                            m_gGrid[i, j, k] = GameObject.CreatePrimitive(PrimitiveType.Cube);
                            m_gGrid[i, j, k].transform.position = m_gGridParent.transform.position + new Vector3(cellDimensions.x * i, cellDimensions.y * j, cellDimensions.z * k);
                            m_gGrid[i, j, k].transform.SetParent(m_gLayers[j].transform);
                            m_gGrid[i, j, k].transform.localScale = cellDimensions;
                        }
                    }
                    else
                    {
                        //m_gGrid[i, j, k] = Instantiate(GameObject.CreatePrimitive(PrimitiveType.Cube),
                        //m_gGridParent.transform.position + new Vector3(cellDimensions.x * i, cellDimensions.y * j, cellDimensions.z * k),
                        //Quaternion.identity,
                        //m_gLayers[j].transform);
                        //m_gGrid[i, j, k].transform.localScale = cellDimensions;

                        m_gGrid[i, j, k] = GameObject.CreatePrimitive(PrimitiveType.Cube);
                        m_gGrid[i, j, k].transform.position = m_gGridParent.transform.position + new Vector3(cellDimensions.x * i, cellDimensions.y * j, cellDimensions.z * k);
                        m_gGrid[i, j, k].transform.SetParent(m_gLayers[j].transform);
                        m_gGrid[i, j, k].transform.localScale = cellDimensions;
                    }
                }
            }
        }
    }

    void OnWizardCreate()
    {
        GridCreate(GridWidth, GridHeight, GridDepth, CellDimensions, Hollow);
    }

}
