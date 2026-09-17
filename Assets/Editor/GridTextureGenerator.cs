using UnityEngine;
using UnityEditor;
using System.IO;
public class GridTextureGenerator
{
    [MenuItem("Tools/Generar Textura de Grilla")]
    public static void GenerateGridTexture()
    {
        int size = 256;
        int lineThickness = 4;
        Texture2D texture = new Texture2D(size, size);

        Color background = new Color(0.18f, 0.2f, 0.18f);
        Color lineColor = new Color(0.28f, 0.32f, 0.28f);

        for (int x = 0; x < size; x++)
        {
            for (int y = 0; y < size; y++)
            {
                bool isLine = x < lineThickness || y < lineThickness;
                texture.SetPixel(x, y, isLine ? lineColor : background);
            }
        }

        texture.Apply();

        byte[] pngData = texture.EncodeToPNG();
        string path = "Assets/Textures/GridTexture.png";

        Directory.CreateDirectory("Assets/Textures");
        File.WriteAllBytes(path, pngData);

        AssetDatabase.Refresh();

        TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
        if (importer != null)
        {
            importer.wrapMode = TextureWrapMode.Repeat;
            importer.filterMode = FilterMode.Bilinear;
            importer.SaveAndReimport();
        }

        Debug.Log("Textura de grilla generada en: " + path);
    }
}