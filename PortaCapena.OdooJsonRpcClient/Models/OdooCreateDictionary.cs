﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Newtonsoft.Json;
using PortaCapena.OdooJsonRpcClient.Attributes;
using PortaCapena.OdooJsonRpcClient.Converters;
using PortaCapena.OdooJsonRpcClient.Extensions;

namespace PortaCapena.OdooJsonRpcClient.Models
{
    public class OdooCreateDictionary : Dictionary<string, object>
    {
        public string TableName { get; internal set; }

        public OdooCreateDictionary() { }

        public OdooCreateDictionary(string tableName)
        {
            TableName = tableName;
        }

        public static OdooCreateDictionary Create()
        {
            return new OdooCreateDictionary();
        }

        public static OdooCreateDictionary Create(string tableName)
        {
            return new OdooCreateDictionary(tableName);
        }

        public static OdooCreateDictionary<T> Create<T>() where T : IOdooAtributtesModel, new()
        {
            return new OdooCreateDictionary<T>();
        }

        public static OdooCreateDictionary<T> Create<T>(Expression<Func<T>> expression) where T : IOdooAtributtesModel, new()
        {
            return new OdooCreateDictionary<T>().Add(expression);
        }

        public static OdooCreateDictionary Create<T>(Expression<Func<T, object>> expression, object value) where T : IOdooAtributtesModel, new()
        {
            return new OdooCreateDictionary<T>().Add(expression, value);
        }
        public static OdooCreateDictionary Create<T>(Expression<Func<T, Enum>> expression, Enum value) where T : IOdooAtributtesModel, new()
        {
            return new OdooCreateDictionary<T>().Add(expression, value);
        }

        public OdooCreateDictionary Add<T>(Expression<Func<T, object>> expression, object value) where T : IOdooAtributtesModel
        {
            if (TableName != null && TryGetOdooTableName(expression, out var tableName))
                TableName = tableName;
            Add(OdooExpresionMapper.GetOdooPropertyName(expression), value);
            return this;
        }

        public OdooCreateDictionary Add<T>(Expression<Func<T, Enum>> expression, Enum value) where T : IOdooAtributtesModel
        {
            if (TableName != null && TryGetOdooTableName(expression, out var tableName))
                TableName = tableName;
            Add(OdooExpresionMapper.GetOdooPropertyName(expression), value.OdooValue());
            return this;
        }

        public OdooCreateDictionary Add<T>(Expression<Func<T>> expression, object value) where T : IOdooAtributtesModel
        {
            if (TableName != null && TryGetOdooTableName(expression, out var tableName))
                TableName = tableName;
            Add(OdooExpresionMapper.GetOdooPropertyName(expression), value);
            return this;
        }

        public OdooCreateDictionary Add<T>(Expression<Func<T>> expression) where T : IOdooAtributtesModel, new()
        {
            if (TableName == null && TryGetOdooTableName(expression, out var tableName))
                TableName = tableName;

            AddFromExpresion(expression);

            return this;
        }

        protected void AddFromExpresion<T>(Expression<Func<T>> expression) where T : IOdooAtributtesModel, new()
        {
            if (!(expression.Body is MemberInitExpression body)) throw new ArgumentException("Invalid Func");

            foreach (var memberExpression in body.Bindings)
            {
                if (memberExpression is MemberAssignment memberExp)
                {
                    var property = (PropertyInfo)memberExpression.Member;
                    var attribute = property.GetCustomAttributes<JsonPropertyAttribute>();

                    var odooName = attribute.FirstOrDefault()?.PropertyName;

                    if (odooName != null)
                    {
                        switch (memberExp.Expression)
                        {
                            case ConstantExpression constantExpression:
                            {
                                var value = constantExpression.Value;
                                Add(odooName, value);
                                continue;
                            }
                            case MemberExpression memberExpr:
                            {
                                var value = Expression.Lambda(memberExpr).Compile().DynamicInvoke();
                                Add(odooName, value);
                                continue;
                            }
                            case UnaryExpression unaryExpression:
                            {
                                var value = Expression.Lambda(unaryExpression).Compile().DynamicInvoke();
                                Add(odooName, value);
                                continue;
                            }
                            case MethodCallExpression methodCallExpression:
                            {
                                var value = Expression.Lambda(methodCallExpression).Compile().DynamicInvoke();
                                Add(odooName, value);
                                continue;
                            }
                            case NewExpression memberInitExpression:
                            {
                                var value = Expression.Lambda(memberInitExpression).Compile().DynamicInvoke();
                                Add(odooName, value);
                                continue;
                            }
                            case NewArrayExpression newArrayExpression:
                            {
                                var value = Expression.Lambda(newArrayExpression).Compile().DynamicInvoke();
                                Add(odooName, value);
                                continue;
                            }
                        }
                    }
                }
                throw new ArgumentException("Invalid Func");
            }
        }


        protected static bool TryGetOdooTableName<T>(Expression<Func<T>> expression, out string result)
        {
            result = null;
            var tableNameAttribute = expression.ReturnType.GetCustomAttributes(typeof(OdooTableNameAttribute), true).FirstOrDefault() as OdooTableNameAttribute;
            if (tableNameAttribute == null) return false;

            result = tableNameAttribute.Name;
            return true;
        }

        protected static bool TryGetOdooTableName<T>(Expression<Func<T, object>> expression, out string result)
        {
            result = null;
            var tableNameAttribute = expression.ReturnType.GetCustomAttributes(typeof(OdooTableNameAttribute), true).FirstOrDefault() as OdooTableNameAttribute;
            if (tableNameAttribute == null) return false;

            result = tableNameAttribute.Name;
            return true;
        }

        protected static bool TryGetOdooTableName<T>(Expression<Func<T, Enum>> expression, out string result)
        {
            result = null;
            var tableNameAttribute = expression.ReturnType.GetCustomAttributes(typeof(OdooTableNameAttribute), true).FirstOrDefault() as OdooTableNameAttribute;
            if (tableNameAttribute == null) return false;

            result = tableNameAttribute.Name;
            return true;
        }
    }
}