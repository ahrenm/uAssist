namespace uAssist.Forms
{
    using System;
    using UnityEngine;
    using UnityEditor;
    using System.Collections;
    using System.Collections.Generic;
    using uAssist.UEditorWidgets;

    public class frmBase : EditorWindow, IWidgetContainer
    {

#region Form Options

        public string DesignerDropOverridePath = "";
        public string DesignerNameSpace = "uAssist.Forms";
        public bool InspectorUpdateRender = false;
        public bool CloseWindow = false;
        
        //Determine wether to include menu option creation in the build
        public bool AutoMenuOption = false;

        public string FormTitle
        {
            get
            {
                return this.title;
            }
            set
            {
                this.title = value;
            }
        }
       
        public float MaxHeight = -1;
        public float MaxWidth = -1;

        private bool _eventsEnabled = true;
        public bool EventsEnabled
        {
            get
            {
                return _eventsEnabled;
            }
            set
            {
                if (value = true && this._eventsEnabled == false)
                {
                    this.EnableEvents();
                }

                if (value == false && this._eventsEnabled == true)
                {
                    this.DisableEvents();
                }

                this._eventsEnabled = value;
            }

        }

#endregion

        [SerializeField]
        private List<UEditorWidgetBase> _children = new List<UEditorWidgetBase>();

        public virtual void InitalizeComponents() { }


        public virtual void OnEnable()
        {
            if (this.EventsEnabled)
            {
                this.EnableEvents();
            }
        }


        protected virtual void EnableEvents(){}
        protected virtual void DisableEvents(){}

        //Public constructor
        public frmBase() : base()
        {
            this.ObjectID = this.GetInstanceID();

            //Initalize all widget components
            this.InitalizeComponents();

            if (this.MaxHeight != -1 && this.MaxWidth != -1)
            {
                this.maxSize = new Vector2(MaxWidth, MaxHeight);
                this.minSize = new Vector2(MaxWidth, MaxHeight);
                this.position = new Rect(((Screen.width - this.MaxWidth) / 2), ((Screen.height - this.MaxHeight) / 2), this.MaxWidth, this.MaxHeight);
            }

        }

        public virtual void OnInspectorUpdate()
        {
            if (this.CloseWindow)
            {
                this.Close();
                return;
            }

            if (this.InspectorUpdateRender)
            {
                this.Repaint();
            }
        }

        public virtual void OnGUI()
        {
            if (this.CloseWindow)
            {
                this.Close();
                return;
            }

            this.RenderChildren();
        }

#region IWidgetContainerInterface

        public int ObjectID
        {
            get;
            set;
        }

        public virtual void AddChild(UEditorWidgetBase addChild, bool bSilent = false)
        {
            addChild.parent = this;
            this._children.Add(addChild);
            if (bSilent == false)
            {
                this.Raise_onContainerChange();
            }
        }

        public void RemoveChild(UEditorWidgetBase removeChild, bool bSilent = false)
        {
            this._children.Remove(removeChild);
            if (bSilent == false)
            {
                this.Raise_onContainerChange();
            }
        }

        public List<UEditorWidgetBase> Children
        {
            get
            {
                return this._children;
            }
            set
            {
                this._children = value;
            }
        }

        public void RenderChildren()
        {
            foreach (UEditorWidgetBase __widget in _children)
            {
                if (__widget != null)
                {
                    __widget.Render();
                }
            }
        }

        public void Raise_onContainerChange()
        {
            if (this.onContainerChange != null)
            {
                this.onContainerChange(this);
            }
        }

        public event ContainerChanged onContainerChange;
    }

#endregion
}