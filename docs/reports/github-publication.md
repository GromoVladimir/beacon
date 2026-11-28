# Подготовка к публикации GitHub

## Прототип боя T04 и эталоны графики v01 — 28.11.2026

До публикации локальная `main` совпадала с `origin/main` (`3a087876a12ff00fc519626edb630e95674aa85c`). GitHub CLI подтвердил авторизацию и публичную видимость существующего репозитория. В коммит входят прототип боя T04, проверочные ассеты импорта T03a, эталоны и карточки происхождения T03b, тесты и документация.

- Unity Editor 6000.6.10f1, CLI 1.0.1. Итоговый прогон **45/45 EditMode** и **17/17 PlayMode**, без пропусков. В первом прогоне четыре теста не получили события виртуальной клавиатуры. Фикстура `PlayerMovementTests` теперь задаёт доставку тестового ввода независимо от фокуса Game View и восстанавливает настройки при завершении. Игровой код для этого не менялся.
- `Check-Baseline.ps1` — **PASS**, 222 записи Assets и 111 GUID. `Test-Baseline.ps1` — **15/15**, `Test-ApplicationFlow.ps1` — **6/6**. `Test-ArtAnchors.ps1` подтвердил девять спрайтов, 12 стыков, опору E03 и две композиции комнаты.
- Проверены ссылки Markdown, исключения `.gitignore`, поля подписи и распространённые форматы секретов. 16 новых PNG побайтово соответствуют LFS-указателям. В diff 46 строк с концевыми пробелами в сериализованных Unity-файлах пробного импорта. Формат этих файлов сохранён.
- Android Development build `build_023a69baa81c` — **Succeeded**, 397 234 мс, 0 ошибок. Из 974 предупреждений 970 относятся к шейдерам, 3 к IL2CPP и 1 к символам отладки для Diagnostics Data. `Builds/PublicationT04/AshBeacon.apk` содержит 70 893 487 байт. SHA256 `5FF09D81258FA5EA2DC05912CCBDFB733B93A47F9CDDFA7941220829C16511B7`.
- Сборка выполнена в открытом импортированном проекте. Пять предварительно изменённых файлов сериализации URP и ProjectSettings остались локальными и побайтово совпадают со снимком до проверок. Сборка чистого клона и запуск на Samsung S25 Ultra в этот прогон не входили.

## Индикация кнопок T03 и план графики — 28.11.2026

Исходный HEAD совпадал с `origin/main` (`89adc8c5c6f1c637fbfd52319646140f111b8f6a`). GitHub CLI подтвердил доступ и видимость PUBLIC у существующего `GromoVladimir/beacon`. Публикуются индикация кнопок и её регрессионные тесты, согласованный план графики и инструкции запуска.

- Unity Editor 6000.6.10f1, Unity CLI 1.0.1. EditMode **11/11**, PlayMode **9/9**, пропусков нет. При переходе PlayMode соединение CLI кратковременно пропало, итоговый отчёт получен после восстановления связи.
- `Check-Baseline.ps1` — **PASS**, 188 записей Assets и 94 GUID. `Test-Baseline.ps1` — **15/15**, `Test-ApplicationFlow.ps1` — **6/6**. `Verify-T03.cs` подтвердил ссылки Gameplay, отсутствие missing scripts и настройки движения.
- Android Development build `build_481073d7c874` — **Succeeded**, 43 791 мс, 0 ошибок, 1 предупреждение об уровне символов отладки для Diagnostics Data. Сцены Bootstrap, MainMenu, Gameplay. `Builds/Publication20261128/AshBeacon.apk` содержит 70 883 024 байта. SHA256 `FFB3862AF06614FB2512D5A16F4C6EBB84FEDA22FE8FC6BC22B322DCECC7AECE`.
- `.gitignore` исключает APK, отчёты тестов, временные каталоги, локальные сохранения, файлы `.env` и ключи подписи. Проверка распространённых форматов секретов не дала совпадений. `git lfs fsck` прошёл.
- Сборка проверяла текущий импортированный проект. Пять исходных локальных изменений сериализации URP и ProjectSettings сохранены в рабочем каталоге и исключены из коммита. Чистый клон не собирался. На момент сборки T03 была в REVIEW. Решение владельца о приёмке записано в [отчёте T03](T03-controls.md).

## Публикация T03 в REVIEW

