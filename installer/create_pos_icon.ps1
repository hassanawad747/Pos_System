param([string]$InstallRoot='C:\BikeZonePOS\App',[string]$ShortcutName='Bike Zone POS')
$ErrorActionPreference = 'Stop'
Add-Type -AssemblyName System.Drawing

$installRoot = $InstallRoot
$exePath = Join-Path $installRoot 'Pos_System.exe'
$iconPath = Join-Path $installRoot 'BikeZonePOS.ico'
$shortcutPath = Join-Path ([Environment]::GetFolderPath('Desktop')) ($ShortcutName+'.lnk')

if (-not (Test-Path $installRoot)) { exit 0 }

$size = 128
$bitmap = New-Object System.Drawing.Bitmap $size, $size
$graphics = [System.Drawing.Graphics]::FromImage($bitmap)
$graphics.SmoothingMode = [System.Drawing.Drawing2D.SmoothingMode]::AntiAlias
$graphics.TextRenderingHint = [System.Drawing.Text.TextRenderingHint]::AntiAliasGridFit

try {
    $graphics.Clear([System.Drawing.Color]::Transparent)

    $rect = New-Object System.Drawing.Rectangle 4,4,120,120
    $path = New-Object System.Drawing.Drawing2D.GraphicsPath
    $radius = 28
    $diameter = $radius * 2
    $path.AddArc($rect.X,$rect.Y,$diameter,$diameter,180,90)
    $path.AddArc($rect.Right-$diameter,$rect.Y,$diameter,$diameter,270,90)
    $path.AddArc($rect.Right-$diameter,$rect.Bottom-$diameter,$diameter,$diameter,0,90)
    $path.AddArc($rect.X,$rect.Bottom-$diameter,$diameter,$diameter,90,90)
    $path.CloseFigure()

    $bgBrush = New-Object System.Drawing.Drawing2D.LinearGradientBrush(
        $rect,
        [System.Drawing.Color]::FromArgb(15,23,42),
        [System.Drawing.Color]::FromArgb(37,99,235),
        45.0)
    $graphics.FillPath($bgBrush,$path)

    $cartPen = New-Object System.Drawing.Pen ([System.Drawing.Color]::White), 7
    $cartPen.StartCap = [System.Drawing.Drawing2D.LineCap]::Round
    $cartPen.EndCap = [System.Drawing.Drawing2D.LineCap]::Round
    $graphics.DrawLine($cartPen,29,38,39,38)
    $graphics.DrawLine($cartPen,39,38,48,76)
    $graphics.DrawLine($cartPen,48,76,91,76)
    $graphics.DrawLine($cartPen,47,52,98,52)
    $graphics.DrawLine($cartPen,98,52,90,76)

    $wheelBrush = New-Object System.Drawing.SolidBrush ([System.Drawing.Color]::FromArgb(52,211,153))
    $graphics.FillEllipse($wheelBrush,48,87,13,13)
    $graphics.FillEllipse($wheelBrush,82,87,13,13)

    $barcodePen = New-Object System.Drawing.Pen ([System.Drawing.Color]::FromArgb(191,219,254)), 3
    foreach ($x in @(57,63,69,76,84)) { $graphics.DrawLine($barcodePen,$x,57,$x,69) }

    $font = New-Object System.Drawing.Font 'Segoe UI', 14, ([System.Drawing.FontStyle]::Bold)
    $textBrush = New-Object System.Drawing.SolidBrush ([System.Drawing.Color]::White)
    $graphics.DrawString('BZ',$font,$textBrush,16,91)

    $hIcon = $bitmap.GetHicon()
    try {
        $icon = [System.Drawing.Icon]::FromHandle($hIcon)
        $stream = [System.IO.File]::Create($iconPath)
        try { $icon.Save($stream) } finally { $stream.Dispose() }
    }
    finally {
        Add-Type @"
using System;
using System.Runtime.InteropServices;
public static class NativeIconCleanup { [DllImport("user32.dll", SetLastError=true)] public static extern bool DestroyIcon(IntPtr hIcon); }
"@ -ErrorAction SilentlyContinue
        [NativeIconCleanup]::DestroyIcon($hIcon) | Out-Null
    }
}
finally {
    if ($graphics) { $graphics.Dispose() }
    if ($bitmap) { $bitmap.Dispose() }
    if ($bgBrush) { $bgBrush.Dispose() }
    if ($cartPen) { $cartPen.Dispose() }
    if ($wheelBrush) { $wheelBrush.Dispose() }
    if ($barcodePen) { $barcodePen.Dispose() }
    if ($font) { $font.Dispose() }
    if ($textBrush) { $textBrush.Dispose() }
    if ($path) { $path.Dispose() }
}

if ((Test-Path $shortcutPath) -and (Test-Path $iconPath) -and (Test-Path $exePath)) {
    $shell = New-Object -ComObject WScript.Shell
    $shortcut = $shell.CreateShortcut($shortcutPath)
    $shortcut.TargetPath = $exePath
    $shortcut.WorkingDirectory = $installRoot
    $shortcut.IconLocation = "$iconPath,0"
    $shortcut.Save()
}

Write-Host "Bike Zone POS branded icon ready: $iconPath" -ForegroundColor Green
