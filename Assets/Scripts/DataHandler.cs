using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class DataHandler
{
    public class BodyInput
    {
        public int mass;
        public Vector3 position;
        public Vector3 velocity;

        public BodyInput(int mass, Vector3 position, Vector3 velocity)
        {
            this.mass = mass;
            this.position = position;
            this.velocity = velocity;
        }
    }
    
    public List<BodyInput> ReadCSV(string path)
    {
        var result = new List<BodyInput>();
        string csv;
        if (File.Exists(path))
        {
            csv = File.ReadAllText(path);
        }
        else
        {
            csv = Resources.Load<TextAsset>(path).text;
        }

        var trimSymbols = new[] { "\t", "\r", "\n", " " };
        foreach (var symbol in trimSymbols)
        {
            csv = csv.Replace(symbol, "");
        }
        
        var splited = csv.Split(",");
        for (int i = 6; i < splited.Length; i+= 7)
        {
            int.TryParse(splited[i - 6], out var mass);
            var position = ParseVector(splited[i - 5], splited[i - 4], splited[i - 3]);
            var velocity = ParseVector(splited[i - 2], splited[i - 1], splited[i - 0]);
            var body = new BodyInput(mass, position, velocity);
            result.Add(body);
        }

        return result;
    }

    private Vector3 ParseVector(string newX, string newY, string newZ)
    {
        float.TryParse(newX, out var x);
        float.TryParse(newY, out var y);
        float.TryParse(newZ, out var z);
        return new Vector3(x, y, z);
    }
}