Исходный HEAD / origin/main: `686a0327dacd0cff827f7a2eca5c7923a60ebba0`.
Существующий репозиторий PRIVATE, авторизация GitHub CLI исправна; доступ не менялся.
Публикуются текущие ввод/движение/взгляд/пауза, Gameplay, definition, тесты,
инструменты проверки и документы. `docs/art-direction.md` сохраняется как Draft;
публикация не утверждает его новые художественные параметры и не меняет GDD.
T03 остаётся REVIEW до Android-приёмки, T04 не начинается.

- Unity CLI 1.0.0 вне sandbox видит ready Editor 6000.6.10f1, проект
  `C:/Repository/likeZelda`. Подключение MCP в sandbox Editor не обнаружило;
  второй Editor не запускался. Обновления Editor/CLI/пакетов не выполнялись.
- Исходная Console: 0 errors / 0 warnings, compilationFailed=false, cursor 6,
  session `17481b4f94c34a709c0de97fd9087466`.
- Повторные EditMode: **11/11 PASS**. PlayMode: **7/7 PASS**, 0 failed/skipped,
  4,74 с; итог получен через `test status --detail full`.
- `Verify-T03.cs`: **PASS**, missingScripts=0, bindings valid, speed=3,5,
  body=0,65 × 0,65, definition GUID `0ef4039c656da364fbbd58314663dda5`.
- `Test-Baseline.ps1`: **15 сценариев PASS**.
- README содержит точную версию Editor, clone/LFS, Bootstrap и управление T03.
  Локальные Markdown-ссылки, `git diff --check` отслеживаемых изменений: **PASS**.
- Исключения Library/Temp/Logs/Builds/TestResults, .env, ключей подписи/лицензии
  и локальных сохранений проверены через `git check-ignore`. Поиск опасных путей
  и распространённых токенов/закрытых ключей в отслеживаемых и новых файлах:
  совпадений нет. Поля подписи/паролей пусты. Произвольные секреты этим не исключаются.
- До проверок сохранена копия текущих Assets/ProjectSettings в игнорируемом Temp.
  Unity удалила этот временный каталог в ходе проверок. Пять файлов с побочными
  изменениями build были чистыми по исходному git status и восстановлены из HEAD:
  DefaultVolumeProfile, UniversalRP, UniversalRenderPipelineGlobalSettings,
  ProjectSettings, UnityConnectSettings. Исходные изменения T03 не откатывались.
- Полный `Check-Baseline.ps1`: **PASS**, exit 0, 188 Assets entries / 94 GUID.
- Android Development build через CLI: **Succeeded**, build ID `build_cddeb79c7317`,
  107790 мс, 0 ошибок, 971 предупреждение (включая Diagnostics Data/debug symbols
  и шейдеры пакета inference/Sentis). Сцены из Build Settings:
  Bootstrap, MainMenu, Gameplay. Команда `build run --target Android`,
  options `["Development"]`, output `Builds/PublicationT03/AshBeacon.apk`.
  Размер по BuildReport 987792898 байт; сам APK 70883024 байт.
  SHA256 APK: `D288B07FD33620126343B59E640196F1C6DF97F7F4D7D18415EC364B21F8FD52`.
  Полный BuildReport сохранён локально в `TestResults/PublicationT03-build.json`.
  Оба артефакта игнорируются Git и не публикуются.
- После сборки новых ошибок с cursor 6 нет: compilationFailed=false,
  consoleErrors=0, consoleWarnings=81 (свёрнутые); Console не очищалась.
- Проверен импортированный каталог с автоматической сериализацией Editor,
  не чистый checkout итогового коммита. Проверка телефона не выполнена,
  T03 остаётся REVIEW, T06 не считается принятой.
- Финальный staged diff: 39 файлов, LFS fsck OK, вне индекса изменений нет.
  Проверка ссылок PASS. `diff --check` сообщает только три намеренных
  Markdown-переноса (два пробела в конце строки) в черновике art-direction.md;
  код, сцены, meta, скрипты и остальные документы проходят проверку.

Ниже — исторические результаты предыдущих публикаций.

## Публикация каркаса T02

