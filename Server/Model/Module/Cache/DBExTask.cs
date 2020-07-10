using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using MongoDB.Driver;

namespace ETModel
{
    public abstract class DBExTask : DBTask
    {
        public List<Tuple<PropertyInfo, object>> parameters { get; set; }

        public Expression<Func<ComponentWithId, bool>> CreateFilter()
        {
            return entity => /*((User)entity).account == "bbb@aaa.aaa"*/true;
            //FilterDefinition<ComponentWithId> filter = null;
            //for(int i = 0; i < parameters.Count; i++)
            //{
            //    var parameter = parameters[i];
            //    var property = parameter.Item1;
            //    var val = parameter.Item2.ToString();
            //    if (filter == null)
            //    {
            //        filter = Builders<ComponentWithId>.Filter.Eq(_ => property.GetValue(_).ToString(), val);
            //    }
            //    else
            //    {
            //        filter &= Builders<ComponentWithId>.Filter.Eq(_ => property.GetValue(_).ToString(), val);
            //    }
            //}
            //return filter;
        }

        public string GetCondition()
        {
            List<string> list = new List<string>();
            for (int i = 0; i < parameters.Count; i++)
            {
                var parameter = parameters[i];
                var propertyName = parameter.Item1.Name;
                var val = parameter.Item2.ToString();
                list.Add($"{propertyName} = {val}");
            }
            return string.Join(" ", list);
        }
    }
}