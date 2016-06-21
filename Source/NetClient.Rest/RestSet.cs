#region using directives

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Linq.Expressions;
using Newtonsoft.Json;

#endregion

namespace NetClient.Rest
{
    public class RestSet<T> : ObservableCollection<T>, ISet<T>
    {
        #region constructors

        public RestSet(Uri baseUri, string pathTemplate, JsonSerializerSettings serializerSettings, Expression expression = null)
        {
            Provider = new RestQueryProvider<T>(baseUri, pathTemplate, serializerSettings);
            Expression = expression ?? Expression.Constant(this);
        }

        #endregion

        #region properties and indexers

        public Type ElementType => typeof(T);

        public Expression Expression { get; }

        public IQueryProvider Provider { get; }

        #endregion

        #region implementations for IEnumerable

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion

        #region implementations for IEnumerable<T>

        public new IEnumerator<T> GetEnumerator()
        {
            return Provider.Execute<IEnumerable<T>>(Expression).GetEnumerator();
        }

        #endregion
    }
}