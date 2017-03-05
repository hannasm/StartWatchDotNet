# Versioning

This is version 1.0.3 of the nuget powershellscripts project

The source code for this project is available for download from: https://github.com/hannasm/Hannasm.NugetPowershellScripts01/releases/tag/1.0.3

The nuget package for this project is available at: https://www.nuget.org/packages/Hannasm.NugetPowershellScripts01/1.0.3

# Hannasm.NugetPowershellScripts01

This package provides a hand-built toolset for creating and publishing nuget packages.

## The featureset includes:

	* Put version number in a single file (metadata.xml)
		* write the same version number to all package.nuspec files
		* generate a SharedAssemblyInfo.cs file with the same verison number
	* Put release notes in a single file (metadata.xml)
		* write release notes to all package.nuspec files
	* automate the process for compiling projects in each build configuration, and generating nuget packages
	* automate the process of discovering latest nuget package and publishing it to nuget


## First let's address the pre-conditions:

	* The nuget package is installed
	* It created a set of ps1 files that you can access from the commandline
	* It created a metadata.xml file, which ideally should be stored in the solution folder
	* It created a package.tempalte.nuspec which also, ideally, should be stored in the solution folder
	* for publishing you need to have your credentials saved to your nuget profile (follow the instructions on http://nuget.org/acccount about installing your APIKey)

## Setup tasks:

	* Create a copy of the package.nuspec.template into each project you are intending to create a nuget package
		* rename the file to package.nuspec
		* it needs to be in the same folder as the .csproj / .vbproj file
		* customize the fields according to your project
		* the scripts will automatically discover these files, but there is no flexibility here
	* Customize metadata.xml with the version number for your project, as well as the release notes you would like included in each nuspec file
	* Once you have generated the package (below) you will also need to include the generated SharedAssemblyInfo.cs file in your project files
	    * in visual studio you need to use an 'add existing item - as link' option instead of the normal 'add existing item' so that you reference the generated file directly, instead of creating a copy. 
		* THis is done by clicking on the little arrow next to the add button in the find file dialog during regular 'add existing item' process and selecting 'add as link'
	    * YOu will need to delete the versioning lines from the auto-generated (ASsemblyInfo.cs file) because duplicate assembly attributes are not allowed
		* this would be a good opportunity to customize the other fields in the assemblyInfo as well

## Packaging:

	* With setup complete you can use the visual studio package manager console to generate nuget packages. Execute the ./GenerateNugetPackages.ps1 file (depending on where you installed the nuget package it may be in  subdirectory)
	* This can take a while because it runs a full build on your solution, and in every build configuration available. 

## Aside:

	* Packaging performs some tasks that should be checked into version control, it updates nuspec files and generates a SharedAssemblyInfo.cs, it is generally reccomended you commit to version control after packaging and before publishing

## Publishing:

	* Once your nuget packages are created you publish by executing ./PublishNugetPackages.ps1 from the package manager console in visual studio
	* NOTE: You must have your credentials stored in nuget before publishing


# Release Notes
 * 1.0.3 - nuget conveniently deletes files ending with .nuspec from the package even when you explicitly try to include them, so had to workaround that
 * 1.0.2 - content files weren't being included because nuget requires backslashes instead of forward slashes apparently
 * 1.0.1 - This is the first release