namespace uAssist.UEditorWidgets
{
    using System;
    using System.Collections.Generic;
    using UnityEditor;
    using UnityEngine;

    public class UEditorPanelBase : UEditorWidgetBase , IWidgetContainer
    {

        [UWidgetProperty("Render Children")]
        public bool ChildrenShouldRender = true;


        public List<UEditorWidgetBase> _children = new List<UEditorWidgetBase>();

        public void AddChild(UEditorWidgetBase addChild, bool bSilent = false)
        {
            //newChild.parent = (IWidgetContainer)this;
            addChild.parent = this;
            this._children.Add(addChild);
            if (bSilent == false)
            {
                this.Raise_onContainerChange();
            }
        }

        public void ClearChilden()
        {
            this._children.Clear();
        }

        //Constructor
        public UEditorPanelBase(eWidgetType type) : base(type) {}


        public virtual void RenderChildren()
        {
            if (this.ChildrenShouldRender)
            {
                foreach (var item in _children)
                {
                    item.Render();
                }
            }
        }

        /*
        protected override void WidgetRender()
        {
          

        }
   */

        public void RemoveChild(UEditorWidgetBase removeChild, bool bSilent = false)
        {
            this._children.Remove(removeChild);
            if (bSilent == false)
            {
                this.Raise_onContainerChange();
            }
        }

        public List<UEditorWidgetBase> Children
        {
            get
            {
                return _children;
            }
            set
            {
                _children = value;
            }
        }

        public void Raise_onContainerChange()
        {
            if (this.onContainerChange != null)
            {
                this.onContainerChange(this);
            }
        }

        public event ContainerChanged onContainerChange;
    }

    [UWidgetWidgetAttribute(eUWidgetDesignerCategory.Panels, "Horizontal Layout")]
    public class UEditorPanelHorizonal : UEditorPanelBase
    {

        public UEditorPanelHorizonal() : base(eWidgetType.PanelHorizontal) 
        {
            //this.LayoutMode = ePositioningLayout.Layout;
            this.Name = "HorizontalLayout";
        }

        protected override void WidgetRender()
        {
            if (this.LayoutMode != ePositioningLayout.Layout)
            {
                GUILayout.BeginArea(new Rect(this.RenderOffsetX, this.RenderOffsetY, this.Width, this.Height), this.Style);
            }
            {
                EditorGUILayout.BeginHorizontal(this.Style, GUILayout.Width(this.Width));
                {
                    RenderChildren();
                }
                EditorGUILayout.EndHorizontal();
            }
            if (this.LayoutMode != ePositioningLayout.Layout)
            {
                GUILayout.EndArea();
            }
        }
    }

    [UWidgetWidgetAttribute(eUWidgetDesignerCategory.Panels, "Vertical Layout")]
    public class UEditorPanelVertical : UEditorPanelBase
    {

        public UEditorPanelVertical() : base(eWidgetType.PanelVertical) 
        {
            //this.LayoutMode = ePositioningLayout.Layout;
            this.Name = "VerticalLayout";
        }

        protected override void WidgetRender()
        {

            if (this.LayoutMode != ePositioningLayout.Layout)
            {
                GUILayout.BeginArea(new Rect(this.RenderOffsetX, this.RenderOffsetY, this.Width, this.Height), this.Style);
            }
            {
                EditorGUILayout.BeginVertical(this.Style, GUILayout.Width(this.Width));
                {
                    RenderChildren();
                }
                EditorGUILayout.EndVertical();
            }
            if (this.LayoutMode != ePositioningLayout.Layout)
            {
                GUILayout.EndArea();
            }
        }
    }

    [UWidgetWidgetAttribute(eUWidgetDesignerCategory.Panels, "Scroll Layout")]
    public sealed class UEditorPanelScroll : UEditorPanelBase
    {

        public UEditorPanelScroll() : base(eWidgetType.PanelScroll) { }

        public Vector2 ScrollPosition = new Vector2();

        protected override void WidgetRender()
        {

            ScrollPosition = EditorGUILayout.BeginScrollView(ScrollPosition, this.Style, GUILayout.Width(((UEditorWidgetBase)this.parent).Width + 10), GUILayout.Height(((UEditorWidgetBase)this.parent).Height + 10));
            {
                RenderChildren();
            }
            EditorGUILayout.EndScrollView();
        }
    }

    [UWidgetWidgetAttribute(eUWidgetDesignerCategory.Panels, "Area Layout")]
    public class UEditorPanelArea : UEditorPanelBase
    {

        public UEditorPanelArea() : base(eWidgetType.PanelArea) 
        {
            this.Name = "AreaPanel";
        }

        protected override void WidgetRender()
        {
            Rect __areaRect;

            if (this.LayoutMode == ePositioningLayout.Layout)
            {
                __areaRect = EditorGUILayout.GetControlRect(GUILayout.Width(this.Width), GUILayout.Height(this.Height));
            }
            else
            {
                __areaRect = new Rect(this.RenderOffsetX, this.RenderOffsetY, this.Width, this.Height);
            }

            //GUI.BeginGroup(new Rect(this.RenderOffsetX, this.RenderOffsetY, this.Width, this.Height), this.Style);
            GUILayout.BeginArea(__areaRect, this.Style);
            {
                RenderChildren();
            }
            GUILayout.EndArea();
        }
    }

}