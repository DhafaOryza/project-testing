using UnityEngine;
using System;
using Assets.PolyRef;

// Base Class (Bisa juga pakai Interface)
[Serializable]
public abstract class BaseEnemyData
{
    public string enemyName;
}

// Subclass 1
[Serializable]
public class ZombieData : BaseEnemyData
{
    public float Damage;
}

// Subclass 2
[Serializable]
public class RoboData : BaseEnemyData
{
    public float laserRange;
    public Color eyeColor;
}

// Script utama yang ditempel di GameObject
public class TestPolymorphism : MonoBehaviour
{
    // Tambahkan atribut [SerializeReference] dari Unity, 
    // DAN [SubclassSelector] yang baru saja Anda buat!
    [SerializeReference, SubclassSelector] 
    public BaseEnemyData enemyData;
}