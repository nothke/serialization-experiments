using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestCastInto : MonoBehaviour
{
    public interface IParent<out T> where T : IChild
    {
        T Thing { get; }
    }

    public interface IInParent<T> where T : IChild
    {
        T Thing { set; }
    }

    public interface IChild { }

    public class ChockChild : IChild { }

    public class Chock : IParent<ChockChild>, IInParent<ChockChild>
    {
        public ChockChild Thing { get; set; }
    }

    public interface IParent2<T> where T : Child
    {
        T Thing { get; set; }
    }

    public abstract class Child { }

    public class ChockChild2 : Child { };

    public class Chock2 : IParent2<ChockChild2>
    {
        public ChockChild2 Thing { get; set; }
    }

    void Start()
    {
        {
            var chock = new Chock2();

            IParent2<Child> chocchock = chock as IParent2<Child>; // Invalid cast!
            var t = chocchock.Thing;

            var deserT = t as IParent2<Child>;
            deserT.Thing = t;
            Debug.Log("Passed 1");
        }

        {
            var chock = new Chock();

            var chochock = chock as IParent<IChild>; // Invalid cast!
            var t = chochock.Thing;

            var deserT = t as IInParent<IChild>;
            deserT.Thing = t;
            Debug.Log("Passed 2");
        }
    }
}
