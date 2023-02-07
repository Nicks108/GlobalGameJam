using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.Rendering;
using System.IO;
using System.Globalization;

public static class SC_BillboardMage
{
    public enum CaptureResolution
    {
        _256x256 = 1,
        _512x512 = 2,
        _1024x1024 = 4,
        _2048x2048 = 8,
        _4096x4096 = 16,
        _8192x8192 = 32,
    }

    public static bool disableRealtimeLightingWhenCapture = false;
    public static CaptureResolution outputImageMultiplier = CaptureResolution._1024x1024; //1 = 1024p x 1024px, 2 = 2048px x 2048px etc.
    public static int samplesPerRow = 4;
    public static int totalRows = 2;
    //public static int totalSamples = 8; //Must be divisible by whatever samplesPerRowIs
    public static int samplePadding = 2; //Padding in pixels per sample (To prevent color bleeding between neighbor samples)

    public static int sampleIsolationLayer = 5; //This is the layer that will be temporarily assigned to the selected object and a sample camera, if other objects appear in the sample texture, try picking a different layer that is not in use by any objects in the Scene.

    public static bool allowMultiSelect = true; //If multi-select is enabled, a separate Billboard Asset will be generated for each selected object (NOTE: If the objects are the same, it's better to generate Billboard Asset for one object then duplicate for the other object to avoid generating unnecessary assets).

    public static bool enableTransparencyFix = true; //Fix black edges by extending pixel color to a nearby transparent pixels.

    //Editor Pref keys
    const string keyPrefix = "EM_";
    const string lastSelectedPathKey = keyPrefix + "LastSelectedPath";
    const string settingsKey = keyPrefix + "Settings";

    public static string defaultSettings = "";

    public static void LoadSettings(bool resetToDefault)
    {
        if(defaultSettings == "")
        {
            defaultSettings = CombineSettings();
        }

        string settingsData = resetToDefault ? defaultSettings : (EditorPrefs.HasKey(settingsKey) ? EditorPrefs.GetString(settingsKey) : "");

        if(settingsData != "")
        {
            string[] data = settingsData.Split(",");

            if(data.Length == 8)
            {
                disableRealtimeLightingWhenCapture = data[0] == "1";
                int defaultOutputImageMultiplier = 4;
                int.TryParse(data[1], out defaultOutputImageMultiplier);
                outputImageMultiplier = (CaptureResolution)defaultOutputImageMultiplier;
                int.TryParse(data[2], out samplesPerRow);
                int.TryParse(data[3], out totalRows);
                int.TryParse(data[4], out samplePadding);
                int.TryParse(data[5], out sampleIsolationLayer);
                allowMultiSelect = data[6] == "1";
                enableTransparencyFix = data[7] == "1";

                if (!resetToDefault)
                {
                    Debug.Log("Loading settings.");
                }
            }
        }

        if (resetToDefault)
        {
            if (EditorPrefs.HasKey(settingsKey))
            {
                Debug.Log("Resetting settings to default.");
                EditorPrefs.DeleteKey(settingsKey);
            }
        }
    }

    public static void SaveSettings()
    {
        string settingsData = CombineSettings();

        if (settingsData != defaultSettings)
        {
            Debug.Log("Saving settings.");
            EditorPrefs.SetString(settingsKey, settingsData);
        }
        else
        {
            if (EditorPrefs.HasKey(settingsKey))
            {
                Debug.Log("Clearing settings to default.");
                EditorPrefs.DeleteKey(settingsKey);
            }
        }
    }

    public static string CombineSettings()
    {
        return (disableRealtimeLightingWhenCapture ? "1" : "0") + "," + ((int)outputImageMultiplier).ToString() + "," + samplesPerRow.ToString() + "," + totalRows.ToString() + "," + samplePadding.ToString() + "," + sampleIsolationLayer.ToString() + "," + (allowMultiSelect ? "1" : "0") + "," + (enableTransparencyFix ? "1" : "0");
    }

    static List<Object> newSelectedObjects = new List<Object>();

