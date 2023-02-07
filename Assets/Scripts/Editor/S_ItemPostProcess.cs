using System.IO;
using NUnit.Framework;
using UnityEngine;
using UnityEditor;
using UnityEditorInternal;
using System.Collections.Generic;

public class S_ItemPostProcess : AssetPostprocessor
{
    void OnPreprocessTexture()
    {
        TextureImporter importer = assetImporter as TextureImporter;
        

        if (importer.assetPath.Contains("Sprites"))
        {
            if (importer.assetPath.Contains(".png"))
            {
                importer.textureType = TextureImporterType.Sprite;
                if (importer.assetPath.Contains("sheet"))
                {
                    //importer.spriteImportMode = SpriteImportMode.Multiple;
                    ProcessSpriteSheet(importer);
                }
            }
        }

        if (importer.assetPath.Contains("GUI") && importer.assetPath.Contains(".png"))
        {
            importer.textureType = TextureImporterType.GUI;
        }
        
    }

    void ProcessSpriteSheet(TextureImporter importer)
    {
        Texture2D texture = new Texture2D(2,2);
        texture.LoadImage(File.ReadAllBytes(assetImporter.assetPath));

        importer.textureType = TextureImporterType.Sprite;
        importer.spriteImportMode = SpriteImportMode.Multiple;
        importer.mipmapEnabled = false;
        importer.filterMode = FilterMode.Point;
        importer.spritePivot = Vector2.down;
        importer.textureCompression = TextureImporterCompression.Uncompressed;

        var textureSettings = new TextureImporterSettings(); // need this stupid class because spriteExtrude and spriteMeshType aren't exposed on TextureImporter
        importer.ReadTextureSettings(textureSettings);
        textureSettings.spriteMeshType = SpriteMeshType.Tight;
        textureSettings.spriteExtrude = 0;

        importer.SetTextureSettings(textureSettings);

        int minimumSpriteSize = 16;
        int extrudeSize = 0;

        Rect[] rects = InternalSpriteUtility.GenerateAutomaticSpriteRectangles(texture, minimumSpriteSize, extrudeSize);
        var rectsList = new List<Rect>(rects);
        rectsList = SortRects(rectsList, texture.width);

        string filenameNoExtension = Path.GetFileNameWithoutExtension(assetImporter.assetPath);
        var metas = new List<SpriteMetaData>();
        int rectNum = 0;

        foreach (Rect rect in rectsList)
        {
            var meta = new SpriteMetaData();
            meta.pivot = Vector2.down;
            meta.alignment = (int)SpriteAlignment.BottomCenter;
            meta.rect = rect;
            meta.name = filenameNoExtension + "_" + rectNum++;
            metas.Add(meta);
        }

        importer.spritesheet = metas.ToArray();
    }



    //[MenuItem("Tools/Slice Spritesheets %&s")]
    //public static void Slice()
    //{
    //    var textures = Selection.GetFiltered<Texture2D>(SelectionMode.Assets);

    //    foreach (var texture in textures)
    //    {
    //        ProcessTexture(texture);
    //    }
    //}


