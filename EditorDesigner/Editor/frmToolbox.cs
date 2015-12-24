namespace uAssist.EditorDesigner
{
    using System;
    using UnityEngine;
    using UnityEditor;
    using System.Collections;
    using System.Collections.Generic;
    using uAssist.UEditorWidgets;
    using uAssist.Forms;

    [Serializable]
    public partial class frmToolbox : frmBase
    {

        private bool _controlKeyPressed = false;
            

        #region Local vars and helpers

        public UEditorWidgetFoldout DesignerRoot
        {
            get
            {
                return this.__widgetDesignerRoot;
            }
        }

        private UEditorWidgetFoldout _selectedFoldout;
        public UEditorWidgetFoldout SelectedFoldout
        {
            get
            {
                return _selectedFoldout;
            }
            set
            {
                //No good can come of this
                if (value == null)
                    return;

                if (this._selectedFoldout != value)
                {
                    //Return the visable status of the old selected foldout if one is set
                    if (this.SelectedFoldout != null)
                    {
                        this._selectedFoldout.FontSize = 11;
                        this._selectedFoldout.FontStyle = FontStyle.Normal;
                    }

                    //Update the reference to the new foldout
                    this._selectedFoldout = value;

                    //Update the visual style 
                    this._selectedFoldout.FontSize = 12;
                    this._selectedFoldout.FontStyle = FontStyle.Bold;

                    //
                    //Now we do a lot of work to update the container settings
                    //Only if we have a Canvas, if not, then bail out
                    if (Canvas == null)
                    {
                        return;
                    }

                    if (value.Name == "__widgetDesignerRoot")
                    {
                        Canvas.CurrentContainer = (IWidgetContainer)Canvas;
                        Canvas.SelectedWidget = value;
                        return;
                    }

                    //Get the actual widget the foldout is bound to
                    Canvas.SelectedWidget = (UEditorWidgetBase)value.BoundObject;

                    //If the selected widget is a container update the active contains
                    if (Canvas.SelectedWidget.GetType().IsSubclassOf(typeof(UEditorPanelBase)))
                    {
                        Canvas.CurrentContainer = (IWidgetContainer)this.Canvas.SelectedWidget;
                    }
                    else
                    {

                        //If the current widget has as parent, make that the active container
                        if (Canvas.SelectedWidget.parent != null)
                        {
                            this.Canvas.CurrentContainer = this.Canvas.SelectedWidget.parent;
                        }
                        else
                        {
                            //If the selected widget has no parent, set the container to the root of the canvas
                            Canvas.CurrentContainer = Canvas.CurrentContainer = (IWidgetContainer)Canvas;
                        }
                    }
                }
            }
        }

        public string ActiveContainerLabel
        {
            get
            {
                //The widget can boot before the canvas has had a change to kick in, so trap this
                if (Canvas != null && Canvas.CurrentContainer != null)
                {
                    if (Canvas.CurrentContainer.GetType().IsSubclassOf(typeof (frmBase)))
                    {
                        return "Active container->root";
                    }
                    else
                    {

                        return "Active container->" + ((UEditorWidgetBase)Canvas.CurrentContainer).Name;
                    }
                }
                else
                {
                    return "";
                }
            }
        }
        

        public IWidgetContainer HierarchyContainer
        {
            get
            {
                return (IWidgetContainer)__widgetDesignerRoot;
            }
            set
            {
                if (value.GetType().IsSubclassOf(typeof(UEditorWidgetFoldout)))
                {
                    __widgetDesignerRoot = (UEditorWidgetFoldout)value;
                }
                else
                {
                    throw new Exception("frmTooBox.HierarchyContainer must be assigned a value of type UEditorWidgetFoldout");
                }
            }
        }

        [SerializeField]
        private int _canvasID = -1;

        private frmCanvas _canvas;
        public frmCanvas Canvas
        {
            get
            {
                if (this._canvas == null && this._canvasID != -1 )
                {
                    this._canvas = (frmCanvas)EditorUtility.InstanceIDToObject(this._canvasID);
                }
                return _canvas;
            }
            set
            {
                this._canvas = value;
                this._canvasID = this.Canvas.GetInstanceID();
                this.BuildComponentHeriachy();
            }
        }

        #endregion


        #region UEditor plumbing

        //Public form constructor
        public frmToolbox(): base()
        {

            //Subscribe to all required events

            this.InspectorUpdateRender = true;

            this.SelectedFoldout = __widgetDesignerRoot;
            this.SelectedFoldout.FontSize = 12;
            this.SelectedFoldout.FontStyle = FontStyle.Bold;
        }


        protected override void EnableEvents()
        {
            if (__widgetDesignerRoot == null)
                return;

            base.EnableEvents();

            //Wire in widget events here. For Example:
            //_btnAddLabel.OnClick += this._btnAddLabel_OnClick;

            __widgetDesignerRoot.OnClick += __newFoldout_OnClick;
            //_btnGenCode.OnClick += _btnGenCode_OnClick;
            //_btnTBA.OnClick += _btnTBA_OnClick;
            _toolbox.OnClick += _toolbox_OnClick;

            //Bind in the hieratchy OnClick events
            EnableHierachyOnClick(__widgetDesignerRoot.Children);

            this._toolbox.BindTo(null, null);
        }



        private void EnableHierachyOnClick(List<UEditorWidgetBase> HierarchyList)
        {
            foreach (var item in HierarchyList)
            {
                UEditorWidgetFoldout __castFoldout = (UEditorWidgetFoldout)item;
                __castFoldout.OnClick += __newFoldout_OnClick;
                if (__castFoldout.Children.Count > 0)
                {
                    EnableHierachyOnClick(__castFoldout.Children);
                }
            }
        }


        protected override void DisableEvents()
        {
            base.DisableEvents();

            //Disable events here. For example:
            //_btnAddLabel.OnClick -= this._btnAddLabel_OnClick;

            //NOTE: This method is called by the UEditor Designer when loading a form in for editing.
            //Any activity required to make the form safe for editing including unsubscribing events and bindings should go here.

            __widgetDesignerRoot.OnClick -= __newFoldout_OnClick;
            //_btnGenCode.OnClick -= _btnGenCode_OnClick;
            //_btnTBA.OnClick -= _btnTBA_OnClick;
            _toolbox.OnClick -= _toolbox_OnClick;


            this._lblActiveContainter.BindTo(null, "");
        }

        private void SetupBindings()
        {
            //Setup widget bindings. For example.
            //_btnAddLabel.BindTo(this, this.Name);

            this._lblActiveContainter.BindTo(this, "ActiveContainerLabel");
        }

        #endregion

        //Rebind the active container label on a re-load
        public override void OnEnable()
        {

            this.EnableEvents();

            //Bind widget controls
            this.SetupBindings();
            if (this.Canvas != null)
            {
                this.BuildComponentHeriachy();
            }
        }


        #region Widget event handlers

        void __newFoldout_OnClick(IUEditorWidgetClickable sender, System.EventArgs e)
        {
            
            if (this._controlKeyPressed)
            {
                        UEditorWidgetBase __castSender = (UEditorWidgetBase)sender;

                        //TODO: if the current object is the root form bail out!!!!!

                        //Is the selected widget a container 
                        if (__castSender.BoundObject.GetType().IsSubclassOf(typeof(UEditorPanelBase)) || __castSender.BoundObject.GetType().IsSubclassOf(typeof(UEditorPanelBase)))
                        {
                            IWidgetContainer __newParent = (IWidgetContainer)__castSender.BoundObject;
                            UEditorWidgetBase __widgetToMove = (UEditorWidgetBase)this.SelectedFoldout.BoundObject;
                            IWidgetContainer __oldParent = ((UEditorWidgetBase)this.SelectedFoldout.BoundObject).parent;

                            __oldParent.RemoveChild(__widgetToMove);
                            __newParent.AddChild(__widgetToMove);
                        }
            }
         

            //Update the selected foldout
            //TODO: Consider wrapping some type saftey around this
            this.SelectedFoldout = (UEditorWidgetFoldout)sender;
        }

        void _toolbox_OnClick(IUEditorWidgetClickable sender, EventArgs e)
        {
            UEditorWidgetBase __newControl = UWidget.Create(Type.GetType(_toolbox.LastSelectedType));
            AddNewWidget(__newControl);
        }

        void _btnTBA_OnClick(IUEditorWidgetClickable sender, EventArgs e)
        {
            AddNewWidget(UWidget.Create<UControlDesignerTools>());
        }

        #endregion


        #region Functional code

        public override void OnGUI()
        {
            base.OnGUI();

            

            Event __unityEvent = Event.current;
            switch (__unityEvent.type)
            {
                case EventType.keyDown:
                    if (Event.current.keyCode == (KeyCode.LeftControl) || Event.current.keyCode == (KeyCode.RightControl))
                    {
                        this._controlKeyPressed = true;
                        Debug.Log("TRUE");
                    }
                    break;
                case EventType.keyUp:
                    if (Event.current.keyCode == (KeyCode.LeftControl) || Event.current.keyCode == (KeyCode.RightControl))
                    {
                        this._controlKeyPressed = false;
                        Debug.Log("FALSE");
                    
                    }
                    break;

            }
        }


        private void AddNewWidget(UEditorWidgetBase newControl)
        {

            //TODO: This functionality is already getting achieved by the onContainerChange() call, work out how to decom this
            //and still keep the select widget working.
            //Create a new hierarchy widget
            UEditorWidgetFoldout __newHierarchyWidget = UWidget.Create<UEditorWidgetFoldout>();
            __newHierarchyWidget.BindTo(newControl, "Name");
            __newHierarchyWidget.ObjectID = newControl.ObjectID;
            __newHierarchyWidget.LayoutMode = ePositioningLayout.Layout;
            //__newHierarchyWidget.OnClick += __newFoldout_OnClick;
            if (newControl.GetType().IsSubclassOf(typeof(UEditorPanelBase)) == false)
            {
                __newHierarchyWidget.BaseStyle = "label";
            }

            Canvas.CurrentContainer.AddChild(newControl);
            UEditorWidgetFoldout __containerInHierarchy = (UEditorWidgetFoldout)UWidget.FindWidgetById(this.HierarchyContainer, Canvas.CurrentContainer.ObjectID);
            __newHierarchyWidget.IndentLevel = __containerInHierarchy.IndentLevel + 1;
           

            //Select the new control
            this.__newFoldout_OnClick(__newHierarchyWidget, new System.EventArgs());

        }

        public void BuildComponentHeriachy()
        {
            this.Canvas.onContainerChange += Hierarchy_onContainerChange;
            BuildHericRecursive(__widgetDesignerRoot, this.Canvas.Children);
        }

        void Hierarchy_onContainerChange(IWidgetContainer sender)
        {
            //Find the widget in the hierarchy
            UEditorWidgetBase __hierarchyContainer = UWidget.FindWidgetById(this.HierarchyContainer, sender.ObjectID);
            if (__hierarchyContainer != null)
            {
                BuildHericRecursive((UEditorWidgetFoldout)__hierarchyContainer, sender.Children);
            }
        }


        
        public void BuildHericRecursive(UEditorWidgetFoldout ParentFoldout, List<UEditorWidgetBase> widgetList)
        {
            ParentFoldout.ClearChilden();

            foreach (var item in widgetList)
            {
                UEditorWidgetFoldout __newFoldout = UWidget.Create<UEditorWidgetFoldout>();
                __newFoldout.IndentLevel = ParentFoldout.IndentLevel + 1;
                __newFoldout.BindTo(item, "Name");
                __newFoldout.LayoutMode = ePositioningLayout.Layout;
                __newFoldout.OnClick += __newFoldout_OnClick;

                //Set the widget ID to match the one on the canvas. 
                //This little bit of hackery help us keep the desinger hierarchy and the toolbox layout in sync.
                __newFoldout.ObjectID = item.ObjectID;


                if (item.GetType().IsSubclassOf(typeof(UEditorPanelBase)))
                {
                    ParentFoldout.AddChild(__newFoldout);
                    UEditorPanelBase __castPanel = (UEditorPanelBase)item;

                    __castPanel.onContainerChange += Hierarchy_onContainerChange;

                    List<UEditorWidgetBase> __childList = new List<UEditorWidgetBase>();
                    foreach (var __childWidgets in __castPanel.Children)
                    {
                        if (item.GetType().IsSubclassOf(typeof(UEditorWidgetBase)))
                        {
                            UEditorWidgetBase __castChild = (UEditorWidgetBase)__childWidgets;
                            __childList.Add(__castChild);
                        }
                    }
                    BuildHericRecursive(__newFoldout, __childList);
                }
                else
                {
                    __newFoldout.BaseStyle = "label";
                    ParentFoldout.AddChild(__newFoldout);
                }
            }
        }

        #endregion

    }
}