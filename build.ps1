$csproj = Get-Content "LatestDirs.csproj" -Raw
$versionMatch = [regex]::Match($csproj, '<Version>(.*)</Version>')
if ($versionMatch.Success) {
    $currentVersion = [version]$versionMatch.Groups[1].Value
    $newVersion = "$($currentVersion.Major).$($currentVersion.Minor).$($currentVersion.Build + 1)"
    $csproj = $csproj -replace "<Version>.*</Version>", "<Version>$newVersion</Version>"
    Set-Content "LatestDirs.csproj" $csproj
    Write-Host "Updated version to $newVersion"
}

dotnet pack -c Release -o ./nupkg
