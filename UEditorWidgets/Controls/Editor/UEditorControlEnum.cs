namespace uAssist.UEditorWidgets
{
    using System;
    using UnityEditor;
    using UnityEngine;

    [UWidgetWidgetAttribute(eUWidgetDesignerCategory.Controls, "Prop - Popup")]
    public class UEditorControlEnum : UEditorWidgetBase
    {

        [UWidgetProperty]
        public UEditorWidgetLabel PropertyLabel = UWidget.Create<UEditorWidgetLabel>();
        
        [UWidgetProperty]
        public UEditorWidgetPopUp PropertyEnumPopup = UWidget.Create<UEditorWidgetPopUp>();

        //Constructor
        public UEditorControlEnum(): base(eWidgetType.Generic) 
        {
            this.Name = "BoolControl";
            this.PropertyLabel.Name = "PropertyLabel";
            this.PropertyEnumPopup.Name = "PropertyEnumPopup";

            this.PropertyLabel.LayoutMode = ePositioningLayout.Layout;
            this.PropertyLabel.Height = EditorGUIUtility.singleLineHeight;
            this.PropertyLabel.Width = 60;

            this.PropertyEnumPopup.LayoutMode = ePositioningLayout.Layout;
            this.PropertyEnumPopup.Height = EditorGUIUtility.singleLineHeight;
            this.PropertyEnumPopup.Width = 100;
            this.PropertyEnumPopup.BaseStyle = "button";

            this.Width = 160; //TODO: Maybe switch this to base.Width?
            this.Height = EditorGUIUtility.singleLineHeight;
            
        }


        public override bool BindTo(object Object, string MemberName)
        {
            

            PropertyLabel.Label = MemberName;
            if (!base.BindTo(Object, MemberName))
            {
                return false;
            }

            return this.PropertyEnumPopup.BindTo(Object, MemberName);
            
        }

        [UWidgetPropertyAttribute]
        public override float Width
        {
            get
            {
                return this.PropertyEnumPopup.Width + this.PropertyLabel.Width;
            }
            set
            {
                //This is swallowed by the control. The actual width is derived from the width of the child controls
                base.Width = value;
                //TODO: Should a warning be thrown here??
            }
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
                    this.PropertyEnumPopup.Render();

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