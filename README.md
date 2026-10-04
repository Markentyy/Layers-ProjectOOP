# Layers - шари CLI / Core / Infra

![build](https://github.com/Markentyy/Layers-ProjectOOP/actions/workflows/build.yml/badge.svg)
![dotnet](https://img.shields.io/badge/.NET-8.0-512BD4?logo=dotnet&logoColor=white)
![csharp](https://img.shields.io/badge/C%23-12-239120?logo=csharp&logoColor=white)
![license](https://img.shields.io/badge/license-MIT-green)
![last-commit](https://img.shields.io/github/last-commit/Markentyy/Layers-ProjectOOP)

Навчальний проєкт: той самий командний інтерфейс для персонажів і тексту,
перекладений на шарувату структуру CLI / Core / Infra. Кожен шар - окремий
проєкт зі своїми моделями, залежності тільки вниз, частини замінні:
дисплей, формат сейвів і презентери міняються без торкання команд і домену.
Героїв і зброю можна стягувати з віддаленої бази (Genshin API) і
вербувати в гру.

## Зміст

- [Можливості](#можливості)
- [Технології](#технології)
- [Структура](#структура)
- [Швидкий старт](#швидкий-старт)
- [Тестування](#тестування)
- [Сумісність з версіями .NET](#сумісність-з-версіями-net)
- [Діаграма класів](#діаграма-класів)
- [Ролі класів](#ролі-класів)
- [Як замінити частину](#як-замінити-частину)
- [Автор](#автор)
- [Ліцензія](#ліцензія)

## Можливості

Все з попереднього шелла (режими `--text` / `--chars`, діалог, 10 команд,
id і імена, сувора перевірка аргументів) плюс нове по шарах:

* **Presenter** - весь вивід і діалоги винесено з команд: `CharsPresenter`,
  `TextPresenter`, `Prompter`. Команди вирішують що, презентер - як.
* **Display (Infra)** - консоль за інтерфейсом `IDisplay`; заміна -
  один клас.
* **Data (Infra)** - стан обох світів серіалізується в JSON
  (`save <file>` / `load <file>` в кожному режимі): персонажі з HP,
  спорядженням і книгами; документ деревом з id. Формат міняється
  реалізацією `ICharsStore` / `ITextStore`.
* **Core без змін API** - для відновлення стану лише `internal RestoreState`
  і `InternalsVisibleTo("Cli")`; публічна поверхня та сама.
* **WebApi (Infra)** - герої і зброя з віддаленої бази Genshin
  (`https://genshin.jmp.blue`, JSON через `HttpClient`): `db` - список,
  `recruit` - вербовка з кидком статів за рідкістю, `fetch` - імпорт зброї,
  `show` - лист персонажа як структурований текст.

## Технології

| Технологія | Версія / примітка |
|---|---|
| C# | 12 |
| .NET (таргет) | 8.0 (`net8.0`, `RollForward LatestMajor`) |
| .NET SDK для збірки | 8 або новіший |
| JSON | `System.Text.Json` для сейвів |
| xUnit | 140 тестів: `Tests.Core` (39), `Tests.Infra` (14), `Tests.Cli` (87) |
| PlantUML | діаграма класів (`docs/`) |
| CI | GitHub Actions (Ubuntu + Windows): збірка, тести, прогін 4 демо |

## Структура

```
Layers.sln / Layers.slnx
src/Core/      - домен: GameSystem (Character, Equipment, Inventory,
                 Ability, CombatResolver), TextSystem (TextElement, Section,
                 Heading, Paragraph, Link, TextDocument)
src/Infra/     - Display (IDisplay, ConsoleDisplay), Data (DTO, ICharsStore /
                 ITextStore, JsonCharsStore / JsonTextStore), WebApi
                 (IGenshinApiClient, GenshinApiClient, DTO, WebApiException)
src/Cli/       - Engine (парсер, команди, REPL), Presenter (Prompter,
                 CharsPresenter, TextPresenter), Chars (Registry, CharsWorld,
                 маппери, команди create/add/act/ls/save/load/db/recruit/fetch/show),
                 Text (TextNavigator, TextSeed, маппер, лист персонажа,
                 команди)
src/App/       - Program.cs: композиційний корінь, вибір режиму
src/Tests.Core/  - 39 тестів домену: бій, інвентар, елементи, документ
src/Tests.Infra/ - 14 тестів: сховища і WebApi-клієнт (раундтріп, помилки)
src/Tests.Cli/   - 87 тестів: парсер, реєстри, команди через FakeDisplay,
                   навігація, маппери (включно з Genshin), REPL
demo/          - chars-demo, text-demo, save-chars, save-text, recruit-demo
docs/          - діаграма класів (.puml + .png) і звіт (.docx)
```

Залежності проєктів тільки вниз: App -> Cli -> {Core, Infra}, Infra -> Core.

## Швидкий старт

Потрібен [.NET 8 SDK](https://dotnet.microsoft.com/download) або новіший.

```bash
dotnet build Layers.sln
dotnet run --project src/App -- --chars
dotnet run --project src/App -- --text
```

Демо-прогони (так перевіряє CI):

```bash
dotnet run --project src/App --no-build -- --chars < demo/chars-demo.txt
dotnet run --project src/App --no-build -- --text < demo/text-demo.txt
dotnet run --project src/App --no-build -- --chars < demo/save-chars.txt
dotnet run --project src/App --no-build -- --text < demo/save-text.txt
```

Живе демо з базою (потрібен інтернет, в CI не ганяється):

```bash
dotnet run --project src/App --no-build -- --chars < demo/recruit-demo.txt
```

Сейви пишуться JSON поруч (`demo/saves/`, ігноряться гітом):

```
> save demo/saves/chars.json
Saved to 'demo/saves/chars.json'.
> load demo/saves/chars.json
Loaded from 'demo/saves/chars.json': 1 characters, 1 items, 1 abilities.
```

## Тестування

```bash
dotnet test Layers.sln
```

Три тестові проєкти йдуть за шарами: `Tests.Core` перевіряє доменну
логіку (бій, інвентар, рендер, зміст), `Tests.Infra` - JSON-сховища на
тимчасових файлах (раундтріп і помилки), `Tests.Cli` - парсер, реєстри,
навігацію, маппери і команди. Замість моків - ручний `FakeDisplay`:
черга вводу і захоплений вивід без зайвих залежностей. CI ганяє тести
на Ubuntu і Windows при кожному пуші.

## Сумісність з версіями .NET

* Збірка: .NET 8 SDK або новіший (класичний `Layers.sln` читають усі).
* Запуск: рантайм .NET 8, 9 або 10 через `RollForward LatestMajor`.
* Рантайми старіші за 8.0 не підійдуть.

## Діаграма класів

Система показана чотирма фігурами: ядро інтерпретатора, режими зі
сховищами, домени Core, тести.

![Ядро: парсер, команди, презентери, дисплей](docs/diagram-engine.png)

![Режими: світи, маппери, команди, JSON-сховища](docs/diagram-modes.png)

![Інфра: дисплей, файлові сховища, WebApi-клієнт](docs/diagram-infra.png)

![Домени Core: персонажі і текст](docs/diagram-core.png)

![Тести: Core, Infra, CLI](docs/diagram-tests.png)

Повний звіт з ролями класів: `docs/Layers_Report.docx`.

## Ролі класів

| Клас | Роль |
|---|---|
| `IDisplay` / `ConsoleDisplay` | Infra: консоль за абстракцією, замінна реалізація |
| `ICharsStore` / `JsonCharsStore`, `ITextStore` / `JsonTextStore` | Infra: сейви, формат замінний, домену не знають (лише DTO) |
| `IShellCommand` / `CommandSet` / `Repl` / `LineParser` | Engine: контракт команд, реєстр, цикл, парсер |
| `CharsPresenter` / `TextPresenter` / `Prompter` | Presenter: весь вивід і діалоги, команди чисті від форматування |
| `Registry` / `CharsWorld` | Моделі режиму: id, пошук, книги здібностей |
| `TextNavigator` | Модель режиму: позиція, шляхи, нумерація |
| `CharsWorldMapper` / `TextMapper` | Міст світ-DTO, відновлення через internal API Core |
| `Character` / `Ability` / `Section` / `TextDocument` | Core: домен без змін публічного API |
| `GenshinApiClient` / `GenshinMapper` / `CharacterSheet` | WebApi: HTTP-клієнт, маппінг за рідкістю, лист персонажа текстом |

## Віддалена база

Обрано рекомендований простий інтерфейс: `https://genshin.jmp.blue`
(JSON, без авторизації). База косметична - статів у персонажів немає,
тому характеристики генеруються випадково за рідкістю:

| Рідкість | HP | ATK | ARM |
|---|---|---|---|
| 5 зірок | 90-120 | 20-30 | 3-6 |
| 4 зірки | 60-90 | 12-20 | 1-4 |
| інша | 40-70 | 8-14 | 0-2 |

Таланти стають здібностями за типом: burst - x3, skill - x2, решта - x1.
Зброя має власні стати: бонус атаки = `baseAttack / 10`, бонус броні =
рідкість. Лист персонажа (`show`) - це `Section`-документ: заголовок,
опис, таланти і спорядження.

## Як замінити частину

* Інший вивід: реалізувати `IDisplay` і передати в `Program.cs` замість
  `ConsoleDisplay` - команди і презентери не чіпати.
* Інший формат сейвів: реалізувати `ICharsStore` / `ITextStore` і
  підмінити `JsonCharsStore` / `JsonTextStore` в `Program.cs`.
* Нова команда: клас з `IShellCommand` + один `Add()` у фабриці режиму.
* Новий режим: світ + презентер + фабрика + гілка в `Program.cs`.

## Автор

Воскобойников Марк, КН-31

## Ліцензія

MIT - див. [LICENSE](LICENSE).
