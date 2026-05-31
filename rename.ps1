$files = Get-ChildItem -Path . -Recurse -Include *.cs,*.xaml,*.csproj,*.xml,*.json | Where-Object { $_.FullName -notmatch '\\obj\\' -and $_.FullName -notmatch '\\bin\\' -and $_.FullName -notmatch '\\.git\\' }

foreach ($file in $files) {
    $content = Get-Content $file.FullName -Raw
    $newContent = $content -replace 'PhotoBoothLight', 'PhotoboothParty' -replace 'photoboothlight', 'photoboothparty'
    if ($content -ne $newContent) {
        Set-Content -Path $file.FullName -Value $newContent -NoNewline
    }
}
Rename-Item -Path 'PhotoBoothLight.csproj' -NewName 'PhotoboothParty.csproj' -ErrorAction SilentlyContinue
