namespace uAssist.UEditorWidgets
{
    using System;
    using System.Collections.Generic;
    using UnityEditor;
    using UnityEngine;
    using uAssist.UEditorWidgets.Internal;

    [UWidgetWidgetAttribute(eUWidgetDesignerCategory.Panels,"Tab Panel")]
    public class UEditorPanelTab : UEditorPanelArea
    {

        [UWidgetPropertyAttribute("Toolbar settings")]
        public UEditorControlToolbarBase ToolBar = UWidget.Create<UEditorControlToolbarBase>();

        [UWidgetPropertyAttribute("Menu Options", CustomEditor = (typeof(Designer_UEditorPanelTab_TabAreaData)), PropCodeGen = typeof(CGen_List_TabPanelItem))]
        public List<UEditorPanelTab_TabPanelItem> TabPanelData = new List<UEditorPanelTab_TabPanelItem>();
        

        public UEditorPanelTab() : base ()
        {
            this.Name = "TabPanel";
            this.ToolBar.Name = "ToolBar";
            //this.AddChild(ToolBar);
            this.Height = 500;
            this.Width = 500;
        }
        

        protected override void WidgetRender()
        {
            

            for (int i = 0; i < this.ToolBar.MenuOptions.Count; i++)
            {
                this.TabPanelData[i].Panel.WidgetShouldRender = false;
            }
            
            if (TabPanelData.Count > 0 && this.ToolBar.SelectedIndex >= 0)
            {
                this.TabPanelData[this.ToolBar.SelectedIndex].Panel.WidgetShouldRender = true;
            }

            base.WidgetRender();
        }

        public override void RenderChildren()
        {
            ToolBar.Render();

            base.RenderChildren();
        }
    }
}