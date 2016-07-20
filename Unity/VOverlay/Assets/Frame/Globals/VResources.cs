using System.Collections.Generic;
using UnityEngine;
using System.Collections;

public static class VResources
{
    static Dictionary<string, Object> resourceCache = new Dictionary<string, Object>();
    public static T Load<T>(string path) where T : Object
    {
        if (!resourceCache.ContainsKey(path))
            resourceCache[path] = Resources.Load<T>(path);
        return (T)resourceCache[path];
    }
}