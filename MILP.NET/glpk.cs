using System;
using System.Runtime.InteropServices;

namespace MILP.NET
{
    public class glpk: IDisposable
    {
        const string glpklibname = "glpk_4_63.dll";

        unsafe struct glp_smcp {
            int msg_lev;            /* message level: */
            int meth;               /* simplex method option: */
            int pricing;            /* pricing technique: */
            int r_test;             /* ratio test technique: */
            double tol_bnd;         /* primal feasibility tolerance */
            double tol_dj;          /* dual feasibility tolerance */
            double tol_piv;         /* pivot tolerance */
            double obj_ll;          /* lower objective limit */
            double obj_ul;          /* upper objective limit */
            int it_lim;             /* simplex iteration limit */
            int tm_lim;             /* time limit, ms */
            int out_frq;            /* display output frequency, ms */
            int out_dly;            /* display output delay, ms */
            int presolve;           /* enable/disable using LP presolver */
            int excl;               /* exclude fixed non-basic variables */
            int shift;              /* shift bounds of variables to zero */
            int aorn;               /* option to use A or N: */
            fixed double foo_bar[33];     /* (reserved) */
        }

        [DllImport(glpklibname)]
        static extern IntPtr glp_create_prob();

        [DllImport(glpklibname)]
        static extern void  glp_delete_prob(IntPtr lp);

        [DllImport(glpklibname)]
        static extern int glp_add_cols(IntPtr lp, int ncs);

        const int GLP_CV = 1;
        [DllImport(glpklibname)]
        static extern void glp_set_col_kind(IntPtr lp, int j, int kind);

        const int GLP_FR = 1;
        const int GLP_LO = 2;
        const int GLP_UP = 3;
        const int GLP_DB = 4;
        const int GLP_FX = 5;
        [DllImport(glpklibname)]
        static extern void glp_set_col_bnds(IntPtr lp, int j, int type, double lb, double ub);

        [DllImport(glpklibname)]
        static extern void glp_add_rows(IntPtr lp, int nrs);

        [DllImport(glpklibname)]
        static extern void glp_set_row_bnds(IntPtr lp, int i, int type, double lb, double ub);

        [DllImport(glpklibname)]
        static extern void glp_set_mat_row(IntPtr lp, int i, int len, int[] ind, double[] val);

        const int GLP_MIN = 1;
        const int GLP_MAX = 2;
        [DllImport(glpklibname)]
        static extern void glp_set_obj_dir(IntPtr lp, int dir);

        [DllImport(glpklibname)]
        static extern void glp_set_obj_coef(IntPtr lp, int j, double coef);

        [DllImport(glpklibname)]
        static extern void glp_init_smcp(ref glp_smcp parm);

        [DllImport(glpklibname)]
        static extern int glp_simplex(IntPtr lp, ref glp_smcp parm);

        [DllImport(glpklibname)]
        static extern double glp_get_obj_val(IntPtr lp);

        [DllImport(glpklibname)]
        static extern double glp_get_col_prim(IntPtr lp, int j);

        Model _model;
        IntPtr _lp;

        public glpk(Model m)
        {
            _model = m;
        }


        public void solve()
        {
            _lp = glp_create_prob();

            int ncols = _model.NumberOfVariables;
            glp_add_cols(_lp,ncols);

            for(int i = 1; i <= ncols; ++i)
            {
                glp_set_col_kind(_lp, i, GLP_CV);
                var lower = _model.GetLowerBound(i - 1);
                var upper = _model.GetUpperBound(i - 1);
                if (lower.HasValue && upper.HasValue)
                {
                    glp_set_col_bnds(_lp, i, GLP_DB, lower.Value, upper.Value);
                }
                else if (lower.HasValue)
                {
                    glp_set_col_bnds(_lp, i, GLP_LO, lower.Value, 0);
                }
                else if (upper.HasValue)
                {
                    glp_set_col_bnds(_lp, i, GLP_UP, 0, upper.Value);
                }
                else
                {
                    glp_set_col_bnds(_lp, i, GLP_FR, 0, 0);
                }

            }

            glp_add_rows(_lp, _model._constraints.Count);

            int currentRow = 0;
            foreach(var c in _model._constraints)
            {
                ++currentRow;

                double lower = 0;
                double upper = 0;

                int type;
                if (c._lowerBound.HasValue && c._upperBound.HasValue)
                {
                    lower = c._lowerBound.Value - c._expression._constant;
                    upper = c._upperBound.Value - c._expression._constant;
                    if (c._lowerBound >= c._upperBound)
                        type = GLP_FX;
                    else
                        type = GLP_DB;
                }
                else if(c._upperBound.HasValue)
                {
                    upper = c._upperBound.Value - c._expression._constant;
                    type = GLP_UP;
                }
                else if(c._lowerBound.HasValue)
                {
                    lower = c._lowerBound.Value - c._expression._constant;
                    type = GLP_LO;
                }
                else
                {
                    type = GLP_FR;
                }

                glp_set_row_bnds(_lp, currentRow, type, lower, upper);

                var indices = new int [c._expression._terms.Count + 1];
                var values = new double[c._expression._terms.Count + 1];

                int count = 0;
                foreach (var t in c._expression._terms)
                {
                    ++count;
                    indices[count] = t.Key + 1;
                    values[count] = t.Value._coefficient;
                }
                glp_set_mat_row(_lp, currentRow, indices.Length - 1, indices, values);
            }

            if (_model._maximize)
                glp_set_obj_dir(_lp, GLP_MAX);
            else
                glp_set_obj_dir(_lp, GLP_MIN);

            switch(_model._objective)
            {
                case Sum s:
                    {
                        glp_set_obj_coef(_lp, 0, s._constant);
                        foreach(var t in s._terms)
                        {
                            glp_set_obj_coef(_lp, t.Key + 1, t.Value._coefficient);
                        }
                    }
                    break;
                default:
                    throw new NotImplementedException();
            }

            var parm = new glp_smcp();
            glp_init_smcp(ref parm);
            glp_simplex(_lp, ref parm);

        }

        public double ObjectiveValue {  get { return glp_get_obj_val(_lp); } }

        public void GetValues(Var1 variables, Action<string, double> iterator)
        {
            int index = 0;
            foreach(var i in variables._index._elements)
            {
                var value = glp_get_col_prim(_lp, variables._startIndex + index + 1);
                iterator(i, value);
                ++index;
            }
        }

        public void GetValues(Var2 variables, Action<string, string, double> iterator)
        {
            int index = 0;
            foreach (var i in variables._index1._elements)
            {
                foreach (var j in variables._index2._elements)
                {
                    var value = glp_get_col_prim(_lp, variables._startIndex + index + 1);
                    iterator(i, j, value);
                    ++index;
                }
            }
        }

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (_lp != IntPtr.Zero)
                    glp_delete_prob(_lp);

                disposedValue = true;
            }
        }

       ~glpk()
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
