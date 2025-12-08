// ShaderStripperConfig.cs
using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ShaderStripperConfig", menuName = "ShaderStripper/Config")]
public class ShaderStripperConfig : ScriptableObject
{
    public int configVersion = 0;
    public List<ShaderRule> shaderRules = new List<ShaderRule>();
    public List<DirectoryRule> directoryRules = new List<DirectoryRule>();

    public void IncrementVersion()
    {
        configVersion++;
#if UNITY_EDITOR
        UnityEditor.EditorUtility.SetDirty(this);
#endif
    }
}

[Serializable]
public class ShaderRule
{
    public Shader shader;
    public List<StripRule> rules = new List<StripRule>();
}

[Serializable]
public class StripRule
{
    public string ruleName = "New Rule";
    public bool enabled = true;
    public RuleType ruleType = RuleType.SingleKeyword;
    public string keyword1 = "";
    public string keyword2 = "";
}

[Serializable]
public class DirectoryRule
{
    public string directoryName = "Directory";
    public UnityEngine.Object directory;
    public bool enabled = true;
    public List<StripRule> rules = new List<StripRule>();
}

public enum RuleType
{
    SingleKeyword,
    KeywordCombination
}