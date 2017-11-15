
namespace MILP.NET
{
    public abstract class Var
    {
        internal int _startIndex = 0;
        abstract internal int Count { get; }
    }



    public class Var1 : Var
    {
        internal Set _index;
        internal Var1(Set index) { _index = index; }
        internal override int Count { get { return _index.Count; } }
        public Expression this[Index index]
        {
            get
            {
                return new Variable(_startIndex + index._rawIndex);
            }
        }
    }
}
