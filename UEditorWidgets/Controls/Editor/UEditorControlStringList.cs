namespace uAssist.UEditorWidgets
{
    using System;
    using UnityEngine;
    using UnityEditor;
    using System.Collections;
    using System.Collections.Generic;



    public class PanelItem : UEditorPanelHorizonal
    {
        public UEditorWidgetTextField TextField = UWidget.Create<UEditorWidgetTextField>();
        public UEditorWidgetButton AddButton = UWidget.Create<UEditorWidgetButton>("", true);
        public UEditorWidgetButton RemoveButton = UWidget.Create<UEditorWidgetButton>("", true);

        
        /*
        public override bool BindTo(object Object, string MemberName)
        {
            if (base.BindTo(Object, MemberName) == false)
            {
                return false;
            }

            if (this.GetBoundValueType() != typeof(string))
            {
                return false;
            }

            //this.TextField.BindTo(Object, MemberName);
            //this.AddButton.BindTo(Object, MemberName);
            this.AddButton.Label = "+";
            //this.RemoveButton.BindTo(Object, MemberName);
            this.RemoveButton.Label = "-";

            

            return true;
        }
        */

        //Public Constructor
        public PanelItem() 
        {
            this.Height = 20;
            this.Width = 220;
            this.LayoutMode = ePositioningLayout.Layout;
            this.TextField.Width = 150;
            this.TextField.LayoutMode = ePositioningLayout.Layout;
            this.AddButton.Width = 20;
            this.AddButton.Height = 16;
            this.AddButton.Label = "+";
            this.AddButton.LayoutMode = ePositioningLayout.Layout;
            this.RemoveButton.Width = 20;
            this.RemoveButton.Height = 16;
            this.RemoveButton.Label = "-";
            this.RemoveButton.LayoutMode = ePositioningLayout.Layout;

            this.AddChild(this.TextField);
            this.AddChild(this.RemoveButton);
            this.AddChild(this.AddButton);

        }

    }

    [UWidgetWidgetAttribute(eUWidgetDesignerCategory.Other,"String List")]
    public class UEditorControlStringList : UEditorPanelVertical
    {
        public event UEditorWidget_MenuItemChanged onItemRemoved;
        public event UEditorWidget_MenuItemChanged onItemAdded;
        
        //This is disabled as an editable property, basically it will try and eat itself.
        [UWidgetPropertyAttribute("Sting List", HideInProperties = true, CustomEditor=(typeof (UEditorControlStringList)), PropCodeGen=(typeof(CGen_StringList)))]
        public List<string> StringList
        {
            get
            {
                return this.GetBoundValue<List<string>>();
            }
            set
            {
                this.SetBoundValue(value);
            }
        }

        public  UEditorControlStringList()
        {
            this.Name = "StringListControl";
            this.SuppressBindingWarnings = true;
            this.LayoutMode = ePositioningLayout.Layout;
            this._newItemPanel.BindTo(-1, "this");

        }

        public override bool BindTo(object Object, string MemberName)
        {
            base.BindTo(Object, MemberName);

            this.StringList = this.GetBoundValue<List<string>>();
            
            BuildPanel();

            return true;
        }

        private PanelItem _newItemPanel = UWidget.Create<PanelItem>("",true);

        [UWidgetPropertyAttribute("Flatten Strings")]
        public bool FlattenStirngs = false;

        [UWidgetPropertyAttribute("Line spacing")]
        public int LineSpacing = 20;

        public void BuildPanel()
        {
            this.ClearChilden();
            for (int i = 0; i < StringList.Count; i++)
            {
                PanelItem __newItem = UWidget.Create<PanelItem>();
                __newItem.TextField.Text = StringList[i];
                __newItem.AddButton.BindTo(i, "this");
                __newItem.AddButton.OnClick += AddButton_OnClick;
                __newItem.RemoveButton.BindTo(i, "this");
                __newItem.RemoveButton.OnClick += RemoveButton_OnClick;
                this.AddChild(__newItem);
            }

            this.Raise_onContainerChange();
        }

        void OnEnable()
        {
            _newItemPanel.AddButton.OnClick += AddButton_OnClick;

            for (int i = 0; i < StringList.Count; i++)
            {
                ((PanelItem)this.Children[i]).AddButton.OnClick += AddButton_OnClick;
                ((PanelItem)this.Children[i]).RemoveButton.OnClick += RemoveButton_OnClick;
            }
        }


        void RemoveButton_OnClick(IUEditorWidgetClickable sender, EventArgs e)
        {
            UEditorWidgetButton __castButton = (UEditorWidgetButton)sender;
            int __index = __castButton.GetBoundValue<int>();
            
            //If the index is the "new item" we can't remove here
            if (__index == -1)
            {
                return;
            }
            
            this.StringList.RemoveAt(__castButton.GetBoundValue<int>());
            BuildPanel();
            if (this.onItemRemoved != null)
            {
                this.onItemRemoved(this, __index);
            }
        }

        void AddButton_OnClick(IUEditorWidgetClickable sender, EventArgs e)
        {
            UEditorWidgetButton __castButton = (UEditorWidgetButton)sender;
            int __index = __castButton.GetBoundValue<int>();

            if (__index == -1)
            {
                this.StringList.Add(string.Empty);
            }
            else
            {
                this.StringList.Insert((__index), string.Empty);
            }
            
            BuildPanel();
            if (this.onItemAdded != null)
            {
                this.onItemAdded(this, __index);
            }
        }

        protected override void WidgetRender()
        {
            this.Height = this.StringList.Count * LineSpacing;

            if (this.StringList.Count != this.Children.Count)
            {
                this.BuildPanel();
            }

            for (int i = 0; i < StringList.Count; i++)
            {
                this.Children[i].Render();

                //Reverify the count
                //Can happen if a string item is removed in the rendering cycle and we loose one.
                if (i < StringList.Count)
                {
                    this.StringList[i] = ((PanelItem)this.Children[i]).TextField.Text;
                }
            }

            _newItemPanel.Render();
            if (_newItemPanel.TextField.Text != "")
            {
                this.StringList.Add(_newItemPanel.TextField.Text);
                BuildPanel();
                _newItemPanel.TextField.Text = String.Empty;
                if (this.onItemAdded != null)
                {
                    this.onItemAdded(this, this.StringList.Count -1);
                }
            }
        }

    }
}