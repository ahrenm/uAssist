namespace uAssist.UEditorWidgets
{
    using System;
    using UnityEditor;
    using UnityEngine;

    [UWidgetWidgetAttribute(eUWidgetDesignerCategory.Controls, "Prop - Generic")]
    public class UEditorControlProperty : UEditorWidgetBase
    {
        [UWidgetProperty]
        public UEditorWidgetLabel PropertyLabel = UWidget.Create<UEditorWidgetLabel>();

        [UWidgetProperty]
        public UEditorWidgetTextField PropertyInputField = UWidget.Create<UEditorWidgetTextField>();

        public override bool BindTo(object Object, string MemberName)
        {
            if (PropertyLabel.Label == "")
            {
                PropertyLabel.Label = MemberName;
            }

            PropertyInputField.BindTo(Object, MemberName);
            

            return true;
        }

        [UWidgetProperty]
        public override float Width
        {
            get
            {
                return this.PropertyInputField.Width + this.PropertyLabel.Width;
            }
            set
            {
                //This is swallowed by the control. The actual width is derived from the width of the child controls
                base.Width = value;
                //TODO: Should a warning be thrown here??
            }
        }

    

        //Constructor
        public UEditorControlProperty() : base(eWidgetType.Generic) 
        {
            this.Name = "PropertyControl";

            this.PropertyLabel.Name = "PropertyLabel";
            this.PropertyLabel.LayoutMode = ePositioningLayout.Layout;
            this.PropertyLabel.Height = EditorGUIUtility.singleLineHeight;
            this.PropertyLabel.Width = 60;

            this.PropertyInputField.Name = "PropertyInputField";
            this.PropertyInputField.LayoutMode = ePositioningLayout.Layout;
            this.PropertyInputField.Height = EditorGUIUtility.singleLineHeight;
            this.PropertyInputField.Width = 100;

            this.Width = 160; //TODO: Maybe switch this to base.Width?
            this.Height = EditorGUIUtility.singleLineHeight;
            
        }

        protected override void WidgetRender()
        {

            this.Style.fixedWidth = this.Width;

            if (this.LayoutMode != ePositioningLayout.Layout)
            {
                GUILayout.BeginArea(new Rect(this.RenderOffsetX, this.RenderOffsetY, this.Width, this.Height), this.Style);
            }
                {

                    EditorGUILayout.BeginHorizontal(this.Style, GUILayout.Width(this.Style.fixedWidth));
                    {
                        this.PropertyLabel.Render();
                        this.PropertyInputField.Render();

                    }
                    EditorGUILayout.EndHorizontal();
                }
            if (this.LayoutMode != ePositioningLayout.Layout)
            {
                GUILayout.EndArea();
            }
        }

    }
}
