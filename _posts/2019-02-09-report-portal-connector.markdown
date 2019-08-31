---
title: "Как настроить отправку результатов тестов в Report Portal"
layout: post
date: 2019-02-09 12:00
headerImage: false
tag:
- xunit
- nunit
- mstest
- specflow
- report portal
star: false
category: blog
author: yurii-hunter
description: Как настроить отправку результатов тестов в Report Portal
---
## Как настроить отправку результатов тестов в Report Portal
Если вы уже попробовали самостоятельно настроить отправку отчетов и ничего не получилось, то этот пост призван вам помочь.

Первое, что нужно понять - не важно какой test framework вы используете, а важно какой test runner выбран для запуска тестов. Report Portal агент подключается к test runner консоли, читает информацию о выполнении тестов и отправляет на сервер Report Portal.

Для каждого из популярных тест фреймворков реализован нативный тест ранер. Для xunit.net - xunit.runner.console.exe, для NUnit - nunit-agent.exe, а для MSTest - mstest.exe. Кроме нативных, можно использовать сторонние ранеры. В Microsoft реализовали vstest и dotnet test ранеры, которые поддерживают запуск тестов для каждого из вышеперечисленных фреймворков. Report Portal предоставляет агенты для каждого из этих тест ранеров.

В отдельную группу вынесем SpecFlow. SpecFlow интегрирован с популярными тест фреймворками и, в конечном итоге, генерирует код под тот, что используется в проекте. Это значит, что SpecFlow тесты тоже можно запускать с помощью тест ранера используя совместимые report portal агенты. Еще доступен отдельный Report Portal agent для SpecFlow. Главное отличие от остальных агентов том, что он не использует данные тест ранера. Взаимодействие построено на основе хуков (Hooks), которые предоставляет SpecFlow. Report Portal агент подписывается на BeforeTestRun, BeforeFeature, BeforeScenario, BeforeStep и т.д. события и на каждое из этих событий отсылает информацию на Report Portal сервер.

#### Как настроить отправку результатов для xUnit тест ранера в Report Portal
Загрузите [архив с xunit агентом](https://github.com/reportportal/agent-net-xunit/releases) и распакуйте в папку с тест ранером. Файлы _ReportPortal.XUnitReporter.dll_ и _xunit.runner.console.exe_ должны находится в одной папке. Обновите настройки к серверу Report Portal в файле конфигураций `ReportPortal.config.json`. Запустите тесты с помощью xunit.runner.console.exe. Тест ранер автоматически подхватит XUnitReporter.dll и результаты будут опубликованы на сервер.
```Batchfile
>_ xunit.runner.console.exe testAssemblyFile.dll
```

#### Как настроить отправку результатов для vstest и dotnet test тест ранера в Report Portal
Установите _ReportPortal.VSTest.TestLogger_ nuget пакет в проект с тестами. Добавьте конфигурационный файл `ReportPortal.config.json` к проекту. Измените значение свойства _Copy if newer_ на _Copy to Output Directory_. Важно чтобы файл конфигурации копировался в папку с артефактами после сборки. При запуске тестов с консоли добавьте аргумент --logger со значением ReportPortal.
```powershell
dotnet vstest .\Test.Assembly.dll  --logger ReportPortal
```

#### Как настроить отправку результатов для SpecFlow в Report Portal
Установите _ReportPortal.SpecFlow_ nuget пакет в проект с тестами. Обновите конфигурационный файл `ReportPortal.config.json`. В случае со спекфлоу вам больше не нужно конфигурировать тест ранер. Report Portal agent - это плагин к SpecFlow. Он подписывается на хуки и отправляет сообщения прямо во время выполнения тестов. Еще один из плюсов агента для SpecFlow - возможность запускать и отправлять результаты тестов напрямую со студии, не используя тест ранер.

#### Как настроить отправку результатов для NUnit в Report Portal
Установите Nuget пакет _ReportPortal.NUnit_ в проект с тестами. Чтобы подключить расширение к тест ранеру, добавьте файл _ReportPortal.addins_ в папку где расположен NUnit Runner. Файл должен содержать относительный путь к _ReportPortal.NUnitExtension.dll_. Чтобы проверить, что плагин подключен, выполните команду _nunit3-console.exe --list-extensions_. Дальше запуск стандартный для NUnit ранера.