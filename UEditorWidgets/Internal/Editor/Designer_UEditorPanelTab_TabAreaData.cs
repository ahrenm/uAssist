namespace uAssist.UEditorWidgets.Internal
{
    using System.Collections.Generic;
    using uAssist.UEditorWidgets;

    public class Designer_UEditorPanelTab_TabAreaData : UEditorWidgetFoldout
    {

        private UEditorPanelVertical _controlPanel = UWidget.Create<UEditorPanelVertical>();
        private UEditorControlStringList _listMenuOptions = UWidget.Create<UEditorControlStringList>("", true);

        private UEditorPanelTab _tabAreaPanel;
        private UEditorPanelTab TabAreaPanel
        {
            get
            {
                if (this._tabAreaPanel == null)
                {
                    this._tabAreaPanel = (UEditorPanelTab)this.BoundObject;
                }
                
                return _tabAreaPanel;
            }
        }

        private List<UEditorPanelTab_TabPanelItem> _tabPanelData;
        private List<UEditorPanelTab_TabPanelItem> TabPanelData
        {
            get
            {
                if (this._tabPanelData == null)
                {
                    this._tabPanelData = this.GetBoundValue<List<UEditorPanelTab_TabPanelItem>>();
                }
                return this._tabPanelData;
            }
        }

        public Designer_UEditorPanelTab_TabAreaData()
        {
            this.LayoutMode = ePositioningLayout.Layout;
            this.AddChild(_controlPanel);
            _controlPanel.LayoutMode = ePositioningLayout.Layout;
            _controlPanel.Padding.left = 15;
            _controlPanel.AddChild(_listMenuOptions);

            _listMenuOptions.LayoutMode = ePositioningLayout.Layout;
                
        }

        public override bool BindTo(object Object, string MemberName)
        {
            if (base.BindTo(Object, MemberName) == false)
            {
                return false;
            }
            
            if (this.GetBoundValueType() != typeof(List<UEditorPanelTab_TabPanelItem>))
            {
                return false;
            }

            //this.TabPanelData = this.GetBoundValue<List<UEditorPanelTab_TabPanelItem>>();
            //this.TabAreaPanel = (UEditorPanelTab)this.BoundObject;

            foreach (var item in TabPanelData)
            {
                this._listMenuOptions.StringList.Add(item.DisplayName);
                //TabAreaPanel.ToolBar.MenuOptions.Add(item.DisplayName);
            }

            base.BindTo(null, null);
            this.Label = "Tab Menu";

            _listMenuOptions.onItemAdded += _listMenuOptions_onItemAdded;
            _listMenuOptions.onItemRemoved += _listMenuOptions_onItemRemoved;

            return true;
        }


        void _listMenuOptions_onItemRemoved(UEditorWidgetBase sender, int  indexRemoved)
        {
            //Remove the actual panel
            UEditorPanelTab_TabPanelItem __itemToRemove = this.TabPanelData[indexRemoved];
            TabAreaPanel.RemoveChild(__itemToRemove.Panel);
            
            //Remove the associated panelData item
            this._tabPanelData.RemoveAt(indexRemoved);

            //Remove if from the Menu options
            this.TabAreaPanel.ToolBar.MenuOptions.RemoveAt(indexRemoved);
        }

        void _listMenuOptions_onItemAdded(UEditorWidgetBase sender, int indexAdded)
        {
            UEditorPanelVertical __newPanel = UWidget.Create<UEditorPanelVertical>();
            __newPanel.Name = TabAreaPanel.Name + "_"  + _listMenuOptions.StringList[indexAdded];
            __newPanel.LayoutMode = ePositioningLayout.Layout;
            __newPanel.WidgetShouldRender = false;

            this.TabPanelData.Insert(indexAdded, new UEditorPanelTab_TabPanelItem(){Panel = __newPanel});
            this.TabAreaPanel.AddChild(__newPanel);
            this.TabAreaPanel.Raise_onContainerChange();
            TabAreaPanel.ToolBar.MenuOptions.Insert(indexAdded, _listMenuOptions.StringList[indexAdded]);
        }


        protected override void WidgetRender()
        {
            //Keep all the data in sync
            for (int i = 0; i < TabPanelData.Count; i++)
            {
                //Check that the display name of the panel data matches what is in the UI 
                if (TabPanelData[i].DisplayName != this._listMenuOptions.StringList[i])
                {
                    TabPanelData[i].DisplayName = this._listMenuOptions.StringList[i];
                }

                TabAreaPanel.ToolBar.MenuOptions[i] = TabPanelData[i].DisplayName;

                if (TabPanelData[i].Panel.Name != TabAreaPanel.Name + "_" + TabPanelData[i].FlatName)
                {
                    TabPanelData[i].Panel.Name = TabAreaPanel.Name + "_" + TabPanelData[i].FlatName;
                }

                //A tab panel could be removed via the hirearchy or other means, if this happens we need to find 
                //and remove the associated panel data item
                bool __panelFound = false;
                foreach (var item in TabAreaPanel.Children)
                {
                    if (item.Name == TabPanelData[i].Panel.Name)
                    {
                        __panelFound = true;
                        break;
                    }
                }
                if (__panelFound == false)
                {
                    this.TabPanelData.RemoveAt(i);
                    this.TabAreaPanel.ToolBar.MenuOptions.RemoveAt(i);
                    this._listMenuOptions.StringList.RemoveAt(i);
                    this._listMenuOptions.BuildPanel();
                }
            }

            base.WidgetRender();
        }
    }
}