using System.Collections.Generic;


namespace MILP.NET
{
    public class Param
    {
    }

    public class Param1 : Param
    {
        Set _index;
        List<double> _values = new List<double>();

        internal Param1(Set index) { _index = index; }
        public double this[Index index]
        {
            get
            {
                return _values[index._rawIndex];
            }
        }

        public void Add(string name, double value)
        {
            if (_values.Count <= 0)
                _values.Capacity = _index.Count;
            _values.Insert(_index.IndexOf(name), value);

        }
    }

}
