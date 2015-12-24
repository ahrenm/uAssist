
namespace uAssist.EditorDesigner
{
    using UnityEngine;
    using UnityEditor;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using uAssist.UEditorWidgets;
    using uAssist.Forms;

    public partial class frmPropertyWindow : frmBase
    {

        private const string _CodeGenDropLocation = "Assets\\uAssist\\Forms\\Editor\\";

        #region Local vars and helpers

        [SerializeField]
        private int _canvasID = 0;
        private frmCanvas _canvas;
        public frmCanvas Canvas
        {
            get
            {
                if (this._canvas == null && this._canvasID != 0)
                {
                    this._canvas = (frmCanvas)EditorUtility.InstanceIDToObject(this._canvasID);
                }
                return _canvas;
            }
            set
            {
                if (value == null)
                    return;

                this._canvas = value;
                this._canvasID = this._canvas.GetInstanceID();
            }
        }

#endregion


#region UEditor plumbing

        //Public form constructor
        public frmPropertyWindow() : base()
        {

            //Bind widget controls
            this.SetupBindings();

            this._vertLayoutFormSettings.WidgetShouldRender = false;
            this._vertLayoutPropSettings.WidgetShouldRender = false;
            
        }

        public override void OnEnable()
        {
            base.OnEnable();

            //Rebuild the property sheet on a reload.
            if (this.Canvas != null && Canvas.SelectedWidget != null)
            {
                this.Canvas_CanvasActiveControlChanged(Canvas.SelectedWidget);
                this.CanvasBindings();
            }
        }

        protected override void EnableEvents() 
        {
            base.EnableEvents();

            //Wire in widget events here. For Example:
            //_btnAddLabel.OnClick += this._btnAddLabel_OnClick;

            _btnRemoveWidget.OnClick += _btnRemoveWidget_OnClick;
            _btnMoveUp.OnClick += _btnMoveUp_OnClick;
            _btnMoveDown.OnClick += _btnMoveDown_OnClick;
            _btnGenerateCode.OnClick += _btnGenerateCode_OnClick;

        }

        

        protected override void DisableEvents()
        {
            base.DisableEvents();

            //Disable events here. For example:
            //_btnAddLabel.OnClick -= this._btnAddLabel_OnClick;

            //NOTE: This method is called by the UEditor Designer when loading a form in for editing.
            //Any activity required to make the form safe for editing including unsubscribing events and bindings should go here.
            
            _btnRemoveWidget.OnClick -= _btnRemoveWidget_OnClick;
            _btnMoveUp.OnClick -= _btnMoveUp_OnClick;
            _btnMoveDown.OnClick -= _btnMoveDown_OnClick;
            _btnGenerateCode.OnClick -= _btnGenerateCode_OnClick;

        }

        private void SetupBindings()
        {
            //Setup widget bindings. For example.
            //_btnAddLabel.BindTo(this, this.Name);


            //TODO: Work out if these actually need to be here
            //this._formName.PropertyLabel.Label = "Class Name";
            //this._formTitle.PropertyLabel.Label = "Form Title";
            //this._formInspectorUpdate.PropertyLabel.Label = "Render on Inspector Update";

        }

        //TODO: This is crap having there here, but because of the dependency on Canvas they need to be called seperatly
        public void CanvasBindings()
        {
            this._formName.BindTo(Canvas.DesignerClass, "name");
            this._formTitle.BindTo(Canvas.DesignerClass, "FormTitle");
            this._formInspectorUpdate.BindTo(Canvas.DesignerClass, "InspectorUpdateRender");
            this._formHeight.BindTo(Canvas.DesignerClass, "MaxHeight");
            this._formWidth.BindTo(Canvas.DesignerClass, "MaxWidth");
            this._formDropLocation.BindTo(Canvas.DesignerClass, "DesignerDropOverridePath");
            this._formNamespace.BindTo(Canvas.DesignerClass, "DesignerNameSpace");
            this._formAutoMenu.BindTo(Canvas.DesignerClass,"AutoMenuOption");
            this._formHeight.BindTo(Canvas.DesignerClass, "MaxHeight");
            this._formWidth.BindTo(Canvas.DesignerClass, "MaxWidth");

            this._formInspectorUpdate.PropertyLabel.Label = "Render on Inspector Update";
            this._formAutoMenu.PropertyLabel.Label = "Create Toolbar Menu";
            
        }


#endregion


#region Widget event handlers

