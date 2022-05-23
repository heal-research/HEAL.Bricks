# Introduction
HEAL.Bricks is a package framework for .NET applications based on [NuGet](https://nuget.org). It allows you to download, manage, update, and execute packages at runtime to extend your application. HEAL.Bricks also offers isolation by executing packages in separate processes or Docker containers.


# Using HEAL.Bricks
HEAL.Bricks provides three NuGet packages:
 * [HEAL.Bricks.Abstractions](https://www.nuget.org/packages/HEAL.Bricks.Abstractions):<br/>
   HEAL.Bricks.Abstractions contains everything needed to build a HEAL.Bricks package, which can be published as NuGet package.
 * [HEAL.Bricks](https://www.nuget.org/packages/HEAL.Bricks):<br/>
   HEAL.Bricks contains everything needed to manage and execute HEAL.Bricks packages in your application.
 * [HEAL.Bricks.UI.WindowsForms](https://www.nuget.org/packages/HEAL.Bricks.UI.WindowsForms):<br/>
   HEAL.Bricks.UI.WindowsForms contains several forms to manage HEAL.Bricks packages in Windows Forms applications.

HEAL.Bricks supports the following .NET run-time environments:
 * .NET 6.0
 * .NET Standard 2.1

See our [Manual](docs/Manual.md) for more details on how to use HEAL.Bricks.


# Background Information
HEAL.Bricks is a spin-off project of [HeuristicLab](https://dev.heuristiclab.com) which is an open-source software environment for heuristic optimization, mainly developed by the research group [HEAL](https://heal.heuristiclab.com) of the [University of Applied Sciences Upper Austria](https://www.fh-ooe.at/en). We work on HeuristicLab for more than 15 years now and needed a flexible package framework, which allows us to download, manage, load, and execute packages at runtime. During the migration from .NET Framework to .NET 6.0, we had to replace our existing plug-in infrastructure, as it used AppDomains for loading plug-ins extensively, which is no longer supported in recent versions of .NET. Therefore, we created HEAL.Bricks as our new runtime package management framework on top of NuGet and seized the moment to add some additional features we missed so far.

The main features of HEAL.Bricks are:
 * use NuGet for packing and distributing packages
 * download, manage, load, and update packages at runtime
 * discover and instantiate types
 * execute packages in isolation in a separate process or a Docker container
 * communicate between host and guest process via various channels
 * support CLI as well as GUI


# Software using HEAL.Bricks
* [HeuristicLab](https://dev.heuristiclab.com) is an open-source software environment for heuristic optimization. 


# License
HEAL.Bricks is licensed under the [MIT License](LICENSE.txt).

```
MIT License

Copyright (c) 2019-present Heuristic and Evolutionary Algorithms Laboratory

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
```