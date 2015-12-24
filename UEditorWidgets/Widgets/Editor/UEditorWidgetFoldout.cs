namespace uAssist.UEditorWidgets
{
    using System;
    using System.Collections.Generic;
    using UnityEditor;
    using UnityEngine;

    [UWidgetWidgetAttribute(eUWidgetDesignerCategory.Decorators, "Fold out")]
    public class UEditorWidgetFoldout : UEditorPanelBase, IUEditorWidgetClickable
    {

        public event UEditorWidget_OnClick OnClick;
        
        protected bool _foldOutOpen = false;

        [UWidgetPropertyAttribute]
        public bool FoldoutOpen
        {
            get { return _foldOutOpen; }
            set { _foldOutOpen = value; }
        }

        [UWidgetPropertyAttribute]
        public int IndentLevel = 0;
        
        [UWidgetPropertyAttribute]
        public int IndentDepth = 10;

        [UWidgetPropertyAttribute]
        public float DefaultWidth = 200f;

        [UWidgetPropertyAttribute]
        public bool ToggleOnLabelClick = true;

        [UWidgetPropertyAttribute]
        public bool Override_AlwaysOpen = false;

        //Constructor
        public UEditorWidgetFoldout() : base(eWidgetType.Foldout) 
        {
            this.Name = "WidgetFoldout";
            this.Padding.left = 15;
            this.Border.FromInt(14,0,13,0);
        }

        [SerializeField]
        private string _label = "";

#region Label formatting

        private int _cachedFontSize = 11;

        [UWidgetPropertyAttribute]
        public int FontSize
        {
            get { return this._cachedFontSize; }
            set
            {
                if (_cachedFontSize != value)
                {
                    this.StyleIsDirty = true;
                    this._cachedFontSize = value;
                }
            }
        }

        private FontStyle _cachedFontStyle = FontStyle.Normal;

        [UWidgetPropertyAttribute]
        public FontStyle FontStyle
        {
            get { return this._cachedFontStyle; }
            set
            {
                if (_cachedFontStyle != value)
                {
                    this.StyleIsDirty = true;
                    this._cachedFontStyle = value;
                }

            }
        }

        protected override void ReBuildGUIStyle()
        {
            base.ReBuildGUIStyle();

            if (this._cachedFontSize != -1)
            {
                this.Style.fontSize = this._cachedFontSize;
            }

            this.Style.fontStyle = this._cachedFontStyle;
        }

#endregion


        [UWidgetPropertyAttribute]
        public string Label
        {
            get
            {
                if (this.BindingType == eBindingType.NotSet)
                {
                    return this._label;
                }
                else
                {
                    return this.GetBoundValue<string>();
                }
                /*
                switch (this.BindingType)
                {
                    case eBindingType.NotSet:
                        return _label;
                    case eBindingType.Property:
                        return (string)_boundPropertyInfo.GetValue(_boundMemberObject, null);
                    case eBindingType.Field:
                        return (string)_boundFieldInfo.GetValue(_boundMemberObject);
                    case eBindingType.Method:
                        return null;
                    default:
                        return null;
                }
                */
            }
            set
            {
                if (this.BindingType == eBindingType.NotSet)
                {
                    this._label = value;
                }
                else
                {
                    this.SetBoundValue(value);
                }
                /*
                switch (this.BindingType)
                {
                    case eBindingType.NotSet:
                        _label = value;
                        break;
                    case eBindingType.Property:
                        _boundPropertyInfo.SetValue(_boundMemberObject, value, null);
                        break;
                    case eBindingType.Field:
                        _boundFieldInfo.SetValue(_boundMemberObject, value);
                        break;
                    case eBindingType.Method:
                        break;
                    default:
                        break;
                }
                */ 
            }
        }

        protected override void WidgetRender()
        {
            //if (this.Label == "")
            //    return;

            if (this.Width == -1)
            {
                this.Width = DefaultWidth;
            }

            if (_StyleIsDirty)
            {
                if (this._cachedFontSize != -1)
                    this.Style.fontSize = this._cachedFontSize;

                this.Style.fontStyle = this._cachedFontStyle;

            }
        
            Rect __foldoutRect;

            if (this.LayoutMode == ePositioningLayout.Layout)
            { 
                __foldoutRect = EditorGUILayout.GetControlRect(true, EditorGUIUtility.singleLineHeight, this.Style, GUILayout.MaxWidth(this.Width));
                if (Event.current.type == EventType.Repaint)
                {
                    this.LastLayoutRect = GUILayoutUtility.GetLastRect();
                }
            }
            else
            {
                __foldoutRect = new Rect(this.RenderOffsetX, this.RenderOffsetY, this.Width, this.Height);
            }

            __foldoutRect.x += (this.IndentLevel * this.IndentDepth);

            if (Override_AlwaysOpen)
            {
                bool __userInput = EditorGUI.Foldout(__foldoutRect, true, this.Label, ToggleOnLabelClick, this.Style);
                FoldoutOpen = true;
                //Did the user click on the control which would normally set the foldout to closed
                if (__userInput == false)
                {
                    Invoke_OnClick();
                }
            }
            else
            {
                bool __newStatus = EditorGUI.Foldout(__foldoutRect, this.FoldoutOpen, this.Label, ToggleOnLabelClick, this.Style);


                if (__newStatus != this.FoldoutOpen)
                {
                    this.FoldoutOpen = __newStatus;
                    Invoke_OnClick();
                }
                else
                    this.FoldoutOpen = __newStatus;
            }

            

            if (FoldoutOpen)
                this.RenderChildren();

        }

        protected void Invoke_OnClick()
        {
            if (OnClick != null)
            {
                //OnClick(new EventArgs(), this, _boundMemberObject, _boundMemberName);
                OnClick(this, new EventArgs());
            }
        }

        
    }


}