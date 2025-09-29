# IEnumerator를 Awaitable로 일괄 변환하는 PowerShell 스크립트

param(
    [string]$ProjectPath = "D:\JaeChang\UintyDev\GASPT\GASPT"
)

Write-Host "🔄 Converting IEnumerator to Awaitable pattern..." -ForegroundColor Yellow

# C# 파일 검색
$csFiles = Get-ChildItem -Path $ProjectPath -Filter "*.cs" -Recurse

foreach ($file in $csFiles) {
    $content = Get-Content $file.FullName -Raw
    $originalContent = $content

    # 변환 규칙들
    $content = $content -replace '\[UnityTest\]', '[Test]'
    $content = $content -replace 'public IEnumerator', 'public async void'
    $content = $content -replace 'private IEnumerator', 'private async Awaitable'
    $content = $content -replace 'protected IEnumerator', 'protected async Awaitable'
    $content = $content -replace 'yield return null;', 'await Awaitable.NextFrameAsync();'
    $content = $content -replace 'yield return new WaitForSeconds\(([^)]+)\);', 'await Awaitable.WaitForSecondsAsync($1);'
    $content = $content -replace 'using System\.Collections;', '// using System.Collections; // 제거됨'
    $content = $content -replace 'using UnityEngine\.TestTools;', '// using UnityEngine.TestTools; // 제거됨'

    # using 추가 (없을 경우에만)
    if ($content -notmatch 'using System\.Threading;') {
        $content = $content -replace '(using UnityEngine;)', "$1`nusing System.Threading;"
    }
    if ($content -notmatch 'using System\.Threading\.Tasks;') {
        $content = $content -replace '(using System\.Threading;)', "$1`nusing System.Threading.Tasks;"
    }

    # 변경사항이 있으면 파일 저장
    if ($content -ne $originalContent) {
        Set-Content -Path $file.FullName -Value $content -Encoding UTF8
        Write-Host "✅ Converted: $($file.Name)" -ForegroundColor Green
    }
}

Write-Host "🎉 Conversion completed!" -ForegroundColor Green
Write-Host "Please review the changes and test your code." -ForegroundColor Yellow