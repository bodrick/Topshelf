# Set active path to script-location:
$path = $MyInvocation.MyCommand.Path
if (!$path) { $path = $psISE.CurrentFile.Fullpath }
if ($path) { $path = Split-Path $path -Parent }
Set-Location $path

dotnet clean -c Release

# Building for packing and publishing.
dotnet pack -c Release --output "$path/artifacts"
