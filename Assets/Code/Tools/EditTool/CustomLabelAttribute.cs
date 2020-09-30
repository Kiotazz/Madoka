using UnityEngine;

public class CustomLabelAttribute : PropertyAttribute
{
    public string name;
    public CustomLabelAttribute(string name) { this.name = name; }
}
