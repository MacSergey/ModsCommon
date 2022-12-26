using ColossalFramework.UI;
using MonoMod.Utils;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using UnityEngine;

namespace ModsCommon.UI
{
    public class CustomUITextField : UITextField
    {
        //static FieldRef<UIFont, int> lineHeightFieldGetter;
        //static FieldRef<UITextField, float[]> charWidthsFieldGetter;
        //static FieldRef<UITextField, List<int>> linesFieldGetter;
        //static FieldRef<UITextField, int> cursorIndexFieldGetter;
        //static FieldRef<UITextField, int> scrollIndexFieldGetter;
        //static FieldRef<UITextField, int> lineScrollIndexFieldGetter;
        //static FieldRef<UITextField, float> leftOffsetFieldGetter;
        //static FieldRef<UITextField, bool> cursorShownFieldGetter;

        //public delegate ref F FieldRef<in T, F>(T instance);

        //static CustomUITextField()
        //{
        //    lineHeightFieldGetter = GetFieldRef<UIFont, int>("m_LineHeight");


        //    //DynamicMethodDefinition lineGetter = new DynamicMethodDefinition("m_LineHeightGetter", typeof(int).MakeByRefType(), new Type[] { typeof(UIFont) });

        //    //var generator = lineGetter.GetILGenerator();
        //    //generator.Emit(OpCodes.Ldarg_0);
        //    //generator.Emit(OpCodes.Ldflda, typeof(UIDynamicFont).GetField("m_LineHeight", BindingFlags.NonPublic | BindingFlags.Instance));
        //    //generator.Emit(OpCodes.Ret);

        //    //lineHeightFieldGetter = lineGetter.Generate().CreateDelegate<FieldRef<UIFont, int>>();
        //}
        //private static FieldRef<T, F> GetFieldRef<T, F>(string fieldName)
        //{
        //    DynamicMethodDefinition lineGetter = new DynamicMethodDefinition($"{typeof(T).Name}.{fieldName}Getter", typeof(F).MakeByRefType(), new Type[] { typeof(T) });

        //    var generator = lineGetter.GetILGenerator();
        //    generator.Emit(OpCodes.Ldarg_0);
        //    generator.Emit(OpCodes.Ldflda, typeof(T).GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance));
        //    generator.Emit(OpCodes.Ret);

        //    return lineGetter.Generate().CreateDelegate<FieldRef<T, F>>();
        //}

        private Vector3 positionBefore;
        public override void ResetLayout() => positionBefore = relativePosition;
        public override void PerformLayout()
        {
            if ((relativePosition - positionBefore).sqrMagnitude > 0.001)
                relativePosition = positionBefore;
        }

        protected override void OnKeyDown(UIKeyEventParameter p)
        {
            if (builtinKeyNavigation && !readOnly && !p.used)
            {
                if (p.keycode == KeyCode.Return && multiline)
                {
                    if (!p.shift)
                    {
                        multiline = false;
                        base.OnKeyDown(p);
                        multiline = true;
                        return;
                    }
                }
            }

            base.OnKeyDown(p);
        }

        //protected override void OnRebuildRenderData()
        //{
        //    ref var field = ref lineHeightFieldGetter(font);
        //    var tempHeight = field;
        //    field = Mathf.RoundToInt(font.lineHeight * textScale);
        //    base.OnRebuildRenderData();
        //    field = tempHeight;
        //}
        //protected override void OnRebuildRenderData()
        //{
        //    if (!multiline)
        //        base.OnRebuildRenderData();
        //    else if (!(base.atlas == null) && !(base.font == null) && base.font.isValid)
        //    {
        //        if (base.textRenderData != null)
        //        {
        //            base.textRenderData.Clear();
        //        }
        //        else
        //        {
        //            UIRenderData item = UIRenderData.Obtain();
        //            m_RenderData.Add(item);
        //        }

