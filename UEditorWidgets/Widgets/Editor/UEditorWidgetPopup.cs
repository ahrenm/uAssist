namespace uAssist.UEditorWidgets
{
    using System;
    using System.Reflection;
    using System.Collections.Generic;
    using UnityEditor;
    using UnityEngine;

    /// <summary>
    /// A generic popup control
    /// 
    /// This control suffers from having to handle 2 mode of operation. The first is popup options
    /// drawn from a provided list of strings. The second is an enumeration type. Currently these 2
    /// modes are badly mashed together and an overhaul of this code is probably a good idea.
    /// </summary>
    [UWidgetWidgetAttribute(eUWidgetDesignerCategory.Widgets, "Popup menu")]
    public sealed class UEditorWidgetPopUp : UEditorWidgetTextBase
    {
        public List<string> Options = new List<string>();

        [SerializeField]
        private bool _isEnum = false;

        [UWidgetPropertyAttribute]
        public bool IsEnum
        {
            get
            {
                return _isEnum;
            }
        }

        private Type __enumType;

        //This calls through to SetEnumGeneric<>();
        //TODO: This is 100% runtime reflection....thar be dragons.
        public bool SetEnum(Type EnumType)
        {
            MethodInfo method = typeof(UEditorWidgetPopUp).GetMethod("SetEnumGeneric");
            MethodInfo generic = method.MakeGenericMethod(EnumType);
            generic.Invoke(this, null);
            return this._isEnum;
        }

        public bool SetEnumGeneric<T>()
        {
            if (!typeof(T).IsEnum)
                return false;

            List<string> __enumOptions;

            if (UWidget.TryGetEnumOptions<T>(out __enumOptions))
            {
                this.Options = __enumOptions;
                this._isEnum = true;
                __enumType = typeof(T);
                return true;
            }
            else
            {
                this._isEnum = false;
                return false;
            }
        }

        public override bool BindTo(object Object, string MemberName)
        {
            if (!base.BindTo(Object, MemberName))
            {
                return false;
            }

            if (this.GetBoundValueType().IsEnum)
            {
                __enumType = this.GetBoundValueType();
                this.SetEnum(__enumType);
                this._isEnum = true;
            }

            /*
            //TODO: So much error checking
            Type __enumType;
            switch (this.BindingType)
            {
                case eBindingType.Property:
                    if (_boundPropertyInfo.PropertyType.IsEnum == true)
                    {
                        __enumType = Type.GetType(_boundPropertyInfo.PropertyType.AssemblyQualifiedName, true);
                        this.SetEnum(__enumType);
                        this._isEnum = true;
                    }
                    break;
                case eBindingType.Field:
                    if (_boundFieldInfo.FieldType.IsEnum == true)
                    {
                        __enumType = Type.GetType(_boundFieldInfo.FieldType.AssemblyQualifiedName, true);
                        this.SetEnum(__enumType);
                        this._isEnum = true;
                    }

                    break;
                default:
                    return false;
            }
            */

            if (this.IsEnum == false && this.Options.Count == 0)
            {
                Debug.LogWarning("UEditorWidgetPopup has been bound to a non Enum property with no options list.\r\nAlways add options to a popup before binding wherever possible.");
            }

            //Perform a simple read and write to init the variable
            var a = this.SelectedItem;
            this.SelectedItem = a;

            return true;

        }


        private string _selectedItem = "";
        public string SelectedItem
        {
            get
            {
                if (this.BindingType == eBindingType.NotSet)
                {
                    return _selectedItem;
                }
                if (this.IsEnum)
                {
                    return Enum.GetName(__enumType, GetBoundValue<object>());
                }
                else
                {
                    return this.GetBoundValue<string>();
                }
            }
            set
            {
                if (this._selectedItem != value)
                {
                    if (this.IsEnum == true)
                    {
                        this.SetBoundValue(Enum.Parse(__enumType, value));
                    }
                    else
                    {
                        //Set the bound member
                        this.SetBoundValue(value);
                    }

                    //Now we need to try and find the text in the avaliable options.
                    if (this.Options.Count > 0)
                    {
                        for (int i = 0; i < this.Options.Count; i++)
                        {
                            if (this.Options[i] == value)
                            {
                                this.SelectedTextIndex = i;
                                break;
                            }
                        }
                    }
                    this._selectedItem = value;
                }
            }
        }

        //TODO: Remove one of these
        public int SelectedEnumValue = 0;
        public int SelectedTextIndex = 0;

        public UEditorWidgetPopUp() : base(eWidgetType.Generic)
        {
            this.Name = "PopUpMenu";
            this.Width = 100;
            this.Height = EditorGUIUtility.singleLineHeight;
            this.Border.FromInt(6, 6, 4, 4);
        }


        protected override void WidgetRender()
        {
            //If the options is completly empty, insert a blank item so the control will at least render
            if (this.Options.Count == 0)
                this.Options.Add(" ");

            //If no item is selected and this is an enum, init the popup to the first option
            if (this.IsEnum && this.SelectedItem == "")
            {
                var __avaliableEnums = Enum.GetNames(__enumType);
                SelectedItem = __avaliableEnums[0];
            }

            //Render a layout popup
            if (this.LayoutMode == ePositioningLayout.Layout)
            {
                if (this.IsEnum)
                {
                    var __selectedItem = EditorGUILayout.EnumPopup(
                        (System.Enum)Enum.Parse(__enumType, 
                        SelectedItem),
                        this.Style);

                    SelectedItem = __selectedItem.ToString();
                    SelectedEnumValue = Convert.ToInt32(__selectedItem);
                }
                else
                {
                    SelectedTextIndex = EditorGUILayout.Popup(
                        SelectedTextIndex, 
                        Options.ToArray(), 
                        this.Style);

                    if (SelectedTextIndex < this.Options.Count)
                    {
                        SelectedItem = this.Options[this.SelectedTextIndex];
                    }
                }
            }
            else //Render a fixed position popup
            {
                if (this.IsEnum)
                {
                    var __selectedItem = EditorGUI.EnumPopup(
                        this.RenderRect, 
                        (System.Enum)Enum.Parse(__enumType, SelectedItem), 
                        this.Style);

                    SelectedItem = __selectedItem.ToString();
                    SelectedEnumValue = Convert.ToInt32(__selectedItem);
                }
                else
                {
                    SelectedTextIndex = EditorGUI.Popup(
                        this.RenderRect, 
                        SelectedTextIndex, 
                        Options.ToArray(), 
                        this.Style);
                    
                    if (SelectedTextIndex < this.Options.Count)
                    {
                        SelectedItem = this.Options[this.SelectedTextIndex];
                    }
                }
            }
        }
    }

}