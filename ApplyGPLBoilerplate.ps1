Param
(
	[Switch] $verifyOnly, # No changes will be made if set to true
	[Switch] $verbose     # Additional output provided if set to true
)

function GetFileContent([string] $path)
{
	[String]::Join($newLine, (Get-Content -Path $path))
}

function GetUserName()
{
	$strComputer = "."
	$Var = GWMI -Comp $strComputer -CL Win32_ComputerSystem
	return $Var.UserName
}

function Recurse([string] $path)
{
	$files = Get-ChildItem -Path $path
	if ($files -ne $null) # Did we find anything?
	{
		foreach ($file in $files)
		{
			# Is it a folder?
			if ($file -isnot [System.IO.DirectoryInfo])
			{
				$extension = $file.Extension.ToLower()
				if ($licencesByExtension.ContainsKey($extension) -and $file.Name -notmatch $excludedFilePattern)
				{
					$fileContent = GetFileContent $file.FullName
					$relativeName = $file.FullName.Replace($startPath, "")
					$firstLicenseLineReplace = [string]::Format($firstLicenseLine, $file.Name)					
					if (-not $fileContent.Contains($firstLicenseLineReplace))					
					{	
						$filesToUpdate.Add($relativeName) | Out-Null
						if (-not $verifyOnly)
						{
							echo ("Updating " + $relativeName)
							([string]::Format($licencesByExtension[$file.Extension.ToLower()], $file.Name, $copyrightDate, $userName, $date) + $fileContent) | Set-Content $file.FullName
							# $file.Name
							# $licencesByExtension[$file.Extension.ToLower()]
						}
					}
					else
					{
						if ($verbose) 
						{
							# Not sure we need this
							#echo ("File " + $file.FullName + " is up to date")
						}
					}
				}
				else
				{
					if ($verbose) 
					{
						# Not sure we need this
						#echo ("Skipping file " + $file.FullName + " because extension " + $file.Extension + " has not associated license" )
					}
				}
			}
			else
			{
				if ($file.Name -match $excludedFolderPattern)
				{
					if ($verbose)
					{
						$skippedFolders.Add($file.FullName.Replace($startPath, "")) | Out-Null
					}
				}
				else
				{
					Recurse $file.FullName
				}
			}
		}
	}	
}

$newLine = [Environment]::NewLine
$userName = GetUserName
$copyrightDate = (Get-Date -format yyyy)
$date = (Get-Date -format d)

$licenseFile = "./OxTail/COPYRIGHT.txt"
$licenseContent = GetFileContent $licenseFile
# We need the first line of the license to search for it in each file
$firstLicenseLine = (Get-Content $licenseFile)[0].Replace("// ", "")

# For each extension a license must be specified
$licencesByExtension = @{}
$licencesByExtension[".cs"] = ($licenseContent + $newLine + $newLine)

## We don't really want most, if any, xml files processed.
## Quite a few of them belong to third party libraries or are configuration
## markup.  Maybe we could turn it on selectively for a few of them.  --Jeff.
## $licencesByExtension[".xml"] = ("<!--" + $newLine + $licenseContent + "-->" + $newLine + $newLine)

# Special folder patterns to exclude
$excludedFolderPattern = "^(bin|obj|Web References|Logs|Images|Config|Resources|SharpSSH)$"

# Special file patterns can also be excluded
$excludedFilePattern = "Designer.cs$"

$filesToUpdate = New-Object System.Collections.ArrayList
$skippedFolders = New-Object System.Collections.ArrayList

cls
if ($verifyOnly) { echo ("** Verify Only **" + $newLine) }

"Processing..."
$startPath = gl # Need to save it for later
$startPath | Recurse

if ($verifyOnly)
{
	""
	"Username: " + $userName
	"Copyright date: " + $copyrightDate
	"Date: " + $date
	""
	if($filesToUpdate.Count -gt 0)
	{
		"The following files need to be updated:"
		""
		foreach ($file in $filesToUpdate)
		{
			echo ("  " + $file)
		}
	}
	else
	{
		echo "** All files are up to date **"
	}
}
else
{
	if($filesToUpdate.Count -eq 0)
	{
		echo "** All files are up to date **"
	}
}
if ($verbose)
{
	if($skippedFolders.Count -gt 0)
	{
		""
		"Skipped folders:"
		""
		foreach ($folder in $skippedFolders)
		{
			echo ("  " + $folder)
		}
	}
}
""
