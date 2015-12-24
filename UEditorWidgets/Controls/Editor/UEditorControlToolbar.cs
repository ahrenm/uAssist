namespace uAssist.UEditorWidgets
{
    using System;
    using UnityEngine;
    using UnityEditor;
    using System.Collections;
    using System.Collections.Generic;

    [UWidgetWidgetAttribute(eUWidgetDesignerCategory.Controls, "Tab Menu")]
    public class UEditorControlToolbar : UEditorControlToolbarBase
    {
        [UWidgetPropertyAttribute("Menu Options", HideInProperties = true, CustomEditor = typeof(UEditorControlStringList), PropCodeGen = (typeof(CGen_StringList)))]
        public override List<string> MenuOptions
        {
            get
            {
                return _menuOptions;
            }
            set
            {
                _menuOptions = value;
            }
        }
    }


    
    public class UEditorControlToolbarBase : UEditorWidgetTextBase
    {
        
        public event UEditorWidget_MenuItemSelected onMenuSelect;

        [UWidgetProperty(HideInProperties = true, PropCodeGen = (typeof(CGen_StringList)))]
        protected List<string> _menuOptions = new List<string>();
        
        public virtual List<string> MenuOptions
        {
            get
            {
                return _menuOptions;
            }
            set
            {
                _menuOptions = value;
            }
        }

        public UEditorControlToolbarBase(): base(eWidgetType.Generic)
        {
            this.Name = "MenuToolbar";
            this.BaseStyle = "button";
            this.Height = 20;
            this.Width = 200;
            this.Alignment = TextAnchor.MiddleCenter;
        }
        

        int _selectedIndex = 0;
        public int SelectedIndex
        {
            get
            {
                return _selectedIndex;
            }
            set
            { 
                if (value != _selectedIndex)
                {
                    _selectedIndex = value;

                    if (onMenuSelect != null)
                    {
                        _selectedIndex = value;
                        this.onMenuSelect(this, this.SelectedMenu);
                    }
                }
            }
        }
        public string SelectedMenu
        {
            get
            {
                return MenuOptions[_selectedIndex];
            }
        }


        protected override void WidgetRender()
        {
            SelectedIndex = GUILayout.Toolbar(SelectedIndex, this.MenuOptions.ToArray(), this.Style, GUILayout.Height(this.Height + 10), GUILayout.Width(this.Width * this.MenuOptions.Count));
        }

    }
}