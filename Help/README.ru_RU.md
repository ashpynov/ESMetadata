# Источник метаданных EmulationStation для Playnite

EmulationStation — это графическая настраиваемая оболочка для эмуляторов. Она хорошо известна в ретрогейминге, и многие сборки ретроэмуляторов основаны на ней, например: Retrobat, Batocera, RecallBox, RetroPie.

Если вы увлекаетесь ретрогеймингом, то, скорее всего, у вас уже есть коллекция ROM-файлов с любимыми обложками и видео геймплея. Возможно, вы потратили много времени на их сбор и полировку.

Этот источник метаданных предназначен для помощи в импорте вашей коллекции из EmulationStation в Playnite.

[Последняя версия](https://github.com/ashpynov/ESMetadata/releases/latest)

## Функции и ограничения

Набор медиаинформации об играх отличается между Playnite и EmulationStation. Например, в Playnite нет штатной поддержки видео или логотипов. Но множество расширений Playnite помогут вам. Например, очень популярное расширение ExtraMetadata поддерживается многими темами и поможет вам добавить видео и логотипы к вашим играм.

Однако нет програмного итерфейса для источников метаданных для работы с этими полями. Поэтому этот источник использует небольшие трюки:
- Некоторая информация, такая как отметка "Избранное" или внутриигровая статистика, импортируется, если запрашиваются поле "Теги" из источника.
- Дополнительные медиа-ресурсы, такие как видео, логотип, рамка, фанарт, руководство, могут быть импортированы как "Ссылки".

Чтобы использовать их как медиа для расширения ExtraMetadata, файлы могут быть скопированы в папку Extrametadata. Также вы можете настроить:
- Копирование некоторых медиа из вашей коллекции,
- Просто сохранение ссылки на медиа из вашей коллекции,
- Если файл был скопирован, вы можете сохранить ссылку на оригинал или заменить ее на ссылку на скопированное медиа.

Вы также можете выбрать приоритеты источников для иконки, обложки или фона и уменьшить размеры.

Имя игры не импортируется при автоматической загрузке метаданных (такова реализация Playnite).

### Поддерживаемые поля gamelist.xml

Вот список поддерживаемых полей:

Данные:
- Name,
- Desc,
- Genre,
- Region,
- ReleaseDate,
- Rating,
- Developer,
- Publisher,
- Favorite,
- PlayCount,
- LastPlayed,
- GameTime

Медиа:
- Path,
- Image,
- Thumbnail,
- Marquee,
- Fanart,
- Video,
- Bezel,
- Manual,
- Boxback,
- Box,
- Magazine,
- Map,
- TitleShot


### Выбор записи игры и нечеткое совпадение

По умолчанию запись в коллекции ищется по пути ROM. Если ваша коллекция достаточно зрелая и организованная, этого должно быть достаточно. Коллекция ищется от ближайшего к расположению ROM. (Общая структура => roms/platform/rom_files + gamelist.xml)

Но если ваша коллекция такая же, как моя: имя ROM не совпадает, несколько копий ROM с разными именами, некоторые добавленные ROM просто для игры или модифицированные. Без описания... Нечеткий поиск попытается найти наиболее похожую заполненную игру в коллекции.

Признаком "незаполненной игры" является отсутствие поля описания. Поэтому, если поле отсутствует, он попытается найти следующую лучшую запись с описанием.

Что такое "лучшее совпадение"? Он попытается сравнить имя файла ROM и имя с именем игры, игнорируя артикли, подчеркивания и символы. И допускает некоторые различия. Поэтому иногда он будет ошибаться. Потому что "eaarth worm jim 2" очень близко к "earthworm jim".

Но в любом случае, если у вас есть несколько версий игры, вы можете выбрать также изображения из других похожих игр.

## Описание параметров конфигурации

![alt text](Help/Options.ru_RU.jpg)

### Приоритеты источников
1. Во время автоматической загрузки метаданных при обновлении библиотеки, эта опция автоматически выберет изображение для иконки, обложки и фона.
2. В выпадающем списке выберите тип изображений для использования и предпочтение. Он выберет первое доступное изображение в указанном списке. Вы можете использовать флажки слева во всплывающем окне, чтобы включить/отключить источник, и кнопки вверх/вниз справа, чтобы изменить порядок.
3. Опция для того, чтобы изображения не были слишком большим для указанного элемента. Выбранное изображение будет пропорционально уменьшено до максимального допустимого размера.
4. Укажите максимальный размер по ширине и высоте.

### Нечеткий поиск
5. Ваша импортируемая коллекция игр может содержать элементы без полезной информации. Обычно это игры, добавленные автоматически или недавно, некоторые вариации уже существующих игр просто для игры один раз. При Отсутствии описания будет выполнен нечеткий поиск следующей похожей записи для импорта информации.
6. При ручном редактировании игры эта опция дает вам возможность увидеть и выбрать изображение из другой похожей игры.
7. Во время нечеткого сопоставления порядок артиклей в названии игры может быть разным или отсутствовать. Эта опция удалит артикли при сравнении имен.

### Дополнительный раздел данных
8. Програмный интерфейс источников метаданных не поддерживает некоторые типы полей, например, отметку "Избранное" или внутриигровую статистику, такую как время последней игры.

   Поэтому расширение источника метаданных EmulationStation напрямую импортирует эту информацию, во время запроса поля "Теги".

   Оригинальный теги останутся неизменными. Но это может заблокировать другой источник в очереди, предоставляющий теги, так как Playnite будет ожидать теги от источника метаданных EmulationStation.
9. Используйте эту опцию, чтобы отметить любимую игру из импортируемой библиотеки. Эта опция только устанавливает метку. Если она уже установлена, она не изменится для не любимой игры в библиотеке игр EmulationStation.
10. Импортируйте информацию о игровой активности, такую как время игры, количество игр, последняя дата игры. Это также не изменяет уже существующие значения.

### Поддержка ссылок и расширения ExtraMetadata
11. Ваша импортируемая коллекция может также содержать некоторые другие полезные материалы. Например, видео, логотип, руководство, рамку и т.д. Некоторые из этих медиа, такие как руководство, имеют очень слабую штатную поддержку в Playnite (только добавление элемента в меню игры).

    Некоторые из этих медиа (видео, логотип) имеют богатую поддержку очень популярным расширением ExtraMetadata и поддерживающими темами. Другие поддерживаются менее популярными расширениями или не поддерживаются вообще.

    Этот раздел позволит источнику метаданных EmulationStation манипулировать полем "Ссылки" для копирования этих медиа в папку игры расширения ExtraMetadata или добавления информации в список ссылок для легкого доступа через меню игры.
12. Список медиа которые будут скопированы в папку ExtraMetadata с фиксированными именами, такими как "VideoTrailer.mp4" или "Logo.png" для поддержки расширением ExtraMetadata, или фиксированным именем и оригинальным расширением, таким как "Manual.pdf".

    Пожалуйста, используйте флажки в выпадающем списке, чтобы отметить элемент для копирования.

    Во время ручного редактирования игры вы увидите ссылки с именами, такими как [ESMS Video] или [ESMS Manual]. Пожалуйста, не удаляйте их, так как это инструкция, какие медиа файлы копировать. Эти ссылки будут автоматически удалены после копирования.

13. Так как имена в случае копирования фиксированы, опледеляет что делать, если файл уже существует во время автоматической/массовой загрузки метаданных.
14. Выберите здесь путь к медиа, которые будут сохранны как ссылки.
15. Playnite имеет выделенное поле игры "Руководство", опция указывает сохранят путь к руководству в этом поле. Опция независима от предыдущей - вы можете выбрать оба варианта, тогда информация будет сохранена и в ссылках и в поле "Руководство".
16. В случае, если вы выбрали копирование медиа в каталог ExtraMetadata и сохранение их как ссылки, опция указывает следует ли использовать путь к медиа в оригинальном расположении или путь к копиям.