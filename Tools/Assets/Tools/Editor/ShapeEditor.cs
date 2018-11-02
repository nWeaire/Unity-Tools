using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Sebastian.Geometry;

[CustomEditor(typeof(ShapeCreator))]
public class ShapeEditor : Editor
{

    private ShapeCreator m_shapeCreator;
    private SelectionInfo m_selectionInfo;
    private bool m_bShapeChangedSinceLastRepaint;

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        string helpMessage = "Left Click to add points.\nShift-Left click on point to delete.\nShift-Left click on empty space to create new shape.";
        EditorGUILayout.HelpBox(helpMessage, MessageType.Info);

        int shapeDeleteIndex = -1;
        m_shapeCreator.showShapesList = EditorGUILayout.Foldout(m_shapeCreator.showShapesList, "Show Shapes List");
        if (m_shapeCreator.showShapesList)
        {
            for (int i = 0; i < m_shapeCreator.shapes.Count; i++)
            {
                GUILayout.BeginHorizontal();
                GUILayout.Label("Shape " + (i + 1));
                GUI.enabled = i != m_selectionInfo.SelectedShapeIndex;
                if (GUILayout.Button("Select"))
                {
                    m_selectionInfo.SelectedShapeIndex = i;
                }
                GUI.enabled = true;
                if (GUILayout.Button("Delete"))
                {
                    shapeDeleteIndex = i;
                }
                GUILayout.EndHorizontal();
            }
        }

        if(shapeDeleteIndex != -1)
        {
            Undo.RecordObject(m_shapeCreator, "Delete Shape");
            m_shapeCreator.shapes.RemoveAt(shapeDeleteIndex);
            m_selectionInfo.SelectedShapeIndex = Mathf.Clamp(m_selectionInfo.SelectedShapeIndex, 0, m_shapeCreator.shapes.Count - 1);
        }

