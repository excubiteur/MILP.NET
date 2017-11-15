using System;
using System.Collections.Generic;

namespace MILP.NET
{
    public struct Index
    {
        internal int _rawIndex;
        internal Index(int index) { _rawIndex = index; }
    }

    public class Set
    {
        internal List<string> _elements = new List<string>();
        Dictionary<string, int> _lookup = new Dictionary<string, int>();


        internal Set() { }
        internal int Count { get { return _elements.Count; } }
        internal int IndexOf(string name)
        {
            int index;
            if (_lookup.TryGetValue(name, out index))
                return index;
            throw new IndexOutOfRangeException();
        }

        public void Add(string element)
        {
            _lookup[element] = _elements.Count;
            _elements.Add(element);
        }

    }







}
