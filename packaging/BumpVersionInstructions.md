* Update README text to indicate current version
* Update Nuget Package Link in README.md
* Update source tag link in README.md
* Update Changelog at bottom of REAMDE.md
* Update Version Number in metadata.xml
* Update release notes in metadata.xml
* run './GenerateNugetPackages.ps1' from nuget package manager console inside visual studio (uses ENV.DTE stuff to build)
  * This will update / create SharedAssemblyInfo.cs
  * This will update nuspec files with versioning info from metadata
* commit to git with all the auto generated files
* run './PublishNugetPackages.ps1' from nuget package manager console inside visual studio (uses ENV.DTE stuff to build)
* create tag for release on github