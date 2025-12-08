// ShaderStripperProcessor.cs
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Pipeline.Utilities;
using UnityEditor.Build.Reporting;
using UnityEditor.Rendering;
using UnityEngine;
using UnityEngine.Rendering;

public class ShaderStripperProcessor : IPreprocessShaders, IPreprocessBuildWithReport
{
    private static ShaderStripperConfig config;
    private static int cachedConfigVersion = -1;
    
    public int callbackOrder => 0;

    public void OnPreprocessBuild(BuildReport report)
    {
        config = LoadConfig();
        if (config == null) return;

        if (cachedConfigVersion != config.configVersion)
        {
            Debug.Log($"<color=yellow>[Shader Stripper]</color> Config version changed: {cachedConfigVersion} -> {config.configVersion}");
            
            cachedConfigVersion = config.configVersion;
        }
    }

    public void OnProcessShader(Shader shader, ShaderSnippetData snippet, IList<ShaderCompilerData> data)
    {
        if (config == null)
        {
            config = LoadConfig();
            if (config == null) return;
        }

        string shaderPath = AssetDatabase.GetAssetPath(shader);
        
        for (int i = data.Count - 1; i >= 0; i--)
        {
            ShaderCompilerData compilerData = data[i];
            
            foreach (var shaderRule in config.shaderRules)
            {
                if (shaderRule.shader == shader)
                {
                    if (ShouldStripShaderRule(shader, compilerData, shaderRule.rules))
                    {
                        LogVariantRemoval(shader, snippet, compilerData, $"Shader Rule: {shader.name}");
                        data.RemoveAt(i);
                        goto NextVariant;
                    }
                }
            }

            foreach (var dirRule in config.directoryRules)
            {
                if (!dirRule.enabled) continue;
                if (dirRule.directory == null) continue;

                string dirPath = AssetDatabase.GetAssetPath(dirRule.directory);
                if (string.IsNullOrEmpty(dirPath)) continue;

                if (shaderPath.StartsWith(dirPath))
                {
                    if (ShouldStripDirectoryRule(shader, compilerData, dirRule.rules))
                    {
                        LogVariantRemoval(shader, snippet, compilerData, $"Directory: {dirPath}");
                        data.RemoveAt(i);
                        break;
                    }
                }
            }
            
            NextVariant:;
        }
    }

    private void LogVariantRemoval(Shader shader, ShaderSnippetData snippet, ShaderCompilerData data, string reason)
    {
        var keywords = data.shaderKeywordSet.GetShaderKeywords();
        string kwStr = keywords.Length > 0 
            ? string.Join(" ", keywords.Select(k => k.name)) 
            : "No Keywords";
        
        Debug.Log($"<color=red>[Stripped]</color> {shader.name} | {snippet.passType} | [{kwStr}] | {reason}");
    }

    private bool ShouldStripShaderRule(Shader shader, ShaderCompilerData compilerData, List<StripRule> rules)
    {
        foreach (var rule in rules)
        {
            if (!rule.enabled) continue;

            if (rule.ruleType == RuleType.SingleKeyword)
            {
                if (!string.IsNullOrEmpty(rule.keyword1))
                {
                    if (HasKeyword(shader, compilerData, rule.keyword1))
                        return true;
                }
            }
            else if (rule.ruleType == RuleType.KeywordCombination)
            {
                if (!string.IsNullOrEmpty(rule.keyword1) && !string.IsNullOrEmpty(rule.keyword2))
                {
                    if (HasKeyword(shader, compilerData, rule.keyword1) && 
                        HasKeyword(shader, compilerData, rule.keyword2))
                        return true;
                }
            }
        }
        return false;
    }

    private bool ShouldStripDirectoryRule(Shader shader, ShaderCompilerData compilerData, List<StripRule> rules)
    {
        return ShouldStripShaderRule(shader, compilerData, rules);
    }

    private bool HasKeyword(Shader shader, ShaderCompilerData compilerData, string keyword)
    {
        var globalKeyword = new ShaderKeyword(keyword);
        var localKeyword = new ShaderKeyword(shader, keyword);
        
        return compilerData.shaderKeywordSet.IsEnabled(globalKeyword) || 
               compilerData.shaderKeywordSet.IsEnabled(localKeyword);
    }

    private static ShaderStripperConfig LoadConfig()
    {
        string[] guids = AssetDatabase.FindAssets("t:ShaderStripperConfig");
        if (guids.Length > 0)
        {
            string path = AssetDatabase.GUIDToAssetPath(guids[0]);
            return AssetDatabase.LoadAssetAtPath<ShaderStripperConfig>(path);
        }
        return null;
    }
}