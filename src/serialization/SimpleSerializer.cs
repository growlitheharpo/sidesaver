using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;

namespace sidesaver.serialization
{
	static class SimpleSerializer
	{
		public static void WriteToFile<T>(StreamWriter w, T obj, BindingFlags flags)
		{
			Type t = obj.GetType();
			var mems = t.GetFields(flags);

			foreach (var m in mems)
			{
				if (m.FieldType == typeof(List<string>))
				{
					WriteStringArray(w, m.Name, m.GetValue(obj) as List<string>);
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
			Type t = targetObj.GetType();
			var mems = t.GetFields(flags);

			Regex reg = new Regex("(\\[)(\\w+)(\\])( )(.+)");

			while (!r.EndOfStream)
			{
				string line = r.ReadLine();
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

				if (member.FieldType == typeof(int))
					member.SetValue(targetObj, int.Parse(memVal));
				else if (member.FieldType == typeof(float))
					member.SetValue(targetObj, float.Parse(memVal));
				else if (member.FieldType == typeof(bool))
					member.SetValue(targetObj, bool.Parse(memVal));
				else if (member.FieldType == typeof(string))
					member.SetValue(targetObj, memVal);
				else if (member.FieldType == typeof(List<string>))
					ReadStringArray(member.GetValue(targetObj) as List<string>, memVal);
				else
					throw new TypeLoadException($"Type ({member.FieldType.Name} is not serializable by default.");
			}
		}

		private static void WriteStringArray(StreamWriter w, string name, List<string> list)
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

		private static void ReadStringArray(List<string> v, string lineVal)
		{
			if (v == null)
				return;

			v.Clear();
			string[] vals = lineVal.Split(',');
			v.AddRange(vals);
		}
	}
}
