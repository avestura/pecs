# Performance Evaluation of Computer Systems

This is a general repo related to "Performance Evaluation of Computer Systems" course of Software Engineering Group in [Iran University of Science and Technology](http://iust.ac.ir/en).

Professor: [Dr. Mohammad Abdollahi Azgomi](http://webpages.iust.ac.ir/azgomi)

Student: [Aryan Ebrahimpour](https://aryan.software)

Projects Language: [F#](https://dotnet.microsoft.com/languages/fsharp)

### Computer Performance and Evaluation

**Computer performance** is the efficiency of a given computer system, or how well the computer performs, when taking all aspects into account. A **computer performance evaluation** is defined as the process by which a computer system's resources and outputs are assessed to determine whether the system is performing at an optimal level. It is similar to a voltmeter that a handyman may use to check the voltage across a circuit. The meter verifies that the correct voltage is passing through the circuit. Similarly, an assessment can be done on a PC using established benchmarks to see if it is performing correctly.<a href="https://study.com/academy/lesson/computer-performance-evaluation-definition-challenges-parameters.html" target="_blank"><sup>1</sup></a>

### Assignments

I put assignments/projects of class here.

##### Paper Assignments

There are currently no paper assignments provided here.
Although you can find some of them in professors webpage.

##### Project Assignments

1. Discrete Event Simulation

### Build and Run

For running scripts:

1. Download and install the stable release of [.NET Core 3 SDK](https://dotnet.microsoft.com/download/dotnet-core/3.0)
2. Run script using `dotnet fsi` command. For example for running DES scripts run:

```powershell
git clone https://github.com/0xaryan/pecs
cd .\pecs\src\1-DiscreteEventSimulation
dotnet fsi .\Sample1.fsx # for the first sample using hardcoded data
dotnet fsi .\Sample2.fsx # for auto-generated data using expotensial distribution
```