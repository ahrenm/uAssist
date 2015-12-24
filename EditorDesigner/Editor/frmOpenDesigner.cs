
namespace uAssist.EditorDesigner
{
    using UnityEngine;
    using UnityEditor;
    using System.Collections;
    using System.Collections.Generic;
    using uAssist.UEditorWidgets;
    using uAssist.Forms;
    using System.Reflection;
    using System;

    public partial class frmOpenDesigner : frmBase
    {

        #region Local vars and helpers

        [Serializable]
        private class ListItems
        {
            public ListItems(string name, Type type)
            {
                this.Name = name;
                this.ItemType = type;
            }

            public Type ItemType;
            public string Name;
        }

        private List<ListItems> _listFormTypes = new List<ListItems>();
        private UEditorWidgetButton _activeButton;

#endregion


#region UEditor plumbing

        //Public form constructor
        public frmOpenDesigner() : base()
        {

            //Subscribe to all required events
            this.EnableEvents();

            _btnOpenForm.GUIEnabled = false;
            _btnCancel.GUIEnabled = true;
            //Bind widget controls
            this.SetupBindings();

        }

        protected override void EnableEvents() 
        {
            base.EnableEvents();

            //Wire in widget events here. For Example:
            //_btnAddLabel.OnClick += this._btnAddLabel_OnClick;
            _btnCancel.OnClick += _btnCancel_OnClick;
            _btnOpenForm.OnClick += _btnOpenForm_OnClick;
           
        }

        

        protected override void DisableEvents()
        {
            base.DisableEvents();

            //Disable events here. For example:
            //_btnAddLabel.OnClick -= this._btnAddLabel_OnClick;

            //NOTE: This method is called by the UEditor Designer when loading a form in for editing.
            //Any activity required to make the form safe for editing including unsubscribing events and bindings should go here.

            _btnCancel.GUIEnabled = false;
            _btnOpenForm.GUIEnabled = false;
            _btnCancel.OnClick -= _btnCancel_OnClick;
            _btnOpenForm.OnClick -= _btnOpenForm_OnClick;

            _formsList.ClearChilden();

        }

        private void SetupBindings()
        {
            //Setup widget bindings. For example.
            //_btnAddLabel.BindTo(this, this.Name);

            FindEditableForms();

        }

        #endregion

        public void FindEditableForms()
        {
            _formsList.ClearChilden();

            //Go searching for attributes 
            foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                foreach (Type type in assembly.GetTypes())
                {
                    object[] __foundAttributes = type.GetCustomAttributes(typeof(UWidgetFormAttribute),  false);
                    if (__foundAttributes != null && __foundAttributes.Length == 1)
                    {
                        UWidgetFormAttribute __formAttribute = __foundAttributes[0] as UWidgetFormAttribute;

                        if (__formAttribute.CanEditInDesigner)
                        {
                            _listFormTypes.Add(new ListItems(type.FullName, type));

                            UEditorWidgetButton __formItem = UWidget.Create<UEditorWidgetButton>();
                            __formItem.BaseStyle = "label";
                            __formItem.Width = 200;
                            __formItem.Height = 16;
                            __formItem.LayoutMode = ePositioningLayout.Layout;
                            __formItem.Label = type.FullName;
                            __formItem.Alignment = TextAnchor.MiddleLeft;
                            __formItem.Padding.left = 5;
                            __formItem.OnClick += __formItem_OnClick;
                            _formsList.AddChild(__formItem);
                        }
                    }
                }
            }
        }



        #region Widget event handlers

        private void _btnOpenForm_OnClick(IUEditorWidgetClickable sender, System.EventArgs e)
        {

            if (this._activeButton != null)
            {
                foreach (var item in _listFormTypes)
                {
                    if (item.Name == _activeButton.Label)
                    {
                        EditorDesignerEngine.LoadEditor(item.ItemType);
                        this.CloseWindow = true;
                    }
                }
            }
        }

        private void _btnCancel_OnClick(IUEditorWidgetClickable sender, System.EventArgs e)
        {
            this.CloseWindow = true;
        }

        private void __formItem_OnClick(IUEditorWidgetClickable sender, EventArgs e)
        {
            if (this._activeButton != null)
            {
                this._activeButton.FontStyle = FontStyle.Normal;
            }

            this._activeButton = sender as UEditorWidgetButton;
            this._activeButton.FontStyle = FontStyle.Bold;
            this._btnOpenForm.GUIEnabled = true;
        }

        #endregion


        #region Functional code

        #endregion

    }
}