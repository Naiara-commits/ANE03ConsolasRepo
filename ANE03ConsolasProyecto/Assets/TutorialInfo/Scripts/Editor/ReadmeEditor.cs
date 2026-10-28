using UnityEngine;
using UnityEditor;
using System.IO;
using System.Reflection;

[CustomEditor(typeof(Readme))]
[InitializeOnLoad]
public class ReadmeEditor : Editor
{
    private static readonly string s_ShowedReadmeSessionStateName =
        "ReadmeEditor.showedReadme";

    private const float k_Space = 16f;

    private const float k_HeaderImageHeight = 120f;
    private const float k_LogoSize = 56f;
    private const float k_BrandAreaHeight = 72f;

    private GUIStyle m_LinkStyle;
    private GUIStyle m_TitleStyle;
    private GUIStyle m_HeadingStyle;
    private GUIStyle m_BodyStyle;

    static ReadmeEditor()
    {
        EditorApplication.delayCall += SelectReadmeAutomatically;
    }

    private static void SelectReadmeAutomatically()
    {
        if (SessionState.GetBool(
                s_ShowedReadmeSessionStateName,
                false))
        {
            return;
        }

        Readme readme = SelectReadme();

        SessionState.SetBool(
            s_ShowedReadmeSessionStateName,
            true);

        if (readme == null)
        {
            return;
        }

        if (!readme.loadedLayout)
        {
            LoadLayout();

            readme.loadedLayout = true;

            EditorUtility.SetDirty(readme);
        }
    }

    private static void LoadLayout()
    {
        string layoutPath =
            Path.Combine(
                Application.dataPath,
                "TutorialInfo/Layout.wlt");

        if (!File.Exists(layoutPath))
        {
            return;
        }

        Assembly assembly =
            typeof(EditorApplication).Assembly;

        System.Type windowLayoutType =
            assembly.GetType(
                "UnityEditor.WindowLayout",
                false);

        if (windowLayoutType == null)
        {
            return;
        }

        MethodInfo method =
            windowLayoutType.GetMethod(
                "LoadWindowLayout",
                BindingFlags.Public |
                BindingFlags.NonPublic |
                BindingFlags.Static);

        if (method == null)
        {
            return;
        }

        method.Invoke(
            null,
            new object[]
            {
                layoutPath,
                false
            });
    }

    private static Readme SelectReadme()
    {
        string[] ids =
            AssetDatabase.FindAssets(
                "Readme t:Readme");

        if (ids.Length == 0)
        {
            return null;
        }

        string path =
            AssetDatabase.GUIDToAssetPath(
                ids[0]);

        Readme readme =
            AssetDatabase.LoadAssetAtPath<Readme>(
                path);

        if (readme != null)
        {
            Selection.activeObject = readme;
        }

        return readme;
    }

    protected override void OnHeaderGUI()
    {
        Readme readme =
            (Readme)target;

        InitStyles();

        DrawHeaderImage(readme);
        DrawBrandHeader(readme);
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        InitStyles();

        DrawHeaderSettings();

        GUILayout.Space(k_Space);

        DrawSeparator();

        GUILayout.Space(k_Space);

        DrawContentSettings();

        serializedObject.ApplyModifiedProperties();
    }

    private void DrawHeaderImage(Readme readme)
    {
        if (readme.headerImage == null)
        {
            return;
        }

        Rect rect =
            GUILayoutUtility.GetRect(
                0f,
                k_HeaderImageHeight,
                GUILayout.ExpandWidth(true));

        GUI.DrawTexture(
            rect,
            readme.headerImage,
            ScaleMode.ScaleAndCrop,
            true);

        GUILayout.Space(4f);
    }

    private void DrawBrandHeader(Readme readme)
    {
        Rect backgroundRect =
            GUILayoutUtility.GetRect(
                0f,
                k_BrandAreaHeight,
                GUILayout.ExpandWidth(true));

        EditorGUI.DrawRect(
            backgroundRect,
            EditorGUIUtility.isProSkin
                ? new Color(
                    0.22f,
                    0.22f,
                    0.22f,
                    1f)
                : new Color(
                    0.82f,
                    0.82f,
                    0.82f,
                    1f));

        float contentX =
            backgroundRect.x + 16f;

        float centerY =
            backgroundRect.center.y;

        if (readme.logo != null)
        {
            Rect logoRect =
                new Rect(
                    contentX,
                    centerY -
                    (k_LogoSize * 0.5f),
                    k_LogoSize,
                    k_LogoSize);

            GUI.DrawTexture(
                logoRect,
                readme.logo,
                ScaleMode.ScaleToFit,
                true);

            contentX +=
                k_LogoSize + 16f;
        }

        if (!string.IsNullOrEmpty(
                readme.title))
        {
            float titleWidth =
                Mathf.Max(
                    40f,
                    backgroundRect.xMax -
                    contentX -
                    12f);

            Rect titleRect =
                new Rect(
                    contentX,
                    backgroundRect.y,
                    titleWidth,
                    backgroundRect.height);

            GUI.Label(
                titleRect,
                readme.title,
                m_TitleStyle);
        }
    }

