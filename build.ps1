[CmdletBinding(DefaultParametersetName='None')] 
param(
    [Parameter(Mandatory=$false)]
    [Switch]
    $Pack,

    [Parameter(ParameterSetName='publish', Mandatory=$false)]
    [Switch]
    $Publish,

    [Parameter(Mandatory=$false)]
    [Switch]
    $Clean,

    [Parameter(Mandatory=$false)]
    [Switch]
    $SkipSamples,

    [Parameter(Mandatory=$false)]
    [string]
    $Configuration = 'Release',

    [Parameter(ParameterSetName='publish', Mandatory=$false)]
    [string]
    $PushSource = 'https://api.nuget.org/v3/index.json',

    [Parameter(ParameterSetName='publish', Mandatory=$true)]
    [string]
    $PushApiKey
)

if ($Publish -eq $true) {
    $Pack = $true
}

if ($Clean -eq $true) {
    dotnet clean -c $Configuration
    if ($LASTEXITCODE -ne 0) {
        exit 1
    }
}

dotnet tool restore
if ($LASTEXITCODE -ne 0) {
    exit 1
}

$Version = dotnet tool run dotnet-gitversion -showvariable NuGetVersion
if ($LASTEXITCODE -ne 0) {
    exit 1
}
Write-Host $Version

dotnet build -c $Configuration -p:Version=$Version --force
if ($LASTEXITCODE -ne 0) {
    exit 1
}

dotnet format --verify-no-changes --no-restore
if ($LASTEXITCODE -ne 0) {
    exit 1
}

dotnet test -c $Configuration --no-build
if ($LASTEXITCODE -ne 0) {
    exit 1
}

if (-not $SkipSamples) {
    dotnet test .\samples\InjectedTests.Samples.sln -c $Configuration
    if ($LASTEXITCODE -ne 0) {
        exit 1
    }
}

if ($Pack -eq $true) {
    Remove-Item * -Recurse -Include *.nupkg

    dotnet pack -c $Configuration -p:Version=$Version --no-build
    if ($LASTEXITCODE -ne 0) {
        exit 1
    }
}

if ($Publish -eq $true) {
    dotnet nuget push "src\**\*.$Version.nupkg" -s $PushSource -k $PushApiKey
    if ($LASTEXITCODE -ne 0) {
        exit 1
    }
}