    [MenuItem("Billboard Mage 1.0/Create Billboard Asset")]
    static void CreateBillboardPrefabs()
    {
        LoadSettings(false);

        newSelectedObjects.Clear();

        if(samplePadding < 0)
        {
            samplePadding = 0;
        }

        if(samplesPerRow <= 0)
        {
            Debug.LogError("'Samples Per Row' must be more than 0.");
            return;
        }

        if (totalRows < 1)
        {
            Debug.LogError("'Total Rows' must be more than 0.");
            return;
        }

        int totalSampelsToCapture = samplesPerRow * totalRows;

        if (totalSampelsToCapture > 100)
        {
            Debug.LogError("'Samples Per Row' x 'Total Rows' must be equal or under 100.");
            return;
        }

        List<GameObject> selectedGameObjects = new List<GameObject>();

        Object[] selectedObjects = Selection.objects;

        for (int i = 0; i < selectedObjects.Length; i++)
        {
            if (selectedObjects[i].GetType() == typeof(GameObject))
            {
                GameObject obj = ((GameObject)selectedObjects[i]);
                if (obj.activeInHierarchy)
                {
                    selectedGameObjects.Add(obj);
                }
            }
        }

        if (selectedGameObjects.Count <= 0)
        {
            EditorUtility.DisplayDialog("Create Billboard Asset", "Select any active object(s) in the Scene", "Ok");
            return;
        }

        if(!allowMultiSelect && selectedGameObjects.Count > 1)
        {
            EditorUtility.DisplayDialog("Create Billboard Asset", "Multi-select is disabled, please select one Scene object at a time.", "Ok");
            return;
        }

        string defaultPath = EditorPrefs.HasKey(lastSelectedPathKey) && EditorPrefs.GetString(lastSelectedPathKey).StartsWith(Application.dataPath) && Directory.Exists(EditorPrefs.GetString(lastSelectedPathKey)) ? EditorPrefs.GetString(lastSelectedPathKey) : Application.dataPath;
        string path = EditorUtility.OpenFolderPanel("Create Billboard Asset", defaultPath, "");
        if (path.Length <= 0 || !Directory.Exists(path))
        {
            //EditorUtility.DisplayDialog("Create Billboard Asset", "No path was selected.", "Ok");
            return;
        }

        if (!path.StartsWith(Application.dataPath))
        {
            EditorUtility.DisplayDialog("Create Billboard Asset", "Selected path must be inside the current project's Assets folder.", "Ok");
            return;
        }

        string relativePath = "Assets" + path.Remove(0, Application.dataPath.Length);

        EditorPrefs.SetString(lastSelectedPathKey, path);

        //Save previous lighting mode
        AmbientMode previousRenderingMode = RenderSettings.ambientMode;
        Color previousAmbientGroundColor = RenderSettings.ambientGroundColor;
        Color previousAambientEquatorColor = RenderSettings.ambientEquatorColor;
        Color previousAambientSkyColor = RenderSettings.ambientSkyColor;
        bool previousFogEnabled = RenderSettings.fog;
        Light[] sceneLights = new Light[0];

        //Disable lighting
        if (disableRealtimeLightingWhenCapture)
        {
            RenderSettings.ambientMode = AmbientMode.Flat;
            RenderSettings.ambientSkyColor = Color.white;
            RenderSettings.fog = false;
            sceneLights = GameObject.FindObjectsOfType<Light>();
            for (int i = 0; i < sceneLights.Length; i++)
            {
                if (!sceneLights[i].enabled)
                {
                    sceneLights[i] = null;
                }
                else
                {
                    sceneLights[i].enabled = false;
                }
            }
        }
        //

        //Create a Camera we will be used for rendering
        GameObject cameraObject = new GameObject("Camera Object");
        Camera camera = cameraObject.AddComponent<Camera>();
        camera.clearFlags = CameraClearFlags.SolidColor;
        camera.orthographic = true;
        camera.nearClipPlane = 0.3f;
        camera.farClipPlane = 10000;
        camera.backgroundColor = Color.clear;
        camera.cullingMask = 1 << sampleIsolationLayer; //UI layer only

        int outputTextureSize = 256 * (int)outputImageMultiplier;
        int renderTextureWidth = (outputTextureSize / samplesPerRow) - (samplePadding * 2);
        int renderTextureHeight = (outputTextureSize / totalRows) - (samplePadding * 2);

        RenderTexture rt = new RenderTexture(renderTextureWidth, renderTextureHeight, 32);
        camera.targetTexture = rt;

        for(int i = 0; i < selectedGameObjects.Count; i++)
        {
            CreateBillboardPrefab(camera, selectedGameObjects[i], path, relativePath, totalSampelsToCapture, outputTextureSize, renderTextureWidth, renderTextureHeight);
        }

        //Clear data
        RenderTexture.active = null; // added to avoid errors 
        GameObject.DestroyImmediate(cameraObject);
        Object.DestroyImmediate(rt);
        //

        //Revert Lighting and Settings
        if (disableRealtimeLightingWhenCapture)
        {
            for (int i = 0; i < sceneLights.Length; i++)
            {
                if (sceneLights[i])
                {
                    sceneLights[i].enabled = true;
                }
            }
            RenderSettings.ambientGroundColor = previousAmbientGroundColor;
            RenderSettings.ambientEquatorColor = previousAambientEquatorColor;
            RenderSettings.ambientSkyColor = previousAambientSkyColor;
            RenderSettings.fog = previousFogEnabled;
            RenderSettings.ambientMode = previousRenderingMode;
        }
        //

        Selection.objects = newSelectedObjects.ToArray();

        AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);
    }

    static void CreateBillboardPrefab(Camera camera, GameObject obj, string savePath, string relativePath, int totalSampelsToCapture, int outputTextureSize, int renderTextureWidth, int renderTextureHeight)
    {
        Renderer[] renderers = obj.GetComponentsInChildren<Renderer>();
        int[] rendererLayers = new int[renderers.Length];
        for (int r = 0; r < renderers.Length; r++)
        {
            rendererLayers[r] = renderers[r].gameObject.layer;
        }

        Bounds bounds = new Bounds();
        bool boundsInitialized = false;
        for (int r = 0; r < renderers.Length; r++)
        {
            if (!boundsInitialized)
            {
                boundsInitialized = true;

                bounds = renderers[r].bounds;
            }
            else
            {
                bounds.Encapsulate(renderers[r].bounds);
            }

            renderers[r].gameObject.layer = sampleIsolationLayer; //Set UI layer temporarily
        }


        camera.Render();

        float angleStep = 360.0f / totalSampelsToCapture;
        float startAngle = -90f;

        float ortographicSize = 0;
        Rect largestBoundsRect = new Rect();
        float angleY = startAngle;
        for (int i = 0; i < totalSampelsToCapture; i++)
        {
            Rect boundsRect = GetOrthographicRect(camera, bounds, angleY);
            float orthographicMultiplication = Mathf.Max(boundsRect.width, boundsRect.height);
            float ortographicSizeTmp = camera.orthographicSize * orthographicMultiplication;
            if (ortographicSizeTmp > ortographicSize)
            {
                ortographicSize = ortographicSizeTmp;
                largestBoundsRect = boundsRect;
            }

            angleY -= angleStep;
        }
        camera.orthographicSize = ortographicSize;
        camera.farClipPlane = (bounds.size.x + bounds.size.z) * 2.5f;

        //Take 8 different Screen shots from various angles
        RenderTexture.active = camera.targetTexture;
        Rect rtReadRect = new Rect(0, 0, renderTextureWidth, renderTextureHeight);
        Texture2D t2D = new Texture2D(outputTextureSize, outputTextureSize);

        float finalAspectRatio = largestBoundsRect.width / largestBoundsRect.height;

        //Reset colors to transparent
        Color32[] colors = t2D.GetPixels32();
        for (int i = 0; i < colors.Length; i++)
        {
            colors[i] = new Color32(0, 0, 0, 0);
        }
        t2D.SetPixels32(colors);
        t2D.Apply();
        //

        angleY = startAngle;
        for (int i = 0; i < totalSampelsToCapture; i++)
        {
            SetCameraPositionRelativeToBounds(camera, bounds, angleY);
            ConfigureCameraStartPosition(camera, bounds, finalAspectRatio);

            camera.Render();

            int row = i / samplesPerRow;
            int column = i % samplesPerRow;

            t2D.ReadPixels(rtReadRect, samplePadding + (renderTextureWidth + (samplePadding * 2)) * column, samplePadding + (renderTextureHeight + (samplePadding * 2)) * row);

            angleY -= angleStep;
        }

        //Fix border transparency color (White/Black outlines)
        if (enableTransparencyFix)
        {
            t2D.FixImageTransparencyBorder();
        }

        byte[] textureBytes = t2D.EncodeToPNG();
        string fileName = obj.name + "_" + obj.GetInstanceID().ToString();

        //string imageName = "Test Name.png";
        string textureSavePath = Path.Combine(savePath, fileName + "_Billboard_Texture.png");
        string textureSaveLocalPath = relativePath + "/" + fileName + "_Billboard_Texture.png";
        File.WriteAllBytes(textureSavePath, textureBytes);

        AssetDatabase.ImportAsset(textureSaveLocalPath);

        Vector3 billboArdOrigin = new Vector3(bounds.center.x, bounds.center.y - (bounds.size.y * 0.5f), bounds.center.z);
        float billboardHeight = /*largestBoundsRect.x - largestBoundsRect.y*/ bounds.size.y;
        float billboardWidth = Vector3.Distance(camera.ViewportToWorldPoint(Vector3.zero), camera.ViewportToWorldPoint(new Vector3(1, 0)));
        Vector3 positionOffset = obj.transform.position - billboArdOrigin;
        float billboardBottom = -positionOffset.y;

        Vector2 bottomRightVertice = new Vector2(1, 0);
        Vector2 bottomLeftVertice = new Vector2(0, 0);
        Vector2 topLeftVertice = new Vector2(0, 1);
        Vector2 topRightVertice = new Vector2(1, 1);

        Vector2[] vertices = new Vector2[4];
        vertices[0] = bottomRightVertice;
        vertices[1] = bottomLeftVertice;
        vertices[2] = topLeftVertice;
        vertices[3] = topRightVertice;

        float xStep = 1.0f / samplesPerRow;
        float yStep = 1.0f / totalRows;

        Vector4[] imageTexCoords = new Vector4[totalSampelsToCapture];

        //If the object pixels do not occupy the whole horizontal sample cell, change texture coords to not include the empty space above
        float percentageOccupied = GetOrthographicRect(camera, bounds, angleY).height;

        //Account for the padding
        float xyShift = (samplePadding * 1.00000f) / (outputTextureSize * 1.00000f);

        for (int i = 0; i < totalSampelsToCapture; i++)
        {
            int row = i / samplesPerRow;
            int column = i % samplesPerRow;

            imageTexCoords[i] = new Vector4((xStep * column) + xyShift, (yStep * row) + xyShift, xStep - (xyShift * 2), (yStep - (xyShift * 2)) * percentageOccupied);
        }

        ushort[] indices = new ushort[6];
        indices[0] = 0;
        indices[1] = 1;
        indices[2] = 2;
        indices[3] = 2;
        indices[4] = 3;
        indices[5] = 0;

        Shader billboardShader = disableRealtimeLightingWhenCapture ? Shader.Find("BillboardMage/Billboard") : Shader.Find("BillboardMage/Billboard Unlit");
        if (billboardShader == null)
        {
            //Backup shader (default Unity shader)
            billboardShader = Shader.Find("Nature/SpeedTree Billboard");
        }
        Material mat = new Material(billboardShader);
        mat.mainTexture = (Texture2D)AssetDatabase.LoadAssetAtPath(textureSaveLocalPath, typeof(Texture2D));
        string materialLocalPath = relativePath + "/" + fileName + "_Billboard_Material.mat";

        AssetDatabase.CreateAsset(mat, materialLocalPath);

        AssetDatabase.ImportAsset(materialLocalPath);

        string guid = "";
        long localId = 0;
        AssetDatabase.TryGetGUIDAndLocalFileIdentifier(mat, out guid, out localId);

        //string billboardAssetName = "Test Name";
        string billboardAssetPath = Path.Combine(savePath, fileName + "_Billboard_Asset.asset");
        string billboardAssetLocalPath = relativePath + "/" + fileName + "_Billboard_Asset.asset";
        File.WriteAllText(billboardAssetPath, GenerateBillboardAssetData(fileName + "_Billboard_Asset", billboardWidth, billboardBottom, billboardHeight, imageTexCoords, vertices, indices, guid, localId));

        AssetDatabase.ImportAsset(billboardAssetLocalPath);

        //Revert renderer layers
        for (int r = 0; r < renderers.Length; r++)
        {
            renderers[r].gameObject.layer = rendererLayers[r]; //Set UI layer temporarily
        }

        //Create BillboardRenderer object
        GameObject billboardRendererObject = new GameObject(obj.name + "_BillboardRenderer");
        positionOffset.y = 0;
        billboardRendererObject.transform.position = obj.transform.position - positionOffset;
        BillboardRenderer billboardRenderer = billboardRendererObject.AddComponent<BillboardRenderer>();
        billboardRenderer.billboard = (BillboardAsset)AssetDatabase.LoadAssetAtPath(billboardAssetLocalPath, typeof(BillboardAsset));
        billboardRenderer.billboard.material = mat; //Update material in the scene
        billboardRendererObject.AddComponent<BillboardMageGizmo>();

        Undo.RecordObject(billboardRendererObject, billboardRendererObject.name + " (Object Create)");

        newSelectedObjects.Add((Object)billboardRendererObject);
    }

    static Rect GetOrthographicRect(Camera camera, Bounds bounds, float angleY)
    {
        SetCameraPositionRelativeToBounds(camera, bounds, angleY);

        Rect boundsRect = BoundsToViewportRect(camera, bounds);

        return boundsRect;
    }

    static void SetCameraPositionRelativeToBounds(Camera camera, Bounds bounds, float angleY)
    {
        Vector3 cameraRotationVector = Quaternion.AngleAxis(angleY, Vector3.up) * ((bounds.size.x + bounds.size.z) * Vector3.forward);
        Vector3 cameraPosition = bounds.center - cameraRotationVector;
        camera.transform.position = cameraPosition;
        camera.transform.LookAt(bounds.center);

        camera.transform.position = cameraPosition;
    }

    static void ConfigureCameraStartPosition(Camera camera, Bounds bounds, float aspectRation)
    {
        Rect boundsRect = BoundsToViewportRect(camera, bounds);

        Vector3 bottomMostPos = camera.ViewportToWorldPoint(new Vector3(0.5f, 0, 0));

        //Move camera so the lower bounds are aligned with the lower camera border
        float posYDifference = 0;
        if (bottomMostPos.y < boundsRect.y && aspectRation > 1)
        {
            posYDifference = (boundsRect.y - bottomMostPos.y);
            Vector3 cameraPosition = camera.transform.position;
            cameraPosition.y += posYDifference;
            camera.transform.position = cameraPosition;
        }
    }

    static Rect BoundsToViewportRect(Camera camera, Bounds bounds)
    {
        Vector3[] boundsCorners = new Vector3[8];
        boundsCorners[0] = bounds.min;
        boundsCorners[1] = bounds.max;
        boundsCorners[2] = new Vector3(boundsCorners[0].x, boundsCorners[0].y, boundsCorners[1].z);
        boundsCorners[3] = new Vector3(boundsCorners[0].x, boundsCorners[1].y, boundsCorners[0].z);
        boundsCorners[4] = new Vector3(boundsCorners[1].x, boundsCorners[0].y, boundsCorners[0].z);
        boundsCorners[5] = new Vector3(boundsCorners[0].x, boundsCorners[1].y, boundsCorners[1].z);
        boundsCorners[6] = new Vector3(boundsCorners[1].x, boundsCorners[0].y, boundsCorners[1].z);
        boundsCorners[7] = new Vector3(boundsCorners[1].x, boundsCorners[1].y, boundsCorners[0].z);

        Vector2 topLeftPosition = new Vector2(-255f, -255f);
        Vector2 bottomRightPosition = new Vector2(-255f, -255f);

        Vector2 topRightPositionWorld = Vector2.zero;
        Vector2 bottomRightPositionWorld = Vector2.zero;

        for (int i = 0; i < boundsCorners.Length; i++)
        {
            Vector3 viewportPoint = camera.WorldToViewportPoint(boundsCorners[i]);

            //Top Left
            if (topLeftPosition.x == -255f || topLeftPosition.x > viewportPoint.x)
            {
                topLeftPosition.x = viewportPoint.x;
            }
            if (topLeftPosition.y == -255f || topLeftPosition.y < viewportPoint.y)
            {
                topLeftPosition.y = viewportPoint.y;
                topRightPositionWorld.y = boundsCorners[i].y;
            }
            //Bottom Right
            if (bottomRightPosition.x == -255f || bottomRightPosition.x < viewportPoint.x)
            {
                bottomRightPosition.x = viewportPoint.x;
            }
            if (bottomRightPosition.y == -255f || bottomRightPosition.y > viewportPoint.y)
            {
                bottomRightPosition.y = viewportPoint.y;
                bottomRightPositionWorld.y = boundsCorners[i].y;
            }
        }

        return new Rect(topRightPositionWorld.y, bottomRightPositionWorld.y, bottomRightPosition.x - topLeftPosition.x, topLeftPosition.y - bottomRightPosition.y);
    }

    static string GenerateBillboardAssetData(string name, float width, float bottom, float height, Vector4[] imageTexCoords, Vector2[] vertices, ushort[] indices, string materialGuid, long materialLocalId)
    {
        NumberFormatInfo nfi = new NumberFormatInfo();
        nfi.NumberDecimalSeparator = ".";

        string[] dataArray = new string[14 + imageTexCoords.Length + vertices.Length];
        int nextIndex = 0;
        dataArray[nextIndex] = "%YAML 1.1";
        nextIndex++;
        dataArray[nextIndex] = "%TAG !u! tag:unity3d.com,2011:";
        nextIndex++;
        dataArray[nextIndex] = "--- !u!226 &22600000";
        nextIndex++;
        dataArray[nextIndex] = "BillboardAsset:";
        nextIndex++;
        dataArray[nextIndex] = "  m_ObjectHideFlags: 0";
        nextIndex++;
        dataArray[nextIndex] = "  m_Name: " + name;
        nextIndex++;
        dataArray[nextIndex] = "  serializedVersion: 1";
        nextIndex++;
        dataArray[nextIndex] = "  width: " + width.ToString(nfi);
        nextIndex++;
        dataArray[nextIndex] = "  bottom: " + bottom.ToString(nfi);
        nextIndex++;
        dataArray[nextIndex] = "  height: " + height.ToString(nfi);
        nextIndex++;
        dataArray[nextIndex] = "  imageTexCoords:";
        nextIndex++;
        for (int i = 0; i < imageTexCoords.Length; i++)
        {
            dataArray[nextIndex] = "  - {x: " + imageTexCoords[i].x.ToString(nfi) + ", y: " + imageTexCoords[i].y.ToString(nfi) + ", z: " + imageTexCoords[i].z.ToString(nfi) + ", w: " + imageTexCoords[i].w.ToString(nfi) + "}";
            nextIndex++;
        }
        dataArray[nextIndex] = "  vertices:";
        nextIndex++;
        for (int i = 0; i < vertices.Length; i++)
        {
            dataArray[nextIndex] = "  - {x: " + vertices[i].x.ToString(nfi) + ", y: " + vertices[i].y.ToString(nfi) + "}";
            nextIndex++;
        }
        dataArray[nextIndex] = "  indices: ";
        for (int i = 0; i < indices.Length; i++)
        {
            dataArray[nextIndex] += "0" + indices[i].ToString(nfi) + "00";
        } 
        nextIndex++;
        dataArray[nextIndex] = "  material: {fileID: " + materialLocalId.ToString(nfi) + ", guid: " + materialGuid + ", type: 2}";
        nextIndex++;

        return string.Join("\n", dataArray);
    }

    static Texture2D FixImageTransparencyBorder(this Texture2D t2D)
    {
        int outputTextureSizeRef = t2D.width;

        //Fill neighbor pixels with the same color
        Color32[] t2DColors = t2D.GetPixels32();

        //Pass 1 - Fill Pixels To The Left And To The right
        int t2DColorsLength = t2DColors.Length;
        int limitNearbyIteration = Mathf.Min(10, samplePadding);
        if(limitNearbyIteration < 1)
        {
            limitNearbyIteration = 1;
        }
        for (int i = 0; i < t2DColorsLength; i++)
        {
            if (t2DColors[i].a > 0)
            {
                int pixelsToLeftCount = i % outputTextureSizeRef;
                int pixelsToRightCount = outputTextureSizeRef - pixelsToLeftCount - 1;

                //Just iterate enough to cover the nearby pixels
                if (pixelsToLeftCount > limitNearbyIteration)
                {
                    pixelsToLeftCount = limitNearbyIteration;
                }
                if (pixelsToRightCount > limitNearbyIteration)
                {
                    pixelsToRightCount = limitNearbyIteration;
                }

                //Iterate to the left
                for (int tmp = 0; tmp < pixelsToLeftCount; tmp++)
                {
                    int pixelIndex = i - tmp - 1;
                    if (pixelIndex >= 0 && t2DColors[pixelIndex].a == 0)
                    {
                        t2DColors[pixelIndex] = t2DColors[i];
                        t2DColors[pixelIndex].a = 1;
                    }
                    else
                    {
                        break;
                    }
                }

                //Iterate to the right
                int skipPixelCount = 0;
                for (int tmp = 0; tmp < pixelsToRightCount; tmp++)
                {
                    int pixelIndex = i + tmp + 1;
                    if (pixelIndex < t2DColorsLength && t2DColors[pixelIndex].a == 0)
                    {
                        t2DColors[pixelIndex] = t2DColors[i];
                        t2DColors[pixelIndex].a = 1;

                        skipPixelCount++;
                    }
                    else
                    {
                        break;
                    }
                }

                i += skipPixelCount;
            }
        }

        //Pass 2 - Fill Pixels To Up
        for (int i = 0; i < t2DColorsLength; i++)
        {
            if (t2DColors[i].a > 0)
            {
                int pixelsUpCount = i / outputTextureSizeRef;

                //Just iterate enough to cover the nearby pixels
                if (pixelsUpCount > limitNearbyIteration)
                {
                    pixelsUpCount = limitNearbyIteration;
                }

                //Iterate to up
                for (int tmp = 0; tmp < pixelsUpCount; tmp++)
                {
                    int pixelIndex = i - ((tmp + 1) * outputTextureSizeRef);
                    if (pixelIndex >= 0 && t2DColors[pixelIndex].a == 0)
                    {
                        t2DColors[pixelIndex] = t2DColors[i];
                        t2DColors[pixelIndex].a = 1;
                    }
                    else
                    {
                        break;
                    }
                }
            }
        }

        //Pass 3 - Fill Pixels Down
        for (int i = 0; i < t2DColorsLength; i++)
        {
            if (t2DColors[i].a > 0)
            {
                int pixelsUpCount = i / outputTextureSizeRef;
                int pixelsDownCount = outputTextureSizeRef - pixelsUpCount - 1;

                //Just iterate enough to cover the nearby pixels
                if (pixelsDownCount > limitNearbyIteration)
                {
                    pixelsDownCount = limitNearbyIteration;
                }

                //Iterate to down
                for (int tmp = 0; tmp < pixelsDownCount; tmp++)
                {
                    int pixelIndex = i + ((tmp + 1) * outputTextureSizeRef);
                    if (pixelIndex < t2DColorsLength && t2DColors[pixelIndex].a == 0)
                    {
                        t2DColors[pixelIndex] = t2DColors[i];
                        t2DColors[pixelIndex].a = 1;
                    }
                    else
                    {
                        break;
                    }
                }

            }
        }

        //Part 4 - Make marked pixels as transparent
        for (int i = 0; i < t2DColorsLength; i++)
        {
            if (t2DColors[i].a == 1)
            {
                t2DColors[i].a = 0;
            }
        }

        t2D.SetPixels32(t2DColors);
        t2D.Apply();
        //

        return t2D;
    }
}
