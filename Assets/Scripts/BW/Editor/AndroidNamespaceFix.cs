using UnityEngine;
using UnityEditor;
using UnityEditor.Android;
using System.IO;
using System.Xml;

public class AndroidNamespaceFix : IPostGenerateGradleAndroidProject
{
    public int callbackOrder => 999;

    public void OnPostGenerateGradleAndroidProject(string path)
    {
        Debug.Log("AndroidNamespaceFix: 开始修复 namespace 和 exported 问题...");
        
        // 获取 unityLibrary 路径
        string unityLibraryPath = Path.Combine(path, "../unityLibrary");
        
        // 修复 backgrounddownload.androidlib
        string modulePath = Path.Combine(unityLibraryPath, "backgrounddownload.androidlib");
        
        // 修复 build.gradle 的 namespace
        FixBuildGradle(
            Path.Combine(modulePath, "build.gradle"),
            "com.unity3d.backgrounddownload"
        );
        
        // 修复 AndroidManifest.xml 的 exported 属性
        FixAndroidManifestXml(
            Path.Combine(modulePath, "src/main/AndroidManifest.xml")
        );
        
        Debug.Log("AndroidNamespaceFix: 修复完成！");
    }

    private void FixBuildGradle(string buildGradlePath, string namespaceValue)
    {
        if (!File.Exists(buildGradlePath))
        {
            Debug.LogWarning($"AndroidNamespaceFix: 文件不存在 - {buildGradlePath}");
            return;
        }

        string content = File.ReadAllText(buildGradlePath);
        
        // 检查是否已经有 namespace
        if (content.Contains("namespace"))
        {
            Debug.Log($"AndroidNamespaceFix: {Path.GetFileName(buildGradlePath)} 已包含 namespace，跳过");
            return;
        }

        // 在 android { 后面添加 namespace
        string androidBlock = "android {";
        if (content.Contains(androidBlock))
        {
            string namespaceDeclaration = $"\n    namespace '{namespaceValue}'";
            content = content.Replace(androidBlock, androidBlock + namespaceDeclaration);
            
            File.WriteAllText(buildGradlePath, content);
            Debug.Log($"AndroidNamespaceFix: 已为 {Path.GetFileName(buildGradlePath)} 添加 namespace: {namespaceValue}");
        }
        else
        {
            Debug.LogError($"AndroidNamespaceFix: 未找到 'android {{' 块在 {buildGradlePath}");
        }
    }

    private void FixAndroidManifestXml(string manifestPath)
    {
        if (!File.Exists(manifestPath))
        {
            Debug.LogWarning($"AndroidNamespaceFix: AndroidManifest.xml 不存在 - {manifestPath}");
            return;
        }

        try
        {
            XmlDocument doc = new XmlDocument();
            doc.Load(manifestPath);
            
            bool modified = false;
            
            // 获取 manifest 节点
            XmlNode manifestNode = doc.SelectSingleNode("/manifest");
            if (manifestNode == null)
            {
                Debug.LogError("AndroidNamespaceFix: 找不到 manifest 根节点");
                return;
            }
            
            // 获取 application 节点
            XmlNode applicationNode = manifestNode.SelectSingleNode("application");
            if (applicationNode == null)
            {
                Debug.LogWarning("AndroidNamespaceFix: 找不到 application 节点");
                return;
            }
            
            // 处理所有 receiver
            modified |= FixComponentsExported(doc, applicationNode, "receiver");
            
            // 处理所有 activity
            modified |= FixComponentsExported(doc, applicationNode, "activity");
            
            // 处理所有 service
            modified |= FixComponentsExported(doc, applicationNode, "service");
            
            if (modified)
            {
                // 保存修改后的文件
                doc.Save(manifestPath);
                Debug.Log($"AndroidNamespaceFix: 已修复 {Path.GetFileName(manifestPath)} 的 exported 属性");
            }
            else
            {
                Debug.Log($"AndroidNamespaceFix: {Path.GetFileName(manifestPath)} 无需修复 exported 属性");
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"AndroidNamespaceFix: 修复 AndroidManifest.xml 时出错: {e.Message}");
        }
    }

    private bool FixComponentsExported(XmlDocument doc, XmlNode applicationNode, string componentType)
    {
        bool modified = false;
        XmlNodeList components = applicationNode.SelectNodes(componentType);
        
        if (components == null || components.Count == 0)
        {
            return false;
        }
        
        foreach (XmlNode component in components)
        {
            // 检查是否有 intent-filter 子节点
            XmlNode intentFilter = component.SelectSingleNode("intent-filter");
            if (intentFilter == null)
            {
                continue; // 没有 intent-filter，不需要 exported
            }
            
            // 检查是否已经有 android:exported 属性
            XmlAttribute exportedAttr = null;
            if (component.Attributes != null)
            {
                foreach (XmlAttribute attr in component.Attributes)
                {
                    if (attr.LocalName == "exported" && attr.NamespaceURI == "http://schemas.android.com/apk/res/android")
                    {
                        exportedAttr = attr;
                        break;
                    }
                }
            }
            
            if (exportedAttr == null)
            {
                // 添加 android:exported="true" 属性
                XmlAttribute newAttr = doc.CreateAttribute("android", "exported", "http://schemas.android.com/apk/res/android");
                newAttr.Value = "true";
                component.Attributes.Append(newAttr);
                
                string componentName = component.Attributes["android:name"]?.Value ?? "Unknown";
                Debug.Log($"AndroidNamespaceFix: 为 {componentType} '{componentName}' 添加了 android:exported=\"true\"");
                modified = true;
            }
        }
        
        return modified;
    }
}