namespace uAssist.EditorDesigner
{
    using System;
    using System.Reflection;
    using System.Collections.Generic;
    using uAssist.Forms;
    using uAssist.UEditorWidgets;
    using UnityEngine;
    using System.Collections;

    public class UWidgetCodeGen
    {


        //Used to store the propery setting strings
        List<string> _propertySetters = new List<string>();


#region Class Properties

        private UEditorWidgetBase _widget;
        public UEditorWidgetBase Widget
        {
            get
            {
                return _widget;
            }
        }

        public string WidgetName
        {
            get
            {
                return _widget.Name;
            }
        }

        public string PropertyRoot = "this.";

#endregion

#region Result Properties

        public List<string> PropertySetters
        {
            get
            {
                List<string> __retvalue = new List<string>();

                foreach (var item in _propertySetters)
                {
                    __retvalue.Add(item);
                }

                return __retvalue;
            }
        }

        public string WidgetDeclaration
        {
            get
            {
                string __retValue = "";
                __retValue += this.Widget.WidgetScope + " ";
                //__retValue += "private ";
                __retValue += Widget.GetType().Name;
                __retValue += " " + Widget.Name;
                __retValue += " = UWidget.Create<";
                __retValue += Widget.GetType().Name;
                __retValue += ">();";
                return __retValue;
            }
        }

        public string WidgetParent
        {
            get
            {
                string __retValue = "";

                if (this.Widget.parent == null || this.Widget.parent.GetType().IsSubclassOf(typeof (frmBase)))
                {
                    __retValue += "this.AddChild(this." + Widget.Name + ", true);";
                }
                else
                {
                    __retValue += "this." + ((UEditorWidgetBase)this.Widget.parent).Name + ".AddChild(this." + Widget.Name + ");";
                }
                return __retValue;
            }
        }

#endregion

        //Constructor
        public UWidgetCodeGen(UEditorWidgetBase Widget, bool PharseOnConstruct = true)
        {
            _widget = Widget;

            if (PharseOnConstruct)
            {
                this.PharseWidget();
            }
        }

        public void PharseWidget()
        {
            //Clear the internal buffer
            this._propertySetters.Clear();

            Type __widgetType = Widget.GetType();

            MemberInfo[] __typeMembers = __widgetType.GetMembers();

            for (int i = 0; i < __typeMembers.Length; i++)
            {
                if (Attribute.IsDefined(__typeMembers[i], typeof(UWidgetPropertyAttribute), true))
                {
                    ProcessMember(__typeMembers[i]);
                }
            }
        }

        private void ProcessMember (MemberInfo WidgetMember)
        {
            object __propObject;
            string __newPropSetter;
            

            //Find the attribute tag
            UWidgetPropertyAttribute __propertyAttribute = null; ;
            object[] __attributes = WidgetMember.GetCustomAttributes(typeof(UWidgetPropertyAttribute), false);
            if (__attributes.Length == 1)
            {
                __propertyAttribute = (UWidgetPropertyAttribute)__attributes[0];
            }

            if (__propertyAttribute == null)
            {
                throw new Exception("Failed to find property attribute for ->" + this.Widget.Name + ":" + WidgetMember.Name);
            }

            //Get a clear reference to the reflected property
            switch (WidgetMember.MemberType)
            {
                case MemberTypes.Field:
                    __propObject = ((FieldInfo)WidgetMember).GetValue(this.Widget);
                    break;
                case MemberTypes.Property:
                    __propObject = ((PropertyInfo)WidgetMember).GetValue(this.Widget, null);
                    break;
                default:
                    throw new Exception("UWidgetCodeGen failed to convert MemberInfo to pharseable input");
            }
            
            //Bail out if we got nothing back
            if (__propObject == null)
            {
                throw new Exception("UWidgetCodeGen failed to convert MemberInfo to pharseable input");
            }

            //The the property is a widget then we need to nest the call.
            if (__propObject.GetType().IsSubclassOf(typeof (UEditorWidgetBase)))
            {
                UWidgetCodeGen __cgSubGen = new UWidgetCodeGen((UEditorWidgetBase)__propObject, false);
                __cgSubGen.PropertyRoot = this.PropertyRoot + _widget.Name + ".";
                __cgSubGen.PharseWidget();

                foreach (var item in __cgSubGen.PropertySetters)
                {
                    _propertySetters.Add(item);
                }
                return;
            }

            //If the Property has a code gen class assigned, then use that.
            if (__propertyAttribute.PropCodeGen != null)
            {
                ICodeGenerator __codeGenerator;

                try
                {
                    __codeGenerator = (ICodeGenerator)Activator.CreateInstance(__propertyAttribute.PropCodeGen);
                }
                catch
                {
                    throw new Exception("Failed to create code gen class for " + WidgetMember.Name + "\r\nDoes the specified code generator implement the ICodeGenerator inferface?");
                }

                List<string> __generatedPropSetters = __codeGenerator.CGenPropertySetters(this.PropertyRoot + _widget.Name + "." + WidgetMember.Name, __propObject);

                foreach (var item in __generatedPropSetters)
                {
                    _propertySetters.Add(item);
                }
                return;
            }

            //Generic property construction code
            __newPropSetter = this.PropertyRoot + this.WidgetName + "." + WidgetMember.Name + " = ";
            string __propValue = GenericPharseValue(__propObject);
            __newPropSetter += __propValue;
            __newPropSetter += ";";
            _propertySetters.Add(__newPropSetter);

        }

        //Will pharse strings, int's, floats, bool's and enum's
        private string GenericPharseValue (object value)
        {
            string __retValue = "";
            bool __successfulCast = false;

            Type __valueType = value.GetType();

            if (__valueType == typeof(string))
            {
                __retValue += "\"";
                __retValue += Convert.ToString(value);
                __retValue += "\"";
                __successfulCast = true;
            }

            if (__valueType == typeof(int))
            {
                __retValue += Convert.ToString(value);
                __successfulCast = true;
            }

            if (__valueType == typeof(float))
            {
                __retValue += Convert.ToString(value);
                __retValue += "f";
                __successfulCast = true;
            }

            if (__valueType == typeof(bool))
            {
                bool __castValue = (bool)value;
                if (__castValue)
                {
                    __retValue += "true";
                }
                else
                {
                    __retValue += "false";
                }
                __successfulCast = true;
            }

            if (__valueType.IsSubclassOf(typeof(System.Enum)))
            {
                __retValue += __valueType.ToString() + "." + Enum.GetName(__valueType, value);
                __successfulCast = true;
            }

            if (__successfulCast != true)
            {
                throw new Exception("Propery seralization failed cast on:\r\n" + value.ToString());
            }

            return __retValue;
        }


    }
}