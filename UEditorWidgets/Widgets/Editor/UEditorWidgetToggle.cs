namespace uAssist.UEditorWidgets
{
    using System;
    using UnityEngine;
    using UnityEditor;
    using System.Collections;

    [UWidgetWidgetAttribute(eUWidgetDesignerCategory.Widgets, "Toggle button")]
    public class UEditorWidgetToggle : UEditorWidgetBase
    {

        public UEditorWidgetToggle() : base(eWidgetType.Generic)
        {
            this.Name = "WidgetToggle";
            this.Width = 16;
            this.Height = 16;
        }

        public override bool BindTo(object Object, string MemberName)
        {
            if (base.BindTo(Object, MemberName) == false)
            {
                return false;
            }

            if (this.GetBoundValueType() != typeof(bool))
            {
                return false;
            }

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

        protected override void WidgetRender()
        {
            this.SetBoundValue(EditorGUILayout.Toggle(this.GetBoundValue<bool>(), GUILayout.Height(this.Height), GUILayout.Width(this.Width) ));
        }


    }
}