        //        base.renderData.material = base.atlas.material;
        //        base.textRenderData.material = base.atlas.material;
        //        RenderBackground();
        //        WrapText();
        //        RenderText();
        //    }
        //}
        //private void WrapText()
        //{
        //    ref var m_LineHeight = ref lineHeightFieldGetter(font);
        //    m_LineHeight = base.font.lineHeight;
        //    float num = PixelsToUnits();
        //    Vector2 maxSize = new Vector2(base.size.x - (float)padding.horizontal, base.size.y - (float)padding.vertical);
        //    Vector3 vector = base.pivot.TransformToUpperLeft(base.size, base.arbitraryPivotOffset);
        //    Vector3 vectorOffset = new Vector3(vector.x + (float)padding.left, vector.y - (float)padding.top, 0f) * num;
        //    string text = m_Text;
        //    string text2 = ((isPasswordField && !string.IsNullOrEmpty(passwordCharacter)) ? PasswordDisplayText(text) : text);
        //    Color32 defaultColor = (base.isEnabled ? base.textColor : base.disabledTextColor);
        //    float textScaleMultiplier = GetTextScaleMultiplier();
        //    using (UIFontRenderer uIFontRenderer = base.font.ObtainRenderer())
        //    {
        //        uIFontRenderer.wordWrap = false;
        //        uIFontRenderer.maxSize = maxSize;
        //        uIFontRenderer.pixelRatio = PixelsToUnits();
        //        uIFontRenderer.textScale = base.textScale * textScaleMultiplier;
        //        uIFontRenderer.characterSpacing = base.characterSpacing;
        //        uIFontRenderer.vectorOffset = vectorOffset;
        //        uIFontRenderer.multiLine = false;
        //        uIFontRenderer.textAlign = UIHorizontalAlignment.Left;
        //        uIFontRenderer.processMarkup = base.processMarkup;
        //        uIFontRenderer.colorizeSprites = base.colorizeSprites;
        //        uIFontRenderer.defaultColor = defaultColor;
        //        uIFontRenderer.bottomColor = (base.useGradient ? new Color32?(base.bottomColor) : null);
        //        uIFontRenderer.overrideMarkupColors = false;
        //        uIFontRenderer.opacity = CalculateOpacity();
        //        uIFontRenderer.outline = base.useOutline;
        //        uIFontRenderer.outlineSize = base.outlineSize;
        //        uIFontRenderer.outlineColor = base.outlineColor;
        //        uIFontRenderer.shadow = base.useDropShadow;
        //        uIFontRenderer.shadowColor = base.dropShadowColor;
        //        uIFontRenderer.shadowOffset = base.dropShadowOffset;
        //        charWidthsFieldGetter(this) = uIFontRenderer.GetCharacterWidths(text2);
        //        m_LineHeight = base.font.lineHeight;
        //    }

        //    ref var m_Lines = ref linesFieldGetter(this);
        //    if (m_Multiline)
        //    {
        //        List<int> words = GetWords();
        //        List<int> list = (m_Lines = CalculateLineBreaks(words));
        //    }
        //    else
        //    {
        //        m_Lines = new List<int>();
        //        m_Lines.Add(0);
        //    }
        //}
        //private void RenderText()
        //{
        //    ref var m_LineHeight = ref lineHeightFieldGetter(font);
        //    ref var m_CursorIndex = ref cursorIndexFieldGetter(this);
        //    ref var m_ScrollIndex = ref scrollIndexFieldGetter(this);
        //    ref var m_Lines = ref linesFieldGetter(this);
        //    ref var m_LineScrollIndex = ref lineScrollIndexFieldGetter(this);
        //    ref var m_CharWidths = ref charWidthsFieldGetter(this);
        //    ref var m_LeftOffset = ref leftOffsetFieldGetter(this);
        //    ref var m_CursorShown = ref cursorShownFieldGetter(this);

