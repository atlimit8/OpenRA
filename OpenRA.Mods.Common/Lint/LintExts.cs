#region Copyright & License Information
/*
 * Copyright 2007-2020 The OpenRA Developers (see AUTHORS)
 * This file is part of OpenRA, which is free software. It is made
 * available to you under the terms of the GNU General Public License
 * as published by the Free Software Foundation, either version 3 of
 * the License, or (at your option) any later version. For more
 * information, see COPYING.
 */
#endregion

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using OpenRA.Support;

namespace OpenRA.Mods.Common.Lint
{
	public class LintExts
	{
		public static IEnumerable<string> GetFieldValues(object ruleInfo, FieldInfo fieldInfo, Action<string> emitError)
		{
			var type = fieldInfo.FieldType;
			if (type == typeof(string))
				return new[] { (string)fieldInfo.GetValue(ruleInfo) };

			if (typeof(IEnumerable<string>).IsAssignableFrom(type))
				return fieldInfo.GetValue(ruleInfo) as IEnumerable<string>;

			if (type == typeof(BooleanExpression) || type == typeof(IntegerExpression))
			{
				var expr = (VariableExpression)fieldInfo.GetValue(ruleInfo);
				return expr != null ? expr.Variables : Enumerable.Empty<string>();
			}

			throw new InvalidOperationException("Bad type for reference on {0}.{1}. Supported types: string, IEnumerable<string>, BooleanExpression, IntegerExpression"
				.F(ruleInfo.GetType().Name, fieldInfo.Name));
		}

		public static IEnumerable<string> GetPropertyValues(object ruleInfo, PropertyInfo propertyInfo, Action<string> emitError)
		{
			var type = propertyInfo.PropertyType;
			if (type == typeof(string))
				return new[] { (string)propertyInfo.GetValue(ruleInfo) };

			if (typeof(IEnumerable).IsAssignableFrom(type))
				return (IEnumerable<string>)propertyInfo.GetValue(ruleInfo);

			if (type == typeof(BooleanExpression) || type == typeof(IntegerExpression))
			{
				var expr = (VariableExpression)propertyInfo.GetValue(ruleInfo);
				return expr != null ? expr.Variables : Enumerable.Empty<string>();
			}

			throw new InvalidOperationException("Bad type for reference on {0}.{1}. Supported types: string, IEnumerable<string>, BooleanExpression, IntegerExpression"
				.F(ruleInfo.GetType().Name, propertyInfo.Name));
		}

		public static IEnumerable<T> GetFieldValues<T>(object ruleInfo, FieldInfo fieldInfo) where T : class
		{
			var type = fieldInfo.FieldType;
			if (fieldInfo.GetValue(ruleInfo) == null)
				return Enumerable.Empty<T>();

			if (typeof(T).IsAssignableFrom(type))
				return new[] { (T)fieldInfo.GetValue(ruleInfo) };

			var enumerable = fieldInfo.GetValue(ruleInfo) as IEnumerable;
			if (enumerable != null)
				return enumerable.AsRecursiveEnumerable<T>();

			return Enumerable.Empty<T>();
		}

		public static IEnumerable<T> GetPropertyValues<T>(object ruleInfo, PropertyInfo propertyInfo) where T : class
		{
			var type = propertyInfo.PropertyType;
			if (propertyInfo.GetValue(ruleInfo) == null)
				return Enumerable.Empty<T>();

			if (typeof(T).IsAssignableFrom(type))
				return new[] { (T)propertyInfo.GetValue(ruleInfo) };

			var enumerable = propertyInfo.GetValue(ruleInfo) as IEnumerable;
			if (enumerable != null)
				return enumerable.AsRecursiveEnumerable<T>();

			return Enumerable.Empty<T>();
		}
	}
}
