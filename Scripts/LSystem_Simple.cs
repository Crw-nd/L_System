using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI; // only needed for standard Text
using TMPro;




    public class LSystemManager1 : MonoBehaviour
{

    [Header("UI")]
    [SerializeField] private TextMeshProUGUI Preset;

    [Header("Preset Settings")]
    [Tooltip("Select which preset to load at start (-1 = default Preset A)")]
    [SerializeField] private int activePreset = -1;

    [Header("Tree Settings")]
    [SerializeField] private float length = 0.5f;
    [SerializeField] private float angle = 25.7f;

    [Header("Angle Control")]
    [SerializeField] private float angleStep = 2.5f;
    [SerializeField] private float defaultAngle = 25.7f;

    [SerializeField] private Material lineMaterial;
    [SerializeField] private float lineWidth = 0.05f;

    private List<LSystemPreset> presets = new List<LSystemPreset>();
    private List<LRule> rules;
    private string lsystem;

    private Stack<TransformHelper> stack = new Stack<TransformHelper>();
    private List<GameObject> lineObjects = new List<GameObject>();

    private Vector3 turtlePos;
    private Quaternion turtleRot;

    void Start()
    {
        LoadPresets();

        // If Inspector value invalid, fallback to Preset A (order = 3)
        if (activePreset < 0 || activePreset >= presets.Count)
        {
            activePreset = presets.FindIndex(p => p.order == 3);
            if (activePreset == -1) activePreset = 0; // fallback
        }

        ApplyPreset(activePreset);
    }

    void Update()
    {

        if (Input.GetKeyDown(KeyCode.Q))
        {
            angle -= angleStep;
            RegenerateTree();
            Debug.Log("Angle Decreased: " + angle);
        }

        if (Input.GetKeyDown(KeyCode.W))
        {
            angle = defaultAngle;
            RegenerateTree();
            Debug.Log("Angle Reset: " + angle);
        }

        if (Input.GetKeyDown(KeyCode.E))
        {
            angle += angleStep;
            RegenerateTree();
            Debug.Log("Angle Increased: " + angle);
        }

        // Use hotkeys 1–6 to select specific presets
        if (Input.GetKeyDown(KeyCode.Alpha1)) { Debug.Log("Hotkey 1 pressed"); ApplyPreset(0); }
        if (Input.GetKeyDown(KeyCode.Alpha2)) { Debug.Log("Hotkey 2 pressed"); ApplyPreset(1); }
        if (Input.GetKeyDown(KeyCode.Alpha3)) { Debug.Log("Hotkey 3 pressed"); ApplyPreset(2); }
        if (Input.GetKeyDown(KeyCode.Alpha4)) { Debug.Log("Hotkey 4 pressed"); ApplyPreset(3); }
        if (Input.GetKeyDown(KeyCode.Alpha5)) { Debug.Log("Hotkey 5 pressed"); ApplyPreset(4); }
        if (Input.GetKeyDown(KeyCode.Alpha6)) { Debug.Log("Hotkey 6 pressed"); ApplyPreset(5); }
        if (Input.GetKeyDown(KeyCode.Alpha7)) { Debug.Log("Hotkey 7 pressed"); ApplyPreset(6); }
        if (Input.GetKeyDown(KeyCode.Alpha8)) { Debug.Log("Hotkey 8 pressed"); ApplyPreset(7); }



        if (Input.GetKeyDown(KeyCode.Alpha1)) SetLetter("Preset F " +
            "Rule: X>F[[X]+X]+F[+FX]-X | F>FF");
        if (Input.GetKeyDown(KeyCode.Alpha2)) SetLetter("Preset E " +
            "Rule: X>F[+X][-X]FX | F>FF");
        if (Input.GetKeyDown(KeyCode.Alpha3)) SetLetter("Preset D " +
            "Rule: X>F[+X]F[-X]+X | F>FF");
        if (Input.GetKeyDown(KeyCode.Alpha4)) SetLetter("Preset A " +
            "Rule: F>F[+F]F[-F]F");
        if (Input.GetKeyDown(KeyCode.Alpha5)) SetLetter("Preset B " +
            "Rule: F>F[+F]F[-F][F]");
        if (Input.GetKeyDown(KeyCode.Alpha6)) SetLetter("Preset C " +
            "Rule: F>FF-[-F+F+F]+[+F-F-F]");
        if (Input.GetKeyDown(KeyCode.Alpha7)) SetLetter("Preset H " +
            "Rule: F-[ [X]+X]+F[+FX]-X");
        if (Input.GetKeyDown(KeyCode.Alpha8)) SetLetter("Preset G " +
            "Rule: F[+X]F[-X]+X");

    }

