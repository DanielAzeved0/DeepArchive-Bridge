$ErrorActionPreference = "Stop"

$root = Split-Path -Parent $MyInvocation.MyCommand.Path
$env:DOTNET_CLI_HOME = Join-Path $root ".dotnet-home"
$env:NUGET_PACKAGES = Join-Path $root ".nuget-packages"
$env:DOTNET_SKIP_FIRST_TIME_EXPERIENCE = "1"
$env:DOTNET_CLI_TELEMETRY_OPTOUT = "1"
$backendTests = Join-Path $root "backend\src\DeepArchiveBridge.Tests\DeepArchiveBridge.Tests.csproj"

function Invoke-Checked {
  param(
    [Parameter(Mandatory = $true)]
    [string] $Label,
    [Parameter(Mandatory = $true)]
    [scriptblock] $Command
  )

  Write-Host $Label
  & $Command
  if ($LASTEXITCODE -ne 0) {
    throw "$Label failed with exit code $LASTEXITCODE"
  }
}

Write-Host "Restoring backend packages..."
Invoke-Checked "Restoring backend packages..." {
  dotnet restore $backendTests --configfile (Join-Path $root "NuGet.config")
}

Invoke-Checked "Building backend..." {
  dotnet build $backendTests --no-restore --disable-build-servers /p:UseSharedCompilation=false
}

Invoke-Checked "Running backend tests..." {
  dotnet test $backendTests --no-build --no-restore --disable-build-servers /p:UseSharedCompilation=false
}

Push-Location (Join-Path $root "frontend")
try {
  Invoke-Checked "Checking frontend types..." {
    npm.cmd run type-check
  }

  Invoke-Checked "Building frontend..." {
    npm.cmd run build
  }
}
finally {
  Pop-Location
}

Write-Host "All checks passed."
