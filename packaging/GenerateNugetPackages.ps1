$scriptPath = split-path $SCRIPT:MyInvocation.MyCommand.Path -parent;

if (!(get-command 'Get-Project' -errorAction SilentlyContinue) -or !$dte) {
	throw ('this script is intended to be run from the nuget package manager console inside visual studio which has access to visual studio DTE');
}

$project = Get-Project | select -First 1
$build = $dte.Solution.SolutionBuild;
if (!$dte.Solution.SolutionBuild) {
	throw ('unable to locate solution build component from visual studio environment');
}

$solutionDir = split-path $dte.Solution.FullName -parent;
$metadataPath = join-path $solutionDir 'metadata.xml';
if (!(test-path $metadataPath)) {
	throw ('unable to find solution metadata at ' + $metadataPath);
}
$metadata = [xml](get-content $metadataPath);
$metadata = $metadata.solution;
if (!$metadata) {
	throw ('metadata file  at ' + $metadataPath + ' is missing root solution node');
}

# Update nuspec file with metadata
foreach ($proj in $dte.Solution) {
  if (!$proj -or !$proj.FullName) { continue; }

  $path = split-path -parent $proj.FullName;
  $nuspec = join-path $path 'package.nuspec';
  if (!(Test-Path $nuspec)) {
    write-verbose ('No nuspec found at ' + $nuspec);
    continue;
  }

  & (join-path $scriptPath 'UpdateNugetPackageIfNeeded.ps1') $metadataPath $nuspec;
}

#Generate assembly info
& (join-path $scriptPath 'GenerateAssemblyInfo.ps1') $solutionDir;

# build solution, use every build configuration available
$oldCfg = $dte.Solution.SolutionBuild.ActiveConfiguration
foreach ($newCfg in $dte.Solution.SolutionBuild.SolutionConfigurations) {
  $newCfg.Activate();

  $build.Clean($true);
  $build.Build($true);
}
$oldCfg.Activate();

# run nuget pack
foreach ($proj in $dte.Solution) {
  if (!$proj -or !$proj.FullName) { continue; }

  write-host 'Got project ' $proj.FullName;
  $path = split-path -parent $proj.FullName;
  $nuspec = join-path $path 'package.nuspec';
  if (!(Test-Path $nuspec)) {
    write-verbose ('No nuspec found at ' + $nuspec);
    continue;
  }

  write-host 'Generating package for nuspec ' $nuspec;

  nuget pack "$nuspec" -Symbol
}


# vim: set expandtab ts=2 sw=2:
