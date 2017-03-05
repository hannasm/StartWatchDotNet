$solutionDir = split-path $dte.Solution.Properties('Path').Value -parent;
$scriptPath = split-path $SCRIPT:MyInvocation.MyCommand.Path -parent;
function publish($package) {
	if (!(test-path $package)) {
		throw ('Unable teo find nuget package ' + $package + ' for publishing');
	}
	nuget push $package;
}

$metadataPath = join-path $solutionDir 'metadata.xml';
if (!(test-path $metadataPath)) {
	throw ('unable to find solution metadata at ' + $metadataPath);
}
$metadata = [xml](get-content $metadataPath);
$metadata = $metadata.solution;
if (!$metadata) {
	throw ('metadata file  at ' + $metadataPath + ' is missing root solution node');
}

$version = $metadata.version;

$packages = @();
$errors = $false;
# run nuget pack
foreach ($proj in $dte.Solution) {
  if (!$proj -or !$proj.FullName) { continue; }

  $path = split-path -parent $proj.FullName;
  $nuspec = join-path $path 'package.nuspec';
  if (!(Test-Path $nuspec)) {
    write-verbose ('No nuspec found at ' + $nuspec);
    continue;
  }

  $nuspecData = [xml](get-content $nuspec);
  $name = $nuspecData.package.metadata.id + '.' + $nuspecDAta.package.metadata.version + '.nupkg';
  $pkg = (join-path $solutionDir $name);

  if (!(Test-PAth $pkg)) {
	write-error ('Unable to find nuget package at ' + $pkg);
	$errors = $true;
  }
  $packages += $pkg;
}

if ($errors) {
  write-error 'One or more errors ocurred while trying to publish';
  return;
}

foreach ($pkg in $packages) {
	nuget push $pkg;
}