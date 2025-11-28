using System.Collections.Generic;

[System.Serializable]
public class LRule
{
    public string predecessor;
    public string successor;
}

[System.Serializable]
public class LSystemPreset
{
    public string name;
    public int order;      
    public string axiom;
    public int iterations;
    public float angle;
    public List<LRule> rules;
}