    static void ProcessTexture(Texture2D texture)
    {
        string path = AssetDatabase.GetAssetPath(texture);
        var importer = AssetImporter.GetAtPath(path) as TextureImporter;

        //importer.isReadable = true;
        importer.textureType = TextureImporterType.Sprite;
        importer.spriteImportMode = SpriteImportMode.Multiple;
        importer.mipmapEnabled = false;
        importer.filterMode = FilterMode.Point;
        importer.spritePivot = Vector2.down;
        importer.textureCompression = TextureImporterCompression.Uncompressed;
 
        var textureSettings = new TextureImporterSettings(); // need this stupid class because spriteExtrude and spriteMeshType aren't exposed on TextureImporter
        importer.ReadTextureSettings(textureSettings);
        textureSettings.spriteMeshType = SpriteMeshType.Tight;
        textureSettings.spriteExtrude = 0;
 
        importer.SetTextureSettings(textureSettings);
 
        int minimumSpriteSize = 16;
        int extrudeSize = 0;

        Rect[] rects = InternalSpriteUtility.GenerateAutomaticSpriteRectangles(texture, minimumSpriteSize, extrudeSize);
        var rectsList = new List<Rect>(rects);
        rectsList = SortRects(rectsList, texture.width);

        string filenameNoExtension = Path.GetFileNameWithoutExtension(path);
        var metas = new List<SpriteMetaData>();
        int rectNum = 0;
 
        foreach (Rect rect in rectsList)
        {
            var meta = new SpriteMetaData();
            meta.pivot = Vector2.down; 
            meta.alignment = (int) SpriteAlignment.BottomCenter;
            meta.rect = rect;
            meta.name = filenameNoExtension + "_" + rectNum++;
            metas.Add(meta);
        }

        importer.spritesheet = metas.ToArray();

        AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
     }
 
    static List<Rect> SortRects(List<Rect> rects, float textureWidth)
    {
        List<Rect> list = new List<Rect>();
        while (rects.Count > 0)
        {
            Rect rect = rects[rects.Count - 1];
            Rect sweepRect = new Rect(0f, rect.yMin, textureWidth, rect.height);
            List<Rect> list2 = RectSweep(rects, sweepRect);
            if (list2.Count <= 0)
            {
                list.AddRange(rects);
                break;
            }
            list.AddRange(list2);
        }
        return list;
    }

    static List<Rect> RectSweep(List<Rect> rects, Rect sweepRect)
    {
        List<Rect> result;
        if (rects == null || rects.Count == 0)
        {
            result = new List<Rect>();
        }
        else
        {
            List<Rect> list = new List<Rect>();
            foreach (Rect current in rects)
            {
                if (current.Overlaps(sweepRect))
                {
                    list.Add(current);
                }
            }
            foreach (Rect current2 in list)
            {
                rects.Remove(current2);
            }
            list.Sort((a, b) => a.x.CompareTo(b.x));
            result = list;
        }
        return result;
    }
}



//using UnityEngine;
//using UnityEditor;
//using System.Collections;
//using System.Collections.Generic;

//public class NameSpritesAutomatically : MonoBehaviour
//{

//    [MenuItem("Sprites/Rename Sprites")]
//    static void SetSpriteNames()
//    {
//        Texture2D myTexture = (Texture2D)Resources.LoadAssetAtPath<Texture2D>("Assets/Sprites/MyTexture.png");

//        string path = AssetDatabase.GetAssetPath(myTexture);
//        TextureImporter ti = AssetImporter.GetAtPath(path) as TextureImporter;
//        ti.isReadable = true;

//        List<SpriteMetaData> newData = new List<SpriteMetaData>();

//        int SliceWidth = 16;
//        int SliceHeight = 16;

//        for (int i = 0; i < myTexture.width; i += SliceWidth)
//        {
//            for (int j = myTexture.height; j > 0; j -= SliceHeight)
//            {
//                SpriteMetaData smd = new SpriteMetaData();
//                smd.pivot = new Vector2(0.5f, 0.5f);
//                smd.alignment = 9;
//                smd.name = (myTexture.height - j) / SliceHeight + ", " + i / SliceWidth;
//                smd.rect = new Rect(i, j - SliceHeight, SliceWidth, SliceHeight);

//                newData.Add(smd);
//            }
//        }

//        ti.spritesheet = newData.ToArray();
//        AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
//    }
//}

//if (ti.spriteImportMode == SpriteImportMode.Multiple)
//{
//// Bug? Need to convert to single then back to multiple in order to make changes when it's already sliced
//ti.spriteImportMode = SpriteImportMode.Single;
//AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
//}
//smd.pivot = new Vector2(0.5f, 0.5f);
//smd.alignment = 9;