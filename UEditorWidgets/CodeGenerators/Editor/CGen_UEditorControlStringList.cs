namespace uAssist.UEditorWidgets
{
    using System;
    using System.Collections.Generic;

    public class CGen_StringList :ICodeGenerator
    {

        public string CGenInitalizer()
        {
            throw new System.NotImplementedException();
        }

        public System.Collections.Generic.List<string> CGenPropertySetters(string PropFQN, object PropValue)
        {
            if (PropValue.GetType() != typeof(List<string>))
            {
                throw new Exception("Failed to cast " + PropFQN + " to type List<String>");
            }

            List<string> __castList = (List<string>)PropValue;
            List<string> __properties = new List<string>();

            foreach (var item in __castList)
            {
                string __value = item.Replace("\"", "\\\"" );
                __properties.Add(PropFQN + ".Add(\"" + __value + "\");");
            }

            return __properties;
        }
    }
}