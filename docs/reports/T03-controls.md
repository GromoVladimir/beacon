# T03 — ввод, движение, взгляд, пауза

22.11.2026. Статус: **REVIEW**, не DONE. Исходная точка — чистый `686a032`. [Спецификация](../specs/T03-controls.md). T04 не начата; push/merge/публикация не выполнялись.

## Результат

В Gameplay появился герой: скорость 3,5 клетки/с, тело 0,65 × 0,65, нормализация диагонали, четыре направления взгляда с гистерезисом 15° и сохранением последнего взгляда. Движение через Dynamic Rigidbody2D, без гравитации/вращения/трения; Continuous collision. Проверены скольжение вдоль стены, упор в угол и движение обратно.

WASD/стрелки и фиксированный touch-стик используют один `PlayerControls`. J/«Меч» передают нажатие и удержание; K/«Предмет» и E/«Действие» — отдельные команды. Буфер 0,12 с симуляции потребляется один раз. Пока нет получателя боевых команд: HP, урон, анимация меча, щит и повтор ударов реализуются в T04. M/I, настоящий инвентарь и выбор экипировки появятся со своими задачами; панель выбора T03 только владеет паузой.

Esc/«Пауза» и Tab/«Выбор предмета» независимо останавливают симуляцию. Потеря фокуса и фон имеют собственные блокировки; снятие одной не снимает остальные. При блокировке сбрасываются скорость, буфер и пальцы; клавиатуре нужна нейтраль перед новым вводом. UI работает при timeScale=0. Выход из Gameplay/отключение контроллера возвращает прежний timeScale и освобождает InputActionMap/PhysicsMaterial2D. Состояние героя принадлежит текущей Gameplay, reset policy — RoomRuntimeState.

Параметры берутся из `PlayerDefinition.asset`, runtime его не меняет. Touch-панель вписана в Screen.safeArea. Временный герой выделен отдельным оранжевым материалом и маркером взгляда. Пакеты, Editor, боевые слои и сцены Bootstrap/MainMenu не изменены.

## Изменённые файлы

Все новые файлы Assets импортированы Editor и имеют `.meta`.

| Область | Файлы |
|---|---|
| Чистые правила | `Assets/AshBeacon/Domain/PlayerMovement.cs`, `Assets/AshBeacon/Application/PlayerControls.cs` |
| Unity-ввод/физика/UI | `Assets/AshBeacon/UnityAdapters/GameplayController.cs`, `PlayerMotor.cs`, `PlayerDefinition.cs`, `TouchControl.cs`, `SafeAreaView.cs`; `AshBeacon.UnityAdapters.asmdef` |
| Контент | `Assets/Scenes/Gameplay.unity`, `Assets/AshBeacon/PlayerDefinition.asset`, `Assets/AshBeacon/UI/PrototypePlayer.mat` |
| Тесты | `Assets/AshBeacon/Tests/EditMode/PlayerControlsTests.cs`, `Assets/AshBeacon/Tests/PlayMode/PlayerMovementTests.cs`, `AshBeacon.PlayModeTests.asmdef` |
| Воспроизводимость | `tools/Configure-T03.cs` (однократная настройка ещё не изменённой T02 Gameplay), `tools/Verify-T03.cs` (повторяемая проверка сцены) |
| Документы | `AGENTS.md`, `README.md`, `docs/{tasks,architecture,requirements,roadmap,decisions,testing}.md`, spec и этот отчёт |

## Проверки

Unity Editor **6000.6.10f1**, активный проект `C:\Repository\likeZelda`, Pipeline через MCP. CLI 1.0.0 из sandbox не достиг Editor (`STATUS_ALL_UNREACHABLE`), MCP подтвердил ready и правильную версию. Доступно обновление CLI 1.0.1; не устанавливалось. Исходный курсор Console 2111, сессия `0c18b01fe38b4864b749768637a3d755`.

