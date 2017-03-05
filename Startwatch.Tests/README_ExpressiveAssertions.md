# Versioning
This is version 0.9.7 of the expressive assertions library

This package is available from nuget at: https://www.nuget.org/packages/ExpressiveAssertions/0.9.7

The source for this release is available on github at: https://github.com/hannasm/ExpressiveAssertionsDotNet/releases/tag/0.9.7

# ExpressiveAssertionsDotNet
Flexible Assertion Library Leveraging the .NET Expression Tree Syntax. This library attempts to provide a robust, open
solution to assertions and unit testing for the .NET EcoSystem that is easily portable across different unit testing
frameworks.

This library is influenced by and in many ways similar to several other assertion libraries you may or may not be familiar with:

* PowerAssert.NET - https://github.com/PowerAssert/PowerAssert.Net
* PAssert from ExpressionToCode - https://github.com/EamonNerbonne/ExpressionToCode
* MSTest Assertion Library - https://msdn.microsoft.com/en-us/library/microsoft.visualstudio.testtools.unittesting.aspx

This library attempts to provide test framework portability by separating the API used for testing program correctness, 
from the underlying mechanism that communicates with the testing framework.

The following native unit testing framwork implementations are currently available:

* MSTest via ExpressiveAssertions.MSTest - ExpressiveAssertions.MSTest.MSTestAssertionTool

# Rendering
One of the main components that is being decoupled in this library is the rendering code. It is possible
to customize the experience of assertion failure in a variety of ways based on the use cases you are targeting.
In the simplest scenario one wants to throw an exception with a textual representation of an assertion
that has failed. In more complex scenarios, it may be desilrable to render obejct graphs, transport
assertion details to remote servers, or a variety of other interesting things.

As part of this design, the work of generating a human-readable textual representation
of an assertion is part of a separte component from the one which actually generates a 
test-framework specific exception signaling that an assertion has failed.

From a client / user perspective, a little extra work is necesarry to setup a chain of 
renders that delivers all the desired features.

When developing for this assertion library, it is necesarry to keep components stripped down to their minimum
feature set, and plan on using multiple components, and composition, to create complete and interesting
functionality.

# Usage
The core product is the IAssertionTool interface. Assertions are implemented through extension methods
built around this interface. 

```C#
using ExpressiveAssertions;

IAssertionTool assert = ExpressiveAssertions.Tooling.ShortAssertionRendererTool.Create(
	ExpressiveAssertions.MSTest.MSTestAssertionTool.Create()
);
```

In this case, the assertion tool is setup to use the MSTest specific implementation and also leverages the 
built in ShortAssertionRenderer to display assertions in a user-friendly, human readable format.

Fundamentally assertions are implemented on the principle of expression trees.  Any 'assertion' consists of a series of 
instructions to access one or more pieces of memory, and to compare the actual state of that memory region to the expected values. These
fundamental assertions are exposed through overloads of the method Check().

```C#
int x = 10;

assert.Check(()=>x == 10); // Success Asserting v_01.x == int32_01 with 'v_01.x'= (10) and 'int32_01'= (10).
assert.Check(()=>x == 20); // Failure Asserting v_01.x == int32_01 with 'v_01.x'= (10) and 'int32_01'= (20)

assert.Check(()=>x, ()=>10, (a,b)=>a==b); // Success Asserting a == b with 'a'= (10) and 'b'= (10). 
assert.Check(()=>x, ()=>10, (a,b)=>a==b); // Failure Asserting a == b with 'a'= (10) and 'b'= (20).
```

The process of communicating the outcome of a test is handled separately from the actual testing. Exacting messaging can evolve 
on an implementation by implementation basis, and be adapted to unexpected new paradigms as needed.

> It's probably worth noting at this point, that all assertions (pass or fail), are communicated to the underlying 
> implementation, and ultimately can be provided to the testing framework. This can enable some more advanced analysis 
> of test quality (through gathering metrics on assertion quantity or quality). It is reccomended that implementations always provide 
> some mechanism for displaying the successful assertions, along with those that fail.

The Check() method overloadss on the IAssertionTool makeup the entirety of the core assertion functionality. 
Everything else is syntactic sugar for calling into these methods.

The set of built-in assertions includes most of those exposed by the MSTEst assertion library. Some additional assertions
are included for asserting data in ordered, and unordered collections. 

## Testing Context
The assertion tooling provides the concept of context which allows for tracking data about unit tests through different scopes.
This can be especially useful when defining many tests that pertain to specific objects, and implementing robust
tests over large data sets with hierarchical layouts. Context solves the problem of tracking the details of where in the testing
process an error occurs in the face of increasing complexity.

