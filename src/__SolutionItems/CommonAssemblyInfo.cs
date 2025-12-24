using System.Reflection;

[assembly: AssemblyProduct("SecuritySystem")]
[assembly: AssemblyCompany("IvAt")]

[assembly: AssemblyVersion("2.0.4.0")]
[assembly: AssemblyInformationalVersion("changes at build")]

#if DEBUG
[assembly: AssemblyConfiguration("Debug")]
#else
[assembly: AssemblyConfiguration("Release")]
#endif