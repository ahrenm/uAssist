namespace uAssist.EditorDesigner
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using UnityEngine;
    using UnityEditor;
    using System.Collections;
    using uAssist.UEditorWidgets;
    using uAssist.Forms;
    
    public class frmCanvas : frmBase
    {

        public delegate void CanvasActiveControl_Changed(UEditorWidgets.UEditorWidgetBase ActiveControl);
        public event CanvasActiveControl_Changed CanvasActiveControlChanged;
        
        
        //The reference for the class currently being edited in the desiger
        public frmBase DesignerClass;


        private UEditorWidgetBase _selectedWidget;
        public UEditorWidgetBase SelectedWidget
        {
            get
            {
                return _selectedWidget;
            }
            set
            {
                if (this._selectedWidget != value)
                {
                    this._selectedWidget = value;
                    if (this.CanvasActiveControlChanged != null)
                    {
                        this.CanvasActiveControlChanged(value);
                    }


                    //Keep the selected items in the toolbox in sync
                    if (value != null)
                    {
                        this.ToolBox.SelectedFoldout = (UEditorWidgetFoldout)UWidget.FindWidgetById(this.ToolBox.HierarchyContainer, value.ObjectID);
                    }
                }
            }

        }

        private IWidgetContainer _currentContainer;
        public IWidgetContainer CurrentContainer
        {
            get
            {
                //If no value is set, default to the canvs
                if (this._currentContainer == null)
                {
                    this._currentContainer = (IWidgetContainer)this;
                }
                return _currentContainer;
            }
            set
            {
                if (_currentContainer != value)
                {
                    _currentContainer = value;
                }
            }
        }

        public frmCanvas():base()
        {
            this.InspectorUpdateRender = true;
            this.FormTitle = "Designer Canvas";
         
        }

#region Window Mgmt

        private frmToolbox _windowToolbox;
        public  frmToolbox ToolBox
        {
            get
            {
                if (this._windowToolbox == null)
                {
                    frmToolbox[] __foundToolbox = Resources.FindObjectsOfTypeAll<frmToolbox>();
                    if (__foundToolbox.Count() > 1)
                    {
                        //Try to determine the active window by seeing which instance is listenting for events.
                        //This should be a fringe case when the Designer window is loaded into the designer.
                        for (int i = 0; i < __foundToolbox.Count(); i++)
                        {
                            if (__foundToolbox[i].EventsEnabled == true)
                            {
                                this._windowToolbox = __foundToolbox[i];
                            }
                        }
                    }
                    if (__foundToolbox.Count() == 1 && __foundToolbox[0].EventsEnabled == true)
                    {
                        this._windowToolbox = __foundToolbox[0];
                    }
                    else
                    {
                        //this._windowToolbox = UnityEditor.EditorWindow.GetWindow<frmToolbox>();
                        this._windowToolbox = UnityEditor.EditorWindow.CreateInstance<frmToolbox>();
                    }
                }
                return this._windowToolbox;
            }
            set
            {
                this._windowToolbox = value;
            }
        }

        private frmPropertyWindow _windowProperty;
        public frmPropertyWindow PropertyPanel
        {
            get
            {
                if (this._windowProperty == null)
                {
                    frmPropertyWindow[] __foundProperty = Resources.FindObjectsOfTypeAll<frmPropertyWindow>();
                    if (__foundProperty.Count() > 1)
                    {
                        //Try to determine the active window by seeing which instance is listenting for events.
                        //This should be a fringe case when the Designer window is loaded into the designer.
                        for (int i = 0; i < __foundProperty.Count(); i++)
                        {
                            if (__foundProperty[i].EventsEnabled == true)
                            {
                                this._windowProperty = __foundProperty[i];
                            }
                        }
                    }
                    if (__foundProperty.Count() == 1)
                    {
                        this._windowProperty = __foundProperty[0];
                    }
                    else
                    {
                        this._windowProperty = UnityEditor.EditorWindow.GetWindow<frmPropertyWindow>(typeof (UnityEditor.Editor));
                    }
                }
                return this._windowProperty;
            }
            set
            {
                this._windowProperty = value;
            }
        }

#endregion

        private Type _designerType;
        public Type DesignerType
        {
            get
            {
                if (_designerType == null && this._designerTypeAsString != "" )
                {
                    _designerType = Type.GetType(this._designerTypeAsString);
                }
                return _designerType;
            }
            set
            {
                _designerType = value;
                _designerTypeAsString = _designerType.AssemblyQualifiedName;
            }

        }
        private string _designerTypeAsString = "";

        //public int DesignerInstanceID;

        public void LoadForm(Type FormToLoad)
        {
            //If the requested class is neither frmBase or a subclass, then don't load it.
            //TODO: Work out this logic
            /*
            if (FormToLoad.GetType() != typeof (frmBase) |  FormToLoad.IsSubclassOf(typeof (frmBase)) == false)
            {
                //TODO: Error out
                return;
            }
            */

            
            this.PropertyPanel.Canvas = this;
            this.CurrentContainer = (IWidgetContainer)this;

            //Load the Designer class
            this.DesignerType = FormToLoad;
            this.DesignerClass = (frmBase)ScriptableObject.CreateInstance(FormToLoad);
            this.DesignerClass.EventsEnabled = false;
            this.DesignerClass.hideFlags = HideFlags.HideAndDontSave;
            //this.DesignerInstanceID = this.DesignerClass.GetInstanceID();
            
            //Cross connect the Canvas componnts to the desinger class
            this.Children = this.DesignerClass.Children;
            this.ToolBox.Canvas = this;
            this.ToolBox.HierarchyContainer.ObjectID = this.ObjectID;

            //Hacky way of tricking the property panel to switch to the form settings views.
            //TODO: Work out how to do this properly
            //this.PropertyPanel.Canvas_CanvasActiveControlChanged(new UEditorWidgetBase(eWidgetType.Generic) { Name = "__widgetDesignerRoot" });
            this.PropertyPanel.CanvasBindings();

            this.ToolBox.Show();
            this.PropertyPanel.Show();

        }

        public override void OnGUI()
        {
            base.OnGUI();

            if (DesignerClass != null)
            {
                if (DesignerClass.MaxWidth >1)
                {
                    EditorGUI.DrawRect(new Rect(DesignerClass.MaxWidth, 0, this.position.width - DesignerClass.MaxWidth, this.position.height), Color.black);
                }
                if (DesignerClass.MaxHeight > 1)
                {
                    EditorGUI.DrawRect(new Rect(0, DesignerClass.MaxHeight, this.position.width, this.position.height - DesignerClass.MaxHeight), Color.black);
                }

            }

        }

        protected override void EnableEvents()
        {
            base.EnableEvents();
            
            this.CanvasActiveControlChanged += this.PropertyPanel.Canvas_CanvasActiveControlChanged;
        }

        protected override void DisableEvents()
        {
            base.DisableEvents();

            this.CanvasActiveControlChanged -= this.PropertyPanel.Canvas_CanvasActiveControlChanged;
        }
     
        
    }
}