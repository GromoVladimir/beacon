# Подготовка к публикации GitHub — 22.11.2026

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
