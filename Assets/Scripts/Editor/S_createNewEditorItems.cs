using System;
using System.CodeDom;
using System.Management.Instrumentation;
using System.Reflection;
//using Boo.Lang;
using UnityEngine;
using UnityEditor;
using UnityEditor.PackageManager.UI;
using UnityEngine.UI;
using System.Collections.Generic;

public class S_createNewEditorItems : EditorWindow
{
    string ClassName;
    string MenuLocation;

    public string AssetPath;
    public string EditorPath;

    [MenuItem("Tools/NTTools/Craete Editor Script")]
    private static void Init()
    {
        S_createNewEditorItems window = (S_createNewEditorItems)EditorWindow.GetWindow(typeof(S_createNewEditorItems));
        window.Show();

    }




    public void OnEnable()
    {
        AssetPath = Application.dataPath;
        EditorPath = AssetPath + "/Editor";
    }

    private bool press = false;
    private void OnGUI()
    {
        CreateNewEditorWindowAndMenuItem();
        EditorGUILayout.Separator();

    }


    private void CreateNewEditorWindowAndMenuItem()
    {
        ClassName = EditorGUILayout.TextField("Class Name", ClassName);
        MenuLocation = EditorGUILayout.TextField("Menue Location", MenuLocation);

        if (GUILayout.Button("Create new editor Menu & and Window"))
        {
            string TemplateFileText = System.IO.File.ReadAllText(EditorPath + "/BasicMenuItemTemplate.txt");
            TemplateFileText = TemplateFileText.Replace("$BasicMenuItemTemplate$", ClassName);
            TemplateFileText = TemplateFileText.Replace("$MenuPath$", MenuLocation);

            System.IO.File.WriteAllText(EditorPath + "/" + ClassName + ".cs", TemplateFileText);

            EditorGUILayout.HelpBox("New Editor Window and menu Created!", MessageType.Info);
        }

        //if (GUILayout.Button("Create new MonoBehaviour"))
        //{
        //}

        CreateNewInspectorGUIFromExistingMonoBehaviour();

    }

    private void CreateNewInspectorGUIAndMonBehaviour()
    {
        ClassName = EditorGUILayout.TextField("Class Name", ClassName);
        //MenuLocation = EditorGUILayout.TextField("Menue Location", MenuLocation);

        string PathToMonoBehaviourBasic = EditorPath + "/BasicMonoBehavior.txt";

        string TemplateFileText = System.IO.File.ReadAllText(EditorPath + "/BasicMenuItemTemplate.txt");
        TemplateFileText = TemplateFileText.Replace("$BasicMonoBehavior$", ClassName);
        //TemplateFileText = TemplateFileText.Replace("$MethodName$", MenuLocation);

        string PathToNewMonoBehaviour = AssetPath + "/" + ClassName + ".cs";
        System.IO.File.WriteAllText(PathToNewMonoBehaviour, TemplateFileText);
        //CreateNewInspectorGUIFromMonoBehaviour(ClassName, PathToNewMonoBehaviour);

        EditorGUILayout.HelpBox("New New Mono Behaviour and Inspector Gui Created!", MessageType.Info);

    }


    private int MonoBehaviourListIndex = 0;
    private void CreateNewInspectorGUIFromExistingMonoBehaviour()
    {
        List<System.Type> MonoBehaviourList = new List<Type>();
        List<string> MonoBehaviourNameList = new List<string>();
        var assemblies = AppDomain.CurrentDomain.GetAssemblies();
        foreach (Assembly assembly in assemblies)
        {
            var types = assembly.GetTypes();
            foreach (Type type in types)
            {
                if (type.IsSubclassOf(typeof(MonoBehaviour)))
                {
                    MonoBehaviourList.Add(type);
                    MonoBehaviourNameList.Add(type.Name.ToString());
                }
            }
        }
        MonoBehaviourListIndex = EditorGUILayout.Popup(MonoBehaviourListIndex, MonoBehaviourNameList.ToArray());
        if (GUILayout.Button("Create new inspector GUI From MonoBehaviour"))
        {
            CreateNewInspectorGUIFromMonoBehaviour(MonoBehaviourList[MonoBehaviourListIndex]);
        }
    }

    private void CreateNewInspectorGUIFromMonoBehaviour(Type MonoBevahiourType)
    {
        string PathToSaveInspectorGui = EditorPath + "/Inspector GUI Scripts";
        if (!System.IO.Directory.Exists(PathToSaveInspectorGui))
            System.IO.Directory.CreateDirectory(PathToSaveInspectorGui);

        string TemplateFileText = System.IO.File.ReadAllText(EditorPath + "/BasicInspectorGui.txt");
        TemplateFileText = TemplateFileText.Replace("$MonoBehaviour$", MonoBevahiourType.Name);

        //Debug.Log("type"+MonoBevahiourType.UnderlyingSystemType);

        List<string> inspectorContriList = BuildInspectorControls(MonoBevahiourType.UnderlyingSystemType);

        string Controles = string.Empty;
        foreach (string contole in inspectorContriList)
        {
            Controles += "\t\t" + contole + Environment.NewLine;
        }

        TemplateFileText = TemplateFileText.Replace("//$InspectorControles$", Controles);

        string PathToNewMonoBehaviour = PathToSaveInspectorGui + "/" + MonoBevahiourType.Name + "InspectorGui.cs";
        System.IO.File.WriteAllText(PathToNewMonoBehaviour, TemplateFileText);
        EditorGUILayout.HelpBox("New Inspector Gui Created for " + MonoBevahiourType.Name + "!", MessageType.Info);
    }

