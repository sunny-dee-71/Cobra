namespace cobra.Classes
{
    public class Co_Object
    {
        public ObjectType Type { get; set; }
        public object Value { get; set; }

        public Co_Object(object obj)
        {
            Value = obj;

            if (obj is string)
                Type = ObjectType.String;
            else if (obj is int)
                Type = ObjectType.Int;
            else if (obj is bool)
                Type = ObjectType.Boolean;
            else
                Type = ObjectType.Other;

        }

        public enum ObjectType
        {
            String,
            Int,
            Boolean,
            Other
        }
    }
}
