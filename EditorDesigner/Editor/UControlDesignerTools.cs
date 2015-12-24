
namespace uAssist.EditorDesigner
{
    using System;
    using System.Reflection;
    using UnityEngine;
    using UnityEditor;
    using System.Collections;
    using System.Collections.Generic;
    using uAssist.UEditorWidgets;

    public partial class UControlDesignerTools : UEditorWidgetBase, IUEditorWidgetClickable
    {

        public event UEditorWidget_OnClick OnClick;

#region Local vars and helpers

        [Serializable]
        public class widgetDef: IConvertible
        {
            public string Label;
            public string WidgetFQName;

            public override string ToString()
            {
                return this.Label;
            }

            public TypeCode GetTypeCode()
            {
                throw new NotImplementedException();
            }

            public bool ToBoolean(IFormatProvider provider)
            {
                throw new NotImplementedException();
            }

            public byte ToByte(IFormatProvider provider)
            {
                throw new NotImplementedException();
            }

            public char ToChar(IFormatProvider provider)
            {
                throw new NotImplementedException();
            }

            public DateTime ToDateTime(IFormatProvider provider)
            {
                throw new NotImplementedException();
            }

            public decimal ToDecimal(IFormatProvider provider)
            {
                throw new NotImplementedException();
            }

            public double ToDouble(IFormatProvider provider)
            {
                throw new NotImplementedException();
            }

            public short ToInt16(IFormatProvider provider)
            {
                throw new NotImplementedException();
            }

            public int ToInt32(IFormatProvider provider)
            {
                throw new NotImplementedException();
            }

            public long ToInt64(IFormatProvider provider)
            {
                throw new NotImplementedException();
            }

            public sbyte ToSByte(IFormatProvider provider)
            {
                throw new NotImplementedException();
            }

            public float ToSingle(IFormatProvider provider)
            {
                throw new NotImplementedException();
            }

            public string ToString(IFormatProvider provider)
            {
                return this.Label;
            }

            public object ToType(Type conversionType, IFormatProvider provider)
            {
                throw new NotImplementedException();
            }

            public ushort ToUInt16(IFormatProvider provider)
            {
                throw new NotImplementedException();
            }

            public uint ToUInt32(IFormatProvider provider)
            {
                throw new NotImplementedException();
            }

            public ulong ToUInt64(IFormatProvider provider)
            {
                throw new NotImplementedException();
            }
        }

        [SerializeField]
        private List<widgetDef> _dataPanels = new List<widgetDef>();
        [SerializeField]
        private List<widgetDef> _dataWidgets = new List<widgetDef>();
        [SerializeField]
        private List<widgetDef> _dataContols = new List<widgetDef>();
        [SerializeField]
        private List<widgetDef> _dataDecorators = new List<widgetDef>();
        [SerializeField]
        private List<widgetDef> _dataOthers = new List<widgetDef>();

        private string _lastSelectedType;
        public string LastSelectedType
        {
            get
            {
                return _lastSelectedType;
            }
        }

        [SerializeField]
        private UEditorPanelVertical _activeGroup;
        public UEditorPanelVertical ActiveGroup
        {
            get
            {
                return _activeGroup;
            }
            set
            {
                if (this._activeGroup != null)
                {
                    _activeGroup.WidgetShouldRender = false;
                }
                _activeGroup = value;
                _activeGroup.WidgetShouldRender = true;
            }
        }

#endregion


#region UEditor plumbing

        //Public form constructor
        public UControlDesignerTools() : base(eWidgetType.Generic)
        {
            //Initalize all widget components
            this.InitalizeComponents();

        }

        public override bool BindTo(object Object, string MemberName)
        {
            //return base.BindTo(Object, MemberName);

            this._dataWidgets.Clear();
            this._vertWidgets.ClearChilden();
            this._dataPanels.Clear();
            this._vertPanels.ClearChilden();
            this._dataContols.Clear();
            this._vertControls.ClearChilden();
            this._dataDecorators.Clear();
            this._vertDecorators.ClearChilden();
            this._dataOthers.Clear();
            this._vertOthers.ClearChilden();

            //Go searching for attributes 
            foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                foreach (Type type in assembly.GetTypes())
                {
                    object[] __foundAttributes = type.GetCustomAttributes(typeof(UWidgetWidgetAttribute), false);
                    if (__foundAttributes != null && __foundAttributes.Length == 1)
                    {
                        UWidgetWidgetAttribute __attribute = (UWidgetWidgetAttribute)__foundAttributes[0];

                        switch (__attribute.DesignerCatogery)
                        {
                            case eUWidgetDesignerCategory.NotSet:
                                break;
                            case eUWidgetDesignerCategory.Panels:
                                this._dataPanels.Add(new widgetDef() { WidgetFQName = type.AssemblyQualifiedName, Label = __attribute.DesignerLabel });
                                break;
                            case eUWidgetDesignerCategory.Widgets:
                                this._dataWidgets.Add(new widgetDef() { WidgetFQName = type.AssemblyQualifiedName, Label = __attribute.DesignerLabel });
                                break;
                            case eUWidgetDesignerCategory.Controls:
                                this._dataContols.Add(new widgetDef() { WidgetFQName = type.AssemblyQualifiedName, Label = __attribute.DesignerLabel });
                                break;
                            case eUWidgetDesignerCategory.Decorators:
                                this._dataDecorators.Add(new widgetDef() { WidgetFQName = type.AssemblyQualifiedName, Label = __attribute.DesignerLabel });
                                break;
                            case eUWidgetDesignerCategory.Other:
                                this._dataOthers.Add(new widgetDef() { WidgetFQName = type.AssemblyQualifiedName, Label = __attribute.DesignerLabel });
                                break;
                            default:
                                break;
                        }
                       
                    }
                }
            }

