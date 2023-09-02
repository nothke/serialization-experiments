This is a serialization experiment

It uses Newtonsoft Json.NET because it supports polymorphic deserialization.

Unlike most other serialization techniques, this one doesn't use reflection so it's compatible with AOT compilation like IL2CPP.

# How to use

An object you wish to save the data of must have an ID component on it. The ID will be automatically set to a unique one.

There are 2 types of serializables, a prefab and non-prefab based

Each serializable MonoBehavior must have an ID component on it and one other component that implements an ISerializable.

An ISerializable simply gets and sets a Data struct. The get and set properties of SerializedData are literally the Serialize() and Deserialize() methods, so they can either simply just set the struct/class, or they can

Lets take an example of a door, it might have an angle of how open it is, and if it's locked. We have 2 fields we want to serialize:

```c#
    float angle;
    bool isLocked;
```

Then we can put that data into a Data struct, and serialize it like this:

```c#
public class Door : MonoBehaviour, ISerializable
{
    [System.Serializable]
    public struct Data : ISerializableData
    {
        public float angle;
        public bool isLocked;
    }

    Data data;

    public ISerializableData SerializedData { get => data; set => data = (Data)value; }
}
```

If your door requires a special setup after it gets deserialized, you can simply add it to the SerializedData set property

```c#
    void UpdateDoorTransform() { }

    public ISerializableData SerializedData
    {
        get => data;

        set
        {
            data = (Data)value;
            UpdateDoorTransform();
        }
    }
```

However, we might be working on data that has already been made long ago, and you are only adding serialization as an afterthought.. No problem, you can simply use get and set as properties that set your data, and use an intermediate "serialization package" struct. It does require a little bit of duplicate writing, but it's doable:

```c#
public class Door : MonoBehaviour, ISerializable
{
    // The fields we want to serialize, but we can't afford putting it into a struct/class
    public float angle;
    public bool isLocked;

    #region Serialization

    // This is our "serialization package" that holds our data
    public struct Data : ISerializableData
    {
        public float angle;
        public bool isLocked;
    }

    // We need to manually get and set the data
    public ISerializableData SerializedData
    {
        get => new Data
        {
            angle = angle,
            isLocked = isLocked
        };

        set
        {
            Data data = (Data)value;
            angle = data.angle;
            isLocked = data.isLocked;
        }
    }

    #endregion
}
```

### Prefabs

The case above is used for persistent objects that are already in the scene (They never get created or destroyed during gameplay). But in case you need to create or destroy or instantiate an unkown number of objects at runtime, and you want to serialize their data, you need to use prefabs.

For prefabs, you simply implement a `ISerializablePrefabInstance` in a component. It defines the name that is used as a handle for knowing which prefab it is an instance of, so you need to use a unique name for each prefab. Then, you need to add the prefab to the list of serializable prefabs in the main Serializer script.

Lets take a Gun as an example. There could be many guns in one scene. Lets say that the only field we want to serialize is ammo:

```c#
public class Gun : MonoBehaviour, ISerializable, ISerializablePrefabInstance
{
    [System.Serializable]
    public struct Data : ISerializableData
    {
        public int ammo;
    }

    public Data data;

    public string PrefabName => "gun";

    public ISerializableData SerializedData { get => data; set => data = (Data)value; }

}
```