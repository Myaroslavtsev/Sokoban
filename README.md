# Sokoban
Study project: Sokoban with add-ons

Сокобан с дополнительными возможностями.

Основные правила:
Игра проходит на двумерном поле с видом сверху. 
Игровое поле разделено на клетки. 
В одной клетке может быть стена, ящик или игрок. 
Также клетка может быть местом для ящика. 
Игрок может двигаться влево, вправо, вверх и вниз, но не может проходить сквозь стены и ящики. 
Игрок может толкнуть ящик, если он не упирается в стену или другой ящик. 
Игрок побеждает, если все ящики оказываются на местах для ящиков.

Дополнения:
- Ограничение по количеству ходов. Если игрок не выиграл за установленное количество ходов, игра считается проигранной.

- Возможность увеличить лимит ходов по запросу.

- Возможность задать силу игрока (количество одновременно передвигаемых ящиков).

- Двери, открываемые ключами либо при проходе через нажимные плиты.

- Ключи, которые игрок может собирать и использовать для открытия дверей. Ключ может быть раздавлен при прохождении ящика.

- Нажимные плиты, открывающие заданные двери при первом прохождении по ним игрока или ящика.

- Возможность собирать разбросанные на поле бомбы и использовать их для уничтожения стен.

- Гравитация (ящики падают друг на друга либо на стены, но игрок может толкать ящик вверх).

- Сохранение и загрузка уровней из файла в папке levels.

- Интерфейс командной строки со следующими командами:
   quit - выход без сохранения
   levels - список уровней, доступных для загрузки
   load - загрузка игры с именем по умолчанию
   load filename.csv - загрузка уровня с заданным именем
   save - сохранение в файл с именем по умолчанию 
   save filename.csv - сохранение в заданный файл
   options - вывод списка активированных опций
     gravity - активировать/деактивировать гравитацию
     movelimit - активировать/снять ограничение на количество ходов
   addmoves 8 - увеличение лимита ходов на заданное число
   setforce 3 - задание силы игрока
   about - контакты автора
   help - список допустимых команд

Диаграмма классов доступна по ссылке:
https://github.com/Myaroslavtsev/Sokoban/blob/master/ConsoleApp1/ClassDiagram.pdf
