using UnityEngine;

public class TestShaderContext : MonoBehaviour
{
    private Material mat;

    void OnEnable()
    {
        var renderer = GetComponent<Renderer>();
        // 在编辑器模式下建议使用 sharedMaterial 防止材质泄漏，
        // 但在运行时测试建议用 material (实例)。
        // 这里为了方便两用，做个判断。
        if (Application.isPlaying)
            mat = renderer.material;
        else
            mat = renderer.sharedMaterial;
    }

    public void SetGray()
    {
        EnsureMaterial();
        mat.DisableKeyword("VARIANT_RED");
        mat.DisableKeyword("VARIANT_BLUE");
        Debug.Log("已切换为：灰色 (默认)");
    }

    public void SetRed()
    {
        EnsureMaterial();
        mat.DisableKeyword("VARIANT_BLUE");
        mat.EnableKeyword("VARIANT_RED");
        Debug.Log("已切换为：红色");
    }

    public void SetBlue()
    {
        EnsureMaterial();
        mat.DisableKeyword("VARIANT_RED");
        mat.EnableKeyword("VARIANT_BLUE");
        Debug.Log("已切换为：蓝色");
    }

    private void EnsureMaterial()
    {
        if (mat == null)
        {
            var renderer = GetComponent<Renderer>();
            mat = Application.isPlaying ? renderer.material : renderer.sharedMaterial;
        }
    }
}