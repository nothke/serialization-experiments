using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Newtonsoft.Json;

public class Player : MonoBehaviour
{
    public class BaseWeapon
    {
        public float ammo;
    }

    public class Gun : BaseWeapon
    {
        public float strength = 10;

        public override string ToString()
        {
            return "Yo, issa gun";
        }
    }

    public class Umbrella : BaseWeapon
    {
        public float kok = 2;
        public float disastrous = 100;

        public override string ToString()
        {
            return "Is that Glock? " + kok;
        }
    }

    public class WeaponCollection
    {
        public List<BaseWeapon> weapons;
    }

    public class PlayerInfo
    {
        public string Name;
        public int Age;
        public Vector3 position;

        [JsonIgnore]
        public Vector3 positionToIgnore;
    }

    public void Start()
    {
        PlayerInfo p = new PlayerInfo();
        p.Name = "Steven";
        p.Age = 21;
        var str = JsonConvert.SerializeObject(p, Formatting.None);

        Debug.Log(str);

        var desP = JsonConvert.DeserializeObject<PlayerInfo>(str);
        Debug.Log(desP.Age);

        Gun gun = new Gun();
        Umbrella umbrella = new Umbrella();
        Umbrella umbrella2 = new Umbrella();
        umbrella2.kok = 10000;

        WeaponCollection collection = new WeaponCollection();
        collection.weapons = new List<BaseWeapon>();

        collection.weapons.Add(gun);
        collection.weapons.Add(umbrella);
        collection.weapons.Add(umbrella2);

        for (int i = 0; i < collection.weapons.Count; i++)
        {
            Debug.Log(collection.weapons[i].GetType());
        }

        JsonSerializerSettings settings = new JsonSerializerSettings();
        settings.TypeNameHandling = TypeNameHandling.Auto;
        var gunStr = JsonConvert.SerializeObject(collection, settings);
        Debug.Log(gunStr);

        WeaponCollection deserializedWeapons = JsonConvert.DeserializeObject<WeaponCollection>(gunStr, settings);

        for (int i = 0; i < deserializedWeapons.weapons.Count; i++)
        {
            Debug.Log(deserializedWeapons.weapons[i]);
        }
    }
}
