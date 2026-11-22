# Подготовка к публикации GitHub — 22.11.2026

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