    void SetLetter(string letter)
    {
        if (Preset != null)
        {
            Preset.text = letter;
        }

        Debug.Log("Preset letter changed to: " + letter);
    }

    void LoadPresets()
    {
        presets.Clear();
        string folder = Path.Combine(Application.streamingAssetsPath, "LSystems");
        var files = Directory.GetFiles(folder, "*.json");

        foreach (var file in files)
        {
            string json = File.ReadAllText(file);
            LSystemPreset preset = JsonUtility.FromJson<LSystemPreset>(json);
            presets.Add(preset);
        }

        if (presets.Count == 0)
            Debug.LogWarning("No L-system JSON presets found!");
    }

    string GenerateLSystemString(LSystemPreset preset)
    {
        string current = preset.axiom;

        for (int i = 0; i < preset.iterations; i++)
        {
            string next = "";
            foreach (char c in current)
            {
                bool replaced = false;
                foreach (var rule in preset.rules)
                {
                    if (c.ToString() == rule.predecessor)
                    {
                        next += rule.successor;
                        replaced = true;
                        break;
                    }
                }
                if (!replaced) next += c;
            }
            current = next;
        }

        return current;
    }

    void ApplyPreset(int index)
    {
        if (index < 0 || index >= presets.Count) return;
        activePreset = index;

        // Clear old lines
        foreach (var obj in lineObjects)
            Destroy(obj);
        lineObjects.Clear();
        stack.Clear();

        var preset = presets[index];
        rules = preset.rules;
        defaultAngle = preset.angle;
        angle = defaultAngle;

        // Generate L-System string
        lsystem = GenerateLSystemString(preset);

        DrawLSystem();

        Debug.Log("Loaded preset: " + preset.name);
    }

    void DrawLSystem()
    {
        turtlePos = Vector3.zero;
        turtleRot = Quaternion.identity;

        foreach (char c in lsystem)
        {
            switch (c)
            {
                case 'F':
                    Vector3 start = turtlePos;
                    turtlePos += turtleRot * Vector3.up * length;
                    CreateLine(start, turtlePos);
                    break;

                case '[':
                    stack.Push(new TransformHelper { position = turtlePos, rotation = turtleRot });
                    break;

                case ']':
                    var helper = stack.Pop();
                    turtlePos = helper.position;
                    turtleRot = helper.rotation;
                    break;

                case 'l':
                    turtleRot *= Quaternion.Euler(0, 0, angle);
                    break;

                case 'r':
                    turtleRot *= Quaternion.Euler(0, 0, -angle);
                    break;

                case 'B':
                    break;
            }
        }
    }

    void CreateLine(Vector3 start, Vector3 end)
    {
        GameObject lineObj = new GameObject("LineSegment");
        LineRenderer lr = lineObj.AddComponent<LineRenderer>();
        lr.material = lineMaterial;
        lr.positionCount = 2;
        lr.SetPosition(0, start);
        lr.SetPosition(1, end);
        lr.startWidth = lineWidth;
        lr.endWidth = lineWidth;
        lineObjects.Add(lineObj);
    }

    void RegenerateTree()
    {
        if (presets.Count == 0) return;

        // Clear old lines
        foreach (var obj in lineObjects)
            Destroy(obj);

        lineObjects.Clear();
        stack.Clear();

        // Regenerate using same preset, new angle
        var preset = presets[activePreset];
        lsystem = GenerateLSystemString(preset);
        DrawLSystem();
    }

}
