$commands = Get-ChildItem -Path FlowProtocol2/Commands -Filter 'Cmd*.cs' -File
$out = @()
foreach ($f in $commands) {
    $lines = Get-Content $f.FullName
    $line = $lines | Where-Object { $_ -like '*return new CommandParser(@"*' } | Select-Object -First 1
    if ($line) {
        $lineParts = $line -split '@"'
        $pat = ''
        if ($lineParts.Count -gt 1) { $rest = $lineParts[1]; $pat = ($rest -split '"')[0] }
        $count = 0
        try {
            $matches = Select-String -Path 'Scripts\**\*' -Pattern $pat -AllMatches -ErrorAction SilentlyContinue
            if ($matches) { $count = ($matches | ForEach-Object { $_.Matches.Count } | Measure-Object -Sum).Sum }
        } catch { $count = 0 }
        $out += ([PSCustomObject]@{ Command = $f.BaseName; Count = [int]$count; Pattern = $pat })
    }
}
$out | Sort-Object -Property Count -Descending | ForEach-Object { "{0}|{1}|{2}" -f $_.Command, $_.Count, $_.Pattern } | Out-File -FilePath command_counts.txt -Encoding utf8
Write-Output 'OK'