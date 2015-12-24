namespace uAssist.UEditorWidgets
{
    using System;
    using System.Reflection;
    using System.Collections.Generic;
    using UnityEditor;
    using UnityEngine;

    [UWidgetWidgetAttribute(eUWidgetDesignerCategory.Decorators,"Seperator Line")]
    public sealed class UEditorDecoratorSeperator : UEditorWidgetBase
    {
        [UWidgetPropertyAttribute]
        public int SeperatorLines = 1;

        public UEditorDecoratorSeperator() :base(eWidgetType.Generic)
        {
            this.Name = "LayoutSpacer";
        }

        protected override void WidgetRender()
        {
            for (int i = 0; i < SeperatorLines; i++)
            {
                EditorGUILayout.Separator();
            }
        }
    }

    [UWidgetWidgetAttribute(eUWidgetDesignerCategory.Decorators, "Horizontal Line")]
    public sealed class UEditorDecoratorHorizontalLine : UEditorWidgetBase
    {

        public UEditorDecoratorHorizontalLine() :base(eWidgetType.Generic)
        {
            this.Height = 1;
            this.LayoutMode = ePositioningLayout.Layout;
            this.Name = "HorizontalLine";
        }

        protected override void WidgetRender()
        {
            if (this.LayoutMode != ePositioningLayout.Layout)
            {
                //GUILayout.BeginArea(new Rect(this.RenderOffsetX, this.RenderOffsetY, this.Width, this.Height), this.Style);
                //GUI.BeginGroup(new Rect(this.RenderOffsetX, this.RenderOffsetY, this.Width, this.Height), this.Style);
                GUI.Box(this.RenderRect, "");
                //GUILayout.EndArea();
                //GUI.EndGroup();
            }
            else
            {
                GUILayout.Box("", new GUILayoutOption[] { GUILayout.ExpandWidth(true), GUILayout.Height(1) });
            }
        }

    }

}