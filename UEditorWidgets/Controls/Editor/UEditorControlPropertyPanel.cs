namespace uAssist.UEditorWidgets
{
    using System;
    using System.Reflection;
    using UnityEditor;
    using UnityEngine;

    public class UEditorControlPropertyPanel : UEditorPanelBase
    {
        private UEditorWidgetBase _boundWidget;


        //Public constructor
        public UEditorControlPropertyPanel() : base(eWidgetType.PanelVertical) 
        {
            this.LayoutMode = ePositioningLayout.Layout;
            this.Name = "PropertiesPanel";

        }

        public override bool BindTo(object Object, string MemberName)
        {
            if (MemberName == "this")
            {
                //TODO: Make this type safe
                _boundWidget = (UEditorWidgetBase)Object;
                CreatePropertyWidgets();
                return true;
            }


            if (base.BindTo(Object, MemberName) == false)
            {
                return false;
            }

            //A reference to the object we are pharsing
            object __refObject = this.GetBoundValue<UEditorWidgetBase>();

            if (__refObject.GetType().IsSubclassOf(typeof (UEditorWidgetBase)) == false)
                {
                    return false;
                }
            _boundWidget = (UEditorWidgetBase)__refObject;

            CreatePropertyWidgets();

            return true;
        }

        private void CreatePropertyWidgets()
        {
            this.Children.Clear();

            MemberInfo[] __rawMembers = _boundWidget.GetType().GetMembers(BindingFlags.Instance | BindingFlags.Public);

            foreach (MemberInfo memberInfo in __rawMembers)
            {
                if (memberInfo.Name == "parent" || memberInfo.Name == "Name")
                {
                    continue;
                }

                if (memberInfo.IsDefined(typeof(UWidgetPropertyAttribute), true))
                {
                    //TODO: Binding returns success/fail, so we should wrap and check accordingly here.
                    if (memberInfo.MemberType == MemberTypes.Property || memberInfo.MemberType == MemberTypes.Field)
                    {
                        //Work out the property type
                        Type __castType;
                        switch (memberInfo.MemberType)
                        {
                            case MemberTypes.Event:
                                __castType = ((EventInfo)memberInfo).EventHandlerType;
                                break;
                            case MemberTypes.Field:
                                __castType = ((FieldInfo)memberInfo).FieldType;
                                break;
                            case MemberTypes.Property:
                                __castType = ((PropertyInfo)memberInfo).PropertyType;
                                break;
                            default:
                                __castType = typeof(System.Object);
                                break;
                        }

                        //Get the required UWidgetPropertyAttribute attribute
                        object[] __attributes = memberInfo.GetCustomAttributes(typeof(UWidgetPropertyAttribute), true);
                        if (__attributes.Length != 1)
                        {
                            throw new Exception ("Unexpected number of UWidgetPropertyAttributes in UEditorControlPropertyPanel.CreatePropertyWidgets()");
                        }
                        UWidgetPropertyAttribute __propAttribute = (UWidgetPropertyAttribute)__attributes[0];

                        //Get the label for the control
                        string __controlLabel = memberInfo.Name;
                        if (__propAttribute.Label != "")
                        {
                            __controlLabel = __propAttribute.Label;
                        }


                        //Now we have the attribute assigned to the property we need to determin the correct control to draw
                        if (__propAttribute.HideInProperties == true)
                        {
                            continue; //Don't generate a widget
                        }

                        //If the attribute calls for a list option box with pre defined values.
                        if (__propAttribute.ListOptions != null && __propAttribute.ListOptions.Length > 0)
                        {
                            UEditorControlEnum __newControl = UWidget.Create<UEditorControlEnum>();
                            __newControl.Name = memberInfo.Name;
                            __newControl.PositionX = 15;
                            __newControl.LayoutMode = ePositioningLayout.Layout;
                            __newControl.PropertyLabel.Width = 100;
                            __newControl.PropertyLabel.Clipping = TextClipping.Clip;
                            __newControl.PropertyEnumPopup.Width = 180;
                            for (int i = 0; i < __propAttribute.ListOptions.Length; i++)
                            {
                                __newControl.PropertyEnumPopup.Options.Add(__propAttribute.ListOptions[i]);
                            }
                            __newControl.BindTo(_boundWidget, memberInfo.Name);
                            __newControl.PropertyLabel.Label = __controlLabel;
                            this.AddChild(__newControl);
                            this.AddChild(UWidget.Create<UEditorDecoratorSeperator>());
                            continue;
                        }

                        //If a custom editor required
                        if (__propAttribute.CustomEditor != null)
                        {
                            if (__propAttribute.CustomEditor.IsSubclassOf(typeof (UEditorWidgetBase)) == false)
                            {
                                Debug.LogError("Custom Editor of property " + memberInfo.Name + " must derive from UEditorWidgetBase");
                                continue;
                            }

                            object __newObject = ScriptableObject.CreateInstance(__propAttribute.CustomEditor);
                            if (__newObject == null)
                            {
                                Debug.Log("Break");
                            }
                            UEditorWidgetBase __newControl = (UEditorWidgetBase)__newObject;
                            __newControl.Name = memberInfo.Name;
                            __newControl.PositionX = 15;
                            __newControl.LayoutMode = ePositioningLayout.Layout;
                            __newControl.BindTo(_boundWidget, memberInfo.Name);
                            this.AddChild(__newControl);
                            this.AddChild(UWidget.Create<UEditorDecoratorSeperator>());

                            continue;
                        }

                        //If the property is of type bool
                        if (__castType == typeof(System.Boolean))
                        {
                            UEditorControlBool __newControl = UWidget.Create<UEditorControlBool>();
                            __newControl.Name = memberInfo.Name;
                            __newControl.PositionX = 15;
                            __newControl.LayoutMode = ePositioningLayout.Layout;
                            __newControl.PropertyLabel.Width = 100;
                            __newControl.PropertyBool.Width = 180;
                            __newControl.BindTo(_boundWidget, memberInfo.Name);
                            __newControl.PropertyLabel.Label = __controlLabel;
                            this.AddChild(__newControl);
                            this.AddChild(UWidget.Create<UEditorDecoratorSeperator>());

                            continue;
                        }

                        //If the property is a regular string or number
                        if (__castType == typeof(System.String) || __castType == typeof(System.Single) || __castType == typeof(System.Int32))
                        {
                            UEditorControlProperty __newControl = UWidget.Create<UEditorControlProperty>();
                            __newControl.Name = memberInfo.Name;
                            __newControl.PositionX = 15;
                            __newControl.LayoutMode = ePositioningLayout.Layout;
                            __newControl.PropertyLabel.Width = 100;
                            __newControl.PropertyLabel.Clipping = TextClipping.Clip;
                            __newControl.PropertyInputField.Width = 180;
                            __newControl.PropertyInputField.Clipping = TextClipping.Clip;
                            __newControl.BindTo(_boundWidget, memberInfo.Name);
                            __newControl.PropertyLabel.Label = __controlLabel;
                            this.AddChild(__newControl);
                            this.AddChild(UWidget.Create<UEditorDecoratorSeperator>());

                            continue;
                        }

                        //If the propery is an Enum
                        if (__castType.IsEnum)
                        {
                            UEditorControlEnum __newControl = UWidget.Create<UEditorControlEnum>();
                            __newControl.Name = memberInfo.Name;
                            __newControl.PositionX = 15;
                            __newControl.LayoutMode = ePositioningLayout.Layout;
                            __newControl.PropertyLabel.Width = 100;
                            __newControl.PropertyLabel.Clipping = TextClipping.Clip;
                            __newControl.PropertyEnumPopup.Width = 180;
                            __newControl.BindTo(_boundWidget, memberInfo.Name);
                            __newControl.PropertyLabel.Label = __controlLabel;
                            this.AddChild(__newControl);
                            this.AddChild(UWidget.Create<UEditorDecoratorSeperator>());

                            continue;
                        }

                        //If the property is derived from UEditorWidgetBase and thus requires a sub panel
                        if (__castType.IsSubclassOf(typeof(UEditorWidgetBase)))
                        {
                            UEditorWidgetFoldout __newControl = UWidget.Create<UEditorWidgetFoldout>();
                            __newControl.Label = __controlLabel;
                            __newControl.FontStyle = FontStyle.Bold;
                            __newControl.LayoutMode = LayoutMode;

                            UEditorPanelVertical __subPanelContainer = UWidget.Create<UEditorPanelVertical>();
                            __subPanelContainer.Padding.left = 20;
                            __subPanelContainer.Width = 400;
                            __subPanelContainer.LayoutMode = ePositioningLayout.Layout;

                            UEditorControlPropertyPanel __subPanel = UWidget.Create<UEditorControlPropertyPanel>();
                            __subPanel.BindTo(_boundWidget, memberInfo.Name);

                            __subPanelContainer.AddChild(__subPanel);
                            __newControl.AddChild(__subPanelContainer);
                            this.AddChild(__newControl);
                            this.AddChild(UWidget.Create<UEditorDecoratorSeperator>());
                        }
                    }
                }
            }

        }


        protected override void WidgetRender()
        {
            base.WidgetRender();

            RenderChildren();
        }


    }
}