```C#
var data = new[] {
	new { FieldOne = 10, FieldTwo = 100 },
	new { FieldOne = 30, FieldTwo = 900 },
	new { FieldOne = 2, FieldTwo = 4 },
};

using (assert.ContextPush())
for (int i =0; i < data.Length; i++) {
	assert.ContextSet("index", i.ToString());
	
	// fieldtwo must be square of fieldone
	assert.IsTrue(()=>data[i].FieldOne * data[i].FieldOne == data[i].FieldTwo );
	// fieldone must be multiple of 10
	assert.IsTrue(()=>data[i].FieldOne % 10 == 0);
}
```

This test eventually fails on the third element, because 2 is not a multiple of 10. Included in the error messaging is the line: 

> 'Depth 1 - index' with value '2'.

This indicates at scope depth of 1, there is a context field defined called index, with the value 2. 
This kind of context information greatly simplifies tracking down issues on failed tests without needing to employ debuggers or
replay code.

## Soft assertions
An assertion tool implementation called SoftAssertionTool enables for capturing multiple assertions, gathering statistics on their outcomes, 
and then even selectively replaying those assertions on another assertion tool at another point in time. This soft assertion tooling
can be used to implement some interesting concepts such as multiple assertions before failure, or more generally conditionally failing
and passing a test based on the nature of the assertions that are made.

One example of this is included in the unordered collection assertions. In the unordered assertions, we need to check that for two collections,
there is a positive correspondence between each element. We need to safely test our assertion conditions against many elements, 
and ignore any assertion failures that occur so long as we find that one correspondence exists between each element in both collections. As soon as
(and only if) any element is found that does not have a positive correspondence, all bets are off and we need to have a yardsale of all 
the different checks that were made while coming to such a conclusion.


# Tests
There is currently a very minimal collection of unit tests available. A more robust test suite is a pre-condition for this project
reaching it's 1.0.0 version release.

# Build Notes
The build for this project depends on ILMerge, and embeds several other assemblies using the /internalize flag. The 
embedded assemblies include:

  * ExpressionToCode - https://github.com/EamonNerbonne/ExpressionToCode
  * ExpressiveReflection - https://github.com/hannasm/ExpressiveReflectionDotnet
  * ExpressiveExpressionTrees - https://github.com/hannasm/ExpressiveExpressionTreesDotNet

These assemblies provide features that are being used internally, but their functionality is not exposed in any way externally as part
of an API, making them perfect candidates for the internalization. You should be able to safely consume this assembly without taking on 
any new dependencies in your own solution.

# Licensing
This code is released on under an MIT license. 

This code uses parts of ExpressionToCode which is licensed under the Apache license. A copy of this license is included.

# Changelog

## 0.9.7
    * 0.9.7 - addition of AssertionInverterTool to treat all success as failure and all failure as success
	* 0.9.7 - addition of IgnoreDeclaredFailureAssertionTool which will ignore any declared failure assertions
	* 0.9.7 - addition of IgnoreInconclsuiveAssertionTool which will ignore any inconclusive assertions

## 0.9.6
    * 0.9.6 - fix bug with mstest handling of assertion messages

## 0.9.5
    * 0.9.5 - fix bug in short assertion renderer causing all failed assertions to be marked as success

## 0.9.4
    * 0.9.4 - fixed another nuget packaging bug

## 0.9.3
    * 0.9.3 - added some tests on the base assertions
    * 0.9.3 - refactored a number of specialized / internally focused classes to separate namespaces to limit clutter in the primary namespace
    * 0.9.3 - cleaned up some documentation in README.md
    * 0.9.3 - nuget package now qualifies the readme as README_ExpressiveAssertions.md instead of just README.md
    * 0.9.3 - MSTest assertion no longer renders assertions automatically, you must specifically chain it with a renderer assertion (such as the builtin ShortAssertionRendererTool)

## 0.9.2
  * better error reporting in the mstest driver
  * started removing code contracts stuff because they don't report error messages the way i want
  * Renamed the Assert() method to be just another overload of Check() instead
  * Add the concept of context data including api methods for using It
  * updates to generic AreEqual and AreNotEqual to use object.Equals() for proper equality checking in case of anonymous types
  * new assertions nfor IsInstanceof() / IsNotInstanceOf()
  * add initial suite of assertions for collection types
  * new assertions for IsNull() / IsNotNull()
  * new assertions for IsTrue() / IsFalse()
  * new assertions for Throws()
  * add SoftAssertionTool
  * unit testing for assertion context code

## 0.9.1
  * adding license files to nuget packages

## 0.9.0
  * initial release is beta / alpha version with some incomplete / missing features