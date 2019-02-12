using System;
using System.Globalization;
using System.IO;
using System.Reflection;

namespace sidesaver
{
	static class AssemblyResolver
	{
		// This neat trick comes courtesy of
		// http://www.digitallycreated.net/Blog/61/combining-multiple-assemblies-into-a-single-exe-for-a-wpf-application
		// and allows us to send just a .exe along, without including extra DLLs
		public static Assembly OnAssemblyResolve(object sender, ResolveEventArgs e)
		{
			Assembly currentAssembly = Assembly.GetExecutingAssembly();
			AssemblyName name = new AssemblyName(e.Name);

			string path = name.Name + ".dll";
			if (name.CultureInfo.Equals(CultureInfo.InvariantCulture) == false)
			{
				path = $@"{name.CultureInfo}\{path}";
			}

			using (Stream s = currentAssembly.GetManifestResourceStream(path))
			{
				if (s == null)
					return null;

				byte[] rawBytes = new byte[s.Length];
				s.Read(rawBytes, 0, rawBytes.Length);
				return Assembly.Load(rawBytes);
			}
		}
	}
}
