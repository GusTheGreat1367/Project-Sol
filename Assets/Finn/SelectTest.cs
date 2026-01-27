using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
public class SelectTest : MonoBehaviour
{
    public PlayerInput PlayerInput;
    private InputAction mousePosInput;
    private InputAction mouseLeftClick;
    private InputAction mouseRightClick;
    public List<PathFinderAI> selectableObjs = new List<PathFinderAI>();
    public List<PathFinderAI> selectedObjs = new List<PathFinderAI>();
    public List<GameObject> selectableGameObjs = new List<GameObject>();
    Rect selectionRect = new Rect();
    private Vector2 selectionStartPos = new Vector2();
    public Texture2D selectionTexture;
    public Color selectionColor = new Color(0.8f, 0.8f, 0.9f, 0.25f);
    public Color selectedColor = new Color(1.0f, 0.0f, 0.0f, 0.6f);
    private Vector2 mouseScreenStartPos = new Vector2();
    private Rect screenSelectionRect = new Rect();
    UILineRenderer lineRenderer;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        PlayerInput = new PlayerInput();
        mousePosInput = PlayerInput.Main.MousePos;
        mouseLeftClick = PlayerInput.Main.MouseClickLeft;
        mouseRightClick = PlayerInput.Main.MouseClickRight;
    }
    private void Start()
    {
        //for (int i = 0; i < selectableGameObjs.Count; i++)
        //{

        //    PathFinderAI newAI = new PathFinderAI();

        //    newAI.obj = selectableGameObjs[i];
        //    //newAI.obj.gameObject.AddComponent<ObstacleObj>();
        //    newAI.obj.name = "AI " + i;
        //    newAI.instanceID = newAI.obj.gameObject.GetInstanceID();
        //    selectableObjs.Add(newAI);

        //}
        selectionTexture = new Texture2D(1, 1);
        lineRenderer = FindFirstObjectByType(typeof(UILineRenderer)) as UILineRenderer;
    }

    private void OnEnable()
    {
        mousePosInput.Enable();
        mouseLeftClick.Enable();
        mouseRightClick.Enable();
    }
    private void OnDisable()
    {
        mousePosInput.Disable();
        mouseLeftClick.Disable();
        mouseRightClick.Disable();
    }

    private void OnGUI()
    {
        if (mouseLeftClick.IsPressed() && selectionTexture != null)
        {
            GUI.color = selectionColor;
            GUI.DrawTexture(screenSelectionRect, selectionTexture);
        }
        for (int i = 0; i < selectedObjs.Count; i++)
        {
            Renderer selectedRend = selectedObjs[i].obj.GetComponent<Renderer>();
            if (selectedRend != null)
            {
                float left = Camera.main.WorldToScreenPoint(selectedRend.bounds.center - new Vector3(selectedRend.bounds.extents.x, 0, 0)).x;
                float right = Camera.main.WorldToScreenPoint(selectedRend.bounds.center + new Vector3(selectedRend.bounds.extents.x, 0, 0)).x;
                float bottomYUp = Camera.main.WorldToScreenPoint(selectedRend.bounds.center - new Vector3(0, selectedRend.bounds.extents.y, 0)).y;
                float topYUp = Camera.main.WorldToScreenPoint(selectedRend.bounds.center + new Vector3(0, selectedRend.bounds.extents.y, 0)).y;

                float x = left;
                float y = Screen.height - topYUp;
                float width = right - left;
                float height = topYUp - bottomYUp;
                Rect selectedObjScreenSpace = new Rect(x, y, width, height);
                GUI.color = selectedColor;
                GUI.DrawTexture(selectedObjScreenSpace, selectionTexture);

            }
            else
            {
                Debug.LogWarning($"Object {selectedObjs[i].obj.name} does not have a renderer, cannot get bounds of selected object");
                continue;
            }
        }
        GUI.color = Color.white;
    }
    // Update is called once per frame
    void Update()
    {
        lineRenderer.ClearLines();
        Vector2 mouseScreenPos = mousePosInput.ReadValue<Vector2>();
        Vector3 screenPointWithDepth = new Vector3(mouseScreenPos.x, mouseScreenPos.y, 0);
        Vector2 mouseWorldPos = Camera.main.ScreenToWorldPoint(screenPointWithDepth);

        if (mouseLeftClick.WasPressedThisFrame())
        {
            selectionStartPos = mouseWorldPos;
            mouseScreenStartPos = mouseScreenPos;
            selectedObjs.Clear();
        }
        else if (mouseLeftClick.WasReleasedThisFrame())
        {
            selectionRect = new Rect();
            selectionStartPos = new Vector2();
            screenSelectionRect = new Rect();
        }
        if (mouseRightClick.IsPressed())
        {
            selectionRect = new Rect();
            selectionStartPos = new Vector2();
            screenSelectionRect = new Rect();
            for (int i = 0; i < selectedObjs.Count; i++)
            {
                selectedObjs[i].targetPos = mouseWorldPos;
                selectedObjs[i].targetSet = true;
            }
        }
        if (mouseLeftClick.IsPressed())
        {
            Vector2 sizeWorld = mouseWorldPos - selectionStartPos;
            selectionRect = new Rect(selectionStartPos, sizeWorld);
            Vector2 sizeScreen = mouseScreenPos - mouseScreenStartPos;
            screenSelectionRect = new Rect(
                mouseScreenStartPos.x,
                Screen.height - mouseScreenStartPos.y,
                sizeScreen.x,
                -sizeScreen.y
            );
            for (int i = 0; i < selectableObjs.Count; i++)
            {
                if (!selectedObjs.Contains(selectableObjs[i]) && selectionRect.Contains(selectableObjs[i].obj.transform.position, true))
                {
                    selectedObjs.Add(selectableObjs[i]);
                    selectableObjs[i].selected = true;
                }
                else if (selectedObjs.Contains(selectableObjs[i]) && !selectionRect.Contains(selectableObjs[i].obj.transform.position, true))
                {
                    selectedObjs.Remove(selectableObjs[i]);
                    selectableObjs[i].selected = false;
                }
            }
        }
        for (int i = 0; i < selectedObjs.Count; i++)
        {
            if (selectedObjs[i].targetSet)
            {
                lineRenderer.DrawLine(new UILineRenderer.LineSegment
                {
                    Color = Color.green,
                    P1 = Camera.main.WorldToScreenPoint(selectedObjs[i].obj.transform.position),
                    P2 = Camera.main.WorldToScreenPoint(selectedObjs[i].targetPos),
                    Width = 1f
                });
            }
        }

    }
}
