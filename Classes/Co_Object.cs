namespace cobra.Classes
{
    public struct Co_Object
    {
        public ObjectType Type { get; set; }
        public object Value { get; set; }

        public Co_Object(object obj)
        {
            if (obj is string)
            {
                Type = ObjectType.String;
                Value = obj;
            }
            else if (obj is int)
            {
                Type = ObjectType.Int;
                Value = obj;
            }
            else if (obj is bool)
            {
                Type = ObjectType.Boolean;
                Value = obj;
            }
            else if (obj is float || obj is double)
            {
                Type = ObjectType.Float;
                Value = Convert.ToSingle(obj);
            }
            else if(obj is FunctionCall)
            {
                Type = ObjectType.Function;
                Value = obj;
            }
            else
            {
                Type = ObjectType.Other;
                Value = obj;
            }
        }

        public enum ObjectType
        {
            String,
            Int,
            Float,
            Boolean,
            Variable,
            Function,
            Other
        }


        public override string ToString()
        {
            return $"[ {Type} : {Value} ]";
        }
    }
}
