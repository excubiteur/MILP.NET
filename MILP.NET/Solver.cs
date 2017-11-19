using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MILP.NET
{
    public abstract class Solver: IDisposable
    {
        protected abstract void GetValues();
        protected abstract double GetValue(int index);
        protected abstract void ReleaseUnmanaged();

        public void GetValues(Var1 variables, Action<string, double> iterator)
        {
            GetValues();

            int index = 0;
            foreach (var i in variables._index._elements)
            {
                var value = GetValue(variables._startIndex + index);
                iterator(i, value);
                ++index;
            }
        }

        public void GetValues(Var2 variables, Action<string, string, double> iterator)
        {
            GetValues();


            int index = 0;
            var indices = from i in variables._index1._elements
                          from j in variables._index2._elements
                          select (i, j);

            foreach (var (i, j) in indices)
            {
                var value = GetValue(variables._startIndex + index);
                iterator(i, j, value);
                ++index;
            }
        }

        public void GetValues(Var3 variables, Action<string, string, string, double> iterator)
        {
            GetValues();


            int index = 0;
            var indices = from i in variables._index1._elements
                          from j in variables._index2._elements
                          from k in variables._index3._elements
                          select (i, j, k);

            foreach (var (i, j, k) in indices)
            {
                var value = GetValue(variables._startIndex + index);
                iterator(i, j, k, value);
                ++index;
            }
        }

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                disposedValue = true;
                ReleaseUnmanaged();
            }
        }

        ~Solver()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        #endregion
    }
}
