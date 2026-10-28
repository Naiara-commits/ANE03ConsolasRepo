using System;
using UnityEngine;

public class Readme : ScriptableObject
{
    [Header("Header")]
    public Texture2D headerImage;
    public Texture2D logo;
    public string title = "HDRPLabStudio";

    [Header("Content")]
    public Section[] sections;

    [HideInInspector]
    public bool loadedLayout;

    [Serializable]
    public class Section
    {
        public string heading;

        [TextArea(3, 10)]
        public string text;

        public string linkText;
        public string url;
    }
}