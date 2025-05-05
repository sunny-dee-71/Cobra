using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace cobra.Classes
{
    class Converter
    {
        public static List<Co_Object> ConvertToLowestNumberType(List<Co_Object> thing)
        {
            var result = new List<Co_Object>();
            var TypeThing = Co_Object.ObjectType.Int;

            foreach (var obj in thing)
            {
                if (!isNumber(obj))
                {
                    return thing;
                }
            }

            foreach (var obj in thing)
            {
                if (obj.Type == Co_Object.ObjectType.Float)
                {
                    if (TypeThing == Co_Object.ObjectType.Int)
                    {
                        TypeThing = Co_Object.ObjectType.Float;
                    }
                }
            }

            foreach (var obj in thing)
            {
                if (TypeThing == Co_Object.ObjectType.Int)
                {
                    result.Add(new Co_Object(Convert.ToInt32(obj.Value)));
                }
                else if (TypeThing == Co_Object.ObjectType.Float)
                {
                    result.Add(new Co_Object(Convert.ToSingle(obj.Value)));
                }
            }

            return result;
        }

        public static bool isNumber(Co_Object thing)
        {
            if (thing.Type == Co_Object.ObjectType.Int || thing.Type == Co_Object.ObjectType.Float)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}
