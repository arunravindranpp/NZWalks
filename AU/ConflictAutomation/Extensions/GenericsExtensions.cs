using Newtonsoft.Json;

namespace ConflictAutomation.Extensions;

public static class GenericsExtensions
{
    public static List<string> ListSerializedObjects<T>(this List<T> listObjects) =>
        listObjects.Select(objT => JsonConvert.SerializeObject(objT)).ToList();


    public static void AppendNewObjectIfInexistent<T>(this List<T> listObjects, T newObject)
    {
        if(newObject is null)
        {
            return;
        }

        List<T> results = [];

        List<string> listSerializedObjects = ListSerializedObjects<T>(listObjects);
        listSerializedObjects.Add(JsonConvert.SerializeObject(newObject));
        listSerializedObjects = listSerializedObjects.Distinct().ToList();
        foreach (var serializedObject in listSerializedObjects)
        {
            results.Add(JsonConvert.DeserializeObject<T>(serializedObject));
        }

        listObjects.Clear();
        listObjects.AddRange(results);
    }


    public static void AppendNewListObjectsIfInexistent<T>(this List<T> listObjects, List<T> listNewObjects)
    {
        if(listNewObjects.IsNullOrEmpty())
        {
            return;
        }

        List<T> results = [];

        List<string> listSerializedObjects = ListSerializedObjects<T>(listObjects);
        List<string> newObjectsSerialized = ListSerializedObjects<T>(listNewObjects);
        listSerializedObjects.AddRange(newObjectsSerialized);
        listSerializedObjects = listSerializedObjects.Distinct().ToList();
        foreach (var serializedObject in listSerializedObjects)
        {
            results.Add(JsonConvert.DeserializeObject<T>(serializedObject));
        }

        listObjects.Clear();
        listObjects.AddRange(results);
    }   
}