            //Now we have the widgets found, build out the menu

            foreach (var widget in _dataWidgets)
            {
                UEditorWidgetButton __newButton = this.BuildButton(widget.Label);
                __newButton.BindTo(widget, "this");
                _vertWidgets.AddChild(__newButton);
            }
            foreach (var widget in _dataContols)
            {
                UEditorWidgetButton __newButton = this.BuildButton(widget.Label);
                __newButton.BindTo(widget, "this");
                _vertControls.AddChild(__newButton);
            }
            foreach (var widget in _dataDecorators)
            {
                UEditorWidgetButton __newButton = this.BuildButton(widget.Label);
                __newButton.BindTo(widget, "this");
                _vertDecorators.AddChild(__newButton);
            }
            foreach (var widget in _dataPanels)
            {
                UEditorWidgetButton __newButton = this.BuildButton(widget.Label);
                __newButton.BindTo(widget, "this");
                _vertPanels.AddChild(__newButton);
            }
            foreach (var widget in _dataOthers)
            {
                UEditorWidgetButton __newButton = this.BuildButton(widget.Label);
                __newButton.BindTo(widget, "this");
                _vertOthers.AddChild(__newButton);
            }

            //Bind group buttons;
            this._btnControls.OnClick += _btnControls_OnClick;
            this._btnDecorators.OnClick += _btnDecorators_OnClick;
            this._btnOthers.OnClick += _btnOthers_OnClick;
            this._btnPanels.OnClick += _btnPanels_OnClick;
            this._btnWidgets.OnClick += _btnWidgets_OnClick;
            return true;
        }



#endregion


#region Widget event handlers

        void AddControl_OnClick(IUEditorWidgetClickable sender, EventArgs e)
        {
            UEditorWidgetButton __buttonPressed = (UEditorWidgetButton)sender;
            widgetDef __widgetDef = (widgetDef)__buttonPressed.BoundObject;

            this._lastSelectedType = __widgetDef.WidgetFQName;

            if (this.OnClick != null)
            {
                this.OnClick(this, new EventArgs());
            }
        }

        void _btnWidgets_OnClick(IUEditorWidgetClickable sender, EventArgs e)
        {
            this.ActiveGroup = _vertWidgets;
        }

        void _btnPanels_OnClick(IUEditorWidgetClickable sender, EventArgs e)
        {
            this.ActiveGroup = _vertPanels;
        }

        void _btnOthers_OnClick(IUEditorWidgetClickable sender, EventArgs e)
        {
            this.ActiveGroup = _vertOthers;
        }

        void _btnDecorators_OnClick(IUEditorWidgetClickable sender, EventArgs e)
        {
            this.ActiveGroup = _vertDecorators;
        }

        void _btnControls_OnClick(IUEditorWidgetClickable sender, EventArgs e)
        {
            this.ActiveGroup = _vertControls;
        }

#endregion


#region Functional code

        protected override void WidgetRender()
        {
            EditorGUILayout.BeginVertical(this.Style, GUILayout.Width(this.Width), GUILayout.Height(this.Height));
            {
                _scrollWindow.Render();
            }
            EditorGUILayout.EndVertical();
        }


        private UEditorWidgetButton BuildButton (string Label)
        {
            UEditorWidgetButton __newButton = Create<UEditorWidgetButton>("", true);
            __newButton.Height = 16;
            __newButton.Width = 100;
            __newButton.Alignment = TextAnchor.MiddleCenter;
            __newButton.BaseStyle = "textField";
            __newButton.LayoutMode = ePositioningLayout.Layout;
            __newButton.Label = Label;
            __newButton.OnClick += AddControl_OnClick;
            return __newButton;
        }



#endregion


        
    }
}