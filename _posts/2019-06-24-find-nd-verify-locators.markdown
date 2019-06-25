---
title: "Find.By — finding & verifying locators"
layout: post
date: 2019-06-24 12:00
headerImage: false
tag:
- resharper
- xpath
- selenium
star: false
category: blog
author: yurii-hunter
description: "Find.By — finding & verifying locators"
---
Начну эту статью с того, что я работаю QA Automationg и того, что я люблю автоматизировать все рутинные действия. Так вот в последнее время для меня таковым стало написание, редактирование и проверка локаторов к элементам на странице. 

Обычно это процесс выглядит так: я пишу xpath выражение в chrome или firepath, потом копирую его и добавляю атрибут к элементу в C# коде. Но локаторы часто нужно исправлять или просто проверить, на какой элемент он указывает. И даже такое просто изменение предиката как [@id='myId'] на [contains(@id = 'Id')] заканчивается падением теста во время выполнения потому, что я написал '=' вместо ',' и поленился проверить изменения. В общем, слишком много действий с копированием, вставкой, переключений между окнами и тому подобного для такой простой задачи. Решил я написать плагин для ReSharper, который бы по Alt+Enter подсвечивал мой элемент в браузере.
# Постановка задачи
Написать плагины для ReSharper и Chrome, которые в связке решали бы две простые задачи:
* Статический анализатор XPath выражений;
* Подсветка любого selenium локатора в браузере по Alt+Enter.
# Решение
С первой задачей все оказалось достаточно просто. Я решил не писать свой парсер и валидатор для xpath, а просто воспользовался классом XmlDocument из .NET библиотеки:
```csharp
XmlDocument document = new XmlDocument();
var navigator = document.CreateNavigator();
try
{
    navigator.Compile("xpath expression");
     //Если не получили исключение, то выражение валидно
}
catch (XPathException exception)
{
    //Выражение не валидно
}
```
<iframe width="560" height="315" src="https://www.youtube.com/embed/PhdkFtNHcmk" frameborder="0" allow="accelerometer; autoplay; encrypted-media; gyroscope; picture-in-picture" allowfullscreen></iframe>

Вторая часть вопроса была реализована следующим образом:
* ReSharper плагин поднимает TcpServer для localhost
* Chrome плагин постоянно спрашивает его «Есть что подсветить?»
* Список чего бы подсветить пустой до тех пор, пока пользователь не нажмет Alt+Enter и не выберет Highlight element

Для поиска элементов в dom с помощью XPath я использую библиотеку jquery.xpath.js.

Так как я никогда раньше не писал плагины для chrome, то самым сложное для меня оказалось отправка запросов с https сайта на http localhost. Google Chrome блокирует все такие запросы с веб-станицы. На поиск решения у меня ушло не мело времени, но оно оказалось очень простым и, наверное, даже логичным: запросы к http можно делать из background.js, а потом передавать результат на страничку, что я и сделал.

# Работа плагина в действии
<iframe width="560" height="315" src="https://www.youtube.com/embed/iaJ_VY-dNNc" frameborder="0" allow="accelerometer; autoplay; encrypted-media; gyroscope; picture-in-picture" allowfullscreen></iframe>

# Ссылки

[Пишем простейший плагин для ReSharper](https://habrahabr.ru/post/270155/) — статья, которая помогла начать писать для ReSharper  
[Find.By chrome](https://chrome.google.com/webstore/detail/findby/phhphchblcckjanhfgimfmhopmjoefnb) — плагин для Google Chrome  
[Find.By resharper](https://resharper-plugins.jetbrains.com/packages/Find.By) — плагин для ReSharper  
[Find.By](https://github.com/yurii-hunter/find-by) — github репозиторий проекта  