| Проверка | Результат |
|---|---|
| Компиляция | PASS: compilationFailed=false |
| EditMode | **11/11 PASS**, 0 failed/skipped: 6 регрессий T02 + 5 новых тестов правил |
| PlayMode, итоговый прогон | **7/7 PASS**, 0 failed/skipped, 5,12 с: 3 регрессии T02 + 4 новых интеграционных теста |
| Сцена/definition | `Verify-T03.cs` PASS: 1 controller, ссылки назначены, 4 touch-контрола, missing scripts=0, speed=3,5, body=0,65 × 0,65 |
| Негативные сценарии | Просроченный/повторно потреблённый ввод; чужой pointer-up; удержание при потере фокуса/фоне; несколько pause owners; удержанная клавиатура после resume; выход из угла; выгрузка во время паузы |
| Валидатор baseline: тесты | **15/15 PASS** (`tools/Test-Baseline.ps1`) |
| Полный статический baseline | **PASS** после завершения Test Runner: 188 записей Assets, 94 GUID. Первые конкурентные запуски пересеклись с удалением временной InitTestScene; они не засчитывались |
| Документация / diff | Ссылки README/AGENTS/docs проверены; `git diff --check` PASS |
| Console после тестов | groundTruth: 0 errors, 0 warnings, compiling=false, compilationFailed=false; старые накопительные счётчики буфера не очищались |
| Android | **НЕ ПРОВЕРЕНО**: ADB daemon не запускается (`could not read ok from ADB Server`, `cannot connect to daemon`). APK T02 не является проверкой T03 |
| Android Development build T03 | **Succeeded**, 0 errors, 982 warnings; build ID `build_15237fbea362`, 657450 мс, размер BuildReport 987792898 байт. `Builds/Android/AshBeacon-T03.apk` |

Итоговый PlayMode JSON сохранён локально в `TestResults/T03-playmode.json` (игнорируется Git). EditMode — результат live Test Runner. XML/CI entrypoints остаются в T06, не добавлялись в T03.

APK T03 собран через MCP `build run`: Android, options Development, явные Bootstrap/MainMenu/Gameplay. Сам APK — **70883024 байт**; размер BuildReport выше учитывает все выходные файлы. Он игнорируется Git, не публиковался и на телефоне не запускался. 982 предупреждения разобраны: 1 Diagnostics Data/debug symbols, 978 предупреждений шейдеров встроенного пакета Sentis (integer divides/modulus, signed/unsigned), 3 сообщения о выделении больших методов TextMeshPro в отдельные IL2CPP cpp-файлы. Это те же категории и количество, что в сборке T02, см. [отчёт публикации](github-publication.md); предупреждений из `Assets/AshBeacon` нет. После build Console: 0 ошибок, 92 свёрнутых предупреждения; до build после тестов было 0/0. Служебные изменения сборки в PlayerSettings/UnityConnectSettings и автоматическое пересохранение DefaultVolumeProfile/URP исключены из итогового diff; пакеты и настройки проекта не меняются задачей T03.

В промежуточном PM-прогоне сравнение пройденного пути оказалось зависимым от числа физических шагов между render-кадрами. Тест исправлен: измеряет путь за реально прошедшее `Time.fixedTimeAsDouble` после установления ввода. После исправления полный PM прошёл. При domain reload MCP иногда возвращал HTTP 400/Invalid response; итог определялся по завершённому `test status` и его summary, а не по ответу запуска. Последний Editor status: ready, PlayMode stopped.

## Как воспроизвести

1. Открыть Bootstrap и нажать Play → «Начать прототип». Герой в центре комнаты, стрелка взгляда внизу.
2. WASD/стрелками или левым стиком пройти к стенам, скользить по ним, зайти в угол и выйти обратным движением. Диагональ не должна быть быстрее прямой; после отпускания взгляд сохраняется.
3. Правым пальцем держать «Меч», левым двигаться; отпускать по очереди. Это проверяет удержание ввода; атаки ещё нет.
4. Esc/кнопкой открыть паузу; Tab/кнопкой открыть выбор предмета. Закрытие только одной панели не запускает мир. После закрытия обеих нужен новый ввод.
5. Не закрывая паузу, нажать «В главное меню» и снова запустить прототип: новая позиция/взгляд, время не заморожено.
6. В Test Runner запустить EditMode и PlayMode. Затем, при остановленном PlayMode, выполнить `script eval` файла `tools/Verify-T03.cs`. Не запускать Configure-T03 повторно на уже настроенной сцене.

## Осталось для приёмки T03

На Samsung S25 Ultra записать версию Android/разрешение и проверить:

- Одновременные стик + меч, независимое отпускание и повторные касания, смену пальцев.
- Четыре стены/угла: нет проникновения, застревания или остаточного движения после отпускания.
- Меню/выбор предмета, Home/возврат, блокировка/разблокировка: мир стоит при блокировке, UI работает, удержания не восстанавливаются сами.
- Safe area и видимость комнаты на 20:9; отдельно формат 16:9. Контролы не перекрывают критические объекты, направление читается.

До записи результата этих сценариев T03 остаётся REVIEW, FR-02/03 и NFR-04 не объявлены полностью принятыми. Следующая задача T04 автоматически не начинается.