        if(GUI.changed)
        {
            m_bShapeChangedSinceLastRepaint = true;
            SceneView.RepaintAll();
        }
    }

    private void OnSceneGUI()
    {
        Event guiEvent = Event.current;

        if (guiEvent.type == EventType.Repaint)
        {
            Draw();
        }
        else if (guiEvent.type == EventType.Layout)
        {
            HandleUtility.AddDefaultControl(GUIUtility.GetControlID(FocusType.Passive));
        }
        else
        {
            Input(guiEvent);
            if (m_bShapeChangedSinceLastRepaint)
            {
                HandleUtility.Repaint();
            }
        }
    }

    void CreateNewShape()
    {
        Undo.RecordObject(m_shapeCreator, "Create Shape");
        m_shapeCreator.shapes.Add(new Shape());
        m_selectionInfo.SelectedShapeIndex = m_shapeCreator.shapes.Count - 1;
        m_bShapeChangedSinceLastRepaint = true;
    }

    void CreateNewPoint(Vector3 position)
    {
        bool mouseIsOverSelectedShape = m_selectionInfo.MouseOverShapeIndex == m_selectionInfo.SelectedShapeIndex;
        int newPointIndex = (m_selectionInfo.mouseIsOverLine && mouseIsOverSelectedShape) ? m_selectionInfo.lineIndex + 1 : SelectedShape.points.Count;
        Undo.RecordObject(m_shapeCreator, "Add Point");
        SelectedShape.points.Insert(newPointIndex, position);
        m_selectionInfo.PointIndex = newPointIndex;
        m_bShapeChangedSinceLastRepaint = true;
        m_selectionInfo.MouseOverShapeIndex = m_selectionInfo.SelectedShapeIndex;
        SelectionPointUnderMouse();
    }

    void SelectionPointUnderMouse()
    {
        m_selectionInfo.pointIsSelected = true;
        m_selectionInfo.mouseIsOverPoint = true;
        m_selectionInfo.mouseIsOverLine = false;
        m_selectionInfo.lineIndex = -1;

        m_selectionInfo.positionAtStartOfDrag = SelectedShape.points[m_selectionInfo.PointIndex];
        m_bShapeChangedSinceLastRepaint = true;
    }

    void Input(Event guiEvent)
    {
        Ray mouseRay = HandleUtility.GUIPointToWorldRay(guiEvent.mousePosition);

        float DrawPlaneHeight = 0;
        float DistToDrawPlane = (DrawPlaneHeight - mouseRay.origin.y) / mouseRay.direction.y;
        Vector3 mousePosition = mouseRay.GetPoint(DistToDrawPlane);

        if (guiEvent.type == EventType.MouseDown && guiEvent.button == 0 && guiEvent.modifiers == EventModifiers.None)
        {
            LeftMouseDown(mousePosition);
        }

        if (guiEvent.type == EventType.MouseDown && guiEvent.button == 0 && guiEvent.modifiers == EventModifiers.Shift)
        {
            ShiftLeftMouseDown(mousePosition);
        }

        if (guiEvent.type == EventType.MouseUp && guiEvent.button == 0)
        {
            LeftMouseUp(mousePosition);
        }

        if (guiEvent.type == EventType.MouseDrag && guiEvent.button == 0 && guiEvent.modifiers == EventModifiers.None)
        {
            LeftMouseDrag(mousePosition);
        }

        if(!m_selectionInfo.pointIsSelected)
        {
            UpdateMouseHoverSelection(mousePosition);
        }
    }

    void Draw()
    {
        for (int shapeIndex = 0; shapeIndex < m_shapeCreator.shapes.Count; shapeIndex++)
        {
            Shape shapeToDraw = m_shapeCreator.shapes[shapeIndex];
            bool shapeIsSelected = shapeIndex == m_selectionInfo.SelectedShapeIndex;
            bool mouseIsOverShape = shapeIndex == m_selectionInfo.MouseOverShapeIndex;
            Color deselectedShapeColor = Color.gray;
            for (int i = 0; i < shapeToDraw.points.Count; i++)
            {
                Vector3 nextPoint = shapeToDraw.points[(i + 1) % shapeToDraw.points.Count];
                if (i == m_selectionInfo.lineIndex && mouseIsOverShape)
                {
                    Handles.color = Color.red;
                    Handles.DrawLine(shapeToDraw.points[i], nextPoint);
                }
                else
                {
                    Handles.color = (shapeIsSelected) ? Color.black : deselectedShapeColor;
                    Handles.DrawDottedLine(shapeToDraw.points[i], nextPoint, 4);
                }

                if (i == m_selectionInfo.PointIndex && mouseIsOverShape)
                {
                    Handles.color = (m_selectionInfo.pointIsSelected) ? Color.black : Color.red;
                }
                else
                {
                    Handles.color = (shapeIsSelected) ? Color.white : Color.gray;
                }

                Handles.DrawSolidDisc(shapeToDraw.points[i], Vector3.up, m_shapeCreator.pointRadius);
            }
        }

        if(m_bShapeChangedSinceLastRepaint)
        {
            m_shapeCreator.UpdateMeshDisplay();
        }

        m_bShapeChangedSinceLastRepaint = false;
    }

    void UpdateMouseHoverSelection(Vector3 mousePosition)
    {
        int mouseHoverIndex = -1;
        int mouseOverShapeIndex = -1;
        for (int shapeIndex = 0; shapeIndex < m_shapeCreator.shapes.Count; shapeIndex++)
        {
            Shape currentShape = m_shapeCreator.shapes[shapeIndex];
            for (int i = 0; i < currentShape.points.Count; i++)
            {
                if (Vector3.Distance(mousePosition, currentShape.points[i]) < m_shapeCreator.pointRadius)
                {
                    mouseHoverIndex = i;
                    mouseOverShapeIndex = shapeIndex;
                    break;
                }
            }
        }
        if (mouseHoverIndex != m_selectionInfo.PointIndex || mouseOverShapeIndex != m_selectionInfo.MouseOverShapeIndex)
        {
            m_selectionInfo.MouseOverShapeIndex = mouseOverShapeIndex;
            m_selectionInfo.PointIndex = mouseHoverIndex;
            m_selectionInfo.mouseIsOverPoint = mouseHoverIndex != -1;

            m_bShapeChangedSinceLastRepaint = true;
        }

        if (m_selectionInfo.mouseIsOverPoint)
        {
            m_selectionInfo.mouseIsOverLine = false;
            m_selectionInfo.lineIndex = -1;
        }
        else
        {
            int mouseOverLineIndex = -1;
            float closetLineDist = m_shapeCreator.pointRadius;
            for (int shapeIndex = 0; shapeIndex < m_shapeCreator.shapes.Count; shapeIndex++)
            {
                Shape currentShape = m_shapeCreator.shapes[shapeIndex];
                for (int i = 0; i < currentShape.points.Count; i++)
                {
                    Vector3 nextPointInShape = currentShape.points[(i + 1) % currentShape.points.Count];
                    float DistFromMouseToLine = HandleUtility.DistancePointToLineSegment(mousePosition.ToXZ(), currentShape.points[i].ToXZ(), nextPointInShape.ToXZ());
                    if (DistFromMouseToLine < closetLineDist)
                    {
                        closetLineDist = DistFromMouseToLine;
                        mouseOverLineIndex = i;
                        mouseOverShapeIndex = shapeIndex;
                    }
                }
            }

            if(m_selectionInfo.lineIndex != mouseOverLineIndex || mouseOverShapeIndex != m_selectionInfo.MouseOverShapeIndex)
            {
                m_selectionInfo.MouseOverShapeIndex = mouseOverShapeIndex;
                m_selectionInfo.lineIndex = mouseOverLineIndex;
                m_selectionInfo.mouseIsOverLine = mouseOverLineIndex != -1;
                m_bShapeChangedSinceLastRepaint = true;
            }
        }
    }

    void LeftMouseDown(Vector3 mousePostion)
    {
        if(m_shapeCreator.shapes.Count == 0)
        {
            CreateNewShape();
        }

        SelectShapeUnderMouse();

        if (m_selectionInfo.mouseIsOverPoint)
        {
            SelectionPointUnderMouse();
        }
        else
        {
            CreateNewPoint(mousePostion);
        }
    }
    
    void ShiftLeftMouseDown(Vector3 mousePosition)
    {
        if(m_selectionInfo.mouseIsOverPoint)
        {
            SelectShapeUnderMouse();
            DeletePointUnderMouse();
        }
        else
        {
            CreateNewShape();
            CreateNewPoint(mousePosition);
        }
    }

    void LeftMouseUp(Vector3 mousePostion)
    {
        if (m_selectionInfo.pointIsSelected)
        {
            SelectedShape.points[m_selectionInfo.PointIndex] = m_selectionInfo.positionAtStartOfDrag;
            Undo.RecordObject(m_shapeCreator, "Move Point");
            SelectedShape.points[m_selectionInfo.PointIndex] = mousePostion;
            m_selectionInfo.pointIsSelected = false;
            m_selectionInfo.PointIndex = -1;
            m_bShapeChangedSinceLastRepaint = true;
        }
    }

    void LeftMouseDrag(Vector3 mousePostion)
    {
        if (m_selectionInfo.pointIsSelected)
        {
            SelectedShape.points[m_selectionInfo.PointIndex] = mousePostion;
            m_bShapeChangedSinceLastRepaint = true;
        }
    }

    void DeletePointUnderMouse()
    {
        Undo.RecordObject(m_shapeCreator, "Delete Point");
        SelectedShape.points.RemoveAt(m_selectionInfo.PointIndex);
        m_selectionInfo.mouseIsOverPoint = false;
        m_selectionInfo.pointIsSelected = false;
        m_bShapeChangedSinceLastRepaint = true;
    }

    private void OnEnable()
    {
        m_shapeCreator = target as ShapeCreator;
        m_selectionInfo = new SelectionInfo();
        Undo.undoRedoPerformed += OnUndoOrRedo;
        Tools.hidden = true;
        m_bShapeChangedSinceLastRepaint = true;
    }

    private void OnDisable()
    {
        Undo.undoRedoPerformed -= OnUndoOrRedo;
        Tools.hidden = false;
    }

    void SelectShapeUnderMouse()
    {
        if(m_selectionInfo.MouseOverShapeIndex != -1)
        {
            m_selectionInfo.SelectedShapeIndex = m_selectionInfo.MouseOverShapeIndex;
            m_bShapeChangedSinceLastRepaint = true;
        }
    }

    Shape SelectedShape
    {
        get
        {
            return m_shapeCreator.shapes[m_selectionInfo.SelectedShapeIndex];
        }
    }

    void OnUndoOrRedo()
    {
        if(m_selectionInfo.SelectedShapeIndex >= m_shapeCreator.shapes.Count || m_selectionInfo.SelectedShapeIndex == -1)
        {
            m_selectionInfo.SelectedShapeIndex = m_shapeCreator.shapes.Count - 1;
        }
        m_bShapeChangedSinceLastRepaint = true;
    }

    public class SelectionInfo
    {
        public int SelectedShapeIndex;
        public int MouseOverShapeIndex;

        public int PointIndex = -1;
        public bool mouseIsOverPoint;
        public bool pointIsSelected;

        public Vector3 positionAtStartOfDrag;

        public int lineIndex = -1;
        public bool mouseIsOverLine;
    }

}
