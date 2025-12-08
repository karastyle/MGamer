using UnityEngine;

[CreateAssetMenu(fileName = "TerrainAtlasConfig", menuName = "Terrain/Atlas Exporter Config")]
public class TerrainAtlasExporterConfig : ScriptableObject
{
    public Terrain terrain;
    public string exportDirectory = "";
    public int atlasResolution = 2048;
    public int tileSize = 512;
    public int padding = 2;
}