# T01 — проверка исходного окружения

22.11.2026. Статус: **DONE**. Все критерии спецификации T01 выполнены.
Этап S0 целиком ещё не принят; T02–T06 в этой работе не начаты.
Основание: [спецификация](../specs/T01-baseline.md), NFR-05/NFR-07, GDD §17–18.
Точка восстановления перед изменениями: чистый коммит `37c3415`.

## Изменённые файлы

| Файл | Результат |
|---|---|
| `Assets/Settings/Renderer2D.asset` | Через Editor очищены шесть отсутствующих устаревших APV-ссылок; при сохранении удалены поля старого формата, включая отсутствующий m_FallOffLookup |
| `Assets/Settings/Lit2DSceneTemplate.scenetemplate` | Удалена отсутствующая зависимость; встроенный preview сохранён, формат нормализован Editor |
| `tools/Verify-Baseline.cs` | Воспроизводимая read-only проверка объектов, ссылок и сцен живого Editor |
| `tools/Check-Baseline.ps1` | GUID пакетов читается из заголовка meta без загрузки всего importer payload |
| `tools/Test-Baseline.ps1` | 15 сценариев, включая ссылку в PackageCache и отсутствующую ссылку при заполненном кэше |
| `docs/tasks.md`, `docs/specs/T01-baseline.md` | Критерии, подтверждённый статус и границы исправления baseline |
| `docs/testing.md`, `docs/decisions.md`, `README.md` | Команды проверки и актуальный следующий шаг |
| `docs/reports/T01-baseline.md` | Итоговый отчёт |

Исходные GUID и meta сохранены. Сцены, Packages и ProjectSettings не изменены.
Игровые системы, новые пакеты, коммиты и push не добавлялись.
Сохранение двух ассетов выполнено через SerializedObject / SaveAssetIfDirty;
массовое пересохранение проекта не выполнялось. Поля APV помечены Obsolete в
установленном URP `Runtime/Deprecated.cs` (перенесены в ProbeVolumeDebugResources).
AssetDatabase подтвердил, что все восемь прежних GUID не разрешаются.
Шесть из них очищены в неиспользуемых полях, седьмой удалён сериализацией устаревшего
поля, восьмой — отсутствующая зависимость шаблона. Подстановка случайных GUID не применялась.

## Окружение и критерии приёмки

| Критерий | Подтверждение |
|---|---|
| Точный проект / Editor | Live MCP: `C:/Repository/likeZelda/Assets`, Unity 6000.6.10f1, ready, compiling=false, updating=false |
| Импорт / ссылки / скрипты | Static PASS; live PASS: 37 объектов, 12 компонентов, обе сцены без Missing Script и отсутствующих сериализованных ссылок |
| Android | activeBuildTarget=Android, IsBuildTargetSupported=true |
| Android tools | Live SDK/NDK/JDK указывают на AndroidPlayer установленного Editor 6000.6.10f1 |
| Версии tools | NDK 27.2.12479018 (r27c), OpenJDK 17.0.21, SDK Build Tools 36.0.0; platforms android-34/36/37.0 |
| Пакеты | URP 17.6.0, Input System 1.20.1, uGUI 2.6.0, Tests 1.8.0; manifest/lock без изменений |
| Сериализация | Live: ForceText, Visible Meta Files |
| LFS | 3.6.1, git-lfs filter-process; fsck OK; ls-files пуст, неразвёрнутых pointers нет |
| Результаты / ограничения | Записаны здесь; T01 DONE, приёмка S0 и сборка не заявляются |

## Проверки

- `powershell -NoProfile -ExecutionPolicy Bypass -File tools/Check-Baseline.ps1`: **PASS**, exit 0; 22 Assets entries, 11 project GUIDs.
- `powershell -NoProfile -ExecutionPolicy Bypass -File tools/Test-Baseline.ps1`: **15 сценариев PASS**, exit 0: валидный набор, встроенная ссылка, неверный Editor, serialization, meta visibility, package drift, missing/orphan/invalid meta, duplicate GUID, broken reference, embedded/cache package, LFS pointer, восстановление fixture.
- `script_eval(file: tools/Verify-Baseline.cs)`: **PASS**, проверены `Assets/Scenes/SampleScene.unity` и `Assets/Settings/Scenes/URP2DSceneTemplate.unity`. Preview scenes закрываются без сохранения рабочих сцен.
- `git lfs fsck`: **OK**. PNG использует LFS; сцены остаются текстовыми. Library/Temp/Logs/Builds/TestResults/.env/Signing игнорируются.
- Хеши manifest/lock/ProjectVersion совпадают с исходными (см. ниже).
- Финальная Console: **0 errors / 0 warnings**, compilationFailed=false, compiling=false; новых ошибок с cursor 8 нет (session 0c18b01fe38b4864b749768637a3d755).
- Локальные Markdown-ссылки и синтаксис PowerShell: **PASS**. `git diff --check`: исправлен пробел, добавленный сериализатором в пустое поле шаблона; повторная проверка без ошибок.

Контрольные SHA256 неизменённых входов:

| Вход | SHA256 |
|---|---|
| Packages/manifest.json | `7455E65DC374FAAA70B096238C0E119AED50976A008AD0E8EDFF1408242F46C0` |
| Packages/packages-lock.json | `0CFEBF62C742A37ED881AC1BBB1F2781B268DF048354392F85B82129D29B7392` |
| ProjectSettings/ProjectVersion.txt | `506DE9A75CCE597F4B897DEACE2DE3E4A1958BC71B788BEC3630D82620D996C3` |

## Ограничения и диагностика среды

CLI 1.0.0 по-прежнему возвращает STATUS_ALL_UNREACHABLE, хотя Unity MCP работает
с тем же проектом. Доступно обновление CLI 1.0.1, оно не выполнялось.
Первый script_eval превысил 30 секунд; повторная read-only диагностика с timeout 60
успешна. Диагностический фрагмент с objectReferenceInstanceIDValue не скомпилировался:
в этой версии требуется objectReferenceEntityIdValue; итоговый скрипт исправлен.
Это не ошибка компиляции проекта. Console не очищалась.

Проверен текущий импортированный каталог, не новый чистый клон. Android APK,
EditMode/PlayMode игровых правил и телефон не проверялись: игровых asmdef ещё нет,
сборка и мобильная проверка относятся к T06. Полная приёмка NFR-05 не закрыта одной T01.
Следующая задача — T02; автоматически не начата.
