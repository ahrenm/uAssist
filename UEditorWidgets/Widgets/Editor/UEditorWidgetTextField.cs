namespace uAssist.UEditorWidgets
{
    using System;
    using System.Reflection;
    using System.Collections.Generic;
    using UnityEditor;
    using UnityEngine;


    /// <summary>
    /// A basic text field
    /// </summary>
    [UWidgetWidgetAttribute(eUWidgetDesignerCategory.Widgets, "Text Field")]
    public class UEditorWidgetTextField : UEditorWidgetTextBase
    {
        [SerializeField]
        private string _text = "";

        [UWidgetPropertyAttribute]
        public string Text
        {
            get
            {
                if (this.BindingType == eBindingType.NotSet)
                {
                    return this._text;
                }
                else
                {
                    return this.GetBoundValue<string>();
                }
            }
            set
            {
                if (this.BindingType == eBindingType.NotSet)
                {
                    this._text = value;
                }
                else
                {
                    this.SetBoundValue(value);
                }
            }
        }

        //Public constructor
        public UEditorWidgetTextField() : base(eWidgetType.TextField)
        {
            this.Name = "TextFieldControl";
            this.Border.FromInt(3, 3, 3, 3);
            this.Height = EditorGUIUtility.singleLineHeight;
        }

        protected override void WidgetRender()
        {
            base.WidgetRender();

            //Wrap the draw call in a check so derived types don't get called.
            if (this.WidgetType == eWidgetType.TextField)
            {
                if (this.LayoutMode == ePositioningLayout.Layout)
                {
                    this.Text = EditorGUILayout.TextField(this.Text,
                        this.Style,
                        GUILayout.Height(this.Height),
                        GUILayout.Width(this.Width),
                        GUILayout.ExpandWidth(this.LayoutExpandWidth),
                        GUILayout.ExpandHeight(this.LayoutExpandHeight));

                    if (Event.current.type == EventType.Repaint)
                    {
                        this.LastLayoutRect = GUILayoutUtility.GetLastRect();
                    }
                }
                else
                {
                    this.Text = EditorGUI.TextField(this.RenderRect, this.Text, this.Style);
                }
            }
        }
    }



}