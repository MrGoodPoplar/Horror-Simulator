using System;
using UnityEngine;

#if UNITY_EDITOR
[AttributeUsage(AttributeTargets.Field)]
public class BindingPathDropdownAttribute : PropertyAttribute
{
    
}
#endif