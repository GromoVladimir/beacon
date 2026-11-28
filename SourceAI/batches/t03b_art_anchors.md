# T03b — эталоны графики v01

Дата отбора 28.11.2026. Набор выбран для следующих генераций «Пепельного маяка». [Спецификация](../../docs/specs/T03b-art-anchors.md) · [цветная комната](../anchors/v01/room_ruin_layout_01.png) · [отчёт](../../docs/reports/T03b-art-anchors.md).

## Источник

Лист создан в ChatGPT с помощью OpenAI ImageGen. Имя версии модели и seed сервис не вернул. Вход состоит из текстового описания по [арт-руководству](../../docs/art-direction.md) и [GDD](../../ash-beacon-game-design.md) проекта. Сторонние изображения, игровые ассеты и шрифты не использовались. Условия использования результата изложены в разделе о правах на выходные материалы OpenAI. [Условия использования](https://openai.com/policies/terms-of-use/) · [Условия для ЕЭЗ, Швейцарии и Великобритании](https://openai.com/policies/eu-terms-of-use/).

Выбранный [исходный лист](../selected/t03b/source_anchor_sheet_01.png), 1254 × 1254 px. SHA256 `72250a016417b3b232415584ef1df03bcb3bb264c05c605a16481c7e3c466fbd`.

### Промпт генерации

> Create original game pixel art production source for Пепельный маяк, a mystical fantasy action adventure in ash-gray stone ruins, slight top-down (see top and front), muted slate stone, teal cape, warm brass, moss green, cyan magic. Output a square contact sheet precisely subdivided into FOUR columns EQUAL width and FOUR rows EQUAL height (4 x 4), each cell background solid pure flat #FF00FF magenta, with ample uninterrupted magenta padding around each foreground object except tiles. No grids, text, labels, logos, shadows outside sprites, floor under character or props, or crossover between cells. Render authentic chunky 32px game pixel art, limit each small sprite to ~25x25 conceptual pixels even though output is high res; no subpixel texture. Exactly these positions (column order left to right): row 1: south-facing ash-grey hooded lighthouse keeper with turquoise cape, slender bright silver sword held in right hand (viewer left), dark brass kite shield in left (viewer right), visible feet and hood opening; compact green mossy Seed Spitter plant monster with 4 squat exposed roots and a walnut-like pod mouth pointed towards viewer; same plant with distinctly inflated spherical seed pod and visible mouth about to fire, same feet; closed warm bronze wooden treasure chest with clearly rectangular body and lock. Row 2: ancient circular brass floor switch with visibly hollow dark center and stone base, no orb; vibrant muted-red heart UI icon; square slate HUD item slot with brass corners, empty dark middle; graphic stylized ice-white cyan crescent sword slash, sharp tapering ends pointing left with the bulging middle facing right. Row 3: impact VFX, short bright jagged amber four-arm star around a white center; single seamless 32 pixel tile of low-contrast charcoal/slate flagstones and faint cracks, presented as a square swatch, identical dark slate outer edges; square top horizontal wall tile, stone masonry with top cap at top, fading to its charcoal floor color at bottom, its right and left edge identical; matching northwest inner wall corner square tile with top and left walls joining smoothly, dark slate floor in lower right. Row 4 left two and right two cells are empty pure #FF00FF. The silhouette of each of the first 9 sprites must be fully isolated against pure magenta. Keep hue magenta exclusive to background. Use original designs and no visual IP references.

## Выбранные файлы

Координаты ячеек даны как строка и столбец, начиная с 1. Для спрайтов выделен рисунок в ячейке, удалён пурпурный фон, рисунок приведён точечной выборкой к кадру 32 × 32 px. У Хранителя и E03 нижняя опора едина, рекомендуемая точка привязки — (16, 3) от нижнего левого угла. У тайлов рекомендована привязка к центру клетки. Взмах в исходнике направлен вправо и повёрнут вниз в композиции. У попадания точка привязки в центре.

| Файл v01 | Роль | Источник |
|---|---|---|
| [player_keeper_idle_south_01.png](../anchors/v01/player_keeper_idle_south_01.png) | Хранитель, стойка лицом на юг, меч и щит | 1, 1 |
| [enemy_seed_spitter_idle_01.png](../anchors/v01/enemy_seed_spitter_idle_01.png) | E03, стойка | 1, 2 |
| [enemy_seed_spitter_tell_01.png](../anchors/v01/enemy_seed_spitter_tell_01.png) | E03, предупреждение перед выстрелом | 1, 3 |
| [prop_ruin_chest_closed_01.png](../anchors/v01/prop_ruin_chest_closed_01.png) | Закрытый сундук | 1, 4 |
| [prop_ruin_ring_idle_01.png](../anchors/v01/prop_ruin_ring_idle_01.png) | Кольцо для бумеранга, покой | 2, 1 |
| [ui_hud_heart_01.png](../anchors/v01/ui_hud_heart_01.png) | Полное сердце | 2, 2 |
| [ui_hud_slot_01.png](../anchors/v01/ui_hud_slot_01.png) | Пустой слот предмета | 2, 3 |
| [vfx_sword_slash_01.png](../anchors/v01/vfx_sword_slash_01.png) | Дуга меча | 2, 4 |
| [vfx_combat_hit_01.png](../anchors/v01/vfx_combat_hit_01.png) | Вспышка попадания | 3, 1 |
| [env_ruin_tileset_01.png](../anchors/v01/env_ruin_tileset_01.png) | Лист 192 × 32 px. Слева направо пол, северная стена, северо-западный угол, западная стена, восточная стена, северо-восточный угол | 3, 2–4. Кромки согласованы попиксельно, боковые стены получены транспонированием и зеркальным отражением, восточный угол — отражением западного |
| [room_ruin_layout_01.png](../anchors/v01/room_ruin_layout_01.png) | Комната 20 × 12 в масштабе 1:1 | Композиция из выбранных кадров и тайлов |
| [room_ruin_layout_gray_01.png](../anchors/v01/room_ruin_layout_gray_01.png) | Та же комната без цвета | Преобразование исходной комнаты по яркости |
| [reference_sprite_contrast_01.png](../anchors/v01/reference_sprite_contrast_01.png) | Девять кадров рядом на тёмном и светлом фоне, 288 × 64 px | Композиция из выбранных кадров |

Результаты AT-17/19 приведены в [отчёте](../../docs/reports/T03b-art-anchors.md). Для T06a использовать конкретные файлы v01 нужного семейства вместе с композицией комнаты для сверки масштаба. Серии кадров и рабочие Unity-импорты появятся при подключении игровых пакетов.
