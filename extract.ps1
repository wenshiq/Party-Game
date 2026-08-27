Add-Type -AssemblyName System.IO.Compression.FileSystem
$zip = [System.IO.Compression.ZipFile]::OpenRead('D:\xinjian\Odyssey\resume.docx')
$entry = $zip.GetEntry('word/document.xml')
$reader = New-Object System.IO.StreamReader($entry.Open())
$xml = $reader.ReadToEnd()
$reader.Close()
$zip.Dispose()
$xml = $xml -replace '</w:p>', "`n"
$texts = [regex]::Matches($xml, '<w:t[^>]*>(.*?)</w:t>')
$sb = New-Object System.Text.StringBuilder
foreach ($m in $texts) { [void]$sb.Append($m.Groups[1].Value) }
$out = $sb.ToString()
$out = $out -replace '&amp;','&' -replace '&lt;','<' -replace '&gt;','>'
[System.IO.File]::WriteAllText('D:\xinjian\Odyssey\resume_text.txt', $out, (New-Object System.Text.UTF8Encoding($false)))
Write-Output "done"