    private void DrawHeaderSettings()
    {
        GUILayout.Label(
            "HDRPLab Readme Header",
            m_HeadingStyle);

        GUILayout.Space(4f);

        SerializedProperty headerImage =
            serializedObject.FindProperty(
                "headerImage");

        SerializedProperty logo =
            serializedObject.FindProperty(
                "logo");

        SerializedProperty title =
            serializedObject.FindProperty(
                "title");

        EditorGUILayout.PropertyField(
            headerImage,
            new GUIContent(
                "Header Image",
                "Wide HDRPLab preview image shown at the top of the Readme Inspector."));

        EditorGUILayout.PropertyField(
            logo,
            new GUIContent(
                "Logo",
                "HDRPLabStudio logo displayed below the preview image."));

        EditorGUILayout.PropertyField(
            title,
            new GUIContent(
                "Title",
                "HDRPLabStudio title displayed beside the logo."));

        GUILayout.Space(8f);

        EditorGUILayout.HelpBox(
            "Assign the HDRPLab preview image and logo here. " +
            "Changes are reflected in the Readme header immediately.",
            MessageType.Info);
    }

    private void DrawContentSettings()
    {
        GUILayout.Label(
            "Content",
            m_HeadingStyle);

        GUILayout.Space(4f);

        SerializedProperty sections =
            serializedObject.FindProperty(
                "sections");

        EditorGUILayout.PropertyField(
            sections,
            new GUIContent(
                "Sections"),
            true);

        GUILayout.Space(k_Space);

        DrawSeparator();

        GUILayout.Space(k_Space);

        Readme readme =
            (Readme)target;

        DrawReadmePreview(
            readme);
    }

    private void DrawReadmePreview(
        Readme readme)
    {
        if (readme.sections == null)
        {
            return;
        }

        foreach (
            Readme.Section section
            in readme.sections)
        {
            if (section == null)
            {
                continue;
            }

            if (!string.IsNullOrEmpty(
                    section.heading))
            {
                GUILayout.Label(
                    section.heading,
                    m_HeadingStyle);
            }

            if (!string.IsNullOrEmpty(
                    section.text))
            {
                GUILayout.Space(3f);

                GUILayout.Label(
                    section.text,
                    m_BodyStyle);
            }

            if (!string.IsNullOrEmpty(
                    section.linkText))
            {
                GUILayout.Space(4f);

                if (LinkLabel(
                        new GUIContent(
                            section.linkText)))
                {
                    if (!string.IsNullOrEmpty(
                            section.url))
                    {
                        Application.OpenURL(
                            section.url);
                    }
                }
            }

            GUILayout.Space(
                k_Space);
        }
    }

    private void DrawSeparator()
    {
        Rect rect =
            EditorGUILayout.GetControlRect(
                false,
                1f);

        EditorGUI.DrawRect(
            rect,
            EditorGUIUtility.isProSkin
                ? new Color(
                    0.16f,
                    0.16f,
                    0.16f,
                    1f)
                : new Color(
                    0.65f,
                    0.65f,
                    0.65f,
                    1f));
    }

    private void InitStyles()
    {
        m_BodyStyle =
            new GUIStyle(
                EditorStyles.label);

        m_BodyStyle.wordWrap =
            true;

        m_BodyStyle.fontSize =
            14;

        m_BodyStyle.richText =
            true;

        m_HeadingStyle =
            new GUIStyle(
                m_BodyStyle);

        m_HeadingStyle.fontStyle =
            FontStyle.Bold;

        m_HeadingStyle.fontSize =
            18;

        m_TitleStyle =
            new GUIStyle(
                EditorStyles.boldLabel);

        m_TitleStyle.fontSize =
            21;

        m_TitleStyle.fontStyle =
            FontStyle.Bold;

        m_TitleStyle.wordWrap =
            false;

        m_TitleStyle.alignment =
            TextAnchor.MiddleLeft;

        m_TitleStyle.clipping =
            TextClipping.Clip;

        m_LinkStyle =
            new GUIStyle(
                m_BodyStyle);

        m_LinkStyle.wordWrap =
            false;

        m_LinkStyle.stretchWidth =
            false;

        m_LinkStyle.normal.textColor =
            new Color(
                0f,
                0.47f,
                0.85f,
                1f);
    }

    private bool LinkLabel(
        GUIContent label,
        params GUILayoutOption[] options)
    {
        Rect position =
            GUILayoutUtility.GetRect(
                label,
                m_LinkStyle,
                options);

        Handles.BeginGUI();

        Handles.color =
            m_LinkStyle.normal.textColor;

        Handles.DrawLine(
            new Vector3(
                position.xMin,
                position.yMax),
            new Vector3(
                position.xMax,
                position.yMax));

        Handles.color =
            Color.white;

        Handles.EndGUI();

        EditorGUIUtility.AddCursorRect(
            position,
            MouseCursor.Link);

        return GUI.Button(
            position,
            label,
            m_LinkStyle);
    }
}