        //    float num = PixelsToUnits();
        //    Vector2 vector = new Vector2(base.size.x - (float)padding.horizontal, base.size.y - (float)padding.vertical);
        //    Vector3 vector2 = base.pivot.TransformToUpperLeft(base.size, base.arbitraryPivotOffset);
        //    Vector3 vector3 = new Vector3(vector2.x + (float)padding.left, vector2.y - (float)padding.top, 0f) * num;
        //    string text = m_Text;
        //    string text2 = ((isPasswordField && !string.IsNullOrEmpty(passwordCharacter)) ? PasswordDisplayText(text) : text);
        //    Color32 defaultColor = (base.isEnabled ? base.textColor : base.disabledTextColor);
        //    float textScaleMultiplier = GetTextScaleMultiplier();
        //    Vector2 vector4 = vector * num;
        //    int num2 = 0;
        //    if (m_Multiline)
        //    {
        //        m_CursorIndex = Mathf.Min(m_CursorIndex, text2.Length);
        //        m_ScrollIndex = 0;
        //        int num3 = Mathf.Max(1, Mathf.CeilToInt((base.size.y - (float)padding.vertical) / m_LineHeight) - 1);
        //        int lineByIndex = GetLineByIndex(m_CursorIndex, cursor: true);
        //        m_LineScrollIndex = Mathf.Min(m_LineScrollIndex, lineByIndex);
        //        m_LineScrollIndex = Mathf.Max(m_LineScrollIndex, lineByIndex - (num3 - 1));
        //        num2 = Mathf.Min(m_LineScrollIndex + num3 - 1, m_Lines.Count - 1);
        //    }
        //    else
        //    {
        //        m_CursorIndex = Mathf.Min(m_CursorIndex, text2.Length);
        //        m_ScrollIndex = Mathf.Min(Mathf.Min(m_ScrollIndex, m_CursorIndex), text2.Length);
        //        m_LineScrollIndex = 0;
        //        m_LeftOffset = 0f;
        //        if (horizontalAlignment == UIHorizontalAlignment.Left)
        //        {
        //            float num4 = 0f;
        //            for (int i = m_ScrollIndex; i < m_CursorIndex; i++)
        //            {
        //                num4 += m_CharWidths[i];
        //            }

        //            while (num4 >= vector4.x && m_ScrollIndex < m_CursorIndex)
        //            {
        //                num4 -= m_CharWidths[m_ScrollIndex++];
        //            }
        //        }
        //        else
        //        {
        //            m_ScrollIndex = Mathf.Max(0, Mathf.Min(m_CursorIndex, text2.Length - 1));
        //            float num5 = 0f;
        //            float num6 = (float)base.font.size * 1.25f * num;
        //            while (m_ScrollIndex > 0 && num5 < vector4.x - num6)
        //            {
        //                num5 += m_CharWidths[m_ScrollIndex--];
        //            }

        //            float num7 = ((text2.Length > 0) ? TextWidth(m_ScrollIndex, m_Text.Length) : 0f);
        //            switch (horizontalAlignment)
        //            {
        //                case UIHorizontalAlignment.Center:
        //                    m_LeftOffset = Mathf.Max(0f, (vector4.x - num7) * 0.5f);
        //                    break;
        //                case UIHorizontalAlignment.Right:
        //                    m_LeftOffset = Mathf.Max(0f, vector4.x - num7);
        //                    break;
        //            }
        //        }
        //    }

        //    if (selectionEnd != selectionStart)
        //    {
        //        RenderSelection();
        //    }

        //    for (int j = m_LineScrollIndex; j <= num2; j++)
        //    {
        //        using UIFontRenderer uIFontRenderer = base.font.ObtainRenderer();
        //        uIFontRenderer.wordWrap = false;
        //        uIFontRenderer.maxSize = vector;
        //        uIFontRenderer.pixelRatio = num;
        //        uIFontRenderer.textScale = base.textScale * textScaleMultiplier;
        //        uIFontRenderer.characterSpacing = base.characterSpacing;
        //        uIFontRenderer.vectorOffset = vector3;
        //        uIFontRenderer.multiLine = false;
        //        uIFontRenderer.textAlign = UIHorizontalAlignment.Left;
        //        uIFontRenderer.processMarkup = base.processMarkup;
        //        uIFontRenderer.colorizeSprites = base.colorizeSprites;
        //        uIFontRenderer.defaultColor = defaultColor;
        //        uIFontRenderer.bottomColor = (base.useGradient ? new Color32?(base.bottomColor) : null);
        //        uIFontRenderer.overrideMarkupColors = false;
        //        uIFontRenderer.opacity = CalculateOpacity();
        //        uIFontRenderer.outline = base.useOutline;
        //        uIFontRenderer.outlineSize = base.outlineSize;
        //        uIFontRenderer.outlineColor = base.outlineColor;
        //        uIFontRenderer.shadow = base.useDropShadow;
        //        uIFontRenderer.shadowColor = base.dropShadowColor;
        //        uIFontRenderer.shadowOffset = base.dropShadowOffset;
        //        if (m_Multiline)
        //        {
        //            string text3 = text2.Substring(m_Lines[j], LineLenght(j));
        //            Vector3 vector5 = new Vector3(CalculateLineLeftOffset(j), (float)(-(j - m_LineScrollIndex)) * m_LineHeight * num);
        //            Vector3 vector7 = (uIFontRenderer.vectorOffset = vector3 + vector5);
        //            uIFontRenderer.Render(text3, base.textRenderData);
        //        }
        //        else
        //        {
        //            vector3.x += m_LeftOffset;
        //            uIFontRenderer.vectorOffset = vector3;
        //            uIFontRenderer.Render(text2.Substring(m_ScrollIndex), base.textRenderData);
        //        }
        //    }

