namespace uAssist.UEditorWidgets
{
    using System;
    using System.Reflection;
    using System.Collections.Generic;
    using UnityEditor;
    using UnityEngine;
    
    [UWidgetWidgetAttribute(eUWidgetDesignerCategory.Widgets, "Label")]
    public class UEditorWidgetLabel : UEditorWidgetTextBase
    {
        [SerializeField]
        private string _label = "";

        [UWidgetPropertyAttribute]
        public string Label
        {
            get
            {
                if (this.BindingType == eBindingType.NotSet)
                {
                    return _label;
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
                    this._label = value;
                }
                else
                {
                    this.SetBoundValue(value);
                }
            }
        }


        //Basic constructor
        public UEditorWidgetLabel()  : this(eWidgetType.Label)
        {
            this.Name = "LabelControl";
            this.Border.FromInt(0, 0, 0, 0);
            this.Height = EditorGUIUtility.singleLineHeight;
            this.Width = 100;
        }


        //Used by widgets that derive from this to pass their type down to the base class
        public UEditorWidgetLabel(eWidgetType type) : base(type) { }


        protected override void WidgetRender()
        {
            base.WidgetRender();

            //Wrap the draw call in a check so derived types don't get called.
            if (this.WidgetType == eWidgetType.Label)
            {
                if (this.LayoutMode == ePositioningLayout.Layout)
                {
                    EditorGUILayout.LabelField(this.Label,
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
                    EditorGUI.LabelField(this.RenderRect, this.Label, this.Style);
                }
            }
        }
    }


}