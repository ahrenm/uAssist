
namespace uAssist.UEditorWidgets
{
    using System;
    using UnityEngine;
    using UnityEditor;
    using System.Collections;
    using System.Collections.Generic;

    [UWidgetWidgetAttribute(eUWidgetDesignerCategory.Controls, "Editor - Rect")]
    public partial class UEditorControlRect : UEditorWidgetBase
    {

#region Local vars and helpers

        public string Label
        {
            get
            {
                return _controlFoldout.Label;
            }
            set
            {
                _controlFoldout.Label = value;
            }
        }

#endregion


#region UEditor plumbing

        //Public form constructor
        public UEditorControlRect() : base(eWidgetType.Generic)
        {
            //Initalize all widget components
            this.InitalizeComponents();

            this.Name = "RectControl";

        }




#endregion


#region Functional code

        public override bool BindTo(object Object, string MemberName)
        {
            if (base.BindTo(Object, MemberName) != true)
            {
                return false;
            }

            Type __cachedType = this.GetBoundValueType();
            object __cachedBoundObject = this.GetBoundValue<object>();

            if (__cachedType == typeof(UnityEngine.Rect) || __cachedType == typeof(UnityEngine.RectOffset) || __cachedType == typeof(RectOffsetSeralizable))
            {
                _inputTop.BindTo(__cachedBoundObject, "top");
                _inputBottom.BindTo(__cachedBoundObject, "bottom");
                _inputLeft.BindTo(__cachedBoundObject, "left");
                _inputRight.BindTo(__cachedBoundObject, "right");
            }
            else
            {
                return false;
            }

            this.Label = "(Rect) " + MemberName;

            return true;
        }

        protected override void WidgetRender()
        {
            if (this.LayoutMode != ePositioningLayout.Layout)
            {
                GUILayout.BeginArea(new Rect(this.RenderOffsetX, this.RenderOffsetY, this.Width, this.Height), this.Style);
            }
            {
                _controlFoldout.Render();
            }
            if (this.LayoutMode != ePositioningLayout.Layout)
            {
                GUILayout.EndArea();
            }
        }


#endregion

    }
}