        //    if (m_CursorShown && selectionEnd == selectionStart)
        //    {
        //        RenderCursor();
        //    }
        //}


        //private List<int> GetWords()
        //{
        //    List<int> list = new List<int>();
        //    list.Add(0);
        //    int num = 0;
        //    bool flag = false;
        //    while (!flag)
        //    {
        //        int num2 = FindNextWord(num);
        //        for (int num3 = FindNextLineBreak(num); num3 < num2; num3 = FindNextLineBreak(num3 + 1))
        //        {
        //            if (num3 != 0)
        //            {
        //                list.Add(num3);
        //            }
        //        }

        //        if (num2 == m_Text.Length)
        //        {
        //            flag = true;
        //            continue;
        //        }

        //        list.Add(num2);
        //        num = num2;
        //    }

        //    return list;
        //}
        //private List<int> CalculateLineBreaks(List<int> words)
        //{
        //    List<int> list = new List<int>();
        //    list.Add(0);
        //    int num = 0;
        //    float num2 = (base.size.x - (float)padding.horizontal) * PixelsToUnits();
        //    int count = words.Count;
        //    for (int i = 0; i < count && words[i] != m_Text.Length; i++)
        //    {
        //        int num3 = ((i == count - 1) ? m_Text.Length : words[i + 1]);
        //        if (m_Text[words[i]] == '\n')
        //        {
        //            list.Add(words[i] + 1);
        //            num++;
        //        }
        //        else
        //        {
        //            if (!(TextWidth(list[num], num3) >= num2))
        //            {
        //                continue;
        //            }

        //            if (words[i] != list[num])
        //            {
        //                list.Add(words[i]);
        //                num++;
        //            }

        //            int num4 = list[num];
        //            for (int j = num4; j < num3; j++)
        //            {
        //                if (TextWidth(list[num], j + 1) >= num2)
        //                {
        //                    list.Add(j);
        //                    num++;
        //                }
        //            }
        //        }
        //    }

        //    return list;
        //}

        //private int FindNextLineBreak(int start)
        //{
        //    int i;
        //    for (i = start; i < m_Text.Length && m_Text[i] != '\n'; i++)
        //    {
        //    }

        //    return i;
        //}
        //private float TextWidth(int begin, int end)
        //{
        //    if (begin < 0 || end > m_Text.Length || end <= begin)
        //    {
        //        return 0f;
        //    }

        //    float num = 0f;
        //    ref var m_CharWidths = ref charWidthsFieldGetter(this);
        //    for (int i = begin; i < end && i != m_Text.Length; i++)
        //    {
        //        num += m_CharWidths[i];
        //    }

        //    return num;
        //}
        //private string PasswordDisplayText(string text)
        //{
        //    return new string(passwordCharacter[0], text.Length);
        //}
        //private int GetLineByIndex(int index, bool cursor = false)
        //{
        //    ref var m_Lines = ref linesFieldGetter(this);
        //    int count = m_Lines.Count;
        //    if (index == m_Text.Length)
        //    {
        //        return count - 1;
        //    }

        //    int i;
        //    for (i = 0; i < count && index >= m_Lines[i] + LineLenght(i); i++)
        //    {
        //    }

        //    if (cursor && m_CursorAtEndOfLine && index == m_Lines[i] && LineLenght(i) > 0)
        //    {
        //        return Mathf.Max(0, i - 1);
        //    }

        //    m_CursorAtEndOfLine = false;
        //    return i;
        //}
    }
}
