# update-database.ps1
# Applies all pending EF Core migrations for every module.
# Run from the solution root: .\Scripts\update-database.ps1

$ErrorActionPreference = "Stop"
$solutionRoot = Split-Path -Parent $PSScriptRoot
$startupProject = "GuitarStore.ApiGateway"

$modules = @(
    @{ Project = "Catalog.Infrastructure" },
    @{ Project = "Customers.Infrastructure" },
    @{ Project = "Orders.Infrastructure" },
    @{ Project = "Warehouse.Core" },
    @{ Project = "Auth.Core" },
    @{ Project = "Payments.Core" },
    @{ Project = "Common.Outbox" }
)

Push-Location $solutionRoot

try {
    foreach ($module in $modules) {
        $project = $module.Project
        Write-Host "`n==> $project" -ForegroundColor Cyan
        dotnet ef database update `
            --project $project `
            --startup-project $startupProject

        if ($LASTEXITCODE -ne 0) {
            Write-Error "Migration failed for $project (exit code $LASTEXITCODE)"
        }
    }

    Write-Host "`nAll migrations applied successfully." -ForegroundColor Green
}
finally {
    Pop-Location
}
