﻿using System.Reflection;

[assembly: AssemblyProduct("SecuritySystem")]
[assembly: AssemblyCompany("IvAt")]

[assembly: AssemblyVersion("1.2.1.0")]
[assembly: AssemblyInformationalVersion("changes at build")]

#if DEBUG
[assembly: AssemblyConfiguration("Debug")]
#else
[assembly: AssemblyConfiguration("Release")]
#endif