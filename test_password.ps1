# Test password verification
$password = "test123"
$storedHash = "FjwlVlbTTG7ub8Xsq31Ubq6LlSryeFkYGhNC3rE9/cQ=:dGVzdHNhbHQxMjM0NTY3ODkwMTIzNDU2Nzg5MDEyMzQ1Njc4OTA="

# Split the hash
$parts = $storedHash.Split(':')
$hashPart = $parts[0]
$saltPart = $parts[1]

Write-Host "Password: $password"
Write-Host "Stored Hash: $hashPart"
Write-Host "Salt: $saltPart"

# Recreate the hash
$salt = [Convert]::FromBase64String($saltPart)
$pbkdf2 = New-Object System.Security.Cryptography.Rfc2898DeriveBytes($password, $salt, 10000, [System.Security.Cryptography.HashAlgorithmName]::SHA256)
$computedHash = $pbkdf2.GetBytes(32)
$computedHashString = [Convert]::ToBase64String($computedHash)

Write-Host "Computed Hash: $computedHashString"
Write-Host "Hashes Match: $($hashPart -eq $computedHashString)"

# Test with wrong password
$wrongPassword = "wrong123"
$pbkdf2Wrong = New-Object System.Security.Cryptography.Rfc2898DeriveBytes($wrongPassword, $salt, 10000, [System.Security.Cryptography.HashAlgorithmName]::SHA256)
$wrongHash = $pbkdf2Wrong.GetBytes(32)
$wrongHashString = [Convert]::ToBase64String($wrongHash)
Write-Host "Wrong Password Hash: $wrongHashString"
Write-Host "Wrong Password Match: $($hashPart -eq $wrongHashString)"