        public void Canvas_CanvasActiveControlChanged(UEditorWidgetBase ActiveControl)
        {
            if (ActiveControl == null)
            {
                return;
            }

            if (ActiveControl.Name == "__widgetDesignerRoot")
            {
                this._vertLayoutFormSettings.WidgetShouldRender = true;
                this._vertLayoutPropSettings.WidgetShouldRender = false;
                this.lblPropertyName.Label = "Form settings";
            }
            else
            {
                this._vertLayoutFormSettings.WidgetShouldRender = false;
                this._vertLayoutPropSettings.WidgetShouldRender = true;

                this.lblPropertyName.Label = ActiveControl.Name;
                this._lblWidgetType.Label = "(" + ActiveControl.GetType().FullName + ")";
                this._widgetName.BindTo(ActiveControl, "Name");
                this._propertiesPanel.BindTo(ActiveControl, "this");
            }
        }


        void _btnRemoveWidget_OnClick(IUEditorWidgetClickable sender, System.EventArgs e)
        {
            
            if (Canvas.SelectedWidget == null)
            {
                return;
            }

            //Remove the widget
            //If the selected widget is a container then we need to remove from it's parent
            if (Canvas.SelectedWidget.GetType().IsSubclassOf(typeof (UEditorPanelBase)))
            {
                ((UEditorPanelBase)Canvas.CurrentContainer).parent.RemoveChild(Canvas.SelectedWidget);
            }
            else //Else remove from the CurrentContainer
            {
                Canvas.CurrentContainer.RemoveChild(Canvas.SelectedWidget);
            }


            //Update the selected widget if a parent exists
            if (Canvas.SelectedWidget.parent.GetType().IsSubclassOf(typeof (UEditorWidgetBase)))
            {
                Canvas.SelectedWidget = (UEditorWidgetBase)Canvas.SelectedWidget.parent;
            }
            else
            {
                Canvas.SelectedWidget = null;
            }
        }

        void _btnMoveUp_OnClick(IUEditorWidgetClickable sender, System.EventArgs e)
        {
            IWidgetContainer __parentContainer = Canvas.SelectedWidget.parent;
            int __widgetIndex = __parentContainer.Children.IndexOf(Canvas.SelectedWidget);
            
            //Did we find the widget and is it in a position that we can move up from
            if (__widgetIndex > 0)
            {
                //Update the canvas
                __parentContainer.Children.RemoveAt(__widgetIndex);
                __parentContainer.Children.Insert(__widgetIndex - 1, Canvas.SelectedWidget);
                __parentContainer.Raise_onContainerChange();
            }

        }

        void _btnMoveDown_OnClick(IUEditorWidgetClickable sender, System.EventArgs e)
        {
            IWidgetContainer __parentContainer = Canvas.SelectedWidget.parent;
            int __widgetIndex = __parentContainer.Children.IndexOf(Canvas.SelectedWidget);

            //Did we find the widget and is it in a position we can move down from
            if (__widgetIndex != -1 && __widgetIndex <= __parentContainer.Children.Count - 2)
            {
                //Update the canvas
                __parentContainer.Children.RemoveAt(__widgetIndex);
                __parentContainer.Children.Insert(__widgetIndex + 1, Canvas.SelectedWidget);
                __parentContainer.Raise_onContainerChange();
            }

        }

        private void _btnGenerateCode_OnClick(IUEditorWidgetClickable sender, System.EventArgs e)
        {
            this.BuildCode();
        }

        #endregion


        #region Functional code

