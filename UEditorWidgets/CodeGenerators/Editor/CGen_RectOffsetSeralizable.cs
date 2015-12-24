namespace uAssist.UEditorWidgets
{
    using System;
    using System.Collections.Generic;


    public class CGen_RectOffsetSeralizable : ICodeGenerator
    {

        public string CGenInitalizer()
        {
            throw new NotImplementedException();
        }

        public System.Collections.Generic.List<string> CGenPropertySetters(string PropFQN, object PropValue)
        {
            if (PropValue.GetType() != typeof(RectOffsetSeralizable))
            {
                throw new Exception("Failed to cast " + PropFQN + " to type List<String>");
            }

            RectOffsetSeralizable __castRectOffset = (RectOffsetSeralizable)PropValue;
            List<string> __properties = new List<string>();
            __properties.Add(PropFQN + ".top = " + __castRectOffset.top.ToString() + ";");
            __properties.Add(PropFQN + ".bottom = " + __castRectOffset.bottom.ToString() + ";");
            __properties.Add(PropFQN + ".left = " + __castRectOffset.left.ToString() + ";");
            __properties.Add(PropFQN + ".right = " + __castRectOffset.right.ToString() + ";");

            return __properties;
        }
    }

}