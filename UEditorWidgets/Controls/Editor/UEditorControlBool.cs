namespace uAssist.UEditorWidgets
{
    using System;
    using UnityEditor;
    using UnityEngine;

    [UWidgetWidgetAttribute(eUWidgetDesignerCategory.Controls, "Prop - Bool")]
    public class UEditorControlBool : UEditorWidgetBase
    {
        [UWidgetPropertyAttribute]
        public UEditorWidgetLabel PropertyLabel = UWidget.Create<UEditorWidgetLabel>();
        
        [UWidgetPropertyAttribute]
        public UEditorWidgetToggle PropertyBool = UWidget.Create<UEditorWidgetToggle>();

        public override bool BindTo(object Object, string MemberName)
        {
            PropertyLabel.Label = MemberName;
            if (!base.BindTo(Object, MemberName))
            {
                return false;
            }
  
            if (this.GetBoundValueType() != typeof(bool))
            {
                throw new Exception("Bound property is not of type bool in UEditorControlBoo.BindTo()");
            }


            this.PropertyBool.BindTo(Object, MemberName);
            
            return true;
        }

        public bool Value
        {
            get
            {
                return this.GetBoundValue<bool>();
                
            }
            set
            {
                this.SetBoundValue(value);
            }
        }

        [UWidgetPropertyAttribute]
        public override float Width
        {
            get
            {
                return this.PropertyBool.Width + this.PropertyLabel.Width;
            }
            set
            {
                //This is swallowed by the control. The actual width is derived from the width of the child controls
                base.Width = value;
                //TODO: Should a warning be thrown here??
            }
        }

        //Constructor
        public UEditorControlBool() : base(eWidgetType.Generic) 
        {
            this.Name = "BoolControl";

            this.PropertyLabel.LayoutMode = ePositioningLayout.Layout;
            this.PropertyLabel.Height = EditorGUIUtility.singleLineHeight;
            //this.PropertyLabel.Width = 60;

            this.PropertyBool.LayoutMode = ePositioningLayout.Layout;
            this.PropertyBool.Height = EditorGUIUtility.singleLineHeight;
            //this.PropertyBool.Width = 100;

            //this.Width = 160; //TODO: Maybe switch this to base.Width?
            this.Height = EditorGUIUtility.singleLineHeight;

            this.PropertyLabel.Name = "PropertyLabel";
            this.PropertyBool.Name = "PropertyBool";
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
                    this.PropertyBool.Render();

                }
                EditorGUILayout.EndHorizontal();
            }
            if (this.LayoutMode != ePositioningLayout.Layout)
            {
                GUILayout.EndArea();
            }

            /*
            if (PropertyBool.GetBoundValue<bool>() == true)
            {
                this.Value = true;
            }
            else
            {
                this.Value = false;
            }
            */
        }



    }

}