    private List<string> BuildInspectorControls(Type MonoBevahiourType)
    {
        List<string> InspectorControlList = new List<string>();

        PropertyInfo[] properties = MonoBevahiourType.GetProperties(BindingFlags.DeclaredOnly | BindingFlags.Public | BindingFlags.Instance);
        foreach (PropertyInfo property in properties)
        {
            InspectorControlList.Add("EditorGUILayout.LabelField(\" " + property.Name + "\", myTarget." + property.Name +
                                     ".ToString());");
        }


        FieldInfo[] fields = MonoBevahiourType.GetFields(BindingFlags.DeclaredOnly | BindingFlags.Public | BindingFlags.Instance);
        foreach (FieldInfo field in fields)
        {
            Debug.Log(field.Name);
            Type propType = field.FieldType;
            if (propType == typeof(int))
                InspectorControlList.Add(GenerateCodeStringForInt(field));
            else if (propType == typeof(string))
                InspectorControlList.Add(GenerateCodeStringForString(field));
            else if (propType == typeof(float)) //propType == typeof(double) ||
                InspectorControlList.Add(GenerateCodeStringForDouble(field));
            else if (propType == typeof(decimal))
                InspectorControlList.Add(GenerateCodeStringForDecimal(field));
            else if (propType == typeof(bool))
                InspectorControlList.Add(GenerateCodeStringForBool(field));
            else if (propType.IsEnum)
                InspectorControlList.Add(GenerateCodeStringForEnum(MonoBevahiourType, field));
            else if (propType == typeof(GameObject) || field.GetType().IsSubclassOf(typeof(MonoBehaviour)))
                InspectorControlList.Add(GenerateCodeStringForGameObject(field));
            else
                Debug.Log("you missed a type! " + field.FieldType);

        }

        return InspectorControlList;
    }

    private string GenerateCodeStringForInt(FieldInfo field)
    {
        string codeString = "myTarget." + field.Name + "= EditorGUILayout.IntField(\" " + field.Name + " \", myTarget." + field.Name + ");";
        return codeString;
    }
    private string GenerateCodeStringForString(FieldInfo field)
    {
        string codeString = "myTarget." + field.Name + "= EditorGUILayout.TextField(\" " + field.Name + " \", myTarget." + field.Name + ");";
        return codeString;
    }


    //shit below is not finished. change the textfield to the appropriot type
    private string GenerateCodeStringForDouble(FieldInfo field)
    {
        string codeString = "myTarget." + field.Name + "= EditorGUILayout.FloatField(\" " + field.Name + " \", myTarget." + field.Name + ");";
        return codeString;
    }
    private string GenerateCodeStringForBool(FieldInfo field)
    {
        string codeString = "myTarget." + field.Name + "= EditorGUILayout.Toggle(\" " + field.Name + " \", myTarget." + field.Name + ");";
        return codeString;
    }
    private string GenerateCodeStringForDecimal(FieldInfo field)
    {
        string codeString = "myTarget." + field.Name + "= EditorGUILayout.FloatField(\" " + field.Name + " \", myTarget." + field.Name + ");";
        return codeString;
    }
    private string GenerateCodeStringForGameObject(FieldInfo field)
    {
        string codeString =
            " EditorGUILayout.LabelField(\"" + field.Name + "\");\n" +
            "myTarget." + field.Name + " = EditorGUILayout.ObjectField(myTarget." + field.Name +
            ", typeof(GameObject)) as GameObject;";
        return codeString;
    }
    private string GenerateCodeStringForEnum(Type monoBehaviourType, FieldInfo field)
    {
        string codeString =
            "myTarget." + field.Name + " = (" + monoBehaviourType.Name + "." + field.FieldType.Name + ")EditorGUILayout.EnumPopup(\"" + field.Name + "\", myTarget." + field.Name + ");";
        return codeString;
    }



    [MenuItem("Assets/Import Animations")]
    private static void importAnimations()
    {
        
        for (int i = 0; i < Selection.gameObjects.Length; i++)
        {
            GameObject GO = Selection.gameObjects[i];
            Debug.Log( GO.name);

            if (GO is GameObject)
            {
                
            }

            string assetPath = AssetDatabase.GetAssetPath(GO);

            ModelImporter modelImporter = AssetImporter.GetAtPath(assetPath) as ModelImporter;

            //modelImporter.animationType = ModelImporterAnimationType.Human;
            //modelImporter.animationWrapMode = WrapMode.Loop; 
            //modelImporter.
            //modelImporter.clipAnimations[0].loopTime = true;
            //modelImporter.clipAnimations[0].keepOriginalOrientation = true; //Yes!!! Found the bastard!!

            Debug.Log(modelImporter.clipAnimations[0].keepOriginalOrientation);

            object[] objectlist = AssetDatabase.LoadAllAssetsAtPath(assetPath);

            Debug.Log("contenctsd of object");
            foreach (object o in objectlist)
            { 
                //Debug.Log(o.GetType());
            }


        }
    }




}
