﻿using UnityEngine;
using UnityModManagerNet;

namespace SolastaUnfinishedBusiness.Api.ModKit;

internal static partial class UI
{
    private static Texture2D fillTexture;
    private static GUIStyle fillStyle;

    private static GUIStyle _buttonStyle;

    private static GUIStyle _largeStyle;
    private static GUIStyle _textBoxStyle;

    private static GUIStyle _toggleStyle;
    public static GUIStyle divStyle;

    private static Texture2D _rarityTexture;
    private static GUIStyle _rarityStyle;
    private static GUIStyle _rarityButtonStyle;

    private static Texture2D _submenuTexture;
    private static GUIStyle _submenuButtonStyle;
    private static Color FillColor { get; } = new(1f, 1f, 1f, 0.65f);

    public static GUIStyle buttonStyle
    {
        get
        {
            if (_buttonStyle == null)
            {
                _buttonStyle = new GUIStyle(GUI.skin.button) { alignment = TextAnchor.MiddleLeft };
            }

            return _buttonStyle;
        }
    }

    public static GUIStyle largeStyle
    {
        get
        {
            if (_largeStyle == null)
            {
                _largeStyle = new GUIStyle(GUI.skin.box) { richText = true };
            }

            _largeStyle.fixedHeight = 24.point();
            //_largeStyle.contentOffset = new Vector2(0, -6.point());
            _largeStyle.padding = new RectOffset(0, 0, -3.point(), 0);
#pragma warning disable CS0618 // Type or member is obsolete
            _largeStyle.clipOffset = new Vector2(0, 3.point());
#pragma warning restore CS0618 // Type or member is obsolete
            _largeStyle.fontSize = 21.point();
            _largeStyle.fontStyle = FontStyle.Bold;
            _largeStyle.normal.background = GUI.skin.label.normal.background;

            return _largeStyle;
        }
    }

    public static GUIStyle textBoxStyle
    {
        get
        {
            if (_textBoxStyle == null)
            {
                _textBoxStyle = new GUIStyle(GUI.skin.box) { richText = true };
            }

            _textBoxStyle.fontSize = 14.point();
            _textBoxStyle.fixedHeight = 19.point();
            _textBoxStyle.margin = new RectOffset(2.point(), 2.point(), 1.point(), 2.point());
            _textBoxStyle.padding = new RectOffset(2.point(), 2.point(), 0.point(), 0);
            _textBoxStyle.contentOffset = new Vector2(0, 2.point());
#pragma warning disable CS0618 // Type or member is obsolete
            _textBoxStyle.clipOffset = new Vector2(0, 2.point());
#pragma warning restore CS0618 // Type or member is obsolete

            return _textBoxStyle;
        }
    }

    public static GUIStyle toggleStyle
    {
        get
        {
            if (_toggleStyle == null)
            {
                _toggleStyle = new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleLeft };
            }

            return _toggleStyle;
        }
    }

    public static GUIStyle ToggleStyle => toggleStyle;

    public static Texture2D RarityTexture
    {
        get
        {
            if (_rarityTexture == null)
            {
                _rarityTexture = new Texture2D(1, 1);
            }

            _rarityTexture.SetPixel(0, 0, RGBA.Black.Color());
            _rarityTexture.Apply();
            return _rarityTexture;
        }
    }

    public static GUIStyle rarityStyle
    {
        get
        {
            if (_rarityStyle == null)
            {
                _rarityStyle = new GUIStyle(GUI.skin.button);
                _rarityStyle.normal.background = RarityTexture;
            }

            return _rarityStyle;
        }
    }

    public static GUIStyle rarityButtonStyle
    {
        get
        {
            if (_rarityButtonStyle == null)
            {
                _rarityButtonStyle = new GUIStyle(GUI.skin.button) { alignment = TextAnchor.MiddleLeft };
                _rarityButtonStyle.normal.background = RarityTexture;
            }

            return _rarityButtonStyle;
        }
    }

    public static Texture2D SubmenuTexture
    {
        get
        {
            if (_submenuTexture == null)
            {
                _submenuTexture = new Texture2D(1, 1);
                _submenuTexture.SetPixel(0, 0, new Color(0.3f, 0.3f, 0.3f, 0.65f));
            }

            _submenuTexture.Apply();
            return _submenuTexture;
        }
    }

    public static GUIStyle submenuButtonStyle
    {
        get
        {
            if (_submenuButtonStyle == null)
            {
                _submenuButtonStyle = new GUIStyle(GUI.skin.button);
#if false
                {
                    alignment = TextAnchor.MiddleLeft
                };

#endif
                _submenuButtonStyle.normal.background = SubmenuTexture;
            }

            return _submenuButtonStyle;
        }
    }

    public static int point(this int x)
    {
        return UnityModManager.UI.Scale(x);
    }

    public static int Point(this int x)
    {
        return UnityModManager.UI.Scale(x);
    }

    public static GUIStyle FillStyle(Color color)
    {
        if (fillTexture == null)
        {
            fillTexture = new Texture2D(1, 1);
        }

        if (fillStyle == null)
        {
            fillStyle = new GUIStyle();
        }

        fillTexture.SetPixel(0, 0, color);
        fillTexture.Apply();
        fillStyle.normal.background = fillTexture;
        return fillStyle;
    }

    public static void Div(Color color, float indent = 0, float height = 0, float width = 0)
    {
        if (fillTexture == null)
        {
            fillTexture = new Texture2D(1, 1);
        }

        //if (divStyle == null) {
        divStyle = new GUIStyle { fixedHeight = 1, };
        //}
        fillTexture.SetPixel(0, 0, color);
        fillTexture.Apply();
        divStyle.normal.background = fillTexture;
        if (divStyle.margin == null)
        {
            divStyle.margin = new RectOffset((int)indent, 0, 4, 4);
        }
        else
        {
            divStyle.margin.left = (int)indent + 3;
        }

        if (width > 0)
        {
            divStyle.fixedWidth = width;
        }
        else
        {
            divStyle.fixedWidth = 0;
        }

        Space(2f * height / 3f);
        GUILayout.Box(GUIContent.none, divStyle);
        Space(height / 3f);
    }
}
