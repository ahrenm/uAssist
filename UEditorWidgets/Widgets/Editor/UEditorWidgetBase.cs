    namespace uAssist.UEditorWidgets
{
    using System;
    using UnityEngine;
    using System.Collections.Generic;
    using System.Reflection;
    using UnityEditor;

    /// <summary>
    /// The base class for all widgets
    /// This class should really be abstract so don't instantiate directly.
    /// </summary>
    public class UEditorWidgetBase : UWidget, IEquatable<UEditorWidgetBase>
    {

        public bool Equals(UEditorWidgetBase other)
        {
            if (other == null)
            {
                return false;
            }
            if (this.ObjectID == other.ObjectID)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        [UWidgetProperty(ListOptions = new string[2] { "private", "public" }, Label = "Widget Scope")]
        public string WidgetScope = "private";

        
#region Style Properties

        protected GUIStyle Style;
        private string _cachedBaseStyle = "";

        /// <summary>
        /// The base GUIStyle for the widget
        /// </summary>
        [UWidgetPropertyAttribute(ListOptions = new string[5]{" ","label","textField","button","toolbar"}, Label="Base GUIStyle")]
        public string BaseStyle
        {
            get 
            {
                return this._cachedBaseStyle;
            }
            set
            {
                //This is a nasty hack to allow for the Designer dropdown to have an option consisting of a single space, which need to equate to string.empty
                string __value;
                if (value == " ")
                {
                    __value = "";
                }
                else
                {
                    __value = value;
                }
                if (_cachedBaseStyle != __value)
                {
                    _cachedBaseStyle = __value;
                    this._StyleIsDirty = true;
                }
            }
        }

        private TextClipping _cachedClipping = TextClipping.Overflow;

        [UWidgetPropertyAttribute]
        public TextClipping Clipping
        {
            get
            {
                return this._cachedClipping;
            }
            set
            {
                if (_cachedClipping != value)
                {
                    _cachedClipping = value;
                    this._StyleIsDirty = true;
                }

            }

        }

        protected bool _StyleIsDirty = true;
        
        /// <summary>
        /// Calculates if a style rebuild is required
        /// </summary>
        public bool StyleIsDirty
        {
            get 
            {
                if (this._StyleIsDirty)
                    return true;
                else
                    if (this._cachedMargin.IsStyleDirty)
                        return true;
                    else
                        if (this._cachedPadding.IsStyleDirty)
                            return true;
                        else
                            if (this._cachedBorder.IsStyleDirty)
                                return true;
                            else
                                return false;
            }
            set
            {
                if (value  == true)
                {
                    this._StyleIsDirty = true;
                }
                else
                {
                    //A clear flag at the widget level should flush down to RectOffestSeralizeable
                    _StyleIsDirty = false;
                    this._cachedMargin.IsStyleDirty = false;
                    this._cachedBorder.IsStyleDirty = false;
                    this._cachedPadding.IsStyleDirty = false;
                }
            }
        }

        /// <summary>
        /// Rebuild the GUIStyle object for the widget.
        /// Can be overridden in derived class to add additional GUIStyle functionality
        /// Overidden methods should call down to base:ReBuildGUIStyle() before their work
        /// </summary>
        protected virtual void ReBuildGUIStyle()
        {
                if (this._cachedBaseStyle != "")
                    this.Style = new GUIStyle(GUI.skin.FindStyle(_cachedBaseStyle));
                else
                {
                    switch (_widgetType)
                    {
                        case eWidgetType.NotSet:
                            break;
                        case eWidgetType.Generic:
                            this.Style = new GUIStyle();
                            break;
                        case eWidgetType.Toggle:
                            this.Style = new GUIStyle(GUI.skin.toggle);
                            break;
                        case eWidgetType.Label:
                            this.Style = new GUIStyle(GUI.skin.label);
                            break;
                        case eWidgetType.TextField:
                            this.Style = new GUIStyle(GUI.skin.textField);
                            break;
                        case eWidgetType.TextArea:
                            this.Style = new GUIStyle(GUI.skin.textArea);
                            break;
                        case eWidgetType.Button:
                            this.Style = new GUIStyle(GUI.skin.button);
                            break;
                        case eWidgetType.Foldout:
                            this.Style = new GUIStyle(GUI.skin.FindStyle("Foldout"));
                            break;
                        case eWidgetType.PanelArea:
                            this.Style = new GUIStyle();
                            break;
                        case eWidgetType.PanelVertical:
                            this.Style = new GUIStyle();
                            break;
                        case eWidgetType.PanelHorizontal:
                            this.Style = new GUIStyle();
                            break;
                        case eWidgetType.PanelScroll:
                            this.Style = new GUIStyle();
                            break;
                        case eWidgetType.PanelToggleArea:
                            this.Style = new GUIStyle();
                            break;
                        default:
                            break;
                    }
                }

                this.Style.margin = this._cachedMargin.ToRectOffset();
                this.Style.border = this._cachedBorder.ToRectOffset();
                this.Style.padding = this._cachedPadding.ToRectOffset();

                if (_cachedHeight != -1)
                    this.Style.fixedHeight = this._cachedHeight;

                if (_cachedWidth != -1)
                    this.Style.fixedWidth = this._cachedWidth;

                this.Style.clipping = this._cachedClipping;

        }

        [SerializeField]
        private RectOffsetSeralizable _cachedMargin = new RectOffsetSeralizable();
        [UWidgetPropertyAttribute(CustomEditor = typeof (UEditorControlRect),PropCodeGen=typeof(CGen_RectOffsetSeralizable))]
        public RectOffsetSeralizable Margin
        {
            get
            {
                return this._cachedMargin;
            }
            set
            {
                this._cachedMargin = value;
                this._cachedMargin.IsStyleDirty = true;
            }
        }

        [SerializeField]
        private RectOffsetSeralizable _cachedPadding = new RectOffsetSeralizable();
        [UWidgetPropertyAttribute(CustomEditor = typeof(UEditorControlRect), PropCodeGen = typeof(CGen_RectOffsetSeralizable))]
        public RectOffsetSeralizable Padding
        {
            get 
            {
                return _cachedPadding;
            }
            set
            {
                _cachedPadding = value;
                _cachedPadding.IsStyleDirty = true;
            }
        }

        [SerializeField]
        private RectOffsetSeralizable _cachedBorder = new RectOffsetSeralizable();
        [UWidgetPropertyAttribute(CustomEditor = typeof(UEditorControlRect), PropCodeGen = typeof(CGen_RectOffsetSeralizable))]
        public RectOffsetSeralizable Border
        {
            get
            {
                return _cachedBorder;
            }
            set
            {
                this._cachedBorder = value;
                _cachedBorder.IsStyleDirty = true;
            }
        }

        private float _cachedHeight = -1;
        [UWidgetPropertyAttribute]
        public float Height
        {
            get { return this._cachedHeight; }
            set 
            {
                if (_cachedHeight != value)
                {
                    this._StyleIsDirty = true;
                    this._cachedHeight = value;
                }
            }
        }

        private float _cachedWidth = -1;
        [UWidgetPropertyAttribute]
        public virtual float Width
        {
            get { return this._cachedWidth; }
            set 
            {
                if (_cachedWidth != value)
                {
                    this._StyleIsDirty = true;
                    this._cachedWidth = value;
                }
            }
        }

        [UWidgetProperty ("Position X")]
        public float PositionX = 1;

        [UWidgetProperty("Position Y")]
        public float PositionY = 1;

#endregion

#region Widget properties

        [UWidgetPropertyAttribute("GUI Enable")]
        public bool GUIEnabled = true;
        //A flag used to determine if this particular widge set GUI.enabled = false and then re-enable it.
        [SerializeField]
        private bool _resetEnableGUI = false;


        [UWidgetPropertyAttribute]
        protected eWidgetType _widgetType;
        public eWidgetType WidgetType
        {
            get { return _widgetType; }
        }

        [UWidgetProperty("Render Widget")]
        public bool WidgetShouldRender = true;

        protected float RenderOffsetX = 0;
        protected float RenderOffsetY = 0;

        [UWidgetPropertyAttribute("Expand Height")]
        public bool LayoutExpandHeight = false;

        [UWidgetPropertyAttribute("Expand Width")]
        public bool LayoutExpandWidth = false;

        [UWidgetPropertyAttribute("Layout Mode")]
        public ePositioningLayout LayoutMode = ePositioningLayout.WindowSpace;

#endregion

        //Constructor
        public UEditorWidgetBase(eWidgetType type)
            : base()
        {
            _widgetType = type;
        }  

        public Rect RenderRect
        {
            get
            {
                if (this.LayoutMode == ePositioningLayout.Layout)
                {
                    return EditorGUILayout.GetControlRect(GUILayout.Width(this.Width), GUILayout.Height(this.Height));
                }
                else
                {
                    return new Rect(this.RenderOffsetX, RenderOffsetY, this.Width, this.Height);
                }
                //return new Rect(this.RenderOffsetX, RenderOffsetY, this.Width, this.Height);
            }
        }

        protected Rect LastLayoutRect = new Rect();

        public Vector2 GlobalPosition()
        {
            if (this.parent == null)
            {
                if (this.LayoutMode == ePositioningLayout.Layout)
                {
                    return new Vector2(this.LastLayoutRect.x, this.LastLayoutRect.y);
                }
                else
                {
                    return new Vector2(this.PositionX, this.PositionY);
                }
            }
            else
            {
                if (this.LayoutMode == ePositioningLayout.Layout)
                {
                    return ((UEditorWidgetBase)parent).GlobalPosition() + new Vector2(this.LastLayoutRect.x, this.LastLayoutRect.y);
                }
                else
                {
                    return new Vector2(this.PositionX, this.PositionY);
                }
            }
        }




        public void Render()
        {
            //Exit out if the render is not required
            if (this.WidgetShouldRender == false)
            {
                return;
            }

            //Rebuild the style if required
            if (this.StyleIsDirty)
            {
                //Reset the dirty flag
                this.StyleIsDirty = false;

                ReBuildGUIStyle();
            }

            if (this.LayoutMode == ePositioningLayout.WindowSpace)
            {
                this.RenderOffsetX = PositionX;
                this.RenderOffsetY = PositionY;
            }
            if (this.LayoutMode == ePositioningLayout.RelativeToParent)
            {
                Vector2 __position = this.GlobalPosition();
                this.RenderOffsetX = __position.x;
                this.RenderOffsetY = __position.y;
            }
            
            if (GUI.enabled && this.GUIEnabled == false)
            {
                GUI.enabled = false;
                this._resetEnableGUI = true;
            }

            //TODO: This is probably a little deep in the widget stack for this call
            if (GUI.enabled && this.BoundMemberCanWrite != true)
            {
                GUI.enabled = false;
                this._resetEnableGUI = true;
            }

            //Pre-render is complete, now render the control
            this.WidgetRender();


            //Post render
            
            if (_resetEnableGUI)
            {
                GUI.enabled = true;
                this._resetEnableGUI = false;
            }
        }

        /// <summary>
        /// The actual render function for an individual widget
        /// This should be overriden by dervice class to provide rendering capabily
        /// </summary>
        protected virtual void WidgetRender(){}

    }
}
