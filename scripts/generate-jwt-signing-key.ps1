# Generates a 2048-bit RSA private key in PKCS#8 PEM ("BEGIN PRIVATE KEY") — the format
# authservice's Jwt__PrivateKeyPem expects. Pure .NET, so it runs in any PowerShell with
# no openssl installed.
#
#   scripts/generate-jwt-signing-key.ps1                     # prints to stdout
#   scripts/generate-jwt-signing-key.ps1 -Path key.pem
#
# Generated, never invented. A key generated for local development is a development
# convenience, not a trust root — never promote one to a deployed environment.

param(
    [string]$Path
)

$rsa = [System.Security.Cryptography.RSA]::Create(2048)
try {
    $pem = $rsa.ExportPkcs8PrivateKeyPem()
}
finally {
    $rsa.Dispose()
}

if ($Path) {
    Set-Content -Path $Path -Value $pem -NoNewline
    Write-Host "Wrote $Path"
}
else {
    Write-Output $pem
}
