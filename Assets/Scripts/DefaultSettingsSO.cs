using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(menuName = "Settings/Default", fileName = "Settings")]
public class DefaultSettingsSO : ScriptableObject
{
    public int bodiesCount = 1000;
    public int threadsCount;
    public float distanceConstraint = 10f;
    public float gravityConstant = 0.02f;
    public float spawnRange = 20;
    public float velocitiesRange = 20;
    public float maxMass = 20;
    public float minMass = 1;
    public string filePath = "Enter file name";
}
