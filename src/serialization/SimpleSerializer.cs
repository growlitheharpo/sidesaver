using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;

namespace sidesaver.serialization
{
	public class SimpleSerializeFieldAttribute : Attribute { }

	static class SimpleSerializer
	{
		public static void WriteToFile<T>(StreamWriter w, T obj, BindingFlags flags)
		{
			if (obj == null)
				return;

			Type t = obj.GetType();
			var mems = t.GetFields(flags);

			foreach (var m in mems)
			{
				if (m.GetCustomAttribute<SimpleSerializeFieldAttribute>() == null)
					continue;

				if (m.GetValue(obj) is ICollection<string> list)
				{
					WriteStringArray(w, m.Name, list);
				}
				else
				{
					string s = $"[{m.Name}] {m.GetValue(obj)}";
					w.WriteLine(s);
				}
			}
		}

		public static void ReadFromFile<T>(StreamReader r, T targetObj, BindingFlags flags)
		{
			if (targetObj == null)
				return;

			Type t = targetObj.GetType();
			var mems = t.GetFields(flags);

			Regex reg = new Regex("(\\[)(\\w+)(\\])( )(.+)");

			while (!r.EndOfStream)
			{
				string? line = r.ReadLine();
				if (line == null)
					continue;

				Match m = reg.Match(line);
				if (m.Length == 0)
					continue;

				var memName = m.Groups[2].Value;
				var memVal = m.Groups[5].Value;

				var member = mems.FirstOrDefault(x => x.Name == memName);
				if (member == null)
					continue;

				if (member.FieldType.GetInterfaces().Any(x => x == typeof(ICollection<string>)))
				{
					if (member.GetValue(targetObj) == null)
						throw new TypeLoadException($"Trying to populate member array \"{member.FieldType.Name}\" but it was null.");

					ReadStringArray(member.GetValue(targetObj) as ICollection<string>, memVal);
				}
				else if (member.FieldType == typeof(int))
					member.SetValue(targetObj, int.Parse(memVal));
				else if (member.FieldType == typeof(float))
					member.SetValue(targetObj, float.Parse(memVal));
				else if (member.FieldType == typeof(bool))
					member.SetValue(targetObj, bool.Parse(memVal));
				else if (member.FieldType == typeof(string))
					member.SetValue(targetObj, memVal);
				else
					throw new TypeLoadException($"Type ({member.FieldType.Name} is not serializable by default.");
			}
		}

		private static void WriteStringArray(StreamWriter w, string name, ICollection<string> list)
		{
			if (w == null)
				return;

			if (list == null)
				return;

			StringBuilder sb = new StringBuilder();
			sb.AppendFormat("[{0}] ", name);
			foreach (var s in list)
			{
				if (s.Contains(','))
					throw new InvalidOperationException("Cannot serialize an array with commas in it.");

				sb.AppendFormat("{0},", s);
			}

			if (list.Count > 0)
				sb.Remove(sb.Length - 1, 1); // remove the last comma

			w.WriteLine(sb.ToString());
		}

		private static void ReadStringArray(ICollection<string>? v, string lineVal)
		{
			if (v == null)
				return;

			v.Clear();
			var vals = lineVal.Split(',');
			foreach (var l in vals)
				v.Add(l);
		}
	}
}
