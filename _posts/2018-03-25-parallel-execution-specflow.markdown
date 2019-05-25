---
title: "Параллельному запуску тестов ReportPortal и SpecFlow быть"
layout: post
date: 2018-03-25 12:00
image: /assets/images/rp_specflow_featured.jpg
headerImage: false
tag:
- reportportal
- xunit.net
- specflow
star: false
category: blog
author: yurii-hunter
description: Параллельному запуску тестов ReportPortal и SpecFlow быть
---
Вышел долгожданный (по крайней мере мной) фикс бага для SpecFlow агента ReportPortal. Суть проблемы заключается в том, что ReportPortal создает пустые тест раны в случае параллельного запуска тестов.

Для решения проблемы пришлось чинить SpecFlow, в следствии чего вышла новая 2.3.1 версия, со сломанной обратной совместимостью. А также были внесены обновления в ReportPortal.SpecFlow клиент. Последний рабочий набор библиотек выглядит так:
```
ReportPortal.Client - 2.0.0
ReportPortal.Shared - 2.0.0-beta4
ReportPortal.SpecFlow - 1.2.2-beta-20
SpecFlow - 2.3.1
SpecFlow.CustomPlugin - 2.3.1
SpecFlow.xUnit - 2.3.1
xunit - 2.3.1
```
__UPD__: ReportPortal.SpecFlow клиент 1.2.2-beta-20 содержит bug. Все feature файлы должны содержать tags и Feature Description иначе при старте получишь NullReferenceException. Ждем нового фикса.