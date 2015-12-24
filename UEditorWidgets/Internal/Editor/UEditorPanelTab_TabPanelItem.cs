namespace uAssist.UEditorWidgets.Internal
{
    using System;
    using System.Collections.Generic;
    using UnityEditor;
    using UnityEngine;
    using uAssist.UEditorWidgets;

    [Serializable]
    public class UEditorPanelTab_TabPanelItem : IEquatable<UEditorPanelTab_TabPanelItem>
    {

        //public int Index;

        //This visual name of the tab
        private string _displayName = string.Empty;
        public string DisplayName
        {
            get
            {
                return _displayName;
            }
            set
            {
                if (value != _displayName)
                {
                    this._displayName = value;
                    this._flatName = UWidget.FlattenString(this._displayName);
                }
            }
        }

        public UEditorPanelTab parent;

        //The code friendly name of the tab
        [SerializeField]
        private string _flatName;
        public string FlatName
        {
            get
            {
                return _flatName;
            }
        }

        [SerializeField]
        private UEditorPanelVertical _panel;

        public UEditorPanelVertical Panel
        {
            get
            {
                if (this._panel != null)
                {
                    return this._panel;
                }
                if (this.parent != null)
                {
                    foreach (var item in this.parent.Children)
                    {
                        if (item.Name.Contains(this._flatName))
                        {
                            this._panel = item as UEditorPanelVertical;
                        }
                    }
                }
                return this._panel;
            }
            set
            {
                _panel = value;
            }
        }

        //We compare on flatname to determine equality
        public bool Equals(UEditorPanelTab_TabPanelItem other)
        {
            if (other == null)
            {
                return false;
            }
            if (this.FlatName == other.FlatName)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}