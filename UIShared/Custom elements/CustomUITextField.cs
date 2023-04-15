using ColossalFramework;
using ColossalFramework.PlatformServices;
using ColossalFramework.UI;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace ModsCommon.UI
{
    public class CustomUITextField : UIInteractiveComponent
    {
        private struct UndoData
        {
            public string text;

            public int position;
        }

        private Vector3 positionBefore;
        public override void ResetLayout() => positionBefore = relativePosition;
        public override void PerformLayout()
        {
            if ((relativePosition - positionBefore).sqrMagnitude > 0.001)
                relativePosition = positionBefore;
        }


        #region PROPERTIES

        protected RectOffset padding;
        public RectOffset Padding
        {
            get => padding ??= new RectOffset();
            set
            {
                value = value.ConstrainPadding();
                if (!Equals(value, padding))
                {
                    padding = value;
                    Invalidate();
                }
            }
        }


        public float CursorBlinkTime { get; set; } = 0.45f;
        public int CursorWidth { get; set; } = 1;


        protected int maxLength = 1024;
        public int MaxLength
        {
            get => maxLength;
            set
            {
                value = Mathf.Max(0, value);
                if (value != maxLength)
                {
                    maxLength = value;
                    if (maxLength < m_Text.Length)
                        text = m_Text.Substring(0, maxLength);

                    Invalidate();
                }
            }
        }


        public bool SelectOnFocus { get; set; }
        public bool SubmitOnFocusLost { get; set; } = true;
        public OnUnfocus ActionOnUnfocus { get; set; } = OnUnfocus.Unfocus;
        public bool Multiline { get; set; }


        private int selectionStart;
        public int SelectionStart
        {
            get => selectionStart;
            set
            {
                if (value != selectionStart)
                {
                    selectionStart = Mathf.Max(0, Mathf.Min(value, m_Text.Length));
                    selectionEnd = Mathf.Max(selectionEnd, selectionStart);
                    Invalidate();
                }
            }
        }


        private int selectionEnd;
        public int SelectionEnd
        {
            get => selectionEnd;
            set
            {
                if (value != selectionEnd)
                {
                    selectionEnd = Mathf.Max(0, Mathf.Min(value, m_Text.Length));
                    selectionStart = Mathf.Max(selectionStart, selectionEnd);
                    Invalidate();
                }
            }
        }

        public int SelectionLength => selectionEnd - selectionStart;
        public string SelectedText => selectionEnd == selectionStart ? string.Empty : m_Text.Substring(selectionStart, SelectionLength);


        private int mouseSelectionAnchor;

        private int scrollIndex;

        private int lineIndex;

        private int lineScrollIndex;

        private int cursorIndex;

        private float leftOffset;

        private bool cursorShown;

        private float[] charWidths;

        private string undoText = string.Empty;

        private bool inProgress;

        private List<int> lines = new List<int>();

        private bool cursorAtEndOfLine;

        public static int kUndoLimit = 20;

        private List<UndoData> undoData = new List<UndoData>(kUndoLimit);

        private int undoCount;

        private bool undoInProgress;

        private bool hasImeInput;

        private string CompositionString => hasImeInput ? Input.compositionString : string.Empty;
        public override string text
        {
            get => m_Text;
            set
            {
                if (value.Length > maxLength)
                    value = value.Substring(0, maxLength);

                value = value.Replace("\t", " ");
                if (value != m_Text)
                {
                    m_Text = value;
                    scrollIndex = 0;
                    cursorIndex = 0;
                    TextChanged();
                    Invalidate();
                }
            }
        }


        #endregion

        #region EVENTS & HANDLERS


        public event Action<string> OnTextChanged;

        public event Action<string> OnTextSubmitted;

        public event Action<string> OnTextCancelled;

        public override void Update()
        {
            base.Update();

            if (!string.IsNullOrEmpty(Input.compositionString))
                Invalidate();
        }

        public override void OnEnable()
        {
            base.OnEnable();

            if (size.magnitude == 0f)
                size = new Vector2(100f, 20f);

            cursorShown = false;
            lineScrollIndex = 0;
            scrollIndex = 0;
            cursorIndex = 0;

            if (font == null || !font.isValid)
                font = GetUIView().defaultFont;
        }

        protected virtual void TextChanged()
        {
            if (!undoInProgress)
            {
                undoData.RemoveRange(undoData.Count - undoCount, undoCount);
                undoData.Add(new UndoData { text = text, position = cursorIndex });

                undoCount = 0;

                if (kUndoLimit != 0 && kUndoLimit <= undoData.Count)
                    undoData.RemoveAt(0);
            }

            OnTextChanged?.Invoke(text);
        }

        protected virtual void OnSubmit(OnUnfocus onUnfocus)
        {
            inProgress = true;
            Unfocus(onUnfocus);
            OnTextSubmitted?.Invoke(text);
        }

        protected virtual void OnCancel(OnUnfocus onUnfocus)
        {
            inProgress = true;
            m_Text = undoText;
            Unfocus(onUnfocus);
            OnTextCancelled?.Invoke(text);
        }

        protected override void OnKeyPress(UIKeyEventParameter p)
        {
            if (builtinKeyNavigation)
            {
                if (char.IsControl(p.character))
                {
                    base.OnKeyPress(p);
                    return;
                }

                base.OnKeyPress(p);
                if (p.used)
                    return;

                DeleteSelection();
                SetIMEPosition();

                if (m_Text.Length < maxLength)
                {
                    if (cursorIndex == m_Text.Length)
                        m_Text += p.character;
                    else
                        m_Text = m_Text.Insert(cursorIndex, p.character.ToString());

                    cursorIndex++;
                    TextChanged();
                    Invalidate();
                }

                p.Use();
            }
            else
                base.OnKeyPress(p);
        }

        protected override void OnKeyDown(UIKeyEventParameter p)
        {
            if (builtinKeyNavigation)
            {
                switch (p.keycode)
                {
                    case KeyCode.A:
                        if (p.control)
                            SelectAll();
                        break;

                    case KeyCode.Insert:
                        if (p.shift)
                        {
                            string text = Clipboard.text;
                            if (!string.IsNullOrEmpty(text))
                                PasteAtCursor(text);
                        }
                        break;

                    case KeyCode.V:
                        if (p.control)
                        {
                            string text2 = Clipboard.text;
                            if (!string.IsNullOrEmpty(text2))
                                PasteAtCursor(text2);
                        }
                        break;

                    case KeyCode.C:
                        if (p.control)
                            CopySelectionToClipboard();
                        break;

                    case KeyCode.X:
                        if (p.control)
                            CutSelectionToClipboard();
                        break;

                    case KeyCode.LeftArrow:
                        if (p.control)
                        {
                            if (p.shift)
                                MoveSelectionPointLeftWord();
                            else
                                MoveToPreviousWord();
                        }
                        else if (p.shift)
                            MoveSelectionPointLeft();
                        else if (SelectionLength > 0)
                            MoveToSelectionStart();
                        else
                            MoveToPreviousChar();
                        break;

                    case KeyCode.RightArrow:
                        if (p.control)
                        {
                            if (p.shift)
                                MoveSelectionPointRightWord();
                            else
                                MoveToNextWord();
                        }
                        else if (p.shift)
                            MoveSelectionPointRight();
                        else if (SelectionLength > 0)
                            MoveToSelectionEnd();
                        else
                            MoveToNextChar();
                        break;

                    case KeyCode.UpArrow:
                        if (Multiline)
                        {
                            if (p.shift)
                                MoveSelectionPointUp();
                            else
                                MoveToUpChar();
                        }
                        break;

                    case KeyCode.DownArrow:
                        if (Multiline)
                        {
                            if (p.shift)
                                MoveSelectionPointDown();
                            else
                                MoveToDownChar();
                        }
                        break;

                    case KeyCode.Home:
                        if (p.shift)
                            SelectToStart();
                        else
                            MoveToStart();
                        break;

                    case KeyCode.End:
                        if (p.shift)
                            SelectToEnd();
                        else
                            MoveToEnd();
                        break;

                    case KeyCode.Delete:
                        if (selectionStart != selectionEnd)
                            DeleteSelection();
                        else if (p.control)
                            DeleteNextWord();
                        else
                            DeleteNextChar();
                        break;

                    case KeyCode.Backspace:
                        if (p.control)
                            DeletePreviousWord();
                        else
                            DeletePreviousChar();
                        break;

                    case KeyCode.Escape:
                        Cancel();
                        break;

                    case KeyCode.Return:
                        if (Multiline && p.shift)
                            AddLineBreak();
                        else
                            OnSubmit(ActionOnUnfocus);
                        break;

                    case KeyCode.Z:
                        if (p.control)
                        {
                            undoInProgress = true;
                            try
                            {
                                undoCount += 1;
                                ClearSelection();
                                text = undoData[undoData.Count - undoCount - 1].text;
                                cursorIndex = undoData[undoData.Count - undoCount - 1].position;
                            }
                            catch { undoCount -= 1; }

                            undoInProgress = false;
                        }
                        break;

                    case KeyCode.Y:
                        if (p.control)
                        {
                            undoInProgress = true;
                            try
                            {
                                undoCount -= 1;
                                ClearSelection();
                                text = undoData[undoData.Count - undoCount - 1].text;
                                cursorIndex = undoData[undoData.Count - undoCount - 1].position;
                            }
                            catch { undoCount += 1; }

                            undoInProgress = false;
                        }
                        break;

                    default:
                        base.OnKeyDown(p);
                        return;
                }

                p.Use();
            }
            else
            {
                base.OnKeyDown(p);
            }
        }

        protected override void OnGotFocus(UIFocusEventParameter p)
        {
            base.OnGotFocus(p);

            hasImeInput = true;
            Input.imeCompositionMode = IMECompositionMode.On;
            SetIMEPosition();
            undoText = text;

            StartCoroutine(MakeCursorBlink());
            if (SelectOnFocus)
            {
                selectionStart = 0;
                selectionEnd = m_Text.Length;
            }

            Invalidate();
        }

        protected override void OnLostFocus(UIFocusEventParameter p)
        {
            base.OnLostFocus(p);

            hasImeInput = false;
            Input.imeCompositionMode = IMECompositionMode.Auto;

            if (!inProgress)
            {
                if (SubmitOnFocusLost)
                    OnSubmit(p.gotFocus != null ? OnUnfocus.None : ActionOnUnfocus);
                else
                    OnCancel(p.gotFocus != null ? OnUnfocus.None : ActionOnUnfocus);
            }

            inProgress = false;
            cursorShown = false;

            ClearSelection();
            Invalidate();
        }

        protected override void OnDoubleClick(UIMouseEventParameter p)
        {
            if (p.buttons.IsFlagSet(UIMouseButton.Left))
            {
                int charIndexAt = GetCharIndexAt(p);
                SelectWordAtIndex(charIndexAt);
            }

            base.OnDoubleClick(p);
        }

        protected override void OnClick(UIMouseEventParameter p)
        {
            p.Use();
            base.OnClick(p);
        }

        protected override void OnMouseDown(UIMouseEventParameter p)
        {
            if (p.buttons.IsFlagSet(UIMouseButton.Left))
            {
                int charIndexAt = GetCharIndexAt(p);
                if (charIndexAt != cursorIndex)
                {
                    cursorIndex = charIndexAt;
                    cursorShown = true;
                    Invalidate();
                    p.Use();
                }

                mouseSelectionAnchor = cursorIndex;
                selectionStart = cursorIndex;
                selectionEnd = cursorIndex;
            }

            base.OnMouseDown(p);
        }

        protected override void OnMouseUp(UIMouseEventParameter p)
        {
            if (p.buttons.IsFlagSet(UIMouseButton.Left) && PlatformService.ShowGamepadTextInput(GamepadTextInputMode.TextInputModeNormal, GamepadTextInputLineMode.TextInputLineModeSingleLine, "Input", maxLength, text))
            {
                p.Use();
                PlatformService.eventSteamGamepadInputDismissed += OnSteamInputDismissed;
            }

            base.OnMouseUp(p);
        }

        private void OnSteamInputDismissed(string str)
        {
            PlatformService.eventSteamGamepadInputDismissed -= OnSteamInputDismissed;
            if (str != null)
            {
                text = str;
                OnSubmit(ActionOnUnfocus);
            }

            MoveToEnd();
            Unfocus();
        }

        protected override void OnMouseMove(UIMouseEventParameter p)
        {
            if (hasFocus && p.buttons.IsFlagSet(UIMouseButton.Left))
            {
                int charIndexAt = GetCharIndexAt(p);
                if (charIndexAt != cursorIndex)
                {
                    cursorIndex = charIndexAt;
                    cursorShown = true;
                    Invalidate();
                    p.Use();
                    selectionStart = Mathf.Min(mouseSelectionAnchor, charIndexAt);
                    selectionEnd = Mathf.Max(mouseSelectionAnchor, charIndexAt);
                    return;
                }
            }

            base.OnMouseMove(p);
        }

        #endregion

        #region MISC

        private void Unfocus(OnUnfocus onUnfocus)
        {
            switch (onUnfocus)
            {
                case OnUnfocus.Unfocus:
                    Unfocus();
                    break;
                case OnUnfocus.FocusRoot:
                    if (this.GetRoot() is UIComponent root)
                        root.Focus();
                    else
                        Unfocus();
                    break;
            }
        }

        public void Cancel()
        {
            ClearSelection();
            scrollIndex = 0;
            cursorIndex = 0;
            Invalidate();
            OnCancel(ActionOnUnfocus);
        }

        private void SetIMEPosition()
        {
            var view = GetUIView();
            var ratio = view.PixelsToUnits();
            var pos = 0f;
            for (int i = scrollIndex; i < cursorIndex && i < charWidths.Length; i++)
                pos += charWidths[i] / ratio;

            var startPos = pivot.TransformToUpperLeft(size, arbitraryPivotOffset);
            startPos = new Vector3(startPos.x + Padding.left, startPos.y - Padding.top, 0f);

            float num3 = pos + leftOffset / ratio + Padding.left;
            var xRatio = (float)view.uiCamera.pixelWidth / (float)view.fixedWidth;
            var yRatio = (float)view.uiCamera.pixelHeight / (float)view.fixedHeight;
            var screenPos = view.uiCamera.WorldToScreenPoint(transform.position);
            screenPos.y = Screen.height - screenPos.y;
            Input.compositionCursorPos = new Vector2(screenPos.x + (startPos.x + num3) * xRatio, screenPos.y + (startPos.y + size.y * 1.5f) * yRatio);
        }

        private int GetCharIndexAt(UIMouseEventParameter p)
        {
            var hitPosition = GetHitPosition(p);
            var ratio = PixelsToUnits();
            var index = 0;

            if (Multiline)
            {
                var line = GetLineByVerticalPosition(hitPosition.y);
                index = GetIndexByHorizontalPosition(hitPosition.x, line);
            }
            else
            {
                index = scrollIndex;
                var offset = leftOffset / ratio + Padding.left;
                for (int i = scrollIndex; i < charWidths.Length; i++)
                {
                    offset += charWidths[i] / ratio;
                    if (offset < hitPosition.x)
                        index += 1;
                }
            }

            return index;
        }

        private IEnumerator MakeCursorBlink()
        {
            if (Application.isPlaying)
            {
                cursorShown = true;
                while (hasFocus)
                {
                    yield return new WaitForSeconds(CursorBlinkTime);
                    cursorShown = !cursorShown;
                    Invalidate();
                }

                cursorShown = false;
            }
        }

        public void ClearSelection()
        {
            selectionStart = 0;
            selectionEnd = 0;
            mouseSelectionAnchor = 0;
        }

        public void MoveToStart()
        {
            ClearSelection();
            SetCursorPos(0);
        }

        public void MoveToEnd()
        {
            ClearSelection();
            SetCursorPos(m_Text.Length);
        }

        public void MoveToNextChar()
        {
            ClearSelection();
            SetCursorPos(cursorIndex + 1);
        }

        public void MoveToPreviousChar()
        {
            ClearSelection();
            SetCursorPos(cursorIndex - 1);
        }

        public void MoveToNextWord()
        {
            ClearSelection();
            if (cursorIndex != m_Text.Length)
            {
                int cursorPos = FindNextWord(cursorIndex);
                SetCursorPos(cursorPos);
            }
        }

        public void MoveToPreviousWord()
        {
            ClearSelection();
            if (cursorIndex != 0)
            {
                int cursorPos = FindPreviousWord(cursorIndex);
                SetCursorPos(cursorPos);
            }
        }

        public int FindPreviousWord(int startIndex)
        {
            var index = 0;
            for (index = startIndex; index > 0; index -= 1)
            {
                char c = m_Text[index - 1];
                if (!char.IsWhiteSpace(c) && !char.IsSeparator(c) && !char.IsPunctuation(c))
                    break;
            }

            for (var endIndex = index; endIndex >= 0; endIndex -= 1)
            {
                if (endIndex == 0)
                {
                    index = 0;
                    break;
                }

                char c = m_Text[endIndex - 1];
                if (char.IsWhiteSpace(c) || char.IsSeparator(c) || char.IsPunctuation(c))
                {
                    index = endIndex;
                    break;
                }
            }

            return index;
        }

        public int FindNextWord(int startIndex)
        {
            var length = m_Text.Length;
            var index = startIndex;

            for (var endIndex = index; endIndex < length; endIndex += 1)
            {
                char c = m_Text[endIndex];
                if (char.IsWhiteSpace(c) || char.IsSeparator(c) || char.IsPunctuation(c))
                {
                    index = endIndex;
                    break;
                }

                if (endIndex == length - 1)
                    index = length;
            }

            for (; index < length; index += 1)
            {
                char c = m_Text[index];
                if (!char.IsWhiteSpace(c) && !char.IsSeparator(c) && !char.IsPunctuation(c))
                    break;
            }

            return index;
        }

        public void MoveSelectionPointRightWord()
        {
            if (cursorIndex != m_Text.Length)
            {
                int cursorPos = FindNextWord(cursorIndex);
                if (selectionEnd == selectionStart)
                {
                    selectionStart = cursorIndex;
                    selectionEnd = cursorPos;
                }
                else if (selectionEnd == cursorIndex)
                    selectionEnd = cursorPos;
                else if (selectionStart == cursorIndex)
                    selectionStart = cursorPos;

                SetCursorPos(cursorPos);
            }
        }

        public void MoveSelectionPointLeftWord()
        {
            if (cursorIndex != 0)
            {
                int cursorPos = FindPreviousWord(cursorIndex);
                if (selectionEnd == selectionStart)
                {
                    selectionEnd = cursorIndex;
                    selectionStart = cursorPos;
                }
                else if (selectionEnd == cursorIndex)
                    selectionEnd = cursorPos;
                else if (selectionStart == cursorIndex)
                    selectionStart = cursorPos;

                SetCursorPos(cursorPos);
            }
        }

        public void MoveSelectionPointRight()
        {
            if (cursorIndex != m_Text.Length)
            {
                if (selectionEnd == selectionStart)
                {
                    selectionEnd = cursorIndex + 1;
                    selectionStart = cursorIndex;
                }
                else if (selectionEnd == cursorIndex)
                    selectionEnd += 1;
                else if (selectionStart == cursorIndex)
                    selectionStart += 1;

                SetCursorPos(cursorIndex + 1);
            }
        }

        public void MoveSelectionPointLeft()
        {
            if (cursorIndex != 0)
            {
                if (selectionEnd == selectionStart)
                {
                    selectionEnd = cursorIndex;
                    selectionStart = cursorIndex - 1;
                }
                else if (selectionEnd == cursorIndex)
                    selectionEnd -= 1;
                else if (selectionStart == cursorIndex)
                    selectionStart -= 1;

                SetCursorPos(cursorIndex - 1);
            }
        }

        public void MoveToSelectionEnd()
        {
            var cursorPos = selectionEnd;
            ClearSelection();
            SetCursorPos(cursorPos);
        }

        public void MoveToSelectionStart()
        {
            var cursorPos = selectionStart;
            ClearSelection();
            SetCursorPos(cursorPos);
        }

        public void SelectAll()
        {
            selectionStart = 0;
            selectionEnd = m_Text.Length;
            scrollIndex = 0;
            SetCursorPos(0);
        }

        public void SelectToStart()
        {
            if (cursorIndex != 0)
            {
                if (selectionEnd == selectionStart)
                    selectionEnd = cursorIndex;
                else if (selectionEnd == cursorIndex)
                    selectionEnd = selectionStart;

                selectionStart = 0;
                SetCursorPos(0);
            }
        }

        public void SelectToEnd()
        {
            if (cursorIndex != m_Text.Length)
            {
                if (selectionEnd == selectionStart)
                    selectionStart = cursorIndex;
                else if (selectionStart == cursorIndex)
                    selectionStart = selectionEnd;

                selectionEnd = m_Text.Length;
                SetCursorPos(m_Text.Length);
            }
        }

        public void SelectWordAtIndex(int index)
        {
            if (m_Text.Length == 0)
                return;

            index = Mathf.Max(Mathf.Min(m_Text.Length - 1, index), 0);
            char c = m_Text[index];
            if (!char.IsLetterOrDigit(c))
            {
                selectionStart = index;
                selectionEnd = index + 1;
                mouseSelectionAnchor = 0;
            }
            else
            {
                selectionStart = index;
                for (var i = index; i > 0 && char.IsLetterOrDigit(m_Text[i - 1]); i -= 1)
                {
                    selectionStart -= 1;
                }

                selectionEnd = index;
                for (var i = index; i < m_Text.Length && char.IsLetterOrDigit(m_Text[i]); i += 1)
                {
                    selectionEnd = i + 1;
                }
            }

            cursorIndex = selectionStart;
            Invalidate();
        }

        private void CutSelectionToClipboard()
        {
            CopySelectionToClipboard();
            DeleteSelection();
        }

        private void CopySelectionToClipboard()
        {
            if (selectionStart != selectionEnd)
                Clipboard.text = m_Text.Substring(selectionStart, SelectionLength);
        }

        private void PasteAtCursor(string clipData)
        {
            DeleteSelection();
            var stringBuilder = new StringBuilder(m_Text.Length + clipData.Length);
            stringBuilder.Append(m_Text);
            foreach (char c in clipData)
            {
                if (c >= ' ' || (Multiline && c == '\n'))
                    stringBuilder.Insert(cursorIndex++, c);
            }

            stringBuilder.Length = Mathf.Min(stringBuilder.Length, maxLength);
            m_Text = stringBuilder.ToString();
            TextChanged();
            SetCursorPos(cursorIndex);
        }

        private void SetCursorPos(int index)
        {
            index = Mathf.Max(0, Mathf.Min(m_Text.Length, index));
            if (index != cursorIndex)
            {
                cursorIndex = index;
                cursorShown = hasFocus;
                scrollIndex = Mathf.Min(scrollIndex, cursorIndex);
                Invalidate();
            }
        }

        private void DeleteSelection()
        {
            if (selectionStart != selectionEnd)
            {
                m_Text = m_Text.Remove(selectionStart, SelectionLength);
                SetCursorPos(selectionStart);
                ClearSelection();
                TextChanged();
                Invalidate();
            }
        }

        private void DeleteNextChar()
        {
            ClearSelection();
            if (cursorIndex < m_Text.Length)
            {
                m_Text = m_Text.Remove(cursorIndex, 1);
                cursorShown = true;
                TextChanged();
                Invalidate();
            }
        }

        private void DeletePreviousChar()
        {
            if (selectionStart != selectionEnd)
            {
                int cursorPos = selectionStart;
                DeleteSelection();
                SetCursorPos(cursorPos);
                return;
            }

            ClearSelection();
            if (cursorIndex != 0)
            {
                m_Text = m_Text.Remove(cursorIndex - 1, 1);
                cursorIndex -= 1;
                cursorShown = true;
                TextChanged();
                Invalidate();
            }
        }

        private void DeleteNextWord()
        {
            ClearSelection();
            if (cursorIndex != m_Text.Length)
            {
                var index = FindNextWord(cursorIndex);
                if (index == cursorIndex)
                    index = m_Text.Length;

                m_Text = m_Text.Remove(cursorIndex, index - cursorIndex);
                TextChanged();
                Invalidate();
            }
        }

        private void DeletePreviousWord()
        {
            ClearSelection();
            if (cursorIndex != 0)
            {
                var index = FindPreviousWord(cursorIndex);
                if (index == cursorIndex)
                    index = 0;

                m_Text = m_Text.Remove(index, cursorIndex - index);
                TextChanged();
                SetCursorPos(index);
            }
        }

        public void MoveToUpChar()
        {
            ClearSelection();
            int cursorPos = FindUpperIndex(cursorIndex);
            SetCursorPos(cursorPos);
        }

        public void MoveToDownChar()
        {
            ClearSelection();
            int cursorPos = FindLowerIndex(cursorIndex);
            SetCursorPos(cursorPos);
        }

        public void MoveSelectionPointDown()
        {
            var index = FindLowerIndex(cursorIndex);

            if (selectionEnd == selectionStart)
            {
                selectionEnd = index;
                selectionStart = cursorIndex;
            }
            else if (selectionEnd == cursorIndex)
                selectionEnd = index;
            else if (selectionStart == cursorIndex)
            {
                if (index <= selectionEnd)
                    selectionStart = index;
                else
                {
                    selectionStart = selectionEnd;
                    selectionEnd = index;
                }
            }

            SetCursorPos(index);
        }

        public void MoveSelectionPointUp()
        {
            var index = FindUpperIndex(cursorIndex);

            if (selectionEnd == selectionStart)
            {
                selectionStart = index;
                selectionEnd = cursorIndex;
            }
            else if (selectionStart == cursorIndex)
                selectionStart = index;
            else if (selectionEnd == cursorIndex)
            {
                if (index >= selectionStart)
                    selectionEnd = index;
                else
                {
                    selectionEnd = selectionStart;
                    selectionStart = index;
                }
            }

            SetCursorPos(index);
        }

        private int FindLowerIndex(int index)
        {
            var lineByIndex = GetLineByIndex(index, cursor: true);
            if (lineByIndex >= lines.Count - 1)
                return m_Text.Length;

            var horizontalPositionByIndex = GetHorizontalPositionByIndex(index);
            return GetIndexByHorizontalPosition(horizontalPositionByIndex, lineByIndex + 1);
        }

        private int FindUpperIndex(int index)
        {
            var lineByIndex = GetLineByIndex(index, cursor: true);
            if (lineByIndex <= 0)
                return 0;

            var horizontalPositionByIndex = GetHorizontalPositionByIndex(index);
            return GetIndexByHorizontalPosition(horizontalPositionByIndex, lineByIndex - 1);
        }

        private int GetIndexByHorizontalPosition(float position, int line)
        {
            if (line < 0)
                return 0;

            if (line >= lines.Count)
                return m_Text.Length;

            var ratio = PixelsToUnits();
            var offset = Padding.left + CalculateLineLeftOffset(line) / ratio;
            var index = lines[line];
            var num3 = lines[line] + LineLenght(line);
            for (; index < num3; index += 1)
            {
                offset += charWidths[index] / ratio;
                if (offset > position || m_Text[index] == '\n')
                    break;
            }

            if (index == num3)
                cursorAtEndOfLine = true;

            return index;
        }

        private float GetHorizontalPositionByIndex(int index)
        {
            var lineByIndex = GetLineByIndex(index);
            var ratio = PixelsToUnits();
            var offset = CalculateLineLeftOffset(lineByIndex) / ratio + Padding.left;
            for (int i = lines[lineByIndex]; i < index; i++)
            {
                offset += charWidths[i] / ratio;
            }

            return offset;
        }

        private void AddLineBreak()
        {
            if (m_Text.Length < maxLength)
            {
                DeleteSelection();

                if (cursorIndex == m_Text.Length)
                    m_Text += '\n';
                else
                    m_Text = m_Text.Insert(cursorIndex, '\n'.ToString());

                cursorIndex += 1;
                TextChanged();
                Invalidate();
            }
        }

        private int GetLineByVerticalPosition(float position)
        {
            var line = lineScrollIndex + Mathf.FloorToInt((position - Padding.top) / (font.lineHeight * textScale)) + scrollIndex;
            return Mathf.Clamp(line, 0, lines.Count - 1);
        }

        #endregion

        #region STYLE

        UITextureAtlas bgAtlas;
        public UITextureAtlas BgAtlas
        {
            get => bgAtlas ?? atlas;
            set
            {
                if (!Equals(value, bgAtlas))
                {
                    bgAtlas = value;
                    Invalidate();
                }
            }
        }


        UITextureAtlas fgAtlas;
        public UITextureAtlas FgAtlas
        {
            get => fgAtlas ?? atlas;
            set
            {
                if (!Equals(value, fgAtlas))
                {
                    fgAtlas = value;
                    Invalidate();
                }
            }
        }

        UITextureAtlas selAtlas;
        public UITextureAtlas SelAtlas
        {
            get => selAtlas ?? atlas;
            set
            {
                if (!Equals(value, selAtlas))
                {
                    selAtlas = value;
                    Invalidate();
                }
            }
        }


        [Obsolete]
        public new Color32 color
        {
            get => base.color;
            set
            {
                bgColors = value;
                fgColors = value;
                base.color = value;
            }
        }
        [Obsolete]
        public new string normalFgSprite
        {
            get => base.normalFgSprite;
            set => base.normalFgSprite = value;
        }
        [Obsolete]
        public new string hoveredFgSprite
        {
            get => base.hoveredFgSprite;
            set => base.hoveredFgSprite = value;
        }
        [Obsolete]
        public new string disabledFgSprite
        {
            get => base.disabledFgSprite;
            set => base.disabledFgSprite = value;
        }
        [Obsolete]
        public new string focusedFgSprite
        {
            get => base.focusedFgSprite;
            set => base.focusedFgSprite = value;
        }
        [Obsolete]
        public new string normalBgSprite
        {
            get => base.normalBgSprite;
            set => base.normalBgSprite = value;
        }
        [Obsolete]
        public new string hoveredBgSprite
        {
            get => base.hoveredBgSprite;
            set => base.hoveredBgSprite = value;
        }
        [Obsolete]
        public new string disabledBgSprite
        {
            get => base.disabledBgSprite;
            set => base.disabledBgSprite = value;
        }
        [Obsolete]
        public new string focusedBgSprite
        {
            get => base.focusedBgSprite;
            set => base.focusedBgSprite = value;
        }


        private bool bold;
        public bool Bold
        {
            get => bold;
            set
            {
                if (value != bold)
                {
                    bold = value;
                    font = value ? ComponentStyle.SemiBoldFont : ComponentStyle.RegularFont;
                    Invalidate();
                }
            }
        }


        #region BACKGROUND SPRITE

        protected UI.SpriteSet bgSprites;
        public UI.SpriteSet BgSprites
        {
            get => bgSprites;
            set
            {
                bgSprites = value;
                Invalidate();
            }
        }
        public string NormalBgSprite
        {
            get => bgSprites.normal;
            set
            {
                if (value != bgSprites.normal)
                {
                    bgSprites.normal = value;
                    Invalidate();
                }
            }
        }
        public string HoveredBgSprite
        {
            get => bgSprites.hovered;
            set
            {
                if (value != bgSprites.hovered)
                {
                    bgSprites.hovered = value;
                    Invalidate();
                }
            }
        }
        public string FocusedBgSprite
        {
            get => bgSprites.focused;
            set
            {
                if (value != bgSprites.focused)
                {
                    bgSprites.focused = value;
                    Invalidate();
                }
            }
        }
        public string DisabledBgSprite
        {
            get => bgSprites.disabled;
            set
            {
                if (value != bgSprites.disabled)
                {
                    bgSprites.disabled = value;
                    Invalidate();
                }
            }
        }

        #endregion

        #region FOREGROUND SPRITE

        protected UI.SpriteSet fgSprites;
        public UI.SpriteSet FgSprites
        {
            get => fgSprites;
            set
            {
                fgSprites = value;
                Invalidate();
            }
        }

        public string NormalFgSprite
        {
            get => fgSprites.normal;
            set
            {
                if (value != fgSprites.normal)
                {
                    fgSprites.normal = value;
                    Invalidate();
                }
            }
        }
        public string HoveredFgSprite
        {
            get => fgSprites.hovered;
            set
            {
                if (value != fgSprites.hovered)
                {
                    fgSprites.hovered = value;
                    Invalidate();
                }
            }
        }
        public string FocusedFgSprite
        {
            get => fgSprites.focused;
            set
            {
                if (value != fgSprites.focused)
                {
                    fgSprites.focused = value;
                    Invalidate();
                }
            }
        }
        public string DisabledFgSprite
        {
            get => fgSprites.disabled;
            set
            {
                if (value != fgSprites.disabled)
                {
                    fgSprites.disabled = value;
                    Invalidate();
                }
            }
        }

        #endregion

        #region BACKGROUND COLOR

        protected ColorSet bgColors = new ColorSet(Color.white);
        public ColorSet BgColors
        {
            get => bgColors;
            set
            {
                bgColors = value;
                OnColorChanged();
            }
        }

        public Color32 NormalBgColor
        {
            get => bgColors.normal;
            set
            {
                if (!bgColors.normal.Equals(value))
                {
                    bgColors.normal = value;
                    OnColorChanged();
                }
            }
        }
        public Color32 FocusedBgColor
        {
            get => bgColors.focused;
            set
            {
                if (!bgColors.focused.Equals(value))
                {
                    bgColors.focused = value;
                    OnColorChanged();
                }
            }
        }
        public Color32 HoveredBgColor
        {
            get => bgColors.hovered;
            set
            {
                if (!bgColors.hovered.Equals(value))
                {
                    bgColors.hovered = value;
                    OnColorChanged();
                }
            }
        }
        public Color32 DisabledBgColor
        {
            get => bgColors.disabled;
            set
            {
                if (!bgColors.disabled.Equals(value))
                {
                    bgColors.disabled = value;
                    OnColorChanged();
                }
            }
        }

        #endregion

        #region FOREGROUND COLOR

        protected ColorSet fgColors = new ColorSet(Color.white);
        public ColorSet FgColors
        {
            get => fgColors;
            set
            {
                fgColors = value;
                OnColorChanged();
            }
        }

        public Color32 NormalFgColor
        {
            get => fgColors.normal;
            set
            {
                if (!fgColors.normal.Equals(value))
                {
                    fgColors.normal = value;
                    OnColorChanged();
                }
            }
        }
        public Color32 FocusedFgColor
        {
            get => fgColors.focused;
            set
            {
                if (!fgColors.focused.Equals(value))
                {
                    fgColors.focused = value;
                    OnColorChanged();
                }
            }
        }
        public Color32 HoveredFgColor
        {
            get => fgColors.hovered;
            set
            {
                if (!fgColors.hovered.Equals(value))
                {
                    fgColors.hovered = value;
                    OnColorChanged();
                }
            }
        }
        public Color32 DisabledFgColor
        {
            get => fgColors.disabled;
            set
            {
                if (!fgColors.disabled.Equals(value))
                {
                    fgColors.disabled = value;
                    OnColorChanged();
                }
            }
        }

        #endregion


        protected Color32 selBgColor = new Color32(0, 105, 210, byte.MaxValue);
        public Color32 SelBgColor
        {
            get => selBgColor;
            set
            {
                selBgColor = value;
                Invalidate();
            }
        }


        protected string selSprite = string.Empty;
        public string SelSprite
        {
            get => selSprite;
            set
            {
                if (value != selSprite)
                {
                    selSprite = value;
                    Invalidate();
                }
            }
        }

        public TextFieldStyle TextFieldStyle
        {
            set
            {
                bgAtlas = value.BgAtlas;
                fgAtlas = value.FgAtlas;

                bgSprites = value.BgSprites;
                fgSprites = value.FgSprites;

                bgColors = value.BgColors;
                fgColors = value.FgColors;

                m_TextColor = value.TextColors.normal;
                m_DisabledTextColor = value.TextColors.disabled;

                selAtlas = value.SelAtlas;
                selSprite = value.SelectionSprite;
                selBgColor = value.SelectionColor;

                Invalidate();
            }
        }

        #endregion

        #region RENDER

        protected UIRenderData BgRenderData { get; set; }
        protected UIRenderData FgRenderData { get; set; }
        protected UIRenderData SelRenderData { get; set; }
        protected UIRenderData TextRenderData { get; set; }

        public override void OnDisable()
        {
            BgRenderData = null;
            FgRenderData = null;
            SelRenderData = null;
            TextRenderData = null;
            base.OnDisable();
        }

        protected override void OnRebuildRenderData()
        {
            if (BgRenderData == null)
            {
                BgRenderData = UIRenderData.Obtain();
                m_RenderData.Add(BgRenderData);
            }
            else
                BgRenderData.Clear();

            if (SelRenderData == null)
            {
                SelRenderData = UIRenderData.Obtain();
                m_RenderData.Add(SelRenderData);
            }
            else
                SelRenderData.Clear();

            if (TextRenderData == null)
            {
                TextRenderData = UIRenderData.Obtain();
                m_RenderData.Add(TextRenderData);
            }
            else
                TextRenderData.Clear();

            if (FgRenderData == null)
            {
                FgRenderData = UIRenderData.Obtain();
                m_RenderData.Add(FgRenderData);
            }
            else
                FgRenderData.Clear();


            RenderBackground();

            if (atlas != null && font != null && font.isValid)
            {
                TextRenderData.material = atlas.material;
                CalculateText();

                RenderSelection();
                RenderText();
                RenderCursor();
            }

            RenderForeground();
        }

        protected override void RenderBackground()
        {
            if (BgAtlas is UITextureAtlas bgAtlas && RenderBackgroundSprite is UITextureAtlas.SpriteInfo backgroundSprite)
            {
                BgRenderData.material = bgAtlas.material;

                var renderOptions = new RenderOptions()
                {
                    atlas = bgAtlas,
                    color = RenderBackgroundColor,
                    fillAmount = 1f,
                    flip = UISpriteFlip.None,
                    offset = pivot.TransformToUpperLeft(size, arbitraryPivotOffset),
                    pixelsToUnits = PixelsToUnits(),
                    size = size,
                    spriteInfo = backgroundSprite,
                };

                if (backgroundSprite.isSliced)
                    Render.RenderSlicedSprite(BgRenderData, renderOptions);
                else
                    Render.RenderSprite(BgRenderData, renderOptions);
            }
        }
        protected virtual UITextureAtlas.SpriteInfo RenderBackgroundSprite
        {
            get
            {
                if (BgAtlas is UITextureAtlas atlas)
                {
                    if (!isEnabled)
                        return atlas[DisabledBgSprite];
                    else if (hasFocus)
                        return atlas[FocusedBgSprite] ?? atlas[NormalBgSprite];
                    else if (m_IsMouseHovering)
                        return atlas[HoveredBgSprite] ?? atlas[NormalBgSprite];
                    else
                        return atlas[NormalBgSprite];
                }

                return null;
            }
        }
        protected virtual Color32 RenderBackgroundColor
        {
            get
            {
                if (!isEnabled)
                    return DisabledBgColor;
                else if (hasFocus)
                    return FocusedBgColor;
                else if (m_IsMouseHovering)
                    return HoveredBgColor;
                else
                    return NormalBgColor;
            }
        }

        protected override void RenderForeground()
        {
            if (FgAtlas is UITextureAtlas fgAtlas && RenderForegroundSprite is UITextureAtlas.SpriteInfo foregroundSprite)
            {
                FgRenderData.material = fgAtlas.material;

                var foregroundRenderSize = GetForegroundRenderSize(foregroundSprite);
                var foregroundRenderOffset = GetForegroundRenderOffset(foregroundRenderSize);

                var renderOptions = new RenderOptions()
                {
                    atlas = fgAtlas,
                    color = RenderForegroundColor,
                    fillAmount = 1f,
                    flip = UISpriteFlip.None,
                    offset = foregroundRenderOffset,
                    pixelsToUnits = PixelsToUnits(),
                    size = foregroundRenderSize,
                    spriteInfo = foregroundSprite,
                };

                if (foregroundSprite.isSliced)
                    Render.RenderSlicedSprite(FgRenderData, renderOptions);
                else
                    Render.RenderSprite(FgRenderData, renderOptions);
            }
        }

        protected virtual UITextureAtlas.SpriteInfo RenderForegroundSprite
        {
            get
            {
                if (FgAtlas is UITextureAtlas atlas)
                {
                    if (!isEnabled)
                        return atlas[DisabledFgSprite];
                    else if (hasFocus)
                        return atlas[FocusedFgSprite];
                    else if (m_IsMouseHovering)
                        return atlas[HoveredFgSprite];
                    else
                        return atlas[NormalFgSprite];
                }

                return null;
            }
        }
        protected virtual Color32 RenderForegroundColor
        {
            get
            {
                if (!isEnabled)
                    return DisabledFgColor;
                else if (hasFocus)
                    return FocusedFgColor;
                else if (m_IsMouseHovering)
                    return HoveredFgColor;
                else
                    return NormalFgColor;
            }
        }

        private void CalculateText()
        {
            var lineHeight = font.lineHeight * textScale;
            var ratio = PixelsToUnits();
            var maxSize = new Vector2(size.x - Padding.horizontal, size.y - Padding.vertical);
            var maxUnitSize = maxSize * ratio;

            var offset = pivot.TransformToUpperLeft(size, arbitraryPivotOffset);
            offset = new Vector3(offset.x + Padding.left, offset.y - Padding.top, 0f) * ratio;
            var text = m_Text + CompositionString;

            var defaultColor = isEnabled ? textColor : disabledTextColor;
            using (UIFontRenderer renderer = font.ObtainRenderer())
            {
                renderer.wordWrap = false;
                renderer.maxSize = maxSize;
                renderer.pixelRatio = PixelsToUnits();
                renderer.textScale = textScale * GetTextScaleMultiplier();
                renderer.characterSpacing = characterSpacing;
                renderer.vectorOffset = offset;
                renderer.multiLine = false;
                renderer.textAlign = UIHorizontalAlignment.Left;
                renderer.processMarkup = processMarkup;
                renderer.colorizeSprites = colorizeSprites;
                renderer.defaultColor = defaultColor;
                renderer.bottomColor = useGradient ? new Color32?(bottomColor) : null;
                renderer.overrideMarkupColors = false;
                renderer.opacity = CalculateOpacity();
                renderer.outline = useOutline;
                renderer.outlineSize = outlineSize;
                renderer.outlineColor = outlineColor;
                renderer.shadow = useDropShadow;
                renderer.shadowColor = dropShadowColor;
                renderer.shadowOffset = dropShadowOffset;
                charWidths = renderer.GetCharacterWidths(text);
            }

            if (Multiline)
            {
                var words = GetWords();
                lines = CalculateLineBreaks(words);

                cursorIndex = Mathf.Min(cursorIndex, text.Length);
                scrollIndex = 0;
                var lineCount = Mathf.Max(1, Mathf.CeilToInt((size.y - Padding.vertical) / lineHeight) - 1);
                var lineByIndex = GetLineByIndex(cursorIndex, cursor: true);
                lineScrollIndex = Mathf.Min(lineScrollIndex, lineByIndex);
                lineScrollIndex = Mathf.Max(lineScrollIndex, lineByIndex - (lineCount - 1));
                lineIndex = Mathf.Min(lineScrollIndex + lineCount - 1, lines.Count - 1);
            }
            else
            {
                lines = new List<int> { 0 };

                cursorIndex = Mathf.Min(cursorIndex, text.Length);
                scrollIndex = Mathf.Min(Mathf.Min(scrollIndex, cursorIndex), text.Length);
                lineScrollIndex = 0;
                leftOffset = 0f;
                if (horizontalAlignment == UIHorizontalAlignment.Left)
                {
                    var lineOffset = 0f;
                    for (int i = scrollIndex; i < cursorIndex; i += 1)
                        lineOffset += charWidths[i];

                    while (lineOffset >= maxUnitSize.x && scrollIndex < cursorIndex)
                        lineOffset -= charWidths[scrollIndex++];
                }
                else
                {
                    scrollIndex = Mathf.Max(0, Mathf.Min(cursorIndex, text.Length - 1));
                    var width = 0f;
                    float num6 = font.size * 1.25f * ratio;
                    while (scrollIndex > 0 && width < maxUnitSize.x - num6)
                    {
                        width += charWidths[scrollIndex--];
                    }

                    width = (text.Length > 0) ? TextWidth(scrollIndex, m_Text.Length) : 0f;
                    switch (horizontalAlignment)
                    {
                        case UIHorizontalAlignment.Center:
                            leftOffset = Mathf.Max(0f, (maxUnitSize.x - width) * 0.5f);
                            break;
                        case UIHorizontalAlignment.Right:
                            leftOffset = Mathf.Max(0f, maxUnitSize.x - width);
                            break;
                    }
                }
            }
        }

        private void RenderText()
        {
            var ratio = PixelsToUnits();
            var maxSize = new Vector2(size.x - Padding.horizontal, size.y - Padding.vertical);
            var offset = pivot.TransformToUpperLeft(size, arbitraryPivotOffset);
            offset = new Vector3(offset.x + Padding.left, offset.y - Padding.top, 0f) * ratio;

            for (var i = lineScrollIndex; i <= lineIndex; i++)
            {
                using UIFontRenderer renderer = font.ObtainRenderer();
                renderer.wordWrap = false;
                renderer.maxSize = maxSize;
                renderer.pixelRatio = ratio;
                renderer.textScale = textScale * GetTextScaleMultiplier();
                renderer.characterSpacing = characterSpacing;
                renderer.vectorOffset = offset;
                renderer.multiLine = false;
                renderer.textAlign = UIHorizontalAlignment.Left;
                renderer.processMarkup = processMarkup;
                renderer.colorizeSprites = colorizeSprites;
                renderer.defaultColor = isEnabled ? textColor : disabledTextColor;
                renderer.bottomColor = useGradient ? new Color32?(bottomColor) : null;
                renderer.overrideMarkupColors = false;
                renderer.opacity = CalculateOpacity();
                renderer.outline = useOutline;
                renderer.outlineSize = outlineSize;
                renderer.outlineColor = outlineColor;
                renderer.shadow = useDropShadow;
                renderer.shadowColor = dropShadowColor;
                renderer.shadowOffset = dropShadowOffset;

                if (Multiline)
                {
                    var lineText = text.Substring(lines[i], LineLenght(i));
                    renderer.vectorOffset = offset + new Vector3(CalculateLineLeftOffset(i), (lineScrollIndex - i) * font.lineHeight * textScale * ratio);
                    renderer.Render(lineText, TextRenderData);
                }
                else
                {
                    offset.x += leftOffset;
                    renderer.vectorOffset = offset;
                    renderer.Render(text.Substring(scrollIndex), TextRenderData);
                }
            }
        }

        private void RenderSelection()
        {
            if (selectionEnd != selectionStart && !string.IsNullOrEmpty(SelSprite) && SelAtlas is UITextureAtlas cursorAtlas && cursorAtlas[SelSprite] is UITextureAtlas.SpriteInfo sprite)
            {
                SelRenderData.material = cursorAtlas.material;

                var lineHeight = font.lineHeight * textScale;
                var ratio = PixelsToUnits();
                var maxSize = (pivot.TransformToUpperLeft(size, arbitraryPivotOffset) + new Vector3(Padding.left, -Padding.top)) * ratio;
                var maxUnitSize = new Vector3(size.x - Padding.horizontal, size.y - Padding.vertical) * ratio;
                var startLine = Mathf.Max(GetLineByIndex(selectionStart), lineScrollIndex);
                int b = Mathf.Min(Mathf.FloorToInt((size.y - Padding.vertical) / lineHeight) + lineScrollIndex, lines.Count - 1);
                var endLine = Mathf.Min(GetLineByIndex(selectionEnd), b);
                for (int i = startLine; i <= endLine; i += 1)
                {
                    var offset = CalculateLineLeftOffset(i);
                    var begin = lines[i] + scrollIndex;
                    var left = (selectionStart >= lines[i]) ? (maxSize.x + offset + TextWidth(begin, selectionStart)) : (maxSize.x + offset);
                    var top = maxSize.y - (i - lineScrollIndex) * lineHeight * ratio;
                    var bottom = Mathf.Max(maxSize.y - maxUnitSize.y, top - font.size * textScale * ratio);
                    var right = (selectionEnd <= lines[i] + LineLenght(i)) ? Mathf.Min(maxSize.x + offset + TextWidth(begin, SelectionEnd), maxSize.x + maxUnitSize.x) : (maxSize.x + offset + TextWidth(begin, lines[i] + LineLenght(i)));


                    AddTriangles(SelRenderData.triangles, SelRenderData.vertices.Count);

                    SelRenderData.vertices.Add(new Vector3(left, top));
                    SelRenderData.vertices.Add(new Vector3(right, top));
                    SelRenderData.vertices.Add(new Vector3(right, bottom));
                    SelRenderData.vertices.Add(new Vector3(left, bottom));

                    var color = SelBgColor;
                    SelRenderData.colors.Add(color);
                    SelRenderData.colors.Add(color);
                    SelRenderData.colors.Add(color);
                    SelRenderData.colors.Add(color);

                    var region = sprite.region;
                    var xRatio = region.width / sprite.pixelSize.x;
                    var yRatio = region.height / sprite.pixelSize.y;
                    SelRenderData.uvs.Add(new Vector2(region.x + xRatio, region.yMax - yRatio));
                    SelRenderData.uvs.Add(new Vector2(region.xMax - xRatio, region.yMax - yRatio));
                    SelRenderData.uvs.Add(new Vector2(region.xMax - xRatio, region.y + yRatio));
                    SelRenderData.uvs.Add(new Vector2(region.x + xRatio, region.y + yRatio));
                }
            }
        }

        private void RenderCursor()
        {
            if (cursorShown && selectionEnd == selectionStart && !string.IsNullOrEmpty(SelSprite) && SelAtlas is UITextureAtlas cursorAtlas && cursorAtlas[SelSprite] is UITextureAtlas.SpriteInfo sprite)
            {
                SelRenderData.material = cursorAtlas.material;

                var ratio = PixelsToUnits();
                var maxSize = m_Pivot.TransformToUpperLeft(size, arbitraryPivotOffset) * ratio;
                var lineByIndex = GetLineByIndex(cursorIndex, cursor: true);
                var topOffset = -Padding.top * ratio - (lineByIndex - lineScrollIndex) * font.lineHeight * textScale * ratio;
                var leftOffset = (CalculateLineLeftOffset(lineByIndex) + TextWidth(lines[lineByIndex] + scrollIndex, cursorIndex) + Padding.left * ratio).Quantize(ratio);
                var width = ratio * GetUIView().ratio * CursorWidth;
                var height = Mathf.Min(font.size * textScale * ratio, (size.y - Padding.vertical) * ratio);

                var topLeft = new Vector3(leftOffset, topOffset) + maxSize;
                var topRight = new Vector3(leftOffset + width, topOffset) + maxSize;
                var bottomLeft = new Vector3(leftOffset, topOffset - height) + maxSize;
                var bottomRight = new Vector3(leftOffset + width, topOffset - height) + maxSize;

                AddTriangles(SelRenderData.triangles, SelRenderData.vertices.Count);
                SelRenderData.vertices.Add(topLeft);
                SelRenderData.vertices.Add(topRight);
                SelRenderData.vertices.Add(bottomRight);
                SelRenderData.vertices.Add(bottomLeft);

                var color = textColor;
                SelRenderData.colors.Add(color);
                SelRenderData.colors.Add(color);
                SelRenderData.colors.Add(color);
                SelRenderData.colors.Add(color);

                Rect region = sprite.region;
                SelRenderData.uvs.Add(new Vector2(region.x, region.yMax));
                SelRenderData.uvs.Add(new Vector2(region.xMax, region.yMax));
                SelRenderData.uvs.Add(new Vector2(region.xMax, region.y));
                SelRenderData.uvs.Add(new Vector2(region.x, region.y));
            }
        }

        static readonly int[] kTriangleIndices = new int[6] { 0, 1, 3, 3, 1, 2 };
        private void AddTriangles(PoolList<int> triangles, int baseIndex)
        {
            for (int i = 0; i < kTriangleIndices.Length; i++)
            {
                triangles.Add(kTriangleIndices[i] + baseIndex);
            }
        }

        private List<int> CalculateLineBreaks(List<int> words)
        {
            var breaks = new List<int> { 0 };
            int line = 0;
            var width = (size.x - Padding.horizontal) * PixelsToUnits();
            var count = words.Count;
            for (int i = 0; i < count && words[i] != m_Text.Length; i += 1)
            {
                int num3 = (i == count - 1) ? m_Text.Length : words[i + 1];
                if (m_Text[words[i]] == '\n')
                {
                    breaks.Add(words[i] + 1);
                    line += 1;
                }
                else
                {
                    if (!(TextWidth(breaks[line], num3) >= width))
                        continue;

                    if (words[i] != breaks[line])
                    {
                        breaks.Add(words[i]);
                        line += 1;
                    }

                    for (int j = breaks[line]; j < num3; j += 1)
                    {
                        if (TextWidth(breaks[line], j + 1) >= width)
                        {
                            breaks.Add(j);
                            line += 1;
                        }
                    }
                }
            }

            return breaks;
        }

        private List<int> GetWords()
        {
            var words = new List<int> { 0 };
            int num = 0;

            while (true)
            {
                int num2 = FindNextWord(num);
                for (int num3 = FindNextLineBreak(num); num3 < num2; num3 = FindNextLineBreak(num3 + 1))
                {
                    if (num3 != 0)
                        words.Add(num3);
                }

                if (num2 == m_Text.Length)
                    break;

                words.Add(num2);
                num = num2;
            }

            return words;
        }

        private int FindNextLineBreak(int start)
        {
            while (start < m_Text.Length && m_Text[start] != '\n')
                start += 1;

            return start;
        }

        private int LineLenght(int line)
        {
            int count = lines.Count;

            if (line < 0 || line >= count)
                return 0;
            else if (line == count - 1)
                return m_Text.Length - lines[line];
            else
                return lines[line + 1] - lines[line];
        }

        private float TextWidth(int begin, int end)
        {
            if (begin < 0 || end > m_Text.Length || end <= begin)
                return 0f;

            var width = 0f;
            for (int i = begin; i < end && i != m_Text.Length; i++)
            {
                width += charWidths[i];
            }

            return width;
        }

        private int GetLineByIndex(int index, bool cursor = false)
        {
            int count = lines.Count;
            if (index == m_Text.Length)
                return count - 1;

            var i = 0;
            while (i < count && index >= lines[i] + LineLenght(i))
                i += 1;

            if (cursor && cursorAtEndOfLine && index == lines[i] && LineLenght(i) > 0)
                return Mathf.Max(0, i - 1);
            else
            {
                cursorAtEndOfLine = false;
                return i;
            }
        }

        private float CalculateLineLeftOffset(int line)
        {
            if (!Multiline)
                return leftOffset;

            float num = TextWidth(lines[line], lines[line] + LineLenght(line));
            float num2 = (size.x - Padding.horizontal) * PixelsToUnits();

            return horizontalAlignment switch
            {
                UIHorizontalAlignment.Left => 0f,
                UIHorizontalAlignment.Center => Mathf.Max(0f, (num2 - num) * 0.5f),
                UIHorizontalAlignment.Right => Mathf.Max(0f, num2 - num),
                _ => 0f,
            };
        }

        #endregion

        public enum OnUnfocus
        {
            None = 0,
            Unfocus = 1,
            FocusRoot = 2,
        }
    }
}