        public void BuildCode()
        {
            //TODO: Expand the valid class checking requirements.
            if (this.Canvas.DesignerClass.name == "")
            {
                throw new Exception("Error starting code gen.\r\nForm must have a valid name");
            }

            //Determine the drop location for the code
            string __dropLocation = _CodeGenDropLocation;
            if (Canvas.DesignerClass.DesignerDropOverridePath != "")
            {
                __dropLocation = Canvas.DesignerClass.DesignerDropOverridePath;
            }


            //Spool up the code gen
            this.Canvas.DesignerClass.Children = this.Canvas.Children;
            UFormCodeGen __cGen = new UFormCodeGen(this.Canvas.DesignerClass);

            //Update the form settings
            __cGen.GeneratedClassName = this.Canvas.DesignerClass.name;
            __cGen.FormSettings.Add("FormTitle", "\"" + this.Canvas.DesignerClass.FormTitle + "\"");
            __cGen.FormSettings.Add("MaxHeight", this.Canvas.DesignerClass.MaxHeight.ToString());
            __cGen.FormSettings.Add("MaxWidth", this.Canvas.DesignerClass.MaxWidth.ToString());


            //Form setting for ->InspectorUpdateRender
            if (this.Canvas.DesignerClass.InspectorUpdateRender == true)
            {
                __cGen.FormSettings.Add("InspectorUpdateRender", "true");
            }

            //Form setting for ->DesignerDropOverridePath
            if (this.Canvas.DesignerClass.DesignerDropOverridePath != "")
            {
                __cGen.FormSettings.Add("DesignerDropOverridePath", "@\"" + this.Canvas.DesignerClass.DesignerDropOverridePath + "\"");
            }

            //Form setting for ->DesignerNameSpace
            if (this.Canvas.DesignerClass.DesignerNameSpace != "")
            {
                __cGen.FormSettings.Add("DesignerNameSpace", "@\"" + this.Canvas.DesignerClass.DesignerNameSpace + "\"");
            }

            //Form setting for ->AutoMenuOption
            if (this.Canvas.DesignerClass.AutoMenuOption != true)
            {
                __cGen.FormSettings.Add("AutoMenuOption", "true");
            }

            __cGen.PharseForm();

            if (System.IO.File.Exists(__dropLocation + this.Canvas.DesignerClass.name + ".cs") == false)
            {
                System.IO.File.WriteAllText(__dropLocation + this.Canvas.DesignerClass.name + ".cs", __cGen.CodeFront);
            }

            //Backup the code behind file if required
            if (System.IO.File.Exists(__dropLocation + this.Canvas.DesignerClass.name + ".generated.cs"))
            {
                DateTime __timeNow = DateTime.Now;
                string __dateStamp = string.Empty;
                __dateStamp += __timeNow.Day.ToString().PadLeft(2, '0');
                __dateStamp += __timeNow.Month.ToString().PadLeft(2, '0');
                __dateStamp += __timeNow.Year.ToString();
                __dateStamp += "_";
                __dateStamp += __timeNow.Hour.ToString().PadLeft(2, '0');
                __dateStamp += __timeNow.Minute.ToString().PadLeft(2, '0');
                __dateStamp += __timeNow.Second.ToString().PadLeft(2, '0');
                System.IO.File.Move(__dropLocation + this.Canvas.DesignerClass.name + ".generated.cs", __dropLocation + this.Canvas.DesignerClass.name + ".generated_backup_" + __dateStamp + ".txt");
            }

            //Write the code behind
            System.IO.File.WriteAllText(__dropLocation + this.Canvas.DesignerClass.name + ".generated.cs", __cGen.CodeBehind);

            AssetDatabase.ImportAsset(__dropLocation + this.Canvas.DesignerClass.name + ".generated.cs");
            AssetDatabase.ImportAsset(__dropLocation + this.Canvas.DesignerClass.name + ".cs");

            AssetDatabase.Refresh();
        }



        #endregion




    }
}