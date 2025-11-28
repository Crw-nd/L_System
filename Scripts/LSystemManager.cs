using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Splines;
public class LSystem_Simple : MonoBehaviour
{
    //Reference List
    private List<LSystemPreset> presets = new List<LSystemPreset>();
    //Reference Class
    private List<LRule> rules;
    private int activePreset = 0;

    private string lsystem;
    [SerializeField] private string axiom;
    [SerializeField] private int iterations;
    Stack<TransformHelper> stack = new Stack<TransformHelper>();
    Stack<int> SplineIndexStack = new Stack<int>();
    TransformHelper helper;

    [SerializeField] public float length;
    [SerializeField] public float angle;
    private List<List<Vector3>> LineList = new List<List<Vector3>>();
    [SerializeField] private Material TreeMaterial;

    //Use Virtual Point to guide mesh creation
    private Vector3 turtlePos;
    private Quaternion turtleRot;

    void Start()
    {
        LoadPresets();
        ApplyPreset(0);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1)) ApplyPreset(0);
        if (Input.GetKeyDown(KeyCode.Alpha2)) ApplyPreset(1);
        if (Input.GetKeyDown(KeyCode.Alpha3)) ApplyPreset(2);
        if (Input.GetKeyDown(KeyCode.Alpha4)) ApplyPreset(3);
        if (Input.GetKeyDown(KeyCode.Alpha5)) ApplyPreset(4);
        if (Input.GetKeyDown(KeyCode.Alpha6)) ApplyPreset(5);
    }

    private void OnDrawGizmos()
    {
        foreach (List<Vector3> line in LineList)
        {
            Gizmos.DrawLine(line[0], line[1]);
        }
    }

    void PatternReading()
    {
        for (int i = 0; i < iterations; i++)
        {
            string next = "";

            foreach (char c in lsystem)
            {
                bool replaced = false;

                foreach (var rule in rules)
                {
                    if (c.ToString() == rule.predecessor)
                    {
                        next += rule.successor;
                        replaced = true;
                        break;
                    }
                }

                if (!replaced)
                    next += c;
            }

            lsystem = next;
        }
    }

    void CreateMesh()
    {
        //Use Virtual Guide to draw mesh
        turtlePos = transform.position;
        turtleRot = Quaternion.identity;
        transform.position = Vector3.zero;
        transform.rotation = Quaternion.identity;

        Vector3 initialPosition;

        GameObject TreeObj = new GameObject(name: "Tree");
        var meshFilter = TreeObj.AddComponent<MeshFilter>();
        meshFilter.mesh = new Mesh();
        var meshRenderer = TreeObj.AddComponent<MeshRenderer>();
        meshRenderer.material = TreeMaterial;

        var container = TreeObj.AddComponent<SplineContainer>();
        container.RemoveSplineAt(0);
        var extrude = TreeObj.AddComponent<SplineExtrude>();
        extrude.Container = container;

        var currentSpline = container.AddSpline();
        var splineIndex = container.Splines.FindIndex(currentSpline);

        currentSpline.Add(new BezierKnot(turtlePos), TangentMode.AutoSmooth);

        foreach (char j in lsystem)
        {
            switch (j)
            {
                case 'F':
                    initialPosition = turtlePos;
                    turtlePos += turtleRot * Vector3.up * length;
                    currentSpline.Add(new BezierKnot(turtlePos), TangentMode.AutoSmooth);
                    LineList.Add(new List<Vector3>() { initialPosition, turtlePos });
                    break;


                case 'B':
                    //do nothing
                    break;
                case '[':
                    stack.Push(new TransformHelper()
                    {
                        position = turtlePos,
                        rotation = turtleRot
                    });

                    SplineIndexStack.Push(splineIndex);

                    int splineCount = currentSpline.Count;
                    int OldSplineIndex = splineIndex;

                    currentSpline = container.AddSpline();
                    splineIndex = container.Splines.FindIndex(currentSpline);

                    currentSpline.Add(new BezierKnot(turtlePos), TangentMode.AutoSmooth);

                    container.LinkKnots(
                        new SplineKnotIndex(OldSplineIndex, splineCount - 1),
                        new SplineKnotIndex(splineIndex, 0)
                    );
                    break;

                case ']':
                    TransformHelper helper = stack.Pop();
                    turtlePos = helper.position;
                    turtleRot = helper.rotation;
                    splineIndex = SplineIndexStack.Pop();
                    currentSpline = container.Splines[splineIndex];
                    break;

                case 'l':
                    turtleRot *= Quaternion.Euler(0, 0, angle);
                    break;
                case 'r':
                    turtleRot *= Quaternion.Euler(0, 0, -angle);
                    break;

            }
        }
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

        // Sort by order field
        presets.Sort((a, b) => a.order.CompareTo(b.order));

        if (presets.Count == 0)
            Debug.LogWarning("No L-system JSON presets found!");
    }


    void ApplyPreset(int index)
    {
        //Used to remove old l system trees and then generate new ones
        GameObject oldTree = GameObject.Find("Tree");
        if (oldTree != null)
            Destroy(oldTree);

        LineList.Clear();
        stack.Clear();
        SplineIndexStack.Clear();

        //Apply new preset
        if (index < 0 || index >= presets.Count) return;

        activePreset = index;

        var p = presets[index];

        axiom = p.axiom;
        iterations = p.iterations;
        angle = p.angle;
        rules = p.rules;

        lsystem = axiom;

        PatternReading();
        CreateMesh();

        Debug.Log("Loaded preset: " + p.name);
    }


}

public static class LsystemExtention
{
    public static int FindIndex(this IReadOnlyList<Spline> splines, Spline spline)
    {
        for (int i = 0; i < splines.Count; i++)
        {
            if (splines[i] == spline)
            {
                return i;
            }
        }
        return -1;
    }
}
