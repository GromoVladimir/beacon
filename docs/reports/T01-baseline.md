# T01 — проверка исходного окружения

22.11.2026. Статус: **BLOCKED**, критерии T01 не выполнены полностью.
Текущий этап S0 не принят. T02–T06 не начаты из-за зависимости от T01.
Основание: [спецификация](../specs/T01-baseline.md), NFR-05/NFR-07, GDD §17–18.
Коммит отсутствует: ветка main ещё не имеет HEAD, все исходники неотслеживаемые.

## Изменения

| Файл | Результат |
|---|---|
| `tools/Check-Baseline.ps1` | Статическая проверка версии, сериализации, manifest/lock, meta/GUID, ссылок и LFS pointers без изменения проекта |
| `tools/Test-Baseline.ps1` | Изолированные положительные/отрицательные сценарии валидатора |
| `docs/specs/T01-baseline.md` | Объём, критерии, ошибки, риски и ограничения |
| `docs/reports/T01-baseline.md` | Данный отчёт |
| `docs/tasks.md` | T01 BLOCKED, конкретная блокировка и ссылки на доказательства |
| `docs/decisions.md` | Уточнён текущий статус начала T01, принятые решения сохранены |
| `docs/testing.md` | Команды статической проверки, границы результата |
| `README.md` | Актуальный следующий шаг и состояние T01 |

Игровой код, Assets, Packages, ProjectSettings, версии и настройки не редактировались.
Перед правками сделана временная копия исходных файлов; коммиты, push и merge не выполнялись.

## Окружение

| Компонент | Наблюдение |
|---|---|
| Editor | ProjectVersion: 6000.6.10f1, revision 40e81493c67d; та же версия в EditorInstance и журнале проекта |
| Путь Editor | `C:\Program Files\Unity\Hub\Editor\6000.6.10f1\Editor\Unity.exe` |
| Android Build Support | Каталог AndroidPlayer и UnityEditor.Android.Extensions.dll присутствуют |
| NDK | 27.2.12479018, r27c (`NDK/source.properties`) |
| OpenJDK | Temurin 17.0.21+9 (`OpenJDK/release`) |
| SDK Build Tools | 36.0.0 |
| SDK platforms | android-34, android-36, android-37.0; наличие не означает выбранный release API |
| SDK command-line tools | 21.0 |
| URP / Input / UI / Tests | manifest: 17.6.0 / 1.20.1 / 2.6.0 / 1.8.0 |
| Pipeline | manifest уже содержит 1.0.1; в этой задаче не добавлялся |
| Serialization | m_SerializationMode: 2 (Force Text) |
| Version Control | m_Mode: Visible Meta Files |
| Git LFS | 3.6.1; filter.lfs.process = git-lfs filter-process; PNG → lfs, сцена → без LFS-фильтра |
| Unity CLI | 1.0.0; уведомляет о 1.0.1; обновление не выполнялось |

Наличие файлов SDK/NDK/JDK не доказывает успешный Android build или выбранные в Editor
внешние пути инструментов. Платформа Android ранее выбрана владельцем; live-проверка
activeBuildTarget пока недоступна. Лицензионные данные не извлекались.

## Выполненные проверки

- `tools/Check-Baseline.ps1`: **FAIL**, обнаружены восемь неразрешённых GUID-ссылок
  (см. ниже). Проверки версии, Force Text, Visible Meta Files, manifest/lock, meta,
  дубликатов GUID и отсутствующих LFS-файлов не выдали других ошибок.
- `tools/Test-Baseline.ps1`: **13 сценариев PASS**. Валидная fixture, неверный Editor,
  Binary serialization, Hidden Meta Files, drift manifest/lock, пропущенная meta,
  дубликат GUID, битая ссылка, ссылка на embedded package, LFS pointer, orphan meta,
  некорректный GUID и восстановленная fixture. Проверки не требуют Unity или новых пакетов.
- `git check-ignore`: Library, Temp, Logs, Builds, TestResults, .env и Signing исключены.
- `git check-attr`: существующее правило PNG использует LFS, сцены остаются текстовыми.
- `git diff --check`: exit 0, но исходные файлы неотслеживаемые, поэтому эта команда
  сама по себе не проверяет содержимое новых файлов.
