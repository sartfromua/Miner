using UnityEngine;

[CreateAssetMenu(fileName = "New Refined", menuName = "Data/Refined")]
public class RefinedData : ScriptableObject
{
    public string oreId; // iron_ore
    public int timeToMelt;
    public Sprite icon;
}