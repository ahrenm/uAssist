namespace uAssist.UEditorWidgets
{
    using System;
    using UnityEngine;
    using System.Collections.Generic;
    using System.Reflection;
    using UnityEditor;

    /// <summary>
    /// The base class for text based widgets
    /// Provides basic text capability for widgets.
    /// </summary>
    public class UEditorWidgetTextBase : UEditorWidgetBase
    {

        private int _cachedFontSize = 11;

        [UWidgetPropertyAttribute("Font Size")]
        public int FontSize
        {
            get { return this._cachedFontSize; }
            set
            {
                if (_cachedFontSize != value)
                {
                    this._StyleIsDirty = true;
                    this._cachedFontSize = value;
                }
            }
        }

        private FontStyle _cachedFontStyle = FontStyle.Normal;

        [UWidgetPropertyAttribute("Font Style")]
        public FontStyle FontStyle
        {
            get { return this._cachedFontStyle; }
            set
            {
                if (_cachedFontStyle != value)
                {
                    this._StyleIsDirty = true;
                    this._cachedFontStyle = value;
                }

            }
        }

        //TODO: Remove nullable type, not really working/adding value
        private bool? _cachedWordWrap;

        [UWidgetPropertyAttribute("Word Wrap")]
        public bool WordWrap
        {
            get
            {
                if (_cachedWordWrap.HasValue)
                {
                    return this._cachedWordWrap.Value;
                }
                else
                {
                    return false;
                }
            }
            set
            {
                if (_cachedWordWrap == null || _cachedWordWrap.Value != value)
                {
                    this._StyleIsDirty = true;
                    this._cachedWordWrap = value;
                }
            }
        }

        private TextAnchor _cachedAlighment = TextAnchor.UpperLeft;

        [UWidgetPropertyAttribute]
        public TextAnchor Alignment
        {
            get { return _cachedAlighment; }
            set
            {
                if (_cachedAlighment != value)
                {
                    this._StyleIsDirty = true;
                    this._cachedAlighment = value;
                }
            }
        }

        //Constructor
        public UEditorWidgetTextBase(eWidgetType type)
            : base(type)
        {
            this.Margin.FromInt(4, 4, 2, 2);
            this.Border.FromInt(3, 3, 3, 3);
            this.Padding.FromInt(3, 3, 1, 2);
        }

        protected override void ReBuildGUIStyle()
        {
            base.ReBuildGUIStyle();

            if (this._cachedFontSize != -1)
            {
                this.Style.fontSize = this._cachedFontSize;
            }

            this.Style.fontStyle = this._cachedFontStyle;

            if (this._cachedWordWrap != null)
            {
                this.Style.wordWrap = this._cachedWordWrap.Value;
            }

            this.Style.alignment = this._cachedAlighment;
        }
    }
}
