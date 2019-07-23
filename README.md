# Introduction
HEAL.Bricks is a plug-in framework for .NET. It allow you to discover, load, and execute plug-ins. HEAL.Bricks offers execution isolation by starting plug-ins in a separate process or Docker container.


# Using HEAL.Bricks
Simply add the [HEAL.Bricks nuget package](https://www.nuget.org/packages/HEAL.Bricks/).

HEAL.Bricks uses the .NET Standard 2.0 interface. It can be included in .NET Core as well as in .NET Framework projects. 


# Background Information
HEAL.Bricks is a spin-off project of [HeuristicLab](https://dev.heuristiclab.com) which is a software environment for heuristic optimization algorithms by the research group [HEAL](https://heal.heuristiclab.com) of the University of Applied Sciences Upper Austria. We have been working on HeuristicLab for more than 15 years and need a flexible plug-in framework. We use HEAL.Bricks for example to separate optimization algorithms and problems into separate plug-ins which can be shipped and loaded independently.

Our main objectives for HEAL.Bricks are:
 - Discover and load plugins
 - Isolated execution of plugins in separate processes or Docker containers


# Software using HEAL.Bricks
* [HeuristicLab](https://dev.heuristiclab.com) is a software environment for heuristic optimization algorithms. 


# License
HEAL.Bricks is [licensed](LICENSE.txt) under the MIT License.

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