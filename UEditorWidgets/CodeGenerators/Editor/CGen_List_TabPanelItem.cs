namespace uAssist.UEditorWidgets
{
    using System;
    using System.Collections.Generic;
    using uAssist.UEditorWidgets.Internal;

    public class CGen_List_TabPanelItem : ICodeGenerator
    {
        public string CGenInitalizer()
        {
            throw new NotImplementedException();
        }

        public List<string> CGenPropertySetters(string PropFQN, object PropValue)
        {
            if (PropValue.GetType() != typeof(List<UEditorPanelTab_TabPanelItem>))
            {
                throw new Exception("Failed to cast " + PropFQN + " to type List<UEditorPanelTab_TabPanelItem>");
            }

            List<UEditorPanelTab_TabPanelItem> __castList = (List<UEditorPanelTab_TabPanelItem>)PropValue;
            List<string> __properties = new List<string>();


            //These operations actually occour one level above the object passed into as the FQN, so we elimnate that level.
            PropFQN = PropFQN.Substring(0, PropFQN.LastIndexOf('.'));

            __properties.Add(PropFQN + ".ToolBar.MenuOptions.Clear();");

            foreach (var item in __castList)
            {
                __properties.Add(PropFQN + ".ToolBar.MenuOptions.Add(\"" + item.DisplayName + "\");");
                __properties.Add(PropFQN +  ".TabPanelData.Add(new UEditorPanelTab_TabPanelItem() { DisplayName = \"" + item.DisplayName + "\", parent = this.TabPanel });");
            }
            return __properties;
        }

    }
}
