using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace LittleHero.EditorTools
{
    public static class ValidatorHelpers
    {
        public static void ValidateRequired<T>(List<Issue> issues, string name) where T : Object
        {
            if (Object.FindAnyObjectByType<T>() == null)
                issues.Add(new Issue(Severity.Error, "Scene", $"{name} 컴포넌트 없음"));
        }

        public static void CheckRef(List<Issue> issues, string cat, SerializedObject so, string fieldName)
        {
            var prop = so.FindProperty(fieldName);
            if (prop == null) { issues.Add(new Issue(Severity.Warning, cat, $"필드 {fieldName} 미존재")); return; }
            if (prop.objectReferenceValue == null) issues.Add(new Issue(Severity.Error, cat, $"{fieldName} 미연결"));
        }
    }
}
