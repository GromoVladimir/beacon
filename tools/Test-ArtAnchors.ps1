[CmdletBinding()]
param(
    [string]$AnchorPath
)

$ErrorActionPreference = 'Stop'
Add-Type -AssemblyName System.Drawing
if (-not $AnchorPath) { $AnchorPath = Join-Path $PSScriptRoot '../SourceAI/anchors/v01' }

function Open-Png {
    param([string]$Name, [int]$Width, [int]$Height)
    $bitmap = [Drawing.Bitmap]::new((Join-Path $AnchorPath "$Name.png"))
    if ($bitmap.Width -ne $Width -or $bitmap.Height -ne $Height -or
        $bitmap.RawFormat.Guid -ne [Drawing.Imaging.ImageFormat]::Png.Guid) {
        $bitmap.Dispose()
        throw "$Name must be a PNG at ${Width}x${Height}."
    }
    $bitmap
}

function Get-SpriteBounds {
    param([string]$Name)
    $bitmap = Open-Png $Name 32 32
    try {
        $left = 32; $top = 32; $right = -1; $bottom = -1
        for ($y = 0; $y -lt 32; $y++) {
            for ($x = 0; $x -lt 32; $x++) {
                $color = $bitmap.GetPixel($x, $y)
                if ($color.A -eq 0) { continue }
                if ($color.A -ne 255 -or $x -eq 0 -or $y -eq 0 -or $x -eq 31 -or $y -eq 31) {
                    throw "$Name has a translucent pixel or touches the cell edge at ($x,$y)."
                }
                if ($color.R -gt 115 -and $color.B -gt 90 -and
                    $color.R -gt ($color.G + 65) -and $color.B -gt ($color.G + 45) -and
                    $color.B -gt ($color.R * 0.65)) {
                    throw "$Name contains the magenta background at ($x,$y)."
                }
                $left = [Math]::Min($left, $x)
                $top = [Math]::Min($top, $y)
                $right = [Math]::Max($right, $x)
                $bottom = [Math]::Max($bottom, $y)
            }
        }
        if ($right -lt $left) { throw "$Name is empty." }
        [Drawing.Rectangle]::FromLTRB($left, $top, ($right + 1), ($bottom + 1))
    }
    finally {
        $bitmap.Dispose()
    }
}

function Assert-TileSeam {
    param(
        [Drawing.Bitmap]$Atlas,
        [int]$First,
        [int]$Second,
        [ValidateSet('Right', 'Down')][string]$Direction
    )
    for ($i = 0; $i -lt 32; $i++) {
        if ($Direction -eq 'Right') {
            $a = $Atlas.GetPixel(($First * 32 + 31), $i)
            $b = $Atlas.GetPixel(($Second * 32), $i)
        }
        else {
            $a = $Atlas.GetPixel(($First * 32 + $i), 31)
            $b = $Atlas.GetPixel(($Second * 32 + $i), 0)
        }
        if ($a.ToArgb() -ne $b.ToArgb() -or $a.A -ne 255) {
            throw "Tile seam $First -> $Second ($Direction) differs at pixel $i."
        }
    }
}

$spriteNames = @(
    'player_keeper_idle_south_01', 'enemy_seed_spitter_idle_01', 'enemy_seed_spitter_tell_01',
    'prop_ruin_chest_closed_01', 'prop_ruin_ring_idle_01', 'ui_hud_heart_01', 'ui_hud_slot_01',
    'vfx_sword_slash_01', 'vfx_combat_hit_01'
)
$bounds = @{}
foreach ($name in $spriteNames) { $bounds[$name] = Get-SpriteBounds $name }
$idle = $bounds['enemy_seed_spitter_idle_01']
$tell = $bounds['enemy_seed_spitter_tell_01']
if ($idle.Bottom -ne $tell.Bottom -or $tell.Width -le $idle.Width -or $tell.Height -le $idle.Height) {
    throw 'The E03 tell must visibly expand while keeping the same baseline.'
}
($comparison = Open-Png 'reference_sprite_contrast_01' 288 64).Dispose()

# Atlas order: floor, north, northwest, west, east, northeast.
$atlas = Open-Png 'env_ruin_tileset_01' 192 32
try {
    $horizontalSeams = @(@(0, 0), @(1, 1), @(2, 1), @(1, 5), @(3, 0), @(0, 4))
    $verticalSeams = @(@(0, 0), @(1, 0), @(2, 3), @(5, 4), @(3, 3), @(4, 4))
    foreach ($pair in $horizontalSeams) { Assert-TileSeam $atlas $pair[0] $pair[1] Right }
    foreach ($pair in $verticalSeams) { Assert-TileSeam $atlas $pair[0] $pair[1] Down }
}
finally {
    $atlas.Dispose()
}

$room = Open-Png 'room_ruin_layout_01' 640 384
try {
    $gray = Open-Png 'room_ruin_layout_gray_01' 640 384
    try {
        for ($y = 0; $y -lt $room.Height; $y++) {
            for ($x = 0; $x -lt $room.Width; $x++) {
                $source = $room.GetPixel($x, $y)
                $pixel = $gray.GetPixel($x, $y)
                $luminance = (54 * $source.R + 183 * $source.G + 19 * $source.B) -shr 8
                if ($source.A -ne 255 -or $pixel.A -ne 255 -or
                    $pixel.R -ne $luminance -or $pixel.G -ne $luminance -or $pixel.B -ne $luminance) {
                    throw "The grayscale room differs from the color room at ($x,$y)."
                }
            }
        }
    }
    finally {
        $gray.Dispose()
    }
}
finally {
    $room.Dispose()
}
Write-Output 'PASS: 9 sprites, 12 tile seams, E03 baseline and 2 room previews.'