Исходный HEAD / origin/main: `6aa1c85d56ec7d4cb34907fa211c25b8b9bb7c6a`.
GitHub CLI авторизован, существующий репозиторий PRIVATE; доступ не менялся.
Публикуются каркас Bootstrap/MainMenu/Gameplay, Domain/Application, адаптеры,
композиция, тесты, TMP Essentials с лицензией OFL и документация T02.
Существовавшая нормализация URP/Quality и SceneTemplateSettings сохранена.
Manifest/lock и версия Editor не обновлялись.

- Повторный Unity EditMode: **6/6 PASS**, 0 failed/skipped.
- Повторный Unity PlayMode: **3/3 PASS**, 0 failed/skipped, 2,24 с.
  Запрос запуска вернул HTTP 400 при domain reload; итог получен отдельно
  через `test status(detail: full)` после завершения.
- `Test-ApplicationFlow.ps1`: **6 PASS** через Mono; дополнительная проверка ядра.
- `Test-Baseline.ps1`: **15 сценариев PASS**.
- Live `Verify-Baseline.cs`: **PASS**, Editor 6000.6.10f1, Android,
  129 объектов / 87 компонентов / пять сцен без отсутствующих ссылок и скриптов.
- README проверен: запуск через Bootstrap, кнопки перехода и команды тестов.
  Локальные Markdown-ссылки: **PASS**.
- `.gitignore` проверен через `git check-ignore`: секреты, Library/Temp/Logs,
  Builds/TestResults и локальные сохранения исключены. Поиск распространённых
  токенов/закрытых ключей и опасных путей в отслеживаемых и новых файлах:
  совпадений нет. Поля keystore/пароля пусты. Это не гарантия обнаружения любого секрета.
- Первый статический прогон пересёкся с очисткой временной InitTestScene
  после PlayMode и завершился FileNotFoundException; он не считается PASS.
- Повторный `Check-Baseline.ps1` завершился: **PASS**, exit 0,
  166 Assets entries / 83 project GUIDs. Прогон занял несколько минут.
- Android Development build: **Succeeded**, 486031 мс, 0 ошибок,
  982 предупреждения (debug symbols/Diagnostics Data и шейдеры пакетов).
  Build ID `build_4de52eeb6fcb`; сцены Bootstrap, MainMenu, Gameplay.
  Команда MCP `build run`: target Android, options Development,
  output `Builds/PublicationT02/AshBeacon.apk`; размер по BuildReport 987161806 байт.
  SHA256 APK: `50162AE09B370CE4A204F82A1BC7B8B79303C4C4EC62DFDBC5C628301FCA3251`.
  APK исключён из Git. Телефон и чистый checkout не проверены; T06 не принята.
- До проверок сохранена копия Assets/ProjectSettings. После сборки восстановлены
  только пять отличавшихся файлов: DefaultVolumeProfile, UniversalRP,
  UniversalRenderPipelineGlobalSettings, ProjectSettings, UnityConnectSettings.
  Все изменения, существовавшие до публикации, сохранены, включая настройки.
  Сборка проверяла текущий Editor с его автоматической сериализацией;
  отдельная сборка чистого клона итогового коммита не выполнялась.
- Финальный индекс: 147 файлов. LFS-фильтр шрифта и наличие локального объекта
  подтверждены, `git lfs fsck` — OK. Вне индекса изменений нет.
  `git diff --cached --check` для кода/документов/скриптов — PASS;
  общая проверка сообщает 274 замечания форматирования Unity meta/YAML и
  импортированных TMP-файлов (концевые пробелы, отступы, пустые строки в конце).
  Массовое форматирование ассетов и сторонних шейдеров не выполнялось.

Предыдущие разделы ниже — исторические результаты до T02.

## Повторная публикация после T01

Исходный HEAD и origin/main: `37c341582ec2f4240a6c2525315b9aef5e122500`.
Remote существует, GitHub CLI подтвердил PRIVATE; доступ не менялся.
Авторизация исправна вне сетевой песочницы. Публикуются существовавшие изменения
T01: два ассета, валидатор с 15 сценариями, live-скрипт и документация.

- Static baseline: PASS, 22 Assets entries / 11 GUID.
- Test-Baseline: PASS, 15 положительных и отрицательных сценариев.
- Unity MCP Verify-Baseline: PASS, точный проект, Editor 6000.6.10f1,
  Android, 37 объектов / 12 компонентов / две сцены.
