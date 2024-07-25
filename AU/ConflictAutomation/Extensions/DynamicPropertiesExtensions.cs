using System.Reflection;

namespace ConflictAutomation.Extensions;

public static class DynamicPropertiesExtensions
{
    public static object GetPropertyValue(this object instanceObject, string propertyName)
    {
        ArgumentNullException.ThrowIfNull(instanceObject);

        Type type = instanceObject.GetType();
        return type.GetProperty(propertyName).GetValue(instanceObject, null);
    }


    public static object SetPropertyValue(this object instanceObject, string propertyName, object value)
    {
        ArgumentNullException.ThrowIfNull(instanceObject);

        Type type = instanceObject.GetType();
        PropertyInfo property = type.GetProperty(propertyName);
        property.SetValue(instanceObject, value);

        return instanceObject;
    }
}
