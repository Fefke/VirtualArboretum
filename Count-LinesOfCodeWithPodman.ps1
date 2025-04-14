
function Invoke-CodeCount ($mount) {
    podman run --rm -v $mount aldanial/cloc .
}

function Invoke-TestCodeCount () {
    Write-Host "Du hast die Testumgebung ausgewählt."
    Invoke-CodeCount ("{0}/VirtualArboretumTests:/tmp" -f $PSScriptRoot)
}

function Invoke-ProdCodeCount () {
    Write-Host "Du hast die Produktivumgebung ausgewählt."
    Invoke-CodeCount ("{0}/VirtualArboretum:/tmp" -f $PSScriptRoot)
}

Write-Host "In welcher Umgebung möchtest du arbeiten?"
Write-Host "prod - Zähle Produktiv-Code Zeilen."
Write-Host "test - Zähle Test-Code Zeilen."

$environment = Read-Host "Bitte gib 'test' oder 'prod'✔️ ein"

switch ($environment.ToLower()) {
    "test" {
        Invoke-TestCodeCount    
    }
    "prod" {
        Invoke-ProdCodeCount    
    }
    default {
        Invoke-ProdCodeCount    
    }
}