- PowerShell parser: оба скрипта без синтаксических ошибок; локальные Markdown-ссылки
  в docs разрешаются. SHA256 manifest/lock/ProjectVersion повторно совпали в конце задачи.
- Журнал `Logs/Editor.log`: содержит завершённые Asset Pipeline Refresh; фильтр
  `error CS`, `Scripts have compiler errors`, `Error importing`, `Failed to import`,
  `MissingReferenceException` не вернул совпадений. Исторический журнал не доказывает
  текущее состояние Console и отсутствие Missing Script в сцене.

Контрольные SHA256 зафиксированных входов:

| Вход | SHA256 |
|---|---|
| Packages/manifest.json | `7455E65DC374FAAA70B096238C0E119AED50976A008AD0E8EDFF1408242F46C0` |
| Packages/packages-lock.json | `0CFEBF62C742A37ED881AC1BBB1F2781B268DF048354392F85B82129D29B7392` |
| ProjectSettings/ProjectVersion.txt | `506DE9A75CCE597F4B897DEACE2DE3E4A1958BC71B788BEC3630D82620D996C3` |

## Блокировка и невыполненные проверки

Статический поиск в Assets и 73 импортированных пакетах (22 709 meta) не разрешил:

| Ассет | GUID |
|---|---|
| Assets/Settings/Lit2DSceneTemplate.scenetemplate | `b3ab972b21db85d48ade9657efdd4771` |
| Assets/Settings/Renderer2D.asset | `e5c6678ed2aaa91408dd3df699057aae` |
| Assets/Settings/Renderer2D.asset | `03cfc4915c15d504a9ed85ecc404e607` |
| Assets/Settings/Renderer2D.asset | `53a11f4ebaebf4049b3638ef78dc9664` |
| Assets/Settings/Renderer2D.asset | `8f96cd657dc40064aa21efcc7e50a2e7` |
| Assets/Settings/Renderer2D.asset | `57d7c4c16e2765b47a4d2069b311bffe` |
| Assets/Settings/Renderer2D.asset | `24ec0e140fb444a44ab96ee80844e18e` |
| Assets/Settings/Renderer2D.asset | `5688ab254e4c0634f8d6c8e0792331ca` |

Это кандидаты на отсутствующие ссылки, а не доказанные дефекты renderer: метаданные
встроенных ресурсов Editor не входят в область поиска. Через AssetDatabase и Inspector
нужно установить, являются ли ссылки встроенными, устаревшими или повреждёнными.
GUID и renderer по догадке не заменялись. Проверка полного кэша занимает несколько минут.

`unity status --format json` обнаружил этот проект, Editor 6000.6.10f1, порт 7800,
но вернул `STATUS_ALL_UNREACHABLE` и предупреждение о невозможности проверить запущенные
процессы. `unity command console status --caller plugin --skill unity-cli --project-path
C:\Repository\likeZelda --format json --timeout 15` завершился с `EDITOR_TIMEOUT`.
Safe Mode по доступным данным не подтверждён; ограничение доступа среды возможно,
но точная причина не установлена. Второй Editor не запускался, безопасность среды
не ослаблялась, сцены вручную вместо live-проверки не редактировались.

`git lfs ls-files` не показал объектов; `git lfs fsck` не прошёл: `Git can't resolve ref:
"HEAD"`. Это не доказательство повреждения LFS и не успешная проверка истории.
Чистый checkout/import не проверялся. Unity EditMode/PlayMode не запускались: игровых
test asmdef ещё нет. Android APK и проверки телефона относятся к T06 и не выполнялись.

Для снятия блокировки нужен доступ к открытому Editor этого проекта: проверить
завершение импорта, ошибки Console, Missing Script и ссылки SampleScene, Android target
и пути Android tools. При недоступности CLI эти пункты можно проверить вручную в Editor
и записать конкретные результаты. Затем повторить статическую проверку и принять T01.
До этого T01 не DONE, переход к T02 и следующему этапу не выполняется.
