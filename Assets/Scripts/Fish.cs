using UnityEngine;

[CreateAssetMenu(fileName = "NewFish", menuName = "Fishing Game/Fish")]
public class Fish : ScriptableObject
{
    public string fishName = "Poisson";
    public int points = 10;
    public Sprite fishSprite; 
}