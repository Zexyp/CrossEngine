using System.Linq;

namespace CrossEngine.Utils;

public class ReflectionUtil
{
    public static string DumpFileds(object obj)
    {
        var fields = obj.GetType().GetFields();
        return "{ " + string.Join(", ", fields.Select(f => $"{f.Name} = {f.GetValue(obj)}")) + " }";
    }
}