- Игровых test asmdef нет; `test list` вернул null. Это не успешный прогон EM/PM.
- Git LFS fsck: OK. Исключения секретов, временных каталогов и сборок проверены
  через git check-ignore; отслеживаемых файлов этих категорий не найдено.
- Поиск распространённых токенов/закрытых ключей в отслеживаемых файлах:
  совпадений нет; проверены пустые поля подписи. Произвольные секреты этим не исключаются.
- README содержит clone/LFS, точный Editor, SampleScene и команды проверок.
  Локальные Markdown-ссылки README/docs: PASS.
- Unity CLI отсутствует в PATH; использован подключённый MCP без установки и обновления.
- Android build через Unity MCP: `run`, target Android, options Development,
  scenes `Assets/Scenes/SampleScene.unity`, output `Builds/PublicationCheck/AshBeacon.apk`.
  Build ID `build_ed2e28fe025d`: **Succeeded**, 852190 мс, 0 ошибок,
  979 предупреждений (Diagnostics Data/debug symbols и предупреждения шейдеров,
  включая Sentis). Размер по BuildReport: 943176498 байт.
  SHA256 APK: `F46495688323DF6E3848A0E86F113CB834767947BAAEA9F4B4FF02D2596DAB80`.
  APK игнорируется Git и не публикуется. Телефон и чистый checkout не проверены.
- Unity при сборке автоматически пересохранила семь ранее чистых файлов:
  DefaultVolumeProfile, UniversalRP, UniversalRenderPipelineGlobalSettings,
  EditorBuildSettings, ProjectSettings, QualitySettings, UnityConnectSettings.
  Эти побочные изменения восстановлены из исходного HEAD после сборки;
  исходные изменения T01 сохранены. Сборка проверяла состояние Editor с его
  автоматической сериализацией, не отдельный чистый checkout итогового коммита.
  Editor/manifest/lock не обновлялись; T06 не считается выполненной.

Ниже сохранён исторический отчёт первой публикации; его BLOCKED/13 сценариев
относятся к исходному коммиту, актуальный статус T01 — DONE.

## Первая публикация

Область: публикация текущих исходников, не приёмка T01 и не выпуск игры.
Существующий origin: https://github.com/GromoVladimir/beacon, PRIVATE, без веток
на момент проверки. Авторизация GitHub CLI подтверждена вне сетевой песочницы.
Настройки доступа не менялись.

## Проверки

- В Git до подготовки не было коммитов; исходные 67 файлов не отслеживались.
- Проверены исключения Library, Temp, Logs, Builds, TestResults, .env, keystore,
  локальных сохранений и лицензий. Добавлены исключения ключей и локальных настроек агента.
- Поиск распространённых форматов GitHub/AWS-токенов и закрытых ключей в публикуемых
  файлах совпадений не дал. Поля подписания/паролей ProjectSettings пустые.
  Это не гарантия обнаружения произвольного секрета.
- README содержит точную версию Editor, запуск SampleScene и команды проверок.
  Локальные Markdown-ссылки README/docs разрешаются.
- `powershell -NoProfile -ExecutionPolicy Bypass -File tools/Test-Baseline.ps1`:
  PASS, 13 сценариев, включая отрицательные.
- В Check-Baseline исправлено вычисление пути по умолчанию для Windows PowerShell 5.
  `powershell -NoProfile -ExecutionPolicy Bypass -File tools/Check-Baseline.ps1`:
  FAIL, 8 неразрешённых GUID в Lit2DSceneTemplate.scenetemplate и Renderer2D.asset.
  Не изменять ссылки по догадке; требуется проверка восстановления пакетов и импорта в T01.
- Unity EditMode/PlayMode не запускались: игровых C#/test asmdef нет.
  Android-сборка не запускалась: build entrypoints ещё не созданы, Editor запущен;
  второй batch Editor над тем же каталогом не запускался. Успех сборки не подтверждён.
- Git LFS установлен, фильтр настроен, в индексе LFS-объектов нет.
- Проверен staged diff: исходники, пакеты с lock, настройки и документы;
  сгенерированные каталоги исключены. Общий diff-check сообщает пробелы в исходных
  Unity YAML и документах; массовое форматирование не выполнялось.
  Для изменённых README, .gitignore и tools diff-check проходит.

T01 остаётся BLOCKED; следующий шаг — проверка Editor/Console/import по спецификации T01.
Публикация исходников не означает готовность к сборке или выпуску Android.
