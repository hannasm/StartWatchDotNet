# StartwatchDotnet
## Versioning

This is version 2.0.0 of the StartwatchDotnet library.

The base package is available from nuget at: https://www.nuget.org/packages/Startwatch/2.0.0

A separate package with extensions for ExpressiveLogging are available from nuget at: https://www.nuget.org/packages/ExpressiveLogging.StartwatchExtensions/2.0.0

The source for this release is available on github at: https://github.com/hannasm/StartwatchDotnet/releases/tag/2.0.0

## Description
Startwatch is an alternative to System.Diagnostics.Stopwatch that is based on some different assumptions about how it will be used:

  * a Startwatch cannot be paused, resumed, restarted or reset. Each instance may be started exactly one time and stopped exactly one time. 
  * a Startwatch can be linked to other Startwatches, and the stop / start events on one startwatch can trigger a start / stop event on other Startwatches
     * a Startwatch can share it's start timestamp with another Startwatch, this forms a descendcy relationship (e.g. parent / child / grandchild)
	 * a Startwatch can share it's end timestamp with another Startwatch, this forms an adjacency relationship (e.g younger / older sibling)
  * a Startwatch can be constructed from timing observations collected using other tools (this is a property sorely missing from System.Diagnostics.Stopwatch)

Using the descendcy and adjacency relationships in particular yields some major benefits:
	* you will use less CPU cycles when measuring timing data in your source code. 
	* you elimninate the possiblity of small gaps in timing data that can arise from other approaches to instrumentation

In the case of one off measurements, Startwatch gives comparable performance to System.Diagnostics.Stopwatch

## Basics
To take a sample just create one:

```csharp
var executionTime = Startwatch.StartNew();

/* execute interesting code here */

executionTime.Stop();
```

The measurement data is stored internally using hardware specific tick units, and measurement data is easily accessed in other common formats:

```csharp
Console.WriteLine("Total Ticks {0}", (long)executionTime.ElapsedTicks);
Console.WriteLine("Total Milliseconds {0}", (long)executionTime.ElapsedMilliseconds);
Console.WriteLine("Total Timespan {0}", (TimeSpan)executionTime.Elapsed);
Console.WriteLine("Start Time {0}", (DateTime?)executionTime.StartTime);
Console.WriteLine("End Time {0}", (DateTime?)executionTime.EndTime);
```

## Advanced usage

Let's start off with just an example of using performance event tracker:

```csharp
	var tracker = new PerformanceEventTracker();

    var setup = tracker.NextEvent().WhenComplete(e => Console.WriteLine("Setup completed in {0}", e.TimeData.ElapsedMilliseconds));

    /* execute interesting code here */
    System.Threading.Thread.Sleep(new System.Random().Next(10, 20));

    var totalTimer = tracker.NextEvent().WhenComplete(e => Console.WriteLine("Loop completed in {0}", e.TimeData.ElapsedMilliseconds));
    var loopInitTimer = tracker.PushFirstEvent().WhenComplete(e => Console.WriteLine("Loop init in {0}", e.TimeData.ElapsedMilliseconds));
    for (int i = 0; i < 100; i++)
    {
        var loopTimer = tracker.NextEvent().WhenComplete(e => Console.WriteLine("Loop timer in {0}", e.TimeData.ElapsedMilliseconds));

        /* execute interesting code here */
        System.Threading.Thread.Sleep(new System.Random().Next(10, 20));
    }
    tracker.PopLastEvent();
    var teardown = tracker.NextEvent().WhenComplete(e => Console.WriteLine("Teardown in {0}", e.TimeData.ElapsedMilliseconds));

    /* execute interesting code here */
    System.Threading.Thread.Sleep(new System.Random().Next(10, 20));

    tracker.Complete().WhenComplete(e => Console.WriteLine("Total duration {0}", e.TimeData.ElapsedMilliseconds));
```

Startwatch provides all of the necesarry low level functionality required to perform high performance linked timing using descendcy and adjacency relationships
but the API lacks much of the finesse desired from an end-user perspective. Performance tracker exposes the same functionality in a slightly more
convenient API at the expense of making some additional assumptions about your use cases. 

Performance event tracker exposes all of the available descendcy and adjacency relationships throuhg the methods Push() / Pop() (for descendcy)
and Next() / New() (for adjacency). 

In the case of descendcy relationships there also exist the special PushFirst() and PopLast() methods which create an additional Startwatch
that is linked to the parent. 

  * PushFirst() the start time of the parent is shared with the child. 
	
  * PopLast() start time is the same as end time of closest older sibling, and end time is the same as its parent end time

Attaching to the WhenComplete() callbacks allow for capturing event data in realtime as the event is being completed (which in some cases such 
as PopLast() can be difficult or impossible to know about ahead of time). The primary extra overhead associated with PerformanceEvenTracker
stems from these callback delegates, and if you have no interest in using them you may want to consider using the Startwatch API
to perform event tracking yourself. That said, the performance impact from these callbacks is minimal except in the most demanding of
applications.

## Extensions
If you use the ExpressiveLogging library, there are several extensions published in the ExpressiveLogging.StartwatchExtensions package.

The first set of extensions allows direct use of Startwatch for performance counter logging.

The second set of extensions integrates PerformanceTracker functionality (described above) into an API consistent with the ExpressiveLogging
tools.

*More documentation is needed here*

## Build Environment

basic build from a commandline is performed with `dotnet build`

unit tests may be run with `dotnet test`

when preparing a new release you must perform all of the following steps:
  * update version number in Starwatch.VersionNumber.md
  * update version number and package links in README.md
  * update release notes in Startwatch.ReleaseNotes.md
  * ensure unit tests are passing
  * perform a release build with `dotnet build -c release`
  * commit final version of all artifacts to source control
  * publish nuget packages
  * create tag for release on github

to push a new version to nuget you must pack and push release builds

```
dotnet pack -c release
dotnet nuget push <path to nupkg> -k <nuget key here> -s https://www.nuget.org/api/v2/packages
```

## Release notes
[For Release Notes See Here](Startwatch.ReleaseNotes.md)
