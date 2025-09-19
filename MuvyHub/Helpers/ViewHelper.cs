using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace MuvyHub.Helpers
{
    public class ViewHelper
    {
        public static string GetDisplayName(Enum enumValue)
        {
            var enumType = enumValue.GetType();
            var member = enumType.GetMember(enumValue.ToString()).FirstOrDefault();
            if (member != null)
            {
                var displayAttribute = member.GetCustomAttribute<DisplayAttribute>();
                if (displayAttribute != null)
                    return displayAttribute.Name;
            }
            return enumValue.ToString();
        }

    }
}
