using UnityEngine;

[CreateAssetMenu(fileName = "New Ore", menuName = "Data/Ore")]
public class OreData : ScriptableObject
{
    public string oreId; // iron_ore
    public int price;
    public Sprite icon;
    public float durability;
    public float chance;
}