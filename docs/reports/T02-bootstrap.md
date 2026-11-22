# T02 — Bootstrap, меню и проверочная комната

22.11.2026. Статус: **DONE**. [Спецификация](../specs/T02-bootstrap.md).
Исходная точка: чистый HEAD `6aa1c85`. T03 не начата; S0 целиком ещё не принят.

## Реализовано

Bootstrap создаёт один постоянный корень с явной композицией и загружает MainMenu.
«Начать прототип» открывает Gameplay, «В главное меню» освобождает игровую сессию.
Повторный старт создаёт новую сессию. Повторный вход через Bootstrap сохраняет
единственного владельца сервисов и возвращает в меню. Глобального singleton нет.

ApplicationFlow принимает ISceneLoader, блокирует повторные команды при загрузке,
публикует сессию только после успешного завершения. Ошибка сохраняет прежнее
состояние/сессию и допускает повтор. UI показывает сообщение при ошибке перехода.
Тестируемость ошибок порта не означает проверку аварийного восстановления геометрии
произвольной повреждённой сцены; это не контракт переходов между комнатами T07.

Gameplay содержит пустую комнату 20 × 12 и четыре BoxCollider2D-стены.
Ортографическая камера сохраняет всю комнату в кадре при изменении соотношения сторон.
Меню на uGUI/TMP, ввод UI через InputSystemUIInputModule; строки в сценах.
Нет героя, движения, боя, сохранений, продолжения или удаления прогресса.
FR-01 реализовано только в объёме каркаса, полная приёмка требования впереди.

## Изменённые файлы

| Файлы / каталог (включая новые .meta) | Назначение |
|---|---|
| `Assets/AshBeacon/Domain/GameSession.cs`, `AshBeacon.Domain.asmdef` | Идентичность одной временной сессии; noEngineReferences |
| `Assets/AshBeacon/Application/ApplicationFlow.cs`, `AshBeacon.Application.asmdef` | Состояние приложения, порт загрузки, защита от повторов; noEngineReferences |
| `Assets/AshBeacon/UnityAdapters/UnitySceneLoader.cs`, `NavigationView.cs`, `RoomCamera.cs`, asmdef | Загрузка сцен, кнопки, камера |
| `Assets/AshBeacon/Composition/BootstrapRoot.cs`, asmdef | Сборка зависимостей и единственный владелец жизненного цикла |
| `Assets/AshBeacon/Tests/EditMode/`, `Tests/PlayMode/` | Шесть тестов ядра и три интеграционных теста, отдельные test asmdef |
| `Assets/AshBeacon/UI/PrototypeFont.asset`, `PrototypeFloor.mat`, `PrototypeWall.mat` | Кириллица и простое представление комнаты |
| `Assets/Scenes/Bootstrap.unity`, `MainMenu.unity`, `Gameplay.unity` | Три новые сцены, созданы через Editor |
| `Assets/TextMesh Pro/` | Essentials из установленного пакета, без примеров; основание — текст меню T02 |
| `ProjectSettings/EditorBuildSettings.asset` | Три игровые сцены, Bootstrap первой; SampleScene сохранена на диске |
| `tools/Create-T02Scenes.cs` | Воспроизводимое создание новых сцен через Editor; отказывается перезаписывать существующие |
| `tools/Test-ApplicationFlow.ps1` | Дополнительный запуск тех же чистых тестов через Mono; не заменяет Unity Test Runner |
| `AGENTS.md`, `README.md`, `docs/tasks.md`, `architecture.md`, `requirements.md`, `roadmap.md`, `testing.md`, `decisions.md` | Актуальный статус, запуск и границы T02 |
| `docs/specs/T02-bootstrap.md`, этот отчёт | Спецификация и доказательства приёмки |

TMP Essentials импортированы из установленного uGUI, manifest/lock не обновлялись.
Шрифт Liberation Sans из Essentials, лицензия OFL сохранена в
`Assets/TextMesh Pro/Fonts/LiberationSans - OFL.txt`. Создан статический атлас
используемых русских строк; сетевые и платные сервисы не добавлялись.

При возобновлении работы обнаружены изменения UniversalRP.asset, ProjectSettings.asset,
QualitySettings.asset и новый SceneTemplateSettings.json. Они не являются реализацией
T02, не откатывались и не включаются в её функциональный объём. В diff PlayerSettings
есть define symbols существующих пакетов; URP/Quality содержат нормализацию Editor.
Их наличие не означает согласованного изменения рендеринга или игровых правил.

## Проверки

| Проверка | Результат |
|---|---|
| Unity EditMode, фильтр Assembly `AshBeacon.EditModeTests` | **6/6 PASS**, 0 failed/skipped, 0,82 с |
| Unity PlayMode, фильтр Assembly `AshBeacon.PlayModeTests` | **3/3 PASS**, 0 failed/skipped, 2,23 с |
| Live `script_eval(file: tools/Verify-Baseline.cs)` | **PASS**, Unity 6000.6.10f1 / Android; 129 объектов, 87 компонентов, 5 сцен без Missing Script/reference |
| Ручной запуск и интерфейс | Владелец подтвердил результат по предложенному чек-листу сообщением «Всё супер» |
| Актуальная Console после тестов | 0 errors / 0 warnings, compilationFailed=false, compiling=false; cursor 1109 |
| Локальные Markdown-ссылки, `git diff --check` | **PASS**; Git сообщает только о будущей нормализации LF → CRLF |
| Manifest / lock / ProjectVersion | Хеши совпадают с зафиксированными в отчёте T01 |
| Повторный `Check-Baseline.ps1` после импорта TMP | Не завершился за несколько минут, остановлен; текущий static PASS не заявляется. Следующий `Test-Baseline.ps1` в той же команде результата не вернул. Приёмка ссылок основана на завершённой live-проверке Editor, не на этом прогоне |

EditMode: полный цикл, повторная/неуместная команда, ошибка старта с повтором,
ошибка возврата без потери сессии, отсутствие порта, отсутствие Unity-зависимостей
в скомпилированных Domain/Application. PlayMode: реальная кнопка и двойное нажатие,
комната/камера, возврат и новая сессия, повторный Bootstrap из Gameplay и меню,
один BootstrapRoot/EventSystem.

Запуск: открыть Bootstrap и нажать Play. В Test Runner выполнить EditMode и PlayMode
по сборкам выше. Через MCP: `test run` с `mode`, `filter-type: Assembly`, `filter`,
`async-tests: true`; после domain reload получить итог через `test status(detail: full)`.
HTTP 400 при перезагрузке домена не считается результатом тестов: итоговые PASS
получены отдельно. XML и результаты CI этой задачей не заявляются.

## Исправления и ограничения

Исправлены недостающий namespace для await AsyncOperation, устаревшие API поиска
объектов и конфликт playModeStartScene с Test Runner. Первый PlayMode-прогон
отменён; после восстановления окружения получен успешный полный результат.
Console не очищалась. Исторические записи исправленной ошибки компиляции и предупреждений
могут оставаться в буфере MCP; актуальный compilationFailed=false.

Проверен текущий импортированный проект. Android APK нового каркаса, управление,
lifecycle и чистая сборка не проверялись — соответствующие задачи T03/T06.
Сборки Infrastructure/Editor заранее не созданы, поскольку реализации для них
в T02 нет. Коммиты, push, merge и публикация не выполнялись.

Следующая доступная задача — T03, только по отдельному поручению.
