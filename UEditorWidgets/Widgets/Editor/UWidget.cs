namespace uAssist.UEditorWidgets
{
    using System;
    using UnityEditor;
    using UnityEngine;
    using System.Linq;
    using System.Reflection;
    using System.Collections;
    using System.Collections.Generic;
    using System.Text.RegularExpressions;
    using uAssist.Forms;

    /// <summary>
    /// Assigned to widget properties to make them editable to the designer
    /// </summary>
    [Serializable]
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false, Inherited = true)]
    public class UWidgetPropertyAttribute : Attribute
    {
        public UWidgetPropertyAttribute(string Label = "")
        {
            this.Label = Label;
        }

        public string[] ListOptions;
        public bool HideInProperties = false;
        public Type CustomEditor = null;
        public string Label = "";
        public Type PropCodeGen;
        public bool BuildProperty = true;
    }

    [AttributeUsage(AttributeTargets.Class,AllowMultiple=false,Inherited=true)]
    public class UWidgetWidgetAttribute :Attribute
    {
        public UWidgetWidgetAttribute (eUWidgetDesignerCategory Catogery, string Label)
        {
            this.DesignerCatogery = Catogery;
            this.DesignerLabel = Label;
        }

        public eUWidgetDesignerCategory DesignerCatogery = eUWidgetDesignerCategory.NotSet;
        public string DesignerLabel = "";
    }

    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false,Inherited = true)]
    public class UWidgetFormAttribute : Attribute
    {
        public bool CanEditInDesigner = true;
        public string Title = string.Empty;
    }

    public enum eUWidgetDesignerCategory
    {
        NotSet = 0,
        Panels = 1,
        Widgets = 2,
        Controls = 3,
        Decorators = 4,
        Other = 5
    }

    /// <summary>
    /// A garbage collector for expired widgets
    /// The GC runs at AppDomain reload. Each .NET widget instance is valid only for the AppDomain cycle it was created in.
    /// At 'Uwidget.Create()' & at GC time, we set widget instances to expire at the NEXT GC run
    /// </summary>
    [InitializeOnLoad]
    public static class UWidgetGC
    {
        [InitializeOnLoadMethod]
        public static void RunGC()
        {
            int __widgetsDestroyed = 0;

            var __widgetsInMemory = Resources.FindObjectsOfTypeAll<UEditorWidgetBase>().ToList<UEditorWidgetBase>();

            foreach (var __searchWidget in __widgetsInMemory)
            {
                if (__searchWidget.RemoveAtGC)
                {
                    ScriptableObject.DestroyImmediate(__searchWidget);
                    __widgetsDestroyed++;
                }
                else
                {
                    __searchWidget.RemoveAtGC = true;
                }
            }
            Debug.Log("Widgets garbage collected->" + __widgetsDestroyed.ToString());
        }
    }

    /// <summary>
    /// Widget clickable scaffolding
    /// </summary>
    public interface IUEditorWidgetClickable
    {
        event UEditorWidget_OnClick OnClick;
    }
    public delegate void UEditorWidget_OnClick(IUEditorWidgetClickable sender, EventArgs e);

    public delegate void UEditorWidget_MenuItemSelected(UEditorWidgetBase sender, string SelectedMenuItem);
    public delegate void UEditorWidget_MenuItemChanged(UEditorWidgetBase sender, int indexUpdated);

    //TODO: Get rid of this entirely
    public enum eWidgetType
    {
        NotSet = 0,
        Generic = 1,
        Toggle = 2,
        Label = 3,
        TextField = 4,
        TextArea = 5,
        Button = 6,
        Foldout = 7,
        PanelArea = 20,
        PanelVertical = 21,
        PanelHorizontal = 22,
        PanelScroll = 23,
        PanelToggleArea = 24
    }

    public enum ePositioningLayout
    {
        NotSet = 0,
        WindowSpace = 1,
        RelativeToParent = 2,
        Layout = 3
    }

    public interface IWidgetContainer
    {
        void AddChild (UEditorWidgetBase addChild, bool bSilent = false);
        void RemoveChild(UEditorWidgetBase removeChild, bool bSilent = false);
        List<UEditorWidgetBase> Children { get; set; }
        void RenderChildren();
        event ContainerChanged onContainerChange;
        int ObjectID { get; set; }
        void Raise_onContainerChange();
    }
    public delegate void ContainerChanged(IWidgetContainer sender);


    public interface ICodeGenerator
    {
        string CGenInitalizer(); //TODO: remove if not implemented
        List<string> CGenPropertySetters(string PropFQN, object PropValue);
    }

    //My thanks to Unity for making RectOffset both sealed and not marked as seralizable
    //So we do this junk.
    [Serializable]
    public class RectOffsetSeralizable
    {
        private int _top, _bottom, _left, _right;

        public int top
        {
            get
            {
                return this._top;
            }
            set
            {
                if (_top != value)
                {
                    this.IsStyleDirty = true;
                }
                this._top = value;
            }
        }
        public int left
        {
            get
            {
                return this._left;
            }
            set
            {
                if (_left != value)
                {
                    this.IsStyleDirty = true;
                }
                this._left = value;
            }
        }
        public int right
        {
            get
            {
                return this._right;
            }
            set
            {
                if (_right != value)
                {
                    this.IsStyleDirty = true;
                }
                this._right = value;
            }
        }
        public int bottom
        {
            get
            {
                return this._bottom;
            }
            set
            {
                if (_bottom != value)
                {
                    this.IsStyleDirty = true;
                }
                this._bottom = value;
            }
        }

        public RectOffset ToRectOffset()
        {
            return new RectOffset(this.left, this.right, this.top, this.bottom);
        }

        public bool IsStyleDirty = false;

        public void FromRectOffset(RectOffset rectOffset)
        {
            this.top = rectOffset.top;
            this.bottom = rectOffset.bottom;
            this.left = rectOffset.left;
            this.right = rectOffset.right;

            this.IsStyleDirty = true;
        }

        public void FromInt(int Left, int Right, int Top, int Bottom)
        {
            this.top = Top;
            this.bottom = Bottom;
            this.left = Left;
            this.right = Right;

            this.IsStyleDirty = true;
        }
    }


    //Main Widget base class
    //=====================

    /// <summary>
    /// The root of the widget hierarchy.
    /// This class should really be abstract but Unity serilazation doesn't handle abstract bases in generic lists
    /// </summary>
    public class UWidget : ScriptableObject
    {

#region Static Methods

        /// <summary>
        /// A flag to tell Unity if created widget should be flagged for seralization.
        /// Is enabled by default. Disable to improve editor performance during design work.
        /// </summary>
        public static bool SeralizationEnabled = true;

        public static T Create<T>(string Name = "", bool bSuppressBindingWarnings = false) where T : UEditorWidgetBase
        {
            return (T)UWidget.Create(typeof(T), Name,bSuppressBindingWarnings);
        }

        /// <summary>
        /// This is the main function used to create widgets
        /// </summary>
        /// <param name="WidgetType"></param>
        /// <param name="Name"></param>
        /// <param name="bSuppressBindingWarnings"></param>
        /// <returns></returns>
        public static UEditorWidgetBase Create(Type WidgetType, string Name = "", bool bSuppressBindingWarnings = false)
        {
            object __retObject = ScriptableObject.CreateInstance(WidgetType);
            UEditorWidgetBase __castWidget = (UEditorWidgetBase)__retObject;
            if (UWidget.SeralizationEnabled)
            {
                __castWidget.hideFlags = HideFlags.HideAndDontSave;
            }
            if (Name != "")
            {
                __castWidget.Name = Name;
            }
            __castWidget.SuppressBindingWarnings = bSuppressBindingWarnings;
            __castWidget.RemoveAtGC = true;
            return __castWidget;
        }

        /// <summary>
        /// Used to 'flatten' a string into a code friendly identifier
        /// </summary>
        /// <param name="stringToFlatten"></param>
        /// <returns></returns>
        public static string FlattenString(string stringToFlatten)
        {
            return Regex.Replace(stringToFlatten, "[^a-zA-Z0-9%_]", string.Empty);
        }

        //public static UEditorWidgetBase FindWidgetById(List<UEditorWidgetBase> RenderableWidgets, int WidgetID)
        public static UEditorWidgetBase FindWidgetById(IWidgetContainer Container, int WidgetID)
        {
            UEditorWidgetBase __retValue = null;

            //If the container itself is the widget we are searching for
            if (Container.ObjectID == WidgetID)
            {
                if (Container.GetType().IsSubclassOf(typeof (UEditorWidgetBase)))
                {
                    return (UEditorWidgetBase)Container;
                }
                else
                {
                    throw new Exception("FindWidgetById is unable to cast to " + Container.GetType().Name);
                }
            }

            //Loop through all children in the container
            foreach (var __widget in Container.Children)
            {
                
                if (__widget.ObjectID == WidgetID)
                {
                    return (UEditorWidgetBase)__widget;
                }

                //TODO: Update this to loop on the IWidgetContainer interface
                if (__widget.GetType().IsSubclassOf(typeof(UEditorPanelBase)))
                {
                    UEditorWidgetBase __subSearchResult = UWidget.FindWidgetById((IWidgetContainer)__widget, WidgetID);
                    if (__subSearchResult != null)
                    {
                        return __subSearchResult;
                    }
                }
            }
            return __retValue;
        }


        //TODO: Find a new home for this function
        public static bool TryGetEnumOptions<T>(out List<string> _enumOptions)
        {
            try
            {
                _enumOptions = Enum.GetNames(typeof(T)).ToList<string>();
            }
            catch
            {
                _enumOptions = new List<string>();
                return false;
            }

            return true;
        }

#endregion

        //The unique ideintifier for thie widget
        public int ObjectID
        {
            get;
            set;
        }

        //Public constructor
        public UWidget() :base()
        {
            ObjectID = this.GetInstanceID();
        }

        //See class UWidgetGC for details  
        public bool RemoveAtGC = false;

        private string _name = "WidgetControl";
        [UWidgetPropertyAttribute]
        public string Name
        {
            get
            {
                return _name;
            }
            set
            {
                if (this._name != value)
                {
                    //Perform a regex to remove likely candidates for breaking compilation of this Name
                    _name = Regex.Replace(value, "[^a-zA-Z0-9%_]", string.Empty);
                }
                _name = value;
            }
        }

#region Binding controls

        //Binding
        [SerializeField]
        private PropertyInfo _boundPropertyInfo = null;
        [SerializeField]
        private FieldInfo _boundFieldInfo = null;
        [SerializeField]
        private MethodInfo _boundMethodInfo = null;
        [SerializeField]
        private object _boundMemberObject = null;
        [SerializeField]
        private string _boundMemberName = "";
        [SerializeField]
        private bool _boundMemberCanWrite = true;
        [SerializeField]
        private int _boundGameObjectID = -1;

        public bool SuppressBindingWarnings = false;

        protected enum eBindingType
        {
            NotSet = 0,
            Property = 1,
            Field = 2,
            Method = 3,
            This = 4
        }

        [SerializeField]
        protected eBindingType BindingType;

        //Access to binding internals for read only queries
        public object BoundObject
        {
            get
            {
                if (this._boundMemberObject == null && this._boundGameObjectID != -1)
                {
                    this.BindTo(EditorUtility.InstanceIDToObject(this._boundGameObjectID), this._boundMemberName);
                }
                return _boundMemberObject;
            }
        }

        public string BoundMemberName
        {
            get
            {
                return _boundMemberName;
            }
        }

        public bool BoundMemberCanWrite
        {
            get
            {
                return this._boundMemberCanWrite;
            }
        }

        [SerializeField]
        private object _localBoundMember;


        public T GetBoundValue<T>()
        {

            object __refObject;

            if (this._boundMemberObject == null && this._boundGameObjectID != -1)
            {
                this.BindTo(EditorUtility.InstanceIDToObject(this._boundGameObjectID), this._boundMemberName);
            }

            switch (this.BindingType)
            {
                case eBindingType.NotSet:
                    //If binding is not set, create a local var to store the value
                    if (this._localBoundMember == null)
                    {
                        //String don't take a parameterless constructor
                        if (typeof(T) == typeof(string))
                        {
                            this._localBoundMember = string.Empty;
                        }
                        else
                        {
                            //Create the object
                            this._localBoundMember = (T)Activator.CreateInstance<T>();
                        }

                        //Null strings are bad so lets sort that out.
                        if (this._localBoundMember.GetType() == typeof(string))
                        {
                            this._localBoundMember = string.Empty;
                        }
                    }
                    return (T)_localBoundMember;
                case eBindingType.Property:
                    __refObject = this._boundPropertyInfo.GetValue(this._boundMemberObject, null);
                    break;
                case eBindingType.Field:
                    __refObject = this._boundFieldInfo.GetValue(this._boundMemberObject);
                    break;
                case eBindingType.This:
                    __refObject = this._boundMemberObject;
                    break;
                default:
                    throw new Exception("Unexpected binding type in UEditorWidget.GetBoundValue<T>");
            }

            if (__refObject == null)
            {
                Debug.Log("Break");
            }

            if (__refObject.GetType() == typeof(T) || __refObject.GetType().IsSubclassOf(typeof(T)))
            {

                return (T)__refObject;
            }
            else
            {
                if (typeof(T) == typeof(string))
                {
                    return (T)Convert.ChangeType(__refObject, typeof(T));
                }
                //TODO: Improve this error message
                throw new Exception("Failed implicit cast in UEditorWidget.GetBoundValue<T>");
            }
        }

        public void SetBoundValue(object newValue)
        {
            //If the property is flagged readonly then bail out
            if (this._boundMemberCanWrite == false)
            {
                return;
            }

            //This will fire the first time we set a localy bound piece of data.
            if (this.BindingType == eBindingType.NotSet && this._localBoundMember == null)
            {
                this._localBoundMember = newValue;
                return;
            }

            //Check the type is correct
            if (this.GetBoundValueType() != newValue.GetType())
            {
                //Attempt a Q&N conversion
                object __convertedValue = Convert.ChangeType(newValue, this.GetBoundValueType());

                if (__convertedValue == null)
                {
                    throw new Exception("Input value type does not equal bound member type in UEditorWidget.SetBoundValue");
                }

                //Assume the conversion was succsssful
                newValue = __convertedValue;
            }

            switch (this.BindingType)
            {
                case eBindingType.NotSet:
                    //Update the local var
                    this._localBoundMember = newValue;
                    break;
                case eBindingType.Property:
                    this._boundPropertyInfo.SetValue(this._boundMemberObject, newValue, null);
                    break;
                case eBindingType.Field:
                    this._boundFieldInfo.SetValue(this._boundMemberObject, newValue);
                    break;
                case eBindingType.This:
                    this._boundMemberObject = newValue;
                    break;
                default:
                    throw new Exception("Unexpected binding type in UEditorWidget.SetBoundValue<T>");
            }

        }

        public Type GetBoundValueType()
        {
            switch (this.BindingType)
            {
                case eBindingType.NotSet:
                    if (this._localBoundMember == null)
                    {
                        return null;
                    }
                    else
                    {
                        return this._localBoundMember.GetType();
                    }
                case eBindingType.Property:
                    return this._boundPropertyInfo.GetValue(this._boundMemberObject, null).GetType();
                case eBindingType.Field:
                    return this._boundFieldInfo.GetValue(this._boundMemberObject).GetType();
                case eBindingType.This:
                    if (this._boundMemberObject != null)
                    {
                        return this._boundMemberObject.GetType();
                    }
                    else
                    {
                        return null;
                    }
                default:
                    throw new Exception("Unexpected binding type in UEditorWidget.GetBoundValueType<T>");
            }
        }


        /// <summary>
        /// Binds a member of an object variable into a widget
        /// </summary>
        /// <param name="Object">An object variable to be referenced</param>
        /// <param name="MemberName">The member (property/field) to bind to</param>
        /// <returns></returns>
        public virtual bool BindTo(object Object, string MemberName)
        {
            if (Object == null)
            {
                this.BindingType = eBindingType.NotSet;
                return true;
            }

            if (MemberName == null || MemberName == "")
                return false;

            if (Object.GetType().IsSubclassOf((typeof(UnityEngine.Object))) == true)
            {
                this._boundGameObjectID = ((UnityEngine.Object)Object).GetInstanceID();
            }
            else
            {
                if (this.SuppressBindingWarnings == false)
                {
                    Debug.LogWarning("Bound member " + MemberName + " in Widget " + this.Name + " does not derive from UnityEngine.GameObject.\r\bThis bind may fail seralization");
                }
            }

            _boundMemberName = MemberName;
            _boundMemberObject = Object;

            if (MemberName == "this")
            {
                this.BindingType = eBindingType.This;
                return true;
            }

            _boundPropertyInfo = Object.GetType().GetProperty(_boundMemberName);
            if (_boundPropertyInfo != null)
            {
                this.BindingType = eBindingType.Property;
                if (_boundPropertyInfo.CanWrite != true)
                {
                    this._boundMemberCanWrite = false;
                }
                _localBoundMember = _boundPropertyInfo.GetValue(_boundMemberObject, null);
            }
            else
            {
                _boundFieldInfo = Object.GetType().GetField(_boundMemberName);
                if (_boundFieldInfo != null)
                {
                    this.BindingType = eBindingType.Field;
                    _localBoundMember = _boundFieldInfo.GetValue(_boundMemberObject);
                }
                else
                {
                    _boundMethodInfo = Object.GetType().GetMethod(_boundMemberName);
                    if (_boundMethodInfo != null)
                    {
                        this.BindingType = eBindingType.Method;
                    }
                    else
                    {
                        throw new System.Exception("Unable to derterming property type on " + Object.ToString() + ":" + _boundMemberName);
                    }
                }
            }
            return true;
        }

#endregion

        [SerializeField]
        private IWidgetContainer _parent;
        [SerializeField]
        private int _parentObjectID = -1;

        /// <summary>
        /// A reference to the parent container.
        /// Unity hates seralizing interfaces, more than it hates polymorphic objects, so we do some nasty in here
        /// </summary>
        public IWidgetContainer parent
        {
            get
            {
                if (_parent == null && _parentObjectID != -1)
                {
                    _parent = (IWidgetContainer)EditorUtility.InstanceIDToObject(_parentObjectID);
                }
                return _parent;
            }
            set 
            {
                _parent = value;
                _parentObjectID = ((UnityEngine.Object)_parent).GetInstanceID();
            }
        }

        public override int GetHashCode()
        {
            return Convert.ToInt32(this.ObjectID);
        }

    }
}